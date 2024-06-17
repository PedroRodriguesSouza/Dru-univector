using Futebol.src.comum;
using Futebol.src.estrategias.estrategia2;
using Futebol.src.sincronismo;
using Futebol.ui;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing.Text;
using System.IO.Ports;
using System.Security.Cryptography.Xml;
using static Futebol.src.estrategias.estrategia1.Estrategia1;
using static Futebol.src.sincronismo.ControleJogo;
using static Futebol.src.visao.DesenhoVisao;


namespace Futebol.src.estrategias.estrategia1;
public class AtacanteUnivectorE1
{
    private AmbienteE1 ambiente;
    private ControleJogo controle;
    private SaidaE1 saida;
    private UtilsE1 utils;
    private DesenhaE1 desenha;
    private UnivectorE1 univector;
    private Vector2D posAnterior = new Vector2D(0, 0);
    public List<Tuple<Vector2D, bool, double>> caminho;
    public List<Tuple<Vector2D, bool, double>> caminhoFinal;
    private List<Double> rampaDestino = new List<Double>();
    private Tuple<double, int>[] dadosAtacante = new Tuple<double, int>[10];
    private double somaDistancia = 0;
    private int somaTempo = 0;
    private int index = 0;
    private bool curva = false;
    private FileStream _stream;
    private StreamWriter _writer;
    private String nomeLogger = "C:\\Users\\drumonstro\\Desktop\\ultima_versao\\drumonsters\\caminho3.txt";
    public AtacanteUnivectorE1(AmbienteE1 ambienteT, ControleJogo controleT, SaidaE1 saidaT, UtilsE1 utilsT, DesenhaE1 desenhaT)
    {
        ambiente = ambienteT;
        controle = controleT;
        saida = saidaT;
        utils = utilsT;
        desenha = desenhaT;
        caminho = new List<Tuple<Vector2D, bool, double>>();
        caminhoFinal = new List<Tuple<Vector2D, bool, double>>();
        univector = new UnivectorE1(ambiente, controle, saida, utils, desenha);
        //_stream = File.OpenWrite(nomeLogger);
        //_writer = new StreamWriter(_stream);
        for (int i = 0; i < dadosAtacante.Length; ++i)
        {
            dadosAtacante[i] = new Tuple<double, int>(0, 0); // (deta_dist, tempo)
        }
    }

    ~AtacanteUnivectorE1()
    {
        //_writer.Close();
        //_stream.Close();
    }
    //private void SalvarDados(RoboE1 robo)
    //{
    //    _writer.WriteLine($"{robo.posicao},{Math.Round(robo.rotacao,2)},{ambiente.Bola.posicao}");
    //    _writer.Flush();
    //}

    public void Executa(RoboE1 robo, int tempo)
    {
        double velocidade;

        somaDistancia -= dadosAtacante[index].Item1;
        somaTempo     -= dadosAtacante[index].Item2;

        dadosAtacante[index] = new Tuple<double, int>(robo.posicao.Distance(posAnterior), tempo);

        somaDistancia += dadosAtacante[index].Item1;
        somaTempo     += dadosAtacante[index].Item2;

        index = (index + 1) % dadosAtacante.Length;

        //velocidade = posAnterior.Distance(robo.posicao) / tempo * 2.7; 
        velocidade = somaDistancia / somaTempo * 2.7;

        robo.velocidade = velocidade;
        posAnterior = robo.posicao.Clone();
        //saida.DesenharTexto(new Vector2D(robo.posicao.x, robo.posicao.y + 60), $"VEL: {Math.Round(velocidade, 2)}", Color.White);

        //SalvarDados(robo);
        //ganhos controle para papel 
        //robo.seguidor.Kp_angular = 0;
        //robo.seguidor.Ki_angular = 0.0;
        //robo.seguidor.Kd_angular = 0.0;

        //robo.seguidor.Kp_angular = 0.2;
        //robo.seguidor.Ki_angular = 0.01;
        //robo.seguidor.Kd_angular = 1.8;

        robo.seguidor.Kp_angular = 1.2; //agressivo = 1.3
        robo.seguidor.Ki_angular = 0.01;
        robo.seguidor.Kd_angular = 0.1;

        robo.seguidor.Kp_linear = 1.0; 
        robo.seguidor.Kd_linear = 0.0;

        robo.seguidor.velocidadeLinearMax = 80;

        BolaE1 bola = ambiente.Bola;
        Campo campo = ambiente.Campo;
        Lado lado = ambiente.Time.lado;

        int pontoControleCurvaRampa = 30;
        double de = 80;
        double kr;
        double raioDesvio = 90;
        double g = 10;
        bool b1, b2;
        b1 = true;
        b2 = false;
        bool desvia = true;
        int quantidadeMaximaPontosUnivector = 800;

        double mutltiplicadoPontoControle = 100;

        int pontoControle = (int)Math.Clamp(mutltiplicadoPontoControle * (0.1 + velocidade / 1.5), 10, 200);
        //int pontoControle = 75;
        Vector2D posRobo = robo.ProximaPosicao(1);
        //Vector2D posRobo = robo.posicao;
        Vector2D posBola = bola.ProximaPosicao(1 * caminho.Count / (double)quantidadeMaximaPontosUnivector);

        //saida.DesenharReta(robo.posicao, posRobo);
        //saida.DesenharReta(bola.posicao, posBola);

        Vector2D sensorF = new Vector2D(Math.Cos(robo.rotacao), Math.Sin(robo.rotacao)).Mult(40).Add(robo.posicao);
        Vector2D sensorT = new Vector2D(Math.Cos(robo.rotacao), Math.Sin(robo.rotacao)).Mult(-40).Add(robo.posicao);

        saida.DesenharDestinoZagueiro(sensorF);
        saida.DesenharDestinoZagueiro(sensorT);

        #region Pedro Corridas
        double x = lado == ControleJogo.Lado.Direito ? ambiente.Campo.gol_esquerdo.ladoDireito : ambiente.Campo.gol_direito.ladoEsquerdo;
        Vector2D traveS = new Vector2D(x, lado == ControleJogo.Lado.Direito ? ambiente.Campo.gol_esquerdo.ladoSuperior : ambiente.Campo.gol_direito.ladoSuperior);
        Vector2D traveI = new Vector2D(x, lado == ControleJogo.Lado.Direito ? ambiente.Campo.gol_esquerdo.ladoInferior : ambiente.Campo.gol_direito.ladoInferior);
        double aS = (posRobo.y - traveS.y) / (posRobo.x - traveS.x);
        double bS = traveS.y - aS * traveS.x;

        double aI = (posRobo.y - traveI.y) / (posRobo.x - traveI.x);
        double bI = traveI.y - aI * traveI.x;

        bool sup = sensorF.x * aS + bS - sensorF.y < 0;
        bool inf = sensorF.y - (sensorF.x * aI + bI) < 0;

        //saida.DesenharReta(traveS, posRobo, !sup);
        //saida.DesenharReta(traveI, posRobo, !inf);

        Limites gol = lado == Lado.Direito ? campo.gol_esquerdo : campo.gol_direito;

        //if (posRobo.Distance(posBola) < 30 && ((sup && inf) || (!sup && !inf)) && !utils.AtrasDaBola(posRobo.x, posBola.x, 20))
        if (Math.Abs(utils.teste(robo.rotacao, posBola.Sub(posRobo))) < 15 && ((sup && inf) || (!sup && !inf)) && !utils.AtrasDaBola(posRobo.x, posBola.x, -40))
        {
            caminho.Clear();
            Vector2D destino = new Vector2D(Math.Cos(robo.rotacao), Math.Sin(robo.rotacao)).Mult((inf && sup) ? -1000 : 1000).Add(posRobo);
            TrajetoriaAtacanteGol(destino, posRobo);
            robo.seguidor.ControleRoboVirtualDuasFrentes(destino, sensorF, sensorT, true, 0.001, tempo, 1);
            foreach (var ponto in caminho)
            {
                saida.DesenharPonto(ponto.Item1, Color.Red);
            }
        }
        #endregion
        else
        {
            #region Decisão do melhor ponto para Gol
            Vector2D posGol = new Vector2D(0, campo.gol_direito.centro.y);
            posGol.x = lado == Lado.Direito ? campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL : campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;

            Vector2D ponto1 = new Vector2D(gol.ladoDireito, gol.ladoInferior);
            Vector2D ponto2 = new Vector2D(gol.ladoEsquerdo, gol.ladoSuperior);
            Vector2D ponto3 = new Vector2D(gol.ladoDireito, gol.ladoSuperior);
            Vector2D ponto4 = new Vector2D(gol.ladoEsquerdo, gol.ladoInferior);

            double aux = Math.Abs(posBola.Sub(posGol).Angle());
            //double aux = Math.Abs(posRobo.Sub(posGol).Angle() - robo.rotacao);
            double menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));

            //aux = Math.Abs(posRobo.Sub(ponto1).Angle() - robo.rotacao);
            //if(Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            //{
            //    posGol = ponto1;
            //    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            //}
            ////aux = Math.Abs(posRobo.Sub(ponto2).Angle() - robo.rotacao);
            ////if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            ////{
            ////    posGol = ponto2;
            ////    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            ////}
            //aux = Math.Abs(posRobo.Sub(ponto3).Angle() - robo.rotacao);
            //if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            //{
            //    posGol = ponto3;
            //    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            //}
            ////aux = Math.Abs(posRobo.Sub(ponto4).Angle() - robo.rotacao);
            ////if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            ////{
            ////    posGol = ponto4;
            ////}

            //aux = Math.Abs(posBola.Sub(ponto1).Angle());
            //if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            //{
            //    posGol = ponto1;
            //    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            //}
            ////aux = Math.Abs(posRobo.Sub(ponto2).Angle() - robo.rotacao);
            ////if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            ////{
            ////    posGol = ponto2;
            ////    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            ////}
            //aux = Math.Abs(posBola.Sub(ponto3).Angle());
            //if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            //{
            //    posGol = ponto3;
            //    menorAngulo = Math.Min(aux, Math.Abs(aux - Math.PI));
            //}
            ////aux = Math.Abs(posRobo.Sub(ponto4).Angle() - robo.rotacao);
            ////if (Math.Min(aux, Math.Abs(aux - Math.PI)) < menorAngulo)
            ////{
            ////    posGol = ponto4;
            ////}

            saida.DesenharPonto(ponto1, Color.Red);
            saida.DesenharPonto(ponto2, Color.Blue);
            saida.DesenharPonto(ponto3, Color.Green);
            saida.DesenharPonto(ponto4, Color.Pink);

            #endregion

            #region Tratamento de jogo na Lateral
            double tolerancia = 50;
            if (utils.LateralInferior(posBola, tolerancia))
            {
                posGol = new Vector2D(lado == Lado.Direito ? campo.limites.ladoEsquerdo : campo.limites.ladoDireito, campo.limites.ladoInferior + 50);
            }
            else if (utils.LateralSuperior(posBola, tolerancia))
            {
                posGol = new Vector2D(lado == Lado.Direito ? campo.limites.ladoEsquerdo : campo.limites.ladoDireito, campo.limites.ladoSuperior - 50);
            }
            if (utils.LateralEsquerdo(posBola, tolerancia))
            {
                if (lado == ControleJogo.Lado.Direito ? posBola.x > campo.limites.centro.x : posBola.x < campo.limites.centro.x)//se esta defendendo na lateral esquerda
                {
                    Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_esquerda : campo.area_goleiro_direita;
                    if (posBola.y < area.ladoInferior)
                    {
                        de = Math.Abs(area.ladoInferior - posBola.y - 20);
                        posGol = new Vector2D(posBola.x, campo.limites.ladoInferior);
                        desvia = false;
                    }
                    else if (posBola.y > area.ladoSuperior)
                    {
                        de = Math.Abs(area.ladoSuperior - posBola.y - 10);
                        posGol = new Vector2D(posBola.x, campo.limites.ladoSuperior);
                        desvia = false;
                    }
                    saida.DesenharDestinoZagueiro(posGol);
                }
                else
                {
                    //posGol = new Vector2D(70, 70);
                    //saida.DesenharDestinoZagueiro(posGol);
                }
            }
            else if (utils.LateralDireito(posBola, tolerancia))
            {
                if (lado == ControleJogo.Lado.Direito ? posBola.x > campo.limites.centro.x : posBola.x < campo.limites.centro.x)//se esta defendendo na lateral direita
                {
                    int gap = -0;
                    Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_esquerda : campo.area_goleiro_direita;
                    if (posBola.y < area.ladoInferior)
                    {
                        de = Math.Abs(area.ladoInferior - posBola.y - 15);
                        posGol = new Vector2D(posBola.x, campo.limites.ladoInferior);
                        desvia = false;
                    }
                    else if (posBola.y > area.ladoSuperior)
                    {
                        de = Math.Abs(area.ladoSuperior - posBola.y - 15);
                        posGol = new Vector2D(posBola.x, campo.limites.ladoSuperior);
                        desvia = false;
                    }
                    saida.DesenharDestinoZagueiro(posGol);
                }
                else //se esta atacando na lateral direita
                {
                    //posGol = new Vector2D(70, 180);
                    //saida.DesenharDestinoZagueiro(posGol);
                }
            }
            #endregion

            caminho.Clear();
            rampaDestino.Clear();
            caminho.Add(new Tuple<Vector2D, bool, double>(posRobo, false, 0));
            kr = de * 1.5;
            desvia = false;
            CamposPotenciaisUnivetoriais(posRobo, posGol, posBola, kr, de, quantidadeMaximaPontosUnivector, desvia, raioDesvio, g, Papel.Atacante, caminho, b1, b2);
            TrajetoriaAtacanteGol(posBola, caminho[caminho.Count - 1].Item1.Clone(), 0.1);

            int tamanhoCaminho = caminho.Count;
            utils.CalculateCurvature(caminho, 10);
            TrajetoriaAtacanteGol(posGol, caminho[caminho.Count - 1].Item1.Clone());

            for (int i = 0; i < caminho.Count; i++)
            {
                rampaDestino.Add(1);
            }

            int avancorampa = Math.Min(tamanhoCaminho - 10, caminho.Count);
            utils.RampaDestino(80, ref rampaDestino, avancorampa, 0.8);

            if (pontoControle >= caminho.Count)
            {
                pontoControle = caminho.Count -1;

            }
            if (caminho.Count <= 0 || pontoControle <= 0)
            {
                return;
            }

            Vector2D destino = caminho[pontoControle].Item1;

            pontoControleCurvaRampa = Math.Min(pontoControleCurvaRampa - 1, caminho.Count - 1);
            double multiplicador = caminho[pontoControleCurvaRampa].Item3;
            bool estado = controle.EstadoAtual == Estado.Executando; // Para o erro integral não explodir.
            //for (int i = 0; i < caminho.Count; ++i)
            //{
            //    //                                          sup, inf, dir, esq       area
            //    if (caminho[i].Item1.Distance(posBola) > 30)
            //    {
            //        if (utils.SaturaForaCampo(caminho[i].Item1, 37, 28, 30, 30, false, 20))
            //        {
            //            //index_primeiro_ponto = i;
            //            //flag = false;
            //        }
            //    }
            //}

            int index_primeiro_ponto = 1;
            int contador = 0;
            while (contador < 1)
            {
                contador++;
                index_primeiro_ponto = 0;
                bool flag = true;
                #region Saturação dos pontos do caminho
                
                if (posGol.y < 200)
                {
                    for (int i = 0; i < caminho.Count; ++i)
                    {
                        //                                          sup, inf, dir, esq       area
                        if (caminho[i].Item1.Distance(posBola) > 0)
                        {
                            if (utils.SaturaForaCampo(caminho[i].Item1, 37, 28, 30, 30, false, 20) && flag)
                            {
                                index_primeiro_ponto = i;
                                flag = false;
                            }
                        }
                    }
                    saida.DesenharPonto(caminho[index_primeiro_ponto].Item1, Color.Orange);
                    caminhoFinal.Clear();
                    index_primeiro_ponto = Math.Min((int)(index_primeiro_ponto + 10), caminho.Count-1);
                    CamposPotenciaisUnivetoriais(posRobo, posGol, caminho[index_primeiro_ponto].Item1, 30, 50, quantidadeMaximaPontosUnivector, desvia, raioDesvio, g, Papel.Atacante, caminhoFinal, b1, b2);
                    for (int i = index_primeiro_ponto; i < caminho.Count; ++i)
                    {
                        caminhoFinal.Add(new Tuple<Vector2D, bool, double>(caminho[i].Item1.Clone(), caminho[i].Item2, caminho[i].Item3));
                    }
                    caminho.Clear();
                    foreach (var ponto in caminhoFinal)
                    {
                        caminho.Add(new Tuple<Vector2D, bool, double>(ponto.Item1.Clone(), ponto.Item2, ponto.Item3));
                    }
                }
                else
                {
                    posGol = new Vector2D(lado == Lado.Direito ? campo.limites.ladoEsquerdo : campo.limites.ladoDireito, campo.limites.ladoInferior + 50);
                    for (int i = 0; i < caminho.Count; ++i)
                    {
                        //                                          sup, inf, dir, esq       area
                        if (caminho[i].Item1.Distance(posBola) > 30)
                        {
                            if (utils.SaturaForaCampo(caminho[i].Item1, 47, 38, 40, 30, false, 30))
                            {
                                index_primeiro_ponto = i;
                            }
                        }
                    }
                    saida.DesenharPonto(caminho[index_primeiro_ponto].Item1, Color.Orange);
                    caminhoFinal.Clear();
                    index_primeiro_ponto = Math.Min(index_primeiro_ponto + 10, caminho.Count-1);
                    CamposPotenciaisUnivetoriais(posRobo, posGol, caminho[index_primeiro_ponto].Item1, kr, de , quantidadeMaximaPontosUnivector, desvia, raioDesvio, g, Papel.Atacante, caminhoFinal, b1, b2);
                    for (int i = index_primeiro_ponto; i < caminho.Count; ++i)
                    {
                        caminhoFinal.Add(new Tuple<Vector2D, bool, double>(caminho[i].Item1.Clone(), caminho[i].Item2, caminho[i].Item3));
                    }
                    caminho.Clear();
                    foreach (var ponto in caminhoFinal)
                    {
                        caminho.Add(new Tuple<Vector2D, bool, double>(ponto.Item1.Clone(), ponto.Item2, ponto.Item3));
                    }
                }
            }
            #endregion
            //desenha.DesenhaTrajetoriaIntensidadeRampaECurva(ref rampaDestino, caminhoFinal);
            if (pontoControle >= caminho.Count)
            {
                pontoControle = caminho.Count - 1;

            }
            if (caminho.Count <= 0 || pontoControle <= 0)
            {
                return;
            }

            desenha.DesenhaTrajetoriaIntensidadeRampaECurva(ref rampaDestino, caminho);
            //robo.seguidor.ControleRoboVirtual(destino, sensorF, estado, multiplicador, rampaDestino[pontoControleCurvaRampa]);
            robo.seguidor.ControleRoboVirtualDuasFrentes(destino, sensorF, sensorT, estado, multiplicador, tempo, rampaDestino[pontoControleCurvaRampa]);
            //robo.seguidor.ControleRoboVirtualDuasFrentes(destino, sensorF, sensorT, estado, rampaDestino[pontoControle]);

            saida.DesenharPonto(caminho[pontoControle].Item1, Color.Yellow);
        }
        robo.seguidor.ImprimirErro();
    }

    public void CamposPotenciaisUnivetoriais(Vector2D robo, Vector2D referencia, Vector2D objetivo, double kr, double de, double passo, bool desvia, double raioDesvio, double g, Papel papel, List<Tuple<Vector2D, bool, double>> path, bool b1 = true, bool b2 = false)
    {
        double angulo = Math.Atan2(objetivo.y - referencia.y, objetivo.x - referencia.x); //angulo entre a bola e o gol

        Vector2D roboRotacionado = utils.RotacionaEixo(angulo, robo);     //coordenadas do robo no eixo rotacionado
        Vector2D bolaRotacionada = utils.RotacionaEixo(angulo, objetivo); //coordenadas da bola no eixo rotacionado

        Vector2D delta = roboRotacionado.Sub(bolaRotacionada);  // delta x e y entre o robo e a bola (rotacionados)
        double alpha = Math.Atan2(delta.y, delta.x);            // angulo entre o robo e a bola

        double teta;
        Vector2D univector2D;
        Vector2D prox;

        if (desvia)
        {
            List<Vector2D> obstaculos = univector.DefineObstaculos(robo, papel);
            Vector2D obstaculo = univector.ObstaculoMaisProximo(robo, obstaculos);

            double r = robo.Distance(obstaculo); // r = 0 caso não exista obstaculos

            if (r == 0)
            {
                teta = univector.PhiTUF(alpha, delta, kr, de, b1, b2);
                univector2D = utils.RotacionaEixo(-angulo, new Vector2D(Math.Cos(teta), Math.Sin(teta)));
                prox = robo.Add(univector2D);
            }
            else if (r < raioDesvio)
            {
                teta = univector.PhiR(roboRotacionado, utils.RotacionaEixo(angulo, obstaculo));
                univector2D = utils.RotacionaEixo(-angulo, new Vector2D(Math.Cos(teta), Math.Sin(teta)));
                prox = robo.Add(univector2D);
            }
            else
            {
                for (int i = 0; i < obstaculos.Count; i++)
                {
                    obstaculos[i] = utils.RotacionaEixo(angulo, obstaculos[i]);
                }

                var aux = objetivo.Distance(robo) < objetivo.Distance(obstaculo) ? 0 : univector.G(r - raioDesvio, g);
                teta = univector.PhiR2(roboRotacionado, obstaculos, raioDesvio) * aux + univector.PhiTUF(alpha, delta, kr, de, b1, b2) * (1 - aux);

                if (aux > 0.15)
                {
                    curva = true;
                }

                univector2D = utils.RotacionaEixo(-angulo, new Vector2D(Math.Cos(teta), Math.Sin(teta)));
                prox = robo.Add(univector2D);
            }
        }
        else
        {
            teta = univector.PhiTUF(alpha, delta, kr, de, b1, b2);
            univector2D = utils.RotacionaEixo(-angulo, new Vector2D(Math.Cos(teta), Math.Sin(teta)));
            prox = robo.Add(univector2D);
        }

        if (passo > 0 && robo.Distance(objetivo) > 15)
        {
            --passo;
            if (papel == Papel.Atacante)
            {
                path.Add(new Tuple<Vector2D, bool, double>(prox, curva, teta));
            }
            CamposPotenciaisUnivetoriais(prox, referencia, objetivo, kr, de, passo, desvia, raioDesvio, g, papel, path, b1, b2);
        }
    }


    public void TrajetoriaAtacanteGol(Vector2D destino, Vector2D posicao, double ang = 0)
    {
        double angulo = Math.Atan2(destino.y - posicao.y, destino.x - posicao.x);
        Vector2D univector = new Vector2D(Math.Cos(angulo), Math.Sin(angulo));
        Vector2D ponto = posicao.Add(univector);
        int distancia = (int)ponto.Distance(destino);
        for (int i = 0; i < distancia; i++)
        {
            caminho.Add(new Tuple<Vector2D, bool, double>(ponto, false, ang));
            ponto = ponto.Add(univector);
        }
    }
}

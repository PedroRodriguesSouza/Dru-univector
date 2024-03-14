using Emgu.CV;
using Emgu.CV.Freetype;
using Futebol.comum;
using Futebol.sincronismo;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static Futebol.sincronismo.ControleJogo;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Futebol.estrategias.estrategia1
{
    public class Estrategia1 : IEstrategia
    {
        private AmbienteE1 ambiente;
        private ConfiguracoesE1 configuracoes;
        private SaidaE1 saida;
        private ControleJogo controle;

        private RoboE1.DadosVelocidade[] dadosRobo;
        private RoboE1.DadosVelocidade[] anteriorRobo;
        private RoboE1.DadosVelocidade[] medicaoRobo;

        public Estrategia1(int qtdTime, int qtdAdversario, ControleJogo controle)
        {
            ambiente = new AmbienteE1(qtdTime, qtdAdversario);
            configuracoes = new ConfiguracoesE1();
            saida = new SaidaE1(controle);
            this.controle = controle;

            dadosRobo = new RoboE1.DadosVelocidade[3];
            anteriorRobo = new RoboE1.DadosVelocidade[3];
            medicaoRobo = new RoboE1.DadosVelocidade[3];

            for (int i = 0; i < dadosRobo.Length; i++)
            {
                dadosRobo[i] = new RoboE1.DadosVelocidade();
                anteriorRobo[i] = new RoboE1.DadosVelocidade();
                medicaoRobo[i] = new RoboE1.DadosVelocidade();
            }

            if (ConfiguracoesE1.GRAVAR_ARQUIVO)
                controle.saida.CriarArquivo("dados.txt");
           /*
            Vector2D pos = Vector2D.Zero();
            pos.x = ConverteUnidades.ConverterParaMetros(236);
            pos.y = ConverteUnidades.ConverterParaMetros(Configuracoes.YMAX - 443);

            controle.simulador.modelo.ambiente.TimeA.Robo[0].posicao.x = pos.x;
            controle.simulador.modelo.ambiente.TimeA.Robo[0].posicao.y = pos.y;
            controle.simulador.modelo.ambiente.TimeA.Robo[0].posicaoInicial.x = pos.x;
            controle.simulador.modelo.ambiente.TimeA.Robo[0].posicaoInicial.y = pos.y;

            pos.x = ConverteUnidades.ConverterParaMetros(381);
            pos.y = ConverteUnidades.ConverterParaMetros(Configuracoes.YMAX - 360);

            controle.simulador.modelo.ambiente.TimeB.Robo[0].posicao.x = pos.x;
            controle.simulador.modelo.ambiente.TimeB.Robo[0].posicao.y = pos.y;
            controle.simulador.modelo.ambiente.TimeB.Robo[0].posicaoInicial.x = pos.x;
            controle.simulador.modelo.ambiente.TimeB.Robo[0].posicaoInicial.y = pos.y; 
            */
        }

        public void PararRobos()
        {
            TimeE1 time = ambiente.Time;
            for (int i = 0; i < time.Robo.Length; i++)
                time.Robo[i].Parar();
        }

        //bool chegou = false;

        public bool BolaDentroArea()
        {
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;

            if (lado == ControleJogo.Lado.Direito && bola.posicao.x > area.ladoEsquerdo &&
                bola.posicao.y < area.ladoSuperior && bola.posicao.y > area.ladoInferior)
                return true;
            else if (lado == ControleJogo.Lado.Esquerdo && bola.posicao.x < area.ladoDireito &&
                bola.posicao.y < area.ladoSuperior && bola.posicao.y > area.ladoInferior)
                return true;
            return false;
        }

        public void GoleiroMinhoca(ref RoboE1 robo)
        {
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;
            Limites gol = lado == ControleJogo.Lado.Direito ? campo.gol_direito : campo.gol_esquerdo;

            double dist = bola.posicao.Distance(bola.proximaPosicao);
            double distrobobola = bola.posicao.Distance(robo.posicao);

            Vector2D destino = Vector2D.Zero();
            destino.x = area.centro.x;

            Vector2D posGol = Vector2D.Zero();
            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = gol.centro.x;//gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = gol.centro.y;
            }
            else
            {
                posGol.x = gol.centro.x;//gol.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = gol.centro.y;
            }

            if (posGol.x != bola.posicao.x)
            {
                double a = (bola.posicao.y - posGol.y) / (bola.posicao.x - posGol.x);
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
                //destino.y = Math.Max(0.3f, Math.Min(campo.limites.altura - 0.3f, destino.y));

            }
            else
                destino.y = area.centro.y;

            if (destino.y > area.ladoSuperior) destino.y = area.ladoSuperior - ConfiguracoesE1.TOLERANCIA_GOLEIRO;
            if (destino.y < area.ladoInferior) destino.y = area.ladoInferior + ConfiguracoesE1.TOLERANCIA_GOLEIRO;

            {
                // se a bola estiver na area, isola a bola

                if (BolaDentroArea())
                {
                    robo.PosicionaMinhoca(ref bola.posicao);
                }
                else
                {
                    if (robo.posicao.x < destino.x - ConfiguracoesE1.TOLERANCIA_POSICAO || robo.posicao.x > destino.x + ConfiguracoesE1.TOLERANCIA_POSICAO)
                    { //fora da tolerancia na linha X
                      // Mantem o Robo perto la linha Fixa em X                  
                        robo.PosicionaMinhoca(ref destino);
                    }
                    else
                    {
                        //Se ele está proximo o suficiente do ponto ele mantem a posição
                        if (robo.posicao.y <= destino.y + ConfiguracoesE1.TOLERANCIA_POSICAO && robo.posicao.y >= destino.y - ConfiguracoesE1.TOLERANCIA_POSICAO)
                        {
                            double teta2 = robo.rotacao - (Math.PI / 2);

                            if (teta2 > 1.4835 && teta2 < 1.6580)
                            {
                                robo.Parar();
                            }
                            else
                            {
                                double velAngular = ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO * Math.Sin(2 * teta2);
                                robo.velocidadeRodaDireita = -(velAngular);
                                robo.velocidadeRodaEsquerda = velAngular;
                            }
                        }
                        else
                        {
                            robo.AjustaMinhoca(ref destino);
                        }
                    }
                }
            }
        }


        public void Goleiro(ref RoboE1 robo)
        {
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;
            Limites gol = lado == ControleJogo.Lado.Direito ? campo.gol_direito : campo.gol_esquerdo;

            robo.papel = RoboE1.Papel.Goleiro;

            if (BolaDentroArea())
            {
                robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
                robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE;
                robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
                robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

                //robo.PosicionaPI(bola.proximaPosicao);
                robo.PosicionaAntiga(bola.proximaPosicao, ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE, ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE, ref saida);
            }
            else
            {
                robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO;
                robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO;
                robo.dadosPID.KP = ConfiguracoesE1.KP_GOLEIRO;
                robo.dadosPID.KI = ConfiguracoesE1.KI_GOLEIRO;


                Vector2D destino = Vector2D.Zero();
                destino.x = area.centro.x;

                Vector2D posGol = Vector2D.Zero();
                if (lado == ControleJogo.Lado.Direito)
                {
                    posGol.x = gol.centro.x;//gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                    posGol.y = gol.centro.y;
                }
                else
                {
                    posGol.x = gol.centro.x;//gol.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                    posGol.y = gol.centro.y;
                }

                if (posGol.x != bola.posicao.x)
                {
                    double a = (bola.posicao.y - posGol.y) / (bola.posicao.x - posGol.x);
                    double b = posGol.y - a * posGol.x;
                    destino.y = a * destino.x + b;
                    //destino.y = Math.Max(0.3f, Math.Min(campo.limites.altura - 0.3f, destino.y));

                }
                else
                    destino.y = area.centro.y;

                if (destino.y > area.ladoSuperior) destino.y = area.ladoSuperior - ConfiguracoesE1.TOLERANCIA_GOLEIRO;
                if (destino.y < area.ladoInferior) destino.y = area.ladoInferior + ConfiguracoesE1.TOLERANCIA_GOLEIRO;

                if (robo.posicao.Distance(destino) < ConfiguracoesE1.TOLERANCIA_POSICAO)
                {
                    /*
                    double angulo = Math.Abs(robo.rotacao);
                    if (angulo > Math.PI / 2 + ConfiguracoesE1.TOLERANCIA_ANGULO || angulo < Math.PI / 2 - ConfiguracoesE1.TOLERANCIA_ANGULO)
                    {
                        robo.Rotaciona(Math.PI / 2);
                        chegou = false;
                    }
                    else chegou = true;*/
                    robo.Parar();

                }
                else
                {
                    //robo.PosicionaPI(destino);
                    robo.PosicionaAntiga(destino, ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO, ref saida);
                    //chegou = false;
                }
                //robo.PosicionaPI(destino, bola.posicao);                //saida.ExibirValores("Goleiro - Destino", destino.x, destino.y, robo.posicao.Distance(destino));

                //desenhar ponto desejado para o goleiro
                if (ConfiguracoesE1.DESENHAR_PONTOS)
                {
                    saida.DesenharDestinoGoleiro(destino);
                    //saida.DesenharDestinoGoleiro(posGol);

                }
            }
        }

        public void Zagueiro(ref RoboE1 robo)
        {
            robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ZAGUEIRO;
            robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ZAGUEIRO;
            robo.dadosPID.KP = ConfiguracoesE1.KP_ZAGUEIRO;
            robo.dadosPID.KI = ConfiguracoesE1.KI_ZAGUEIRO;

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            //Limites gol = lado == ControleJogo.Lado.Direito ? campo.gol_direito : campo.gol_esquerdo;

            robo.papel = RoboE1.Papel.Zagueiro;

            Vector2D destino = Vector2D.Zero();
            Vector2D posGol = Vector2D.Zero();

            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_direito.centro.y;
                destino.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO;
            }
            else
            {
                posGol.x = campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_esquerdo.centro.y;
                destino.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO;
            }

            if (posGol.x != bola.posicao.x)
            {
                double a = (bola.posicao.y - posGol.y) / (bola.posicao.x - posGol.x);
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
            }
            else
                destino.y = posGol.y;

            if (destino.y > campo.limites.ladoSuperior - ConfiguracoesE1.TOLERANCIA_GOLEIRO) destino.y = campo.limites.ladoSuperior - ConfiguracoesE1.TOLERANCIA_GOLEIRO;
            if (destino.y < campo.limites.ladoInferior + ConfiguracoesE1.TOLERANCIA_GOLEIRO) destino.y = campo.limites.ladoInferior + ConfiguracoesE1.TOLERANCIA_GOLEIRO;

            if (robo.posicao.Distance(destino) < ConfiguracoesE1.TOLERANCIA_POSICAO)
                robo.Parar();
            else
                robo.PosicionaAntiga(destino, ConfiguracoesE1.VEL_LINEAR_MAX_ZAGUEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_ZAGUEIRO, ref saida);
                //robo.PosicionaPI(destino);


            /*
                        //Vector2D proxPosi = Vector2D.Zero();
                        List<Vector2D> proxPosi = null;

                  //      for (int i = 0; i < ambiente.Time.Robo.LongLength; i++)
                        {
                    //        if (robo.posicao.x > destino.x)
                            {
                  //              if (ambiente.Adversario.Robo[i].posicao.x > destino.x && ambiente.Adversario.Robo[i].posicao.x < robo.posicao.x)
            //                        if (Math.Abs(CalculaDistanciaReta(robo.posicao, destino, ambiente.Adversario.Robo[i].posicao)) < 32)
                                        //proxPosi = ProximoOrops(robo.posicao, destino, ambiente.Adversario.Robo[i].posicao);
                  //                      proxPosi = CurvaDesvio(ref robo.posicao, ref destino, ref ambiente.Adversario.Robo[i].posicao);
                            }
                      //      else
                //                if (ambiente.Adversario.Robo[i].posicao.x < destino.x && ambiente.Adversario.Robo[i].posicao.x > robo.posicao.x)
              //                  if (Math.Abs(CalculaDistanciaReta(robo.posicao, destino, ambiente.Adversario.Robo[i].posicao)) < 32)
                                    //proxPosi = ProximoOrops(robo.posicao, destino, ambiente.Adversario.Robo[i].posicao);
                                //    proxPosi = 
                                CurvaDesvio(ref robo.posicao, ref destino, ref ambiente.Adversario.Robo[0].posicao);
                        }

                        robo.PosicionaDesvio(destino, ambiente.Adversario.Robo[0].posicao, 20, 20);
            */
            /*
                        if (proxPosi == null)
                        {
                            if (robo.posicao.Distance(destino) < ConfiguracoesE1.TOLERANCIA_POSICAO)
                                robo.Parar();
                            else
                                robo.PosicionaDesvio(destino, ambiente.Adversario.Robo[0].posicao, 60, 60);
                        }
                        else
                        {
                            if (robo.posicao.Distance(proxPosi[0]) < ConfiguracoesE1.TOLERANCIA_POSICAO)
                                robo.Parar();
                            else
                                robo.PosicionaDesvio(destino, ambiente.Adversario.Robo[0].posicao, 60, 60);
                        }
            */
            //desenhar ponto desejado para o zagueiro
            if (ConfiguracoesE1.DESENHAR_PONTOS)
            {
               // saida.DesenharDestinoZagueiro(posGol);
                saida.DesenharDestinoZagueiro(destino);
                //saida.DesenharDestinoGoleiro(proxPosi);
                //if (proxPosi != null)
                 //   saida.DesenharCurva(proxPosi.ToArray());
            }

        }

        public void Atacante(ref RoboE1 robo)
        {
            Vector2D proximoPonto = Vector2D.Zero(), pontoControle, golAdversario = Vector2D.Zero();

            robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
            robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE;
            robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
            robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            robo.papel = RoboE1.Papel.Atacante;

            // if (!AjustaParede(ref robo))
            {
                if (bola.posicao.Distance(robo.posicao) > ConfiguracoesE1.TOLERANCIA_ATACANTE ||
                ((lado == ControleJogo.Lado.Direito) ? (robo.posicao.x < bola.proximaPosicao.x + ConfiguracoesE1.DIAMETRO_BOLA) :
                    (robo.posicao.x > bola.proximaPosicao.x - ConfiguracoesE1.DIAMETRO_BOLA)))
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }

                    saida.DesenharDestinoGoleiro(golAdversario);

                    if (lado == ControleJogo.Lado.Direito && bola.posicao.x > campo.area_goleiro_direita.ladoEsquerdo && (bola.posicao.y > campo.area_goleiro_direita.ladoSuperior || bola.posicao.y < campo.area_goleiro_direita.ladoInferior))
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else if (lado == ControleJogo.Lado.Esquerdo && bola.posicao.x < campo.area_goleiro_esquerda.ladoDireito && (bola.posicao.y > campo.area_goleiro_esquerda.ladoSuperior || bola.posicao.y < campo.area_goleiro_esquerda.ladoInferior)) //&& robo.posicao.x < campo.area_goleiro_direita.ladoEsquerdo && robo.posicao.y > campo.area_goleiro_direita.ladoSuperior)
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else
                        pontoControle = bola.proximaPosicao.Sub(golAdversario.Sub(bola.proximaPosicao).Unitary().Mult(robo.posicao.Distance(bola.proximaPosicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                    AjustaPontoControleCampo(ref pontoControle);

                    Vector2D[] pontos = CurvaBezier(robo.posicao, pontoControle, bola.proximaPosicao);
                    proximoPonto = pontos[2];
                    /*
                    if (BolaDentroArea())
                    {
                        if (lado == ControleJogo.Lado.Esquerdo)
                            proximoPonto = new Vector2D(campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                        else
                            proximoPonto = new Vector2D(campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                    }
                    */
                    //desenhar curva de belzier
                    
                    if (ConfiguracoesE1.DESENHAR_PONTOS)
                        saida.DesenharCurvaDelzier(pontos, pontoControle);
                    
                }
                else
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }
                    proximoPonto.x = golAdversario.x;
                    proximoPonto.y = golAdversario.y;


                    //desenhar posicao robo, bola e ponto desejado


                }

                if(!Lateral(ref proximoPonto, ref robo, ref bola))
                    robo.PosicionaAntiga(proximoPonto, ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE, ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE, ref saida);

                //if(CalculaDistanciaReta(robo.posicao, proximoPonto, ))
                //if (ConfiguracoesE1.DESENHAR_PONTOS) saida.DesenharPontosAtacante(robo.posicao, bola.posicao, proximoPonto);
                //robo.PosicionaPIAtacante(proximoPonto, bola.posicao);
                //robo.PosicionaPI(proximoPonto);
                //robo.PosicionaNaoLinear(proximoPonto, ref saida);
            }
        }
        public void AjustaPontoControleCampo(ref Vector2D ponto)
        {
            if (ponto.x < ambiente.Campo.limites.ladoEsquerdo)
                ponto.x = ambiente.Campo.limites.ladoEsquerdo;// + ConfiguracoesE1.TOLERANCIA_CAMPO; // somar traz o ponto de ref para dentro

            if (ponto.x > ambiente.Campo.limites.ladoDireito)
                ponto.x = ambiente.Campo.limites.ladoDireito;// - ConfiguracoesE1.TOLERANCIA_CAMPO; // subtrair traz o ponto de ref para dentro

            if (ponto.y < ambiente.Campo.limites.ladoInferior)
                ponto.y = ambiente.Campo.limites.ladoInferior;// + ConfiguracoesE1.TOLERANCIA_CAMPO;

            if (ponto.y > ambiente.Campo.limites.ladoSuperior)
                ponto.y = ambiente.Campo.limites.ladoSuperior;// - ConfiguracoesE1.TOLERANCIA_CAMPO;

        }


        public void DefinirPapeis()
        {
            int numRobos = ambiente.Time.Robo.Length;
            BolaE1 bola = ambiente.Bola;
            TimeE1 time = ambiente.Time;

            if (numRobos == 3)
            {
                bool robo1Atras = time.Robo[1].AtrasDaBola(time.lado, bola.proximaPosicao.x);
                bool robo2Atras = time.Robo[2].AtrasDaBola(time.lado, bola.proximaPosicao.x);

                if ((robo1Atras && robo2Atras) || (!robo1Atras && !robo2Atras))
                {
                    if (time.Robo[1].posicao.Distance(bola.proximaPosicao) <= time.Robo[2].posicao.Distance(bola.proximaPosicao))
                    {
                        AtacanteUnivector2(ref time.Robo[1]);
                        Zagueiro(ref time.Robo[2]);
                    }
                    else
                    {
                        AtacanteUnivector2(ref time.Robo[2]);
                        Zagueiro(ref time.Robo[1]);
                    }
                }
                else if (robo1Atras)
                {
                    AtacanteUnivector2(ref time.Robo[1]);
                    Zagueiro(ref time.Robo[2]);
                }
                else if (robo2Atras)
                {
                    AtacanteUnivector2(ref time.Robo[2]);
                    Zagueiro(ref time.Robo[1]);
                }
                else
                {
                    Zagueiro(ref time.Robo[1]);
                    Zagueiro(ref time.Robo[2]);
                }
                GoleiroMinhoca(ref time.Robo[0]);
            }

            else if (numRobos == 2)
            {
                Zagueiro(ref time.Robo[0]);
                Atacante(ref time.Robo[1]);
            }
            else if (numRobos == 1)
            {
                Atacante(ref time.Robo[0]);

                //Goleiro(ref time.Robo[0]);
                //Zagueiro(ref time.Robo[0]);
            }

        }

        public void ExecutaEstrategia(ref Ambiente ambiente_jogo)
        {

            ambiente.AtualizarPosicoes(ref ambiente_jogo);


            TimeE1 time = ambiente.Time;
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;



            bola.PreverPosicaoBola();


            //desenhar segmento de previsao de bola
            if (ConfiguracoesE1.DESENHAR_PONTOS)
                saida.DesenharPrevisaoBola(ref bola);

            //Atacante(ref time.Robo[0]);

            //PararRobos();
            //Zagueiro(ref time.Robo[0]);

            DefinirPapeis(); //para visualizar belzier com o jogo pausado            
            switch (ambiente.EstadoJogo)
            {
                case ControleJogo.Estado.Parado:
                    PararRobos();
                    break;
                case ControleJogo.Estado.Pausado:
                    PararRobos();
                    break;
                case ControleJogo.Estado.Executando:
                    //Atacante(ref time.Robo[0]);
                    //Goleiro(ref time.Robo[0]);
                    DefinirPapeis();
                    //time.Robo[0].Rotaciona(Math.PI / 2, ref saida);
                    //time.Robo[0].Posiciona_PI(campo.limites.centro, ref saida);
                    //Atacante(time.Robo[0]);
                    break;
                case ControleJogo.Estado.Posicionando:
                    //DefinirPapeis();
                    PararRobos();
                    if (time.Robo[controle.RoboPosicionando].posicao.Distance(controle.PosicionamentoRobo) > ConfiguracoesE1.TOLERANCIA_POSICAO)
                        time.Robo[controle.RoboPosicionando].Posiciona(controle.PosicionamentoRobo,
                            ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE, ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE);
                    break;
            }

            ambiente.DefinirVelocidades(ref ambiente_jogo);

            /*
            saida.ExibirValoresStr("ErroLinear", "SomaErroLinear", "ErroAngular", "SomaErroAngular", "ControleLinear", "ControleAngular");
            saida.ExibirValoresDouble(time.Robo[0].erro_linear_atacante, time.Robo[0].soma_erro_linear_atacante, time.Robo[0].erro_angular_atacante, time.Robo[0].soma_erro_angular_atacante, time.Robo[0].controlador_linear_atacante, time.Robo[0].controlador_angular_atacante);

            saida.ExibirValoresStr();
            */

            /*
            if (ConfiguracoesE1.EXIBIR_DADOS)
            {
                saida.ExibirValoresStr("SetP Dir", "SetP Esq", "Med Dir", "Med Esq", "PI Dir", "PI Esq");
                saida.ExibirValoresDouble(time.Robo[0].dadosPID.robo.velocidadeRodaDireita, time.Robo[0].dadosPID.robo.velocidadeRodaEsquerda,
                time.Robo[0].medicaoPID.robo.velocidadeRodaDireita, time.Robo[0].medicaoPID.robo.velocidadeRodaEsquerda,
                time.Robo[0].velocidadeRodaDireita, time.Robo[0].velocidadeRodaEsquerda);
                saida.ExibirValoresStr("Erro Dir", "Erro Esq", "Soma Erro Dir", "Soma Erro Esq");
                saida.ExibirValoresDouble(time.Robo[0].dadosPID.erroDir, time.Robo[0].dadosPID.erroEsq, time.Robo[0].dadosPID.somaErroDir, time.Robo[0].dadosPID.somaErroEsq);

                saida.ExibirValoresStr("Dest X", "Dest Y");
                saida.ExibirValoresDouble(campo.limites.centro.x, campo.limites.centro.y);
                saida.ExibirValoresStr("Pos X", "Pos Y", "Rotacao", "Alfa", "Teta");
                saida.ExibirValoresDouble(time.Robo[0].posicao.x, time.Robo[0].posicao.y, time.Robo[0].rotacao, time.Robo[0].dadosPID.alfa, time.Robo[0].dadosPID.teta);
                saida.ExibirValoresStr("Pos X Ant", "Pos Y Ant", "Rotacao Ant");
                saida.ExibirValoresDouble(time.Robo[0].anteriorPID.robo.posicao.x, time.Robo[0].anteriorPID.robo.posicao.y, time.Robo[0].anteriorPID.robo.rotacao);
            }
            */

            for (int i = 0; i < Configuracoes.QTD_TIME; i++)
                time.Robo[i].anteriorPID = time.Robo[i].dadosPID.Clone();

            //saida.ExibirValoresStr();

            //saida.ExibirValoresStr("KP Linear", "KI Linear", "KP Angular", "KI Angular");
            //saida.ExibirValoresDouble(ConfiguracoesE1.KP_LINEAR, ConfiguracoesE1.KI_LINEAR, ConfiguracoesE1.KP_ANGULAR, ConfiguracoesE1.KI_ANGULAR);




            //saida.ExibirValoresStr("Set Point", "Rotacao", "Vel Esq", "Vel Dir", "KP", "KI");
            //saida.ExibirValoresDouble(Math.PI / 2, time.Robo[0].rotacao, time.Robo[0].velocidadeRodaEsquerda, time.Robo[0].velocidadeRodaDireita, ConfiguracoesE1.KP_ROTACIONA, ConfiguracoesE1.KI_ROTACIONA);


            if (ConfiguracoesE1.EXIBIR_DADOS)
            {
                saida.ExibirDadosRobos(ref ambiente, ref time);
                saida.ExibirDadosBola(ref ambiente);

            }

            if (ConfiguracoesE1.GRAVAR_ARQUIVO)
                saida.SalvarDadosArquivo(ref ambiente);
        }

        public bool Lateral(ref Vector2D proximoPonto, ref RoboE1 robo, ref BolaE1 bola)
        {
            ControleJogo.Lado lado = ambiente.Time.lado;

            double LimiteVirtualsup = ambiente.Campo.limites.ladoSuperior - 40;// ConfiguracoesE1.TOLERANCIA_CAMPO;
            double LimiteVirtualinf = ambiente.Campo.limites.ladoInferior + 40;// ConfiguracoesE1.TOLERANCIA_CAMPO;
            double LimiteVirtualdir = ambiente.Campo.limites.ladoDireito - 50;// ConfiguracoesE1.TOLERANCIA_CAMPO;
            double LimiteVirtualesq = ambiente.Campo.limites.ladoEsquerdo + 50;// ConfiguracoesE1.TOLERANCIA_CAMPO;
            double rotacaoDesejadaLateral = Math.PI;


            if (robo.posicao.x < LimiteVirtualesq && bola.posicao.x < LimiteVirtualesq)
            {
                if(robo.posicao.y > ambiente.Campo.area_goleiro_direita.ladoSuperior && robo.posicao.y > bola.posicao.y)
                {
                    robo.AjustaAngulo(-Math.PI / 2);
                    return true;
                }
                else if (robo.posicao.y < ambiente.Campo.area_goleiro_direita.ladoInferior && robo.posicao.y < bola.posicao.y)
                {
                    robo.AjustaAngulo(Math.PI / 2);
                    return true;
                }
            }

            if (robo.posicao.y > LimiteVirtualsup && bola.posicao.y > LimiteVirtualsup)//se bola e atacante estão na lateral
            {
                if (robo.posicao.y < bola.posicao.y) //se a bola esta mais proxima da parede que o robo               
                {
                   robo.AjustaAngulo(rotacaoDesejadaLateral);
                   return true;
                }
            }
            else if (robo.posicao.y < LimiteVirtualinf && bola.posicao.y < LimiteVirtualinf)
            {
                if (robo.posicao.y > bola.posicao.y)
                {
                    robo.AjustaAngulo(rotacaoDesejadaLateral);
                    return true;
                }
            }
            return false;
        }

        public Vector2D[] CurvaBezier(Vector2D origem, Vector2D controle, Vector2D destino)
        {
            int n = (int)(1.0 / ConfiguracoesE1.PASSO_BELZIER);
            Vector2D[] pontos = new Vector2D[n];
            double t = ConfiguracoesE1.PASSO_BELZIER;
            pontos[0] = origem;
            for (int i = 1; i < n - 1; i++)
            {
                pontos[i] = ProximoCurvaBezier(t, pontos[i - 1], controle, destino);
                t += ConfiguracoesE1.PASSO_BELZIER;
            }
            pontos[n - 1] = destino;
            return pontos;
        }

        public Vector2D ProximoCurvaBezier(double t, Vector2D origem, Vector2D controle, Vector2D destino)
        {
            Vector2D proximo = new Vector2D();
            double aux = 1 - t;
            proximo.x = aux * aux * origem.x + 2 * t * aux * controle.x + t * t * destino.x;
            proximo.y = aux * aux * origem.y + 2 * t * aux * controle.y + t * t * destino.y;
            return proximo;
        }

        const double Q = 0.0015;

        public Vector2D[] CurvaBezierPotencial(Vector2D origem, Vector2D controle, Vector2D destino, Vector2D obstaculo)
        {
            double Fr, angulo, dist;
            Vector2D delta = Vector2D.Zero();

            int n = (int)(1.0 / ConfiguracoesE1.PASSO_BELZIER);
            Vector2D[] pontos = new Vector2D[n];
            Vector2D proximo = Vector2D.Zero();
            double t = ConfiguracoesE1.PASSO_BELZIER;

            pontos[0] = origem;
            for (int i = 1; i < n - 1; i++)
            {
                proximo = ProximoCurvaBezier(t, pontos[i - 1], controle, destino);
                dist = obstaculo.Distance(proximo);
                Fr = Math.Abs(Q / (dist));
                angulo = Math.Atan2(proximo.y - obstaculo.y, proximo.x - obstaculo.x);
                delta.x = 2 * Fr * Math.Cos(angulo);
                delta.y = 2 * Fr * Math.Sin(angulo);
                pontos[i] = proximo.Add(delta);
                t += ConfiguracoesE1.PASSO_BELZIER;
            }
            pontos[n - 1] = destino;
            return pontos;

        }

        public double CalculaDistanciaReta(Vector2D origem, Vector2D destino, Vector2D obstaculo)
        {
            double ta, tb, tc;
            //ta = origem.y - destino.y;
            //tb = destino.x - origem.x;
            //tc = (origem.x * destino.y) - (origem.y * destino.x);
            ta = (destino.y - origem.y) / (destino.x - origem.x);
            tb = -1;
            tc = origem.y - ta * origem.x;
            return (ta * obstaculo.x + tb * obstaculo.y + tc) / Math.Sqrt(ta * ta + tb * tb); ;
        }

        public Vector2D CalculaDerivada(ref Vector2D robo, ref Vector2D destino, ref Vector2D obstaculo, double raio)
        {
            double d = CalculaDistanciaReta(robo, destino, obstaculo);
            Vector2D robo_ = robo.Sub(obstaculo);
            //Vector2D robo_ = obstaculo.Sub(robo);

            double deltaX, deltaY;
            double circ = (raio * raio - robo_.x * robo_.x - robo_.y * robo_.y);
            double dir = d / Math.Abs(d);
            deltaX = dir * robo_.y + 200*robo_.x * circ;
            deltaY = -dir * robo_.x + 200*robo_.y * circ;

            Vector2D deltaVet = new Vector2D(deltaX, deltaY);

            //return deltaVet.Add(obstaculo);
            return deltaVet;
        }

        public void CurvaDesvio(ref Vector2D origem, ref Vector2D destino, ref Vector2D obstaculo)
        {
            float raio = 100;


            //List<Vector2D> ListaPontos = new List<Vector2D>();

            saida.DesenharCirculo(obstaculo, raio);
            saida.DesenharReta(origem, destino);


            
            Vector2D ponto1 = obstaculo.Clone();
            Vector2D ponto2 = Vector2D.Zero();

            ponto1.x -= 200;
            ponto1.y -= 200;

            for (int i = 0; i <= 400; i = i + 20)
            {
                for(int j = 0; j <= 400; j = j + 20)
                {
                    ponto2 = ponto1.Clone();
                    ponto2.x += i;
                    ponto2.y += j;

                    if (ponto2.x != obstaculo.x || ponto2.y != obstaculo.y)
                    {
                        var der = CalculaDerivada(ref ponto2, ref destino, ref obstaculo, raio);
                        var der1 = Redimensiona(der, 10);//delta.Mult(0.2);
                        var ponto3 = ponto2.Add(der1);

                        saida.DesenharVetores(ponto2, ponto3);
                    }
                }
            }
            
            Vector2D pontoAtual = origem.Clone();
            Vector2D proximoPonto = Vector2D.Zero();

            int p = 0;

            while (p < 50000)
            {
                //proximoPonto = Redimensiona(ProximoOrops(ref pontoAtual, ref destino, ref obstaculo).Sub(pontoAtual), 0.08).Add(pontoAtual);
                var delta = CalculaDerivada(ref pontoAtual, ref destino, ref obstaculo, raio);
                var delta1 = Redimensiona(delta, 10);//delta.Mult(0.2);
                //var delta1 = delta.Mult(0.000004);
                proximoPonto = pontoAtual.Add(delta1);
                //proximoPonto.x = pontoAtual.x + Math.Abs(delta1.x);
                //proximoPonto.y = pontoAtual.y + delta1.y;

                //proximoPonto = ProximoOrops(ref pontoAtual, ref destino, ref obstaculo);
                //var delta2 = Redimensiona(delta, 10);//delta.Mult(0.2);
                //var proximoPonto1 = pontoAtual.Add(delta2);

                saida.DesenharVetores(pontoAtual, proximoPonto);

                //ListaPontos.Add(proximoPonto);
                pontoAtual = proximoPonto.Clone();

                //                pontoAtual = pontoAtual.Mult(0.000004);
                p++;
            }
            //saida.DesenharVetores(ListaPontos.ToArray());
            //Vector2D[] pontos = ListaPontos.ToArray();
            //ListaPontos.Clear();
            
            //return ListaPontos;//pontos;
            //return Pontos;
        }

        public Vector2D Redimensiona(Vector2D vetor, double novoTamanho)
        {
            double alfa = vetor.Angle();
            double vx = Math.Cos(alfa) * novoTamanho;
            double vy = Math.Sin(alfa) * novoTamanho;
            return new Vector2D(vx, vy);
        }
        /*
                        public bool AjustaParede(ref RoboE1 robo)
                        {
                            Campo campo = ambiente.Campo;

                            if (Math.Abs(robo.posicao.y - campo.limites.ladoSuperior) < 0.08 || Math.Abs(robo.posicao.y - campo.limites.ladoInferior) < 0.08)
                            {

                                double rotacao_minima = Math.PI / 15;
                                if (Math.Abs(robo.rotacao) < Math.PI - rotacao_minima && Math.Abs(robo.rotacao) > rotacao_minima) {
                                    robo.Rotaciona(Math.PI);
                                    return true;
                                }                
                            }
                            else if ( (robo.posicao.y > campo.gol_direito.ladoSuperior || robo.posicao.y < campo.gol_direito.ladoInferior) && (Math.Abs(robo.posicao.x - campo.limites.ladoEsquerdo) < 0.08 || Math.Abs(robo.posicao.x - campo.limites.ladoDireito) < 0.08))
                            {
                                double rotacao_minima = Math.PI / 15;
                                if (Math.Abs(robo.rotacao) < Math.PI / 2 + rotacao_minima && Math.PI - Math.Abs(robo.rotacao) > Math.PI / 2 - rotacao_minima)
                                {
                                    robo.Rotaciona(Math.PI / 2);
                                    return true;
                                }
                            }
                            return false;
                        }
                */

        public void AtacanteUnivector(ref RoboE1 robo)
        {
            Vector2D proximoPonto = Vector2D.Zero(), pontoControle, golAdversario = Vector2D.Zero();

            robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
            robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE;
            robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
            robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            robo.papel = RoboE1.Papel.Atacante;

            DesenhaTrajetoriaSimplificada(robo.posicao, bola.posicao, 0, 1);

            //Repulsiva
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //k0 / d0 * n0
            //k0 coeficiente de atração
            //d0 disancia do robo para a bola
            //n0 univector na direção do robo para a bola

                double k0 = 8000;
            //double d0 = Math.Sqrt(Math.Pow((robo.posicao.x - bola.posicao.x),2) + Math.Pow((robo.posicao.y - bola.posicao.y), 2));
            double d0 = robo.posicao.Distance(bola.posicao);
            Vector2D n0 = new Vector2D();
            n0 = bola.posicao.Sub(robo.posicao).Unitary();

            Vector2D forcaR = new Vector2D(k0 / d0 * n0.x, k0 /  d0 * n0.y);
            Vector2D prox = new Vector2D(robo.posicao.x - forcaR.x, robo.posicao.y - forcaR.y);

            //saida.DesenharReta(robo.posicao, prox);

            // if (!AjustaParede(ref robo))
            {
                if (bola.posicao.Distance(robo.posicao) > ConfiguracoesE1.TOLERANCIA_ATACANTE ||
                ((lado == ControleJogo.Lado.Direito) ? (robo.posicao.x < bola.proximaPosicao.x + ConfiguracoesE1.DIAMETRO_BOLA) :
                    (robo.posicao.x > bola.proximaPosicao.x - ConfiguracoesE1.DIAMETRO_BOLA)))
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }

                    saida.DesenharDestinoGoleiro(golAdversario);

                    if (lado == ControleJogo.Lado.Direito && bola.posicao.x > campo.area_goleiro_direita.ladoEsquerdo && (bola.posicao.y > campo.area_goleiro_direita.ladoSuperior || bola.posicao.y < campo.area_goleiro_direita.ladoInferior))
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else if (lado == ControleJogo.Lado.Esquerdo && bola.posicao.x < campo.area_goleiro_esquerda.ladoDireito && (bola.posicao.y > campo.area_goleiro_esquerda.ladoSuperior || bola.posicao.y < campo.area_goleiro_esquerda.ladoInferior)) //&& robo.posicao.x < campo.area_goleiro_direita.ladoEsquerdo && robo.posicao.y > campo.area_goleiro_direita.ladoSuperior)
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else
                        pontoControle = bola.proximaPosicao.Sub(golAdversario.Sub(bola.proximaPosicao).Unitary().Mult(robo.posicao.Distance(bola.proximaPosicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                    AjustaPontoControleCampo(ref pontoControle);

                    Vector2D[] pontos = CurvaBezier(robo.posicao, pontoControle, bola.proximaPosicao);
                    proximoPonto = pontos[2];
                    /*
                    if (BolaDentroArea())
                    {
                        if (lado == ControleJogo.Lado.Esquerdo)
                            proximoPonto = new Vector2D(campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                        else
                            proximoPonto = new Vector2D(campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                    }
                    */
                    //desenhar curva de belzier

                    if (ConfiguracoesE1.DESENHAR_PONTOS)
                        saida.DesenharCurvaDelzier(pontos, pontoControle);

                }
                else
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }
                    proximoPonto.x = golAdversario.x;
                    proximoPonto.y = golAdversario.y;



                }
            }
        }
        public void DesenhaTrajetoria(Vector2D robo, Vector2D bola)
        {
            float raio = 100;

            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            //saida.DesenharCirculo(obstaculo, raio);
            //saida.DesenharReta(origem, destino);


            Vector2D ponto1 = campo.limites.centro.Clone();
            Vector2D ponto2 = Vector2D.Zero();

            ponto1.x -= 350;
            ponto1.y -= 300;

            for (int k = 0; k <= 700; k = k + 20)
            {
                for (int j = 0; j <= 600; j = j + 20)
                {
                    ponto2 = ponto1.Clone();
                    ponto2.x += k;
                    ponto2.y += j;

                    //if (ponto2.x != bola.x || ponto2.y != bola.y)
                    {
                        //TUF
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //constantes de e Kr
                        //de = 5.37 # cm -> raio da espiral
                        //kr = 4.15 # cm -> suavizacao da espiral 
                        //pixel2metros = 400
                        //de = 21.5
                        //kr = 16,6

                        //r = sqrt(x2 + y2) distancia entre origem e o ponto
                        //theta = arctan2(y / x)

                        //se r > de :
                        //HS_CW = theta + 90º * ((de + Kr) / (r + Kr))
                        //HS_CCW = theta - 90º * ((de + Kr) / (r + Kr))

                        //se r<de :
                        //HS_CW = theta + 90º* sqrt(r/ de)
                        //HS_CCW = theta - 90º* sqrt(r/ de)

                        //double de = 21.5;
                        //double kr = 16.6;
                        //Vector2D origem = new Vector2D();

                        //origem = ambiente.Campo.limites.centro;
                        //double x = ponto2.x - origem.x;
                        //double y = ponto2.y - origem.y;

                        //double r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                        //double theta = Math.Atan2(y, x);

                        //double g90 = Math.PI / 2;

                        //if(r<de)
                        //{
                        //    double hs_cw = theta + g90 * (de + kr) / (r + kr);
                        //    double hs_ccw = theta - g90 * (de + kr) / (r + kr);
                        //}
                        //else
                        //{
                        //    double hs_cw = theta + g90 * Math.Sqrt(r/de);
                        //    double hs_ccw = theta - g90 * Math.Sqrt(r/de);
                        //}
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ////em desenvolvimento

                        //AUF
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        //Vector2D[] obstaculos = new Vector2D[2] {bola, robo};
                        //double di, cosi = 0, seni = 0, cos = 0, sen = 0;

                        //for (int i = 0; i < obstaculos.Length; i++)
                        //{
                        //    di = Math.Sqrt(Math.Pow(ponto2.x - obstaculos[i].x, 2) + Math.Pow(ponto2.y - obstaculos[i].y, 2));
                        //    cosi = (ponto2.x - obstaculos[i].x) / di;
                        //    seni = (ponto2.y - obstaculos[i].y) / di;
                        //    cos += cosi / di;
                        //    sen += seni / di;
                        //}
                        ////arctan2(cos / sqrt(cos2 + sin2), sin / sqrt(cos2 + sin2))

                        //double aux = Math.Sqrt(Math.Pow(cos, 2) + Math.Pow(sen, 2));
                        //double AUF = Math.Atan2(cos / aux, sen / aux);

                        //Vector2D prox = new Vector2D(ponto2.x + Math.Sin(AUF) * 20, ponto2.y + Math.Cos(AUF) * 20);

                        //saida.DesenharReta(ponto2, prox);

                        //Atrativa
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //kt.dt.nt
                        //kt coeficiente de atração
                        //dt distancia do robo para a bola
                        //nt univector na direção do robo para a bola

                        //double kt = 50;
                        ////double dt = Math.Sqrt(Math.Pow((robo.posicao.x - bola.posicao.x),2) + Math.Pow((robo.posicao.y - bola.posicao.y), 2));
                        //double dt = robo.Distance(bola);
                        //dt = 1;
                        //Vector2D nt = new Vector2D();
                        //nt = bola.Sub(robo).Unitary();

                        //Vector2D forcaA = new Vector2D(kt * dt * nt.x, kt * dt * nt.y);
                        //Vector2D prox = new Vector2D(robo.x + forcaA.x, robo.y + forcaA.y);

                        //Vector2D retaGol = Vector2D.Zero();
                        //Vector2D posGol = Vector2D.Zero();
                        //retaGol.x = robo.x;

                        //if (lado == ControleJogo.Lado.Direito)
                        //{
                        //    posGol.x = campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        //    posGol.y = campo.gol_esquerdo.centro.y;
                        //}
                        //else
                        //{
                        //    posGol.x = campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        //    posGol.y = campo.gol_direito.centro.y;
                        //}

                        //double a = (bola.y - posGol.y) / (bola.x - posGol.x);
                        //double b = posGol.y - a * posGol.x;
                        //retaGol.y = a * retaGol.x + b;

                        //dt = retaGol.Distance(robo);
                        //nt = retaGol.Sub(robo).Unitary();
                        //kt = 0.5;
                        //Vector2D reta = new Vector2D(kt * dt * nt.x, kt * dt * nt.y);


                        //prox = prox.Add(reta);

                        ////saida.DesenharReta(robo, prox);
                        ////saida.DesenharReta(retaGol, posGol);

                        ////prox = pontoReta.Clone();
                        //var der1 = Redimensiona(prox, 1);//delta.Mult(0.2);
                        //var ponto3 = robo.Add(der1);



                        //saida.DesenharVetores(ponto2, ponto3);
                    }
                }
            }



        }


        public void DesenhaTrajetoriaSimplificada(Vector2D robo, Vector2D bola, int cont, int aux)
        {
            if (robo.x < bola.x)
                aux = 2;

            else if (robo.x > bola.x + 100)
                aux = 1;

            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            //Atrativa
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //kt.dt.nt
            //kt coeficiente de atração
            //dt distancia do robo para a bola
            //nt univector na direção do robo para a bola

            double kt = 10 + robo.Distance(bola) * 0.2;
            //double dt = Math.Sqrt(Math.Pow((robo.posicao.x - bola.posicao.x),2) + Math.Pow((robo.posicao.y - bola.posicao.y), 2));
            double dt = robo.Distance(bola);
            dt = 1;
            Vector2D nt = new Vector2D();
            nt = bola.Sub(robo).Unitary();

            Vector2D forcaA = new Vector2D(kt * dt * nt.x, kt * dt * nt.y);
            Vector2D prox = new Vector2D(forcaA.x,forcaA.y);

            Vector2D retaGol = Vector2D.Zero();
            Vector2D posGol = Vector2D.Zero();


            retaGol.x = robo.x;
            if(aux == 2)
                retaGol.x += Math.Abs(robo.y - bola.y);

            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_esquerdo.centro.y;
            }
            else
            {
                posGol.x = campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_direito.centro.y;
            }

            double a = (bola.y - posGol.y) / (bola.x - posGol.x);
            double b = posGol.y - a * posGol.x;
            retaGol.y = a * retaGol.x + b;

            dt = retaGol.Distance(robo);
            nt = retaGol.Sub(robo).Unitary();
            kt = 1;
            Vector2D reta = new Vector2D(kt * dt * nt.x, kt * dt * nt.y);

            Vector2D p1 = prox;
            Vector2D p2 = p1.Add(reta);
            Vector2D p3 = p2.Unitary().Mult(10).Add(robo);


            //saida.DesenharReta(robo, p3);
            saida.DesenharReta(retaGol, posGol);


            saida.DesenharVetores(robo, p3);
            if(p3.Distance(bola) > 10 && cont < 500)
            {
                cont++;
                DesenhaTrajetoriaSimplificada(p3, bola, cont, aux);
            }
        }

        public void AtacanteUnivector2(ref RoboE1 robo)
        {
            Vector2D proximoPonto = Vector2D.Zero(), pontoControle, golAdversario = Vector2D.Zero();

            robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
            robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE;
            robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
            robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            robo.papel = RoboE1.Papel.Atacante;

            //Repulsiva
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //x = robo
            //xi = obstaculos
            //di = distancia

            //vector2d [] obstaculos = new vector2d[1] { bola.posicao };
            //double di, cosi, seni, cos = 0, sen = 0;

            //for(int i = 0; i < obstaculos.length; i++)
            //{
            //    di = math.sqrt(math.pow(robo.posicao.x - obstaculos[i].x, 2) + math.pow(robo.posicao.y - obstaculos[i].y, 2));
            //    cosi = (robo.posicao.x - obstaculos[i].x) / di;
            //    seni = (robo.posicao.y - obstaculos[i].y) / di;
            //    cos += cosi / di;
            //    sen += seni / di;
            //}
            ////arctan2(cos / sqrt(cos2 + sin2), sin / sqrt(cos2 + sin2))

            //double aux = math.sqrt(math.pow(cos, 2) + math.pow(sen, 2));
            //double auf = math.atan2(cos/aux, sen/aux);

            //Atrativa
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            DesenhaTrajetoria(robo.posicao, bola.posicao);

            //saida.DesenharReta(robo.posicao, prox);

            // if (!AjustaParede(ref robo))
            {
                if (bola.posicao.Distance(robo.posicao) > ConfiguracoesE1.TOLERANCIA_ATACANTE ||
                ((lado == ControleJogo.Lado.Direito) ? (robo.posicao.x < bola.proximaPosicao.x + ConfiguracoesE1.DIAMETRO_BOLA) :
                    (robo.posicao.x > bola.proximaPosicao.x - ConfiguracoesE1.DIAMETRO_BOLA)))
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }

                    saida.DesenharDestinoGoleiro(golAdversario);

                    if (lado == ControleJogo.Lado.Direito && bola.posicao.x > campo.area_goleiro_direita.ladoEsquerdo && (bola.posicao.y > campo.area_goleiro_direita.ladoSuperior || bola.posicao.y < campo.area_goleiro_direita.ladoInferior))
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else if (lado == ControleJogo.Lado.Esquerdo && bola.posicao.x < campo.area_goleiro_esquerda.ladoDireito && (bola.posicao.y > campo.area_goleiro_esquerda.ladoSuperior || bola.posicao.y < campo.area_goleiro_esquerda.ladoInferior)) //&& robo.posicao.x < campo.area_goleiro_direita.ladoEsquerdo && robo.posicao.y > campo.area_goleiro_direita.ladoSuperior)
                    {
                        pontoControle = bola.posicao.Sub(campo.limites.centro.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));
                        pontoControle.y = (pontoControle.y + campo.limites.centro.y) / 2;
                    }
                    else
                        pontoControle = bola.proximaPosicao.Sub(golAdversario.Sub(bola.proximaPosicao).Unitary().Mult(robo.posicao.Distance(bola.proximaPosicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                    AjustaPontoControleCampo(ref pontoControle);

                    Vector2D[] pontos = CurvaBezier(robo.posicao, pontoControle, bola.proximaPosicao);
                    proximoPonto = pontos[2];
                    /*
                    if (BolaDentroArea())
                    {
                        if (lado == ControleJogo.Lado.Esquerdo)
                            proximoPonto = new Vector2D(campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                        else
                            proximoPonto = new Vector2D(campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_ZAGUEIRO, robo.posicao.y);
                    }
                    */
                    //desenhar curva de belzier

                    if (ConfiguracoesE1.DESENHAR_PONTOS)
                        saida.DesenharCurvaDelzier(pontos, pontoControle);

                }
                else
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                    {
                        golAdversario.x = campo.gol_direito.centro.x + ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_direito.centro.y;
                    }
                    else
                    {
                        golAdversario.x = campo.gol_esquerdo.centro.x - ConfiguracoesE1.DESLOCAMENTO_GOL;
                        golAdversario.y = campo.gol_esquerdo.centro.y;
                    }
                    proximoPonto.x = golAdversario.x;
                    proximoPonto.y = golAdversario.y;



                }
            }
        }















    }
}

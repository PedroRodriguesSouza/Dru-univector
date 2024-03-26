using Futebol.comum;
using Futebol.estrategias.estrategia2;
using Futebol.sincronismo;
using Futebol.src.estrategias.estrategia1;
using System;
using System.Collections.Generic;
using static Futebol.sincronismo.ControleJogo;

namespace Futebol.estrategias.estrategia1
{
    public class Estrategia1 : IEstrategia
    {
        private const double PI = Math.PI;
        private const double DOISPI = Math.PI * 2.0;
        private const double PIDOIS = Math.PI / 2.0;

        private AmbienteE1 ambiente;
        private ConfiguracoesE1 configuracoes;
        private SaidaE1 saida;
        private ControleJogo controle;

        int iGoleiro = 0;
        int iAtacante = 1;
        int iZagueiro = 2;

        public Estrategia1(int qtdTime, int qtdAdversario, ControleJogo controle)
        {
            saida = new SaidaE1(controle);
            ambiente = new AmbienteE1(qtdTime, qtdAdversario, ref saida);
            configuracoes = new ConfiguracoesE1();
            this.controle = controle;

            if (ConfiguracoesE1.GRAVAR_ARQUIVO)
                controle.saida.CriarArquivo("dados.txt");
        }

        public void PararRobos()
        {
            TimeE1 time = ambiente.Time;
            for (int i = 0; i < time.Robo.Length; i++)
                time.Robo[i].Parar();
        }

        //bool chegou = false;

        public bool BolaDentroArea(double coeficiente)
        {
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;
            Vector2D proximaPosicaoBola = bola.ProximaPosicao(coeficiente);

            if (lado == ControleJogo.Lado.Direito && proximaPosicaoBola.x > area.ladoEsquerdo && proximaPosicaoBola.x < area.ladoDireito &&
                proximaPosicaoBola.y < area.ladoSuperior && proximaPosicaoBola.y > area.ladoInferior)
                return true;
            else if (lado == ControleJogo.Lado.Esquerdo && proximaPosicaoBola.x < area.ladoDireito && proximaPosicaoBola.x > area.ladoEsquerdo &&
                proximaPosicaoBola.y < area.ladoSuperior && proximaPosicaoBola.y > area.ladoInferior)
                return true;
            return false;
        }

        public bool DentroArea(Vector2D ponto)
        {
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;

            if (lado == ControleJogo.Lado.Direito && ponto.x > area.ladoEsquerdo && ponto.x < area.ladoDireito &&
                ponto.y < area.ladoSuperior && ponto.y > area.ladoInferior)
                return true;
            else if (lado == ControleJogo.Lado.Esquerdo && ponto.x < area.ladoDireito && ponto.x > area.ladoEsquerdo &&
                ponto.y < area.ladoSuperior && ponto.y > area.ladoInferior)
                return true;
            return false;
        }

        public void SaturaForaCampo(ref Vector2D ponto, double tolerancia)
        {
            Limites campo = ambiente.Campo.limites;
            if (ponto.y > campo.ladoSuperior - tolerancia)
                ponto.y = campo.ladoSuperior - tolerancia;

            if (ponto.y < campo.ladoInferior + tolerancia)
                ponto.y = campo.ladoInferior + tolerancia;

            if (ponto.x < campo.ladoEsquerdo + tolerancia)
                ponto.x = campo.ladoEsquerdo + tolerancia;

            if (ponto.x > campo.ladoDireito - tolerancia)
                ponto.x = campo.ladoDireito - tolerancia;
        }

        public bool AtrasDaBola(int indiceRobo, ControleJogo.Lado lado, double posicaoXBola, Limites limitesCampo)
        {
            RoboE1 robo = ambiente.Time.Robo[indiceRobo];
            if (lado == ControleJogo.Lado.Direito)
            {
                if (robo.posicao.x >= posicaoXBola)
                {
                    /* if (iZagueiro == indiceRobo && robo.posicao.x < posicaoXBola + ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE)
                     {
                         saida.DesenharReta(Vector2D.Create(posicaoXBola + ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE, Configuracoes.YMAX - limitesCampo.ladoSuperior), Vector2D.Create(posicaoXBola + ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE, Configuracoes.YMAX - limitesCampo.ladoInferior));
                         return false;
                     }*/
                    return true;
                }
            }
            else
            {
                if (robo.posicao.x <= posicaoXBola)
                {
                    /*if (iZagueiro == indiceRobo && robo.posicao.x > posicaoXBola - ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE)
                    {
                        saida.DesenharReta(Vector2D.Create(posicaoXBola - ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE, limitesCampo.ladoSuperior), Vector2D.Create(posicaoXBola - ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE, limitesCampo.ladoInferior));
                        return false;
                    }*/
                    return true;
                }
            }
            return false;
        }

        public void GoleiroMinhoca(ref RoboE1 robo)
        {
            //Declara e atualiza variaveis
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            //ControleJogo.Lado lado = ambiente.Time.lado;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;
            Limites gol = lado == ControleJogo.Lado.Direito ? campo.gol_direito : campo.gol_esquerdo;

            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.GOLEIRO_COEFICIENTE_PREVISAO_BOLA);

            //Calcula o destino segundo a linha do goleiro
            Vector2D destino = Vector2D.Zero();

            //Captura a posição do centro do gol
            Vector2D posGol = Vector2D.Zero();
            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL; //gol.centro.x;//gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = gol.centro.y;

                destino.x = area.ladoDireito - ConfiguracoesE1.GOLEIRO_DESLOCAMENTO_X;
            }
            else
            {
                posGol.x = gol.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL; //gol.centro.x;//
                posGol.y = gol.centro.y;

                destino.x = area.ladoEsquerdo + ConfiguracoesE1.GOLEIRO_DESLOCAMENTO_X;
            }

            //Define o valor de y da posição destino
            if (posGol.x != proximaPosicaoBola.x)
            {
                double a = (proximaPosicaoBola.y - posGol.y) / (proximaPosicaoBola.x - posGol.x);
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
                //destino.y = Math.Max(0.3f, Math.Min(campo.limites.altura - 0.3f, destino.y));

            }
            else
                destino.y = area.centro.y;

            //Define um valor máximo e mínimo de y para a posição do goleiro
            if (destino.y > area.ladoSuperior) destino.y = area.ladoSuperior - ConfiguracoesE1.GOLEIRO_DESLOCAMENTO_Y;
            if (destino.y < area.ladoInferior) destino.y = area.ladoInferior + ConfiguracoesE1.GOLEIRO_DESLOCAMENTO_Y;

            //Verifica se o robo está dentro do gol
            if (robo.posicao.x > campo.area_goleiro_direita.ladoDireito || robo.posicao.x < campo.area_goleiro_esquerda.ladoEsquerdo)
            {
                if (destino.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                else if (destino.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
            }

            //Se a bola esta dentro da área o goleiro tenta tirar
            if (BolaDentroArea(ConfiguracoesE1.GOLEIRO_COEFICIENTE_PREVISAO_BOLA))
            {
                //robo.PosicionaMinhocaNaoLinear(bola.proximaPosicao, ref saida); //2theta
                //robo.PosicionaAntiga(bola.proximaPosicao, ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ref saida);

                // robo.PosicionaPID_Henrique(destino, ConfiguracoesE1.ATACANTE_KP_LINEAR, ConfiguracoesE1.ATACANTE_KI_LINEAR, ConfiguracoesE1.ATACANTE_KD_LINEAR, ConfiguracoesE1.ATACANTE_KP_ANGULAR, ConfiguracoesE1.ATACANTE_KI_ANGULAR, ConfiguracoesE1.ATACANTE_KD_ANGULAR, ConfiguracoesE1.ATACANTE_KP_ROTACIONA_ANGULAR, ConfiguracoesE1.ATACANTE_KI_ROTACIONA_ANGULAR, ConfiguracoesE1.ATACANTE_KD_ROTACIONA_ANGULAR, ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_FATOR_VELOCIDADE, false);

                saida.ExibirValoresStr("Bola dentro da area");

                SaturaForaCampo(ref destino, ConfiguracoesE1.TOLERANCIA_CAMPO);

                robo.PosicionaPID_Henrique(iGoleiro, destino, 200,
                ConfiguracoesE1.GOLEIRO_F_LINEAR_NABOLA_KP, ConfiguracoesE1.GOLEIRO_F_LINEAR_NABOLA_KI, ConfiguracoesE1.GOLEIRO_F_LINEAR_NABOLA_KD,
                ConfiguracoesE1.GOLEIRO_F_ANGULAR_NABOLA_KP, ConfiguracoesE1.GOLEIRO_F_ANGULAR_NABOLA_KI, ConfiguracoesE1.GOLEIRO_F_ANGULAR_NABOLA_KD,
                ConfiguracoesE1.GOLEIRO_T_LINEAR_NABOLA_KP, ConfiguracoesE1.GOLEIRO_T_LINEAR_NABOLA_KI, ConfiguracoesE1.GOLEIRO_T_LINEAR_NABOLA_KD,
                ConfiguracoesE1.GOLEIRO_T_ANGULAR_NABOLA_KP, ConfiguracoesE1.GOLEIRO_T_ANGULAR_NABOLA_KI, ConfiguracoesE1.GOLEIRO_T_ANGULAR_NABOLA_KD,
                ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KD,
                ConfiguracoesE1.GOLEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.GOLEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.GOLEIRO_F_FATOR_VELOCIDADE, ConfiguracoesE1.GOLEIRO_T_FATOR_VELOCIDADE, false);

                /*robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
                robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX;
                robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
                robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

                robo.PosicionaPI(bola.proximaPosicao, true);*/
                //robo.PosicionaMinhoca(ref destino);
            }

            //Se a bola estiver fora ele deve assumir a posição calculada
            else
            {
                /*
                robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO;
                robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO;
                robo.dadosPID.KP = ConfiguracoesE1.KP_GOLEIRO;
                robo.dadosPID.KI = ConfiguracoesE1.KI_GOLEIRO;
                */
                // Mantem o Robo perto la linha Fixa em X
                if (robo.posicao.x < destino.x - ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X || robo.posicao.x > destino.x + ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X)
                {
                    saida.ExibirValoresStr("Fora da linha");

                    //fora da tolerancia na linha X          
                    //robo.PosicionaMinhoca(ref destino, ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO);
                    /*
                    robo.PosicionaPID_Henrique(iGoleiro, destino, 500,
                            ConfiguracoesE1.ATACANTE_1_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KD,
                            ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KD,
                            ConfiguracoesE1.ATACANTE_1_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KD,
                            ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KD,
                            ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                            ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_1_T_FATOR_VELOCIDADE);
                    */

                    SaturaForaCampo(ref destino, ConfiguracoesE1.TOLERANCIA_CAMPO);


                    robo.PosicionaPID_Henrique(iGoleiro, destino, 200,
                        ConfiguracoesE1.GOLEIRO_F_LINEAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_F_LINEAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_F_LINEAR_FORALINHA_KD,
                        ConfiguracoesE1.GOLEIRO_F_ANGULAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_F_ANGULAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_F_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.GOLEIRO_T_LINEAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_T_LINEAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_T_LINEAR_FORALINHA_KD,
                        ConfiguracoesE1.GOLEIRO_T_ANGULAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_T_ANGULAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_T_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.GOLEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.GOLEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.GOLEIRO_F_FATOR_VELOCIDADE, ConfiguracoesE1.GOLEIRO_T_FATOR_VELOCIDADE, false);

                    //robo.PosicionaMinhocaNaoLinear(destino, ref saida);
                    //robo.PosicionaAntiga(destino, ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO, ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO, ref saida);
                    //robo.PosicionaPI(destino, true);
                    //robo.PosicionaPI(destino, true);
                }
                else
                {
                    // Se ele está proximo o suficiente do ponto ele mantem a posição
                    if (robo.posicao.y <= destino.y + ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_Y
                        && robo.posicao.y >= destino.y - ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_Y)
                    {
                        saida.ExibirValoresStr("Rotaciona");
                        double tetaE = robo.CalcularErroAngularDuasFrentes(PIDOIS);
                        saida.ExibirValores("TetaE: ", tetaE);
                        if (Math.Abs(tetaE) <= ConfiguracoesE1.GOLEIRO_TOLERANCIA_ANGULO)  //esta no ponto e na rotacao desejada
                            robo.Parar();
                        else //esta no ponto, mas nao rotacao desejada                         
                            robo.RotacionaPID_Henrique(iGoleiro, PIDOIS, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_KD, ConfiguracoesE1.GOLEIRO_ROTACIONA_MAX);
                        //robo.RotacionaAntiga(Math.PI / 2, ConfiguracoesE1.ROTACIONA_MAX_GOLEIRO);
                    }
                    else  // ele esta na linha mas nao na tolerancia de Y, fora do ponto
                    {
                        saida.ExibirValoresStr("Na linha");

                        SaturaForaCampo(ref destino, ConfiguracoesE1.TOLERANCIA_CAMPO);


                        robo.PosicionaPID_Henrique(iGoleiro, destino, 200,
                            ConfiguracoesE1.GOLEIRO_F_LINEAR_NALINHA_KP, ConfiguracoesE1.GOLEIRO_F_LINEAR_NALINHA_KI, ConfiguracoesE1.GOLEIRO_F_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.GOLEIRO_F_ANGULAR_NALINHA_KP, ConfiguracoesE1.GOLEIRO_F_ANGULAR_NALINHA_KI, ConfiguracoesE1.GOLEIRO_F_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.GOLEIRO_T_LINEAR_NALINHA_KP, ConfiguracoesE1.GOLEIRO_T_LINEAR_NALINHA_KI, ConfiguracoesE1.GOLEIRO_T_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.GOLEIRO_T_ANGULAR_NALINHA_KP, ConfiguracoesE1.GOLEIRO_T_ANGULAR_NALINHA_KI, ConfiguracoesE1.GOLEIRO_T_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.GOLEIRO_ROTACIONA_ANGULAR_FORALINHA_KD,
                            ConfiguracoesE1.GOLEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.GOLEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.GOLEIRO_F_FATOR_VELOCIDADE, ConfiguracoesE1.GOLEIRO_T_FATOR_VELOCIDADE, false);

                        //robo.AjustaMinhoca(ref destino, ConfiguracoesE1.VEL_LINEAR_MAX_ZAGUEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_ZAGUEIRO);
                        //robo.PosicionaAntiga(destino, ConfiguracoesE1.VEL_ANGULAR_MAX_GOLEIRO, ConfiguracoesE1.VEL_LINEAR_MAX_GOLEIRO, ref saida);
                        //robo.PosicionaMinhocaNaoLinear(destino, ref saida, true);                
                    }
                }
            }
            if (ConfiguracoesE1.DESENHAR_PONTOS)
            {
                saida.DesenharDestinoGoleiro(destino);
                saida.DesenharReta(Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoInferior));
                saida.DesenharReta(Vector2D.Create(destino.x + ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x + ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));
                saida.DesenharReta(Vector2D.Create(destino.x - ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x - ConfiguracoesE1.GOLEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));
            }
        }

        public void ZagueiroMinhoca(ref RoboE1 robo)
        {
            //Declara e atualiza variaveis
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            //ControleJogo.Lado lado = ambiente.Time.lado;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Limites area = lado == ControleJogo.Lado.Direito ? campo.area_goleiro_direita : campo.area_goleiro_esquerda;
            Limites gol = lado == ControleJogo.Lado.Direito ? campo.gol_direito : campo.gol_esquerdo;

            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.GOLEIRO_COEFICIENTE_PREVISAO_BOLA);

            //Calcula o destino segundo a linha do goleiro
            Vector2D destino = Vector2D.Zero();

            //Captura a posição do centro do gol
            Vector2D posGol = Vector2D.Zero();

            bool usarCamposPotenciais = false;
            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL; //gol.centro.x;//gol.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = gol.centro.y;

                destino.x = area.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
                usarCamposPotenciais = (robo.posicao.x < proximaPosicaoBola.x);

            }
            else
            {
                posGol.x = gol.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL; //gol.centro.x;//
                posGol.y = gol.centro.y;

                destino.x = area.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
                usarCamposPotenciais = (robo.posicao.x > proximaPosicaoBola.x);

            }







            //Define o valor de y da posição destino
            if (posGol.x != proximaPosicaoBola.x)
            {
                double a = (proximaPosicaoBola.y - posGol.y) / (proximaPosicaoBola.x - posGol.x);
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
                //destino.y = Math.Max(0.3f, Math.Min(campo.limites.altura - 0.3f, destino.y));

            }
            else
                destino.y = area.centro.y;

            if (destino.y > campo.limites.ladoSuperior - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoSuperior - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y;
            if (destino.y < campo.limites.ladoInferior + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoInferior + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y;


            // else
            //{
            //destino = DesviaZagueiro(ref destino, ref robo, ref bola);
            //Verifica se o robo está dentro do gol
            /*
            if (lado == ControleJogo.Lado.Direito && robo.posicao.x > campo.gol_direito.ladoEsquerdo)
            {
                if (destino.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                else if (destino.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
            }

            //Mesma verificação para o outro lado
            else if (lado == ControleJogo.Lado.Esquerdo && robo.posicao.x < campo.gol_esquerdo.ladoDireito)
            {
                if (destino.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                else if (destino.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    destino.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
            }*/

            //Se a bola estiver fora ele deve assumir a posição calculada
            //else
            //{
            // Mantem o Robo perto la linha Fixa em X
            if (robo.posicao.x < destino.x - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X || robo.posicao.x > destino.x + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X)
            {
                saida.ExibirValoresStr("Fora da linha");
                Vector2D proximoPonto = destino.Clone();

                if (DentroArea(proximoPonto))
                {
                    double distSup, distInf, distX;

                    if (lado == ControleJogo.Lado.Direito)
                    {
                        distSup = campo.area_goleiro_direita.ladoSuperior - proximoPonto.y;
                        distInf = proximoPonto.y - campo.area_goleiro_direita.ladoInferior;
                        distX = proximoPonto.x - campo.area_goleiro_direita.ladoEsquerdo;

                        if (distSup < distInf && distSup < distX)
                            proximoPonto.y = campo.area_goleiro_direita.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else if (distInf < distSup && distInf < distX)
                            proximoPonto.y = campo.area_goleiro_direita.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else
                            proximoPonto.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.TOLERANCIA_CAMPO;
                    }
                    else
                    {
                        distSup = campo.area_goleiro_esquerda.ladoSuperior - proximoPonto.y;
                        distInf = proximoPonto.y - campo.area_goleiro_esquerda.ladoInferior;
                        distX = campo.area_goleiro_esquerda.ladoDireito - proximoPonto.x;

                        if (distSup < distInf && distSup < distX)
                            proximoPonto.y = campo.area_goleiro_esquerda.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else if (distInf < distSup && distInf < distX)
                            proximoPonto.y = campo.area_goleiro_esquerda.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else
                            proximoPonto.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.TOLERANCIA_CAMPO;
                    }
                }

                if (robo.posicao.x > campo.area_goleiro_direita.ladoDireito || robo.posicao.x < campo.area_goleiro_esquerda.ladoEsquerdo)
                {
                    if (proximoPonto.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                        proximoPonto.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                    else if (proximoPonto.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                        proximoPonto.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                }

                SaturaForaCampo(ref proximoPonto, ConfiguracoesE1.TOLERANCIA_CAMPO);

                if (iZagueiro == 1)
                    robo.PosicionaPID_Henrique(iZagueiro, proximoPonto, 100,
                        ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KD,
                        ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_1_T_FATOR_VELOCIDADE);
                else
                    robo.PosicionaPID_Henrique(iZagueiro, proximoPonto, 100,
                        ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KD,
                        ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_2_T_FATOR_VELOCIDADE);







                //if (usarCamposPotenciais && robo.posicao.Distance(bola.posicao) < ConfiguracoesE1.RAIO_POTENCIAL)
                //{
                //   CamposPotenciaisAntigo(robo, destino, true);
                //}
                // else
                //{
                /*
                    if (robo.posicao.x <= campo.limites.ladoEsquerdo + ConfiguracoesE1.TOLERANCIA_CAMPO ||
                       robo.posicao.x >= campo.limites.ladoDireito - ConfiguracoesE1.TOLERANCIA_CAMPO ||
                       robo.posicao.y >= campo.limites.ladoSuperior - ConfiguracoesE1.TOLERANCIA_CAMPO ||
                       robo.posicao.y <= campo.limites.ladoInferior + ConfiguracoesE1.TOLERANCIA_CAMPO)
                    {
                        if (iZagueiro == 1)
                            robo.PosicionaRotacionaPID_Henrique(iZagueiro, destino, 100,
                                ConfiguracoesE1.ATACANTE_1_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_1_T_FATOR_VELOCIDADE);
                        else
                            robo.PosicionaRotacionaPID_Henrique(iZagueiro, destino, 100,
                                ConfiguracoesE1.ATACANTE_2_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_2_T_FATOR_VELOCIDADE);

                    }
                    else
                    {
                        if (iZagueiro == 1)
                            robo.PosicionaPID_Henrique(iZagueiro, destino, 100,
                                ConfiguracoesE1.ATACANTE_1_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_1_T_FATOR_VELOCIDADE);
                        else
                            robo.PosicionaPID_Henrique(iZagueiro, destino, 100,
                                ConfiguracoesE1.ATACANTE_2_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_2_T_FATOR_VELOCIDADE);

                    }
                    */




            }
            else
            {
                // Se ele está proximo o suficiente do ponto ele mantem a posição
                if (robo.posicao.y <= destino.y + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_Y
                    && robo.posicao.y >= destino.y - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_Y)
                {
                    saida.ExibirValoresStr("Rotaciona");
                    double tetaE = robo.CalcularErroAngularDuasFrentes(PIDOIS);
                    saida.ExibirValores("TetaE: ", tetaE);
                    if (Math.Abs(tetaE) <= ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_ANGULO)  //esta no ponto e na rotacao desejada
                        robo.Parar();
                    else //esta no ponto, mas nao rotacao desejada   
                    {




                        if (iZagueiro == 1)
                        {
                            robo.RotacionaPID_Henrique(iZagueiro, PIDOIS, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KD, ConfiguracoesE1.ZAGUEIRO_ROTACIONA_MAX);
                        }
                        else
                        {
                            robo.RotacionaPID_Henrique(iZagueiro, PIDOIS, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KD, ConfiguracoesE1.ZAGUEIRO_ROTACIONA_MAX);
                        }



                    }
                    //robo.RotacionaAntiga(Math.PI / 2, ConfiguracoesE1.ROTACIONA_MAX_GOLEIRO);
                }
                else  // ele esta na linha mas nao na tolerancia de Y, fora do ponto
                {
                    saida.ExibirValoresStr("Na linha");

                    if (DentroArea(destino))
                    {
                        double distSup, distInf, distX;

                        if (lado == ControleJogo.Lado.Direito)
                        {
                            distSup = campo.area_goleiro_direita.ladoSuperior - destino.y;
                            distInf = destino.y - campo.area_goleiro_direita.ladoInferior;
                            distX = destino.x - campo.area_goleiro_direita.ladoEsquerdo;

                            if (distSup < distInf && distSup < distX)
                                destino.y = campo.area_goleiro_direita.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                            else if (distInf < distSup && distInf < distX)
                                destino.y = campo.area_goleiro_direita.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                            else
                                destino.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.TOLERANCIA_CAMPO;
                        }
                        else
                        {
                            distSup = campo.area_goleiro_esquerda.ladoSuperior - destino.y;
                            distInf = destino.y - campo.area_goleiro_esquerda.ladoInferior;
                            distX = campo.area_goleiro_esquerda.ladoDireito - destino.x;

                            if (distSup < distInf && distSup < distX)
                                destino.y = campo.area_goleiro_esquerda.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                            else if (distInf < distSup && distInf < distX)
                                destino.y = campo.area_goleiro_esquerda.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                            else
                                destino.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.TOLERANCIA_CAMPO;
                        }
                    }

                    if (robo.posicao.x > campo.area_goleiro_direita.ladoDireito || robo.posicao.x < campo.area_goleiro_esquerda.ladoEsquerdo)
                    {
                        if (destino.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                            destino.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                        else if (destino.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                            destino.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                    }

                    SaturaForaCampo(ref destino, ConfiguracoesE1.TOLERANCIA_CAMPO);

                    if (iZagueiro == 1)
                        robo.PosicionaPID_Henrique(iZagueiro, destino, 100,
                            ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_1_T_FATOR_VELOCIDADE);
                    else
                        robo.PosicionaPID_Henrique(iZagueiro, destino, 100,
                            ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_NALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_NALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_NALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KD,
                            ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_2_T_FATOR_VELOCIDADE);

                }
            }


            if (ConfiguracoesE1.DESENHAR_PONTOS)
            {
                saida.DesenharDestinoGoleiro(destino);
                saida.DesenharReta(Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoInferior));
                saida.DesenharReta(Vector2D.Create(destino.x + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));
                saida.DesenharReta(Vector2D.Create(destino.x - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));
            }
        }

        public void Zagueiro(ref RoboE1 robo)
        {

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            Vector2D destino = Vector2D.Zero();
            Vector2D posGol = Vector2D.Zero();

            double distBola = bola.posicao.Distance(robo.posicao);

            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);

            if (ConfiguracoesE1.DESENHAR_PONTOS)
                saida.DesenharPrevisaoBola(ref bola, ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);

            Vector2D golAdversario = Vector2D.Zero();

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


            //Define o lado do campo à defender e a posição em x
            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_direito.centro.y;
                destino.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
            }
            else
            {
                posGol.x = campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_esquerdo.centro.y;
                destino.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
            }

            //Calcula a posição em y
            if (posGol.x != bola.posicao.x)
            {
                double a = (bola.posicao.y - posGol.y) / (bola.posicao.x - posGol.x);
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
            }
            else
                destino.y = posGol.y;

            double deslocamentoY = ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y;

            if (lado == Lado.Direito && bola.posicao.x > campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X)
            {
                deslocamentoY = campo.limites.ladoSuperior - campo.area_goleiro_direita.ladoSuperior;

            }
            else if (lado == Lado.Esquerdo && bola.posicao.x < campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X)
            {
                deslocamentoY = campo.limites.ladoSuperior - campo.area_goleiro_direita.ladoSuperior;
            }

            //var bola_prev = bola.ProximaPosicao(ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);
            //destino.y = bola_prev.y;
            //Define um valor máximo e mínimo para y
            if (destino.y > campo.limites.ladoSuperior - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoSuperior - deslocamentoY;
            if (destino.y < campo.limites.ladoInferior + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoInferior + deslocamentoY;


            /////////////////////////////////////////////////////////////////////////////////////////
            saida.DesenharReta(Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x, Configuracoes.YMAX - campo.limites.ladoInferior));
            saida.DesenharReta(Vector2D.Create(destino.x + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x + ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));
            saida.DesenharReta(Vector2D.Create(destino.x - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(destino.x - ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_POSICAO_X, Configuracoes.YMAX - campo.limites.ladoInferior));


            if (!AtrasDaBola(iZagueiro, lado, bola.posicao.x, campo.limites))
            {
                Vector2D pontoControle = new Vector2D(destino.x, destino.y < campo.limites.centro.y ?
                    campo.limites.ladoSuperior - (campo.limites.ladoSuperior - campo.limites.centro.y) / 2.0 :
                    campo.limites.ladoInferior + (campo.limites.centro.y - campo.limites.ladoInferior) / 2.0);

                //pontoControle = proximaPosicaoBola.Sub(golAdversario.Sub(proximaPosicaoBola).Unitary().Mult(robo.posicao.Distance(proximaPosicaoBola) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                AjustaPontoControleCampo(ref pontoControle);


                Vector2D[] pontos = CurvaBezierPotencial(robo.posicao, pontoControle, destino, bola.posicao, ambiente.Time.Robo[iAtacante].posicao);
                Vector2D proximoPonto = pontos[1];


                if (iZagueiro == 1)
                    robo.PosicionaRotacionaPID_Henrique(iZagueiro, proximoPonto, 500,
                        ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_1_T_FATOR_VELOCIDADE);
                else //if(iZagueiro == 2)
                    robo.PosicionaRotacionaPID_Henrique(iZagueiro, proximoPonto, 500,
                        ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_2_T_FATOR_VELOCIDADE);




                //desenhar curva de belzier
                if (ConfiguracoesE1.DESENHAR_PONTOS)
                    saida.DesenharCurvaDelzier(pontos, pontoControle);
                saida.DesenharDestinoZagueiro(pontoControle);

                return;
            }


            //Envia valor de posição para a função de posicionamento
            //Caso esteja muito proximo do ponto ele se mantem na posição atual
            //Caso contrario ele tenta se posicionar no objetivo
            if (ConfiguracoesE1.DESENHAR_PONTOS)
            {
                saida.DesenharDestinoZagueiro(destino);
            }
            if (robo.posicao.Distance(destino) < ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_PONTO)
            {
                robo.Parar();
                return;
            }
            else
            {
                if (DentroArea(destino))
                {
                    double distSup, distInf, distX;

                    if (lado == ControleJogo.Lado.Direito)
                    {
                        distSup = campo.area_goleiro_direita.ladoSuperior - destino.y;
                        distInf = destino.y - campo.area_goleiro_direita.ladoInferior;
                        distX = destino.x - campo.area_goleiro_direita.ladoEsquerdo;

                        if (distSup < distInf && distSup < distX)
                            destino.y = campo.area_goleiro_direita.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else if (distInf < distSup && distInf < distX)
                            destino.y = campo.area_goleiro_direita.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else
                            destino.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.TOLERANCIA_CAMPO;
                    }
                    else
                    {
                        distSup = campo.area_goleiro_esquerda.ladoSuperior - destino.y;
                        distInf = destino.y - campo.area_goleiro_esquerda.ladoInferior;
                        distX = campo.area_goleiro_esquerda.ladoDireito - destino.x;

                        if (distSup < distInf && distSup < distX)
                            destino.y = campo.area_goleiro_esquerda.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else if (distInf < distSup && distInf < distX)
                            destino.y = campo.area_goleiro_esquerda.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                        else
                            destino.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.TOLERANCIA_CAMPO;
                    }
                }

                //Verifica se o robo está dentro do gol
                if (robo.posicao.x > campo.area_goleiro_direita.ladoDireito || robo.posicao.x < campo.area_goleiro_esquerda.ladoEsquerdo)
                {
                    if (destino.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                        destino.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                    else if (destino.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                        destino.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                }

                SaturaForaCampo(ref destino, ConfiguracoesE1.TOLERANCIA_CAMPO);

                //robo.PosicionaAntiga(destino, ConfiguracoesE1.VEL_LINEAR_MAX_ZAGUEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_ZAGUEIRO, ConfiguracoesE1.VEL_ANGULAR_MAX_ZAGUEIRO);
                if (iZagueiro == 1)
                    robo.PosicionaRotacionaPID_Henrique(iZagueiro, destino, 500,
                        ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_1_T_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_1_ROTACIONA_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_1_T_FATOR_VELOCIDADE);
                else //if(iZagueiro == 2)
                    robo.PosicionaRotacionaPID_Henrique(iZagueiro, destino, 500,
                        ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_F_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_LINEAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KP, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KI, ConfiguracoesE1.ZAGUEIRO_2_T_ANGULAR_KD,
                        ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KP, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KI, ConfiguracoesE1.ZAGUEIRO_2_ROTACIONA_ANGULAR_FORALINHA_KD,
                        ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ZAGUEIRO_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ZAGUEIRO_2_T_FATOR_VELOCIDADE);

                //Desenha ponto desejado para o zagueiro
            }


        }
        public void Atacante(ref RoboE1 robo)
        {
            bool noPonto = false;
            Vector2D proximoPonto = Vector2D.Zero(), pontoControle, golAdversario = Vector2D.Zero();

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.ATACANTE_COEFICIENTE_PREVISAO_BOLA);

            if (ConfiguracoesE1.DESENHAR_PONTOS)
                saida.DesenharPrevisaoBola(ref bola, ConfiguracoesE1.ATACANTE_COEFICIENTE_PREVISAO_BOLA);

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

            if (lado == Lado.Direito)
                saida.DesenharReta(Vector2D.Create(bola.posicao.x + 3 * ConfiguracoesE1.ATACANTE_TOLERANCIA, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(bola.posicao.x + 3 * ConfiguracoesE1.ATACANTE_TOLERANCIA, Configuracoes.YMAX - campo.limites.ladoInferior));
            else
                saida.DesenharReta(Vector2D.Create(bola.posicao.x - 3 * ConfiguracoesE1.ATACANTE_TOLERANCIA, Configuracoes.YMAX - campo.limites.ladoSuperior), Vector2D.Create(bola.posicao.x - 3 * ConfiguracoesE1.ATACANTE_TOLERANCIA, Configuracoes.YMAX - campo.limites.ladoInferior));

            double distReta = CalculaDistanciaReta(proximaPosicaoBola, golAdversario, robo.posicao);

            double distBola = bola.posicao.Distance(robo.posicao);

            /*if (!AtrasDaBola(iAtacante, lado, bola.posicao.x, campo.limites) && distBola < ConfiguracoesE1.ATACANTE_TOLERANCIA && 
                ((lado == Lado.Direito && bola.posicao.x < campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X) ||
                (lado == Lado.Esquerdo && bola.posicao.x > campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X)))
            {
                robo.Parar();
                return;
            }
            else */
            if (distBola > 3 * ConfiguracoesE1.ATACANTE_TOLERANCIA)
            {

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
                    pontoControle = proximaPosicaoBola.Sub(golAdversario.Sub(proximaPosicaoBola).Unitary().Mult(robo.posicao.Distance(proximaPosicaoBola) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                AjustaPontoControleCampo(ref pontoControle);

                Vector2D[] pontos = CurvaBezier(robo.posicao, pontoControle, proximaPosicaoBola);
                proximoPonto = pontos[5];

                if (BolaDentroArea(ConfiguracoesE1.ATACANTE_COEFICIENTE_PREVISAO_BOLA))
                {
                    if (lado == ControleJogo.Lado.Esquerdo)
                        proximoPonto = new Vector2D(campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X, robo.posicao.y);
                    else
                        proximoPonto = new Vector2D(campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X, robo.posicao.y);
                }

                //desenhar curva de belzier
                if (ConfiguracoesE1.DESENHAR_PONTOS)
                    saida.DesenharCurvaDelzier(pontos, pontoControle);
            }
            else if (distBola > ConfiguracoesE1.ATACANTE_TOLERANCIA /*|| Math.Abs(distReta) >= 30*/)
            {
                proximoPonto = proximaPosicaoBola.Clone();
            }
            else //if (AtrasDaBola(iAtacante, lado, bola.posicao.x, campo.limites))// if (Math.Abs(distReta) < 30)
            {
                proximoPonto.x = golAdversario.x;
                proximoPonto.y = golAdversario.y;
            }

            if (ConfiguracoesE1.DESENHAR_PONTOS)
                saida.DesenharPontosAtacante(robo.posicao, bola.posicao, proximoPonto);


            if (lado == ControleJogo.Lado.Direito)
            {
                Limites areaGoleiro = campo.area_goleiro_direita;
                if (proximaPosicaoBola.x > areaGoleiro.ladoEsquerdo)// && proximaPosicaoBola.Sub(bola.posicao).x>0)
                {
                    if (proximaPosicaoBola.y > areaGoleiro.ladoSuperior)
                    {
                        proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        if (robo.posicao.y > areaGoleiro.ladoSuperior)
                        {
                            proximoPonto.x = campo.limites.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                            proximoPonto.y = areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                            if (robo.posicao.Distance(proximoPonto) < ConfiguracoesE1.ATACANTE_TOLERANCIA_EUCLIDIANA)
                            {
                                noPonto = true;
                            }
                        }
                        if (robo.posicao.y < areaGoleiro.ladoInferior)
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        }
                    }
                    else if (proximaPosicaoBola.y < areaGoleiro.ladoInferior)
                    {
                        proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        if (robo.posicao.y < areaGoleiro.ladoInferior)
                        {
                            proximoPonto.x = campo.limites.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                            proximoPonto.y = areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                            if (robo.posicao.Distance(proximoPonto) < ConfiguracoesE1.ATACANTE_TOLERANCIA_EUCLIDIANA)
                            {
                                noPonto = true;
                            }
                        }
                        if (robo.posicao.y > areaGoleiro.ladoSuperior)
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        }
                    }
                    else
                    {
                        if (robo.posicao.y > campo.limites.centro.y)
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            if (robo.posicao.y > areaGoleiro.ladoSuperior)
                            {
                                proximoPonto.x = campo.limites.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                                proximoPonto.y = areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                                if (robo.posicao.Distance(proximoPonto) < ConfiguracoesE1.ATACANTE_TOLERANCIA_EUCLIDIANA)
                                {
                                    noPonto = true;
                                }
                            }
                            if (robo.posicao.y < areaGoleiro.ladoInferior)
                            {
                                proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            }
                        }
                        else
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            if (robo.posicao.y < areaGoleiro.ladoInferior)
                            {
                                proximoPonto.x = campo.limites.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                                proximoPonto.y = areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                                if (robo.posicao.Distance(proximoPonto) < ConfiguracoesE1.ATACANTE_TOLERANCIA_EUCLIDIANA)
                                {
                                    noPonto = true;
                                }
                            }
                            if (robo.posicao.y > areaGoleiro.ladoSuperior)
                            {
                                proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            }
                        }
                    }
                }
                else
                {
                    if (robo.posicao.x > areaGoleiro.ladoEsquerdo)
                    {
                        if (robo.posicao.y > areaGoleiro.centro.y)
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        else
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoEsquerdo - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                    }
                }
            }
            else
            {
                //jogo na lateral
                Limites areaGoleiro = campo.area_goleiro_esquerda;
                if (proximaPosicaoBola.x < areaGoleiro.ladoDireito)
                {
                    if (proximaPosicaoBola.y > areaGoleiro.ladoSuperior)
                    {
                        proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        if (robo.posicao.y > areaGoleiro.ladoSuperior)
                        {
                            proximoPonto.x = campo.limites.ladoEsquerdo + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                            proximoPonto.y = areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                        }

                    }
                    else if (proximaPosicaoBola.y < areaGoleiro.ladoInferior)
                    {
                        proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        if (robo.posicao.y < areaGoleiro.ladoInferior)
                        {
                            proximoPonto.x = campo.limites.ladoEsquerdo + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                            proximoPonto.y = areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                        }
                    }
                    else
                    {
                        if (robo.posicao.y > campo.limites.centro.y)
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            if (robo.posicao.y > areaGoleiro.ladoSuperior)
                            {
                                proximoPonto.x = campo.limites.ladoEsquerdo + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                                proximoPonto.y = areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                            }
                        }
                        else
                        {
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                            if (robo.posicao.y < areaGoleiro.ladoInferior)
                            {
                                proximoPonto.x = campo.limites.ladoEsquerdo + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2;
                                proximoPonto.y = areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                            }
                        }
                    }
                }
                else
                {
                    if (robo.posicao.x < areaGoleiro.ladoDireito)
                    {
                        if (robo.posicao.y < areaGoleiro.centro.y)
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoInferior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                        else
                            proximoPonto = Vector2D.Create(areaGoleiro.ladoDireito - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS, areaGoleiro.ladoSuperior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS);
                    }
                }
            }




            if (DentroArea(proximoPonto))
            {
                double distSup, distInf, distX;

                if (lado == ControleJogo.Lado.Direito)
                {
                    distSup = campo.area_goleiro_direita.ladoSuperior - proximoPonto.y;
                    distInf = proximoPonto.y - campo.area_goleiro_direita.ladoInferior;
                    distX = proximoPonto.x - campo.area_goleiro_direita.ladoEsquerdo;

                    if (distSup < distInf && distSup < distX)
                        proximoPonto.y = campo.area_goleiro_direita.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                    else if (distInf < distSup && distInf < distX)
                        proximoPonto.y = campo.area_goleiro_direita.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                    else
                        proximoPonto.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.TOLERANCIA_CAMPO;
                }
                else
                {
                    distSup = campo.area_goleiro_esquerda.ladoSuperior - proximoPonto.y;
                    distInf = proximoPonto.y - campo.area_goleiro_esquerda.ladoInferior;
                    distX = campo.area_goleiro_esquerda.ladoDireito - proximoPonto.x;

                    if (distSup < distInf && distSup < distX)
                        proximoPonto.y = campo.area_goleiro_esquerda.ladoSuperior + ConfiguracoesE1.TOLERANCIA_CAMPO;

                    else if (distInf < distSup && distInf < distX)
                        proximoPonto.y = campo.area_goleiro_esquerda.ladoInferior - ConfiguracoesE1.TOLERANCIA_CAMPO;

                    else
                        proximoPonto.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.TOLERANCIA_CAMPO;
                }
            }

            //Verifica se o robo está dentro do gol
            if (robo.posicao.x > campo.area_goleiro_direita.ladoDireito || robo.posicao.x < campo.area_goleiro_esquerda.ladoEsquerdo)
            {
                if (proximoPonto.y > campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    proximoPonto.y = campo.gol_direito.ladoSuperior - ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;
                else if (proximoPonto.y < campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS)
                    proximoPonto.y = campo.gol_direito.ladoInferior + ConfiguracoesE1.DISTANCIA_ENTRE_RODAS;


                if (lado == ControleJogo.Lado.Direito)
                    proximoPonto.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
                else
                    proximoPonto.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
            }

            SaturaForaCampo(ref proximoPonto, ConfiguracoesE1.TOLERANCIA_CAMPO);


            if (/*robo.posicao.x <= campo.limites.ladoEsquerdo + ConfiguracoesE1.TOLERANCIA_CAMPO ||
                                robo.posicao.x >= campo.limites.ladoDireito - ConfiguracoesE1.TOLERANCIA_CAMPO ||*/
                                robo.posicao.y >= campo.limites.ladoSuperior - ConfiguracoesE1.TOLERANCIA_CAMPO ||
                                robo.posicao.y <= campo.limites.ladoInferior + ConfiguracoesE1.TOLERANCIA_CAMPO)
            {
                if (iAtacante == 1)
                {
                    if (noPonto)
                        robo.Parar();
                    else
                        robo.PosicionaRotacionaPID_Henrique(iAtacante, proximoPonto, 500,
                                ConfiguracoesE1.ATACANTE_1_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_1_T_FATOR_VELOCIDADE);

                }
                else
                {
                    if (noPonto)
                        robo.Parar();
                    else
                        robo.PosicionaRotacionaPID_Henrique(iAtacante, proximoPonto, 500,
                                ConfiguracoesE1.ATACANTE_2_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_2_T_FATOR_VELOCIDADE);

                } //if(iAtacante == 2) {


            }
            else
            {
                if (iAtacante == 1)
                {
                    if (noPonto)
                        robo.Parar();
                    else
                        robo.PosicionaPID_Henrique(iAtacante, proximoPonto, 500,
                                ConfiguracoesE1.ATACANTE_1_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_1_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_1_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_1_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_1_T_FATOR_VELOCIDADE);

                }
                else
                {
                    if (noPonto)
                        robo.Parar();
                    else
                        robo.PosicionaPID_Henrique(iAtacante, proximoPonto, 500,
                                ConfiguracoesE1.ATACANTE_2_F_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_F_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_F_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_LINEAR_KP, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KI, ConfiguracoesE1.ATACANTE_2_T_LINEAR_KD,
                                ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_T_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KP, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KI, ConfiguracoesE1.ATACANTE_2_ROTACIONA_ANGULAR_KD,
                                ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX, ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_2_F_FATOR_VELOCIDADE, ConfiguracoesE1.ATACANTE_2_T_FATOR_VELOCIDADE);

                } //if(iAtacante == 2) {

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

        bool troca = false;

        public void DefinirPapeis()
        {
            int numRobos = ambiente.Time.Robo.Length;
            BolaE1 bola = ambiente.Bola;
            TimeE1 time = ambiente.Time;
            Limites limites = ambiente.Campo.limites;
            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.ATACANTE_COEFICIENTE_PREVISAO_BOLA);
            ControleJogo.Lado lado = ambiente.Time.lado;
            //iAtacante = 1;
            //iZagueiro = 1;
            //iGoleiro = 0;
            //GoleiroMinhoca(ref time.Robo[iGoleiro]);
            // ZagueiroMinhoca(ref time.Robo[iZagueiro]);
            //Atacante(ref time.Robo[iAtacante]);

            if (numRobos == 3)
            {
                iGoleiro = 0;
                GoleiroMinhoca(ref time.Robo[iGoleiro]);

                bool robo1Atras = AtrasDaBola(1, lado, proximaPosicaoBola.x, limites);
                bool robo2Atras = AtrasDaBola(2, lado, proximaPosicaoBola.x, limites);

                if (!troca)
                {
                    if (robo1Atras && robo2Atras)
                    {

                        if (time.Robo[1].posicao.Distance(proximaPosicaoBola) <= time.Robo[2].posicao.Distance(proximaPosicaoBola))
                        {
                            iAtacante = 1;
                            iZagueiro = 2;
                        }
                        else
                        {
                            iAtacante = 2;
                            iZagueiro = 1;
                        }
                        AtacanteUnivector2(ref time.Robo[iAtacante]);
                        Zagueiro(ref time.Robo[iZagueiro]);
                    }
                    else if ((!robo1Atras && !robo2Atras))
                    {
                        iAtacante = 2;
                        iZagueiro = 1;

                        Zagueiro(ref time.Robo[iAtacante]);
                        Zagueiro(ref time.Robo[iZagueiro]);
                    }
                    else if (robo1Atras)
                    {
                        iAtacante = 1;
                        iZagueiro = 2;
                        AtacanteUnivector2(ref time.Robo[iAtacante]);
                        Zagueiro(ref time.Robo[iZagueiro]);
                        troca = true;
                    }
                    else
                    { //2 atras da bola
                        iAtacante = 2;
                        iZagueiro = 1;
                        AtacanteUnivector2(ref time.Robo[iAtacante]);
                        Zagueiro(ref time.Robo[iZagueiro]);
                        troca = true;
                    }
                }
                else if ((lado == Lado.Direito && time.Robo[iZagueiro].posicao.x >= bola.posicao.x + ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE)
                    || (lado == Lado.Esquerdo && time.Robo[iZagueiro].posicao.x <= bola.posicao.x - ConfiguracoesE1.ATACANTE_TOLERANCIA_DEFINE))
                {
                    troca = false;
                }
                else
                {
                    AtacanteUnivector2(ref time.Robo[iAtacante]);
                    Zagueiro(ref time.Robo[iZagueiro]);
                }

            }

            else if (numRobos == 2)
            {
                Zagueiro(ref time.Robo[0]);
                Atacante(ref time.Robo[1]);
                iAtacante = 0;
                iZagueiro = 1;
            }
            else if (numRobos == 1)
            {
                //Atacante(ref time.Robo[0]);
                //Goleiro(ref time.Robo[0]);
                GoleiroMinhoca(ref time.Robo[0]);
                iGoleiro = 0;
                //Zagueiro(ref time.Robo[0]);
            }

        }

        public void ExecutaEstrategia(ref Ambiente ambiente_jogo)
        {
            ambiente.AtualizarPosicoes(ref ambiente_jogo);

            TimeE1 time = ambiente.Time;
            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;

            //desenhar segmento de previsao de bola
            //if (ConfiguracoesE1.DESENHAR_PONTOS)
            //   saida.DesenharPrevisaoBola(ref bola, ConfiguracoesE1.ATACANTE_COEFICIENTE_PREVISAO_BOLA);

            //Atacante(ref time.Robo[1]);
            //GoleiroMinhoca(ref time.Robo[0]);
            //iZagueiro = 1;
            //Zagueiro(ref time.Robo[1]);
            //PararRobos();
            //Zagueiro(ref time.Robo[0]);


            DefinirPapeis(); //para visualizar belzier com o jogo pausado            
            if (ConfiguracoesE1.EXIBIR_DADOS)
            {
                saida.ExibirDadosRobos(ref ambiente, ref time);
                saida.ExibirDadosBola(ref ambiente);
            }
            switch (ambiente.EstadoJogo)
            {
                case ControleJogo.Estado.Parado:
                    //troca = false;
                    PararRobos();
                    break;
                case ControleJogo.Estado.Pausado:
                    // troca = false;
                    PararRobos();
                    break;
                case ControleJogo.Estado.Executando:
                    DefinirPapeis();
                    break;
                case ControleJogo.Estado.Posicionando:
                    PararRobos();
                    if (controle.RoboPosicionando != -1 &&
                        controle.RoboPosicionando < time.Robo.Length &&
                        time.Robo[controle.RoboPosicionando].posicao.Distance(controle.PosicionamentoRobo) > ConfiguracoesE1.ATACANTE_TOLERANCIA_POSICAO_X)

                        time.Robo[controle.RoboPosicionando].Posiciona(controle.PosicionamentoRobo,
                            ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX,
                            ConfiguracoesE2.ATACANTE_VEL_ANGULAR_MAX);
                    break;
            }

            ambiente.DefinirVelocidades(ref ambiente_jogo);

            if (ConfiguracoesE1.GRAVAR_ARQUIVO)
                saida.SalvarDadosArquivo(ref ambiente);
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

        //const double Q = 655;

        public Vector2D[] CurvaBezierPotencial(Vector2D origem, Vector2D controle, Vector2D destino, Vector2D bola, Vector2D atacante)
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
                dist = bola.Distance(proximo);
                Fr = Math.Abs(ConfiguracoesE1.ZAGUEIRO_Q / (dist));
                angulo = Math.Atan2(proximo.y - bola.y, proximo.x - bola.x);

                delta.x = 2 * Fr * Math.Cos(angulo);
                delta.y = 2 * Fr * Math.Sin(angulo);

                dist = atacante.Distance(proximo);
                Fr = Math.Abs(ConfiguracoesE1.ZAGUEIRO_Q / (dist));
                angulo = Math.Atan2(proximo.y - atacante.y, proximo.x - atacante.x);

                delta.x += 2 * Fr * Math.Cos(angulo);
                delta.y += 2 * Fr * Math.Sin(angulo);

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
            deltaX = dir * robo_.y + 200 * robo_.x * circ;
            deltaY = -dir * robo_.x + 200 * robo_.y * circ;

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
                for (int j = 0; j <= 400; j = j + 20)
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

        public bool PotenciaDoPonto(Vector2D a, Vector2D b, double raio)
        {
            double distance = a.Distance(b);
            return ((distance * distance) - (raio * raio) <= 0);

        }

        public Vector2D[] EscreveCandidatos(ref RoboE1 robo)
        {
            TimeE1 time = ambiente.Time;
            BolaE1 bola = ambiente.Bola;
            TimeE1 adversario = ambiente.Adversario;
            int size = time.Robo.Length + adversario.Robo.Length;
            //Adicionar todos os elementos para a lista de desvio jogadores(desconsiderando o jogador atual), adversarios e bola
            Vector2D[] candidatos = new Vector2D[size];

            for (int i = 0; i < size; ++i)
            {
                candidatos[i] = Vector2D.Zero();
            }
            int cont = 0;
            //considerado -> a considerar
            //a bola tem de verificar se vai considerar ou não
            foreach (var o in time.Robo)
            {
                if (o.posicao.x != robo.posicao.x || o.posicao.y != robo.posicao.y)
                {
                    candidatos[cont] = o.posicao;
                    cont++;
                }
            }

            foreach (var o in adversario.Robo)
            {
                candidatos[cont] = o.posicao;
                cont++;
            }
            candidatos[cont] = bola.posicao;
            return candidatos;
        }
        public Vector2D DesviaZagueiro(ref Vector2D proximoPonto, ref RoboE1 robo, ref BolaE1 bola)
        {
            ControleJogo.Lado lado = ambiente.Time.lado;
            Vector2D pontoDesviado = proximoPonto.Clone();
            double Qx, Qy, distanciaMax = 0, dist = 0;
            int dir;
            Vector2D[] candidatos = EscreveCandidatos(ref robo);
            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);
            Vector2D somatorio = Vector2D.Zero();
            List<Vector2D> aux = new List<Vector2D>();
            bool desviando = false;

            //Calculo o ponto para uma posição mais proxima do zagueiro
            if (robo.posicao.Distance(proximoPonto) > ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO)
                pontoDesviado = Redimensiona(proximoPonto.Sub(robo.posicao), ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO).Add(robo.posicao);

            //Verifica para cada um dos objetos do vetor se ele deve ser evitado (com exceção da bola)
            if (!PotenciaDoPonto(pontoDesviado, proximoPonto, ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_MINIMA_DESVIO))
                for (int i = 0; i < candidatos.Length - 1; ++i)
                {
                    if (PotenciaDoPonto(pontoDesviado, candidatos[i], ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO))
                    {
                        somatorio = somatorio.Add(candidatos[i]);
                        aux.Add(candidatos[i]);
                        saida.DesenharCirculoAtacante(candidatos[i], (float)ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO);
                        desviando = true;
                    }
                }
            //Verifica e desvia do somatório de objetos a se desviar
            if (desviando)
            {
                somatorio.x = somatorio.x / aux.Count;
                somatorio.y = somatorio.y / aux.Count;
                //somatorio representa o centro entre os obstaculos (media)

                foreach (var o in aux)
                {
                    dist = somatorio.Distance(o);
                    if (dist > distanciaMax)
                        distanciaMax = dist;
                }

                saida.DesenharCirculoZagueiro(somatorio, (float)(ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax));
                saida.DesenharReta(robo.posicao, bola.posicao);

                double d = CalculaDistanciaReta(proximaPosicaoBola, robo.posicao, somatorio);

                if (robo.posicao.x < bola.posicao.x)
                    dir = -(int)(d / Math.Abs(d));
                else
                    dir = (int)(d / Math.Abs(d));

                Vector2D delta = robo.posicao.Sub(somatorio);
                double ang = delta.Angle();

                //A direção é invertida quando o lado é contrário
                DefineDirZagueiro(ref dir, somatorio, distanciaMax, proximoPonto, robo.posicao);

                Qx = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Cos(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * dir)) + somatorio.x;
                Qy = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Sin(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * dir)) + somatorio.y;

                pontoDesviado = Vector2D.Create(Qx, Qy);

                pontoDesviado = Vector2D.Create(Qx, Qy);
            }

            //Refaz a verificação conferindo a posicao da bola, no caso desviar da bola é a prioridade máxima para o zagueiro
            if (PotenciaDoPonto(pontoDesviado, proximaPosicaoBola, ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO))
            {
                desviando = true;
                if (lado == ControleJogo.Lado.Direito)
                {
                    if (bola.posicao.x > robo.posicao.x)
                    {
                        if (PotenciaDoPonto(robo.posicao, proximaPosicaoBola, ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO))
                        {
                            saida.DesenharCirculoZagueiro(bola.posicao, (float)ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO);
                            if (robo.posicao.y > bola.posicao.y)
                                dir = 1;
                            else
                                dir = -1;
                            //InverteDirecao(ref dir, somatorio, distanciaMax, proximoPonto, robo.posicao);
                            Vector2D delta = robo.posicao.Sub(bola.posicao);
                            double ang = delta.Angle();

                            Qx = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Cos(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * (-dir))) + bola.posicao.x;
                            Qy = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Sin(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * (-dir))) + bola.posicao.y;

                            pontoDesviado = Vector2D.Create(Qx, Qy);
                        }
                    }
                }
                else if (bola.posicao.x < robo.posicao.x)
                {
                    if (PotenciaDoPonto(robo.posicao, proximaPosicaoBola, ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO))
                    {
                        saida.DesenharCirculoZagueiro(bola.posicao, (float)ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO);
                        //d = CalculaDistanciaReta(ambiente.Campo.gol_esquerdo.centro, pontoDesviado, bola.posicao);
                        //dir = (int)(d / Math.Abs(d));
                        if (robo.posicao.y > bola.posicao.y)
                            dir = -1;
                        else
                            dir = 1;
                        Vector2D delta = robo.posicao.Sub(bola.posicao);
                        double ang = delta.Angle();

                        Qx = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Cos(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * (-dir))) + bola.posicao.x;
                        Qy = ((ConfiguracoesE1.ZAGUEIRO_RAIO_DESVIO + distanciaMax) * Math.Sin(ang + ConfiguracoesE1.ZAGUEIRO_ANGULO_DESVIO * (-dir))) + bola.posicao.y;

                        pontoDesviado = Vector2D.Create(Qx, Qy);
                    }
                }
            }
            if (desviando)
                return pontoDesviado;
            else
                return proximoPonto;
        }

        public Vector2D DesviaAtacante(ref Vector2D proximoPonto, ref Vector2D pontoControle, ref RoboE1 robo, ref BolaE1 bola)
        {
            ControleJogo.Lado lado = ambiente.Time.lado;
            Campo campo = ambiente.Campo;

            Vector2D[] candidatos = EscreveCandidatos(ref robo);

            //Vetor candidatos representa todos os objetos que não sejam o proprio robo
            //O ultimo valor deste vetor SEMPRE será a bola, porém o vetor pode ter valor variado de 1 à 6 posições
            Vector2D pontoDesviado = proximoPonto.Clone();
            double Qx, Qy, dist, distanciaMax = 0;
            //double d;
            int dir;
            //A bola tem de ser tratada diferente
            Vector2D somatorio = Vector2D.Zero();
            List<Vector2D> aux = new List<Vector2D>();
            bool desviar = false;
            bool desviando = false;

            if (proximoPonto.Distance(robo.posicao) > ConfiguracoesE1.ATACANTE_RAIO_DESVIO)
                pontoDesviado = Redimensiona(proximoPonto.Sub(robo.posicao), ConfiguracoesE1.ATACANTE_RAIO_DESVIO).Add(robo.posicao);
            if (lado == ControleJogo.Lado.Direito)
            {
                if (pontoDesviado.x > campo.limites.centro.x)
                    desviar = true;
            }
            else
            {
                if (pontoDesviado.x < campo.limites.centro.x)
                    desviar = true;
            }

            if (desviar && !PotenciaDoPonto(proximoPonto, bola.posicao, ConfiguracoesE1.ATACANTE_TOLERANCIA_MINIMA_DESVIO))
                for (int i = 0; i < candidatos.Length - 1; ++i)
                {
                    if (PotenciaDoPonto(pontoDesviado, candidatos[i], ConfiguracoesE1.ATACANTE_RAIO_DESVIO))
                    {
                        somatorio = somatorio.Add(candidatos[i]);
                        aux.Add(candidatos[i]);
                        saida.DesenharCirculoAtacante(candidatos[i], (float)ConfiguracoesE1.ATACANTE_RAIO_DESVIO);
                        desviando = true;
                    }
                }
            if (desviando)
            {
                somatorio.x = somatorio.x / aux.Count;
                somatorio.y = somatorio.y / aux.Count;
                //somatorio representa o centro entre os obstaculos (media)

                foreach (var o in aux)
                {
                    dist = somatorio.Distance(o);
                    if (dist > distanciaMax)
                        distanciaMax = dist;
                }

                saida.DesenharCirculoAtacante(somatorio, (float)(ConfiguracoesE1.ATACANTE_RAIO_DESVIO + distanciaMax));
                saida.DesenharReta(robo.posicao, pontoControle);

                double d = 0;
                if (robo.posicao.x < pontoControle.x)
                {
                    d = CalculaDistanciaReta(robo.posicao, pontoControle, somatorio);
                    dir = -(int)(d / Math.Abs(d));
                }
                else
                {
                    d = CalculaDistanciaReta(robo.posicao, pontoControle, somatorio);
                    dir = (int)(d / Math.Abs(d));
                }

                DefineDirAtacante(ref dir, somatorio, distanciaMax, proximoPonto, robo.posicao);

                Vector2D delta = robo.posicao.Sub(somatorio);
                double ang = delta.Angle();

                Qx = ((ConfiguracoesE1.ATACANTE_RAIO_DESVIO + distanciaMax) * Math.Cos(ang + ConfiguracoesE1.ATACANTE_ANGULO_DESVIO * dir)) + somatorio.x;
                Qy = ((ConfiguracoesE1.ATACANTE_RAIO_DESVIO + distanciaMax) * Math.Sin(ang + ConfiguracoesE1.ATACANTE_ANGULO_DESVIO * dir)) + somatorio.y;

                pontoDesviado = Vector2D.Create(Qx, Qy);
                return pontoDesviado;
            }
            else
                return proximoPonto;
        }
        public void DefineDirAtacante(ref int dir, Vector2D somatorio, double distanciaMax, Vector2D proximoPonto, Vector2D posicao)
        {
            ControleJogo.Lado lado = ambiente.Time.lado;
            Campo campo = ambiente.Campo;
            if (lado == ControleJogo.Lado.Direito)
            {
                //if(somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS/2)) < campo.limites.ladoSuperior || 
                if (somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoSuperior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y < posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y > posicao.y))
                {
                    dir = -1;
                }
                else if (somatorio.y - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoInferior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y > posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y < posicao.y))
                {
                    dir = 1;
                }
            }
            else
            {
                if (somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoSuperior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y > posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y < posicao.y))
                {
                    dir = 1;
                }
                else if (somatorio.y - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoInferior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y < posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y > posicao.y))
                {
                    dir = -1;
                }
            }
        }
        public void DefineDirZagueiro(ref int dir, Vector2D somatorio, double distanciaMax, Vector2D proximoPonto, Vector2D posicao)
        {
            ControleJogo.Lado lado = ambiente.Time.lado;
            Campo campo = ambiente.Campo;
            if (proximoPonto.x < posicao.x)
            {
                //if(somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS/2)) < campo.limites.ladoSuperior || 
                if (somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoSuperior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y < posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y > posicao.y))
                {
                    dir = -1;
                }
                else if (somatorio.y - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoInferior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y > posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y < posicao.y))
                {
                    dir = 1;
                }
            }
            else
            {
                if (somatorio.y + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoSuperior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y > posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y < posicao.y))
                {
                    dir = 1;
                }
                else if (somatorio.y - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoInferior ||
                    (somatorio.x - (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) < campo.limites.ladoEsquerdo && proximoPonto.y < posicao.y) ||
                    (somatorio.x + (distanciaMax + ConfiguracoesE1.ATACANTE_RAIO_DESVIO + (ConfiguracoesE1.DISTANCIA_ENTRE_RODAS / 2)) > campo.limites.ladoDireito && proximoPonto.y > posicao.y))
                {
                    dir = -1;
                }
            }
        }

        #region Campos Potenciais da estratégia Antiga

        //public const double DISTANCIA_BOLA_PARADA = 18;

        private void Evitar(RoboE1 robo, double angulo)
        {
            double teta = angulo - robo.rotacao;

            while (teta > Math.PI) teta -= 2 * Math.PI;
            while (teta < -Math.PI) teta += 2 * Math.PI;

            double velLinear = ConfiguracoesE1.ATACANTE_VEL_LINEAR_MAX * Math.Cos(teta);
            double velAngular = ConfiguracoesE1.ATACANTE_VEL_ANGULAR_MAX * Math.Sin(teta);

            if (velAngular > ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX)
            {
                velAngular = ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX;
            }
            else
            {
                if (velAngular < -ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX)
                {
                    velAngular = -ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX;
                }

            }
            robo.velocidadeRodaDireita = velLinear + velAngular;
            robo.velocidadeRodaEsquerda = velLinear - velAngular;
        }

        public void CamposPotenciaisAntigo(RoboE1 robo, Vector2D posicao, bool ativo)
        {
            // Módulo da força atrativa
            double dx = 0, dy = 0;
            Forca F = new Forca(); // Força Repulsiva Auxiliar Aliados
            Forca Fa = new Forca(); // Força  Atrativa
            Forca Fr = new Forca(); // Força Repulsiva
            Forca Faux = new Forca(); // Força Repulsiva Auxiliar Aliados
            Forca Fro = new Forca(); // Força Repulsiva Oponentes

            Vector2D proximaPosicaoBola = ambiente.Bola.ProximaPosicao(ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);

            //if (!ativo)
            //{
            //    //Força  Atrativa
            //    dx = proximaPosicaoBola.x - robo.posicao.x;
            //    dy = proximaPosicaoBola.y - robo.posicao.y;
            //    Fa.SetXYMod(dx, dy, ConfiguracoesE1.K_MODULO_CAMPO_POTENCIAL); // módulo constante
            //}
            //else
            //{
            //    //Força  Atrativa que a posição tem sobre o robo
            //    dx = posicao.x - robo.posicao.x;
            //    dy = posicao.y - robo.posicao.y;
            //    //Fa.SetXYMod(dx, dy, ConfiguracoesE1.K_MODULO_CAMPO_POTENCIAL); // módulo constante
            //    Fa.SetAtracao(dx, dy, ConfiguracoesE1.K_MODULO_CAMPO_POTENCIAL); // módulo constante
            //}


            ////Força de Repulsiva
            //for (int i = 0; i < Configuracoes.QTD_TIME; i++)
            //{
            //    // Time
            //    //if (robo != ambiente.Time.Robo[i])
            //    //{
            //    //    dx = ambiente.Time.Robo[i].posicao.x - robo.posicao.x;
            //    //    dy = ambiente.Time.Robo[i].posicao.y - robo.posicao.y;
            //    //    //F.SetXY(-dx, -dy, ConfiguracoesE1.RAIO_POTENCIAL);
            //    //    F.SetRepulsao(dx, dy, ConfiguracoesE1.ZAGUEIRO_Q, ConfiguracoesE1.RAIO_POTENCIAL);
            //    //    saida.DesenharCirculo(ambiente.Time.Robo[i].posicao, (float)F.Modulo);
            //    //    Fr = Fr.Soma(F);    // Força repulsiva robos do time      
            //    //}
            //}

            //for (int i = 0; i < Configuracoes.QTD_ADV; i++)
            //{
            //    // Adversário
            //    //  dx = env.opponent[i].pos.x - robot.pos.x;
            //    //  dy = env.opponent[i].pos.y - robot.pos.y;
            //    //   Faux.setXY(-dx, -dy, Raio);
            //    //    Fro = Fro.Soma(ref Faux);  // Forca repulsiva adversários
            //}


            if (ativo)
            {
                dx = proximaPosicaoBola.x - robo.posicao.x;
                dy = proximaPosicaoBola.y - robo.posicao.y;
                //F.SetXY(-dx, -dy, ConfiguracoesE1.RAIO_POTENCIAL); // módulo constante
                F.SetRepulsao(dx, dy, ConfiguracoesE1.ZAGUEIRO_Q, ConfiguracoesE1.RAIO_POTENCIAL);
                saida.DesenharCirculo(ambiente.Bola.posicao, (float)F.Modulo);
                Fr = Fr.Soma(F);
            }

            //Fr = Fr.Multiplica(ConfiguracoesE1.Q_ZAGUEIRO * 100);           // Repulsão dos robôs time
            Fr = Fr.Soma(Fa);

            //saida.DesenharDestinoGoleiro(Fr.ToVector2D());
            //saida.DesenharReta(Vector2D().Zero(), Fr.ToVector2D());
            Evitar(robo, Fr.Angulo);
        }

        public void ZagueiroComCamposPotenciais(RoboE1 robo)
        {

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;

            Vector2D proximaPosicaoBola = bola.ProximaPosicao(ConfiguracoesE1.ZAGUEIRO_COEFICIENTE_PREVISAO_BOLA);

            Vector2D destino = Vector2D.Zero();
            Vector2D posGol = Vector2D.Zero();

            bool usarCamposPotenciais = false;
            if (lado == ControleJogo.Lado.Direito)
            {
                posGol.x = campo.gol_direito.ladoDireito + ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_direito.centro.y;
                destino.x = campo.area_goleiro_direita.ladoEsquerdo - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
                usarCamposPotenciais = (robo.posicao.x < proximaPosicaoBola.x);
            }
            else
            {
                posGol.x = campo.gol_esquerdo.ladoEsquerdo - ConfiguracoesE1.DESLOCAMENTO_GOL;
                posGol.y = campo.gol_esquerdo.centro.y;
                destino.x = campo.area_goleiro_esquerda.ladoDireito + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_X;
                usarCamposPotenciais = (robo.posicao.x > proximaPosicaoBola.x);
            }

            if (posGol.x != proximaPosicaoBola.x)
            {
                double dx = (proximaPosicaoBola.x - posGol.x);
                double a = (dx == 0) ? 0 : (proximaPosicaoBola.y - posGol.y) / dx;
                double b = posGol.y - a * posGol.x;
                destino.y = a * destino.x + b;
            }
            else
            {
                destino.y = posGol.y;
            }

            if (destino.y > campo.limites.ladoSuperior - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoSuperior - ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y;
            if (destino.y < campo.limites.ladoInferior + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y) destino.y = campo.limites.ladoInferior + ConfiguracoesE1.ZAGUEIRO_DESLOCAMENTO_Y;

            if (robo.posicao.Distance(destino) < ConfiguracoesE1.ZAGUEIRO_TOLERANCIA_PONTO)
                robo.Parar();
            else
            {
                if (usarCamposPotenciais)
                {
                    CamposPotenciaisAntigo(robo, destino, true);
                }
                else
                {
                    robo.PosicionaAntiga(destino, ConfiguracoesE1.ZAGUEIRO_VEL_LINEAR_MAX, ConfiguracoesE1.ZAGUEIRO_VEL_ANGULAR_MAX, ConfiguracoesE1.ATACANTE_TOLERANCIA_ANGULO);
                }
            }

            //desenhar ponto desejado para o zagueiro
            if (false) //(ConfiguracoesE1.DESENHAR_PONTOS)
            {
                // saida.DesenharDestinoZagueiro(posGol);
                saida.DesenharDestinoZagueiro(destino);
                //saida.DesenharDestinoGoleiro(proxPosi);
            }

        }

        #endregion
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

        public void DesenhaCampo(Vector2D robo, Vector2D bola)
        {

            Campo campo = ambiente.Campo;
            Lado lado = ambiente.Time.lado;


            Vector2D ponto1 = campo.limites.centro.Clone();
            Vector2D ponto2;

            ponto1.x -= 350;
            ponto1.y -= 300;

            for (int k = 0; k <= 700; k = k + 20)
            {
                for (int j = 0; j <= 600; j = j + 20)
                {
                    ponto2 = ponto1.Clone();
                    ponto2.x += k;
                    ponto2.y += j;

                    Vector2D prox = AtrativaSimples(ponto2, bola, 50, 0.5);
                    prox = ponto2.Add((prox.Unitary().Mult(15)));


                    saida.DesenharReta(ponto2,prox);

                }
            }



        }


        public void DesenhaTrajetoria(Vector2D robo, Vector2D bola, int cont)
        {

            Vector2D p3 = bola.Clone();

            int passo = 5;

            if(robo.x - bola.x > 100)
                p3 = AtrativaSimples(robo, bola, 100, 2).Unitary();
            else
                p3 = UnivectorField(robo, bola, 100, (80 + passo), passo).Unitary();


            if (robo.Add(p3.Mult(passo)).Distance(bola) > 2 * passo && cont != 0)
            {
                cont--;
                saida.DesenharVetores(robo, robo.Add(p3.Mult(passo)));
                DesenhaTrajetoria(robo.Add(p3.Mult(passo)), bola, cont);
            }
            else
                saida.DesenharVetores(robo, bola);
        }


        #region UnivectorField
        public void AtacanteUnivector2(ref RoboE1 robo)
        {
            Vector2D proximoPonto = Vector2D.Zero(), pontoControle, golAdversario = Vector2D.Zero();

            //robo.dadosPID.velLinearMax = robo.medicaoPID.velLinearMax = ConfiguracoesE1.VEL_LINEAR_MAX_ATACANTE;
            //robo.dadosPID.velAngularMaxima = robo.medicaoPID.velAngularMaxima = ConfiguracoesE1.VEL_ANGULAR_MAX_ATACANTE;
            //robo.dadosPID.KP = ConfiguracoesE1.KP_ATACANTE;
            //robo.dadosPID.KI = ConfiguracoesE1.KI_ATACANTE;

            BolaE1 bola = ambiente.Bola;
            Campo campo = ambiente.Campo;
            Lado lado = ambiente.Time.lado;

            //robo.papel = RoboE1.Papel.Atacante;

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

            DesenhaCampo(robo.posicao, bola.posicao);
            DesenhaTrajetoria(robo.posicao, bola.posicao, 1000);

            //saida.DesenharReta(robo.posicao, prox);

            // if (!AjustaParede(ref robo))
            {
                double tolerancia = 10;
                if (bola.posicao.Distance(robo.posicao) > tolerancia || //ConfiguracoesE1.TOLERANCIA_ATACANTE ||
                ((lado == ControleJogo.Lado.Direito) ? (robo.posicao.x < bola.posicao.x + ConfiguracoesE1.DIAMETRO_BOLA) :
                    (robo.posicao.x > bola.posicao.x - ConfiguracoesE1.DIAMETRO_BOLA)))
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
                        pontoControle = bola.posicao.Sub(golAdversario.Sub(bola.posicao).Unitary().Mult(robo.posicao.Distance(bola.posicao) * ConfiguracoesE1.COEFICIENTE_PONTO_CONTROLE));

                    AjustaPontoControleCampo(ref pontoControle);

                    Vector2D[] pontos = CurvaBezier(robo.posicao, pontoControle, bola.posicao);
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

        public double PhiH(double theta, double p, double kr, double de, bool cw)
        {
            double pi2 = Math.PI / 2;
            double aux;

            if (p > de)
                aux = pi2 * (2 - ((de + kr) / (p + kr)));
            else
                aux = pi2 * Math.Sqrt(p / de);

            if (!cw) return theta + aux;

            else return theta - aux;
        }

        public Vector2D NH(double theta)
        {
            return new Vector2D(Math.Cos(theta), Math.Sin(theta));
        }

        public double PhiR(Vector2D robo, Tuple<Vector2D, bool>[] obstaculos)
        {
            //Vector2D[] obstaculos = new Vector2D[2] {bola, robo};
            double di, cosi, seni, cos = 0, sen = 0;

            for (int i = 0; i < obstaculos.Length; i++)
            {
                if (obstaculos[i].Item2) // se ele estever marcado como obstaculo
                {
                    di = Math.Sqrt(Math.Pow(robo.x - obstaculos[i].Item1.x, 2) + Math.Pow(robo.y - obstaculos[i].Item1.y, 2));
                    cosi = (robo.x - obstaculos[i].Item1.x) / di;
                    seni = (robo.y - obstaculos[i].Item1.y) / di;
                    cos += cosi / di;
                    sen += seni / di;
                }
            }

            double aux = Math.Sqrt(Math.Pow(cos, 2) + Math.Pow(sen, 2));
            double phi = Math.Atan2(cos / aux, sen / aux);
            //phi = Math.Atan2(sen / aux, cos / aux);

            return phi;
        }
        public double PhiAUF(Vector2D robo, Tuple<Vector2D, bool>[] obstaculos)
        {
            return PhiR(robo, obstaculos);
        }

        public double PhiTUF(double theta, Vector2D ponto, double kr, double de, double passo)
        {
            double yl = ponto.y + de;
            double yr = ponto.y - de;

            double phl = Math.Sqrt(Math.Pow(ponto.x, 2) + Math.Pow(yr, 2));
            double phr = Math.Sqrt(Math.Pow(ponto.x, 2) + Math.Pow(yl, 2));

            if (ponto.y < -de + passo)
                return PhiH(theta, phl, kr, de, false);

            else if (ponto.y > de - passo)
                return PhiH(theta, phr, kr, de, true);

            else //(-de < ponto.y && ponto.y < de)
            {
                double phiccw = PhiH(theta, phl, kr, de, true);
                double phicw = PhiH(theta, phr, kr, de, false);

                Vector2D nhCcw = NH(phiccw);
                Vector2D nhCw = NH(phicw);

                Vector2D tuf = (nhCcw.Mult(Math.Abs(yl)).Add(nhCw.Mult(Math.Abs(yr)))).Mult(1 / (2 * de));

                return Math.Atan2(tuf.y, tuf.x);
            }
        }


        public double PhiComposto(RoboE1 robo, Vector2D bola, double passo)
        {
            //d_min = 3.48 # cm
            //delta = 4.57 # cm

            double dmin = 13.92;
            double delta = 18.30;

            double kr = 100; // atenuacao da curva
            double de = 100; // distancia minima de desvio

            double theta = Math.Atan2(robo.posicao.y - bola.y, robo.posicao.y - bola.y);

            Tuple<Vector2D, bool>[] obstaculos = DefineObstaculos(robo, bola);

            double r = robo.posicao.Distance(ObstaculoMaisProximo(robo, obstaculos)); // r = 0 caso não exista obstaculos

            if (r == 0) return PhiTUF(theta, robo.posicao, kr, de, passo);

            else if (r <= dmin) return PhiAUF(robo.posicao, obstaculos);

            else return PhiAUF(robo.posicao, obstaculos) * G(r - dmin, delta) + PhiTUF(theta, robo.posicao, kr, de, passo) * (1 - G(r - dmin, delta));
        }

        public double PhiComposto(Vector2D robo, Vector2D bola, double passo) // para fins de teste
        {
            //d_min = 3.48 # cm
            //delta = 4.57 # cm

            double dmin = 0;
            double delta = 20;

            double kr = 100; // atenuacao da curva
            double de = 100; // distancia minima de desvio

            double theta = Math.Atan2(robo.y - bola.y, robo.y - bola.y);

            Tuple<Vector2D, bool>[] obstaculos = DefineObstaculos(bola);

            double r = robo.Distance(ObstaculoMaisProximo(robo, obstaculos)); // r = 0 caso não exista obstaculos

            if (r == 0) return PhiTUF(theta, robo, kr, de, passo);

            else if (r <= dmin) return PhiAUF(robo, obstaculos);

            else return PhiAUF(robo, obstaculos) * G(r - dmin, delta) + PhiTUF(theta, robo, kr, de, passo) * (1 - G(r - dmin, delta));
        }

        public double G(double r, double delta)
        {
            return Math.Pow(Math.E, (r * r / (2 * delta * delta)));
        }

        public Vector2D ObstaculoMaisProximo(RoboE1 robo, Tuple<Vector2D, bool>[] obstaculos)
        {
            Vector2D maisProximo = new Vector2D(Double.PositiveInfinity, Double.PositiveInfinity);

            for (int i = 0; i < obstaculos.Length; i++)
                if (obstaculos[i].Item2 && robo.posicao.Distance(obstaculos[i].Item1) < robo.posicao.Distance(maisProximo))
                    maisProximo = obstaculos[i].Item1;
            
            
            if (maisProximo.x == Double.PositiveInfinity && maisProximo.y == Double.PositiveInfinity)
                maisProximo = robo.posicao;

            return maisProximo; // retorna a posicao do proprio robo caso não haja nenhum obstaculo
        }

        public Vector2D ObstaculoMaisProximo(Vector2D robo, Tuple<Vector2D, bool>[] obstaculos) // para fins de teste
        {
            Vector2D maisProximo = new Vector2D(Double.PositiveInfinity, Double.PositiveInfinity);

            for (int i = 0; i < obstaculos.Length; i++)
                if (obstaculos[i].Item2 && robo.Distance(obstaculos[i].Item1) < robo.Distance(maisProximo))
                    if (robo.Distance(obstaculos[i].Item1) > 0)
                        maisProximo = obstaculos[i].Item1;

            if (maisProximo.x == Double.PositiveInfinity && maisProximo.y == Double.PositiveInfinity)
                maisProximo = robo;

            return maisProximo; // retorna a posicao do robo caso não tenha nenhum obstaculo
        }

        public Tuple<Vector2D, bool>[] DefineObstaculos(RoboE1 robo, Vector2D bola)
        {
            //if(autoPosicionando)
            Tuple<Vector2D, bool>[] obstaculos =
            {
                Tuple.Create(ambiente.Adversario.Robo[0].posicao, true),
                Tuple.Create(ambiente.Adversario.Robo[1].posicao, true),
                Tuple.Create(ambiente.Adversario.Robo[2].posicao, true),
                Tuple.Create(bola, false),
                Tuple.Create(ambiente.Time.Robo[0].posicao, true),
                Tuple.Create(ambiente.Time.Robo[1].posicao, true),
                Tuple.Create(ambiente.Time.Robo[2].posicao, true),
            };
            //if (robo.papel == RoboE1.Papel.Atacante)
            //{
            //    double distanciaMinDesvioAtacante = 100;
            //    if (robo.posicao.Distance(bola) > distanciaMinDesvioAtacante)
            //    {
            //        obstaculos[0] = new(ambiente.Adversario.Robo[0].posicao, true);
            //        obstaculos[1] = new(ambiente.Adversario.Robo[1].posicao, true);
            //        obstaculos[2] = new(ambiente.Adversario.Robo[2].posicao, true);
            //    }
            //}
            //else if (robo.papel == RoboE1.Papel.Zagueiro)
            //{
            //    if (ambiente.Time.Robo[0].papel == RoboE1.Papel.Atacante)
            //        obstaculos[4] = new(ambiente.Time.Robo[0].posicao, true);
            //    else if (ambiente.Time.Robo[1].papel == RoboE1.Papel.Atacante)
            //        obstaculos[5] = new(ambiente.Time.Robo[1].posicao, true);
            //    else
            //        obstaculos[6] = new(ambiente.Time.Robo[2].posicao, true);
            //}
            return obstaculos;
        }
        public Tuple<Vector2D, bool>[] DefineObstaculos(Vector2D bola) // para fins de teste
        {
            Tuple<Vector2D, bool>[] obstaculos =
            {
            Tuple.Create(ambiente.Adversario.Robo[0].posicao, true),
            Tuple.Create(ambiente.Adversario.Robo[1].posicao, true),
            Tuple.Create(ambiente.Adversario.Robo[2].posicao, true),
            Tuple.Create(bola, false),
            Tuple.Create(ambiente.Time.Robo[0].posicao, true),
            Tuple.Create(ambiente.Time.Robo[1].posicao, true),
            Tuple.Create(ambiente.Time.Robo[2].posicao, false),
            };
            return obstaculos;
        }
        public Vector2D RotacionaEixo(double angulo, Vector2D ponto)
        {
            return new Vector2D(ponto.x * Math.Cos(angulo) + ponto.y * Math.Sin(angulo), -ponto.x * Math.Sin(angulo) + ponto.y * Math.Cos(angulo));
        }

        public Vector2D UnivectorField(Vector2D robo, Vector2D bola, double kr, double de, double passo)
        {
            Campo campo = ambiente.Campo;
            ControleJogo.Lado lado = ambiente.Time.lado;
            Vector2D gol = lado == ControleJogo.Lado.Direito ? campo.gol_esquerdo.centro : campo.gol_direito.centro;

            double angulo = Math.Atan2(bola.y - gol.y, bola.x - gol.x); //angulo entre a bola e o gol

            Vector2D roboRotacionado = RotacionaEixo(angulo, robo); //coordenadas do robo no eixo rotacionado
            Vector2D bolaRotacionada = RotacionaEixo(angulo, bola); //coordenadas da bola no eixo rotacionado

            Vector2D delta = roboRotacionado.Sub(bolaRotacionada);  // delta x e y entre o robo e a bola (rotacionados)
            double alpha = Math.Atan2(delta.y, delta.x);            // angulo entre o robo e a bola

            double teta = PhiTUF(alpha, delta, kr, de, passo); // angulo entre a possicao do robo e seu proximo passo

            Vector2D univector = new Vector2D(Math.Cos(teta), Math.Sin(teta));

            return RotacionaEixo(-angulo, univector); // rotacionando de volta para as coordenadas originais da entrada
        }

        #endregion

        public Vector2D AtrativaSimples(Vector2D robo, Vector2D objetivo, double FObjetivo, double fReta)
        {
            Campo campo = ambiente.Campo;
            Lado lado = ambiente.Time.lado;

            double dt = robo.Distance(objetivo);
            dt = 1;
            double tam = dt * FObjetivo;
            Vector2D nt = objetivo.Sub(robo).Unitary();

            Vector2D fAtrativaObjetivo = nt.Mult(tam);

            Vector2D retaGol = Vector2D.Zero();
            Vector2D posGol = Vector2D.Zero();

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

            retaGol.x = robo.x;
            double a = (objetivo.y - posGol.y) / (objetivo.x - posGol.x);
            double b = posGol.y - a * posGol.x;
            retaGol.y = a * retaGol.x + b;

            dt = retaGol.Distance(robo);
            tam = dt * fReta;
            nt = retaGol.Sub(robo).Unitary();

            Vector2D fAtrativareta = nt.Mult(tam);

            return fAtrativaObjetivo.Add(fAtrativareta);

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace Classe_Rotas_SARP
{
    class ModeloUberPool
    {
        struct Cliente
        {
            public int Embarque;
            public int Desembarque;
            public int Carga;
            public int[] CargaGenero;
            public double Duracao;
            public double InstanteChegada;
            public double InstanteSaida;
            public int Gerero;
            public int PosicaoNaRota;
            public int EmQueCarro;
            public double TempoDeViagem;
        }
        Cliente usuario;

        struct Frota
        {
            public int Capacidade;
            public int TempoMaximoTrabalho;
            public double[,] Ocupacao;
        }
        Frota frota;

        struct ProblemaUberPool
        {
            public int NumeroDeCarros;
            public int NumeroDeClientes;
            public Cliente[] usuario;
            public Frota[] frota;
            public int TamanhoMaximo;
            public int LarguraHorizonte;
            public double[,] Localizacoes;
            public double[,] Distancias;
            public int TempoTotalInstancia;
            public int Horizonte;
            public int TempoMaximoViagemUsuario;
            public int[,] GrafoFactivel;
            public int[,,] SolucaoExata;
            public int[,] SolucaoExataNaoNula;
            public int NumeroDeGeneros;
        }
        ProblemaUberPool problema;
        struct Solucao
        {
            public int[,] RotaGeral;
            public int[] TamanhoAtualRotas;
            public int[,] CargaPontual;
            public double[] CustoRota;
            public double[,] InstanteAtendimento;
        }
        Solucao rota;

        public void LeituraInstancias(string nomeArquivo, string caminho)
        {
            // No modo do Cordeau
            string charEspaco = Convert.ToString(' ');
            string charNoEspaco = "";
            caminho = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\InstanciasCordeau\";
            caminho = caminho + nomeArquivo + ".txt";
            Stream entrada = File.Open(caminho, FileMode.Open);
            StreamReader leitor = new StreamReader(entrada);
            string linha = leitor.ReadLine();
            string[] aux = linha.Split(' ');
            problema.NumeroDeCarros = Convert.ToInt16(aux[0]);
            problema.NumeroDeClientes = Convert.ToInt16(aux[1]);
            problema.TamanhoMaximo = 2 * problema.NumeroDeClientes + 2;
            problema.frota = new Frota[problema.NumeroDeCarros];
            problema.usuario = new Cliente[problema.TamanhoMaximo];
            rota.TamanhoAtualRotas = new int[problema.NumeroDeCarros];
            problema.Horizonte = Convert.ToInt16(aux[2]);
            for (int i = 0; i < problema.NumeroDeCarros; i++) { problema.frota[i].Capacidade = Convert.ToInt16(aux[3]); }
            problema.TempoMaximoViagemUsuario = Convert.ToInt16(aux[4]);
            problema.Localizacoes = new double[problema.TamanhoMaximo, 2];
            rota.RotaGeral = new int[problema.NumeroDeCarros, problema.TamanhoMaximo + 1];

            double[,] tabelaPrincipal = new double[problema.TamanhoMaximo, 7];

            //ListaEmbarques = new int[QtdCarros, TamanhoMax];
            //ListaDesembarques = new int[QtdCarros, TamanhoMax];
            int contador = 0;
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                contador = 0;
                linha = leitor.ReadLine();
                aux = linha.Split(' ');
                for (int k = 0; k < aux.Length; k++)
                {
                    if (aux[k] != charEspaco && aux[k] != charNoEspaco)
                    {
                        tabelaPrincipal[i, contador] = Convert.ToDouble(aux[k]);
                        contador += 1;
                    }
                }
            }
            entrada.Close();
            leitor.Close();

            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                problema.Localizacoes[i, 0] = tabelaPrincipal[i, 1] / 1000;
                problema.Localizacoes[i, 1] = tabelaPrincipal[i, 2] / 1000;
                problema.usuario[i].Duracao = tabelaPrincipal[i, 3];
                problema.usuario[i].Carga = Convert.ToInt16(tabelaPrincipal[i, 4]);
                problema.usuario[i].InstanteChegada = tabelaPrincipal[i, 5];
                problema.usuario[i].InstanteSaida = tabelaPrincipal[i, 6];
            }
            problema.Distancias = new double[problema.TamanhoMaximo, problema.TamanhoMaximo];
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                for (int j = 0; j < problema.TamanhoMaximo; j++)
                {
                    problema.Distancias[i, j] = Math.Sqrt((problema.Localizacoes[i, 0] - problema.Localizacoes[j, 0]) * (problema.Localizacoes[i, 0] - problema.Localizacoes[j, 0]) +
                        (problema.Localizacoes[i, 1] - problema.Localizacoes[j, 1]) * (problema.Localizacoes[i, 1] - problema.Localizacoes[j, 1]));
                }
            }

            AjustarJanelas();
        } // fim leitor
        public void AjustarJanelas()
        {
            for (int i = 1; i <= problema.NumeroDeClientes; i++)
            {
                if (problema.usuario[i + problema.NumeroDeClientes].InstanteChegada != 0)
                {
                    problema.usuario[i].InstanteChegada = problema.usuario[i + problema.NumeroDeClientes].InstanteChegada - problema.TempoMaximoViagemUsuario - problema.usuario[i + problema.NumeroDeClientes].Duracao;
                    problema.usuario[i].InstanteSaida = problema.usuario[i + problema.NumeroDeClientes].InstanteSaida - problema.Distancias[i, problema.NumeroDeClientes + i] - problema.usuario[i + problema.NumeroDeClientes].Duracao;
                }
            }
            for (int i = problema.NumeroDeClientes + 1; i <= 2 * problema.NumeroDeClientes; i++)
            {
                if (problema.usuario[i].InstanteChegada == 0)
                {
                    problema.usuario[i].InstanteChegada = problema.usuario[i - problema.NumeroDeClientes].InstanteChegada + problema.Distancias[i - problema.NumeroDeClientes, i] + problema.usuario[i].Duracao;
                    problema.usuario[i].InstanteSaida = problema.usuario[i - problema.NumeroDeClientes].InstanteSaida + problema.TempoMaximoViagemUsuario + problema.usuario[i].Duracao;
                }
            }
        }

        public void ModeloDARP()
        {
            string Agora = Convert.ToString(DateTime.Now);
            string[] a = Agora.Split(' ');
            string[] a0 = a[0].Split('/');
            string[] a1 = a[1].Split(':');
            Agora = a0[0] + "_" + a0[1] + "_" + a0[2] + "_" + a1[0] + "_" + a1[1] + "_" + a1[2];

            /// Calcundo algumas constantes que usamos na linearização da carga e janela de tempo
            double[,,] M = new double[problema.NumeroDeCarros, problema.TamanhoMaximo, problema.TamanhoMaximo];
            double[,,] U = new double[problema.NumeroDeCarros, problema.TamanhoMaximo, problema.TamanhoMaximo];

            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo; i++)
                {
                    for (int j = 0; j < problema.TamanhoMaximo; j++)
                    {
                        M[k, i, j] = Math.Max(0, problema.usuario[i].InstanteSaida + problema.usuario[i].Duracao + +problema.Distancias[i, j] - problema.usuario[j].InstanteChegada);
                        U[k, i, j] = Math.Min(frota.Capacidade, frota.Capacidade + problema.usuario[i].Carga);
                    }
                }
            }

            ///////////////
            ///////////////
            string nome = Convert.ToString(problema.NumeroDeClientes) + '_' + Convert.ToString(problema.NumeroDeCarros) + "_" + Convert.ToString(frota.Capacidade);
            string caminho = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\saidas\";
            // Model
            GRBEnv env = new GRBEnv(caminho + "outputSARPTeste(" + Agora + "_" + nome + ").log");
            GRBModel model = new GRBModel(env);


            model.Set(GRB.StringAttr.ModelName, "SARPTeste");
            model.Set(GRB.DoubleParam.TimeLimit, 3600);
            ///// Declaração de variáveis

            // Variável x
            GRBVar[,,] x = new GRBVar[problema.NumeroDeCarros, problema.TamanhoMaximo, problema.TamanhoMaximo];
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                for (int j = 0; j < problema.TamanhoMaximo; j++)
                {
                    for (int k = 0; k < problema.NumeroDeCarros; k++)
                    {
                        x[k, i, j] = model.AddVar(0, 1, problema.Distancias[i, j], GRB.BINARY, "x" + k + "_" + i + "_" + j);
                        if (i == problema.TamanhoMaximo) { x[k, i, j].UB = 0.0; }
                    }
                }
            }

            // Variável B do tempo no nó i
            GRBVar[,] B = new GRBVar[problema.NumeroDeCarros, problema.TamanhoMaximo];
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                for (int k = 0; k < problema.NumeroDeCarros; k++)
                {
                    B[k, i] = model.AddVar(0, GRB.INFINITY, 0, GRB.CONTINUOUS, "B" + i + "_" + k);
                }
            }

            // Variavel Q da carga no nó i
            GRBVar[,] Q = new GRBVar[problema.NumeroDeCarros, problema.TamanhoMaximo];
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                for (int k = 0; k < problema.NumeroDeCarros; k++)
                {
                    Q[k, i] = model.AddVar(0, GRB.INFINITY, 0, GRB.CONTINUOUS, "Q" + i + "_" + k);
                }
            }

            // Varivavel L tempo de viagem do usuario quando passa por i
            GRBVar[,] L = new GRBVar[problema.NumeroDeCarros, problema.TamanhoMaximo];
            for (int i = 0; i < problema.TamanhoMaximo; i++)
            {
                for (int k = 0; k < problema.NumeroDeCarros; k++)
                {
                    L[k, i] = model.AddVar(0, GRB.INFINITY, 0, GRB.CONTINUOUS, "L" + i + "_" + k);
                }
            }



            ///////////////////////// Senso do modelo
            model.Set(GRB.IntAttr.ModelSense, GRB.MINIMIZE);

            ///////////////////////// Restrições
            // Add constraints


            // Restrição 2
            GRBLinExpr soma2;
            GRBLinExpr soma3;

            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 1; i <= problema.NumeroDeClientes; i++)
                {
                    soma2 = 0;
                    soma3 = 0;
                    for (int j = 0; j < problema.TamanhoMaximo; j++)
                    {
                        soma2.AddTerm(1, x[k, i, j]);
                        soma3.AddTerm(1, x[k, problema.NumeroDeClientes + i, j]);
                    }
                    string st = "Rt2" + i + k;
                    model.AddConstr(soma2 - soma3 == 0, st);
                    soma2.Clear();
                    soma3.Clear();
                }
            }

            // Restrição 1
            GRBLinExpr soma = 0.0;
            for (int i = 1; i <= problema.NumeroDeClientes; i++)
            {
                for (int j = 0; j < problema.TamanhoMaximo; j++)
                {
                    for (int k = 0; k < problema.NumeroDeCarros; k++)
                    {
                        soma.AddTerm(1, x[k, i, j]);
                    }
                }
                model.AddConstr(soma == 1, "Rt1" + "_" + i);
                soma.Clear();
            }



            // Restrição 3;
            GRBLinExpr soma4 = 0;
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int j = 0; j < problema.TamanhoMaximo; j++)
                {
                    soma4.AddTerm(1, x[k, 0, j]);
                }
                model.AddConstr(soma4 == 1, "Rt3" + "_" + k);
                soma4.Clear();
            }

            // Restrição 4;
            GRBLinExpr soma5 = 0;
            GRBLinExpr soma6 = 0;
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo - 1; i++)
                {
                    for (int j = 0; j < problema.TamanhoMaximo; j++)
                    {
                        soma5.AddTerm(1, x[k, j, i]);
                        soma6.AddTerm(1, x[k, i, j]);
                    }
                    model.AddConstr(soma5 - soma6 == 0, "Rt4" + i + k);
                    soma5.Clear();
                    soma6.Clear();
                }
            }

            // Restrição 5
            GRBLinExpr soma7 = 0;
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo; i++)
                {
                    soma7.AddTerm(1, x[k, i, problema.TamanhoMaximo - 1]);
                }
                model.AddConstr(soma7 == 1, "Rt5" + k);
                soma7.Clear();
            }

            // Restrição 7 Atualizar Q
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo; i++)
                {
                    for (int j = 0; j < problema.TamanhoMaximo; j++)
                    {
                        model.AddConstr(Q[k, j] >= Q[k, i] + problema.usuario[j].Carga - U[k, i, j] * (1 - x[k, i, j]), "Rt7_" + k + "_" + i + "_" + j);
                    }
                }
            }

            // Restrição 8 Atualizar B
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo; i++)
                {
                    for (int j = 0; j < problema.TamanhoMaximo; j++)
                    {
                        model.AddConstr(B[k, j] >= B[k, i] + problema.usuario[i].Duracao + problema.Distancias[i, j] - M[k, i, j] * (1 - x[k, i, j]), "Rt6_" + k + "_" + i + "_" + j);
                    }
                }
            }

            // Restrição 9 atualizar L
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 1; i <= problema.NumeroDeClientes; i++)
                {
                    model.AddConstr(L[k, i] == B[k, problema.NumeroDeClientes + i] - B[k, i] - problema.usuario[i].Duracao, "Rt8_" + k + "_" + i);
                }
            }

            // Restrição 10 Intervalo do B limitado por T
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                model.AddConstr(B[k, problema.TamanhoMaximo - 1] - B[k, 0] <= problema.LarguraHorizonte, "Rt9_" + k);
            }

            // Restrição 11 janela de B
            for (int k = 0; k < problema.NumeroDeCarros; k++)
            {
                for (int i = 0; i < problema.TamanhoMaximo; i++)
                {
                    model.AddConstr(problema.usuario[i].InstanteChegada <= B[k, i], "RtjanelaBe_" + k + "_" + i);
                    model.AddConstr(B[k, i] <= problema.usuario[i].InstanteSaida, "RtjanelaBl_" + k + "_" + i);
                }
            }

            // Restrição 12 janela de L
            for (int i = 1; i <= problema.NumeroDeClientes; i++)
            {
                for (int k = 0; k < problema.NumeroDeCarros; k++)
                {
                    model.AddConstr(problema.Distancias[i, problema.NumeroDeClientes + i] <= L[k, i], "Rt10e_" + i + "_" + k);
                    model.AddConstr(L[k, i] <= problema.TempoMaximoViagemUsuario, "Rt10l_" + i + "_" + k);
                }
            }

            // Restrição 12 janela Q
            for (int i = 1; i < problema.TamanhoMaximo; i++)
            {
                for (int k = 0; k < problema.NumeroDeCarros; k++)
                {
                    model.AddConstr(Math.Max(0, problema.usuario[i].Carga) <= Q[k, i], "Rt11e_" + i + "_" + k);
                    model.AddConstr(Q[k, i] <= Math.Min(problema.frota[0].Capacidade, problema.frota[0].Capacidade + problema.usuario[i].Carga), "Rt11l_" + i + "_" + k);
                }
            }

            string Str = Convert.ToString(problema.NumeroDeClientes) + "_" + Convert.ToString(problema.NumeroDeCarros) + Convert.ToString(problema.frota[0].Capacidade);

            // Escrever o modelo lp
            string caminho1 = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\modelos\Modelo";
            model.Write(Str + "_" + Agora + ".lp");

            // Otimizar o modelo
            model.Optimize();

            // Arquivos
            // modelo.Write("C:\Users\hellen\Dropbox\DNA sequencing\codes\DNA_Sequencing_2\Modelos\modelo.lp")
            // modelo.Write("C:\Users\hellen\Dropbox\DNA sequencing\codes\DNA_Sequencing_2\Modelos\modelo.mps")   

            string caminho2 = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\solucoes\modelo.ilp";
            if (model.Get(GRB.IntAttr.Status) == GRB.Status.INFEASIBLE)
            {
                model.ComputeIIS();    //computa o modelo ilp, infactível linear problem;
                model.Write(caminho2);
            }

            string caminho3 = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\solucoes\solucaoModelo";
            // procurar infactibilidade
            if (model.Get(GRB.IntAttr.Status) != GRB.Status.INFEASIBLE)
            {
                if (model.SolCount != 0)
                {
                    model.Write(caminho3 + Str + "_" + Agora + ".sol");
                    // modelo.Write("C:\Users\hellen\Dropbox\DNA sequencing\codes\DNA_Sequencing_2\Solucoes\solucao.mst")

                    problema.SolucaoExata = new int[problema.NumeroDeCarros - 1, problema.TamanhoMaximo, problema.TamanhoMaximo];

                    int qtdnNula = 0;
                    for (int k = 0; k < problema.NumeroDeCarros; k++)
                    {
                        for (int i = 0; i < problema.TamanhoMaximo; i++)
                        {
                            for (int j = 0; j < problema.TamanhoMaximo; j++)
                            {
                                problema.SolucaoExata[k, i, j] = Convert.ToInt32(x[k, i, j].X);
                                if (x[k, i, j].X != 0)
                                { qtdnNula += 1; }
                            }
                        }
                    }

                    problema.SolucaoExataNaoNula = new int[qtdnNula, 3];
                    int cont = 0;
                    int[] QtdAtendimentosCarro = new int[problema.NumeroDeCarros - 1];
                    for (int k = 0; k < problema.NumeroDeCarros; k++)
                    {
                        for (int i = 0; i < problema.TamanhoMaximo; i++)
                        {
                            for (int j = 0; j < problema.TamanhoMaximo; j++)
                            {
                                if (x[k, i, j].X != 0)
                                {
                                    problema.SolucaoExataNaoNula[cont, 0] = k;
                                    problema.SolucaoExataNaoNula[cont, 1] = i;
                                    problema.SolucaoExataNaoNula[cont, 2] = j;
                                    problema.SolucaoExataNaoNula[cont, 3] = Convert.ToInt32(x[k, i, j].X);
                                    cont += 1;
                                    if (k == 0)
                                    { QtdAtendimentosCarro[k] += 1; }
                                    if (k == 1)
                                    { QtdAtendimentosCarro[k] += 1; }
                                }
                            }
                        }
                    }

                    frota.Ocupacao = new double[problema.NumeroDeCarros, qtdnNula];

                    for (int i = 0; i < qtdnNula; i++)
                    {
                        if (problema.usuario[problema.SolucaoExataNaoNula[i, 1]].Carga > 0)
                        {
                            problema.usuario[i].TempoDeViagem = L[problema.SolucaoExataNaoNula[i, 0], problema.SolucaoExataNaoNula[i, 1]].X;
                        }

                    }

                    ///////// Escrever os extras, no caso tempo usuario viagem

                    string caminho4 = @"C:\Users\hcspe\Documents\Doutorado\TeseUberPool\Classe_Rotas_SARP\MSCordeau\Extras\";
                    caminho4 = caminho4 + Str + "_" + Agora + ".txt";

                    Stream entrada = File.Create(caminho4);
                    StreamWriter escritor = new StreamWriter(entrada);

                    escritor.WriteLine("Tempo Usuario Viagem");
                    for (int i = 0; i < qtdnNula; i++) { escritor.Write(Convert.ToString(problema.usuario[i].TempoDeViagem) + " "); }
                    escritor.WriteLine("   ");

                    entrada.Close();
                    escritor.Close();

                }
            }


        }
    }
}

using ConsoleText.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static readonly string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + AppDomain.CurrentDomain.BaseDirectory + @"DatabaseModel.mdf;Integrated Security=True";
        static void Main(string[] args)
        {
            #region teste impar
            Console.WriteLine("Digite um número para testar se é impar");
            int numero = Convert.ToInt32(Console.ReadLine());
            bool resultado1 = TesteImpar(numero);
            Console.WriteLine(resultado1.ToString());
            #endregion

            #region formata Array
            string[] arrayString = { "Yuri", "Marcus", "Joao", "Jhon", "Henrique", "Mary" };
            string resultado2 = FormataTexto(arrayString);
            Console.WriteLine(resultado2);
            #endregion

            #region aluno
            Aluno[] arrayAlunos = {  new Aluno { Nome = "Jonathan", DtNasc = DateTime.Parse("31/08/1996") },
                                    new Aluno { Nome = "Marcus", DtNasc = DateTime.Parse("08/07/1995") },
                                    new Aluno { Nome = "Janaina", DtNasc = DateTime.Parse("15/11/1916") },
                                    new Aluno { Nome = "Lucky", DtNasc = DateTime.Parse("01/01/2020") },
                                    new Aluno { Nome = "Hulk", DtNasc = DateTime.Parse("08/07/1988") }};

            string resultado3 = FormataAlunoTexto(arrayAlunos);
            Console.WriteLine(resultado3);
            #endregion

            #region IMPORTAR CSV
            int resultado4 = ImportaCSV();
            Console.WriteLine("Quantidade de registros importados : " + resultado4);
            #endregion

            #region Busca
            Console.WriteLine("Digite o nome para busca");
            string nome = Console.ReadLine();
            List<string> resultado5 = BuscaCliente(nome);
            Console.WriteLine(String.Join(",", resultado5));
            #endregion
            Console.ReadKey();
        }
        #region Metodos a serem implementados

        /// <summary>
        /// busca o cliente por nome no banco de dados
        /// </summary>
        /// <param name="NM_CLIENTE"></param>
        /// <returns></returns>
        private static List<string> BuscaCliente(string NM_CLIENTE)
        {
            List<string> nomes = new List<string>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[SP_CONSULTA_CLIENTE]";
                    cmd.Parameters.Add("@nomeCliente", SqlDbType.VarChar);
                    cmd.Parameters[0].Value = NM_CLIENTE;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        foreach (DataRow linhas in table.Rows)
                        {
                            string nome = linhas["NM_CLIENTE"].ToString();
                            nomes.Add(nome);
                        }
                    }

                }
            }
            return nomes;
        }
        /// <summary>
        /// Importa arquivo csv
        /// </summary>
        /// <returns></returns>
        private static int ImportaCSV()
        {

            int registros = 0;
            int nr_cliente;
            string tx_cpf, nm_cliente;
            DateTime dt_nasc;
            //Crio a conexão com o local de db
            using (SqlConnection conexao = new SqlConnection(ConnectionString))
            {

                conexao.Open();
                //crio a string de conexão
                string sql = "INSERT INTO TB_DADOS_CLIENTE VALUES (@NR_CLIENTE, @TX_CPF,@NM_CLIENTE, @DT_NASC)";
                //verifico o estado da conexão
                if (conexao.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand(sql, conexao))
                    {
                        //leio o arquivo csv
                        using (var reader = new StreamReader(@"Arquivo_Importacao.csv"))
                        {
                            string linha;
                            //passo linha por linha no csv
                            while ((linha = reader.ReadLine()) != null)
                            {
                                nr_cliente = Convert.ToInt32(linha.Split(';')[0]);
                                tx_cpf = linha.Split(';')[1];
                                nm_cliente = linha.Split(';')[2];
                                dt_nasc = DateTime.Parse(linha.Split(';')[3]);

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@NR_CLIENTE", SqlDbType.Int);
                                cmd.Parameters.Add("@TX_CPF", SqlDbType.VarChar);
                                cmd.Parameters.Add("@NM_CLIENTE", SqlDbType.VarChar);
                                cmd.Parameters.Add("@DT_NASC", SqlDbType.DateTime);
                                cmd.Parameters[0].Value = nr_cliente;
                                cmd.Parameters[1].Value = tx_cpf;
                                cmd.Parameters[2].Value = nm_cliente;
                                cmd.Parameters[3].Value = dt_nasc;

                                //verifico se existe algum registro duplicado no banco
                                if (!VerificaRegistro(nr_cliente, tx_cpf, nm_cliente, dt_nasc))
                                {
                                    cmd.ExecuteNonQuery();
                                    registros++;
                                }

                            }

                        }
                    }



                }

            }
            return registros;
        }

        private static bool TesteImpar(int numero)
        {
            //crio um vetor com os numero pares possiveis.
            string[] par = { "0", "2", "4", "6", "8" };
            string strInt = numero.ToString();
            //verifico se existe a ultimação do número é terminado com a possibilidade de número par.
            if (par.Contains(strInt[strInt.Length - 1].ToString()))
                return false;
            else
                return true;

        }

        private static string FormataTexto(string[] arrayString)
        {
            //Orderno o array
            Array.Sort(arrayString);

            // adiciona virgula entre os registros
            string strJoin = String.Join(", ", arrayString);

            //retorna e adiciona ponto ao final
            return strJoin + ".";

        }
        private static string FormataAlunoTexto(Aluno[] arrayAlunos)
        {
            List<string> retorno = new List<string>(); ;

            //ordeno os alunos por idade.
            arrayAlunos = arrayAlunos.OrderBy(x => x.DtNasc).ToArray();

            //passo por todos os alunos para concatenar o retorno
            foreach (var aluno in arrayAlunos)
            {
                //pego a idade
                int idade = DateTime.Now.Year - aluno.DtNasc.Year;
                if (DateTime.Now.Month < aluno.DtNasc.Month || (DateTime.Now.Month == aluno.DtNasc.Month && DateTime.Now.Day < aluno.DtNasc.Day))
                    idade--;

                //preencho a minha lista com o formato odernado e concatenado
                retorno.Add(aluno.Nome + "(" + idade + ")");
            }
            //retono minha lista no formato certo
            return String.Join(", ", retorno) + ".";
        }
        /// <summary>
        /// Esse metodo verifica se existe um registro
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="conexao"></param>
        /// <returns></returns>
        private static bool VerificaRegistro(int nr_cliente, string tx_cpf, string nm_cliente, DateTime dt_nasc)
        {
            using (SqlConnection cnx = new SqlConnection(ConnectionString))
            {
                cnx.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnx;
                    cmd.CommandText = "SELECT *  FROM TB_DADOS_CLIENTE where NR_CLIENTE = @NR_CLIENTE and TX_CPF = @TX_CPF and NM_CLIENTE = @NM_CLIENTE and DT_NASC = @DT_NASC";

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@NR_CLIENTE", nr_cliente);
                    cmd.Parameters.AddWithValue("@TX_CPF", tx_cpf);
                    cmd.Parameters.AddWithValue("@NM_CLIENTE", nm_cliente);
                    cmd.Parameters.AddWithValue("@DT_NASC", dt_nasc);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        if (table.Rows.Count > 0)
                            return true;
                        else
                            return false;
                    }
                }
            }

        }
    }

    #endregion
}


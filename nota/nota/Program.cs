using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Collections;


namespace nota
{
    class Program
    {
        private static readonly HttpClient http_client = new HttpClient();
        private static string url = "";
        private static Hashtable ids;

        
        static Hashtable ParseConfig(string file_path) {
            string config_text;
            Hashtable resp = new Hashtable();
            

            if (File.Exists(file_path)) {
                config_text = File.ReadAllText(file_path);
                string[] lineas = Regex.Split(config_text, "\r\n");
                for (int i = 0; i < lineas.Length; i++)
                {
                    string linea = lineas[i];
                    string[] partes = Regex.Split(linea, ":");

                    string key = partes[0];
                    var resto =  partes.Skip(1);
                    string value = string.Join(":", resto);

                    resp.Add(key, value);
                }
            }
            return resp;
        }

        static void setup() {
            string config_file_path = AppDomain.CurrentDomain.BaseDirectory + "\\nota.json";
            Hashtable config = ParseConfig(config_file_path);
            if (config.ContainsKey("url")) {
                url = Convert.ToString(config["url"]);
            }

            ids = GetIdsFromDirPath();
        }

        static async void post(Dictionary<string,string> data) 
        {
            var content = new FormUrlEncodedContent(data);
            var response = await http_client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(responseString + "(" + responseString.Length + ")");
            Console.ResetColor();
            if (responseString=="") {
                Environment.Exit(0);
            }
        }

        static Hashtable GetIdsFromDirPath() {
            Hashtable resp = new Hashtable();

            string full_path = Environment.CurrentDirectory;
            string current_dir = full_path.Substring(0, full_path.Length - 1);
            string[] partes = Regex.Split(current_dir, @"\\");
            string user_dir = partes[partes.Length - 1];
            string homework_dir = partes[partes.Length - 2];

            resp.Add("user", Regex.Split(user_dir, "-")[0].Trim());
            resp.Add("homework", Regex.Split(homework_dir, "-")[0].Trim());
            return resp;
        }

        static void Main(string[] args)
        {
            setup();

            string nota;
            string detalle;

            Console.WriteLine("Puede ingresar notas de la siguiente manera:");
            Console.WriteLine("- Una nota sobre 1: 0.8");
            Console.WriteLine("- varias notas sobre 1 concatenadas con el signo +: 0.5 + 0.3 + 0.4 + 1");
            Console.Write("Ingrese la nota: ");
            nota = Console.ReadLine();
            Console.Write("Detalle:");
            detalle = Console.ReadLine();

            File.Delete("feedback.txt");
            File.WriteAllText("feedback.txt", nota + "\n" + detalle);

            Console.Write("¿Envío la retroalimentación al estudiante? y/n ");
            char send = Console.ReadKey().KeyChar;
            if (send != 'y') {
                return;
            }

            var post_values = new Dictionary<string, string>
            {
               { "message", detalle },
               { "user", (string) ids["user"] },
               { "homework", (string) ids["homework"] },
               { "nota" , nota }
            };
            post(post_values);
            Console.ReadLine();
        }
    }
}

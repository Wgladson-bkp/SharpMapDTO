using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {

        Console.WriteLine("Indique o caminho do arquivo a ser mapeado");
        
        string path = " "+Console.ReadLine().Trim();

        Console.WriteLine("o caminho indicado foi: "+ path);

        Console.ReadKey();

        if (!File.Exists(path))
        {
            Console.WriteLine("O arquivo não foi encontrado.");
            Console.ReadKey();
            return;
        }

    
        List<string> propsPublicas = new List<string>();
        //List<string> privateProperties = new List<string>();

    
        Regex regexProps = new Regex(@"\b(public|private)\s+(\w+)\s+(\w+)\b");

        
        Regex regexClass = new Regex(@"\bpublic\s+class\s+(\w+)\b");

        string nomeClasse = null;

        
        string[] linhasCodigo = File.ReadAllLines(path);

        bool validaClasse = false;

        
        foreach (string item in linhasCodigo)
        {
            if (!validaClasse && regexClass.IsMatch(item))
            {
                Match classnome = regexClass.Match(item);
                nomeClasse = classnome.Groups[1].Value;
                validaClasse = true;
                continue;
            }

            if (validaClasse)
            {
                validaClasse = !item.Trim().StartsWith("{");
                continue;
            }

            Match okProps = regexProps.Match(item);
            if (okProps.Success)
            {
                string modificador = okProps.Groups[1].Value;
                string tipo = okProps.Groups[2].Value;
                string nomePropriedade = okProps.Groups[3].Value;

                if (modificador == "public")
                {
                    propsPublicas.Add($"{modificador} {tipo} {nomePropriedade}");
                }
                //else if (modificador == "private")
                //{
                //    propsPublicas.Add($"{modificador} {tipo} {nomePropriedade}");
                //}
            }
        }

        if (nomeClasse != null)
        {
            string dtoString = GenerateDTOClassString(nomeClasse, propsPublicas);


            Clipboard.SetText(dtoString);


            Console.WriteLine(dtoString);
        }
        else
        {
            Console.WriteLine("Não foi possível identificar o nome da classe");
        }


        
        Console.ReadKey();
    }

    static string GenerateDTOClassString(string nomeClasse, List<string> propsPublicas)
    {
        // Nome da classe DTO
        string classeNomeDTO = nomeClasse + "DTO";

        // String para armazenar a representação da classe DTO
        string classeNomeString = $"public class {classeNomeDTO}" + Environment.NewLine +
                             "{" + Environment.NewLine +
                             $"\tpublic {classeNomeDTO}({nomeClasse} {nomeClasse.ToLower()})" + Environment.NewLine +
                             "\t{"+ Environment.NewLine;

        // Mapeia as propriedades no construtor
        foreach (string itemProp in propsPublicas)
        {
            string[] parts = itemProp.Split(' ');
            string nomePropreidadeDTO = parts[2]+"DTO";
            classeNomeString += $"\t\t{nomePropreidadeDTO} = {nomeClasse.ToLower()}.{parts[2]};" + Environment.NewLine;
        }

        classeNomeString += "\t}" + Environment.NewLine + Environment.NewLine;

        // Adiciona as propriedades públicas dentro da classe DTO
        foreach (string itemProp in propsPublicas)
        {
            string[] parts = itemProp.Split(' ');
            string nomeProps = parts[2]+"DTO"; // Nome da propriedade original
            string nomePropridadeDTO = nomeProps;

            classeNomeString += $"\tpublic {parts[1]} {nomePropridadeDTO} {{ get; set; }}" + Environment.NewLine;
        }

        classeNomeString += "}";

      

        return classeNomeString;
    }
}

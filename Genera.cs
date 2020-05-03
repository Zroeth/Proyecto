using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beta
{
    class Genera
    {
        internal string Generar_ER_Tokens(string[] lineas)
        {

            string tokens = (@"( |	)*(TOKEN)( |	)*(\d)+( |	)*=( |	)*");
            string[] token = new string[1];
            token[0] += "(";

            string tokenes = (@"([A-Z]+)(   | )+([A-Z]+)(   | )*(\*)");
            string tokenes9 = (@"([A-Z]+)(	| |)+(\|)( |	|)+([A-Z]+)(	| )+(\*)");
            string tokenes3 = (@"('.')(   | )*([A-Z]+)(   | )*('.')");
            string tokenes4 = (@"(')(.)(')(')(.)(')(')(.)(')");
            string tokenes5 = (@"(')(.)(')(')(.)(')");
            string tokenes6 = (@"(')(.)(')");
            string tokenes7 = (@"( |	|)+([A-Z]+)( |	|)+(\()( |	|)+([A-Z]+)( |	|)+(\|)( |	|)+([A-Z]+)( |	|)+(\))( |	|)+(\*)( |	|)+(\{)( |	|)+([A-Z]+)(\(\))( |	|)+(\})");
            string tokenes8 = (@"( |	|)+([A-Z]+)( |	|)+(\()( |	|)+([A-Z]+)( |	|)+(\|)( |	|)+([A-Z]+)( |	|)+(\))( |	|)+(\*)( |	|)+('.')+( |	|)+(\{)( |	|)+([A-Z]+)(\(\))( |	|)+(\})");
            for (int i = 0; i < lineas.Length; i++)
            {
                token[0] += Regex.Replace(lineas[i], tokens, "(" + "$1") + ")" + " | ";
                token[0] = token[0].Replace("	", "");
            }
         
            token[0] = Regex.Replace(token[0], tokenes7, "$2" + "·" + "$4" + "$6" + "$8" + "$10" + "$12" + "$14");
            token[0] = Regex.Replace(token[0], tokenes8, "$2" + "·" + "$4" + "$6" + "$8" + "$10" + "$12" + "$14" + "·" +"$16");

            token[0] = token[0].Replace("'('", "'〈'");
            token[0] = token[0].Replace("')'", "'〉'");
            token[0] = token[0].Replace("'*'", "'×'");
            token[0] = Regex.Replace(token[0], tokenes9, "$1" + "$3" + "$5" + "$7");
            token[0] = Regex.Replace(token[0], tokenes, "$1" + "·" + "$3" + "$5");
           // File.WriteAllLines(@"C:\Users\Zer0\Desktop\token.txt", token);


            token[0] = Regex.Replace(token[0], tokenes3, "$1" + "·" + "$3" + "·" + "$5");
            token[0] = Regex.Replace(token[0], tokenes4, "$2" + "·" + "$5" + "·" + "$8");
            token[0] = Regex.Replace(token[0], tokenes5, "$2" + "·" + "$5");
            token[0] = Regex.Replace(token[0], tokenes6, "$2");
            
            //token[0] = Regex.Replace(token[0], tokenes7, "$2" + "·" + "$4" + "$6" + "$8" + "$10" + "$12" + "$14" + "·" + "$16" + "$18" + "〈〉" + "$21");

            token[0] = token[0].Replace(" ", "");
            string quitar = token[0];
            //File.WriteAllText(@"C:\Users\Zer0\Desktop\quitar.txt", quitar);
            token[0] = quitar.Remove(quitar.Length - 1);
            token[0] += ")" + "·" + "#";
            //File.WriteAllLines(@"C:\Users\Zer0\Desktop\token.txt", token);
            //  File.WriteAllLines(@"C:\Users\Zer0\Desktop\mach.txt", erMatch);
            token[0] = token[0].Replace("(+)", "(┼)");
            token[0] = token[0].Replace("(*)", "(×)");
           
            return token[0];
        }

        internal string[] Hacer_Match(string[] lineas, int inicioTokens, int finalTokens, string[] erMatch)
        {

            string tokens = (@"( |	|)*(TOKEN)( |	|)*(\d)+(   | |)*=( |	|)*");
            var palabras = new Regex(@"( |  |)+(\b[A-Z]{3}[A-Z]*\b)( |  |)+");
            
            for (int i = inicioTokens; i < finalTokens; i++)
            {
                Match match = palabras.Match(Regex.Replace(lineas[i], tokens, "$1"));
                if (match.Success)
                {
                    erMatch[i] = match.Groups[2].Value;
                }
            }
            erMatch = erMatch.Where(x => !string.IsNullOrEmpty(x)).ToArray();

           //File.WriteAllLines(@"C:\Users\Zer0\Desktop\mach.txt", erMatch);
            return erMatch;
        }

        internal string[] Generar_ER_Sets(string[] lineas, int inicioTokens, int finalTokens)
        {

            string sets = (@"((((( |    )*=( |   )* *|(\+))(')(.)(')(..)(')(.)('))( )*)*|(( *|\+? *)(')(.)(')(..)(')(.)(')( )*(\+)( )*(')(.)(')(..)(')(.)(')( )*)*|(( *|(\+))(')(.)(')( *))*|( |    )*=( |   )*( |)*(CHR)(\()[0-9]+(\))..(CHR)(\()[0-9]+(\))( |)*)*(|)$");


            string[] set = new string[lineas.Length];

            for (int i = inicioTokens; i < finalTokens; i++)
            {
                set[i] = Regex.Replace(lineas[i], sets, "$1");
                set[i] = Regex.Replace(set[i], @"\t", "");
                set[i] = Regex.Replace(set[i], @" ", "");
            }
            set = set.Where(x => !string.IsNullOrEmpty(x)).ToArray();
           // File.WriteAllLines(@"C:\Users\Zer0\Desktop\set.txt", set);

            return set;
        }



    }
}

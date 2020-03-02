using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beta
{
    class Agrupaciones
    {
        internal Boolean EstaBalanceadoQm(char[] exp)
        {
            Stack<char> st = new Stack<char>();
            for (int i = 0; i < exp.Length; i++)
            {
                if (exp[i] == '{' || exp[i] == '(' || exp[i] == '[' || exp[i] == '\'')
                {
                    st.Push(exp[i]);
                }
                if (exp[i] == '}' || exp[i] == ')' || exp[i] == ']' || exp[i] == '\'')
                {
                    if (st.Count == 0)
                    {

                        return false;

                    }
                    else if (!SeEncontroParejaQm(st.Pop(), exp[i]))
                    {
                        return false;
                    }
                }
            }
            if (st.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        internal Boolean SeEncontroParejaQm(char character1, char character2)
        {
            if (character1 == '(' && character2 == ')')
            {
                return true;

            }
            else if (character1 == '{' && character2 == '}')
            {
                return true;
            }
            else if (character1 == '[' && character2 == ']')
            {
                return true;
            }
            else if (character1 == '\'' && character2 == '\'')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal int Distancia(string[] lineas, int i, Regex terminales)
        {

            for (int j = i; j < lineas.Length; j++)
            {
                Match match2 = terminales.Match(lineas[j]);
                if (match2.Success)
                {
                    return j;

                }
            }

            return lineas.Length;
        }


        internal string[] Omitir(string[] linea, int pos)
        {
            linea[pos] = linea[pos].Replace("'(''*'", " ");
            linea[pos] = linea[pos].Replace("'*'')'", " ");
            linea[pos] = linea[pos].Replace("'{'", " ");
            linea[pos] = linea[pos].Replace("'}'", " ");
            linea[pos] = linea[pos].Replace("'('", " ");
            linea[pos] = linea[pos].Replace("')'", " ");
            linea[pos] = linea[pos].Replace("'['", " ");
            linea[pos] = linea[pos].Replace("']'", " ");
            linea[pos] = linea[pos].Replace("{", " ");
            linea[pos] = linea[pos].Replace("}", " ");
            return linea;
        }
        internal string Revisar(string[] linea, int pos, string capturada)
        {
            string[] rev = linea;
            linea[pos] = linea[pos].Replace(" ", "");
            if (rev[pos] == ("TOKENS"))
            {
                capturada += "TOKENS";

            }
            if (rev[pos] == ("ACTIONS"))
            {
                capturada += "ACTIONS";
            }
            if (rev[pos] == ("RESERVADAS()"))
            {
                capturada += "RESERVADAS()";
            }
            if (rev[pos].Contains("ERROR"))
            {
                capturada += "ERROR";
            }
            return capturada;
        }
    }


}


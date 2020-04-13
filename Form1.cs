using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Beta
{
    public partial class Form1 : Form
    {
        //Redondear esquinas del form
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr Redondear
          (
              int nLeftRect,   
              int nTopRect,      
              int nRightRect,    
              int nBottomRect,   
              int nWidthEllipse, 
              int nHeightEllipse 
          );


        public Form1()
        {

            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(Redondear(0, 0, Width, Height, 20, 20));
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox1.Text = "ARRASTRAR ARCHIVO AQUÍ";
            
        }

      
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
    

        }


        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //Para leer obtener el archivo atrastandolo 

            //guarda la ubicacion el documento en un string[]
            string[] ubicacion = (string[])(e.Data.GetData(DataFormats.FileDrop));
            string direccion = "";
            foreach (string lineas in ubicacion)
            {
                //solo se aceptan .txt
                if (File.Exists(lineas) && lineas.EndsWith(".txt"))
                {
                    using (TextReader tr = new StreamReader(lineas))
                    {

                        bandera = 0;
                        richTextBox1.Text = tr.ReadToEnd();
                        direccion = lineas;
                        richTextBox2.ResetText();
                        richTextBox3.ResetText();
                        richTextBox4.ResetText();
                        richTextBox5.ResetText();
                        richTextBox6.ResetText();
                        richTextBox7.ResetText();
                        richTextBox7.Text = "FOLLOWS:";
                        richTextBox6.Text = "FIRST:";
                        richTextBox5.Text = "LAST:";
                        var ds = dataGridView1.DataSource;
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = ds;
                        dataGridView1.Rows.Clear();
                        dataGridView1.Columns.Clear();


                        arbol.BorrarArbol(arbol.Root);
                    }
                    Proceder(direccion);
                }
                else
                {
                    MessageBox.Show("Archivo inválido");
                }

            }
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Left;


        }
        string nuevoToke;
        static int bandera;
        static int inicioTokens,inicioSets,finalTokens,finalSets;
        static string [] erSets,erMatch;

        List<Follows> Pntsig = new List<Follows>();
        List<string[]> Tabla = new List<string[]>();
        //para permitir el analisis 
        static Agrupaciones agrupacion = new Agrupaciones();
        //arbol 
        Tree arbol = new Tree();
        Genera generar = new Genera();
        Factores proceso = new Factores();
        //Permite movel la aplicacion atrastandolo desde cualquier objeto 
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        //metodo que realiza toda la operacion de analisis
        public void Proceder(string archivo)
        {

            //guarda el contenido del archivo, por linea, en un string[]
            string[] lineas = File.ReadAllLines(archivo);
            string[] lines = File.ReadAllLines(archivo);
            string[] omitir = lines;
            string[] espacios = lines;

            int distancia;

            string llaves = "";
            string capturada = "";
            //ER
            var terminales = new Regex(
             @"((?i)SETS|TOKENS|ACTIONS(?-i))([\W \W]+|)$");
            var variables = new Regex(
               @"(([A-Za-z])+( *)=(( |)*((( *|(\+))(')(.)(')(..)(')(.)('))( )*)*|(( *|\+? *)(')(.)(')(..)(')(.)(')( )*(\+)( )*(')(.)(')(..)(')(.)(')( )*)*|(( *|(\+))(')(.)(')( *))*|( |)*(CHR)(\()[0-9]+(\))..(CHR)(\()[0-9]+(\))( |)*)*(|)$)");
            var tokens = new Regex(
               @"(TOKEN)( |	)*(\d)+( |	|)+=((( |	|)+(')(.)('))+|( |	|)+([A-Z]+( |	|)+[A-Z]+( |	|)+\*)|('.')( |	|)+[A-Z]+( |	|)+('.')( |	|)+\|('.')( |	|)+[A-Z]+( |	|)+('.')( |	|)+|( |	|)+[A-Z]+( |	|)+\(( |	|)+[A-Z]+( |	|)+\|( |	|)+[A-Z]+( |	|)+\)( |	|)+\*( |	|)+\{( |	|)+[A-Z]+\(\)( |	|)+\})( |	|)+$");
            var funciones = new Regex(
               @"( |	|)*(([A-Z])+(\(\))( |	|)*)|( |	|)*((\{)|(\}))( |	|)*");
            var errores = new Regex(
               @"( |	|)*(ERROR)( |	|)*=( |	|)*([0-9]+)( |	|)*");
            var tokensfunciones = new Regex(
               @"(([0-9])+( )*=( )*((')([A-Z]+)('))( *|))|( )*((\{)|(\}))( )*");

            //analisis de signos de agrupacion
            for (int i = 0; i < omitir.Length; i++)
            {

                omitir = agrupacion.Omitir(omitir, i);

                char[] caracter = omitir[i].ToCharArray();
                if (!agrupacion.EstaBalanceadoQm(caracter))
                {
                    richTextBox2.Text += Environment.NewLine + "Los signos no están correctamente balanceados en la línea: " + (i + 1);
                    Señalar(richTextBox1, i, Color.Red);
                    richTextBox3.Text += Environment.NewLine + "Se espera que los signos esten balanceados en la línea " + (i + 1);
                }

                capturada = agrupacion.Revisar(omitir, i, capturada);

            }
            //en caso de que falte alguna palabra indispensable
            if (!capturada.Contains("TOKENS") || !capturada.Contains("RESERVADAS()") || !capturada.Contains("ACTIONS") || !capturada.Contains("ERROR"))
            {
                MessageBox.Show("ERROR");
                if (!capturada.Contains("TOKENS"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró TOKENS";
                    richTextBox3.Text += Environment.NewLine + "La palabra TOKENS debe existir";
                }
                if (!capturada.Contains("RESERVADAS()"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró la función RESERVADAS()";
                    richTextBox3.Text += Environment.NewLine + "La palabra RESERVADAS() debe existir";
                }
                if (!capturada.Contains("ACTIONS"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró ACTIONS";
                    richTextBox3.Text += Environment.NewLine + "La palabra ACTIONS debe existir";
                }
                if (!capturada.Contains("ERROR"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró una definición de ERROR";
                    richTextBox3.Text += Environment.NewLine + "La palabra ERROR debe existir";
                }
            }


            //identificar que lineas pertenecen a que ER, luego los agrega a un arbol con un identificador
            for (int i = 0; i < lineas.Length; i++)
            {
                //Para detectar terminales :D
                Match match = terminales.Match(lineas[i]);

                if (match.Success)
                {
                    string v = match.Groups[0].Value;

                    if (espacios[i].Replace(" ", "") == "SETS")
                    {
                        bandera = 1;
                        arbol.Add("SETS");

                        continue;
                    }

                    if (espacios[i].Replace(" ", "") == "TOKENS")
                    {

                        if (bandera == 1)
                        {
                            //no hay nada entre un sets y un tokens 
                            richTextBox2.Text += Environment.NewLine + "Debe existir al menos un SET";
                            richTextBox3.Text += Environment.NewLine + "Agregue un SET válido";
                        }
                        bandera = 2;
                        arbol.Add("TOKENS");
                        continue;
                    }
                    if (espacios[i].Replace(" ", "") == "ACTIONS")
                    {

                        if (bandera == 2)
                        {
                            //no hay nada entre un tokens y un actions
                            richTextBox2.Text += Environment.NewLine + "Debe existir al menos un TOKEN";
                            richTextBox3.Text += Environment.NewLine + "Agregue un TOKEN válido";

                        }
                        bandera = 3;
                        arbol.Add("ACTIONS");
                        continue;
                    }
                }
                else
                {
                    //SETS
                    if (bandera == 1)
                    {
                        //se detecto sets asi que minimo tiene que haber un set
                        string[] linea2 = lineas;
                        distancia = agrupacion.Distancia(omitir, i, terminales);
                        inicioSets = i;
                        finalSets = distancia;
                        Analizar(i, distancia, variables, linea2, lineas);
                        arbol.Add("|");
                    }

                    if (bandera == 2)
                    {
                        //se detecto tokens asi que minimo tiene que haber un token
                        string[] linea2 = lineas;
                        distancia = agrupacion.Distancia(omitir, i, terminales);
                        inicioTokens = i;
                        finalTokens = distancia;
                        Analizar(i, distancia, tokens, linea2, lines);
                        arbol.Add("*");
                    }
                    if (bandera == 3)
                    {
                        string[] linea2 = lineas;
                        linea2[i] = lineas[i];
                        distancia = agrupacion.Distancia(lineas, i, errores);
                        for (int j = i; j < distancia; j++)
                        {
                            Match match2 = funciones.Match(linea2[j]);
                            string v2 = match2.Groups[0].Value;

                            if (match2.Success)
                            {
                                arbol.Add("RESERVADAS()");
                                int distancia2 = agrupacion.Distancia(omitir, j + 1, errores);

                                for (int k = j + 1; k < distancia2; k++)
                                {
                                    if (lineas[k].Contains("{"))
                                    {
                                        llaves += lineas[k];

                                    }
                                    if (lineas[k].Contains("}"))
                                    {
                                        llaves += lineas[k];
                                    }
                                }
                                char[] caracter = llaves.ToCharArray();

                                //chequea que la cantidad de llaves en las funciones sea correcta si no pues f :'T
                                if (agrupacion.EstaBalanceadoQm(caracter))
                                {
                                    //la cantidad de llaves en la funcion es correcta;
                                    for (int k = j + 1; k < distancia2; k++)
                                    {
                                        Match match3 = tokensfunciones.Match(linea2[k]);
                                        string v3 = match3.Groups[0].Value;

                                        if (match3.Success)
                                        {
                                            bandera = 4;
                                            arbol.Add("-");
                                            i = k;
                                        }
                                        else
                                        {
                                            if (lines[k] == "")
                                            {

                                                continue;
                                            }
                                            else
                                            {
                                                richTextBox2.Text += Environment.NewLine + "Se detecto un token de función invalido: " + lineas[k] + " en la línea: " + (k + 1);
                                                richTextBox3.Text += Environment.NewLine + "Se esperaba DIGITO = 'IDENTIFICADOR': " + lineas[k] + " en la línea: " + (k + 1);
                                                Señalar(richTextBox1, k + 1, Color.Red);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    richTextBox2.Text += Environment.NewLine + "La cantidad de llaves en la función es incorrecta";
                                    richTextBox3.Text += Environment.NewLine + "Balancee las llaves en la función ";
                                }
                                j = distancia2 - 1;
                            }


                            else
                            {
                                if (lineas[j] == "")
                                {
                                    continue;
                                }
                                else
                                {
                                    richTextBox2.Text += Environment.NewLine + "Se detectó un ACTION invalido: " + linea2[j] + " en la línea: " + (j + 1);
                                    richTextBox3.Text += Environment.NewLine + "Se esperaba una FUNCION() valida: " + linea2[j] + " en la línea: " + (j + 1);
                                    Señalar(richTextBox1, j + 1, Color.Red);
                                }
                            }

                        }
                        arbol.Add("+");
                    }

                    if (bandera == 4)
                    {
                        Match match4 = errores.Match(lineas[i]);
                        string v5 = match4.Groups[0].Value;

                        if (match4.Success)
                        {
                            arbol.Add("ERROR");
                        }
                        else
                        {
                            if (lines[i] == "")
                            {
                                continue;
                            }
                            else
                            {
                                int algo = i;
                                richTextBox2.Text += Environment.NewLine + "Se detecto un ERROR invalido: " + lineas[i] + " en la línea: " + (i + 1);
                                richTextBox3.Text += Environment.NewLine + "Se esperaba ERROR = + DIGITO: " + lineas[i] + " en la línea: " + (i + 1);
                                Señalar(richTextBox1, i + 1, Color.Red);
                            }
                        }
                    }
                }
            }
            richTextBox4.Text += "Arbol (In Orden):";
            //devuelve el arbol que se creo por inorden
            arbol.InOrder();
            richTextBox4.Text += Environment.NewLine + (arbol.ObtenerNodos());
            erMatch = new string[lineas.Length];

            try
            {
                nuevoToke = generar.Generar_ER_Tokens(lineas, inicioTokens, finalTokens, erMatch);
                erSets = generar.Generar_ER_Sets(lineas, inicioSets, finalSets);
                erMatch = generar.Hacer_Match(lineas, inicioTokens, finalTokens, erMatch);
            }
            catch
            {
                MessageBox.Show("Contenido en el archivo invalido");
                return;
            }

            if (erSets.Length < erMatch.Length)
            {
                Array.Resize<string>(ref erSets, erMatch.Length);
            }
            else if (erSets.Length > erMatch.Length)
            {
                Array.Resize<string>(ref erMatch, erSets.Length);
            }
            for (int i = 0; i < erSets.Length; i++)
            {
                if (erSets[i] == null)
                {
                    erSets[i] = " ";
                }
                if (erMatch[i] == null)
                {
                    erMatch[i] = " ";
                }
            }

            for (int i = 0; i < erSets.Length; i++)
            {
                for (int j = 0; j < erMatch.Length; j++)
                {
                    if (erSets[i] != " ")
                    {
                        if (erMatch[j].Contains(erSets[i]))
                        {
                            erMatch[j] = " ";
                            erSets[i] = " ";
                            j = -1;
                        }
                    }
                }
            }
            for (int i = 0; i < erMatch.Length; i++)
            {
                if (erMatch[i] != " ")
                {
                    richTextBox3.Text += Environment.NewLine + "TOKEN NO DECLARADO EN SETS";
                    richTextBox2.Text += Environment.NewLine + "DEBE DECLARAR UN SET PARA EL TOKEN";
                    break;
                }
            }
            try
            {
                ArrayList entrada = proceso.Dividir(nuevoToke);
                ArrayList salida = proceso.Recorrer(entrada);
                Obtener(salida);
            }
            catch
            {
                MessageBox.Show("El archivo no cuenta con un formato de TOKENS valido");
            }

            //   File.WriteAllLines(@"C:\Users\Zer0\Desktop\lineas.txt",lineas);
            //   File.WriteAllLines(@"C:\Users\Zer0\Desktop\lines.txt", lines);
            //   File.WriteAllLines(@"C:\Users\Zer0\Desktop\Nmatch.txt", erMatch);
            //   File.WriteAllLines(@"C:\Users\Zer0\Desktop\Nsets.txt", erSets);

        }



        //resalta la linea del richtextbox que esta mal
        public static void Señalar( RichTextBox richTextBox, int index, Color color)
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = richTextBox.BackColor;
            var lines = richTextBox.Lines;
            if (index < 0 || index >= lines.Length)
                return;
            var start = richTextBox.GetFirstCharIndexFromLine(index);  
            var length = lines[index].Length;
            richTextBox.Select(start, length);                
            richTextBox.SelectionBackColor = color;
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //Permite mover el form 
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

       

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }
        void Analizar(int i, int distancia, Regex regex, string[] linea2, string[] lineas)
        {
            for (int j = i; j < distancia; j++)
            {
                Match match2 = regex.Match(linea2[j]);
                string v2 = match2.Groups[0].Value;
                if (match2.Success)
                {
                    //pues se detecto un set|token que es valido que asi que banderita puede cambiar 
                    bandera = 0;
                }
                else
                {
                    if (lineas[j] == ""|| lineas[j] == "\t" || lineas[j] == " ")
                    {
                
                        continue;
                    }
                    else
                    {
                        richTextBox2.Text += Environment.NewLine + "Línea invalida: " + linea2[j] + "  en la línea:  " + (j + 1);
                        richTextBox3.Text += Environment.NewLine + "Agregue una expresión válida" + "  en la línea:  " + (j + 1);

                    }
                }
            }
        }
        public void Obtener(ArrayList cadenaT)
        {
            Stack<Nodo> Simbolos = new Stack<Nodo>();
            List<Nodo> cadenaMod = new List<Nodo>();
            ArrayList ST = new ArrayList();
            int cont = 1;
            for (int i = 0; i < cadenaT.Count; i++)
            {
                if (cadenaT[i].ToString() != "·" && cadenaT[i].ToString() != "(" && cadenaT[i].ToString() != "|" && cadenaT[i].ToString() != ")" && cadenaT[i].ToString() != "*" && cadenaT[i].ToString() != "+" && cadenaT[i].ToString() != "?")
                {
                    Follows Siguiente = new Follows();
                    Siguiente.indice = cont;
                    Siguiente.grupo = cadenaT[i].ToString();
                    Pntsig.Add(Siguiente);
                    Nodo N = new Nodo();
                    N.simbolo = cadenaT[i].ToString();
                    N.First.Add(cont);
                    N.Last.Add(cont);
                    N.Nulo = false;
                    cont++;
                    cadenaMod.Add(N);
                    if ((!ST.Contains(cadenaT[i])) && cadenaT[i].ToString() != "#")
                        ST.Add(cadenaT[i]);
                }
                else
                {
                    Nodo N = new Nodo();
                    N.simbolo = cadenaT[i].ToString();
                    cadenaMod.Add(N);
                }
            }
            for (int i = 0; i < cadenaT.Count; i++)
            {
                if (cadenaT[i].ToString() != "·" && cadenaT[i].ToString() != "(" && cadenaT[i].ToString() != "|" && cadenaT[i].ToString() != ")" && cadenaT[i].ToString() != "*" && cadenaT[i].ToString() != "+" && cadenaT[i].ToString() != "?")
                {
                    Simbolos.Push(cadenaMod[i]);
                }
                else
                {
                    NuevoNodo(cadenaT[i].ToString(), ref Simbolos);
                }
            }
            Imprimir();
            Queue<string> grupo1 = new Queue<string>();
            List<string> grupo2 = new List<string>();
            string[] conjuntos = new string[ST.Count + 1];
            string temp = "";
            Nodo N2 = Simbolos.Pop();
            List<int> First = N2.First;
            for (int i = 0; i < First.Count; i++)
            {
                temp += First[i] + ",";
            }
            temp = temp.TrimEnd(',');
            conjuntos[0] = temp;
            grupo1.Enqueue(temp);
            grupo2.Add(temp);
            ST.Sort();
            while (grupo1.Count != 0)
            {
                if (grupo1.Count != 0)
                {
                    conjuntos[0] = grupo1.Dequeue();
                }
                for (int i = 0; i < ST.Count; i++)
                {
                    for (int j = 0; j < Pntsig.Count; j++)
                    {
                        if (ST[i].ToString() == Pntsig[j].grupo && conjuntos[0].Contains(Pntsig[j].indice.ToString()))
                        {
                            for (int k = 0; k < Pntsig[j].Follow.Count; k++)
                            {
                                if (conjuntos[i + 1] == null || conjuntos[i + 1] == "")
                                {
                                    conjuntos[i + 1] += Pntsig[j].Follow[k].ToString();
                                }
                                else if (!conjuntos[i + 1].Contains(Pntsig[j].Follow[k].ToString()))
                                {
                                    conjuntos[i + 1] += "," + Pntsig[j].Follow[k].ToString();
                                }
                            }
                            conjuntos[i + 1] = conjuntos[i + 1].TrimEnd(',');
                            if (conjuntos[0] != conjuntos[i + 1] && !grupo2.Contains(conjuntos[i + 1]))
                            {
                                grupo1.Enqueue(conjuntos[i + 1]);
                                grupo2.Add(conjuntos[i + 1]);
                            }
                        }
                    }
                }
                Tabla.Add(conjuntos);
                conjuntos = new string[ST.Count + 1];

            }
            TablaDgrd(ST, grupo2);
            ST = null;
            grupo2 = null;
        }
        public void TablaDgrd(ArrayList cadena1, List<string> grupo2)
        {
            ArrayList objgrupo = new ArrayList();
            for (int i = 65; i <= 90; i++)
            {
                objgrupo.Add((char)i);
            }
            for (int j = 0; j <= 26; j++)
            {
                for (int l = 0; l <= 26; l++)
                {
                    objgrupo.Add(objgrupo[j].ToString() + objgrupo[l].ToString());
                }
            }
            dataGridView1.RowCount = grupo2.Count + 1;
            dataGridView1.ColumnCount = cadena1.Count + 1;
            for (int i = 1; i <= cadena1.Count; i++)
            {
               dataGridView1.Rows[0].Cells[i].Value = cadena1[i - 1].ToString();
            }
            for (int i = 0; i < Tabla.Count; i++)
            {
                for (int j = 0; j < cadena1.Count + 1; j++)
                {
                    for (int k = 0; k < grupo2.Count; k++)
                    {
                        if (Tabla[i][j] == grupo2[k])
                        {
                            if (i == 0 && j == 0)
                            {
                                if (Tabla[i][j].Contains(Pntsig.Last().indice.ToString()) && j == 0)
                                {
                                  dataGridView1.Rows[i + 1].Cells[j].Value = objgrupo[k];
                                }
                                else
                                {
                                   dataGridView1.Rows[i + 1].Cells[j].Value = objgrupo[k];
                                }
                            }
                            else
                            {
                                if (Tabla[i][j].Contains(Pntsig.Last().indice.ToString()) && j == 0)
                                {
                                   dataGridView1.Rows[i + 1].Cells[j].Value = objgrupo[k];
                                }
                                else
                                {
                                    dataGridView1.Rows[i + 1].Cells[j].Value = objgrupo[k];
                                }
                            }
                        }
                    }
                }
            }
            Tabla.Clear();
            richTextBox3.Text += Environment.NewLine + "Archivo correcto";
        }
        public void Imprimir()
        {
            string linea = "";
            for (int i = 0; i < Pntsig.Count; i++)
            {
                linea += Pntsig[i].indice.ToString() + ")  ";
                for (int j = 0; j < Pntsig[i].Follow.Count; j++)
                {
                    if (j == Pntsig[i].Follow.Count - 1)
                    {
                        linea += Pntsig[i].Follow[j].ToString();
                    }
                    else
                    {
                        linea += Pntsig[i].Follow[j].ToString() + ",";
                    }
                }
                richTextBox7.Text += Environment.NewLine + linea;
                linea = "";
            }
        }
        public void NuevoNodo(string op, ref Stack<Nodo> simb)
        {
            Nodo Nuevo = new Nodo();
            switch (op)
            {
                case "*":
                    Nodo C1_Asterisco = simb.Pop();
                    Nuevo.simbolo = "(" + C1_Asterisco.simbolo + ")*";
                    Nuevo.First = C1_Asterisco.First;
                    Nuevo.Last = C1_Asterisco.Last;
                    foreach (var s in Nuevo.First)
                    {
                        richTextBox6.Text += Environment.NewLine + "First de: " + "(*) : " + s;
                    }
                    foreach (var s in Nuevo.Last)
                    {
                        richTextBox5.Text += Environment.NewLine + "Last de: " + "(*) : " + s;
                    }
                    Nuevo.Nulo = true;
                    simb.Push(Nuevo);
                    ObtenerFollow(Nuevo);
                    break;
                case "+":
                    Nodo C1_Mas = simb.Pop();
                    Nuevo.simbolo = "(" + C1_Mas.simbolo + ")+";
                    Nuevo.First = C1_Mas.First;
                    Nuevo.Last = C1_Mas.Last;
                    foreach (var s in Nuevo.First)
                    {
                        richTextBox6.Text += Environment.NewLine + "First de: " + "(+) : " + s;
                    }
                    foreach (var s in Nuevo.Last)
                    {
                        richTextBox5.Text += Environment.NewLine + "Last de: " + "(+) : " + s;
                    }
                    Nuevo.Nulo = false;
                    simb.Push(Nuevo);
                    ObtenerFollow(Nuevo);
                    break;
                case "?":
                    Nodo C1_Interrogacion = simb.Pop();
                    Nuevo.simbolo = "(" + C1_Interrogacion.simbolo + ")?";
                    Nuevo.First = C1_Interrogacion.First;
                    Nuevo.Last = C1_Interrogacion.Last;
                    foreach (var s in Nuevo.First)
                    {
                        richTextBox6.Text += Environment.NewLine + "First de: " + "(?) : " + s;
                    }
                    foreach (var s in Nuevo.Last)
                    {
                        richTextBox5.Text += Environment.NewLine + "Last de: " + "(?) : " + s;
                    }
                    Nuevo.Nulo = true;
                    simb.Push(Nuevo);
                    break;
                case "·":
                    Nodo simbcont = simb.Pop();
                    Nodo simbcont2 = simb.Pop();
                    Nuevo.simbolo = simbcont2.simbolo + "·" + simbcont.simbolo;
                    if (simbcont2.Nulo)
                    {
                        List<int> FirstTemp = new List<int>();
                        for (int i = 0; i < simbcont2.First.Count; i++)
                        {
                            FirstTemp.Add(simbcont2.First[i]);
                        }
                        for (int i = 0; i < simbcont.First.Count; i++)
                        {
                            FirstTemp.Add(simbcont.First[i]);
                        }
                        Nuevo.First = FirstTemp;
                    }
                    else
                    {
                        Nuevo.First = simbcont2.First;
                    }
                    if (simbcont.Nulo)
                    {
                        List<int> LastTemp = new List<int>();
                        for (int i = 0; i < simbcont2.Last.Count; i++)
                        {
                            LastTemp.Add(simbcont2.Last[i]);
                        }
                        for (int i = 0; i < simbcont.Last.Count; i++)
                        {
                            LastTemp.Add(simbcont.Last[i]);
                        }
                        Nuevo.Last = LastTemp;
                    }
                    else
                    {
                        Nuevo.Last = simbcont.Last;
                    }
                    if (simbcont2.Nulo == true && simbcont.Nulo == true)
                    {
                        Nuevo.Nulo = true;
                    }
                    else
                    {
                        Nuevo.Nulo = false;
                    }
                    simb.Push(Nuevo);
                    ObtenerFollowSig(simbcont2, simbcont);
                    foreach (var s in Nuevo.First)
                    {
                        richTextBox6.Text += Environment.NewLine + "First de: " + "\"" + simb.Peek().simbolo + "\"" + " : " + s;
                    }
                    foreach (var s in Nuevo.Last)
                    {
                        richTextBox5.Text += Environment.NewLine + "Last de: " + "\"" + simb.Peek().simbolo + "\"" + " : " + s;
                    }
                    break;
                case "|":
                    Nodo C2_Or = simb.Pop();
                    Nodo C1_Or = simb.Pop();
                    Nuevo.simbolo = C1_Or.simbolo + "|" + C2_Or.simbolo;
                    for (int i = 0; i < C1_Or.First.Count; i++)
                    {
                        Nuevo.First.Add(C1_Or.First[i]);
                    }
                    for (int i = 0; i < C2_Or.First.Count; i++)
                    {
                        Nuevo.First.Add(C2_Or.First[i]);
                    }
                    for (int i = 0; i < C1_Or.Last.Count; i++)
                    {
                        Nuevo.Last.Add(C1_Or.Last[i]);
                    }
                    for (int i = 0; i < C2_Or.Last.Count; i++)
                    {
                        Nuevo.Last.Add(C2_Or.Last[i]);
                    }
                    if (C1_Or.Nulo == true || C2_Or.Nulo == true)
                    {
                        Nuevo.Nulo = true;
                    }
                    else
                    {
                        Nuevo.Nulo = false;
                    }
                    simb.Push(Nuevo);
                    foreach (var s in Nuevo.First)
                    {
                        richTextBox6.Text += Environment.NewLine + "First de: " + "(|) : " + s;
                    }
                    foreach (var s in Nuevo.Last)
                    {
                        richTextBox5.Text += Environment.NewLine + "Last de: " + "(|) : " + s;
                    }
                    break;

            }
        }

        public void ObtenerFollow(Nodo nuevo)
        {
            for (int i = 0; i < nuevo.Last.Count; i++)
            {
                for (int j = 0; j < Pntsig.Count; j++)
                {
                    if (nuevo.Last[i] == Pntsig[j].indice)
                    {
                        for (int k = 0; k < nuevo.First.Count; k++)
                        {
                            if (!Pntsig[j].Follow.Contains(nuevo.First[k]))
                            {
                                Pntsig[j].Follow.Add(nuevo.First[k]);
                            }
                        }
                    }
                }
            }
        }
        public void ObtenerFollowSig(Nodo nuevo, Nodo nuevo2)
        {
            for (int i = 0; i < nuevo.Last.Count; i++)
            {
                for (int j = 0; j < Pntsig.Count; j++)
                {
                    if (nuevo.Last[i] == Pntsig[j].indice)
                    {
                        for (int k = 0; k < nuevo2.First.Count; k++)
                        {
                            if (!Pntsig[j].Follow.Contains(nuevo2.First[k]))
                            {
                                Pntsig[j].Follow.Add(nuevo2.First[k]);
                            }
                        }
                    }
                }
            }
        }
        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }
        private void richTextBox1_Click(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;


        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //al darle click al gif, se cierra el programa
            Close();
        }

        private void richTextBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void richTextBox2_Leave(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox2_Click(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox2_Enter(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox3_Click(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox3_Enter(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox3_Leave(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

       
       
        private void richTextBox4_Click(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox6_Click(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox6_Enter(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox6_Leave(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void richTextBox5_Click(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox5_Enter(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox5_Leave(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void richTextBox7_Click(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox7_Enter(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox7_Leave(object sender, EventArgs e)
        {
            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox7_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Form1_DragEnter_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void richTextBox4_Enter(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox4_Leave(object sender, EventArgs e)
        {

            //Oculta el signo de intercalación
            ActiveControl = null;
        }

        private void richTextBox4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void richTextBox1_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}

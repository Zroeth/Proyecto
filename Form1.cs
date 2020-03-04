using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

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
             //Para leer obtener el archivo atrastandolo 
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //guarda la ubicacion el documento en un string[]
                string[] ubicacion = (string[])(e.Data.GetData(DataFormats.FileDrop));
                string direccion="";
                foreach (string lineas in ubicacion)
                {
                    //solo se aceptan .txt
                    if (File.Exists(lineas) &&  lineas.EndsWith(".txt"))
                    {
                        using (TextReader tr = new StreamReader(lineas))
                        {

                            bandera = 0;
                            richTextBox1.Text = tr.ReadToEnd();
                            direccion = lineas;
                            richTextBox2.ResetText();
                            richTextBox3.ResetText();
                            richTextBox4.ResetText();
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
         

        }
    
        static int bandera;
        //para permitir el analisis 
        static Agrupaciones agrupacion = new Agrupaciones();
        //arbol 
        Tree arbol = new Tree();

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
               @"(TOKEN)( |	)*(\d)+( |	)*=( |	)*(((\(+)( |	)*[A-Z]+( |	)*[A-Z]+( |	)*(\)+)( |	)*)+|((( |	|)*((')+(.)+(')+)+( |	|)*)+)|((([A-Z]*)+( |	)*((\*|\||\(|\)|\{|\}))+)( |	))*|([A-Z]+( |	)*[A-Z]+( |	)*((\*|\||\(|\)|\{|\})))|\})+");
            var funciones = new Regex(
               @"(([A-Z])+(\(\))( )*)|( )*((\{)|(\}))( )*");
            var errores = new Regex(
               @"(ERROR)( )*=( )*([0-9]+)( )*");
            var tokensfunciones = new Regex(
               @"(([0-9])+( )*=( )*((')([A-Z]+)('))( *|))|( )*((\{)|(\}))( )*");

            //analisis de signos de agrupacion
            for (int i = 0; i < omitir.Length; i++)
            {

                omitir = agrupacion.Omitir(omitir, i);

                char[] caracter = omitir[i].ToCharArray();
                if (!agrupacion.EstaBalanceadoQm(caracter))
                {
                    richTextBox2.Text += Environment.NewLine+"Los signos no están correctamente balanceados en la línea: " +(i+1);
                    Señalar(richTextBox1, i , Color.Red);
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
                        linea2[i] = lineas[i];
                        distancia = agrupacion.Distancia(omitir, i, terminales);
                        Analizar(i, distancia, variables, linea2, lineas);
                        arbol.Add("|");
                    }

                    if (bandera == 2)
                    {
                        //se detecto tokens asi que minimo tiene que haber un token
                        string[] linea2 = lineas;
                        linea2[i] = lineas[i];
                        distancia = agrupacion.Distancia(omitir, i, terminales);
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
                                                Señalar(richTextBox1, k+1, Color.Red);
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
                                int algo=i;
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
            richTextBox4.Text += Environment.NewLine+( arbol.ObtenerNodos());
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

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
         
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
                        if (lineas[j] == "")
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

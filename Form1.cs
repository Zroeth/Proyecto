using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;


namespace Beta
{
    public partial class Form1 : Form
    {

      
       
        public Form1()
        {
            InitializeComponent();
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox1.Text = "ARRASTRAR ARCHIVO AQUI";
            
        }
  
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                
                string[] ubicacion = (string[])(e.Data.GetData(DataFormats.FileDrop));
                string direccion="";
                foreach (string lineas in ubicacion)
                {
                    if (File.Exists(lineas) &&  lineas.EndsWith(".txt"))
                    {
                        using (TextReader tr = new StreamReader(lineas))
                        {

                            bandera = 0;
                            richTextBox1.Text = tr.ReadToEnd();
                            direccion = lineas;
                            richTextBox2.ResetText();
                        }

                    }
                    else
                    {
                        MessageBox.Show("Archivo invalido");
                    }
                   
                }
                richTextBox1.SelectAll();
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                Proceder(direccion);
            }
         

        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        static int bandera;
        static Agrupaciones agrupacion = new Agrupaciones();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        public void Proceder(string archivo)
        {


            string[] lineas = File.ReadAllLines(archivo);
            string[] lines = File.ReadAllLines(archivo);
            string[] omitir = lines;
            string[] espacios = lines;
            int distancia;
            string llaves = "";
            string capturada = "";

            var terminales = new Regex(
             @"((?i)SETS|TOKENS|ACTIONS(?-i))([\W \W]+|)$");
            var variables = new Regex(
               @"(([A-Za-z])+( *)=(( |)*((( *|(\+))(')(.)(')(..)(')(.)('))( )*)*|(( *|\+? *)(')(.)(')(..)(')(.)(')( )*(\+)( )*(')(.)(')(..)(')(.)(')( )*)*|(( *|(\+))(')(.)(')( *))*|( |)*(CHR)(\()[0-9]+(\))..(CHR)(\()[0-9]+(\))( |)*)*(|)$)");
            var tokens = new Regex(
               @"((TOKEN)( |	)*(\d)+( |	)*=( |	)*(((\()( |	)*[A-Z]+( |	)*[A-Z]+( |	)*(\))( |	)*)+|(( |	)*((')(.)+('))+( |	)*)|([A-Z])+( |	)*((.)*|))+)");
            var funciones = new Regex(
               @"(([A-Z])+(\(\))( )*)|( )*((\{)|(\}))( )*");
            var errores = new Regex(
               @"(ERROR)( )*=( )*([0-9]+)( )*");
            var tokensfunciones = new Regex(
               @"(([0-9])+( )*=( )*((')([A-Z]+)('))( *|))|( )*((\{)|(\}))( )*");

            for (int i = 0; i < omitir.Length; i++)
            {

                omitir = agrupacion.Omitir(omitir, i);

                char[] caracter = omitir[i].ToCharArray();
                if (!agrupacion.EstaBalanceadoQm(caracter))
                {
                    richTextBox2.Text += Environment.NewLine+"Los signos no están correctamente balanceados en la línea: " +(i+1);
                    Señalar(richTextBox1, i , Color.Red);
                }
              
                capturada = agrupacion.Revisar(omitir, i, capturada);

            }
            if (!capturada.Contains("TOKENS") || !capturada.Contains("RESERVADAS()") || !capturada.Contains("ACTIONS") || !capturada.Contains("ERROR"))
            {
                MessageBox.Show("ERROR");
                if (!capturada.Contains("TOKENS"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró TOKENS";
                }
                if (!capturada.Contains("RESERVADAS()"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró la función RESERVADAS()";
                }
                if (!capturada.Contains("ACTIONS"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró ACTIONS";
                }
                if (!capturada.Contains("ERROR"))
                {
                    richTextBox2.Text += Environment.NewLine + "No se encontró una definición de ERROR";
                }
            }


       
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
                        continue;
                    }

                    if (espacios[i].Replace(" ", "") == "TOKENS")
                    {

                        if (bandera == 1)
                        {
                            //no hay nada entre un sets y un tokens asi que efe
                            richTextBox2.Text += Environment.NewLine + "Debe existir al menos un SET";
                        }
                        bandera = 2;
                        continue;
                    }
                    if (espacios[i].Replace(" ", "") == "ACTIONS")
                    {

                        if (bandera == 2)
                        {
                            //no hay nada entre un tokens y un actions asi que efe
                            richTextBox2.Text += Environment.NewLine + "Debe existir al menos un TOKEN";
                        }
                        bandera = 3;
                        continue;
                    }
                }
                else
                {
                    //SETS
                    if (bandera == 1)
                    {
                        //pues se detecto sets asi que minimo tiene que haber un set uwu
                        string[] linea2 = lineas;
                        linea2[i] = lineas[i];
                        distancia = agrupacion.Distancia(omitir, i, terminales);
                        Analizar(i, distancia, variables, linea2, lineas);
                    }

                    if (bandera == 2)
                    {
                        //pues se detecto tokens asi que minimo tiene que haber un set uwu
                        string[] linea2 = lineas;
                        linea2[i] = lineas[i];
                        distancia = agrupacion.Distancia(omitir, i, terminales);
                        Analizar(i, distancia, tokens, linea2, lines);
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
                                                Señalar(richTextBox1, k+1, Color.Red);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    richTextBox2.Text += Environment.NewLine + "La cantidad de llaves en la función es incorrecta";
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
                                    Señalar(richTextBox1, j + 1, Color.Red);
                                }
                            }
                          
                        }
                    }

                    if (bandera == 4)
                    {
                        Match match4 = errores.Match(lineas[i]);
                        string v5 = match4.Groups[0].Value;

                        if (match4.Success)
                        {
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
                                richTextBox2.Text += Environment.NewLine + "Se detecto un error invalido: " + lineas[i] + " en la línea: " + (i + 1);
                                Señalar(richTextBox1, i + 1, Color.Red);
                            }
                        }
                    }
                }
            }
        }

        public static void Señalar( RichTextBox richTextBox, int index, Color color)
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = richTextBox.BackColor;
            var lines = richTextBox.Lines;
            if (index < 0 || index >= lines.Length)
                return;
            var start = richTextBox.GetFirstCharIndexFromLine(index);  // Get the 1st char index of the appended text
            var length = lines[index].Length;
            richTextBox.Select(start, length);                 // Select from there to the end
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
                        //pues se detecto un set que es valido que asi que banderita puede cambiar :')
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
                           richTextBox2.Text += Environment.NewLine + "Línea invalida: " + linea2[j] + "  en:  " + (j + 1);
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
            ActiveControl = null;
        }

        private void richTextBox2_Click(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void richTextBox2_Enter(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void richTextBox3_Click(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void richTextBox3_Enter(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void richTextBox3_Leave(object sender, EventArgs e)
        {
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
    }
}

using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsApp2
{
    
    public partial class Form1 : Form


    {   
        Pen pen;
        double[] signal;
        double[] normal_signal;
        double[] filtred_signal;
        double[] normal_filtred_signal;
        int[] QRS;
        char[] NAME;
        int[] Cluster;
        bool[] filtered;
        DataTable table;
        int[] MAX;
        

        int dot_per_QRS = 400;

       
        void draw_cluster()
        {
            Graphics graphics = pictureBox4.CreateGraphics();
            graphics.Clear(Color.White);

            double pix_per_dot = (double)pictureBox4.Width / (double)dot_per_QRS;

            Point[] points = new Point[dot_per_QRS];
            for(int i = 2;i < QRS.Length-1; i++)
            {
                if (filtered[i])
                {
                    for (int j = 0; j < points.Length; j++)
                    {
                        points[j] = new Point((int)((double)j * pix_per_dot), pictureBox4.Height - (int)(normal_signal[QRS[i] - dot_per_QRS / 2 + j] * pictureBox4.Height));
                    }
                    graphics.DrawLines(pen, points);
                }
                
            }
        }
        void draw_lorenz()
        {

            Graphics graphics = pictureBox3.CreateGraphics();
            graphics.Clear(Color.White);

            double[] RR = new double[QRS.Length - 1];
            for(int i = 0;i < QRS.Length - 1; i++)
            {
                RR[i] = QRS[i + 1] - QRS[i];
            }
            RR = Fourier.Normalization(RR);

            for (int i = 0; i < RR.Length - 1; i++) {
                if (filtered[i])
                    graphics.DrawEllipse(pen,(int)(RR[i]* pictureBox3.Width), (int)(pictureBox3.Height - RR[i+1]* pictureBox3.Height), 10, 10);
                
            }
            graphics.DrawString(QRS.Length.ToString(),  new Font("Arial", 16), new SolidBrush(Color.Black), new Point(100, 100));
          
           
        }

        void draw_all_plot()
        {
            Graphics all = pictureBox2.CreateGraphics();
            all.Clear(Color.White);

            double dot_per_pix = (double)signal.Length / (double)pictureBox2.Width;
            Point[] all_points = new Point[pictureBox2.Width];

            for (int i = 0; i < pictureBox2.Width; i++)
            {
                all_points[i] = new Point(i, (int)(pictureBox2.Height - (normal_signal[(int)(i * dot_per_pix)]*pictureBox2.Height)));
            }
            all.DrawLine(pen, new Point((int)((double)hScrollBar1.Value / (double)dot_per_pix), 0), new Point((int)((double)hScrollBar1.Value / (double)dot_per_pix), pictureBox1.Height));
            all.DrawLine(pen, new Point((int)((double)(hScrollBar1.Value + pictureBox1.Width / (hScrollBar2.Value / 100)) / (double)dot_per_pix), 0), new Point((int)((double)(hScrollBar1.Value + pictureBox1.Width / (hScrollBar2.Value / 100)) / (double)dot_per_pix), pictureBox1.Height));
            all.DrawLines(pen, all_points);

        }

        void draw_window()
        {
            Graphics graphics = pictureBox1.CreateGraphics();
            List<Point> temp = new();
            List<Point> temp_1 = new();

            for (int i = 0, n = 0; i < pictureBox1.Width; i += hScrollBar2.Value / 100, n++)
            {
                temp.Add(new Point(i, (int)(pictureBox1.Height - (normal_signal[n + hScrollBar1.Value]* pictureBox1.Height))));
                temp_1.Add(new Point(i, (int)(pictureBox1.Height - (normal_filtred_signal[n + hScrollBar1.Value] * pictureBox1.Height))));

            }
            Point [] points = temp.ToArray();
            graphics.Clear(Color.White);
            graphics.DrawLines(pen, points);
            points = temp_1.ToArray();
            graphics.DrawLines(pen, points);
        }

    
        static int[] stringToArrayInt(string text)
        {
            string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int[] array = new int[words.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = Int32.Parse(words[i]);
            return array;
        }

        public Form1()
        {
            
            InitializeComponent();

            pen = new(Color.Black, 1f);

            

        }
        private void map_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                    hScrollBar2.Value = Math.Min(hScrollBar2.Value + e.Delta, hScrollBar2.Maximum);
                else
                    hScrollBar2.Value = Math.Max(hScrollBar2.Value + e.Delta, hScrollBar2.Minimum);
                hScrollBar1.Maximum = signal.Length - 1010 / (hScrollBar2.Value / 100);
                hScrollBar1.Value = Math.Min(hScrollBar1.Value, hScrollBar1.Maximum);

            }
            else {
                if (e.Delta > 0)
                    hScrollBar1.Value = Math.Min(hScrollBar1.Value + e.Delta/10, hScrollBar1.Maximum);
                else
                    hScrollBar1.Value = Math.Max(hScrollBar1.Value + e.Delta/10, hScrollBar1.Minimum);
            }

            draw_window();
            draw_all_plot();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

            draw_window();
            draw_all_plot();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            hScrollBar1.Maximum = signal.Length - 1010 / (hScrollBar2.Value / 100);
            hScrollBar1.Value = Math.Min(hScrollBar1.Value, hScrollBar1.Maximum);
            draw_window();
            draw_all_plot();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath = ofd.FileName;
            }

            string[] lines = System.IO.File.ReadAllLines(@filePath);


            int[] temp = stringToArrayInt(lines[0]);
            signal = new double[temp.Length];
            for (int i = 0; i < temp.Length; i++) signal[i] = temp[i];
            normal_signal = Fourier.Normalization(signal);

            
            filtred_signal = new double[signal.Length];
            
            for (int i = 0; i < signal.Length; i++) filtred_signal[i] = signal[i];
            
            filtred_signal = Fourier.Bandpass_filter(filtred_signal, 360, 5, 17);

            filtred_signal = Fourier.Difference(filtred_signal);
            filtred_signal = Fourier.Square(filtred_signal);
            filtred_signal = Fourier.Integration(filtred_signal);
            //MAX = Fourier.AAMT(d_list);
            


            table = new DataTable();
            table.Columns.Add("QRS", typeof(int));
            table.Columns.Add("TAG", typeof(char));
            table.Columns.Add("CLUSTER", typeof(int));

            

            string[] equal = System.IO.File.ReadAllLines("C:\\Users\\asuna\\CLionProjects\\Fourier\\cmake-build-debug\\qrs.txt");

            QRS = new int[equal.Length];
            HashSet<int> QRS_UNIQUE = new();
            NAME = new char[equal.Length];
            HashSet<char> NAME_UNIQUE = new();
            Cluster = new int[equal.Length];
            HashSet<int> CLUSTER_UNIQUE = new();
            for (int i = 0; i < equal.Length; i++)
            {
                string[] words = equal[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                QRS[i] = Int32.Parse(words[0]);
                QRS_UNIQUE.Add(QRS[i]);
                NAME[i] = Char.Parse(words[1]);
                NAME_UNIQUE.Add(NAME[i]);
                Cluster[i] = i % 10;
                CLUSTER_UNIQUE.Add(Cluster[i]);
                table.Rows.Add(QRS[i], NAME[i],0);
            }

            dataGridView1.DataSource = table;
            /*
            int j = 0,
                r = 0,
                s = 0;
            while(j < equal_arr.Length && r < MAX.Length)
            {
                while (r < MAX.Length-1 && Math.Abs(equal_arr[j] - MAX[r]) > 15 && MAX[r] - equal_arr[j] > 15 )
                    r++;
                if (Math.Abs(equal_arr[j] - MAX[r]) <= 15)
                    s += 1;
                j++;
            }
            

            StreamWriter sw = new StreamWriter("C:\\Users\\asuna\\CLionProjects\\Fourier\\cmake-build-debug\\Test1.txt", true, Encoding.ASCII);
            sw.Write(s + "\n" + equal_arr.Length + "\n");
            for (int i = 0; i < MAX.Length; i++)
                sw.Write(MAX[i].ToString()+"\n");
            sw.Close();
            */
            normal_filtred_signal = Fourier.Normalization(filtred_signal);

            
            pictureBox1.MouseWheel += new MouseEventHandler(map_MouseWheel);
            hScrollBar1.Maximum = signal.Length - pictureBox1.Width / (hScrollBar2.Value / 100);

            draw_all_plot();
            //draw_cluster();
            //draw_lorenz();
            draw_window();

            char[] NAME_arr = new char[NAME_UNIQUE.Count];
            NAME_UNIQUE.CopyTo(NAME_arr, 0);
            for(int i = 0; i < NAME_arr.Length;i++)
            {
                checkedListBox1.Items.Add(NAME_arr[i]);
            }

            int[] CLUSTER_arr = new int[CLUSTER_UNIQUE.Count];
            CLUSTER_UNIQUE.CopyTo(CLUSTER_arr, 0);
            for(int i = 0;i < CLUSTER_UNIQUE.Count; i++)
            {
                checkedListBox2.Items.Add(CLUSTER_arr[i]);
            }

            filtered = new bool[QRS.Length];
            for (int i = 0; i < filtered.Length; i++) filtered[i] = true;
            
        }

        private void filtr()
        {
            table.Clear();
            HashSet<char> SET_1 = new();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i) == true) SET_1.Add((char)checkedListBox1.Items[i]);

            HashSet<int> SET_2 = new();
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
                if (checkedListBox2.GetItemChecked(i) == true) SET_2.Add((int)checkedListBox2.Items[i]);


            for (int i = 0; i < QRS.Length; i++)
            {
                if ((SET_1.Contains(NAME[i]) || SET_1.Count == 0) && (SET_2.Contains(Cluster[i]) || SET_2.Count == 0))
                {
                    table.Rows.Add(QRS[i], NAME[i], Cluster[i]);
                    filtered[i] = true;
                }
                else filtered[i] = false;
            }
            dataGridView1.DataSource = table;
            draw_cluster();
            draw_lorenz();
            
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            filtr();
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            filtr();
        }
    }
}
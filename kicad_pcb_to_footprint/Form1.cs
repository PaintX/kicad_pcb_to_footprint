using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace kicad_pcb_to_footprint
{
    public partial class Form1 : Form
    {
        struct s_Offset
        {
            public float x;
            public float y;
            public float ang;
        };

        s_Offset sOffset;
        Bitmap image1;


        public void DrawLine(PointF p1, PointF p2, float ang ,Color c, Bitmap bmp)
        {
            Pen blackPen = new Pen(c, 0.2f);

            using (var graphics = Graphics.FromImage(bmp))
            {
                //move rotation point to center of image
                graphics.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                graphics.RotateTransform(ang);
                graphics.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                graphics.DrawLine(blackPen, p1,p2);
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            image1 = new Bitmap(320, 240);

            String[] lines = File.ReadAllLines(textBox1.Text);

            if (lines[0].StartsWith("(kicad_pcb"))
            { 
                //-- fichier kicad_pcb

                foreach ( String l in lines )
                {
                    String line = l.Trim();

                    line = line.Replace("(", "");
                    line = line.Replace(")", "");

                    if (line.StartsWith("at"))
                    {
                        line = line.Replace(".", ",");

                        String[] explo = line.Split(' ');

                        sOffset.x = float.Parse(explo[1]);
                        sOffset.y = float.Parse(explo[2]);
                        try
                        {
                            sOffset.ang = float.Parse(explo[3]);
                        }
                        catch { sOffset.ang = 0;  }
                    }

                    if (line.StartsWith("fp_line"))
                    {
                        line = line.Replace(".", ",");
                        String[] explo = line.Split(' ');
                        PointF start = new PointF();
                        PointF end = new PointF();

                        start.X = float.Parse(explo[2]);
                        start.Y = float.Parse(explo[3]);

                        end.X = float.Parse(explo[5]);
                        end.Y = float.Parse(explo[6]);

                        start.X += sOffset.x;
                        start.Y += sOffset.y;

                        end.X += sOffset.x;
                        end.Y += sOffset.y;

                        DrawLine(start, end, sOffset.ang, Color.Black, image1);
                    }

                    if (line.StartsWith("gr_line"))
                    {
                        line = line.Replace(".", ",");
                        String[] explo = line.Split(' ');

                        PointF start = new PointF();
                        PointF end = new PointF();

                        start.X = float.Parse(explo[2]);
                        start.Y = float.Parse(explo[3]);

                        end.X = float.Parse(explo[5]);
                        end.Y = float.Parse(explo[6]);

                        DrawLine(start, end,0/* float.Parse(explo[8])*/, Color.Black, image1);
                    }


                }

                

                StreamWriter file = new StreamWriter(textBox1.Text + ".kicad_mod");


                pictureBox1.Image = image1;
            }
        }



    }
}

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
        s_Offset sMousePos;
        s_Offset sStart;
        Bitmap image1;

        public float factor = 10.0f;

        public PointF _getNewCoord(PointF p1, PointF center,float ang)
        {
            PointF ptDepart = new PointF(p1.X,p1.Y);

            double angleDegre = ang * -1;
            double angleRadian = Math.PI * angleDegre / 180;
            double sina = Math.Sin(angleRadian);
            double cosa = Math.Cos(angleRadian);
            double x1 = ((ptDepart.X - center.X) * cosa) - ((ptDepart.Y - center.Y) * sina) + center.X;
            double y1 = ((ptDepart.X - center.X) * sina) + ((ptDepart.Y - center.Y) * cosa) + center.Y;

            return new PointF((float)x1, (float)y1);
        }



        public void DrawLine(PointF p1, PointF p2, float ang ,Color c, Bitmap bmp)
        {
            Pen blackPen = new Pen(c, 0.2f);
            var graphics = Graphics.FromImage(bmp);

            graphics.DrawLine(blackPen, p1,p2);

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

        public void _RedrawFoorprint()
        {
            image1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            image1.SetPixel(0, 0, Color.Black);

            if (textBox1.Text.Length > 0)
            {
                String[] lines = File.ReadAllLines(textBox1.Text);
                if (lines[0].StartsWith("(kicad_pcb"))
                {
                    //-- fichier kicad_pcb
                    foreach (String l in lines)
                    {
                        String line = l.Trim();

                        line = line.Replace("(", "");
                        line = line.Replace(")", "");

                        if (line.StartsWith("area"))
                        {
                            line = line.Replace(".", ",");
                            String[] explo = line.Split(' ');
                            PointF start = new PointF();
                            PointF end = new PointF();

                            start.X = float.Parse(explo[1]);
                            start.Y = float.Parse(explo[2]);

                            end.X = float.Parse(explo[3]);
                            end.Y = float.Parse(explo[4]);

                            sStart.x = (start.X);
                            sStart.y = (start.Y);

                            // image1 = new Bitmap((int)(Math.Ceiling(end.X - start.X) * factor), (int)(Math.Ceiling(end.Y - start.Y) * factor));
                            // pictureBox1.Width = (int)(Math.Ceiling(end.X - start.X) * factor);
                            // pictureBox1.Height = (int)(Math.Ceiling(end.Y - start.Y) * factor);
                        }

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
                            catch
                            {
                                sOffset.ang = 0.0f;
                            }

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

                            PointF o = new PointF(sOffset.x, sOffset.y);
                            start = _getNewCoord(start, o, sOffset.ang);
                            end = _getNewCoord(end, o, sOffset.ang);

                            start.X -= sStart.x;
                            end.X -= sStart.x;

                            start.Y -= sStart.y;
                            end.Y -= sStart.y;

                            start.X *= factor;
                            start.Y *= factor;

                            end.X *= factor;
                            end.Y *= factor;

                            start.X += sMousePos.x;
                            end.X += sMousePos.x;

                            start.Y += sMousePos.y;
                            end.Y += sMousePos.y;

                            DrawLine(start, end, 0, Color.Black, image1);
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

                            start.X -= sStart.x;
                            end.X -= sStart.x;

                            start.Y -= sStart.y;
                            end.Y -= sStart.y;

                            start.X *= factor;
                            start.Y *= factor;

                            end.X *= factor;
                            end.Y *= factor;

                            start.X += sMousePos.x;
                            end.X += sMousePos.x;

                            start.Y += sMousePos.y;
                            end.Y += sMousePos.y;
                            /*
                                                        PointF o = new PointF(0.0f, 0.0f);
                                                        start = _getNewCoord(start, o, 90.0f);
                                                        end = _getNewCoord(end, o, 90.0f);
                                                        */
                            DrawLine(start, end, 0, Color.Black, image1);
                        }


                    }
                }
                //  StreamWriter file = new StreamWriter(textBox1.Text + ".kicad_mod");

            }
            pictureBox1.Image = image1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _RedrawFoorprint();
        }


        bool isDown = false;
        bool isMoved = false;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isDown = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if ( isMoved == false )
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    factor += 1.0f;
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    factor -= 1.0f;
                }
                _RedrawFoorprint();
            }
            isMoved = false;
            isDown = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDown == true)
            {
                sMousePos.x = (e.X - pictureBox1.Location.X);
                sMousePos.y = (e.Y - pictureBox1.Location.Y);
                _RedrawFoorprint();
                isMoved = true;
            }
        }



    }
}

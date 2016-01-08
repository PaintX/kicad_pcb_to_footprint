using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace kicad_pcb_to_footprint
{
    public partial class Form1 : Form
    {
        Bitmap image1;
        kicad_parser kicad;

        public void _RedrawFoorprint()
        {
            if ((image1 != null) && (kicad != null ))
            {
                kicad.draw(image1);
                pictureBox1.Image = image1;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            kicad = new kicad_parser();
            image1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;

                if (textBox1.Text.Length > 0)
                {
                    kicad.Parse(textBox1.Text);
                }
            }

            _RedrawFoorprint();
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
                image1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if ( kicad != null )
                        kicad.setFactor(kicad.getFactor() + 1.0);
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (kicad != null)
                        kicad.setFactor(kicad.getFactor() - 1.0);
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
                image1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                PointF p = new PointF(e.X - pictureBox1.Width / 2, e.Y - pictureBox1.Height/2);
                kicad.setMousePosition(p);
                _RedrawFoorprint();
                isMoved = true;
            }
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _RedrawFoorprint();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                kicad.saveFootprint(textBox1.Text);
            }
            /*if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //textBox1.Text = openFileDialog1.FileName;

                if (saveFileDialog1.FileName.Length > 0)
                {
                    kicad.saveFootprint(saveFileDialog1.FileName);
                    //kicad.Parse(textBox1.Text);
                }
            } */ 
        }
    }
}

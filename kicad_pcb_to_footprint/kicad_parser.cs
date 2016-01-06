using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace kicad_pcb_to_footprint
{
    class kicad_parser
    {
        double factor;
        PointF mousePosition;
        kicad_element kicad_elements = new kicad_element();

        public kicad_parser()
        {
            mousePosition = new PointF(0,0);
            factor = 10.0;
            kicad_elements.createList();
        }

        public PointF _getNewCoord(PointF p1, PointF center, float ang)
        {
            PointF ptDepart = new PointF(p1.X, p1.Y);

            double angleDegre = ang * -1;
            double angleRadian = Math.PI * angleDegre / 180;
            double sina = Math.Sin(angleRadian);
            double cosa = Math.Cos(angleRadian);
            double x1 = ((ptDepart.X - center.X) * cosa) - ((ptDepart.Y - center.Y) * sina) + center.X;
            double y1 = ((ptDepart.X - center.X) * sina) + ((ptDepart.Y - center.Y) * cosa) + center.Y;

            return new PointF((float)x1, (float)y1);
        }

        private void _DrawCircle(Bitmap bmp, Color c,
                                  float centerX, float centerY, float radius)
        {
            Pen pen = new Pen(c, 0.2f);
            var g = Graphics.FromImage(bmp);
            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        private void _FillCircle(Bitmap bmp, Brush brush,
                                      float centerX, float centerY, float radius)
        {
            var g = Graphics.FromImage(bmp);
            g.FillEllipse(brush, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }
        
        private void _DrawLine(PointF p1, PointF p2, float ang, Color c, Bitmap bmp)
        {
            Pen blackPen = new Pen(c, 0.2f);
            var graphics = Graphics.FromImage(bmp);

            graphics.DrawLine(blackPen, p1, p2);

        }

        private Color _GetKicadColor(String layer)
        {
            Color c = new Color();
            c = Color.Black;
            if (layer.StartsWith("Edge,Cuts"))
            {
                c = Color.Yellow;
            }

            if (layer.StartsWith("F,SilkS"))
            {
                c = Color.CadetBlue;
            }

            if (layer.StartsWith("Cmts,User"))
            {
                c = Color.Blue;
            }

            if (layer.StartsWith("*,Cu"))
            {
                c = Color.Gold;
            }

            return c;
        }

        public void Parse(String file)
        {
            String[] lines = File.ReadAllLines(file);
            if (lines[0].StartsWith("(kicad_pcb"))
            {
                //-- fichier kicad_pcb
                foreach (String l in lines)
                {
                    String line = l.Trim();

                    line = line.Replace("(", "");
                    line = line.Replace(")", "");

                    kicad_element.kicad_elements ke = new kicad_element.kicad_elements();

                    line = line.Replace(".", ",");
                    String[] explo = line.Split(' ');

                    if (explo[0].Equals("area"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_AREA;

                        ke.rect.start.x = double.Parse(explo[1]);
                        ke.rect.start.y = double.Parse(explo[2]);

                        ke.rect.end.x = double.Parse(explo[3]);
                        ke.rect.end.y = double.Parse(explo[4]);

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("at"))
                    {

                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_POSITION;

                        ke.pos.x = float.Parse(explo[1]);
                        ke.pos.y = float.Parse(explo[2]);
                        try
                        {
                            ke.angle = float.Parse(explo[3]);
                        }
                        catch
                        {
                            ke.angle = 0.0f;
                        }
                        kicad_elements.add(ke);
                    }
                    if (explo[0].Equals("fp_line"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_LINE;

                        ke.line.start.x = float.Parse(explo[2]);
                        ke.line.start.y = float.Parse(explo[3]);

                        ke.line.end.x = float.Parse(explo[5]);
                        ke.line.end.y = float.Parse(explo[6]);

                        ke.color = _GetKicadColor(explo[8]);

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("gr_line"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND;

                        ke.line.start.x = float.Parse(explo[2]);
                        ke.line.start.y = float.Parse(explo[3]);

                        ke.line.end.x = float.Parse(explo[5]);
                        ke.line.end.y = float.Parse(explo[6]);

                        ke.color = _GetKicadColor(explo[10]);

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("pad"))
                    {
                        if ( explo[3].Equals("oval"))
                        {
                            ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_OVAL;

                            ke.circle.x = float.Parse(explo[5]);
                            ke.circle.y = float.Parse(explo[6]);

                            //-- pad
                            try
                            {
                                ke.circle.r = float.Parse(explo[8]) / 2.0f;
                                ke.color = _GetKicadColor(explo[13]);
                            }
                            catch
                            {
                                ke.circle.r = float.Parse(explo[9]) / 2.0f;
                                ke.color = _GetKicadColor(explo[14]);
                            }

                            kicad_elements.add(ke);

                            //-- trou
                            try
                            {
                                ke.circle.r = float.Parse(explo[11]) / 2.0f;
                            }
                            catch
                            {
                                ke.circle.r = float.Parse(explo[12]) / 2.0f;
                            }
                            ke.color = Color.White;
                            kicad_elements.add(ke);
                        }
                        

                        
                    }
                }
            }
        }


        struct S_OFFSET
        {
            public double x;
            public double y;
            public double angle;
        };

        S_OFFSET positionStart = new S_OFFSET();
        S_OFFSET offset = new S_OFFSET();
        
        public void setFactor(double f)
        {
            factor = f;
        }
        public double getFactor()
        {
            return factor;
        }

        public void setMousePosition(PointF p)
        {
            mousePosition.X = p.X;
            mousePosition.Y = p.Y;
        }


        public void draw(Bitmap bmp)
        {
            draw(bmp, false);
        }
        public void draw(Bitmap bmp,Boolean inFile)
        {
            for (int i = 0; i < kicad_elements.count(); i++)
            {
                kicad_element.kicad_elements ke = kicad_elements.get(i);

                switch (ke.type)
                {
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_AREA):
                    {
                        positionStart.x = ke.rect.start.x;
                        positionStart.y = ke.rect.start.y;
                        positionStart.angle = ke.angle;
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_POSITION):
                    {
                        offset.x = ke.pos.x;
                        offset.y = ke.pos.y;
                        offset.angle = ke.angle;
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_OVAL):
                    {
                        PointF p1 = new PointF((float)ke.circle.x, (float)ke.circle.y);

                        p1.X += (float)offset.x;
                        p1.Y += (float)offset.y;
                
                        PointF o = new PointF((float)offset.x, (float)offset.y);
                        p1 = _getNewCoord(p1, o, (float)offset.angle);


                        p1.X -= (float)positionStart.x;
                        p1.Y -= (float)positionStart.y;

                        p1.X *= (float)factor;
                        p1.Y *= (float)factor;

                        p1.X += mousePosition.X;
                        p1.Y += mousePosition.Y;

                        SolidBrush myBrush = new SolidBrush(ke.color);

                        _FillCircle(bmp, myBrush , p1.X, p1.Y, (float)(ke.circle.r * factor));
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND):
                    {
                        PointF p1 = new PointF((float)ke.line.start.x, (float)ke.line.start.y);
                        PointF p2 = new PointF((float)ke.line.end.x, (float)ke.line.end.y);

                        p1.X -= (float)positionStart.x;
                        p2.X -= (float)positionStart.x;

                        p1.Y -= (float)positionStart.y;
                        p2.Y -= (float)positionStart.y;

                        p1.X *= (float)factor;
                        p1.Y *= (float)factor;

                        p2.X *= (float)factor;
                        p2.Y *= (float)factor;

                        p1.X += mousePosition.X;
                        p2.X += mousePosition.X;

                        p1.Y += mousePosition.Y;
                        p2.Y += mousePosition.Y;

                        _DrawLine(p1, p2, 0, ke.color, bmp);
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_LINE):
                    {
                        PointF p1 = new PointF((float)ke.line.start.x, (float)ke.line.start.y);
                        PointF p2 = new PointF((float)ke.line.end.x, (float)ke.line.end.y);

                        p1.X += (float)offset.x;
                        p2.X += (float)offset.x;

                        p1.Y += (float)offset.y;
                        p2.Y += (float)offset.y;

                        PointF o = new PointF((float)offset.x, (float)offset.y);
                        p1 = _getNewCoord(p1, o, (float)offset.angle);
                        p2 = _getNewCoord(p2, o, (float)offset.angle);

                        p1.X -= (float)positionStart.x;
                        p2.X -= (float)positionStart.x;

                        p1.Y -= (float)positionStart.y;
                        p2.Y -= (float)positionStart.y;

                        p1.X *= (float)factor;
                        p1.Y *= (float)factor;

                        p2.X *= (float)factor;
                        p2.Y *= (float)factor;

                        p1.X += mousePosition.X;
                        p2.X += mousePosition.X;

                        p1.Y += mousePosition.Y;
                        p2.Y += mousePosition.Y;

                        _DrawLine(p1, p2, 0, ke.color, bmp);
                        break;
                    }
                }
            }
        }

        public void saveFootprint(String fileStr)
        {
            String[] lines = File.ReadAllLines(fileStr);
            StreamWriter file = new StreamWriter(fileStr + ".kicad_mod");
            if (lines[0].StartsWith("(kicad_pcb"))
            {
                file.WriteLine("(module test (layer F.Cu) (tedit 0)");
                file.WriteLine("(solder_mask_margin 0.1)");

                
              
                file.WriteLine(")");
            }
            file.Close();

        }
    }
}

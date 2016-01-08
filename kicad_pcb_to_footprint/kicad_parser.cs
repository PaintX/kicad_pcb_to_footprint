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
        StreamWriter file;
        int numPad = 0;

        public kicad_parser()
        {
            mousePosition = new PointF(0,0);
            factor = 10.0;
            kicad_elements.createList();
        }

        public PointF _getNewCoord(PointF p1, PointF center, float ang)
        {
            PointF ptDepart = new PointF(p1.X, p1.Y);

            double angleDegre = ang;
            double angleRadian = Math.PI * (angleDegre / 180);
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

        private void _FillRectangle(Bitmap bmp, Brush brush,
                                      PointF[] p)
        {
            var g = Graphics.FromImage(bmp);
            g.FillPolygon(brush, p);
            //g.FillRectangle(brush, s.X, s.Y, e.X - s.X, e.Y - s.Y);
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
            mousePosition = new PointF(0, 0);
            kicad_element.kicad_layer_element layer = kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_TOP;
            kicad_elements.createList();
            String[] lines = File.ReadAllLines(file);
            if (lines[0].StartsWith("(kicad_pcb"))
            {
                //-- fichier kicad_pcb
                foreach (String l in lines)
                {
                    String line = l.Trim();
                    kicad_element.kicad_elements ke = new kicad_element.kicad_elements();

                    ke.file_Line = line;

                    line = line.Replace("(", "");
                    line = line.Replace(")", "");

                    line = line.Replace(".", ",");
                    String[] explo = line.Split(' ');
                    ke.file_line_param = line.Split(' ');

                    ke.layer = layer;
                    ke.color = _GetKicadColor("");

                    if (explo[0].Equals("module"))
                    {
                        int idxStart = kicad_elements.findIdx(ke, "layer");

                        String param = kicad_elements.getStringAt(ke,idxStart + 1);

                        if (param.Equals("B,CU"))
                            layer = kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_BOTTOM;
                        else if (param.Equals("F,CU"))
                            layer = kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_TOP;
                    }
                    if (explo[0].Equals("area"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_AREA;

                        ke.rect.start.x = kicad_elements.getValueAt(ke, 1);
                        ke.rect.start.y = kicad_elements.getValueAt(ke, 2);

                        ke.rect.size.width = kicad_elements.getValueAt(ke, 3) - ke.rect.start.x;
                        ke.rect.size.height = kicad_elements.getValueAt(ke, 4) - ke.rect.start.y;

                        ke.rect.start.x += ke.rect.size.width / 2;
                        ke.rect.start.y += ke.rect.size.height / 2;

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("at"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_POSITION;

                        ke.pos.x = kicad_elements.getValueAt(ke, 1);
                        ke.pos.y = kicad_elements.getValueAt(ke, 2);
                        ke.angle = kicad_elements.getValueAt(ke, 3);

                        kicad_elements.add(ke);
                    }
                    if (explo[0].Equals("fp_line"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_LINE;

                        int idxStart = kicad_elements.findIdx(ke, "start");

                        ke.line.start.x = kicad_elements.getValueAt(ke,idxStart+1);
                        ke.line.start.y = kicad_elements.getValueAt(ke, idxStart + 2);

                        int idxEnd = kicad_elements.findIdx(ke, "end");
                        ke.line.end.x = kicad_elements.getValueAt(ke, idxEnd + 1);
                        ke.line.end.y = kicad_elements.getValueAt(ke, idxEnd + 2);

                        ke.color = _GetKicadColor(explo[8]);

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("gr_circle"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND_CIRCLE;

                        int idx = kicad_elements.findIdx(ke, "center");
                        ke.circle.x = kicad_elements.getValueAt(ke, idx + 1);
                        ke.circle.y = kicad_elements.getValueAt(ke, idx + 2);

                        PointF end = new PointF();
                        idx = kicad_elements.findIdx(ke, "end");
                        end.X = (float)kicad_elements.getValueAt(ke, idx + 1);
                        end.Y = (float)kicad_elements.getValueAt(ke, idx + 2);

                        ke.circle.px = end.X;
                        ke.circle.py = end.Y;

                       //  (x - x0)2 + (y - y0)2 = r2
                        double r2 = Math.Pow((end.X - ke.circle.x), 2.0) + Math.Pow((end.Y - ke.circle.y), 2.0);
                        ke.circle.r = Math.Sqrt(r2);


                        kicad_elements.add(ke);
                    }
                    if (explo[0].Equals("gr_line"))
                    {
                        ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND_LINE;

                        int idxStart = kicad_elements.findIdx(ke, "start");

                        ke.line.start.x = kicad_elements.getValueAt(ke, idxStart + 1);
                        ke.line.start.y = kicad_elements.getValueAt(ke, idxStart + 2);

                        int idxEnd = kicad_elements.findIdx(ke, "end");
                        ke.line.end.x = kicad_elements.getValueAt(ke, idxEnd + 1);
                        ke.line.end.y = kicad_elements.getValueAt(ke, idxEnd + 2);

                        ke.color = _GetKicadColor(explo[10]);

                        kicad_elements.add(ke);
                    }

                    if (explo[0].Equals("pad"))
                    {
                        if (explo[3].Equals("oval") || explo[3].Equals("circle"))
                        {
                            ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_PAD_OVAL;

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
                         /*   try
                            {
                                ke.circle.r = float.Parse(explo[11]) / 2.0f;
                            }
                            catch
                            {
                                ke.circle.r = float.Parse(explo[12]) / 2.0f;
                            }
                            ke.color = Color.White;
                            kicad_elements.add(ke);*/
                        }

                        if (explo[3].Equals("rect"))
                        {
                            ke.type = kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_PAD_RECT;

                            ke.rect.start.x = kicad_elements.getValueAt(ke, 5);
                            ke.rect.start.y = kicad_elements.getValueAt(ke, 6);

                            try
                            {
                                ke.angle = float.Parse(explo[7]);

                                ke.rect.size.width = kicad_elements.getValueAt(ke, 9);
                                ke.rect.size.height = kicad_elements.getValueAt(ke, 10);

                                ke.color = _GetKicadColor(explo[14]);
                            }
                            catch
                            {
                                ke.rect.size.width = kicad_elements.getValueAt(ke, 8);
                                ke.rect.size.height = kicad_elements.getValueAt(ke, 9);
                                ke.color = _GetKicadColor(explo[13]);
                            }
                            

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

        kicad_element.kicad_rectangle area = new kicad_element.kicad_rectangle();
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
            numPad = 0;
            for (int i = 0; i < kicad_elements.count(); i++)
            {
                Boolean write = false;
                kicad_element.kicad_elements ke = kicad_elements.get(i);

                switch (ke.type)
                {
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_AREA):
                    {
                        area.start.x = ke.rect.start.x;
                        area.start.y = ke.rect.start.y;

                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_POSITION):
                    {
                        offset.x = ke.pos.x;
                        offset.y = ke.pos.y;
                        offset.angle = ke.angle;
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_PAD_RECT):
                    {
                        PointF[] p = new PointF[4];

                        PointF center = new PointF((float)ke.rect.start.x, (float)ke.rect.start.y);

                        p[0].X = center.X - ((float)ke.rect.size.width / 2.0f);
                        p[0].Y = center.Y + ((float)ke.rect.size.height / 2.0f);

                        p[1].X = center.X + ((float)ke.rect.size.width / 2.0f);
                        p[1].Y = center.Y + ((float)ke.rect.size.height / 2.0f);

                        p[2].X = center.X + ((float)ke.rect.size.width / 2.0f);
                        p[2].Y = center.Y - ((float)ke.rect.size.height / 2.0f);

                        p[3].X = center.X - ((float)ke.rect.size.width / 2.0f);
                        p[3].Y = center.Y - ((float)ke.rect.size.height / 2.0f);

                        double mirror = 1.0f;
                        if (ke.layer == kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_TOP)
                        { 
                            mirror *= -1;
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            p[j].X -= (float)(offset.x* mirror);
                            p[j].Y -= (float)(offset.y* mirror);
                        }

                        center.X += (float)offset.x;
                        center.Y += (float)offset.y;

                        PointF o = new PointF((float)offset.x, (float)offset.y);
                        p[0] = _getNewCoord(p[0], o, (float)((offset.angle) * mirror));
                        p[1] = _getNewCoord(p[1], o, (float)((offset.angle) * mirror));
                        p[2] = _getNewCoord(p[2], o, (float)((offset.angle) * mirror));
                        p[3] = _getNewCoord(p[3], o, (float)((offset.angle) * mirror));


                        center.X -= (float)area.start.x;
                        center.Y -= (float)area.start.y;


                        if (inFile == false)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                p[j].X -= (float)(area.start.x);
                                p[j].X *= (float)factor;
                                p[j].X += mousePosition.X;

                                p[j].Y -= (float)(area.start.y);
                                p[j].Y *= (float)factor;
                                p[j].Y += mousePosition.Y;

                                p[j].X += (float)bmp.Width / 2;
                                p[j].Y += (float)bmp.Height / 2;
                            }

                            SolidBrush myBrush = new SolidBrush(ke.color);
                            _FillRectangle(bmp, myBrush, p);
                        }
                        else
                        {
                            int idx = kicad_elements.findIdx(ke, "pad");
                            kicad_elements.setStringAt(ke, idx + 1, numPad.ToString());

                            idx = kicad_elements.findIdx(ke, "at");
                            kicad_elements.setStringAt(ke, idx + 1, center.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, center.Y.ToString());

                            numPad++;
                            write = true;
                        }
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_PAD_OVAL):
                    {
                        PointF p1 = new PointF((float)ke.circle.x, (float)ke.circle.y);

                        p1.X += (float)offset.x;
                        p1.Y += (float)offset.y;

                        double mirror = 1.0f;
                        if (ke.layer == kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_TOP)
                        {
                            mirror *= -1;
                        }

                        PointF o = new PointF((float)offset.x, (float)offset.y);
                        p1 = _getNewCoord(p1, o, (float)(offset.angle * mirror));

                        p1.X -= (float)area.start.x;
                        p1.Y -= (float)area.start.y;


                        if (inFile == false)
                        {
                            p1.X *= (float)factor;
                            p1.Y *= (float)factor;

                            p1.X += (float)bmp.Width / 2;
                            p1.Y += (float)bmp.Height / 2;

                            p1.X += mousePosition.X;
                            p1.Y += mousePosition.Y;

                            SolidBrush myBrush = new SolidBrush(ke.color);

                            _FillCircle(bmp, myBrush, p1.X, p1.Y, (float)(ke.circle.r * factor));
                        }
                        else
                        {
                            int idx = kicad_elements.findIdx(ke, "pad");
                            kicad_elements.setStringAt(ke, idx + 1, numPad.ToString());

                            idx = kicad_elements.findIdx(ke, "at");
                            kicad_elements.setStringAt(ke, idx + 1, p1.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p1.Y.ToString());

                            numPad++;
                            write = true;
                        }
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND_CIRCLE):
                    {
                        PointF p1 = new PointF((float)ke.circle.x, (float)ke.circle.y);
                        PointF p2 = new PointF((float)ke.circle.px, (float)ke.circle.py);

                        p1.X -= (float)area.start.x;
                        p1.Y -= (float)area.start.y;

                        p2.X -= (float)area.start.x;
                        p2.Y -= (float)area.start.y;

                        if (inFile == false)
                        {
                            p1.X *= (float)factor;
                            p1.Y *= (float)factor;

                            p1.X += (float)bmp.Width / 2;
                            p1.Y += (float)bmp.Height / 2;

                            p1.X += mousePosition.X;
                            p1.Y += mousePosition.Y;

                            _DrawCircle(bmp, ke.color, p1.X, p1.Y, (float)(ke.circle.r * factor));
                        }
                        else 
                        {
                            int idx = kicad_elements.findIdx(ke, "gr_circle");
                            kicad_elements.setStringAt(ke, idx, "fp_circle");

                            idx = kicad_elements.findIdx(ke, "center");
                            kicad_elements.setStringAt(ke, idx + 1, p1.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p1.Y.ToString());

                            idx = kicad_elements.findIdx(ke, "end");
                            kicad_elements.setStringAt(ke, idx + 1, p2.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p2.Y.ToString());

                            idx = kicad_elements.findIdx(ke, "layer");

                            if (!kicad_elements.getStringAt(ke, idx + 1).Equals("Margin"))
                                kicad_elements.setStringAt(ke, idx + 1, "F,SilkS");

                            write = true;
                        }
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_GROUND_LINE):
                    {
                        PointF p1 = new PointF((float)ke.line.start.x, (float)ke.line.start.y);
                        PointF p2 = new PointF((float)ke.line.end.x, (float)ke.line.end.y);

                        p1.X -= (float)area.start.x;
                        p2.X -= (float)area.start.x;

                        p1.Y -= (float)area.start.y;
                        p2.Y -= (float)area.start.y;

                        if (inFile == false)
                        {
                            p1.X *= (float)factor;
                            p1.Y *= (float)factor;

                            p2.X *= (float)factor;
                            p2.Y *= (float)factor;

                            p1.X += (float)bmp.Width / 2;
                            p2.X += (float)bmp.Width / 2;

                            p1.Y += (float)bmp.Height / 2;
                            p2.Y += (float)bmp.Height / 2;

                            p1.X += mousePosition.X;
                            p2.X += mousePosition.X;

                            p1.Y += mousePosition.Y;
                            p2.Y += mousePosition.Y;

                            _DrawLine(p1, p2, 0, ke.color, bmp);
                        }
                        else
                        {
                            //-- gr_line transform to fp_line
                            int idx = kicad_elements.findIdx(ke, "gr_line");
                            kicad_elements.setStringAt(ke, idx, "fp_line");

                            idx = kicad_elements.findIdx(ke, "start");
                            kicad_elements.setStringAt(ke, idx + 1, p1.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p1.Y.ToString());

                            idx = kicad_elements.findIdx(ke, "end");
                            kicad_elements.setStringAt(ke, idx + 1, p2.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p2.Y.ToString());

                            idx = kicad_elements.findIdx(ke, "layer");

                            if ( !kicad_elements.getStringAt(ke, idx + 1).Equals("Margin") )
                                kicad_elements.setStringAt(ke, idx + 1, "F,SilkS");

                            idx = kicad_elements.findIdx(ke, "angle");
                            kicad_elements.setStringAt(ke, idx, "");
                            kicad_elements.setStringAt(ke, idx + 1, "");

                            write = true;
                        }
                        break;
                    }
                    case (kicad_element.kicad_type_element.KICAD_TYPE_ELEMENT_LINE):
                    {
                        PointF p1 = new PointF((float)ke.line.start.x, (float)ke.line.start.y);
                        PointF p2 = new PointF((float)ke.line.end.x, (float)ke.line.end.y);

                        p1.X += (float)offset.x;
                        p1.Y += (float)offset.y;

                        p2.X += (float)offset.x;
                        p2.Y += (float)offset.y;
                        
                        double mirror = 1.0f;
                        if (ke.layer == kicad_element.kicad_layer_element.KICAD_LAYER_ELEMENT_TOP)
                        {
                            mirror *= -1;
                        }

                        PointF o = new PointF((float)offset.x, (float)offset.y);
                        p1 = _getNewCoord(p1, o, (float)(offset.angle * mirror));
                        p2 = _getNewCoord(p2, o, (float)(offset.angle * mirror));

                        p1.X -= (float)area.start.x;
                        p2.X -= (float)area.start.x;

                        p1.Y -= (float)area.start.y;
                        p2.Y -= (float)area.start.y;

                        if (inFile == false)
                        {
                            p1.X *= (float)factor;
                            p1.Y *= (float)factor;

                            p2.X *= (float)factor;
                            p2.Y *= (float)factor;

                            p1.X += (float)bmp.Width/2;
                            p2.X += (float)bmp.Width / 2;

                            p1.Y += (float)bmp.Height / 2;
                            p2.Y += (float)bmp.Height / 2;

                            p1.X += mousePosition.X;
                            p2.X += mousePosition.X;

                            p1.Y += mousePosition.Y;
                            p2.Y += mousePosition.Y;

                            _DrawLine(p1, p2, 0, ke.color, bmp);
                        }
                        else
                        {
                            int idx = kicad_elements.findIdx(ke, "start");
                            kicad_elements.setStringAt(ke, idx + 1, p1.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p1.Y.ToString());

                            idx = kicad_elements.findIdx(ke, "end");
                            kicad_elements.setStringAt(ke, idx + 1, p2.X.ToString());
                            kicad_elements.setStringAt(ke, idx + 2, p2.Y.ToString());

                            write = true;
                        }
                        break;
                    }
                }

                if (inFile == true && write == true)
                {
                    Boolean parenthese_ouvert = false;
                    file.Write("(");
                    foreach (String str in ke.file_line_param)
                    {

                        if (    str.Equals("start") ||
                                str.Equals("end") ||
                                str.Equals("layer") ||
                                str.Equals("layers") ||
                                str.Equals("at") ||
                                str.Equals("size") ||
                                str.Equals("drill") ||
                                str.Equals("center") ||
                                str.Equals("width")
                            )
                        {
                            if ( parenthese_ouvert == true )
                                file.Write(")");
                            file.Write("(");

                            parenthese_ouvert = true;
                        }

                        file.Write(str.Replace(",", ".") + " ");
                    }

                    if (parenthese_ouvert == true)
                    {
                        file.Write(")");
                    }
                    file.Write(")");
                    file.WriteLine("");
                }
            }
        }

        public void saveFootprint(String fileStr)
        {
            Parse(fileStr);
            if (kicad_elements.count() > 0)
            {
                file = new StreamWriter(fileStr + ".kicad_mod");
                file.WriteLine("(module test (layer F.Cu) (tedit 0)");

                draw(null, true);

                file.WriteLine(")");
                file.Close();
            }
            
        }
    }
}

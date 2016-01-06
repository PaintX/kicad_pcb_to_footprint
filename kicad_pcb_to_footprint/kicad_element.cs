using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace kicad_pcb_to_footprint
{
    public class kicad_element
    {
        public enum kicad_type_element
        {
            KICAD_TYPE_ELEMENT_AREA = 0,
            KICAD_TYPE_ELEMENT_POSITION,
            KICAD_TYPE_ELEMENT_LINE,
            KICAD_TYPE_ELEMENT_GROUND,
            KICAD_TYPE_ELEMENT_PAD,
            KICAD_TYPE_ELEMENT_OVAL,
        };


        public struct coord
        {
            public double x;
            public double y;
        };

        public struct kicad_line
        {
            public coord start;
            public coord end;
        };

        public struct kicad_rectangle
        {
            public coord start;
            public coord end;
        };

        public struct kicad_circle
        {
            public double x;
            public double y;
            public double r;
        };

        public struct kicad_pos
        {
            public double x;
            public double y;
        };

        public struct kicad_elements
        {
            public kicad_element.kicad_type_element type;

            public kicad_line line;
            public kicad_circle circle;
            public kicad_pos pos;
            public kicad_rectangle rect;

            public double angle;

            public Color color;
        };

        List<kicad_elements> parts;

        public void createList()
        {
            // Create a list of parts.
            parts = new List<kicad_elements>();
        }

        public void add(kicad_elements ke)
        {
            parts.Add(ke);
        }

        public int count()
        {
            return parts.Count();
        }

        public kicad_elements get(int idx)
        {
            return parts.ElementAt(idx);
        }

    }
}

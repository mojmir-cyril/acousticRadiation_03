#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System.Linq;
using System.Collections.Generic;
using SVSLoggerF472;


//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using System;

namespace SVSEntityManagerF472
{

    public class SAnnoLegend : SAnnoObject
    {
        private int _x1   { get; set; }
        private int _y1   { get; set; }
        private int _size { get; set; }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Methods:
        //
        // --------------------------------------------------------------------------------------------------------
        public SAnnoLegend(SEntityManager em, int x1 = 15, int y1 = 100, int size = 17) : base(em)
        {
            logger.Msg("SAnnoLegend(...)");
            _x1   = x1;
            _y1   = y1;
            _size = size;
        }
        public class SLabel
        {
            public string text;
            public int color;
            public SLabel(string text, int color)
            {
                this.text  = text;
                this.color = color;
            }
        }
        public void DrawAnnoLegend(List<string> head1            = null,  // main head lines
                                   List<string> head2            = null,  // contour head lines
                                   List<SColors.SColor> colors   = null,  // contours
                                   bool reversed                 = false,
                                   List<string> head3            = null,  // labels head lines
                                   List<SLabel> labels3          = null,  // labels
                                   List<string> head4            = null,  // labels head lines
                                   List<SLabel> labels4          = null,  // labels
                                   List<string> head5            = null,  // labels head lines
                                   List<SLabel> labels5          = null,  // labels
                                   List<string> head6            = null,  // labels head lines
                                   List<SLabel> labels6          = null,  // labels
                                   List<string> head7            = null,  // labels head lines
                                   List<SLabel> labels7          = null,  // labels
                                   int textColor                 = 0)
        {
            logger.Msg("DrawAnnoLegend(...)");
            int x1 = _x1, y1 = _y1, size = _size, dy = size;
            if (head1 != null)
            {
                if (head1.Count() >= 1) logger.Msg(" - head1");
                foreach (string line in head1)
                {
                    __Label(x1, y1 + 4, line, textColor);
                    y1 += (int)(size * 0.9);
                }
                if (head1.Count() >= 1) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
            }
            if (head2 != null)
            {
                if (head2.Count() >= 1) logger.Msg(" - head2");
                if (head2.Count() >= 1) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
                foreach (string line in head2)
                {
                    __Label(x1, y1 + 4, line, textColor);
                    y1 += (int)(size * 0.85);
                }
                if (head2.Count() >= 1) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
            }
            if (colors != null)
            {
                if (colors.Count() >= 1)
                {
                    logger.Msg(" - colors");
                    y1 += reversed ? 0 : size * (colors.Count() - 1);
                    dy = reversed ? size : -size;
                }
                foreach (SColors.SColor color in colors)
                { 
                    (double v1, double v2) = reversed ? (color.minValue, color.maxValue) : (color.maxValue, color.minValue);
                    string s1 = $"{v1:g5}";
                    string s2 = $"{v2:g5}";
                    __Label(x1 + size + size / 3 + 4, y1 + 4, s1, textColor);
                    __Label(x1 + size + size / 3 + 4, y1 + size + 4, s2, textColor);
                    __Rect(x1, y1, x1 + size, y1 + size, color.color);
                    y1 += dy;
                }
                if (colors.Count() >= 1) y1 += (int)(reversed ? (size * 0.8) : (size * (colors.Count() + 1.8)));
                dy = size;
                y1 += size / 2;
            }
            for (int ii = 0; ii < 5; ii ++)
            {
                List<string>      hh = ii == 0 ? head3   : ii == 1 ? head4   : ii == 2 ? head5   : ii == 3 ? head6   : head7;
                List<SLabel> ll = ii == 0 ? labels3 : ii == 1 ? labels4 : ii == 2 ? labels5 : ii == 3 ? labels6 : labels7;
                if (hh != null)
                {
                    if (hh.Count() >= 1) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
                    foreach (string line in hh)
                    {
                        __Label(x1, y1 + 4, line, textColor);
                        y1 += (int)(size * 0.85);
                    }
                    if (hh.Count() >= 1) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
                }
                if (ll != null)
                {
                    foreach (SLabel label in ll)
                    {
                        __Label(x1 + size + size / 3 + 4, (int)(y1 + size * 0.5 + 4), label.text, textColor);
                        __Rect(x1, y1, x1 + size, y1 + size, label.color);
                        y1 += (int)(dy * 1.6);
                    }
                }
                if (hh != null || ll != null) y1 += (int)(size * 0.4); // mezera mezi legendou a zahlavim
            }
        }
        private IPixelPoint __P2D(int x, int y)
        {
            return api.Graphics.CreatePixelPoint(x, y);
        }
        private void __Rect(int x1, int y1, int x2, int y2,
                         int backColor = 0xCCCCCC, int borderColor = 0x000000, 
                         int borderWeight = 1, double trans = 0.0)
        {
            bool start = true;
            List<IPixelPoint> ps = new List<IPixelPoint>();

            for (int y = y1; y <= y2; y++)
            {
                if (start) { ps.Add(__P2D(x1 + 1, y)); ps.Add(__P2D(x2 - 1, y)); }
                else { ps.Add(__P2D(x2 - 1, y)); ps.Add(__P2D(x1 + 1, y)); }
                start = !start;
            }
            IPolyline<IPoint> back = api.Graphics.Scene.Factory2D.CreatePolyline(ps);
            back.Color = backColor;
            back.LineWeight = 1;
            back.Translucency = trans;
            graphicsEntities.Add(back);
            ps.Clear();
            ps.Add(__P2D(x1, y1));
            ps.Add(__P2D(x2, y1));
            ps.Add(__P2D(x2, y2));
            ps.Add(__P2D(x1, y2));
            ps.Add(__P2D(x1, y1));
            IPolyline<IPoint> rect = api.Graphics.Scene.Factory2D.CreatePolyline(ps);
            rect.Color = borderColor;
            rect.LineWeight = borderWeight - 1;
            rect.Translucency = trans;
            graphicsEntities.Add(rect);
        }
        private void __Label(int x1, int y1, string text, int color = 0x000000)
        {
            IText2D label = api.Graphics.Scene.Factory2D.CreateText(__P2D(x1, y1), text);
            label.Color = color;
            graphicsEntities.Add(label);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Contour Settings:
        //
        // -------------------------------------------------------------------------------------------
        public class SContourSettings
        {
            public string   label1      { get; set; }  // first line
            public string   label2      { get; set; }  // second line
            public int      count       { get; set; }
            public double   minValue    { get; set; }
            public double   maxValue    { get; set; }
            public string   unit        { get; set; } 
            public SContourSettings(string label1, string label2, int count, double minValue, double maxValue, string unit)
            {
                this.label1   = label1;
                this.label2   = label2;
                this.count    = count;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.unit     = unit;
            } 
            public void WriteLog(Action<string> Log)
            {
                Log($" - SAnnoLegendContourSettings:");
                Log($"   - label1   : {label1  }");
                Log($"   - label2   : {label2  }");
                Log($"   - count    : {count   }");
                Log($"   - minValue : {minValue}");
                Log($"   - maxValue : {maxValue}");
                Log($"   - unit     : {unit    }");
            }
        }
    }
}

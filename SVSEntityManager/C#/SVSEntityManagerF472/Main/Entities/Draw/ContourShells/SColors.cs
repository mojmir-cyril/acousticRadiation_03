#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
 

namespace SVSEntityManagerF472
{ 
    public class SColors
    {
        public string               label           { get; set; }
        public List<SColor>         bands           { get; set; }
        public SColor               minBand         { get => bands.First(); }
        public SColor               maxBand         { get => bands.Last(); }
        public List<double>         borderValues    { get => bands.Select(b => b.minValue).Append(maxBand.maxValue).ToList(); }
        public class SColor
        {
            public SColors          colorBands  { get; set; }
            public double           minValue    { get; set; }
            public double           maxValue    { get; set; }
            public int              color       { get; set; }
            public int              index       { get; set; } //  => colorBands.GetBandIndex(color); }
            public (int, int, int)  rgb         { get => SColorUtils.FromInt(color); }
            public SColor(SColors colorBands, double minValue, double maxValue, int color, int index)
            {
                this.colorBands = colorBands;
                this.minValue   = minValue;
                this.maxValue   = maxValue;
                this.color      = color; 
                this.index      = index; 
            } 
            internal bool Contain(double val) => val >= minValue && val <= maxValue;
        }
        public SColor GetBand(double value = -1) => bands[GetBandIndex(value)];
        public int GetBandIndex(int color) => bands.IndexOf(bands.Find(b => b.color == color));
        public int GetBandIndex(double value = -1)
        { 
            for (int i = 0; i < bands.Count(); i++)
                if ((value >= bands[i].minValue) && (value <= bands[i].maxValue)) return i;
            //
            //  Out of bands:
            //
            int j = bands.Count() - 1;
            if (value > bands[j].maxValue) return j;
            return 0;
        }
        public int GetColor(double value = -1) => bands[GetBandIndex(value)].color;
        public void CreateBands(SAnnoLegend.SContourSettings set)
        {
            label = set.label1;
            CreateBands(set.count, set.minValue, set.maxValue);
        }
        public void CreateBands(int numberOfColors = 9, double minValue = 0, double maxValue = 100)
        {
            bands = GetColors(numberOfColors, minValue, maxValue, colorBandsObject: this);
        }  
        public static List<SColor> GetColors(int numberOfColors, double min, double max, int over = 0xff00ee, int under = 0xdddddd, SColors colorBandsObject = null)
        {
            int colorvalue;
            List<SColor> colors = new List<SColor>();
            if (min == max) numberOfColors = 1;
            double increment = (max - min) / numberOfColors;
            for (int i = 0; i < numberOfColors; i++)
            {
                double val = (min + i * increment) + increment / 2.0;
                if (numberOfColors == 5)
                { 
                    colorvalue = i == 0 ? 0x0000FF : 
                                 i == 1 ? 0x00FFFF :
                                 i == 2 ? 0x00FF00 :
                                 i == 3 ? 0xFFFF00 :
                                 i == 4 ? 0xFF0000 : 0x000000;
                }
                else if (numberOfColors == 9)
                { 
                    colorvalue = i == 0 ? 0x0000FF : 
                                 i == 1 ? 0x0099FF :
                                 i == 2 ? 0x00FFFF :
                                 i == 3 ? 0x00FF99 :
                                 i == 4 ? 0x00FF00 :
                                 i == 5 ? 0x99FF00 :
                                 i == 6 ? 0xFFFF00 :
                                 i == 7 ? 0xFF9900 :
                                 i == 8 ? 0xFF0000 : 0x000000;
                } 
                else
                {
                    colorvalue = GetColorRedBlue(val, min, max, over, under); 
                }
                SColor color = new SColor(colorBandsObject, min + i * increment, min + (i + 1) * increment, colorvalue, i);
                colors.Add(color);
            }
            return colors;
        }
        public static int GetColorRedBlue(double val, double min, double max, int over = 0xff00ee, int under = 0xdddddd)
        {
            if (min >= max) return 0x0000ff;
            if (val > max) return over;
            if (val < min) return under;
            double e1 = 0.50;
            double e2 = 0.60;
            double s = 1.10;
            double q = (max - min) / 4.0;
            double quarter = min + q * s;
            double middle = (max + min) / 2.0;
            double threequarter = max - q * s;

            int r, g, b;
            r = g = b = 0;

            if (val < quarter)
            {
                r = 0;
                g = (int)(255 * (Math.Pow(val - min, e2) / Math.Pow(quarter - min, e2)));
                b = 255;
            }
            else if ((val < middle) && (val >= quarter))
            {
                r = 0;
                g = 255;
                b = (int)(255 * (Math.Pow(middle - val, e1) / Math.Pow(middle - quarter, e1)));
            }
            else if (val >= threequarter)
            {
                r = 255;
                g = (int)(255 * (Math.Pow(max - val, e2) / Math.Pow(max - threequarter, e2)));
                b = 0;
            }
            else if ((val >= middle) && (val < threequarter))
            {

                r = (int)(255 * (Math.Pow(val - middle, e1) / Math.Pow(threequarter - middle, e1)));
                g = 255;
                b = 0;
            }
            return (r << 16) + (g << 8) + (b << 0);
        }
        public static int GetColorBlackWhite(double val, double min, double max)
        {
            int r = (int)(255 * (val - Math.Min(min, max)) / Math.Abs(max - min));
            return (r << 16) + (r << 8) + (r << 0);
        }
        public class SPalette
        {
            public string name { get; set; }
            public int color { get; set; }

            public SPalette(string name, int color)
            {
                this.name = name;
                this.color = color;
            }
        } 
        public static List<SPalette> GetPalette()
        {
            return new List<SPalette>() { new SPalette("Red" ,                0xFF0000),
                                          new SPalette("Light Blue" ,         0x8888FF),
                                          new SPalette("Light Violet" ,       0xAA88FF),
                                          new SPalette("Dark Blue" ,          0x000080),
                                          new SPalette("Blue" ,               0x0000FF),
                                          //                                       
                                          //  Object Status:                       
                                          //                                       
                                          new SPalette("Petrol" ,             0x00AADD),     // manualColor
                                          new SPalette("Violet" ,             0xEE82EE),     // errorColor
                                          new SPalette("Orange" ,             0xFFA500),     // AvgColor
                                          new SPalette("Dark Yellow" ,        0xDDDD66),     // outsideColor
                                          new SPalette("Light Green" ,        0x00DD00),     // mappedColor
                                          new SPalette("Light Gray" ,         0xEEEEEE),     // notLoadedColor
                                          new SPalette("Medium Gray" ,        0xCCCCCC),     // ignoredColor
                                          new SPalette("Green" ,              0x008000),
                                          //                                       
                                          //  Object Selection:                    
                                          //                                       
                                          new SPalette("Cornflower Blue" ,    0x6495ED),
                                          new SPalette("Pink" ,               0xFFC0CB),
                                          new SPalette("Dark Khaki" ,         0xBDB76B),
                                          new SPalette("Gold" ,               0xFFD700),
                                          new SPalette("Cyan" ,               0x00FFFF),
                                          new SPalette("Light Red" ,          0xff9999),
                                          new SPalette("Olive" ,              0x808000),
                                          new SPalette("Lemon Chiffon" ,      0xFFFACD),
                                          new SPalette("Green Yellow" ,       0xADFF2F),
                                          new SPalette("Dark Salmon" ,        0xE9967A),
                                          new SPalette("Steel Blue" ,         0x4682B4),
                                          new SPalette("Aquamarine" ,         0x7FFFD4),
                                          new SPalette("Thistle" ,            0xD8BFD8),
                                          new SPalette("Burly Wood" ,         0xDEB887),
                                          new SPalette("Green" ,              0x00FF00),
                                          new SPalette("Coral" ,              0xFF7F50),
                                          new SPalette("Yellow" ,             0xFFFF00),
                                          new SPalette("Magenta" ,            0xFF00FF),
                                          new SPalette("Purple" ,             0x800080),
                                          new SPalette("Navy" ,               0x191970),
                                          new SPalette("Gray" ,               0x808080),
                                          new SPalette("Lime" ,               0x00FF00),
                                          new SPalette("Teal" ,               0x008080),
                                          new SPalette("Chocolate" ,          0xD2691E),
                                          new SPalette("DarkOliveGreen" ,     0x556B2F),
                                          new SPalette("DodgerBlue" ,         0x1E90FF),
                                          new SPalette("LightCoral" ,         0xF08080),
                                          new SPalette("DarkGoldenRod" ,      0xB8860B),
                                          new SPalette("LightYellow" ,        0xFFFFE0),
                                          new SPalette("MediumPurple" ,       0x9370DB),
                                          new SPalette("Orchid" ,             0xDA70D6),
                                          new SPalette("SeaGreen" ,           0x2E8B57),
                                          //
                                          //  Black:
                                          //
                                          new SPalette("Black" ,              0x000000)};
        }
        public static List<string> GetPaletteNames()
        {
            return (from c in GetPalette() select c.name).ToList();
        }
        public static int GetPaletteColor(string name)
        {
            foreach (SPalette c in GetPalette()) if (c.name == name) return c.color;
            return -1;
        }
        public static int GetPaletteColor(int index)
        {
            List<SPalette> p = GetPalette();
            if (index >= p.Count()) index -= p.Count();
            return p[index].color;
        }

    }
}

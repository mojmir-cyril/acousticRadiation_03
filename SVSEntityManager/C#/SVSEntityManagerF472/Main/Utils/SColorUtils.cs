#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text; 
using System.Drawing;
using System.Linq;

namespace SVSEntityManagerF472
{
    public static class SColorUtils
    {
        public static int FromRGB(int r, int g, int b) => Color.FromArgb(0, r, g, b).ToArgb(); 
        public static (int r, int g, int b) FromInt(int color) => ((color >> 16) & 0xff, (color >> 8) & 0xff, (color >> 0) & 0xff); 
        public static int MidColor(params int[] colors)
        { 
            List<int> rs = new List<int>();
            List<int> gs = new List<int>();
            List<int> bs = new List<int>();
            foreach (int color in colors)
            {
                (int r1, int g1, int b1) = FromInt(color);
                rs.Add(r1);
                gs.Add(g1);
                bs.Add(b1); 
            }
            return FromRGB((int)rs.Average(), (int)gs.Average(), (int)bs.Average()); 
        }

    }
}

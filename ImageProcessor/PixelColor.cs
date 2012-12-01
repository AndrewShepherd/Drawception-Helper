using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("({Red}, {Green}, {Blue})")]
    public struct ColorRGBA
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }

    [DebuggerDisplay("({Hue}, {Saturation}, {Lightness})")]
    public struct ColorHSL
    {
        public double Hue;
        public double Saturation;
        public double Lightness;

        internal static ColorHSL FromSystemDrawingColor(System.Drawing.Color color)
        {
            ColorHSL rv = new ColorHSL
            {
                Hue = color.GetHue() / 360.0,
                Saturation = color.GetSaturation(),
                Lightness = color.GetBrightness()
            };
            return rv;
        }

        public static ColorHSL FromRGBA(ColorRGBA colorRGBA)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(colorRGBA.Alpha, colorRGBA.Red, colorRGBA.Green, colorRGBA.Red);
            return FromSystemDrawingColor(color);
        }

        internal static ColorHSL FromWindowsMediaColor(System.Windows.Media.Color c)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
            return FromSystemDrawingColor(color);
        }

        // http://stackoverflow.com/a/147426/25216
        private static ColorRGBA FromHSLA(double H, double S, double L, double A)
        {
            double v;
            double r, g, b;
            if (A > 1.0)
                A = 1.0;
                
            r = L;   // default to gray
            g = L;
            b = L;
            v = (L <= 0.5) ? (L * (1.0 + S)) : (L + S - L * S);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = L + L - v;
                sv = (v - m) / v;
                H *= 6.0;
                sextant = (int)H;
                fract = H - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            ColorRGBA rgb = new ColorRGBA();
            rgb.Red = Convert.ToByte(r * 255.0f);
            rgb.Green = Convert.ToByte(g * 255.0f);
            rgb.Blue = Convert.ToByte(b * 255.0f);
            rgb.Alpha = Convert.ToByte(A * 255.0f);
            return rgb;
        }


        internal ColorRGBA ToColorRGBA()
        {
            return FromHSLA(this.Hue, this.Saturation, this.Lightness, 1.0);
        }
    }
    

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{
    struct ImageProcessingJob
    {
        public BitmapSource Image;
        public double Fidelity;
        public List<Color> AllowableColors;
        public ColorRGBA[,] PixelColors;
        public int PixelWidth;
        public int PixelHeight;
        public double DpiX;
        public double DpiY;
        public ColorMatchingAlgorithm ColorMatchingAlgorithm;
    }
}

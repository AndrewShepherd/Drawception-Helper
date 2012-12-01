using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{

    // You can convert this into a bitmap
    struct ImageProcessingResults
    {
        public int PixelWidth;
        public int PixelHeight;
        public double DpiX;
        public double DpiY;
        public ColorRGBA[,] Pixels;
    }

    static class ImageProcessingFunctions
    {

        private static ColorHSL MatchColor(IEnumerable<ColorHSL> allowable, ColorHSL source)
        {
            ColorHSL bestMatch = default(ColorHSL);
            double bestMatchValue = double.MaxValue;
            foreach (var color in allowable)
            {
                double[] diffs = new double[] { color.Hue - source.Hue, color.Lightness - source.Lightness, color.Saturation - source.Saturation };
                double matchValue = diffs.Select(d => d * d).Sum();
                if (matchValue < bestMatchValue)
                {

                    bestMatchValue = matchValue;
                    bestMatch = color;
                }
            }
            return bestMatch;
        }

        public static ColorRGBA MatchColor(IEnumerable<ColorRGBA> allowable, ColorRGBA source)
        {
            ColorRGBA bestMatch = default(ColorRGBA);
            double bestMatchValue = double.MaxValue;
            foreach (var color in allowable)
            {
                double[] diffs = new double[] { color.Red - source.Red, color.Green - source.Green, color.Blue - source.Blue, color.Alpha - source.Alpha };
                double matchValue = diffs.Select(d => d * d).Sum();
                if (matchValue < bestMatchValue)
                {
                    bestMatchValue = matchValue;
                    bestMatch = color;
                }
            }
            return bestMatch;
        }



        private static ColorRGBA MatchColor(IEnumerable<Color> allowable, ColorRGBA source, ColorMatchingAlgorithm colorMatchingAlgorithm)
        {
            if (colorMatchingAlgorithm == ColorMatchingAlgorithm.HSV)
            {
                ColorHSL bestHSL = MatchColor(allowable.Select(c => ColorHSL.FromWindowsMediaColor(c)).ToArray(),
                                              ColorHSL.FromRGBA(source)
                                              );
                return bestHSL.ToColorRGBA();
            }
            else
            {

                ColorRGBA bestRGBA = MatchColor(allowable.Select(c => new ColorRGBA { Alpha = c.A, Blue = c.B, Green = c.G, Red = c.R }).ToArray(),
                                                source);
                return bestRGBA;
            }
        }


        static IEnumerable<Rect> GetPartitions(int width, int height, double proportion)
        {
            double squareSize = 1 / proportion;
            int xLast = 0;
            for (double x = 0.0; x < width; x += squareSize)
            {
                int xLeft = xLast;
                int xRight = (int)Math.Ceiling(x + squareSize);
                int yLast = 0;

                for (double y = 0.0; y < height; y += squareSize)
                {
                    int yTop = yLast;
                    int yBottom = (int)Math.Ceiling(y + squareSize);
                    yield return new Rect(new Point(xLeft, yTop), new Size(xRight - xLeft, yBottom - yTop));
                    yLast = yBottom;

                }
                xLast = xRight;
            }
        }

        static IEnumerable<T> ElementsWithinRectangle<T>(T[,] source, Rect rect)
        {
            for (int x = (int)Math.Floor(rect.Left); x < Math.Min(source.GetLength(0), (int)Math.Ceiling(rect.Right)); ++x)
            {
                for (int y = (int)Math.Floor(rect.Top); y < Math.Min(source.GetLength(1), (int)Math.Ceiling(rect.Bottom)); ++y)
                {
                    yield return source[x, y];
                }
            }
        }


        static ColorRGBA CalculateAverageColor(IEnumerable<ColorRGBA> colors)
        {
            ulong count = 0;
            ulong blue = 0, green = 0, red = 0, alpha = 0;
            foreach (var color in colors)
            {
                blue += color.Blue;
                green += color.Green;
                red += color.Red;
                alpha += color.Alpha;
                ++count;
            }
            if (count == 0)
            {
                return default(ColorRGBA);
            }
            return new ColorRGBA
            {
                Alpha = (byte)(alpha / count),
                Blue = (byte)(blue / count),
                Green = (byte)(green / count),
                Red = (byte)(red / count)
            };
        }


        public static ImageProcessingResults Process(ImageProcessingJob imageProcessingJob)
        {
            // Generate a new pixel array based on the old pixel array
            ColorRGBA[,] originalPixelColours = imageProcessingJob.PixelColors; 
            var dimensionOne = originalPixelColours.GetLength(0);
            var dimensionTwo = originalPixelColours.GetLength(1);

            //PixelColor[,] processedPixels = (PixelColor[,])originalPixelColours.Clone();
            ColorRGBA[,] processedPixels = new ColorRGBA[dimensionOne, dimensionTwo];
            // Work out the rectangles
            var sourcePartitions = GetPartitions(dimensionOne, dimensionTwo, imageProcessingJob.Fidelity).ToArray();
            foreach (var partition in sourcePartitions)
            {
                var averageColor = CalculateAverageColor(ElementsWithinRectangle(originalPixelColours, partition));

                if (imageProcessingJob.AllowableColors.Count > 0)
                {
                    averageColor = MatchColor(imageProcessingJob.AllowableColors, averageColor, imageProcessingJob.ColorMatchingAlgorithm);
                }
                int xMin = (int)Math.Floor(partition.Left);
                int xMax = Math.Min(processedPixels.GetLength(0), (int)Math.Ceiling(partition.Right));
                int yMax = Math.Min(processedPixels.GetLength(1), (int)Math.Ceiling(partition.Bottom));
                int yMin = (int)Math.Floor(partition.Top);
                for (int x = xMin; x < xMax; ++x)
                {
                    for (int y = yMin; y < yMax; ++y)
                    {
                        processedPixels[x, y] = averageColor;
                    }
                }
            }

            int pixelWidth = imageProcessingJob.PixelWidth;
            int pixelHeight = imageProcessingJob.PixelHeight;
            var dpiX = imageProcessingJob.DpiX;
            var dpiY = imageProcessingJob.DpiY;
            return new ImageProcessingResults
            {
                PixelHeight = pixelHeight,
                PixelWidth = pixelWidth,
                DpiX = dpiX,
                DpiY = dpiY,
                Pixels = processedPixels
            };
        }

        internal static BitmapSource CreateBitmap(ImageProcessingResults results)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(results.PixelWidth, results.PixelHeight, results.DpiX, results.DpiY, PixelFormats.Bgra32, null);
            BitmapUtils.PutPixels(writeableBitmap, results.Pixels);
            return writeableBitmap;
        }
    }
}

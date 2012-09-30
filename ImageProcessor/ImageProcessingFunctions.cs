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
        public PixelColor[,] Pixels;
    }

    static class ImageProcessingFunctions
    {


        private static PixelColor MatchColor(IEnumerable<Color> allowable, PixelColor source)
        {
            int bestMatchValue = int.MaxValue;
            Color bestMatch = default(Color);
            foreach (var color in allowable)
            {
                int[] diffs = {
                                 color.R - source.Red,
                                 color.B - source.Blue,
                                 color.G - source.Green
                              };
                int matchValue = diffs.Select(d => d * d).Sum();
                if (matchValue < bestMatchValue)
                {
                    bestMatch = color;
                    bestMatchValue = matchValue;
                }
            }
            return new PixelColor { Alpha = 255, Blue = bestMatch.B, Green = bestMatch.G, Red = bestMatch.R };
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


        static PixelColor CalculateAverageColor(IEnumerable<PixelColor> colors)
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
                return default(PixelColor);
            }
            return new PixelColor
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
            PixelColor[,] originalPixelColours = imageProcessingJob.PixelColors; 
            var dimensionOne = originalPixelColours.GetLength(0);
            var dimensionTwo = originalPixelColours.GetLength(1);

            //PixelColor[,] processedPixels = (PixelColor[,])originalPixelColours.Clone();
            PixelColor[,] processedPixels = new PixelColor[dimensionOne, dimensionTwo];
            // Work out the rectangles
            var sourcePartitions = GetPartitions(dimensionOne, dimensionTwo, imageProcessingJob.Fidelity).ToArray();
            foreach (var partition in sourcePartitions)
            {
                var averageColor = CalculateAverageColor(ElementsWithinRectangle(originalPixelColours, partition));

                if (imageProcessingJob.AllowableColors.Count > 0)
                {
                    averageColor = MatchColor(imageProcessingJob.AllowableColors, averageColor);
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

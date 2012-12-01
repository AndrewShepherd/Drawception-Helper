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
    static class BitmapUtils
    {
        #if UNSAFE
          public unsafe static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
          {
            fixed(PixelColor* buffer = &pixels[0, 0])
              source.CopyPixels(
                new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                (IntPtr)(buffer + offset),
                pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
                stride);
          }
        #else
                public static void CopyPixels(this BitmapSource source, ColorRGBA[,] pixels, int stride, int offset)
                {
                    var height = source.PixelHeight;
                    var width = source.PixelWidth;
                    var pixelBytes = new byte[height * width * 4];
                    source.CopyPixels(pixelBytes, stride, 0);
                    int y0 = offset / width;
                    int x0 = offset - width * y0;
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            pixels[x + x0, y + y0] = new ColorRGBA
                            {
                                Blue = pixelBytes[(y * width + x) * 4 + 0],
                                Green = pixelBytes[(y * width + x) * 4 + 1],
                                Red = pixelBytes[(y * width + x) * 4 + 2],
                                Alpha = pixelBytes[(y * width + x) * 4 + 3],
                            };
                }
        #endif


        public static IEnumerable<ColorRGBA> GetPixels(byte[] pixelByteArray, PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormats.Bgra32)
            {
                for (int i = 0; i < pixelByteArray.Length; i += 4)
                {
                    yield return new ColorRGBA
                    {
                        Blue = pixelByteArray[i],
                        Green = pixelByteArray[i + 1],
                        Red = pixelByteArray[i + 2],
                        Alpha = pixelByteArray[i + 3]
                    };
                }
            }
            else if (pixelFormat == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixelByteArray.Length; i += 4)
                {
                    yield return new ColorRGBA
                    {
                        Blue = pixelByteArray[i],
                        Green = pixelByteArray[i + 1],
                        Red = pixelByteArray[i + 2],
                        Alpha = 255
                    };
                }
            }
            else
            {
                yield break;
            }
        }

        public static ColorRGBA[,] GetPixels(BitmapSource source)
        {

            int height = source.PixelHeight;
            int width = source.PixelWidth;
            ColorRGBA[,] result = new ColorRGBA[width, height];

            int nStride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixelByteArray = new byte[source.PixelHeight * nStride];
            source.CopyPixels(pixelByteArray, nStride, 0);
            ColorRGBA[] pixelColors = GetPixels(pixelByteArray, source.Format).ToArray();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y*width + x;
                    result[x, y] = pixelColors[index];
                }
            }
            //BitmapUtils.CopyPixels(source, result, width * 4, 0);
            return result;
        }


        public static void PutPixels(WriteableBitmap bitmap, ColorRGBA[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            byte[] bytes = new byte[pixels.Length*4];
            for(int y = 0; y < height; ++y)
            {
                for(int x = 0; x < width; x+=1)
                {
                    var index = (y * width + x)*4;
                    bytes[index] = pixels[x, y].Blue;
                    bytes[index+1] = pixels[x,y].Green;
                    bytes[index+2] = pixels[x,y].Red;
                    bytes[index + 3] = pixels[x, y].Alpha;
                }
            }

            Int32Rect rect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            bitmap.WritePixels(rect, bytes, width * 4, 0, 0);
        }
    }
}

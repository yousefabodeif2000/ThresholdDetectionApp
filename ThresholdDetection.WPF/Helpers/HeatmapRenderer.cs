using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThresholdDetection.WPF.Helpers
{
    public static class HeatmapRenderer
    {
        public static byte[] GeneratePixelData(double[,] matrix)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            var pixels = new byte[width * height * 4]; // BGRA format

            double min = double.MaxValue;
            double max = double.MinValue;

            // Find range
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double v = matrix[y, x];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            // Map to colors (heatmap gradient)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double normalized = (matrix[y, x] - min) / (max - min);
                    GetHeatColor(normalized, out byte r, out byte g, out byte b);

                    int idx = (y * width + x) * 4;
                    pixels[idx + 0] = b;
                    pixels[idx + 1] = g;
                    pixels[idx + 2] = r;
                    pixels[idx + 3] = 255;
                }
            }

            return pixels;
        }

        public static WriteableBitmap CreateBitmap(byte[] pixels, int width, int height)
        {
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            bmp.Freeze(); // now it’s safe to assign
            return bmp;
        }

        private static void GetHeatColor(double value, out byte r, out byte g, out byte b)
        {
            value = Math.Clamp(value, 0, 1);
            r = (byte)(value * 255);
            g = (byte)((1 - Math.Abs(value - 0.5) * 2) * 255);
            b = (byte)((1 - value) * 255);
        }
    }

}

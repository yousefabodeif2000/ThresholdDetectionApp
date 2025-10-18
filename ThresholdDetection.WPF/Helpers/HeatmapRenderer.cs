using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThresholdDetection.WPF.Helpers
{
    /// <summary>
    /// Provides helper methods for generating heatmap images from 2D numeric data.
    /// Supports pixel array generation, bitmap creation, resolution scaling, and heatmap color mapping.
    /// </summary>
    public static class HeatmapRenderer
    {

        /// <summary>
        /// Generates a BGRA byte array from a 2D numeric matrix, mapping values to heatmap colors.
        /// </summary>
        /// <param name="matrix">The 2D array of double values.</param>
        /// <returns>A byte array in BGRA format suitable for bitmap creation.</returns>
        /// <remarks>
        /// The minimum and maximum values in the matrix are used to normalize values to [0,1],
        /// which are then mapped to a heatmap gradient using <see cref="GetHeatColor"/>.
        /// </remarks>
        public static byte[] GeneratePixelData(double[,] matrix)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            var pixels = new byte[width * height * 4]; 

            double min = double.MaxValue;
            double max = double.MinValue;


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double v = matrix[y, x];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

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

        /// <summary>
        /// Creates a <see cref="WriteableBitmap"/> from BGRA pixel data.
        /// </summary>
        /// <param name="pixels">The pixel data in BGRA format. Length must be at least <c>width * height * 4</c>.</param>
        /// <param name="width">The width of the bitmap in pixels.</param>
        /// <param name="height">The height of the bitmap in pixels.</param>
        /// <returns>
        /// A frozen <see cref="WriteableBitmap"/> that can be safely used in WPF bindings or UI elements.
        /// </returns>
        /// <remarks>
        /// The method writes the pixel data directly into the bitmap using <see cref="WriteableBitmap.WritePixels"/>,
        /// then freezes the bitmap to make it thread-safe and improve rendering performance.
        /// </remarks>
        public static WriteableBitmap CreateBitmap(byte[] pixels, int width, int height)
        {
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            bmp.Freeze();
            return bmp;
        }

        /// <summary>
        /// Maps a normalized value [0,1] to an RGB heatmap color.
        /// </summary>
        /// <param name="value">Normalized input value between 0 and 1.</param>
        /// <param name="r">Red component of output color.</param>
        /// <param name="g">Green component of output color.</param>
        /// <param name="b">Blue component of output color.</param>
        /// <remarks>
        /// Produces a gradient where 0 → blue, 1 → red, and green peaks in the middle.
        /// </remarks>
        private static void GetHeatColor(double value, out byte r, out byte g, out byte b)
        {
            value = Math.Clamp(value, 0, 1);
            r = (byte)(value * 255);
            g = (byte)((1 - Math.Abs(value - 0.5) * 2) * 255);
            b = (byte)((1 - value) * 255);
        }
    }
}

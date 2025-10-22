using ThresholdDetection.Core.Models;

namespace ThresholdDetection.Core.Services
{
    /// <summary>
    /// Detects contiguous regions in a 2D numeric array that exceed a specified threshold.
    /// Uses an iterative stack-based flood fill with 8-neighbor connectivity.
    /// Returns bounding boxes for all detected regions.
    /// </summary>
    public class Detector
    {
        /// <summary>
        /// Scans the provided 2D data array and returns bounding boxes around regions
        /// where values are strictly greater than the given threshold.
        /// </summary>
        /// <param name="data">The 2D array of numeric values to scan.</param>
        /// <param name="threshold">The threshold value. Only cells with values more than threshold are considered part of a region.</param>
        /// <returns>
        /// An array of <see cref="IBox"/> objects representing the bounding boxes
        /// of detected contiguous regions.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The detection algorithm uses 8-neighbor connectivity (horizontal, vertical, and diagonal neighbors).
        /// NaN values are ignored and treated as outside the threshold region.
        /// </para>
        /// <para>
        /// The method is optimized with a preallocated stack to reduce memory allocations.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        public IBox[] GetBoxes(double[,] data, double threshold)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int height = data.GetLength(0);
            int width = data.GetLength(1);
            var visited = new bool[height, width];
            var boxes = new List<IBox>();

            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            var stack = new Stack<(int y, int x)>(Math.Min(4096, height * width));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x]) continue;
                    double v = data[y, x];
                    if (double.IsNaN(v) || !(v > threshold))
                    {
                        visited[y, x] = true;
                        continue;
                    }

                    int minX = x, maxX = x, minY = y, maxY = y;
                    stack.Clear();
                    stack.Push((y, x));
                    visited[y, x] = true;

                    while (stack.Count > 0)
                    {
                        var (cy, cx) = stack.Pop();

                        if (cx < minX) minX = cx;
                        if (cx > maxX) maxX = cx;
                        if (cy < minY) minY = cy;
                        if (cy > maxY) maxY = cy;

                        for (int k = 0; k < 8; k++)
                        {
                            int ny = cy + dy[k];
                            int nx = cx + dx[k];

                            if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[ny, nx])
                            {
                                double nv = data[ny, nx];
                                if (!double.IsNaN(nv) && nv > threshold)
                                {
                                    visited[ny, nx] = true;
                                    stack.Push((ny, nx));
                                }
                                else
                                {
                                    visited[ny, nx] = true;
                                }
                            }
                        }
                    }

                    boxes.Add(new Box(minX, minY, maxX - minX + 1, maxY - minY + 1));
                }
            }
            //return boxes.ToArray();
            
            var mergedBoxes = new List<IBox>(boxes);
            var toRemove = new HashSet<IBox>();

            foreach (var bottom in boxes)
            {
                int bottomEdge = bottom.YStart + bottom.YLength;
                if (bottomEdge >= height - 1)
                {
                    foreach (var top in boxes)
                    {
                        if (top.YStart == 0 && !toRemove.Contains(top))
                        {
                            bool xOverlap = !(top.XStart > bottom.XStart + bottom.XLength ||
                                              top.XStart + top.XLength < bottom.XStart);
                            if (xOverlap)
                            {
                                int newXStart = Math.Min(bottom.XStart, top.XStart);
                                int newXEnd = Math.Max(bottom.XStart + bottom.XLength, top.XStart + top.XLength);
                                int newYStart = bottom.YStart;
                                int newYLength = bottom.YLength + top.YLength;

                                mergedBoxes.Add(new Box(newXStart, newYStart, newXEnd - newXStart, newYLength));

                                toRemove.Add(top);
                                toRemove.Add(bottom);
                            }
                        }
                    }
                }
            }

            mergedBoxes.RemoveAll(b => toRemove.Contains(b));

            return mergedBoxes.ToArray();
            
        }



    }
}

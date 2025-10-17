using System;
using System.Collections.Generic;

using ThresholdDetection.Core.Models;

namespace ThresholdDetection.Core.Services
{
    /// <summary>
    /// Optimized contiguous region detector using an iterative stack-based flood fill.
    /// Uses 8-neighbor connectivity and returns bounding boxes.
    /// </summary>
    public class Detector
    {
        public IBox[] GetBoxes(double[,] data, double threshold)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            // allow thresholds including zero or negative values now

            int height = data.GetLength(0);
            int width = data.GetLength(1);
            var visited = new bool[height, width];
            var boxes = new List<IBox>();

            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            // Preallocate an array-based stack to avoid many small allocations
            var stack = new Stack<(int y, int x)>(capacity: Math.Min(4096, height * width));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x]) continue;
                    double v = data[y, x];
                    if (double.IsNaN(v) || !(v > threshold))
                    {
                        visited[y, x] = true; // mark as processed so we don't revisit NaNs or below-threshold cells
                        continue;
                    }

                    // start region
                    int minX = x, maxX = x, minY = y, maxY = y;
                    stack.Clear();
                    stack.Push((y, x));
                    visited[y, x] = true;

                    while (stack.Count > 0)
                    {
                        var (cy, cx) = stack.Pop();

                        // update bounds
                        if (cx < minX) minX = cx;
                        if (cx > maxX) maxX = cx;
                        if (cy < minY) minY = cy;
                        if (cy > maxY) maxY = cy;

                        // check neighbors
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
                                    visited[ny, nx] = true; // mark as visited to skip later
                                }
                            }
                        }
                    }

                    boxes.Add(new Box(minX, minY, width: maxX - minX + 1, height: maxY - minY + 1));
                }
            }

            return boxes.ToArray();
        }
    }
}

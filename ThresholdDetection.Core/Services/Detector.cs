using ThresholdDetection.Core.Interfaces;
using ThresholdDetection.Core.Models;

namespace ThresholdDetection.Core.Services
{
    /// <summary>
    /// Provides functionality to detect contiguous regions (clusters) in a 2D matrix where the values exceed a specified threshold.
    /// Each detected region is represented as a bounding box.
    /// </summary>
    public class Detector
    {
        /// <summary>
        /// Scans a 2D data matrix and detects all contiguous regions with values above the specified threshold.
        /// </summary>
        /// <param name="data">
        /// The 2D array of double values to analyze.
        /// Each element represents a numeric intensity or measurement at a grid cell.
        /// </param>
        /// <param name="threshold">
        /// The numeric threshold value above which a cell is considered part of a region.
        /// </param>
        /// <returns>
        /// An array of <see cref="IBox"/> objects, each representing
        /// a detected region as a bounding box (XStart, YStart, XLength, YLength).
        /// </returns>
        public IBox[] GetBoxes(double[,] data, double threshold)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (threshold <= 0)
                throw new ArgumentException("Threshold must be above 0.", nameof(threshold));

            int height = data.GetLength(0);
            int width = data.GetLength(1);
            bool[,] visited = new bool[height, width];
            List<IBox> boxes = new();

            // Directions for 8-neighbor connectivity (N, S, E, W, and diagonals)
            int[] dx = { -1, 0, 1, 0, -1, 1, -1, 1 };
            int[] dy = { 0, -1, 0, 1, -1, -1, 1, 1 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Skip already visited cells or those below threshold
                    if (!visited[y, x] && data[y, x] > threshold)
                    {
                        // Perform BFS to find the contiguous region
                        var (minX, maxX, minY, maxY) = ExploreRegion(data, threshold, visited, y, x, dx, dy);

                        // Store bounding box of detected region
                        boxes.Add(new Box
                        {
                            XStart = minX,
                            YStart = minY,
                            XLength = maxX - minX + 1,
                            YLength = maxY - minY + 1
                        });
                    }
                }
            }

            return boxes.ToArray();
        }

        /// <summary>
        /// Explores a contiguous region starting from a given cell (x, y)
        /// and determines the region's bounding box using Breadth-First Search (BFS).
        /// </summary>
        /// <param name="data">The 2D matrix of numeric values.</param>
        /// <param name="threshold">The threshold for region inclusion.</param>
        /// <param name="visited">A matrix tracking visited cells.</param>
        /// <param name="startY">The Y-coordinate of the starting cell.</param>
        /// <param name="startX">The X-coordinate of the starting cell.</param>
        /// <param name="dx">Array of X-direction offsets (8 directions).</param>
        /// <param name="dy">Array of Y-direction offsets (8 directions).</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>minX</c> – leftmost X coordinate in the region.</description></item>
        /// <item><description><c>maxX</c> – rightmost X coordinate in the region.</description></item>
        /// <item><description><c>minY</c> – topmost Y coordinate in the region.</description></item>
        /// <item><description><c>maxY</c> – bottommost Y coordinate in the region.</description></item>
        /// </list>
        /// </returns>
        private (int minX, int maxX, int minY, int maxY) ExploreRegion(
            double[,] data,
            double threshold,
            bool[,] visited,
            int startY,
            int startX,
            int[] dx,
            int[] dy)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            Queue<(int, int)> queue = new();
            queue.Enqueue((startY, startX));
            visited[startY, startX] = true;

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            while (queue.Count > 0)
            {
                var (y, x) = queue.Dequeue();

                for (int i = 0; i < dx.Length; i++)
                {
                    int ny = y + dy[i];
                    int nx = x + dx[i];

                    if (nx >= 0 && ny >= 0 && nx < width && ny < height &&
                        !visited[ny, nx] && data[ny, nx] > threshold)
                    {
                        visited[ny, nx] = true;
                        queue.Enqueue((ny, nx));

                        // Update region boundaries
                        minX = Math.Min(minX, nx);
                        maxX = Math.Max(maxX, nx);
                        minY = Math.Min(minY, ny);
                        maxY = Math.Max(maxY, ny);
                    }
                }
            }

            return (minX, maxX, minY, maxY);
        }
    }
}

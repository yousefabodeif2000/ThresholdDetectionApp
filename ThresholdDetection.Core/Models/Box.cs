namespace ThresholdDetection.Core.Models
{
    /// <summary>
    /// Implementation of IBox to store bounding box data.
    /// </summary>
    public class Box : IBox
    {
        /// <summary>
        /// Starting X coordinate of the box.
        /// </summary>
        public int XStart { get; }
        /// <summary>
        /// Starting Y coordinate of the box.
        /// </summary>
        public int YStart { get; }
        /// <summary>
        /// Length of the X dimension.
        /// </summary>
        public int XLength { get; }
        /// <summary>
        /// Length of the Y dimension.
        /// </summary>
        public int YLength { get; }

        public Box(int x, int y, int width, int height)
        {
            XStart = x;
            YStart = y;
            XLength = width;
            YLength = height;
        }
    }
}

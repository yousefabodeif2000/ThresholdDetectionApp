namespace ThresholdDetection.Core.Models
{
    /// <summary>
    /// Implementation of IBox to store bounding box data.
    /// </summary>
    public class Box : IBox
    {
        public int XStart { get; }
        public int YStart { get; }
        public int XLength { get; }
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

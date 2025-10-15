using ThresholdDetection.Core.Interfaces;

namespace ThresholdDetection.Core.Models
{
    /// <summary>
    /// Implementation of IBox to store bounding box data.
    /// </summary>
    public class Box : IBox
    {
        /// <summary>
        /// Box x-axis start.
        /// </summary>
        public int XStart { get; set; }
        /// <summary>
        /// Box width (x dimension).
        /// </summary>
        public int XLength { get; set; }
        /// <summary>
        /// Box y-axis start.
        /// </summary>
        public int YStart { get; set; }
        /// <summary>
        /// Box height (y dimension).
        /// </summary>
        public int YLength { get; set; }

        public override string ToString()
        {
            return $"Box: X={XStart}, Y={YStart}, Width={XLength}, Height={YLength}";
        }
    }
}

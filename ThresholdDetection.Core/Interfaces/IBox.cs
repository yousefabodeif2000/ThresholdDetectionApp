using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThresholdDetection.Core.Interfaces
{
    /// <summary>
    /// Represents a bounding box around detected data.
    /// </summary>
    public interface IBox
    {
        int XStart { get; set; }     // X-axis start position
        int XLength { get; set; }    // Width (X dimension)
        int YStart { get; set; }     // Y-axis start position
        int YLength { get; set; }    // Height (Y dimension)
    }

}

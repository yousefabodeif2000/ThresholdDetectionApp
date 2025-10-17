using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThresholdDetection.Core.Models
{
    public interface IBox
    {
        int XStart { get; }
        int YStart { get; }
        int XLength { get; }
        int YLength { get; }
    }
}


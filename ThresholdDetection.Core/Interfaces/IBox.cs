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


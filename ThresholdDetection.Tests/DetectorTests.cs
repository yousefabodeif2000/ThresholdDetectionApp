using Xunit;
using ThresholdDetection.Core.Services;

namespace ThresholdDetection.Tests
{
    public class DetectorTests
    {
        [Fact]
        public void SingleCluster_ShouldReturnOneBox()
        {
            double[,] data = new double[,]
            {
                { 0.1, 0.7, 0.1 },
                { 0.2, 0.8, 0.9 },
                { 0.1, 0.6, 0.0 }
            };


            double threshold = 0.5;
            var detector = new Detector();
            var boxes = detector.GetBoxes(data, threshold);

            Assert.Single(boxes); // only one cluster expected


            var box = boxes[0];
            Assert.Equal(1, box.XStart);
            Assert.Equal(2, box.XLength);
            Assert.Equal(0, box.YStart);
            Assert.Equal(3, box.YLength);
        }

        [Fact]
        public void TwoSeparateClusters_ShouldReturnTwoBoxes()
        {
            double[,] data = new double[,]
            {
                { 0.6, 0.1, 0.0, 0.0 },
                { 0.7, 0.0, 0.0, 0.8 },
                { 0.1, 0.0, 0.0, 0.9 }
            };

            double threshold = 0.5;
            var detector = new Detector();
            var boxes = detector.GetBoxes(data, threshold);

            Assert.Equal(2, boxes.Length);
        }

        [Fact]
        public void AllBelowThreshold_ShouldReturnNoBoxes()
        {
            double[,] data = new double[,]
            {
                { 0.1, 0.2 },
                { 0.3, 0.4 }
            };

            double threshold = 0.5;
            var detector = new Detector();
            var boxes = detector.GetBoxes(data, threshold);

            Assert.Empty(boxes);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using ThresholdDetection.Core.Services;

class Program
{
    static void Main()
    {
        string path = @"D:\Work\WPF Development\ThresholdDetectionApp\data\c-data.csv"; // Update this path
        double[,] data = LoadCsvAsMatrix(path); 

        double threshold = 1.00; // You can change this
        var detector = new Detector();
        var boxes = detector.GetBoxes(data, threshold);

        Console.WriteLine($"Detected {boxes.Length} boxes above threshold {threshold}:");

        foreach (var box in boxes)
        {
            Console.WriteLine($"Box => XStart:{box.XStart}, YStart:{box.YStart}, W:{box.XLength}, H:{box.YLength}");
        }
    }

    static double[,] LoadCsvAsMatrix(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToArray();

        int rows = lines.Length;
        int cols = lines[0].Split(',').Length;

        double[,] matrix = new double[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            var values = lines[y].Split(',')
                                 .Select(v => double.Parse(v.Trim()))
                                 .ToArray();

            for (int x = 0; x < cols; x++)
                matrix[y, x] = values[x];
        }

        return matrix;
    }
}

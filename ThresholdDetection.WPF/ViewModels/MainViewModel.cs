using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ThresholdDetection.Core.Models;
using ThresholdDetection.Core.Services;

namespace ThresholdDetection.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private double _threshold = 0.5;
        public double Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ObservableCollection<double>> _matrixRows = new();
        public ObservableCollection<ObservableCollection<double>> MatrixRows
        {
            get => _matrixRows;
            set { _matrixRows = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Box> _boxes = new();
        public ObservableCollection<Box> Boxes
        {
            get => _boxes;
            set { _boxes = value; OnPropertyChanged(); }
        }

        public ICommand DetectCommand { get; }
        public ICommand LoadCsvCommand { get; }

        private double[,] _data;

        public MainViewModel()
        {
            DetectCommand = new RelayCommand(_ => Detect());
            LoadCsvCommand = new RelayCommand(_ => LoadCsv());
        }

        private void LoadCsv()
        {
            var openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                var lines = File.ReadAllLines(openFileDialog.FileName);
                var rows = lines.Select(line => line.Split(',').Select(double.Parse).ToArray()).ToArray();

                int height = rows.Length;
                int width = rows[0].Length;
                _data = new double[height, width];

                MatrixRows.Clear();

                for (int y = 0; y < height; y++)
                {
                    var row = new ObservableCollection<double>();
                    for (int x = 0; x < width; x++)
                    {
                        _data[y, x] = rows[y][x];
                        row.Add(rows[y][x]);
                    }
                    MatrixRows.Add(row);
                }
            }
        }

        private void Detect()
        {
            if (_data == null)
                return;

            var detector = new Detector();
            var boxes = detector.GetBoxes(_data, Threshold);
            Boxes = new ObservableCollection<Box>(boxes.Cast<Box>());
        }
    }
}

using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ThresholdDetection.Core.Models;
using ThresholdDetection.Core.Services;
using ThresholdDetection.WPF.Views;

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
        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (Math.Abs(_zoomLevel - value) > 0.0001)
                {
                    _zoomLevel = value;
                    OnPropertyChanged();
                }
            }
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
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }

        private double[,] _data;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        private bool _isDataLoaded;
        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set
            {
                _isDataLoaded = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public MainViewModel()
        {
            DetectCommand = new RelayCommand(_ => Detect());
            LoadCsvCommand = new RelayCommand(_ => LoadCsv());
            ZoomInCommand = new RelayCommand(_ => Zoom(1.25));
            ZoomOutCommand = new RelayCommand(_ => Zoom(0.8));
            ResetZoomCommand = new RelayCommand(_ => ResetZoom());
        }

        private async void LoadCsv()
        {
            var openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusMessage = "Loading CSV file...";
                    IsLoading = true;

                    var fileName = openFileDialog.FileName;

                    // Load in background thread
                    var lines = await Task.Run(() => File.ReadAllLines(fileName));
                    var rows = lines
                        .Select(line => line.Split(',')
                        .Select(s => double.TryParse(s, out var val) ? val : double.NaN)
                        .ToArray())
                        .ToArray();

                    int height = rows.Length;
                    int width = rows[0].Length;
                    _data = new double[height, width];

                    MatrixRows.Clear();

                    StatusMessage = "Parsing data...";

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
                    IsDataLoaded = true;
                    StatusMessage = $"Loaded CSV ({_data.GetLength(0)}x{_data.GetLength(1)}).";

                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading CSV: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
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
        private void Zoom(double factor)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var main = (MainWindow)Application.Current.MainWindow;
                main.ZoomTransform.ScaleX *= factor;
                main.ZoomTransform.ScaleY *= factor;
            });
        }

        private void ResetZoom()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var main = (MainWindow)Application.Current.MainWindow;
                main.ZoomTransform.ScaleX = 1;
                main.ZoomTransform.ScaleY = 1;
                main.PanTransform.X = 0;
                main.PanTransform.Y = 0;
            });
        }
    }
}

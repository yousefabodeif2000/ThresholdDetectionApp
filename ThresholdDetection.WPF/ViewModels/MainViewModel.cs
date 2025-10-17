using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ThresholdDetection.Core.Services;
using ThresholdDetection.Core.Models;
using Microsoft.Win32;

namespace ThresholdDetection.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Detector _detector = new();
        private double[,]? _data;
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private double _threshold = 100;
        private double _zoom = 1.0;
        private double _offsetX;
        private double _offsetY;

        public ObservableCollection<Box> Boxes { get; } = new();

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); UpdateCommandStates(); }
        }
        private int _loadProgress;
        public int LoadProgress
        {
            get => _loadProgress;
            set { _loadProgress = value; OnPropertyChanged(nameof(LoadProgress)); }
        }

        public double Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(nameof(Threshold)); }
        }

        public double Zoom
        {
            get => _zoom;
            set { _zoom = value; OnPropertyChanged(nameof(Zoom)); }
        }

        public double OffsetX
        {
            get => _offsetX;
            set { _offsetX = value; OnPropertyChanged(nameof(OffsetX)); }
        }

        public double OffsetY
        {
            get => _offsetY;
            set { _offsetY = value; OnPropertyChanged(nameof(OffsetY)); }
        }
        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        private bool _isDataLoaded;
        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set { _isDataLoaded = value; OnPropertyChanged(nameof(IsDataLoaded)); }
        }

        private ObservableCollection<ObservableCollection<double>>? _matrixRows;

        public ObservableCollection<ObservableCollection<double>>? MatrixRows
        {
            get => _matrixRows;
            set
            {
                _matrixRows = value;
                OnPropertyChanged(nameof(MatrixRows));
            }
        }


        private int _matrixCols;
        public int MatrixCols
        {
            get => _matrixCols;
            set { _matrixCols = value; OnPropertyChanged(nameof(MatrixCols)); }
        }


        // Commands
        public ICommand LoadCsvCommand { get; set; } = new RelayCommand(() => { });
        public ICommand DetectCommand { get; set; } = new RelayCommand(() => { });
        public ICommand FitToScreenCommand { get; set; } = new RelayCommand(() => { });
        public ICommand ZoomInCommand { get; set; } = new RelayCommand(() => { });
        public ICommand ZoomOutCommand { get; set; } = new RelayCommand(() => { });
        public ICommand ResetViewCommand { get; set; } = new RelayCommand(() => { });


        public MainViewModel()
        {
            LoadCsvCommand = new RelayCommand(async () => await LoadCsvAsync(), _ => !IsBusy);
            DetectCommand = new RelayCommand(async () => await DetectAsync(), _ => !IsBusy && _data != null);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            ResetViewCommand = new RelayCommand(ResetView);
            FitToScreenCommand = new RelayCommand(FitToScreen);
        }

        private async Task LoadCsvAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Select Data File"
            };

            if (dialog.ShowDialog() != true)
                return;

            IsBusy = true;
            StatusMessage = "Loading CSV...";
            LoadProgress = 0;
            IsDataLoaded = false;

            try
            {
                var lines = await Task.Run(() => File.ReadAllLines(dialog.FileName));
                int rows = lines.Length;
                int cols = lines[0].Split(',').Length;

                //Heavy lifting off the UI thread
                var (matrix, progress) = await Task.Run(() =>
                {
                    var temp = new double[rows, cols];
                    for (int y = 0; y < rows; y++)
                    {
                        var parts = lines[y].Split(',');
                        for (int x = 0; x < cols; x++)
                            temp[y, x] = double.Parse(parts[x]);
                    }
                    return (temp, 100);
                });

                //Only now, update the observable collections on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var newMatrix = new ObservableCollection<ObservableCollection<double>>();
                    for (int y = 0; y < matrix.GetLength(0); y++)
                    {
                        var row = new ObservableCollection<double>();
                        for (int x = 0; x < matrix.GetLength(1); x++)
                            row.Add(matrix[y, x]);
                        newMatrix.Add(row);
                    }

                    MatrixRows = newMatrix;
                    _data = matrix;
                    MatrixCols = cols;
                    IsDataLoaded = true;
                    StatusMessage = $"Loaded {rows}x{cols} matrix.";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                LoadProgress = 0;
            }
        }





        private async Task DetectAsync()
        {
            if (_data == null)
            {
                MessageBox.Show("No data loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            StatusMessage = "Detecting regions...";

            try
            {
                var boxes = await Task.Run(() => _detector.GetBoxes(_data, _threshold));
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Boxes.Clear();
                    foreach (var b in boxes)
                        Boxes.Add(new Box(b.XStart, b.YStart, b.XLength, b.YLength));
                });

                StatusMessage = $"Detected {Boxes.Count} regions.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ZoomIn() => Zoom *= 1.25;
        private void ZoomOut() => Zoom /= 1.25;

        private void ResetView()
        {
            Zoom = 1.0;
            OffsetX = 0;
            OffsetY = 0;
        }

        private void FitToScreen()
        {
            // Example logic — can be adjusted based on canvas size
            Zoom = 0.75;
            OffsetX = 0;
            OffsetY = 0;
        }

        private void UpdateCommandStates()
        {
            (LoadCsvCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (DetectCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

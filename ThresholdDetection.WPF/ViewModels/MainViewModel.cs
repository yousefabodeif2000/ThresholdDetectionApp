using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ThresholdDetection.Core.Models;
using ThresholdDetection.Core.Services;
using ThresholdDetection.WPF.Helpers;

namespace ThresholdDetection.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Detector _detector = new();
        private double[,]? _data;
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private double _threshold = 0.5;
        private WriteableBitmap? _heatmapImage;
        private bool _isDataLoaded;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set { _isDataLoaded = value; OnPropertyChanged(nameof(IsDataLoaded)); }
        }

        public double Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(nameof(Threshold)); }
        }

        public WriteableBitmap? HeatmapImage
        {
            get => _heatmapImage;
            set { _heatmapImage = value; OnPropertyChanged(nameof(HeatmapImage)); }
        }
        private ObservableCollection<IBox> _boxes = new();
        public ObservableCollection<IBox> Boxes
        {
            get => _boxes;
            set
            {
                _boxes = value;
                OnPropertyChanged(nameof(Boxes));
            }
        }
        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }
        private double _zoomLevel = 1.0;

        public double PanX
        {
            get => _panX;
            set { _panX = value; OnPropertyChanged(nameof(PanX)); }
        }
        private double _panX;

        public double PanY
        {
            get => _panY;
            set { _panY = value; OnPropertyChanged(nameof(PanY)); }
        }
        private double _panY;

        public ICommand ZoomInCommand => new RelayCommand(() => ZoomLevel *= 1.1);
        public ICommand ZoomOutCommand => new RelayCommand(() => ZoomLevel /= 1.1);
        public ICommand ResetViewCommand => new RelayCommand(() =>
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        });

        // Commands
        public ICommand LoadCsvCommand { get; }
        public ICommand DetectCommand { get; }

        public MainViewModel()
        {
            LoadCsvCommand = new RelayCommand(async () => await LoadCsvAsync(), _ => !IsBusy);
            DetectCommand = new RelayCommand(async () => await DetectAsync(), _ => !IsBusy && IsDataLoaded);
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
            //LoadProgress = 0;
            IsDataLoaded = false;

            try
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(dialog.FileName));
                int rows = lines.Length;
                int cols = lines[0].Split(',').Length;

                var matrix = new double[rows, cols];

                await Task.Run(() =>
                {
                    for (int y = 0; y < rows; y++)
                    {
                        var parts = lines[y].Split(',');
                        for (int x = 0; x < cols; x++)
                            matrix[y, x] = double.Parse(parts[x]);

                        //if (y % 100 == 0)
                        //    LoadProgress = (int)((double)y / rows * 100);
                    }
                });

                _data = matrix;
                StatusMessage = "Rendering heatmap...";

                // Generate pixel data off-thread
                var pixelData = await Task.Run(() => HeatmapRenderer.GeneratePixelData(matrix));

                // Create the Bitmap *on the UI thread* safely
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var bmp = HeatmapRenderer.CreateBitmap(pixelData, cols, rows);
                    HeatmapImage = bmp;
                });


                IsDataLoaded = true;
                StatusMessage = $"Loaded {rows}x{cols} matrix.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                //LoadProgress = 0;
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
                Boxes = await Task.Run(() => new ObservableCollection<IBox>(_detector.GetBoxes(_data, _threshold)));
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

        private void UpdateCommandStates()
        {
            (LoadCsvCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (DetectCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ThresholdDetection.Core.Models;
using ThresholdDetection.Core.Services;
using ThresholdDetection.WPF.Helpers;

namespace ThresholdDetection.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the main window of the Threshold Detection application.
    /// Manages CSV data loading, heatmap rendering, region detection, and viewport interactions.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when a property value changes, enabling WPF data binding.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #region STATE PROPERTIES
        /// <summary>
        /// Status message displayed in the UI.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        /// <summary>
        /// Indicates whether a background operation (loading or detection) is in progress.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); UpdateCommandStates(); }
        }

        /// <summary>
        /// Indicates whether data has been successfully loaded.
        /// </summary>
        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set { _isDataLoaded = value; OnPropertyChanged(nameof(IsDataLoaded)); }
        }

        /// <summary>
        /// Threshold value used for detecting regions in the heatmap.
        /// </summary>
        public double Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(nameof(Threshold)); }
        }
        #endregion

        #region CONTAINER PROPERTIES
        /// <summary>
        /// The rendered heatmap as a <see cref="WriteableBitmap"/>.
        /// </summary>
        public WriteableBitmap? HeatmapImage
        {
            get => _heatmapImage;
            set { _heatmapImage = value; OnPropertyChanged(nameof(HeatmapImage)); }
        }

        /// <summary>
        /// Collection of bounding boxes representing detected regions.
        /// </summary>
        public ObservableCollection<IBox> Boxes
        {
            get => _boxes;
            set
            {
                _boxes = value;
                OnPropertyChanged(nameof(Boxes));
            }
        }
        #endregion

        #region UX VIEWPORT PROPERTIES
        /// <summary>
        /// Zoom factor applied to the heatmap display.
        /// </summary>
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                double minZoom = 1.0;
                if (value < minZoom) value = minZoom;

                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    OnPropertyChanged(nameof(ZoomLevel));
                }
            }
        }

        /// <summary>
        /// Pan offset along the X-axis.
        /// </summary>
        public double PanX
        {
            get => _panX;
            set { _panX = value; OnPropertyChanged(nameof(PanX)); }
        }

        /// <summary>
        /// Pan offset along the Y-axis.
        /// </summary>
        public double PanY
        {
            get => _panY;
            set { _panY = value; OnPropertyChanged(nameof(PanY)); }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Command to zoom in the heatmap view.
        /// </summary>
        public ICommand ZoomInCommand => new RelayCommand(() => ZoomLevel *= 1.1);

        /// <summary>
        /// Command to zoom out the heatmap view.
        /// </summary>
        public ICommand ZoomOutCommand => new RelayCommand(() => ZoomLevel /= 1.1);

        /// <summary>
        /// Command to reset zoom and pan to their default values.
        /// </summary>
        public ICommand ResetViewCommand => new RelayCommand(() =>
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        });

        /// <summary>
        /// Command to load a CSV file and generate a heatmap.
        /// </summary>
        public ICommand LoadCsvCommand { get; }

        /// <summary>
        /// Command to detect regions in the loaded data using the current threshold.
        /// </summary>
        public ICommand DetectCommand { get; }
        #endregion

        #region PRIVATE VARIABLES
        private readonly Detector _detector = new();
        private ObservableCollection<IBox> _boxes = new();
        private double[,]? _data;
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private double _threshold = 0.5;
        private WriteableBitmap? _heatmapImage;
        private bool _isDataLoaded;
        private double _zoomLevel = 1.0;
        private double _panX;
        private double _panY;
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="MainViewModel"/> and sets up commands.
        /// </summary>
        public MainViewModel()
        {
            LoadCsvCommand = new RelayCommand(async () => await LoadCsvAsync(), _ => !IsBusy);
            DetectCommand = new RelayCommand(async () => await DetectAsync(), _ => !IsBusy && IsDataLoaded);
        }

        /// <summary>
        /// Asynchronously loads a CSV file, parses the numeric data, and generates a heatmap.
        /// Updates <see cref="IsDataLoaded"/>, <see cref="HeatmapImage"/>, and <see cref="StatusMessage"/>.
        /// </summary>
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

        /// <summary>
        /// Asynchronously detects contiguous regions in the loaded data using <see cref="Detector"/>.
        /// Updates <see cref="Boxes"/> and <see cref="StatusMessage"/>.
        /// </summary>
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

        /// <summary>
        /// Updates the CanExecute state of commands after changes to <see cref="IsBusy"/> or <see cref="IsDataLoaded"/>.
        /// </summary>
        private void UpdateCommandStates()
        {
            (LoadCsvCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (DetectCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThresholdDetection.WPF.ViewModels;

namespace ThresholdDetection.WPF.Views
{
    public partial class MainWindow : Window
    {
        private static readonly Regex _numericRegex = new(@"^\d+([.,]\d+)?$");

        private Point _lastMousePosition;
        private bool _isPanning;

        public MainWindow()
        {
            InitializeComponent();

            // Hook viewmodel ZoomLevel -> ZoomTransform if you have a ViewModel property
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.ZoomLevel))
                    {
                        ZoomTransform.ScaleX = vm.ZoomLevel;
                        ZoomTransform.ScaleY = vm.ZoomLevel;
                    }
                };
            }

            // Mouse handlers (also can be set in XAML)
            VisualizationCanvas.MouseWheel += VisualizationCanvas_MouseWheel;
            VisualizationCanvas.MouseLeftButtonDown += VisualizationCanvas_MouseLeftButtonDown;
            VisualizationCanvas.MouseLeftButtonUp += VisualizationCanvas_MouseLeftButtonUp;
            VisualizationCanvas.MouseMove += VisualizationCanvas_MouseMove;
        }



        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                e.Handled = !_numericRegex.IsMatch(proposed);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow navigation and control keys
            if (e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Tab)
                return;
        }

        private void VisualizationCanvas_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (DataContext is MainViewModel vm && !vm.IsDataLoaded)
                return;

            // Zoom toward mouse position (optional)
            var mousePos = e.GetPosition(VisualizationCanvas);

            double oldScale = ZoomTransform.ScaleX;
            double zoomFactor = e.Delta > 0 ? 1.12 : (1.0 / 1.12);
            double newScale = Math.Clamp(oldScale * zoomFactor, 0.1, 8.0);

            // Adjust PanTransform so zoom is centered around mouse
            // screen -> content translation:
            // newOffset = mousePos - (mousePos - oldOffset) * (newScale/oldScale)
            double oldOffsetX = PanTransform.X;
            double oldOffsetY = PanTransform.Y;

            double scaledMouseX = mousePos.X * (newScale / oldScale);
            double scaledMouseY = mousePos.Y * (newScale / oldScale);

            // compute new pan to keep mouse point stationary
            PanTransform.X = mousePos.X - (mousePos.X - oldOffsetX) * (newScale / oldScale);
            PanTransform.Y = mousePos.Y - (mousePos.Y - oldOffsetY) * (newScale / oldScale);

            ZoomTransform.ScaleX = newScale;
            ZoomTransform.ScaleY = newScale;
        }

        private void VisualizationCanvas_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && !vm.IsDataLoaded) return;

            _isPanning = true;
            _lastMousePosition = e.GetPosition(this);
            VisualizationCanvas.CaptureMouse();

            if (DataContext is MainViewModel v) v.StatusMessage = "Panning...";
        }

        private void VisualizationCanvas_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (!_isPanning) return;
            _isPanning = false;
            VisualizationCanvas.ReleaseMouseCapture();

            if (DataContext is MainViewModel v) v.StatusMessage = "Ready";
        }

        private void VisualizationCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isPanning) return;

            var current = e.GetPosition(this);
            var delta = current - _lastMousePosition;
            PanTransform.X += delta.X;
            PanTransform.Y += delta.Y;
            _lastMousePosition = current;
        }

        // Optional: Fit to screen button handler — scale to fit content
        private void OnFitToScreenClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm || !vm.IsDataLoaded) return;

            int rows = vm.MatrixRows?.Count ?? 0;
            int cols = (rows > 0) ? vm.MatrixRows[0].Count : 0;
            if (rows == 0 || cols == 0) return;

            const double cellSize = 20.0; // must match ScaleConverter or Border sizes
            double contentWidth = cols * cellSize;
            double contentHeight = rows * cellSize;

            // get window client area
            double availableW = VisualizationCanvas.ActualWidth;
            double availableH = VisualizationCanvas.ActualHeight;

            // if actual sizes are zero (not measured), use window size minus margins
            if (availableW <= 0) availableW = this.ActualWidth - 100;
            if (availableH <= 0) availableH = this.ActualHeight - 150;

            double scaleX = availableW / contentWidth;
            double scaleY = availableH / contentHeight;
            double scale = Math.Min(scaleX, scaleY);
            if (scale > 1.0) scale = 1.0;

            ZoomTransform.ScaleX = scale;
            ZoomTransform.ScaleY = scale;

            // center
            PanTransform.X = (availableW - contentWidth * scale) / 2;
            PanTransform.Y = (availableH - contentHeight * scale) / 2;
        }
    }
}

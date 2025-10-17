using System.Windows;
using System.Windows.Input;

namespace ThresholdDetection.WPF.Views
{
    public partial class MainWindow : Window
    {
        private Point _lastMousePosition;
        private bool _isPanning;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                if (e.Delta > 0)
                    vm.ZoomInCommand.Execute(null);
                else
                    vm.ZoomOutCommand.Execute(null);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastMousePosition = e.GetPosition(this);
                Mouse.Capture((IInputElement)sender);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                Mouse.Capture(null);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && DataContext is ViewModels.MainViewModel vm)
            {
                Point pos = e.GetPosition(this);
                vm.PanX += (pos.X - _lastMousePosition.X) / vm.ZoomLevel;
                vm.PanY += (pos.Y - _lastMousePosition.Y) / vm.ZoomLevel;
                _lastMousePosition = pos;
            }
        }

    }
}

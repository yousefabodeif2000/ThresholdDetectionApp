using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThresholdDetection.WPF.Views
{
    public partial class MainWindow : Window
    {
        private Point _lastMousePos;
        private bool _isPanning;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 1.1 : 1 / 1.1;
            ZoomTransform.ScaleX *= zoom;
            ZoomTransform.ScaleY *= zoom;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastMousePos = e.GetPosition(this);
            PanCanvas.CaptureMouse();
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            PanCanvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point pos = e.GetPosition(this);
                PanTransform.X += pos.X - _lastMousePos.X;
                PanTransform.Y += pos.Y - _lastMousePos.Y;
                _lastMousePos = pos;
            }
        }
    }
}

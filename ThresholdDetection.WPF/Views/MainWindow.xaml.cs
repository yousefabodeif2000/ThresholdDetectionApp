using System.Windows;
using System.Windows.Input;

namespace ThresholdDetection.WPF.Views
{
    /// <summary>
    /// Interaction logic for the main window of the application.
    /// Handles mouse input for zooming and panning the heatmap view.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Last recorded mouse position for panning calculations.
        /// </summary>
        private Point _lastMousePosition;

        /// <summary>
        /// Indicates whether the user is currently performing a pan operation.
        /// </summary>
        private bool _isPanning;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles mouse wheel events to zoom in or out of the heatmap.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing event data.</param>
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

        /// <summary>
        /// Handles the mouse down event to initiate panning.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing event data.</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastMousePosition = e.GetPosition(this);
                Mouse.Capture((IInputElement)sender);
            }
        }

        /// <summary>
        /// Handles the mouse up event to stop panning.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing event data.</param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                Mouse.Capture(null);
            }
        }

        /// <summary>
        /// Handles the mouse move event to pan the heatmap when the left button is pressed.
        /// Adjusts the <see cref="ViewModels.MainViewModel.PanX"/> and <see cref="ViewModels.MainViewModel.PanY"/> properties.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing event data.</param>
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

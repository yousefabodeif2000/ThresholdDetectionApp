using System;
using System.Windows.Input;

namespace ThresholdDetection.WPF.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = _ => execute();
        }

        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = _ => execute();
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Manually forces WPF to requery CanExecute state.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

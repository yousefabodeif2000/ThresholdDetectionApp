using System.Windows.Input;

namespace ThresholdDetection.WPF.ViewModels
{
    /// <summary>
    /// A general-purpose implementation of <see cref="ICommand"/> that allows
    /// binding UI actions to delegates in the ViewModel.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new <see cref="RelayCommand"/> that executes a parameterless action.
        /// Can always execute.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = _ => execute();
        }

        /// <summary>
        /// Initializes a new <see cref="RelayCommand"/> that executes a parameterless action
        /// and optionally defines a predicate to determine if it can execute.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">Optional predicate to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = _ => execute();
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new <see cref="RelayCommand"/> that executes an action
        /// with an optional parameter and optionally defines a predicate to determine if it can execute.
        /// </summary>
        /// <param name="execute">The action to execute, taking an optional parameter.</param>
        /// <param name="canExecute">Optional predicate to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command action.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// Subscribes to WPF's <see cref="CommandManager.RequerySuggested"/> automatically.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Manually forces WPF to re-evaluate the <see cref="CanExecute"/> state for this command.
        /// Useful if conditions change dynamically outside of UI events.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

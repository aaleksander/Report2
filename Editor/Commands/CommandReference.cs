using System;
using System.Windows;
using System.Windows.Input;

namespace Commands
{
    /// <summary>
    /// This class facilitates associating a key binding in XAML markup to a command
    /// defined in a View Model by exposing a Command dependency property.
    /// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
    /// </summary>
    public class CommandReference : Freezable, ICommand
    {
    	/// <summary>
    	/// Команда
    	/// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandReference), new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

		/// <summary>
    	/// конструктор
    	/// </summary>
        public CommandReference()
        {
            // Blank
        }

		/// <summary>
		/// событие "CanExecute" изменилось
		/// </summary>
        public event EventHandler CanExecuteChanged;   	
        
        /// <summary>
        /// Команда
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Может ли данная команда выполнится
        /// </summary>
        /// <param name="parameter">некий входной параметр</param>
        /// <returns>Истина - если команда может выполняться</returns>
        public bool CanExecute(object parameter)
        {
            if (this.Command != null)
            {
                return this.Command.CanExecute(parameter);
            }
            
            return false;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="parameter">Параметр для команды</param>
        public void Execute(object parameter)
        {
            this.Command.Execute(parameter);
        }

        #region Freezable

        /// <summary>
        /// пока не готово
        /// </summary>
        /// <returns>Выкинет исключение</returns>
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        #endregion
        
        /// <summary>
        /// свойство команды изменилось
        /// </summary>
        /// <param name="d">ссылка на этот объект</param>
        /// <param name="e">аргументы</param>
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandReference commandReference = d as CommandReference;
            ICommand oldCommand = e.OldValue as ICommand;
            ICommand newCommand = e.NewValue as ICommand;

            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }
            
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }
    }
}

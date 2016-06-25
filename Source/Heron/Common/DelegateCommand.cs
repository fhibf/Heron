using System;
using System.Windows.Input;

namespace Heron.Common {
    public class DelegateCommand : ICommand {

        // References:
        // http://relentlessdevelopment.wordpress.com/2010/03/30/simplified-mvvm-commanding-with-delegatecommand/

        private Action _executeMethod;

        public DelegateCommand(Action executeMethod) {
            this._executeMethod = executeMethod;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            if (this._executeMethod != null)
                this._executeMethod.Invoke();
        }

    }

    public class DelegateCommand<T> : ICommand {

        private Action<T> _executeMethod;
        
        public DelegateCommand(Action<T> executeMethod) {
            this._executeMethod = executeMethod;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            if (this._executeMethod != null)
                this._executeMethod.Invoke((T)parameter);
        }

    }
}

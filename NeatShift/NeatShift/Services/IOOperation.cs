using System;
using System.Threading;

namespace NeatShift.Services
{
    public class IOOperation
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isCompleted;
        private bool _isCancelled;

        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
        public event EventHandler? Completed;
        public event EventHandler? Cancelled;

        public bool IsCompleted
        {
            get => _isCompleted;
            private set
            {
                _isCompleted = value;
                if (value)
                {
                    OnCompleted();
                }
            }
        }

        public bool IsCancelled
        {
            get => _isCancelled;
            private set
            {
                _isCancelled = value;
                if (value)
                {
                    OnCancelled();
                }
            }
        }

        public IOOperation()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected virtual void OnProgressChanged(int progressPercentage, string message)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs 
            { 
                ProgressPercentage = progressPercentage, 
                Message = message 
            });
        }

        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCancelled()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            IsCancelled = true;
        }

        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource.Token;
        }
    }
} 
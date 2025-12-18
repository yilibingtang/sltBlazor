using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YX.Services
{
    public record NotificationMessage(Guid Id, string Message, bool IsError);

    public class NotificationService
    {
        public event Action? OnChange;

        readonly List<NotificationMessage> _messages = new();

        public IReadOnlyList<NotificationMessage> Messages => _messages.AsReadOnly();

        public void Show(string message, bool isError = false, int timeoutMs = 3500)
        {
            var id = Guid.NewGuid();
            var nm = new NotificationMessage(id, message, isError);
            _messages.Add(nm);
            NotifyStateChanged();

            // remove after timeout
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(timeoutMs);
                    Remove(id);
                }
                catch { }
            });
        }

        public void Remove(Guid id)
        {
            var existing = _messages.FirstOrDefault(m => m.Id == id);
            if (existing != null)
            {
                _messages.Remove(existing);
                NotifyStateChanged();
            }
        }

        void NotifyStateChanged() => OnChange?.Invoke();
    }
}

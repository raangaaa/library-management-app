using Avalonia.Controls;
using Avalonia.Controls.Notifications;

public class NotificationService
{
    private readonly WindowNotificationManager _notificationManager;

    public NotificationService(Window window)
    {
        _notificationManager = new WindowNotificationManager(window)
        {
            Position = NotificationPosition.BottomRight
        };
    }

    public void Show(string title, string message)
    {
        _notificationManager.Show(new Notification(title, message));
    }
}

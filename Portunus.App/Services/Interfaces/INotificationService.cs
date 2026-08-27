
namespace Portunus.App.Services.Interfaces
{
    public interface INotificationService
    {
        void Success(string title, string message);
        void Error(string title, string message);
        void ShowNative(string title, string body, string iconPath)
;    }
}

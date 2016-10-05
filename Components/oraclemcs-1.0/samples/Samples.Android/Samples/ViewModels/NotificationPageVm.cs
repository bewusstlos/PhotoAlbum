using System.Threading.Tasks;
using System.Windows.Input;
using Oracle.Cloud.Mobile.Notifications;
using Xamarin.Forms;

namespace Samples.ViewModels
{
    public class NotificationPageVm : ViewModelBase
    {
        private readonly Notifications _notificationService;

        public NotificationPageVm()
        {
            _notificationService = App.Backend.GetService<Notifications>();
        }

        public async Task Register()
        {
            //This would normally happen at startup  
            await _notificationService.RegisterForNotificationsAsync(NotificationsListener.DeviceToken);
        }

        public async Task Deregister()
        {
            await _notificationService.DeregisterForNotificationsAsync(NotificationsListener.DeviceToken);
        }

    }
}
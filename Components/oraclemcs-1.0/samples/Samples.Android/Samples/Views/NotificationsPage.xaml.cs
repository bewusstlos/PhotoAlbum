using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.Cloud.Mobile.Notifications;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class NotificationsPage : ContentPage
    {
        private NotificationPageVm _viewModel;

        public NotificationsPage()
        {
            InitializeComponent();
	        _viewModel = new NotificationPageVm();
	        this.BindingContext = _viewModel;
        }

        public async void Register(object sender, EventArgs e)
        {
            await _viewModel.Register();
            await DisplayAlert("Register", "Registered for notification", "OK");
        }

        public async void Deregister(object sender, EventArgs e)
        {
            await _viewModel.Deregister();
            await DisplayAlert("De-Register", "No longer registered for notification", "OK");
        }


    }
}

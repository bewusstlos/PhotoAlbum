using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class MenuPage : ContentPage
    {
        private readonly MenuPageVm _viewModel;

		public MenuPage()
        {
            InitializeComponent();
            _viewModel = new MenuPageVm();
            BindingContext = _viewModel;
        }

        public async void NavigateToStorage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StoragePage());
        }

        public async void NavigateToNotifications(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificationsPage());
        }

        public async void NavigateToUserDetails(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserDetailsPage());
        }

        public async void NavigateToCustomApis(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CustomApiPage());
        }

        private async void NavigateAnalytics(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnalyticsPage());
        }

        public async void Logout(object sender, EventArgs e)
        {
            await _viewModel.Logout();
            await Navigation.PopAsync();
        }
        
    }
}
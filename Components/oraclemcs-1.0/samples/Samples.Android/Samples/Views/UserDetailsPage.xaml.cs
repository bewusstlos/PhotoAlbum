using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.Cloud.Mobile.MobileBackend;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class UserDetailsPage : ContentPage
    {
        private UserDetailsPageVm _viewModel;

        public UserDetailsPage()
        {
            InitializeComponent();
            _viewModel = new UserDetailsPageVm();
            this.BindingContext = _viewModel;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadUserDetails();
        }

    }
}

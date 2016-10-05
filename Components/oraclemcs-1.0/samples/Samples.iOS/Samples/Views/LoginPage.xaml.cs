using System.Collections.Generic;
using System.Diagnostics;
using Oracle.Cloud.Mobile.Analytics;
using Oracle.Cloud.Mobile.Authorization;
using Oracle.Cloud.Mobile.Configuration;
using Oracle.Cloud.Mobile.MobileBackend;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class LoginPage : ContentPage
    {
        private LoginPageVm _viewModel;

        public LoginPage()
        {
            InitializeComponent();

            _viewModel = new LoginPageVm();
            this.BindingContext = _viewModel;
        }

        public async void Login(object sender, EventArgs e)
        {
            var success = await _viewModel.Login();
            if (success)
                await Navigation.PushAsync(new MenuPage());
        }
       
    }
}
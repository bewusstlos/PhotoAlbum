using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.Cloud.Mobile.Analytics;
using Oracle.Cloud.Mobile.Authorization;
using Oracle.Cloud.Mobile.Configuration;
using Oracle.Cloud.Mobile.MobileBackend;
using PushNotification.Plugin;
using Samples.Views;
using Xamarin.Forms;

namespace Samples
{
    public partial class App : Application
    {
        private Analytics _analytics;

        public App()
        {
            InitializeComponent();
            // The root page of your application
            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
            _analytics = Backend.GetService<Analytics>();
            _analytics.StartSession();

            CrossPushNotification.Current.Register();
        }

        public static MobileBackend Backend { get { return MobileBackendManager.Manager.DefaultMobileBackend; } }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            _analytics.EndSession();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            _analytics.StartSession();
        }
    }
}

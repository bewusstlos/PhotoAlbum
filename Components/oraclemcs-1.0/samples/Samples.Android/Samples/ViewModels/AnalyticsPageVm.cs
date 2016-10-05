using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oracle.Cloud.Mobile.Analytics;

namespace Samples.ViewModels
{
    class AnalyticsPageVm : ViewModelBase
    {
        private string _eventName;

        public string EventName
        {
            get { return _eventName; }
            set { _eventName = value; NotifyPropertyChanged(); }
        }

        public async Task SubmitEvent()
        {
            var analytics = App.Backend.GetService<Analytics>();
            analytics.LogEvent(EventName);
            await analytics.FlushAsync();
            EventName = string.Empty;
        }
    }
}

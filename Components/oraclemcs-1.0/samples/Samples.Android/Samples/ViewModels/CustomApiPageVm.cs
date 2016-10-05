using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Oracle.Cloud.Mobile.Storage;

namespace Samples.ViewModels
{
    public class CustomApiPageVm : ViewModelBase
    {
        private string _outputMessage;

        public string OutputMessage
        {
            get { return _outputMessage; }
            set { _outputMessage = value; NotifyPropertyChanged();}
        }

        public async Task TestCustomApi()
        {
            using (var client = App.Backend.CreateHttpClient())
            {

                var response =
                    await client.GetAsync(App.Backend.CustomCodeUri + "/customapitest/test");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                OutputMessage = jObject.Value<string>("sample");

            }
        }
    }
}

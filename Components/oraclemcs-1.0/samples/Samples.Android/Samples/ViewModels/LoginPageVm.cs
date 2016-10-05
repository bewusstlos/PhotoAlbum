using System;
using System.Threading.Tasks;
using Oracle.Cloud.Mobile.Authorization;

namespace Samples.ViewModels
{
    public class LoginPageVm : ViewModelBase
    {
        private string _outputMessage;
        private string _password;
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyPropertyChanged();
            }
        }

        public string OutputMessage
        {
            get { return _outputMessage; }
            set
            {
                _outputMessage = value;
                NotifyPropertyChanged();
            }
        }

        public async Task<bool> Login()
        {
            try
            {
                bool success = await App.Backend.Authorization.AuthenticateAsync(UserName, Password);
                if (success)
                {
                    OutputMessage = string.Empty;
                    Password = string.Empty;
                    UserName = string.Empty;
                    return true;
                }

                OutputMessage = "The username or password was incorrect";
            }
            catch (Exception ex)
            {
                OutputMessage = string.Format("An error occured during the request: {0}", ex.Message);
            }
            Password = string.Empty;

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oracle.Cloud.Mobile.Authorization;

namespace Samples.ViewModels
{
    public class UserDetailsPageVm : ViewModelBase
    {
        private string _userName;
        private string _firstName;
        private string _lastName;
        private string _email;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; NotifyPropertyChanged(); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; NotifyPropertyChanged(); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; NotifyPropertyChanged(); }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; NotifyPropertyChanged(); }
        }

        public UserDetailsPageVm()
        {
            
        }

        public async Task LoadUserDetails()
        {
            var user = await App.Backend.Authorization.GetCurrentUserAsync();
            PopulateFromUser(user);
        }

        private void PopulateFromUser(User user)
        {
            UserName = user.UserName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.EMail;
        }
    }
}

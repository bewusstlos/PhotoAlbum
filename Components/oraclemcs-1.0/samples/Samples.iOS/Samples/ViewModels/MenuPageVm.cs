using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Samples.ViewModels
{
    public class MenuPageVm : ViewModelBase
    {
        public MenuPageVm()
        {
        }

        public async Task Logout()
        {
            await App.Backend.Authorization.LogoutAsync();
            
        }

    }
}

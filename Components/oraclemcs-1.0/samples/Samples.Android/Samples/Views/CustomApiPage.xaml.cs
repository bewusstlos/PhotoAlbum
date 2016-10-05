using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class CustomApiPage : ContentPage
    {
        private readonly CustomApiPageVm _viewModel;

        public CustomApiPage()
        {
            InitializeComponent();
            _viewModel = new CustomApiPageVm();
            this.BindingContext = _viewModel;
        }

        public async void TestCustomApi(object sender, EventArgs e)
        {
            await _viewModel.TestCustomApi();
        }
    }
}

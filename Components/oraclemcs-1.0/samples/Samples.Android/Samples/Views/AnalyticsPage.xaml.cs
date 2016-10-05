using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
	public partial class AnalyticsPage : ContentPage
	{
	    private AnalyticsPageVm _viewModel;

        public AnalyticsPage ()
		{
            InitializeComponent();
            _viewModel = new AnalyticsPageVm();
            this.BindingContext = _viewModel;
		}

        public async void SubmitEvent(object sender, EventArgs e)
        {
            await _viewModel.SubmitEvent();
            await DisplayAlert("Analytics Event", "The custom analytics event was submitted", "OK");
        }
	}
}

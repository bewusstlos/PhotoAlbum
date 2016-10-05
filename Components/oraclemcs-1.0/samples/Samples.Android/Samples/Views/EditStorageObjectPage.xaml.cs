using System;
using System.Collections.Generic;
using Oracle.Cloud.Mobile.Storage;
using Samples.ViewModels;
using Samples.Views;
using Xamarin.Forms;

namespace Samples
{
    public partial class EditStorageObjectPage : ContentPage
	{
        private readonly EditStorageObjectPageVm _viewModel;

	    public EditStorageObjectPage(StorageCollection storageCollection, string id = null)
	    {
	        InitializeComponent();
            _viewModel = new EditStorageObjectPageVm(storageCollection, id);

	        BindingContext = _viewModel;
	    }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadStorageObject();

        }

        public async void SaveStorageObject(object sender, EventArgs e)
        {
            await _viewModel.SaveStorageObject();
            await Navigation.PopAsync();
        }


	}
}


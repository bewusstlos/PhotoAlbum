using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.Cloud.Mobile.Storage;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.Views
{
    public partial class StoragePage : ContentPage
	{
        private readonly StoragePageVm _storagePageVm;

	    public StoragePage()
		{
	        InitializeComponent();

	        _storagePageVm = new StoragePageVm();
	        this.BindingContext = _storagePageVm;

		}

	    protected async override void OnAppearing()
	    {
	        base.OnAppearing();
            await _storagePageVm.PopulateCollectionAsync();
	    }

	    public async void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e == null) return;
			((ListView)sender).SelectedItem = null;
	        var storageObject = e.Item as StorageObject;
	        if (storageObject == null)
	            return;
            await Navigation.PushAsync(new EditStorageObjectPage(_storagePageVm.Collection, storageObject.Id));
		}

        public void OnCancel(object sender, EventArgs e)
        {
        }

        public async void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var answer = await DisplayAlert("Delete Storage Object", "Are you sure you want to delete this item?", "OK", "Cancel");
            if (answer)
            {
                var storageObject = (StorageObject) mi.BindingContext;
                await _storagePageVm.Delete(storageObject);
            }
        }

        public async void AddStorageObject(object sender, EventArgs e)
		{
			await Navigation.PushAsync (new EditStorageObjectPage(_storagePageVm.Collection));
		}
		
		
	}
}

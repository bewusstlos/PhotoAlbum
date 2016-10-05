using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Oracle.Cloud.Mobile.Storage;

namespace Samples.ViewModels
{
    public class StoragePageVm : ViewModelBase
    {
        private readonly Storage _storageService;

        public StoragePageVm()
        {
            _storageService = App.Backend.GetService<Storage>();
            StorageObjects = new ObservableCollection<StorageObject>();
        }

        public ObservableCollection<StorageObject> StorageObjects { get; set; }
        public StorageCollection Collection { get; set; }

        public async Task PopulateCollectionAsync()
        {
            Collection = await _storageService.GetCollectionAsync("XamarinFiles");
            //need logic here to keep trying if we have more items?
            StorageObjects.Clear();
            var items = await Collection.GetObjectsAsync(0, 100);
            foreach (var storageObject in items)
            {
                StorageObjects.Add(storageObject);
            }
        }

        public async Task Delete(StorageObject storageObject)
        {
            await Collection.DeleteObjectAsync(storageObject.Id);
            StorageObjects.Remove(storageObject);
        }
    }
}
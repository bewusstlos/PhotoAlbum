using System.Threading.Tasks;
using System.Windows.Input;
using Oracle.Cloud.Mobile.Storage;
using Samples.ViewModels;
using Xamarin.Forms;

namespace Samples.ViewModels
{
    public class EditStorageObjectPageVm : ViewModelBase
    {
        private readonly StorageCollection _storageCollection;
        private readonly string _id;
        private StorageObject _storageObject;
        private string _name;
        private string _content;

        public EditStorageObjectPageVm(StorageCollection storageCollection, string id)
        {
            _storageCollection = storageCollection;
            _id = id;
        }

        public async Task LoadStorageObject()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _storageObject = new StorageObject(_storageCollection);
            }
            else
            {
                _storageObject = await _storageCollection.GetObjectAsync(_id);
                Name = _storageObject.Name;
                Content = await _storageObject.ReadAsStringAsync();
            }
        }

        public async Task SaveStorageObject()
        {
            _storageObject.Name = Name;
            _storageObject.LoadPayload(Content, "text/plain");
            if (string.IsNullOrEmpty(_id))
            {
                await _storageCollection.PostObjectAsync(_storageObject);
            }
            else
            {
                await _storageCollection.PutObjectAsync(_storageObject);    
            }
            
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; NotifyPropertyChanged(); }
        }

    }
}
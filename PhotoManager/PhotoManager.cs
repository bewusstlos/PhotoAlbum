using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Attributes;
using Android.Content;
using Android.Widget;

namespace PhotoManager
{
    public class PhotosManager
    {
        public SQLiteConnection db;
        public SQLite.Net.Interop.ISQLitePlatform platform;
        public Context c;
        public string path;

        public PhotosManager(SQLite.Net.Interop.ISQLitePlatform platform, string dbPath, Context c, bool firstStart = false)
        {
            this.platform = platform;
            this.path = dbPath;

            db = new SQLiteConnection(platform, dbPath);
            if (firstStart)
            {
                db.CreateTable<Label>();
                db.Insert(new Label { LabelName = "Unsigned" });
                db.CreateTable<Photo>();
            }
        }

        public string GetPhotoPath(int id)
        {
                var query = from r in db.Table<Photo>()
                            where r.Id == id
                            select r.Path;
                return query.SingleOrDefault();
        }

        public List<Photo> GetPhotosToList()
        {
                return db.Table<Photo>().ToList<Photo>();
        }

        public void ChangeLabel(int id, string newLabelName)
        {
                var query = from r in db.Table<Label>()
                            where r.Id == id
                            select r;
                db.Update(new Label { Id = query.SingleOrDefault().Id, LabelName = newLabelName });
                Toast.MakeText(c,"Label succsessfully renamed",ToastLength.Short).Show();
        }

        public List<Photo> GetPhotosOfLabelToList(Label l)
        {
                var query = from r in db.Table<Photo>()
                            where r.LabelId == l.Id
                            select r;
                return query.ToList<Photo>();
        }

        public List<Label> GetLabelsToList()
        {
                return db.Table<Label>().ToList<Label>();
        }

        public void AddLabel(string labelName)
        {
                db.Insert(new Label { LabelName = labelName });
        }

        public void DeleteLabelWithout(int labelId)
        {
                var defaultLabel = from r in db.Table<Label>()
                                   where r.LabelName == "Unsigned"
                                   select r.Id;

                var photoQuery = from p in db.Table<Photo>()
                                 where p.LabelId == labelId
                                 select new Photo { Id = p.Id, LabelId = defaultLabel.SingleOrDefault(), Path = p.Path };

                foreach (var items in photoQuery)
                {
                    db.InsertOrReplace(items, typeof(Photo));
                }
                db.Delete<Label>(labelId);
        }

        public void AddPhoto(string photoPath)
        {
                var query = from r in db.Table<Label>()
                            where r.LabelName == "Unsigned"
                            select r.Id;
                db.Insert(new Photo { Path = photoPath, LabelId = query.SingleOrDefault() }, typeof(Photo));           
        }

        public void DeletePhoto(int id)
        {
                db.Delete<Photo>(id); 
        }

        public void ChangePhotoLabel(int photoId, int labelId)
        {
                var query = from r in db.Table<Photo>()
                            where r.Id == photoId
                            select r;
            try
            {
                db.Update(new Photo { Id = photoId, LabelId = labelId, Path = query.SingleOrDefault().Path });
            }
            catch
            {

            }   
        }
    }

    [Table("Labels")]
    public class Label
    {
        [Column("Id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("Name"), Unique, NotNull]
        public string LabelName { get; set; }

        public Label() { }
    }
    [Table("Photos")]
    public class Photo
    {
        [Column("Id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("Label Id")]
        public int LabelId { get; set; }
        [Column("Path")]
        public string Path { get; set; }

        public Photo() { }
    }
}

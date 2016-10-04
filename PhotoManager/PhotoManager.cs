using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Attributes;

namespace PhotoManager
{
    public class PhotosManager
    {
        public SQLiteConnection db;
        public SQLite.Net.Interop.ISQLitePlatform platform;
        public string path;

        public PhotosManager(SQLite.Net.Interop.ISQLitePlatform platform, string dbPath, bool firstStart = false)
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

        public List<Photo> GetPhotosToList()
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                return db.Table<Photo>().ToList<Photo>();
            }
        }

        public void ChangeLabel(int id, string newLabelName)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                var query = from r in db.Table<Label>()
                            where r.Id == id
                            select r;
                db.Update(new Label { Id = query.Single().Id, LabelName = newLabelName });
            }
        }

        public List<Photo> GetPhotosOfLabelToList(Label l)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                var query = from r in db.Table<Photo>()
                            where r.LabelId == l.Id
                            select r;
                return query.ToList<Photo>();
            }
        }

        public List<Label> GetLabelsToList()
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                return db.Table<Label>().ToList<Label>();
            }
        }

        public void AddLabel(string labelName)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                db.Insert(new Label { LabelName = labelName });
            }
        }

        public void DeleteLabelWithout(int labelId)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {

                var photoQuery = from p in db.Table<Photo>()
                                 where p.LabelId == labelId
                                 select new Photo { Id = p.Id, LabelId = 0, Path = p.Path };

                foreach (var items in photoQuery)
                {
                    db.Update(items);
                }
            }
        }

        public void AddPhoto(string photoPath)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {

                var query = from r in db.Table<Label>()
                            where r.LabelName == "Unsigned"
                            select r.Id;

                db.Insert(new Photo { Path = photoPath, LabelId = query.Single() }, typeof(Photo));
            }

        }

        public void DeletePhoto(int id)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                db.Delete<Photo>(id);
            }
        }

        public void ChangePhotoLabel(int photoId, string labelName)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
            {
                var labelQuery = from l in db.Table<Label>()
                                 where l.LabelName == labelName
                                 select l.Id;

                var photo = from p in db.Table<Photo>()
                            where p.Id == photoId
                            select p;

                if (labelQuery.Count<int>() == 0)
                    db.Insert(new Label { LabelName = labelName });
                db.Update(new Photo { Id = photo.Single<Photo>().Id, LabelId = db.Table<Label>().Last<Label>().Id, Path = photo.Single<Photo>().Path });
            }
        }

        public void RenameLabel(int labelId, string newLabelName)
        {
            using (var db = new SQLiteConnection(this.platform, this.path))
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedisServer
{
    public class RedisManager
    {
        public static Redis redis = new Redis("192.168.228.2", 6379);        
        public List<Photo>  p;
        public List<Label> l;
        
        public RedisManager()
        {
            p = new List<Photo>();
            l = new List<Label>();
            try
            {
                DeserializeLabels();
                DeserializePhotos();
            }
            catch (OverflowException)
            {

            }

            var checkForDefLabel = from r in l
                                   where r.LabelName == "Unsigned"
                                   select r;

            if(checkForDefLabel.Count() == 0)
                l.Add(new Label { Id = 0, LabelName = "Unsigned" });
        }

        public List<Photo> GetPhotosToList()
        {
            return p;
        }

        public List<Label> GetLabelsToList()
        {
            return l;
        }

        public void AddLabel(string labelName)
        {
            l.Add(new Label { Id = l.Count, LabelName = labelName });
        }

        public void ChangePhotoLabel(int photoId, int labelid)
        {
            var findPhoto = from r in p
                            where r.Id == photoId
                            select r;

            var findByteArray = from r in p
                            where r.Id == photoId
                            select r.ByteArray;
            byte[] temp = findByteArray.SingleOrDefault();
            p.Remove(findPhoto.SingleOrDefault());
            p.Add(new Photo { Id = photoId, LabelId = labelid, ByteArray = temp });
        }

        public void RenameLabel(int labelId, string labelName)
        {
            var query = from r in l
                        where r.Id == labelId
                        select r;
            query.SingleOrDefault().LabelName = labelName;
        }

        public void DeleteLabel(int labelId)
        {
            var query = from r in p
                        where r.LabelId == labelId
                        select r;
            foreach(var r in query)
            {
                r.LabelId = 0;
            }
            l.RemoveAt(labelId);
        }

        public void DeletePhoto(int id)
        {
            var findPhoto = from r in p
                            where r.Id == id
                            select r;
            p.Remove(findPhoto.SingleOrDefault());
        }

        public void AddPhoto(byte [] byteArray)
        {
            p.Add(new Photo { Id = p.Count, LabelId = 0, ByteArray = byteArray });
        }

        public void ChangeLabel(int id, string labelName)
        {
            var query = from r in l
                        where r.Id == id
                        select r;
            l.Remove(query.SingleOrDefault());
            l.Add(new Label { Id = id, LabelName = labelName });
        }

        public List<Photo> GetPhotoForSomeLabel(Label l)
        {
            List<Photo> pEach = new List<Photo>();
            var query = from r in p
                        where r.LabelId == l.Id
                        select r;
            foreach(var r in query)
            {
                pEach.Add(r);
            }

            return pEach;
        }

        public void SerializePhotos()
        {
            string jsonPhotos = JsonConvert.SerializeObject(p);
            redis.Set("photos", jsonPhotos);

        }

        public void SerializeLabels()
        {
            string jsonLabels = JsonConvert.SerializeObject(l);
            redis.Set("labels", jsonLabels);
        }

        public List<Photo> DeserializePhotos()
        {
            string jsonPhotos = redis.GetString("photos");
            p = JsonConvert.DeserializeObject<List<Photo>>(jsonPhotos);
            return p;
        }

        public List<Label> DeserializeLabels()
        {
            string jsonLabels = redis.GetString("labels");
            l = JsonConvert.DeserializeObject<List<Label>>(jsonLabels);
            return l;
        }

            public class Photo
            {
                public int Id { get; set; }
                public int LabelId { get; set; }
                public byte[] ByteArray { get; set; }
            }
            public class Label
            {
                public int Id { get; set; }
                public string LabelName { get; set; }
            }
    }
}

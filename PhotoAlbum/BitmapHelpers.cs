using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.IO;

namespace PhotoAlbum
{
    public static class BitmapHelpers
    {
        public static byte [] ImageToByteArray(Bitmap b)
        {
            MemoryStream stream = new MemoryStream();
            b.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);
            return stream.ToArray();
        }

        public static Bitmap LoadAndResizeBitmap(this byte[] byteArray, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true, OutHeight = height, OutWidth=width };

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeByteArray(byteArray, 0, byteArray.Length, options);

            return resizedBitmap;
        }
    }
}
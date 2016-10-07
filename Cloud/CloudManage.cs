using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Net.Wifi;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhotoManager;

namespace Cloud
{
    public class CloudManage
    {

        public CloudManage(Context c)
        {
            WifiManager wm = (WifiManager)c.GetSystemService(Service.WifiService);
            string host = wm.ConnectionInfo.IpAddress.ToString();
            Redis r = new Redis(host);
            r.Set("image", "byteArrImage");
            RedisSub rs = new RedisSub(r.Host, r.Port);

            rs.Subscribe("image");
        }
    }
}
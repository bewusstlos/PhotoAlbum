using PushNotification.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    public class NotificationsListener : IPushNotificationListener
    {
        public static string DeviceToken
        {
            get; private set;
        }

        public void OnError(string message, PushNotification.Plugin.Abstractions.DeviceType deviceType)
        {
            Console.WriteLine("A notifications error occured: {0} ", message);
        }

        public void OnMessage(IDictionary<string, object> parameters, PushNotification.Plugin.Abstractions.DeviceType deviceType)
        {
            //handle push notifications here
        }

        public void OnRegistered(string token, PushNotification.Plugin.Abstractions.DeviceType deviceType)
        {
            DeviceToken = token;
        }

        public void OnUnregistered(PushNotification.Plugin.Abstractions.DeviceType deviceType)
        {

        }
    }
}

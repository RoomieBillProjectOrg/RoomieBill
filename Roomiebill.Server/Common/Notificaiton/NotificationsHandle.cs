using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace Roomiebill.Server.Common.Notificaiton
{
    public class NotificationsHandle
    {
        public NotificationsHandle() { }

        public void SendNotificationByTopicAsync(string title, string body, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic) || topic.Length > 900 || !System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z0-9_-]+$"))
                {
                    throw new ArgumentException("Invalid topic name.");
                }

            var message = new Message()
            {
                Topic = topic,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            // Send a message to the device corresponding to the provided
            // registration token.
            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);
        }
    }
}
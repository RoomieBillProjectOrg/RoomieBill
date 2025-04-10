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

        public static void SendNotificationByTopicAsync(string title, string body, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic) || topic.Length > 900 || !System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z0-9_-]+$"))
                {
                    throw new ArgumentException("Invalid topic name.");
                }

            var message = new Message()
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                // Send a message to the device corresponding to the provided
                // registration token.
                string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending notification: " + ex.Message);
            }
            
        }

        public static void SendNotificationByTokenAsync(string title, string body, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 900)
            {
                throw new ArgumentException("Invalid token.");
            }

            var message = new Message()
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                // Send a message to the device corresponding to the provided
                // registration token.
                if (!token.Equals("Test"))
                {
                    string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;                
                } 
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending notification: " + ex.Message);
            }
        }
    }
}
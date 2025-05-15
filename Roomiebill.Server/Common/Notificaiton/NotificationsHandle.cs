using FirebaseAdmin.Messaging;
using System.Text.RegularExpressions;

namespace Roomiebill.Server.Common.Notificaiton
{
    /// <summary>
    /// Handles Firebase Cloud Messaging notifications for both topic-based and token-based delivery.
    /// </summary>
    public class NotificationsHandle
    {
        private const int MAX_LENGTH = 900;
        private const string TOPIC_PATTERN = @"^[a-zA-Z0-9_-]+$";
        private const string TEST_TOKEN = "Test";
        private const string TEST_TOPIC = "test-topic";

        /// <summary>
        /// Initializes a new instance of the NotificationsHandle class.
        /// </summary>
        public NotificationsHandle() { }

        /// <summary>
        /// Sends a notification to all devices subscribed to a specific topic.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="body">The notification message body.</param>
        /// <param name="topic">The topic to send the notification to. Must be alphanumeric with optional underscores and hyphens.</param>
        /// <exception cref="ArgumentException">When topic is invalid or exceeds maximum length.</exception>
        /// <exception cref="Exception">When Firebase messaging fails.</exception>
        public static void SendNotificationByTopicAsync(string title, string body, string topic)
        {
            // Validate topic format
            if (string.IsNullOrWhiteSpace(topic) || 
                topic.Length > MAX_LENGTH || 
                !Regex.IsMatch(topic, TOPIC_PATTERN))
            {
                throw new ArgumentException("Invalid topic name. Topic must be non-empty, less than 900 characters, and contain only letters, numbers, underscores, and hyphens.");
            }

            var message = new Message
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                // Skip actual sending for test topic
                if (!topic.Equals(TEST_TOPIC))
                {
                    string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending notification to topic '{topic}': {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a notification to a specific device using its FCM token.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="body">The notification message body.</param>
        /// <param name="token">The Firebase Cloud Messaging registration token for the target device.</param>
        /// <exception cref="ArgumentException">When token is invalid or exceeds maximum length.</exception>
        /// <exception cref="Exception">When Firebase messaging fails.</exception>
        public static void SendNotificationByTokenAsync(string title, string body, string token)
        {
            // Validate token
            if (string.IsNullOrWhiteSpace(token) || token.Length > MAX_LENGTH)
            {
                throw new ArgumentException("Invalid token. Token must be non-empty and less than 900 characters.");
            }

            var message = new Message
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                // Skip actual sending for test token
                if (!token.Equals(TEST_TOKEN))
                {
                    string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending notification to token '{token}': {ex.Message}");
            }
        }
    }
}

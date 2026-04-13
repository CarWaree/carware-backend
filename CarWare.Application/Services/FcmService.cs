using CarWare.Application.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;

namespace CarWare.Application.Services
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _log;

        public FcmService(ILogger<FcmService> log) => _log = log;

        /// Send a push notification to a single device token.
        public async Task<string> SendAsync(string token, string title, string body,
            Dictionary<string, string>? data = null)
        {
            var message = new Message
            {
                Token = token,
                Notification = new FirebaseNotification { Title = title, Body = body },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default", Badge = 1 }
                }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _log.LogInformation("FCM single send succeeded. MessageId={MessageId}", messageId);
            return messageId;
        }

        /// Send to multiple device tokens at once.
        /// FCM allows up to 500 tokens per multicast call.
        /// This method batches automatically if the list exceeds 500.
        public async Task SendMulticastAsync(List<string> tokens, string title, string body,
            Dictionary<string, string>? data = null)
        {
            const int batchSize = 500;
            var batches = tokens
                .Select((t, i) => new { Token = t, Index = i })
                .GroupBy(x => x.Index / batchSize)
                .Select(g => g.Select(x => x.Token).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                var msg = new MulticastMessage
                {
                    Tokens = batch,
                    Notification = new FirebaseNotification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>()
                };

                var result = await FirebaseMessaging.DefaultInstance
                    .SendEachForMulticastAsync(msg);

                _log.LogInformation(
                    "FCM multicast batch: {Success} sent, {Fail} failed out of {Total}",
                    result.SuccessCount, result.FailureCount, batch.Count);

                // Log individual failures for token cleanup
                for (int i = 0; i < result.Responses.Count; i++)
                {
                    var resp = result.Responses[i];
                    if (!resp.IsSuccess)
                        _log.LogWarning(
                            "FCM token failed: {Token} | Error: {Error}",
                            batch[i], resp.Exception?.Message);
                }
            }
        }

        ///Broadcast to all subscribers of a topic (e.g. "all-users")
        public async Task SendToTopicAsync(string topic, string title, string body)
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new FirebaseNotification { Title = title, Body = body }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _log.LogInformation(
                "FCM topic broadcast to '{Topic}' succeeded. MessageId={MessageId}",
                topic, messageId);
        }
    }
}
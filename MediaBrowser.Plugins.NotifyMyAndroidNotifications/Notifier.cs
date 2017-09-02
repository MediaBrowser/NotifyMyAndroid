﻿using System.Collections.Generic;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.NotifyMyAndroidNotifications.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.NotifyMyAndroidNotifications
{
    public class Notifier : INotificationService
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        public Notifier(ILogManager logManager, IHttpClient httpClient)
        {
            _logger = logManager.GetLogger(GetType().Name);
            _httpClient = httpClient;
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && IsValid(options) && options.Enabled;
        }

        private NotifyMyAndroidOptions GetOptions(User user)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.MediaBrowserUserId, user.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));
        }

        public string Name
        {
            get { return Plugin.Instance.Name; }
        }

        public Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            var parameters = new Dictionary<string, string>
            {
                {"apikey", options.Token},
                {"application", "Emby"}
            };

            if (string.IsNullOrEmpty(request.Description))
            {
                parameters.Add("event", request.Name);
                parameters.Add("description", "-");
            }
            else
            {
                parameters.Add("event", request.Name);
                parameters.Add("description", request.Description);
            }

            _logger.Debug("NotifyMyAndroid to {0} - {1} - {2}", options.Token, request.Name, request.Description);

            return _httpClient.Post("https://www.notifymyandroid.com/publicapi/notify", parameters, cancellationToken);
        }

        private bool IsValid(NotifyMyAndroidOptions options)
        {
            return !string.IsNullOrEmpty(options.Token);
        }
    }
}

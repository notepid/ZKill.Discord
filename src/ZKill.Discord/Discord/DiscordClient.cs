using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using ZKill.Discord.Logging;

namespace ZKill.Discord.Discord
{
    public class DiscordClient
    {
        private readonly string _webhookUrl;
        private readonly ILogger _logger;

        public DiscordClient(string webhookUrl, ILogger logger)
        {
            _webhookUrl = webhookUrl;
            _logger = logger;
        }

        public void SendTextMessage(string message)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("content", message)
            });

            using (var client = new HttpClient { BaseAddress = new Uri(_webhookUrl) })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsync("", formContent).Result;
                response.EnsureSuccessStatusCode();
            }
        }
    }
}

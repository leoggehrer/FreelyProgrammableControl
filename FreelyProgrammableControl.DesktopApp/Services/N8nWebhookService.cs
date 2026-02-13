using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FreelyProgrammableControl.DesktopApp.Services
{
    /// <summary>
    /// Service zum Aufruf von n8n Webhooks.
    /// </summary>
    public class N8nWebhookService
    {
        private readonly string _saveToGoogleDriveWebhookUrl;

        public N8nWebhookService()
        {
            var settings = ConfigurationHelper.GetSettings();

            _saveToGoogleDriveWebhookUrl = settings.N8n.SaveToGoogleDriveWebhookUrl ?? string.Empty;
        }

        public async Task SaveToGoogleDriveAsync(string filename, string fpcSource)
        {
            if (string.IsNullOrWhiteSpace(_saveToGoogleDriveWebhookUrl))
            {
                throw new InvalidOperationException("Die n8n Webhook-URL ist nicht konfiguriert.");
            }

            var payload = JsonSerializer.Serialize(new
            {
                fileName = filename,
                fpcSource = fpcSource
            });

            using var httpClient = new HttpClient();
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(_saveToGoogleDriveWebhookUrl, content);

            response.EnsureSuccessStatusCode();
        }
    }
}

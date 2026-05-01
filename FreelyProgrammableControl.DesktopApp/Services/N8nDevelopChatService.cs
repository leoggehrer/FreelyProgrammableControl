using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FreelyProgrammableControl.DesktopApp.Services
{
    /// <summary>
    /// Service zum Aufruf des n8n Workflows <c>FPCDevelopPipeline</c>.
    /// Sendet Chat-Nachrichten an den Webhook und liefert die Antwort des Orchestrator-Agents zurück.
    /// </summary>
    public class N8nDevelopChatService
    {
        private readonly string _webhookUrl;
        private readonly TimeSpan _timeout;

        public N8nDevelopChatService(TimeSpan? timeout = null)
        {
            var settings = ConfigurationHelper.GetSettings();
            _webhookUrl = settings.N8N.DevelopPipelineWebhookUrl ?? string.Empty;

            if (timeout.HasValue)
            {
                _timeout = timeout.Value;
            }
            else
            {
                var configuredSeconds = settings.N8N.DevelopPipelineTimeoutSeconds;
                _timeout = configuredSeconds > 0
                    ? TimeSpan.FromSeconds(configuredSeconds)
                    : TimeSpan.FromMinutes(30);
            }
        }

        /// <summary>
        /// Schickt eine Chat-Nachricht an die FPCDevelopPipeline und gibt die Antwort des Orchestrators zurück.
        /// </summary>
        /// <param name="chatInput">Der eigentliche User-Prompt.</param>
        /// <param name="sessionId">Eindeutige Session-ID, damit n8n die Konversation der Pipeline zuordnen kann.</param>
        public async Task<string> SendAsync(string chatInput, string sessionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                throw new InvalidOperationException("Die DevelopPipeline Webhook-URL ist nicht konfiguriert (N8N:DevelopPipelineWebhookUrl).");
            }

            var payload = JsonSerializer.Serialize(new
            {
                chatInput,
                sessionId
            });

            using var httpClient = new HttpClient { Timeout = _timeout };
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(_webhookUrl, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExtractResponseText(responseText);
        }

        /// <summary>
        /// Extrahiert die textliche Antwort aus der n8n-Antwort.
        /// Akzeptiert <c>{output: "..."}</c>, <c>{response: {output: "..."}}</c>, einfache Strings sowie Arrays.
        /// </summary>
        private static string ExtractResponseText(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return string.Empty;
            }

            try
            {
                using var document = JsonDocument.Parse(responseText);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    return ExtractFromElement(root[0]) ?? responseText;
                }

                return ExtractFromElement(root) ?? responseText;
            }
            catch (JsonException)
            {
                return responseText;
            }
        }

        private static string? ExtractFromElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var key in new[] { "output", "text", "response", "message", "answer", "data" })
            {
                if (element.TryGetProperty(key, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        return prop.GetString();
                    }

                    if (prop.ValueKind == JsonValueKind.Object)
                    {
                        var nested = ExtractFromElement(prop);
                        if (nested != null)
                        {
                            return nested;
                        }
                    }
                }
            }

            return element.GetRawText();
        }
    }
}

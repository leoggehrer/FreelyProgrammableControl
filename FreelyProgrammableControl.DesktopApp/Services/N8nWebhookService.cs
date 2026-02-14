using System;
using System.Collections.Generic;
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
        private readonly string _getFpcSampleListWebhookUrl;
        private readonly string _loadFpcSampleWebhookUrl;

        public N8nWebhookService()
        {
            var settings = ConfigurationHelper.GetSettings();

            _saveToGoogleDriveWebhookUrl = settings.N8n.SaveToGoogleDriveWebhookUrl ?? string.Empty;
            _getFpcSampleListWebhookUrl = settings.N8n.GetFpcSampleListWebhookUrl ?? string.Empty;
            _loadFpcSampleWebhookUrl = settings.N8n.LoadFpcSampleWebhookUrl ?? string.Empty;
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

        public async Task<IReadOnlyList<FpcSampleListItem>> GetFpcSampleListAsync()
        {
            if (string.IsNullOrWhiteSpace(_getFpcSampleListWebhookUrl))
            {
                throw new InvalidOperationException("Die n8n Webhook-URL für die Dateiliste ist nicht konfiguriert.");
            }

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(_getFpcSampleListWebhookUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            var items = new List<FpcSampleListItem>();
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in root.EnumerateArray())
                {
                    AddListItem(entry, items);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entry in itemsElement.EnumerateArray())
                    {
                        AddListItem(entry, items);
                    }
                }
                else
                {
                    AddListItem(root, items);
                }
            }

            return items;
        }

        public async Task<FpcSampleLoadResult> LoadFpcSampleAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Die Datei-ID darf nicht leer sein.", nameof(id));
            }

            using var httpClient = new HttpClient();

            if (!string.IsNullOrWhiteSpace(_loadFpcSampleWebhookUrl))
            {
                try
                {
                    var payload = JsonSerializer.Serialize(new
                    {
                        id
                    });

                    using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    using var response = await httpClient.PostAsync(_loadFpcSampleWebhookUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        return new FpcSampleLoadResult(ExtractFpcSource(responseText), FpcSampleLoadSource.PrimaryWebhook);
                    }
                }
                catch (HttpRequestException)
                {
                }
            }

            if (!string.IsNullOrWhiteSpace(_getFpcSampleListWebhookUrl))
            {
                var fallbackUrls = new[]
                {
                    BuildUrlWithQueryParameter(_getFpcSampleListWebhookUrl, "id", id),
                    BuildUrlWithQueryParameter(_getFpcSampleListWebhookUrl, "fileId", id)
                };

                foreach (var fallbackUrl in fallbackUrls)
                {
                    try
                    {
                        using var response = await httpClient.GetAsync(fallbackUrl);
                        if (!response.IsSuccessStatusCode)
                        {
                            continue;
                        }

                        var responseText = await response.Content.ReadAsStringAsync();
                        return new FpcSampleLoadResult(ExtractFpcSource(responseText), FpcSampleLoadSource.FallbackListById);
                    }
                    catch (HttpRequestException)
                    {
                    }
                }
            }

            throw new InvalidOperationException("Datei konnte über keinen konfigurierten n8n-Endpunkt geladen werden.");
        }

        private static string BuildUrlWithQueryParameter(string baseUrl, string key, string value)
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
        }

        private static string ExtractFpcSource(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return string.Empty;
            }

            var candidateText = responseText;

            try
            {
                using var document = JsonDocument.Parse(responseText);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetStringProperty(root, "fpcSource", out var fpcSource))
                    {
                        candidateText = fpcSource;
                        return ExtractFpcCodeBlock(candidateText);
                    }

                    if (TryGetStringProperty(root, "source", out var source))
                    {
                        candidateText = source;
                        return ExtractFpcCodeBlock(candidateText);
                    }

                    if (TryGetStringProperty(root, "content", out var contentValue))
                    {
                        candidateText = contentValue;
                        return ExtractFpcCodeBlock(candidateText);
                    }

                    if (TryGetStringProperty(root, "data", out var dataValue))
                    {
                        candidateText = dataValue;
                        return ExtractFpcCodeBlock(candidateText);
                    }
                }

                if (root.ValueKind == JsonValueKind.String)
                {
                    candidateText = root.GetString() ?? string.Empty;
                    return ExtractFpcCodeBlock(candidateText);
                }
            }
            catch (JsonException)
            {
                return ExtractFpcCodeBlock(responseText);
            }

            return ExtractFpcCodeBlock(candidateText);
        }

        private static string ExtractFpcCodeBlock(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            const string openingFence = "```fpc";
            const string closingFence = "```";

            var openingIndex = content.IndexOf(openingFence, StringComparison.OrdinalIgnoreCase);
            if (openingIndex < 0)
            {
                return content;
            }

            var codeStart = openingIndex + openingFence.Length;
            while (codeStart < content.Length && (content[codeStart] == '\r' || content[codeStart] == '\n'))
            {
                codeStart++;
            }

            var closingIndex = content.IndexOf(closingFence, codeStart, StringComparison.Ordinal);
            if (closingIndex < 0)
            {
                return content;
            }

            return content[codeStart..closingIndex].TrimEnd('\r', '\n');
        }

        private static void AddListItem(JsonElement element, ICollection<FpcSampleListItem> items)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (!TryGetStringProperty(element, "id", out var id) || string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var name = TryGetStringProperty(element, "name", out var foundName) && !string.IsNullOrWhiteSpace(foundName)
                ? foundName
                : id;

            items.Add(new FpcSampleListItem(id, name));
        }

        private static bool TryGetStringProperty(JsonElement element, string propertyName, out string value)
        {
            value = string.Empty;

            if (!element.TryGetProperty(propertyName, out var propertyValue))
            {
                return false;
            }

            switch (propertyValue.ValueKind)
            {
                case JsonValueKind.String:
                    value = propertyValue.GetString() ?? string.Empty;
                    return true;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    value = propertyValue.ToString();
                    return true;
                default:
                    return false;
            }
        }
    }

    public record FpcSampleListItem(string Id, string Name)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public enum FpcSampleLoadSource
    {
        PrimaryWebhook,
        FallbackListById
    }

    public record FpcSampleLoadResult(string Source, FpcSampleLoadSource LoadSource);
}

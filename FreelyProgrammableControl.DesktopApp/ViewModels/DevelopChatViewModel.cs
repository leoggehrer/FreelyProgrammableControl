using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.DesktopApp.Services;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// ViewModel für das Develop-Chat-Fenster. Sendet Nachrichten an den n8n
    /// Workflow <c>FPCDevelopPipeline</c> und stellt die Antworten dar.
    /// </summary>
    public partial class DevelopChatViewModel : ViewModelBase
    {
        private readonly N8nDevelopChatService chatService;
        private readonly Action<string>? applyCodeCallback;
        private readonly string sessionId;

        public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();

        [ObservableProperty]
        private string inputText = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusText = string.Empty;

        public string SessionId => sessionId;

        /// <summary>
        /// Erzeugt das Chat-ViewModel.
        /// </summary>
        /// <param name="applyCodeCallback">Callback, mit dem ein extrahierter FPC-Codeblock in den Editor übernommen wird.</param>
        public DevelopChatViewModel(Action<string>? applyCodeCallback = null)
        {
            chatService = new N8nDevelopChatService();
            this.applyCodeCallback = applyCodeCallback;
            sessionId = $"desktop-{Guid.NewGuid():N}";
            StatusText = $"Session: {sessionId}";
        }

        [RelayCommand(CanExecute = nameof(CanSend))]
        private async Task SendAsync()
        {
            var prompt = (InputText ?? string.Empty).Trim();
            if (prompt.Length == 0)
            {
                return;
            }

            var userMessage = new ChatMessageViewModel
            {
                Author = "Du",
                Text = prompt,
                IsUser = true
            };
            Messages.Add(userMessage);

            InputText = string.Empty;

            var pending = new ChatMessageViewModel
            {
                Author = "FPCDevelopPipeline",
                Text = "… denkt nach …",
                IsUser = false,
                IsPending = true
            };
            Messages.Add(pending);

            IsBusy = true;
            StatusText = "Sende Anfrage an FPCDevelopPipeline …";
            SendCommand.NotifyCanExecuteChanged();

            try
            {
                var answer = await chatService.SendAsync(prompt, sessionId);
                pending.Text = string.IsNullOrWhiteSpace(answer) ? "(leere Antwort)" : answer;
                pending.IsPending = false;
                pending.ExtractedFpcCode = ExtractFpcCode(pending.Text);
                StatusText = pending.HasFpcCode
                    ? "Antwort erhalten – FPC-Codeblock erkannt."
                    : "Antwort erhalten.";
            }
            catch (Exception ex)
            {
                pending.Text = $"Fehler: {ex.Message}";
                pending.IsPending = false;
                StatusText = "Fehler beim Aufruf der Pipeline.";
            }
            finally
            {
                IsBusy = false;
                SendCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanSend()
        {
            return IsBusy == false && string.IsNullOrWhiteSpace(InputText) == false;
        }

        partial void OnInputTextChanged(string value)
        {
            SendCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void ClearHistory()
        {
            Messages.Clear();
            StatusText = $"Session: {sessionId} – Verlauf gelöscht (Pipeline-Memory bleibt server-seitig).";
        }

        [RelayCommand]
        private void ApplyCode(ChatMessageViewModel? message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.ExtractedFpcCode))
            {
                return;
            }

            applyCodeCallback?.Invoke(message.ExtractedFpcCode!);
            StatusText = "FPC-Code in den Editor übernommen.";
        }

        /// <summary>
        /// Extrahiert den ersten FPC-Codeblock aus einer Markdown-Antwort.
        /// Akzeptiert <c>```fpc</c>-Fences und – als Fallback – generische <c>```</c>-Blöcke,
        /// sofern sie wie FPC-Code aussehen.
        /// </summary>
        private static string? ExtractFpcCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var fpcMatch = Regex.Match(text, "```fpc\\s*\\r?\\n(?<code>.*?)\\r?\\n```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (fpcMatch.Success)
            {
                return fpcMatch.Groups["code"].Value.Trim('\r', '\n');
            }

            var genericMatch = Regex.Match(text, "```[a-zA-Z0-9_-]*\\s*\\r?\\n(?<code>.*?)\\r?\\n```", RegexOptions.Singleline);
            if (genericMatch.Success)
            {
                var candidate = genericMatch.Groups["code"].Value.Trim('\r', '\n');
                if (LooksLikeFpc(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool LooksLikeFpc(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return Regex.IsMatch(code, "\\b(LD|LDN|AND|OR|ANDN|ORN|ST|STN|JMP|JMPC|JMPCN|RET|CAL|CTU|CTD|TON|TOF|TP)\\b", RegexOptions.IgnoreCase);
        }
    }
}

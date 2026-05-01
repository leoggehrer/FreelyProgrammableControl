using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Repräsentiert eine einzelne Chat-Nachricht (User oder Assistant) im Develop-Chat.
    /// </summary>
    public partial class ChatMessageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string author = string.Empty;

        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        /// <summary><c>true</c> für Nachrichten vom User, <c>false</c> für Antworten der Pipeline.</summary>
        [ObservableProperty]
        private bool isUser;

        /// <summary><c>true</c>, solange auf eine Antwort gewartet wird (Platzhalter-Nachricht).</summary>
        [ObservableProperty]
        private bool isPending;

        /// <summary>FPC-Codeblock aus der Antwort, falls vorhanden – ermöglicht "Code übernehmen".</summary>
        [ObservableProperty]
        private string? extractedFpcCode;

        public bool HasFpcCode => string.IsNullOrWhiteSpace(ExtractedFpcCode) == false;

        partial void OnExtractedFpcCodeChanged(string? value)
        {
            OnPropertyChanged(nameof(HasFpcCode));
        }
    }
}

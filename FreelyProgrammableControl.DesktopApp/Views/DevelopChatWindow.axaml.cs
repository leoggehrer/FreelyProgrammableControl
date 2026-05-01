using Avalonia.Controls;
using Avalonia.Threading;
using FreelyProgrammableControl.DesktopApp.ViewModels;

namespace FreelyProgrammableControl.DesktopApp.Views
{
    public partial class DevelopChatWindow : Window
    {
        public DevelopChatWindow()
        {
            InitializeComponent();
        }

        public DevelopChatWindow(DevelopChatViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.Messages.CollectionChanged += (_, _) => ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (this.FindControl<ScrollViewer>("HistoryScroll") is { } scroll)
                {
                    scroll.ScrollToEnd();
                }
            });
        }
    }
}

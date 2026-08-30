using Avalonia.Controls;

namespace Portunus.App.Views
{
    public partial class MainWindow : Window
    {
        private bool _isExiting = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // Intercepta o "X" da janela e a esconde, a menos que seja um fechamento forçado
            if (!_isExiting)
            {
                e.Cancel = true;
                this.Hide();
            }

            base.OnClosing(e);
        }

        // Método chamado pelo TrayIcon (App.axaml.cs) para encerrar de verdade
        public void ForceClose()
        {
            _isExiting = true;
            this.Close();
        }
    }
}
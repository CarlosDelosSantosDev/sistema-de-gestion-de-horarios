using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaHorarios
{
    public partial class SoporteView : UserControl
    {
        public SoporteView()
        {
            InitializeComponent();
        }

        private void WhatsApp_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.Tag is string telefono)
            {
                try
                {
                    string url = $"https://wa.me/{telefono}";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("No se pudo abrir WhatsApp.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Correo_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.Tag is string correo)
            {
                try
                {
                    string url = $"mailto:{correo}";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("No se pudo abrir el correo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

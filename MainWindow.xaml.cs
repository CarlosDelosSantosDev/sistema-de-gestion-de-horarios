using SistemaHorarios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SistemaHorarios
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseService databaseService;
        public MainWindow()
        {
            InitializeComponent();
            databaseService = new DatabaseService();
            VerificarConexion();
        }

        private void VerificarConexion()
        {
            if (databaseService.TestConnection())
            {
                MostrarMensaje("Conectado al sistema de base de datos", "#FF4CAF50");
            }
            else
            {
                MostrarMensaje("Error de conexión con la base de datos", "#FFF44336");
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MostrarMensaje("Por favor, complete todos los campos", "#FFF44336");
                return;
            }

            // Mostrar loading en el botón
            btnLogin.Content = "VERIFICANDO...";
            btnLogin.IsEnabled = false;

            // Simular un pequeño delay para mejor UX
            System.Threading.Tasks.Task.Delay(300).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (databaseService.ValidarLogin(username, password))
                    {
                        string tipoUsuario = databaseService.ObtenerTipoUsuario(username);
                        MostrarMensaje("✓ Credenciales válidas", "#FF4CAF50");
                        AbrirPanelPrincipal(username, tipoUsuario);
                    }
                    else
                    {
                        MostrarMensaje("Usuario o contraseña incorrectos", "#FFF44336");
                        btnLogin.Content = "ACCEDER AL SISTEMA";
                        btnLogin.IsEnabled = true;
                    }
                });
            });
        }

        private void MostrarMensaje(string mensaje, string colorHex)
        {
            messageBorder.Visibility = Visibility.Visible;
            txtMensaje.Text = mensaje;
            messageBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(colorHex));

            // Cambiar color de texto según el fondo
            if (colorHex == "#FF4CAF50" || colorHex == "#FFF44336")
            {
                txtMensaje.Foreground = Brushes.White;
            }
            else
            {
                txtMensaje.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF546E7A"));
            }
        }

        private void AbrirPanelPrincipal(string username, string tipoUsuario)
        {
            this.Hide();

            MainDashboard dashboard = new MainDashboard(username, tipoUsuario);
            dashboard.Closed += (s, args) => this.Close();
            dashboard.Show();
        }

        // Eventos para mejorar la UX
        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "admin")
            {
                txtUsername.SelectAll();
            }
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPassword.SelectAll();
        }


    }
}

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
using System.Windows.Shapes;

namespace SistemaHorarios
{
    /// <summary>
    /// Lógica de interacción para MainDashboard.xaml
    /// </summary>
    public partial class MainDashboard : Window
    {
        private string username;
        private string tipoUsuario;

        public MainDashboard(string username, string tipoUsuario)
        {
            InitializeComponent();
            this.username = username;
            this.tipoUsuario = tipoUsuario;
            InitializeDashboard();

        }

        private void InitializeDashboard()
        {
            txtWelcome.Text = $"Bienvenido, {username} ({tipoUsuario.ToUpper()})";
            txtUserWelcome.Text = $"¿Qué deseas hacer hoy, {username}?";
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string opcion = button.Tag.ToString();

            // Ocultar mensaje de bienvenida
            panelWelcome.Visibility = Visibility.Collapsed;
            contentArea.Visibility = Visibility.Visible;

            switch (opcion)
            {
                case "Profesores":
                    MostrarProfesores();
                    break;
                case "Materias":
                    MostrarMaterias();
                    break;
                case "Grupos":
                    MostrarGrupos();
                    break;
                case "Carreras":
                    MostrarCarreras();
                    break;
                case "Horarios":
                    MostrarHorarios();
                    break;
                case "CrearHorario":
                    MostrarCrearHorario();
                    break;
                case "MostrarSoporte":
                    MostrarSoporte();
                    break;

            }
        }

        private void MostrarSoporte()
        {
            contentArea.Content = new SoporteView();
        }


        private void MostrarProfesores()
        {
            contentArea.Content = new ProfesoresView();
        }

        private void MostrarMaterias()
        {
            contentArea.Content = new MateriasView();
        }

        private void MostrarGrupos()
        {
            contentArea.Content = new GruposView();
        }

        private void MostrarCarreras()
        {
            contentArea.Content = new CarrerasView(); ;
        }

        private void MostrarHorarios()
        {
            contentArea.Content = new HorariosGuardadosView();
        }

        private void MostrarCrearHorario()
        {
            contentArea.Content = new HorarioView(); ;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?",
                "Cerrar Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}

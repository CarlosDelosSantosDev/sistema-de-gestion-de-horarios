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
using SistemaHorarios.Models;
using SistemaHorarios.Services;

namespace SistemaHorarios
{
    public partial class CarrerasView : UserControl
    {
        private CarrerasService carrerasService;

        public CarrerasView()
        {
            InitializeComponent();
            carrerasService = new CarrerasService();
            CargarCarreras();
        }

        private void CargarCarreras()
        {
            try
            {
                var carreras = carrerasService.ObtenerTodasCarreras();
                dgCarreras.ItemsSource = carreras;
                txtStatus.Text = $"Total de carreras: {carreras.Count}";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar carreras: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error al cargar datos";
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarCarreras();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CarreraFormWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarCarreras();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int idCarrera = (int)button.Tag;

            var carrera = ObtenerCarreraPorId(idCarrera);
            if (carrera != null)
            {
                var ventana = new CarreraFormWindow(carrera);
                if (ventana.ShowDialog() == true)
                {
                    CargarCarreras();
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int idCarrera = (int)button.Tag;

            var result = MessageBox.Show($"¿Estás seguro de eliminar esta carrera?",
                                       "Confirmar Eliminación",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (carrerasService.EliminarCarrera(idCarrera))
                    {
                        MessageBox.Show("Carrera eliminada correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarCarreras();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error al eliminar carrera: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private Carrera ObtenerCarreraPorId(int idCarrera)
        {
            foreach (Carrera carrera in dgCarreras.Items)
            {
                if (carrera.IdCarrera == idCarrera)
                {
                    return carrera;
                }
            }
            return null;
        }
    }
}

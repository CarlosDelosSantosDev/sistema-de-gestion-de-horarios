using SistemaHorarios.Models;
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
    /// Lógica de interacción para ProfesoresView.xaml
    /// </summary>
    public partial class ProfesoresView : UserControl
    {
        private ProfesoresService profesoresService;
        public ProfesoresView()
        {
            InitializeComponent();
            profesoresService = new ProfesoresService();
            CargarProfesores();
        }
        // MÉTODO PARA CARGAR LOS PROFESORES DESDE LA BASE DE DATOS
        private void CargarProfesores()
        {
            try
            {
                // LLAMAR AL SERVICIO PARA OBTENER LOS DATOS
                var profesores = profesoresService.ObtenerTodosProfesores();

                // MOSTRAR LOS DATOS EN LA TABLA
                dgProfesores.ItemsSource = profesores;

                // ACTUALIZAR EL STATUS
                txtStatus.Text = $"Total de profesores: {profesores.Count}";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar profesores: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error al cargar datos";
            }
        }

        // BOTÓN REFRESCAR - VOLVER A CARGAR LOS DATOS
        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarProfesores();
        }

        // BOTÓN AGREGAR - ABRIR FORMULARIO PARA NUEVO PROFESOR
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ProfesorFormWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarProfesores(); // Recargar después de agregar
            }
        }

        // BOTÓN EDITAR - ABRIR FORMULARIO PARA EDITAR PROFESOR
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int numeroTrabajador = (int)button.Tag;

            // BUSCAR EL PROFESOR EN LA TABLA
            var profesor = ObtenerProfesorPorNumero(numeroTrabajador);
            if (profesor != null)
            {
                var ventana = new ProfesorFormWindow(profesor);
                if (ventana.ShowDialog() == true)
                {
                    CargarProfesores(); // Recargar después de editar
                }
            }
        }

        // BOTÓN ELIMINAR - ELIMINAR PROFESOR
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int numeroTrabajador = (int)button.Tag;

            // CONFIRMAR ELIMINACIÓN
            var result = MessageBox.Show($"¿Estás seguro de eliminar al profesor con número {numeroTrabajador}?",
                                       "Confirmar Eliminación",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (profesoresService.EliminarProfesor(numeroTrabajador))
                    {
                        MessageBox.Show("Profesor eliminado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarProfesores(); // Recargar después de eliminar
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error al eliminar profesor: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // MÉTODO AUXILIAR PARA BUSCAR PROFESOR POR NÚMERO
        private Profesor ObtenerProfesorPorNumero(int numeroTrabajador)
        {
            foreach (Profesor profesor in dgProfesores.Items)
            {
                if (profesor.NumeroTrabajador == numeroTrabajador)
                {
                    return profesor;
                }
            }
            return null;
        }

        private void dgProfesores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

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
    public partial class MateriasView : UserControl
    {
        private MateriasService materiasService;

        public MateriasView()
        {
            InitializeComponent();
            materiasService = new MateriasService();
            CargarMaterias();
        }

        private void CargarMaterias()
        {
            try
            {
                var materias = materiasService.ObtenerTodasMaterias();
                dgMaterias.ItemsSource = materias;
                txtStatus.Text = $"Total de materias: {materias.Count}";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar materias: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error al cargar datos";
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarMaterias();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new MateriaFormWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarMaterias();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int idMateria = (int)button.Tag;

            var materia = ObtenerMateriaPorId(idMateria);
            if (materia != null)
            {
                var ventana = new MateriaFormWindow(materia);
                if (ventana.ShowDialog() == true)
                {
                    CargarMaterias();
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int idMateria = (int)button.Tag;

            var result = MessageBox.Show($"¿Estás seguro de eliminar esta materia?",
                                       "Confirmar Eliminación",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (materiasService.EliminarMateria(idMateria))
                    {
                        MessageBox.Show("Materia eliminada correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarMaterias();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error al eliminar materia: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private Materia ObtenerMateriaPorId(int idMateria)
        {
            foreach (Materia materia in dgMaterias.Items)
            {
                if (materia.IdMateria == idMateria)
                {
                    return materia;
                }
            }
            return null;
        }

        private void dgMaterias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
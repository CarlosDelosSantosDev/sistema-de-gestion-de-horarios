using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SistemaHorarios.Services;

namespace SistemaHorarios
{
    public partial class GruposView : UserControl
    {
        private GruposService gruposService;

        public GruposView()
        {
            InitializeComponent();
            gruposService = new GruposService();
            CargarGrupos();
        }

        private void CargarGrupos()
        {
            try
            {
                var grupos = gruposService.ObtenerTodosGrupos();
                dgGrupos.ItemsSource = grupos;

                int enLineaCount = grupos.Count(g =>
                    !string.IsNullOrEmpty(g.Modalidad) &&
                    g.Modalidad.ToUpper() == "EN LÍNEA");

                txtStatus.Text = $"Total de grupos: {grupos.Count} | En línea: {enLineaCount} | Presencial: {grupos.Count - enLineaCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error al cargar datos";
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarGrupos();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GrupoFormWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarGrupos();
                MessageBox.Show("Grupo agregado correctamente", "Éxito",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;

                if (button.Tag == null || !int.TryParse(button.Tag.ToString(), out int idGrupo))
                {
                    MessageBox.Show("No se pudo identificar el grupo a editar", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var grupo = gruposService.ObtenerGrupoPorId(idGrupo);

                if (grupo != null)
                {
                    var ventana = new GrupoFormWindow(grupo);
                    if (ventana.ShowDialog() == true)
                    {
                        CargarGrupos();
                        MessageBox.Show("Grupo actualizado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró el grupo especificado", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar grupo: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;

                if (button.Tag == null || !int.TryParse(button.Tag.ToString(), out int idGrupo))
                {
                    MessageBox.Show("No se pudo identificar el grupo a eliminar", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"¿Estás seguro de eliminar este grupo? Esta acción no se puede deshacer.",
                                           "Confirmar Eliminación",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (gruposService.EliminarGrupo(idGrupo))
                    {
                        MessageBox.Show("Grupo eliminado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarGrupos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar grupo: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 

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
using SistemaHorarios.Models;
using SistemaHorarios.Services;

namespace SistemaHorarios
{
    public partial class GrupoFormWindow : Window
    {
        private GruposService gruposService;
        private CarrerasService carrerasService;
        private TurnosService turnosService;
        private Grupo grupo;
        private bool esNuevo;

        public string Titulo => esNuevo ? "Agregar Nuevo Grupo" : "Editar Grupo";

        public GrupoFormWindow()
        {
            InitializeComponent();
            InitializeServices();
            esNuevo = true;
            DataContext = this;
            CargarCombos();
        }

        public GrupoFormWindow(Grupo grupoExistente)
        {
            InitializeComponent();
            InitializeServices();
            grupo = grupoExistente;
            esNuevo = false;
            DataContext = this;
            CargarCombos();
            CargarDatosExistente();
        }

        private void InitializeServices()
        {
            gruposService = new GruposService();
            carrerasService = new CarrerasService();
            turnosService = new TurnosService();
        }

        private void CargarCombos()
        {
            var carreras = carrerasService.ObtenerTodasCarreras();
            var turnos = turnosService.ObtenerTodosTurnos();

            cmbCarrera.ItemsSource = carreras;
            cmbTurno.ItemsSource = turnos;
        }

        private void CargarDatosExistente()
        {
            txtGrado.Text = grupo.Grado.ToString();
            txtSeccion.Text = grupo.Seccion;
            cmbCarrera.SelectedValue = grupo.IdCarrera;
            cmbTurno.SelectedValue = grupo.IdTurno;

            // ✅ CARGAR MODALIDAD EXISTENTE
            if (!string.IsNullOrEmpty(grupo.Modalidad))
            {
                foreach (ComboBoxItem item in cmbModalidad.Items)
                {
                    if (item.Content.ToString() == grupo.Modalidad.ToUpper())
                    {
                        cmbModalidad.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                // ✅ OBTENER MODALIDAD SELECCIONADA
                string modalidadSeleccionada = cmbModalidad.SelectedItem != null ?
                    ((ComboBoxItem)cmbModalidad.SelectedItem).Content.ToString() : "PRESENCIAL";

                var grupoActualizado = new Grupo
                {
                    Grado = int.Parse(txtGrado.Text),
                    Seccion = string.IsNullOrWhiteSpace(txtSeccion.Text) ? null : txtSeccion.Text.Trim(),
                    IdCarrera = (int)cmbCarrera.SelectedValue,
                    IdTurno = (int)cmbTurno.SelectedValue,
                    Modalidad = modalidadSeleccionada // ✅ AGREGAR MODALIDAD
                };

                bool resultado;
                if (esNuevo)
                {
                    resultado = gruposService.AgregarGrupo(grupoActualizado);
                }
                else
                {
                    grupoActualizado.IdGrupo = grupo.IdGrupo;

                    // Debug temporal
                    MessageBox.Show($"Actualizando grupo ID: {grupoActualizado.IdGrupo}\n" +
                                  $"Grado: {grupoActualizado.Grado}\n" +
                                  $"Sección: {grupoActualizado.Seccion}\n" +
                                  $"Carrera ID: {grupoActualizado.IdCarrera}\n" +
                                  $"Turno ID: {grupoActualizado.IdTurno}\n" +
                                  $"Modalidad: {grupoActualizado.Modalidad}", // ✅ AGREGAR MODALIDAD AL DEBUG
                                  "Debug - Datos a Actualizar");

                    resultado = gruposService.ActualizarGrupo(grupoActualizado);
                }

                if (resultado)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el grupo", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            // Validar grado
            if (string.IsNullOrWhiteSpace(txtGrado.Text))
            {
                MessageBox.Show("El grado es obligatorio", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGrado.Focus();
                return false;
            }

            if (!int.TryParse(txtGrado.Text, out int grado) || grado <= 0)
            {
                MessageBox.Show("El grado debe ser un número válido mayor a 0", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGrado.Focus();
                txtGrado.SelectAll();
                return false;
            }

            // Validar carrera
            if (cmbCarrera.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una carrera", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCarrera.Focus();
                return false;
            }

            // Validar turno
            if (cmbTurno.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un turno", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTurno.Focus();
                return false;
            }

            // ✅ VALIDAR MODALIDAD (siempre debe tener selección por el IsSelected="True")
            if (cmbModalidad.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una modalidad", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbModalidad.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
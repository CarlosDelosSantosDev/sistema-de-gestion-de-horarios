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
using System.Windows.Shapes;

namespace SistemaHorarios
{
    /// <summary>
    /// Lógica de interacción para ProfesorFormWindow.xaml
    /// </summary>
    public partial class ProfesorFormWindow : Window
    {
        private ProfesoresService profesoresService;
        private Profesor profesor;
        private bool esNuevo;

        public string Titulo => esNuevo ? "Agregar Nuevo Profesor" : "Editar Profesor";
        public ProfesorFormWindow()
        {
            InitializeComponent();
            profesoresService = new ProfesoresService();
            esNuevo = true;
            DataContext = this;
        }

        public ProfesorFormWindow(Profesor profesorExistente)
        {
            InitializeComponent();
            profesoresService = new ProfesoresService();
            profesor = profesorExistente;
            esNuevo = false;
            DataContext = this;
            CargarDatosExistente();
        }

        private void CargarDatosExistente()
        {
            txtNumeroTrabajador.Text = profesor.NumeroTrabajador.ToString();
            txtNombre.Text = profesor.NombreDocente;
            txtApellidoPaterno.Text = profesor.ApellidoPaterno;
            txtApellidoMaterno.Text = profesor.ApellidoMaterno;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var nuevoProfesor = new Profesor
                {
                    NumeroTrabajador = int.Parse(txtNumeroTrabajador.Text),
                    NombreDocente = txtNombre.Text.Trim(),
                    ApellidoPaterno = txtApellidoPaterno.Text.Trim(),
                    ApellidoMaterno = txtApellidoMaterno.Text.Trim()
                };

                bool resultado;
                if (esNuevo)
                {
                    resultado = profesoresService.AgregarProfesor(nuevoProfesor);
                }
                else
                {
                    resultado = profesoresService.ActualizarProfesor(nuevoProfesor);
                }

                if (resultado)
                {
                    DialogResult = true;
                    Close();
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
            if (string.IsNullOrWhiteSpace(txtNumeroTrabajador.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidoPaterno.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios (*)", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(txtNumeroTrabajador.Text, out int numero))
            {
                MessageBox.Show("El número de trabajador debe ser un valor numérico", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
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

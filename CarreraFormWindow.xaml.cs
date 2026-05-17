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
    public partial class CarreraFormWindow : Window
    {
        private CarrerasService carrerasService;
        private Carrera carrera;
        private bool esNuevo;

        public string Titulo => esNuevo ? "Agregar Nueva Carrera" : "Editar Carrera";

        public CarreraFormWindow()
        {
            InitializeComponent();
            carrerasService = new CarrerasService();
            esNuevo = true;
            DataContext = this;
        }

        public CarreraFormWindow(Carrera carreraExistente)
        {
            InitializeComponent();
            carrerasService = new CarrerasService();
            carrera = carreraExistente;
            esNuevo = false;
            DataContext = this;
            CargarDatosExistente();
        }

        private void CargarDatosExistente()
        {
            txtNombre.Text = carrera.Nombre;
            txtClave.Text = carrera.Clave;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var nuevaCarrera = new Carrera
                {
                    Nombre = txtNombre.Text.Trim(),
                    Clave = txtClave.Text.Trim().ToUpper()
                };

                bool resultado;
                if (esNuevo)
                {
                    resultado = carrerasService.AgregarCarrera(nuevaCarrera);
                }
                else
                {
                    nuevaCarrera.IdCarrera = carrera.IdCarrera;
                    resultado = carrerasService.ActualizarCarrera(nuevaCarrera);
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
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios (*)", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (txtClave.Text.Trim().Length > 20)
            {
                MessageBox.Show("La clave no puede tener más de 20 caracteres", "Validación",
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
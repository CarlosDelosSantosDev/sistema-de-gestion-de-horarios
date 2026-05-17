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
    public partial class MateriaFormWindow : Window
    {
        private MateriasService materiasService;
        private Materia materia;
        private bool esNuevo;

        public string Titulo => esNuevo ? "Agregar Nueva Materia" : "Editar Materia";

        public MateriaFormWindow()
        {
            InitializeComponent();
            materiasService = new MateriasService();
            esNuevo = true;
            DataContext = this;
        }

        public MateriaFormWindow(Materia materiaExistente)
        {
            InitializeComponent();
            materiasService = new MateriasService();
            materia = materiaExistente;
            esNuevo = false;
            DataContext = this;
            CargarDatosExistente();
        }

        private void CargarDatosExistente()
        {
            txtNombre.Text = materia.Nombre;
            txtGrupo.Text = materia.Grupo;
            txtClave.Text = materia.Clave;
            
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var nuevaMateria = new Materia
                {
                    Nombre = txtNombre.Text.Trim(),
                    Grupo = string.IsNullOrWhiteSpace(txtGrupo.Text) ? null : txtGrupo.Text.Trim(),
                    Clave = string.IsNullOrWhiteSpace(txtGrupo.Text) ? null : txtClave.Text.Trim(),
                    
                };

                bool resultado;
                if (esNuevo)
                {
                    resultado = materiasService.AgregarMateria(nuevaMateria);
                }
                else
                {
                    nuevaMateria.IdMateria = materia.IdMateria;
                    resultado = materiasService.ActualizarMateria(nuevaMateria);
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
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, complete los campos obligatorios (*)", "Validación",
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

using SistemaHorarios.Models;
using SistemaHorarios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SistemaHorarios
{
    public partial class HorariosGuardadosView : UserControl
    {
        private HorariosService horariosService;
        private List<Horario> horariosActuales;
        private string periodoSeleccionado;

        private HorarioTradicional horarioSeleccionado;
        

        public class PeriodoInfo
        {
            public string Periodo { get; set; }
            public int HorariosCount { get; set; }
            public List<Horario> Horarios { get; set; }

            
        }

        // Modelo para horario tradicional
        public class HorarioTradicional
        {
            public int NumeroTrabajador { get; set; }
            public string NombreProfesor { get; set; }
            public List<Horario> Clases { get; set; }
        }

        public HorariosGuardadosView()
        {
            InitializeComponent();
            horariosService = new HorariosService();
            CargarPeriodosConHorarios();
        }

        private void CargarPeriodosConHorarios()
        {
            try
            {
                var periodosDisponibles = new List<string>
                {
                    "ENE-ABR 2026", "MAY-AGO 2026", "SEP-DIC 2026",
                    "ENE-ABR 2027", "MAY-AGO 2027", "SEP-DIC 2027",
                    "ENE-ABR 2028", "MAY-AGO 2028", "SEP-DIC 2028"
                };

                var periodosConHorarios = new List<PeriodoInfo>();

                foreach (var periodo in periodosDisponibles)
                {
                    var horarios = horariosService.ObtenerHorariosPorPeriodo(periodo);
                    if (horarios != null && horarios.Count > 0)
                    {
                        // ✅ CONTAR HORARIOS (profesores distintos), no clases
                        int cantidadHorarios = horarios
                            .GroupBy(h => h.IdProfesor)
                            .Count();

                        periodosConHorarios.Add(new PeriodoInfo
                        {
                            Periodo = periodo,
                            HorariosCount = cantidadHorarios,  // ✅ Número de horarios, no de clases
                            Horarios = horarios
                        });
                    }
                }

                if (periodosConHorarios.Count > 0)
                {
                    itemsControlPeriodos.ItemsSource = periodosConHorarios;
                    txtNoPeriodos.Visibility = Visibility.Collapsed;
                }
                else
                {
                    itemsControlPeriodos.Visibility = Visibility.Collapsed;
                    txtNoPeriodos.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar periodos: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardPeriodo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is PeriodoInfo periodoInfo)
            {
                periodoSeleccionado = periodoInfo.Periodo;
                horariosActuales = periodoInfo.Horarios;

                MostrarVistaDetalle();
            }
        }

        private void MostrarVistaDetalle()
        {
            panelPeriodos.Visibility = Visibility.Collapsed;
            panelDetalleHorarios.Visibility = Visibility.Visible;

            // Resetear selección
            horarioSeleccionado = null;
            if (txtInstruccion != null)
                txtInstruccion.Visibility = Visibility.Collapsed;

            // Agrupar horarios por profesor
            var horariosPorProfesor = AgruparHorariosPorProfesor(horariosActuales);

            // ✅ Mostrar cantidad en el título
            int cantidadHorarios = horariosPorProfesor.Count;
            txtTituloPeriodo.Text = $"HORARIOS - {periodoSeleccionado} ({cantidadHorarios} profesor(es))";

            itemsControlHorariosProfesores.ItemsSource = horariosPorProfesor;

            // Crear las tablas tradicionales para cada profesor
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CrearTablasTradicionales(horariosPorProfesor);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private List<HorarioTradicional> AgruparHorariosPorProfesor(List<Horario> horarios)
        {
            var grupos = horarios.GroupBy(h => new { h.IdProfesor, h.NombreProfesor })
                                .Select(g => new HorarioTradicional
                                {
                                    NumeroTrabajador = g.Key.IdProfesor,
                                    NombreProfesor = g.Key.NombreProfesor,
                                    Clases = g.ToList()
                                })
                                .OrderBy(h => h.NombreProfesor)
                                .ToList();

            return grupos;
        }

        private void CrearTablasTradicionales(List<HorarioTradicional> horariosProfesores)
        {
            for (int i = 0; i < itemsControlHorariosProfesores.Items.Count; i++)
            {
                var container = itemsControlHorariosProfesores.ItemContainerGenerator.ContainerFromIndex(i);
                if (container is ContentPresenter contentPresenter)
                {
                    var stackPanel = FindVisualChild<StackPanel>(contentPresenter);
                    if (stackPanel != null)
                    {
                        var grid = FindVisualChild<Grid>(stackPanel);
                        if (grid != null && grid.Name == "gridHorarioTradicional")
                        {
                            var horarioProfesor = itemsControlHorariosProfesores.Items[i] as HorarioTradicional;
                            CrearTablaHorarioTradicional(grid, horarioProfesor.Clases);
                        }
                    }
                }
            }
        }

        private void CrearTablaHorarioTradicional(Grid grid, List<Horario> clases)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            // Configurar columnas: Hora + 6 días (Lunes a Sábado)
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) }); // Columna de horas
            string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            foreach (var dia in dias)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Configurar filas: Encabezado + filas de horas
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) }); // Fila de encabezados

            // ENCABEZADOS DE DÍAS
            for (int i = 0; i < dias.Length; i++)
            {
                var label = new Label
                {
                    Content = dias[i].ToUpper(),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F")),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };
                Grid.SetColumn(label, i + 1);
                Grid.SetRow(label, 0);
                grid.Children.Add(label);
            }

            // DEFINIR BLOQUES DE HORARIO (diurnos y nocturnos)
            var bloquesHorarios = new[]
            {
                // Horario diurno
                new { Hora = "07:30 - 08:20", Inicio = new TimeSpan(7, 30, 0) },
                new { Hora = "08:20 - 09:10", Inicio = new TimeSpan(8, 20, 0) },
                new { Hora = "09:10 - 10:00", Inicio = new TimeSpan(9, 10, 0) },
                new { Hora = "10:00 - 10:50", Inicio = new TimeSpan(10, 0, 0) },
                new { Hora = "10:50 - 11:40", Inicio = new TimeSpan(10, 50, 0) },
                new { Hora = "11:40 - 12:30", Inicio = new TimeSpan(11, 40, 0) },
                new { Hora = "12:30 - 13:20", Inicio = new TimeSpan(12, 30, 0) },
                new { Hora = "13:20 - 14:10", Inicio = new TimeSpan(13, 20, 0) },
                new { Hora = "14:10 - 15:00", Inicio = new TimeSpan(14, 10, 0) },
                new { Hora = "15:00 - 15:50", Inicio = new TimeSpan(15, 0, 0) },
                new { Hora = "15:50 - 16:40", Inicio = new TimeSpan(15, 50, 0) },
                new { Hora = "16:40 - 17:30", Inicio = new TimeSpan(16, 40, 0) },
                new { Hora = "17:30 - 18:20", Inicio = new TimeSpan(17, 30, 0) },
                new { Hora = "18:20 - 19:10", Inicio = new TimeSpan(18, 20, 0) },
                new { Hora = "19:10 - 20:00", Inicio = new TimeSpan(19, 10, 0) },
                
                // Horario nocturno
                new { Hora = "18:30 - 19:15", Inicio = new TimeSpan(18, 30, 0) },
                new { Hora = "19:15 - 20:00", Inicio = new TimeSpan(19, 15, 0) },
                new { Hora = "20:00 - 20:45", Inicio = new TimeSpan(20, 0, 0) },
                new { Hora = "20:45 - 21:30", Inicio = new TimeSpan(20, 45, 0) },
                new { Hora = "21:30 - 22:15", Inicio = new TimeSpan(21, 30, 0) },
                new { Hora = "22:15 - 23:00", Inicio = new TimeSpan(22, 15, 0) }
            };

            // CREAR FILAS PARA CADA BLOQUE DE HORARIO
            for (int fila = 0; fila < bloquesHorarios.Length; fila++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });

                var bloque = bloquesHorarios[fila];

                // Celda de hora
                var labelHora = new Label
                {
                    Content = bloque.Hora,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = fila < 15 ? Brushes.LightBlue : Brushes.LightGray, // Azul para diurno, Gris para nocturno
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Foreground = Brushes.Black
                };
                Grid.SetColumn(labelHora, 0);
                Grid.SetRow(labelHora, fila + 1);
                grid.Children.Add(labelHora);

                // Celdas vacías para cada día
                for (int col = 1; col <= dias.Length; col++)
                {
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Tag = new { Dia = dias[col - 1], HoraInicio = bloque.Inicio }
                    };
                    Grid.SetColumn(border, col);
                    Grid.SetRow(border, fila + 1);
                    grid.Children.Add(border);
                }
            }

            // LLENAR CON LAS CLASES DEL PROFESOR
            LlenarTablaConClases(grid, clases);
        }

        private void LlenarTablaConClases(Grid grid, List<Horario> clases)
        {
            foreach (var clase in clases)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Border border && border.Tag != null)
                    {
                        try
                        {
                            dynamic tag = border.Tag;
                            if (tag.Dia == clase.DiaSemana && tag.HoraInicio == clase.HoraInicio)
                            {
                                // ✅ CREAR GRID INTERNO CON DOS FILAS
                                var gridInterno = new Grid();
                                gridInterno.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                                gridInterno.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                                // ✅ TEXTO SUPERIOR: Grupo/Materia
                                string textoSuperior = ObtenerTextoSuperior(clase);
                                var textBlockSuperior = new TextBlock
                                {
                                    Text = textoSuperior,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 10,
                                    FontWeight = FontWeights.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Foreground = Brushes.Black
                                };
                                Grid.SetRow(textBlockSuperior, 0);
                                gridInterno.Children.Add(textBlockSuperior);

                                // ✅ TEXTO INFERIOR: Modalidad
                                string textoInferior = ObtenerTextoModalidad(clase);
                                var textBlockInferior = new TextBlock
                                {
                                    Text = textoInferior,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 9,
                                    FontWeight = FontWeights.Normal,
                                    TextAlignment = TextAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Foreground = Brushes.Black
                                };
                                Grid.SetRow(textBlockInferior, 1);
                                gridInterno.Children.Add(textBlockInferior);

                                Color colorFondo = ObtenerColorPorClase(clase);

                                border.Child = gridInterno;
                                border.Background = new SolidColorBrush(colorFondo);
                                border.BorderBrush = Brushes.Black;
                                border.BorderThickness = new Thickness(1);

                                // ✅ TOOLTIP MEJORADO
                                string tooltip = CrearTooltip(clase);
                                border.ToolTip = tooltip;

                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }
            }
        }

        // ✅ MÉTODO PARA TEXTO SUPERIOR (Grupo/Materia)
        private string ObtenerTextoSuperior(Horario clase)
        {
            if (!string.IsNullOrEmpty(clase.ClaveMateria))
            {
                return clase.ClaveMateria + (clase.EsNP ? " - NP" : "");
            }

            if (!string.IsNullOrEmpty(clase.ClaveCarrera) && clase.Grado > 0)
            {
                string texto = $"{clase.ClaveCarrera} {clase.Grado}{clase.Seccion} {clase.Turno}".Trim();
                return texto + (clase.EsNP ? " - NP" : "");
            }

            if (!string.IsNullOrEmpty(clase.NombreMateria))
            {
                string texto = clase.NombreMateria.Length > 10 ?
                    clase.NombreMateria.Substring(0, 8) + ".." : clase.NombreMateria;
                return texto + (clase.EsNP ? " - NP" : "");
            }

            return clase.EsNP ? "NP" : "Clase";
        }

        // ✅ MÉTODO PARA TEXTO INFERIOR (Modalidad)
        private string ObtenerTextoModalidad(Horario clase)
        {
            if (!string.IsNullOrEmpty(clase.Modalidad) && clase.Modalidad.ToUpper() == "EN LÍNEA")
                return "EN LÍNEA";
            else
                return "PRESENCIAL";
        }

        // ✅ MÉTODO PARA CREAR TOOLTIP
        private string CrearTooltip(Horario clase)
        {
            string tooltip = $"{clase.DiaSemana} {clase.HoraInicio:hh\\:mm}-{clase.HoraFin:hh\\:mm}\n";

            if (!string.IsNullOrEmpty(clase.NombreMateria))
                tooltip += $"Materia: {clase.NombreMateria}\n";
            if (!string.IsNullOrEmpty(clase.ClaveMateria))
                tooltip += $"Clave: {clase.ClaveMateria}\n";
            if (!string.IsNullOrEmpty(clase.ClaveCarrera))
            {
                string grupoCompleto = $"{clase.ClaveCarrera} {clase.Grado}{clase.Seccion} {clase.Turno}".Trim();
                tooltip += $"Grupo: {grupoCompleto}\n";
            }

            string modalidad = ObtenerTextoModalidad(clase);
            tooltip += $"Modalidad: {modalidad}\n";

            if (clase.EsNP)
                tooltip += "⚠️ NO PAGADO";

            return tooltip;
        }

        private string ObtenerTextoClaseCompacto(Horario clase)
        {
            // Prioridad 1: Mostrar clave de materia si existe
            if (!string.IsNullOrEmpty(clase.ClaveMateria))
            {
                string texto = clase.ClaveMateria;
                if (clase.EsNP) texto += " NP";
                return texto;
            }

            // Prioridad 2: Mostrar carrera y grupo si existe - ✅ INCLUIR TURNO
            if (!string.IsNullOrEmpty(clase.ClaveCarrera) && clase.Grado > 0)
            {
                // ✅ INCLUIR EL TURNO EN EL TEXTO
                string texto = $"{clase.ClaveCarrera} {clase.Grado}{clase.Seccion} {clase.Turno}".Trim();
                if (clase.EsNP) texto += " NP";
                return texto;
            }

            // Prioridad 3: Mostrar nombre de materia abreviado
            if (!string.IsNullOrEmpty(clase.NombreMateria))
            {
                string texto = clase.NombreMateria.Length > 10 ?
                    clase.NombreMateria.Substring(0, 8) + ".." : clase.NombreMateria;
                if (clase.EsNP) texto += " NP";
                return texto;
            }

            return clase.EsNP ? "NP" : "Clase";
        }

        private Color ObtenerColorPorClase(Horario clase)
        {
            if (clase.EsNP)
                return Colors.LightYellow;  // Amarillo claro para NP

            // Colores CLAROS según el tipo de contenido
            if (!string.IsNullOrEmpty(clase.ClaveMateria))
            {
                // Generar color CLARO basado en la clave de materia
                int hash = clase.ClaveMateria.GetHashCode();
                return Color.FromArgb(255,
                    (byte)(hash % 55 + 200),  // Rango 200-255 (colores claros)
                    (byte)((hash >> 8) % 55 + 200),
                    (byte)((hash >> 16) % 55 + 200));
            }

            if (!string.IsNullOrEmpty(clase.ClaveCarrera))
            {
                // Colores CLAROS para grupos
                var coloresGrupos = new[]
                {
                    Colors.LightGreen,      // Verde claro
                    Colors.LightBlue,       // Azul claro  
                    Colors.LightCyan,       // Cyan claro
                    Colors.LightPink,       // Rosa claro
                    Colors.LightSalmon,     // Salmón claro
                    Colors.LightSeaGreen,   // Verde mar claro
                    Colors.LightSkyBlue,    // Azul cielo claro
                    Colors.LightSteelBlue,  // Azul acero claro
                    Colors.LightCoral,      // Coral claro
                    Colors.PaleGreen,       // Verde pálido
                    Colors.PaleTurquoise,   // Turquesa pálido
                    Colors.PaleVioletRed    // Violeta rojizo pálido
                };

                int index = Math.Abs(clase.ClaveCarrera.GetHashCode()) % coloresGrupos.Length;
                return coloresGrupos[index];
            }

            // Color por defecto CLARO
            return Colors.LightGray;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            panelDetalleHorarios.Visibility = Visibility.Collapsed;
            panelPeriodos.Visibility = Visibility.Visible;
            periodoSeleccionado = null;
            horariosActuales = null;
        }

        // Métodos para los botones (por ahora vacíos)


        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (horarioSeleccionado == null)
            {
                MessageBox.Show("❌ Primero selecciona un horario haciendo clic en él",
                    "Selección Requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"¿Estás seguro de que quieres editar el horario de:\n\n" +
                                       $"👨‍🏫 {horarioSeleccionado.NombreProfesor}\n" +
                                       $"🔢 Número: {horarioSeleccionado.NumeroTrabajador}\n" +
                                       $"📅 Periodo: {periodoSeleccionado}\n" +
                                       $"📚 Clases: {horarioSeleccionado.Clases.Count}\n\n" +
                                       $"Se abrirá el editor de horarios con estos datos.",
                                       "Confirmar Edición",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AbrirEditorHorario(horarioSeleccionado);
            }
        }

        private void AbrirEditorHorario(HorarioTradicional horarioTradicional)
        {
            try
            {
                // Crear un objeto Horario básico con la información del profesor
                var horarioBase = new Horario
                {
                    IdProfesor = horarioTradicional.NumeroTrabajador,
                    NombreProfesor = horarioTradicional.NombreProfesor,
                    Periodo = periodoSeleccionado
                };

                // Crear instancia del UserControl de crear horarios
                var horarioView = new HorarioView();

                // Pasar los datos del horario seleccionado
                horarioView.CargarHorarioParaEdicion(horarioBase);

                // ✅ NAVEGAR AL HORARIOVIEW USANDO EL CONTENTCONTROL DEL MAIN DASHBOARD
                NavegarAHorarioView(horarioView);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir editor: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ MÉTODO PARA NAVEGAR AL HORARIOVIEW
        private void NavegarAHorarioView(HorarioView horarioView)
        {
            // Obtener la ventana principal (MainDashboard)
            var mainDashboard = Window.GetWindow(this) as MainDashboard;
            if (mainDashboard != null)
            {
                // Ocultar panel de bienvenida si está visible
                if (mainDashboard.panelWelcome != null)
                    mainDashboard.panelWelcome.Visibility = Visibility.Collapsed;

                // Mostrar contentArea si no está visible
                if (mainDashboard.contentArea != null)
                {
                    mainDashboard.contentArea.Visibility = Visibility.Visible;
                    mainDashboard.contentArea.Content = horarioView;
                }
            }
            else
            {
                // Fallback: si no se encuentra el MainDashboard, usar ventana
                var ventana = new Window
                {
                    Title = $"Editor de Horarios - {horarioSeleccionado.NombreProfesor} - {periodoSeleccionado}",
                    Content = horarioView,
                    WindowState = WindowState.Maximized,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };
                ventana.ShowDialog();
            }
        }

        private void ActualizarVistaDespuesDeEditar()
        {
            try
            {
                // 1. Actualizar la lista de horarios actuales
                horariosActuales = horariosService.ObtenerHorariosPorPeriodo(periodoSeleccionado);

                // 2. Si ya no hay horarios, mostrar mensaje y volver
                if (horariosActuales == null || horariosActuales.Count == 0)
                {
                    MessageBox.Show("✅ No hay horarios en este periodo",
                        "Sin Horarios", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Volver a la vista de periodos
                    BtnVolver_Click(null, null);
                    return;
                }

                // 3. Resetear selección
                horarioSeleccionado = null;
                if (txtInstruccion != null)
                    txtInstruccion.Visibility = Visibility.Collapsed;

                // 4. Actualizar el ItemsSource con los nuevos datos
                var horariosPorProfesor = AgruparHorariosPorProfesor(horariosActuales);
                itemsControlHorariosProfesores.ItemsSource = horariosPorProfesor;

                // 5. Forzar actualización visual
                itemsControlHorariosProfesores.Items.Refresh();

                // 6. Recrear las tablas
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CrearTablasTradicionales(horariosPorProfesor);
                }), System.Windows.Threading.DispatcherPriority.Loaded);

                // 7. Actualizar título con nueva cantidad
                int cantidadHorarios = horariosPorProfesor.Count;
                txtTituloPeriodo.Text = $"HORARIOS - {periodoSeleccionado} ({cantidadHorarios} profesor(es))";

                // 8. Actualizar también las cards de periodos
                CargarPeriodosConHorarios();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la vista: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AbrirEnNuevaVentana(UserControl userControl)
        {
            var ventana = new Window
            {
                Title = "Editor de Horarios",
                Content = userControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            ventana.Show();
            // Cerrar esta ventana si es necesario
            // this.Close(); 
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (horarioSeleccionado == null)
            {
                MessageBox.Show("❌ Primero selecciona un horario haciendo clic en él",
                    "Selección Requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Estás seguro de ELIMINAR el horario completo de:\n\n" +
                $"👨‍🏫 {horarioSeleccionado.NombreProfesor}\n" +
                $"🔢 Número: {horarioSeleccionado.NumeroTrabajador}\n" +
                $"📅 Periodo: {periodoSeleccionado}\n" +
                $"📚 Clases: {horarioSeleccionado.Clases.Count}\n\n" +
                $"⚠️ Esta acción no se puede deshacer",
                "CONFIRMAR ELIMINACIÓN",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                bool eliminado = horariosService.EliminarHorarioCompletoProfesor(
                    horarioSeleccionado.NumeroTrabajador, periodoSeleccionado);

                if (eliminado)
                {
                    MessageBox.Show($"✅ Horario eliminado correctamente:\n{horarioSeleccionado.NombreProfesor}",
                        "Eliminación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    // ✅ ACTUALIZAR INMEDIATAMENTE SIN SALIR DE LA VISTA
                    ActualizarVistaDespuesDeEliminar();
                }
                else
                {
                    MessageBox.Show("❌ Error al eliminar el horario",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ✅ NUEVO MÉTODO PARA ACTUALIZAR LA VISTA INMEDIATAMENTE
        private void ActualizarVistaDespuesDeEliminar()
        {
            try
            {
                // 1. Actualizar la lista de horarios actuales
                horariosActuales = horariosService.ObtenerHorariosPorPeriodo(periodoSeleccionado);

                // 2. Si ya no hay horarios, mostrar mensaje y volver
                if (horariosActuales == null || horariosActuales.Count == 0)
                {
                    MessageBox.Show("✅ Todos los horarios han sido eliminados",
                        "Eliminación Completa", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Volver a la vista de periodos
                    BtnVolver_Click(null, null);

                    // ✅ ACTUALIZAR CARDS TAMBIÉN
                    CargarPeriodosConHorarios();
                    return;
                }

                // 3. Resetear selección
                horarioSeleccionado = null;
                if (txtInstruccion != null)
                    txtInstruccion.Visibility = Visibility.Collapsed;

                // 4. Actualizar el ItemsSource con los nuevos datos
                var horariosPorProfesor = AgruparHorariosPorProfesor(horariosActuales);
                itemsControlHorariosProfesores.ItemsSource = horariosPorProfesor;

                // 5. Forzar actualización visual
                itemsControlHorariosProfesores.Items.Refresh();

                // 6. Recrear las tablas
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CrearTablasTradicionales(horariosPorProfesor);
                }), System.Windows.Threading.DispatcherPriority.Loaded);

                // 7. Actualizar título con nueva cantidad
                int cantidadHorarios = horariosPorProfesor.Count;
                txtTituloPeriodo.Text = $"HORARIOS - {periodoSeleccionado} ({cantidadHorarios} profesor(es))";

                // ✅ SOLO ESTA LÍNEA NUEVA
                CargarPeriodosConHorarios();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la vista: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (horarioSeleccionado == null)
            {
                MessageBox.Show("❌ Primero selecciona un horario haciendo clic en él",
                    "Selección Requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ventanaImpresion = new VentanaPrevisualizacionImpresion(horarioSeleccionado, periodoSeleccionado);
                ventanaImpresion.Owner = Window.GetWindow(this);
                ventanaImpresion.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar vista de impresión: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnImprimirTodo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(periodoSeleccionado))
                {
                    MessageBox.Show("No hay un periodo seleccionado.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Obtener todos los horarios del periodo actual
                var horariosService = new HorariosService();
                var todosLosHorarios = horariosService.ObtenerHorariosPorPeriodo(periodoSeleccionado);

                if (todosLosHorarios == null || !todosLosHorarios.Any())
                {
                    MessageBox.Show("No se encontraron horarios para este periodo.", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Agrupar por profesor
                var horariosPorProfesor = AgruparHorariosPorProfesor(todosLosHorarios);

                // Mostrar mensaje de confirmación
                var resultado = MessageBox.Show(
                    $"¿Estás seguro de que quieres imprimir TODOS los horarios del periodo {periodoSeleccionado}?\n\n" +
                    $"Se imprimirán {horariosPorProfesor.Count} horarios de profesores.\n\n" +
                    $"Esta operación puede tomar varios minutos.",
                    "Confirmar impresión masiva",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado != MessageBoxResult.Yes) return;

                // Deshabilitar botón mientras se imprime
                var boton = sender as Button;
                var botonOriginalContent = boton.Content;
                boton.IsEnabled = false;
                boton.Content = "IMPRIMIENDO...";

                // **CORRECIÓN: No usar Owner para evitar que cierre la ventana principal**
                var ventanaProgreso = CrearVentanaProgreso(horariosPorProfesor.Count);
                bool cancelado = false;

                // Configurar evento de cancelación
                var btnCancelar = (ventanaProgreso.Content as StackPanel)?.Children
                    .OfType<Button>().FirstOrDefault(b => b.Name == "btnCancelar");

                if (btnCancelar != null)
                {
                    btnCancelar.Click += (s, args) => {
                        cancelado = true;
                        ventanaProgreso.Close();
                    };
                }

                ventanaProgreso.Show();

                // Configurar diálogo de impresión UNA VEZ
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true)
                {
                    ventanaProgreso.Close();
                    boton.IsEnabled = true;
                    boton.Content = botonOriginalContent;
                    return;
                }

                printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                // Imprimir cada horario
                int horariosImpresos = 0;
                int errores = 0;

                foreach (var horarioProfesor in horariosPorProfesor)
                {
                    if (cancelado) break;

                    try
                    {
                        // Actualizar progreso
                        horariosImpresos++;
                        ActualizarProgreso(ventanaProgreso, horariosImpresos, horariosPorProfesor.Count, horarioProfesor.NombreProfesor);

                        // Forzar actualización de la UI
                        await Task.Delay(50);

                        // **CORRECIÓN: Crear ventana de impresión sin Owner**
                        var ventanaImpresion = new VentanaPrevisualizacionImpresion(horarioProfesor, periodoSeleccionado)
                        {
                            ShowInTaskbar = false,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Visibility = Visibility.Hidden // Ocultar en lugar de no mostrar
                        };

                        // Cargar la ventana
                        ventanaImpresion.Show();

                        // Esperar a que se cargue completamente
                        await Task.Delay(300);

                        // Imprimir
                        printDialog.PrintVisual(ventanaImpresion.BorderParaImpresion,
                            $"Horario - {horarioProfesor.NombreProfesor} - {periodoSeleccionado}");

                        ventanaImpresion.Close();
                    }
                    catch (Exception exHorario)
                    {
                        errores++;
                        Console.WriteLine($"Error al imprimir horario de {horarioProfesor.NombreProfesor}: {exHorario.Message}");
                    }
                }

                ventanaProgreso.Close();

                // Mostrar resumen
                MostrarResumenImpresion(horariosImpresos, horariosPorProfesor.Count, errores, cancelado);

                // Rehabilitar botón
                boton.IsEnabled = true;
                boton.Content = botonOriginalContent;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir todos los horarios: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Rehabilitar botón en caso de error
                if (sender is Button boton)
                {
                    boton.IsEnabled = true;
                    boton.Content = "IMPRIMIR TODO";
                }
            }
        }

        // **CORRECIÓN: Método auxiliar sin Owner**
        private Window CrearVentanaProgreso(int totalHorarios)
        {
            var ventana = new Window
            {
                Title = $"Imprimiendo {totalHorarios} horarios...",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen, // **Cambiado de CenterOwner**
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                ShowInTaskbar = false // **No mostrar en taskbar**
            };

            var stack = new StackPanel { Margin = new Thickness(20) };

            var txtProgreso = new TextBlock
            {
                Name = "txtProgreso",
                Text = $"Preparando impresión de {totalHorarios} horarios...",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            var progressBar = new ProgressBar
            {
                Name = "progressBar",
                Height = 20,
                Minimum = 0,
                Maximum = totalHorarios,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var btnCancelar = new Button
            {
                Name = "btnCancelar",
                Content = "Cancelar",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            stack.Children.Add(txtProgreso);
            stack.Children.Add(progressBar);
            stack.Children.Add(btnCancelar);

            ventana.Content = stack;
            return ventana;
        }

        private void ActualizarProgreso(Window ventana, int actual, int total, string nombreProfesor)
        {
            if (ventana.Content is StackPanel stack)
            {
                var txtProgreso = stack.Children.OfType<TextBlock>().FirstOrDefault(t => t.Name == "txtProgreso");
                var progressBar = stack.Children.OfType<ProgressBar>().FirstOrDefault(p => p.Name == "progressBar");

                if (txtProgreso != null)
                    txtProgreso.Text = $"Imprimiendo horario {actual} de {total}:\n{nombreProfesor}";

                if (progressBar != null)
                    progressBar.Value = actual;
            }
        }

        // Agrega este método si quieres usar la versión con ventana de progreso
        private void MostrarResumenImpresion(int impresos, int total, int errores, bool cancelado)
        {
            string mensaje = cancelado ?
                $"Impresión cancelada:\n{impresos} de {total} horarios impresos." :
                $"Impresión completada:\n{impresos} de {total} horarios impresos correctamente.";

            if (errores > 0)
            {
                mensaje += $"\n{errores} horarios tuvieron errores.";
            }

            MessageBox.Show(mensaje, "Impresión masiva completada",
                MessageBoxButton.OK,
                errores > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        private void DeseleccionarTodosLosHorarios()
        {
            for (int i = 0; i < itemsControlHorariosProfesores.Items.Count; i++)
            {
                var container = itemsControlHorariosProfesores.ItemContainerGenerator.ContainerFromIndex(i);
                if (container is ContentPresenter contentPresenter)
                {
                    var border = FindVisualChild<Border>(contentPresenter);
                    if (border != null && border.Name == "borderHorario")
                    {
                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F"));
                        border.BorderThickness = new Thickness(2);
                        border.Background = Brushes.White;
                    }
                }
            }
        }

        // Método para manejar clic en el horario
        private void BorderHorario_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            // Deseleccionar todos los horarios primero
            DeseleccionarTodosLosHorarios();

            // Seleccionar este horario
            border.BorderBrush = new SolidColorBrush(Colors.Red);
            border.BorderThickness = new Thickness(3);
            border.Background = new SolidColorBrush(Color.FromArgb(255, 255, 240, 240));

            // Obtener el horario seleccionado
            var contentPresenter = FindVisualParent<ContentPresenter>(border);
            if (contentPresenter != null)
            {
                horarioSeleccionado = contentPresenter.Content as HorarioTradicional;
            }

            // Mostrar instrucción
            txtInstruccion.Visibility = Visibility.Visible;
            txtInstruccion.Text = $"Seleccionado: {horarioSeleccionado?.NombreProfesor} - Listo para eliminar";
            txtInstruccion.Foreground = Brushes.Red;
        }

        private void ActualizarCardsPeriodos()
        {
            // Recargar periodos para actualizar los contadores
            CargarPeriodosConHorarios();
        }
    }   
}
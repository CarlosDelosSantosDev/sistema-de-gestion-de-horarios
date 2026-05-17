using SistemaHorarios.Models;
using SistemaHorarios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace SistemaHorarios
{
    public partial class HorarioView : UserControl
    {
        private HorariosService horariosService;
        private ProfesoresService profesoresService;
        private MateriasService materiasService;

        private object elementoArrastrado;
        private Dictionary<string, Horario> horariosTemporales = new Dictionary<string, Horario>();

        private bool modoRelleno = false;
        private Horario horarioParaRellenar;
        private Point posicionInicialRelleno;

        public HorarioView()
        {
            InitializeComponent();
            InicializarServicios();
            CargarDatosIniciales();
            CrearEstructuraHorario();
        }

        private void InicializarServicios()
        {
            horariosService = new HorariosService();
            profesoresService = new ProfesoresService();
            materiasService = new MateriasService();
        }

        private void CargarDatosIniciales()
        {
            try
            {
                cmbProfesores.ItemsSource = profesoresService.ObtenerTodosProfesores();
                lstMaterias.ItemsSource = materiasService.ObtenerTodasMaterias();

                if (cmbPeriodo.Items.Count > 0)
                    cmbPeriodo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbPeriodo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPeriodo.SelectedItem is ComboBoxItem periodoItem)
            {
                string periodo = periodoItem.Content.ToString();
                CargarGruposPorPeriodo(periodo);
            }
        }

        private void CargarGruposPorPeriodo(string periodo)
        {
            try
            {
                var grupos = horariosService.ObtenerTodosGruposOrdenados(periodo);

                if (grupos == null || grupos.Count == 0)
                {
                    MessageBox.Show("No se encontraron grupos para el periodo seleccionado", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    lstGrupos.ItemsSource = null;
                    return;
                }

                // ✅ DEFINIR la variable gradosDelPeriodo usando el servicio
                List<int> gradosDelPeriodo = horariosService.DeterminarGradosPorPeriodo(periodo);

                foreach (var grupo in grupos)
                {
                    // ✅ Ahora gradosDelPeriodo está definida
                    grupo.EsDelPeriodo = gradosDelPeriodo.Contains(grupo.Grado) ? "✅" : "";
                }

                lstGrupos.ItemsSource = grupos;

                // ✅ DEBUG FINAL
                Console.WriteLine($"=== DEBUG CargarGruposPorPeriodo ===");
                Console.WriteLine($"Periodo: {periodo}");
                Console.WriteLine($"Grados del periodo: {string.Join(", ", gradosDelPeriodo)}");
                Console.WriteLine($"Total grupos mostrados: {grupos.Count}");

                // ✅ Usar la variable ya definida
                var gruposDelPeriodoList = grupos.Where(g => gradosDelPeriodo.Contains(g.Grado)).ToList();
                var otrosGrupos = grupos.Where(g => !gradosDelPeriodo.Contains(g.Grado)).ToList();

                Console.WriteLine($"Grupos del periodo: {gruposDelPeriodoList.Count}");
                Console.WriteLine($"Otros grupos: {otrosGrupos.Count}");

                // Mostrar algunos ejemplos
                Console.WriteLine("--- Primeros 5 grupos del periodo ---");
                foreach (var grupo in gruposDelPeriodoList.Take(5))
                {
                    Console.WriteLine($"  {grupo.ClaveCompleta} | Grado: {grupo.Grado}");
                }

                Console.WriteLine("--- Primeros 5 otros grupos ---");
                foreach (var grupo in otrosGrupos.Take(5))
                {
                    Console.WriteLine($"  {grupo.ClaveCompleta} | Grado: {grupo.Grado}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Elemento_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem != null)
            {
                elementoArrastrado = listView.SelectedItem;
                MostrarInfoDebug(); // DEBUG
                DragDrop.DoDragDrop(listView, elementoArrastrado, DragDropEffects.Copy);
            }
        }

        // Agrega este diccionario para trackear conflictos
        private Dictionary<string, string> conflictosDetectados = new Dictionary<string, string>();


        private void CeldaHorario_Drop(object sender, DragEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag == null) return;

            dynamic tag = border.Tag;

            if (elementoArrastrado == null || cmbProfesores.SelectedItem == null || cmbPeriodo.SelectedItem == null)
            {
                MessageBox.Show("Selecciona profesor y periodo primero", "Datos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var profesor = cmbProfesores.SelectedItem as Profesor;
            string periodo = (cmbPeriodo.SelectedItem as ComboBoxItem).Content.ToString();
            string claveCelda = $"{tag.Dia}_{tag.HoraInicio}";

            if (horariosTemporales.ContainsKey(claveCelda))
            {
                MessageBox.Show("Ya hay un horario en esta celda", "Celda ocupada",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // CREAR HORARIO TEMPORAL PARA VALIDACIÓN
            Horario horario = new Horario
            {
                IdProfesor = profesor.NumeroTrabajador,
                DiaSemana = tag.Dia,
                HoraInicio = tag.HoraInicio,
                HoraFin = tag.HoraFin,
                Periodo = periodo,
                NombreProfesor = profesor.NombreCompleto,
                EsNP = false,
                // ✅ CALCULAR HORAS SEGÚN TURNO
                Horas = CalcularHorasPorClase(tag.HoraInicio, tag.HoraFin, tag.EsNocturno)
            };

            // ASIGNAR INFORMACIÓN
            if (elementoArrastrado is Materia materia)
            {
                horario.TextoMostrado = materia.Nombre;
                horario.IdMateria = materia.IdMateria;
                horario.ClaveMateria = materia.Clave;
                horario.EsMateria = true;
                horario.TipoElemento = "Materia";
            }
            else if (elementoArrastrado is Grupo grupo)
            {
                // ✅ INCLUIR EL TURNO Y MODALIDAD EN EL TEXTO MOSTRADO
                string turnoAbreviado = grupo.NombreTurno?.Substring(0, 1) ?? "";
                string modalidadAbreviada = (!string.IsNullOrEmpty(grupo.Modalidad) && grupo.Modalidad.ToUpper() == "EN LÍNEA")
                    ? "EL"
                    : "PR";

                string textoMostrado = $"{grupo.ClaveCarrera} {grupo.Grado}{grupo.Seccion ?? ""} {turnoAbreviado} {modalidadAbreviada}".Trim();

                horario.TextoMostrado = textoMostrado;
                horario.IdGrupo = grupo.IdGrupo;
                horario.ClaveCarrera = grupo.ClaveCarrera;
                horario.Grado = grupo.Grado;
                horario.Seccion = grupo.Seccion;
                horario.Turno = turnoAbreviado;
                horario.Modalidad = grupo.Modalidad; // ✅ GUARDAR LA MODALIDAD COMPLETA
                horario.EsMateria = false;
                horario.TipoElemento = "Grupo";
            }

            // Resto del código igual...
            string mensajeError = ValidarHorarioAntesDeAgregar(horario);

            if (!string.IsNullOrEmpty(mensajeError))
            {
                MostrarCeldaConConflicto(border, horario, mensajeError);
                conflictosDetectados[claveCelda] = mensajeError;
                MessageBox.Show($"❌ CONFLICTO DETECTADO:\n\n{mensajeError}",
                    "Conflicto de Horario", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MostrarHorarioEnCelda(border, horario);
            horariosTemporales[claveCelda] = horario;

            if (conflictosDetectados.ContainsKey(claveCelda))
                conflictosDetectados.Remove(claveCelda);

            ActualizarContadorHoras();
        }

        // Método para validar horario antes de agregar
        private string ValidarHorarioAntesDeAgregar(Horario horario)
        {
            try
            {
                // Validar conflicto de materia en el mismo periodo
                if (horario.TipoElemento == "Materia" && horario.IdMateria > 0)
                {
                    bool hayConflictoMateria = !horariosService.ValidarConflictoMateriaPeriodo(horario);
                    if (hayConflictoMateria)
                    {
                        string infoConflicto = horariosService.ObtenerInfoConflicto(horario);
                        return $"MATERIA YA OCUPADA:\n{infoConflicto}";
                    }
                }

                // Validar conflicto de grupo en el mismo periodo
                if (horario.TipoElemento == "Grupo" && horario.IdGrupo > 0)
                {
                    bool hayConflictoGrupo = !horariosService.ValidarConflictoGrupoPeriodo(horario);
                    if (hayConflictoGrupo)
                    {
                        string infoConflicto = horariosService.ObtenerInfoConflicto(horario);
                        return $"GRUPO YA OCUPADO:\n{infoConflicto}";
                    }
                }

                // Validar horas del profesor
                if (!ValidarHorasAntesDeAgregar(horario))
                {
                    return "El profesor excede las 30 horas permitidas";
                }

                return null; // No hay conflictos
            }
            catch (Exception ex)
            {
                return $"Error en validación: {ex.Message}";
            }
        }

        // Método para mostrar celda con conflicto (en ROJO)
        private void MostrarCeldaConConflicto(Border border, Horario horario, string mensajeError)
        {
            var textBlock = new TextBlock
            {
                Text = $"{horario.TextoMostrado}\n🚫",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White
            };

            border.Child = textBlock;
            border.Background = new SolidColorBrush(Colors.Red);
            border.BorderBrush = Brushes.DarkRed;
            border.BorderThickness = new Thickness(2);
            border.ToolTip = $"🚫 CONFLICTO:\n{mensajeError}";
        }

        private void MostrarHorarioEnCelda(Border border, Horario horario)
        {
            var textBlock = new TextBlock
            {
                Text = horario.TextoMostrado,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };

            Color color = horario.EsMateria ? Colors.LightBlue : Colors.LightGreen;

            border.Child = textBlock;
            border.Background = new SolidColorBrush(color);
            border.ToolTip = $"{horario.DiaSemana} {horario.HoraInicio:hh\\:mm}-{horario.HoraFin:hh\\:mm}\n{horario.TextoMostrado}";
        }

        private void CeldaHorario_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Child != null && border.Tag != null)
            {
                var contextMenu = new ContextMenu();
                dynamic tag = border.Tag;
                string claveCelda = $"{tag.Dia}_{tag.HoraInicio}";

                var menuEliminar = new MenuItem { Header = "❌ Eliminar" };
                menuEliminar.Click += (s, args) => EliminarHorario(border, claveCelda);

                var menuNP = new MenuItem { Header = "⚠️ Marcar como NP" };
                menuNP.Click += (s, args) => MarcarComoNP(border, claveCelda);

                contextMenu.Items.Add(menuEliminar);
                contextMenu.Items.Add(menuNP);

                border.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }

        private string ObtenerClaveDesdeTag(dynamic tag)
        {
            // Usa exactamente la misma interpolación que en CeldaHorario_Drop / CrearHorario
            // tag.HoraInicio puede ser TimeSpan, tostring mantiene la igualdad si usas la misma expresión
            return $"{tag.Dia}_{tag.HoraInicio}";
        }


        private void EliminarHorario(Border border, string claveCelda)
        {
            try
            {
                // --- 1) Si no recibiste la clave por parámetro, intenta obtenerla desde el tag
                if (string.IsNullOrEmpty(claveCelda) && border?.Tag != null)
                {
                    dynamic tag = border.Tag;
                    claveCelda = ObtenerClaveDesdeTag(tag);
                }

                // --- 2) Eliminar del diccionario temporal (si existe)
                if (!string.IsNullOrEmpty(claveCelda) && horariosTemporales.ContainsKey(claveCelda))
                {
                    horariosTemporales.Remove(claveCelda);
                }

                // --- 3) Eliminar cualquier registro de conflictos para esa clave
                if (!string.IsNullOrEmpty(claveCelda) && conflictosDetectados.ContainsKey(claveCelda))
                {
                    conflictosDetectados.Remove(claveCelda);
                }

                // --- 4) Limpiar visualmente la celda
                if (border != null)
                {
                    border.Child = null;
                    border.ToolTip = null;

                    // Restaurar color original según el tag
                    if (border.Tag != null)
                    {
                        try
                        {
                            dynamic tag = border.Tag;
                            border.Background = tag.EsNocturno ? Brushes.WhiteSmoke : Brushes.White;

                            // Normalizar Tag para que solo contenga los campos mínimos
                            border.Tag = new
                            {
                                Dia = tag.Dia,
                                HoraInicio = tag.HoraInicio,
                                HoraFin = tag.HoraFin,
                                EsNocturno = tag.EsNocturno
                            };
                        }
                        catch
                        {
                            // Si tag no tiene los campos esperados, asigna un tag mínimo
                            border.Tag = null;
                            border.Background = Brushes.White;
                        }
                    }
                    else
                    {
                        border.Background = Brushes.White;
                    }

                    // Restaurar bordes (opcional)
                    border.BorderBrush = Brushes.LightGray;
                    border.BorderThickness = new Thickness(0.5);
                }

                // --- 5) Forzar actualización de contadores / UI
                ActualizarContadorHoras();

                // Si tienes alguna lista o vista que dependa de horariosTemporales, refrescarla aquí.
                // Ejemplo: itemsControlHorariosProfesores.Items.Refresh(); // si aplica

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la hora: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void MarcarComoNP(Border border, string claveCelda)
        {
            if (horariosTemporales.ContainsKey(claveCelda))
            {
                var horario = horariosTemporales[claveCelda];

                // Agregar o quitar NP
                if (horario.TextoMostrado.Contains(" - NP"))
                {
                    horario.TextoMostrado = horario.TextoMostrado.Replace(" - NP", "");
                    horario.EsNP = false;
                    // ✅ RESTAURAR HORAS CUANDO SE QUITA NP
                    horario.Horas = CalcularHorasPorClase(horario.HoraInicio, horario.HoraFin, horario.EsNocturno);
                }
                else
                {
                    horario.TextoMostrado += " - NP";
                    horario.EsNP = true;
                    // ✅ LAS CLASES NP NO CUENTAN HORAS
                    horario.Horas = 1;
                }

                // Actualizar visualización
                MostrarHorarioEnCelda(border, horario);

                // ACTUALIZAR CONTADOR AL CAMBIAR NP
                
            }
        }

        // Modifica el método BtnGuardar_Click para validar ANTES de guardar
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProfesores.SelectedItem == null || cmbPeriodo.SelectedItem == null)
            {
                MessageBox.Show("Selecciona profesor y periodo primero", "Datos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (horariosTemporales.Count == 0)
            {
                MessageBox.Show("No hay horarios para guardar", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ✅ VALIDAR QUE NO HAYA CONFLICTOS ANTES DE GUARDAR
            if (conflictosDetectados.Count > 0)
            {
                string mensajeConflictos = "❌ NO SE PUEDE GUARDAR - CONFLICTOS DETECTADOS:\n\n";
                foreach (var conflicto in conflictosDetectados)
                {
                    mensajeConflictos += $"{conflicto.Key}: {conflicto.Value}\n\n";
                }

                MessageBox.Show(mensajeConflictos, "Conflictos Pendientes",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var profesor = cmbProfesores.SelectedItem as Profesor;
            string periodo = (cmbPeriodo.SelectedItem as ComboBoxItem).Content.ToString();

            // PRIMERO: Eliminar horarios anteriores del mismo profesor/periodo
            if (!horariosService.EliminarHorariosProfesorPeriodo(profesor.NumeroTrabajador, periodo))
            {
                MessageBox.Show("Error al limpiar horarios anteriores", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // VALIDAR CADA HORARIO ANTES DE GUARDAR
            int horariosGuardados = 0;
            int errores = 0;

            foreach (var kvp in horariosTemporales)
            {
                var horario = kvp.Value;
                horario.IdProfesor = profesor.NumeroTrabajador;
                horario.Periodo = periodo;
                horario.NombreProfesor = profesor.NombreCompleto;

                // Validación final antes de guardar
                string validacion = ValidarHorarioAntesDeAgregar(horario);
                if (!string.IsNullOrEmpty(validacion))
                {
                    MessageBox.Show($"Error en {kvp.Key}: {validacion}", "Error de Validación",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    errores++;
                    continue;
                }

                if (horariosService.ValidarYGuardarHorario(horario))
                {
                    horariosGuardados++;
                }
                else
                {
                    errores++;
                }
            }

            if (horariosGuardados > 0)
            {
                MessageBox.Show($"✅ Horario completo guardado para:\n{profesor.NombreCompleto}\nPeriodo: {periodo}\nClases: {horariosGuardados}",
                    "Horario Guardado", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarHorarioActual();
            }

            if (errores > 0)
            {
                MessageBox.Show($"{errores} clases tuvieron errores", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCrearOtroHorario_Click(object sender, RoutedEventArgs e)
        {
            if (horariosTemporales.Count > 0)
            {
                var result = MessageBox.Show("¿Estás seguro de que quieres crear un nuevo horario? Se perderán los cambios no guardados.",
                                            "Nuevo Horario",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            LimpiarHorarioActual();
            MessageBox.Show("✅ Listo para crear un nuevo horario", "Nuevo Horario",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LimpiarHorarioActual()
        {
            // Limpiar selecciones
            cmbProfesores.SelectedIndex = -1;
            if (txtNombreHorario != null)
                txtNombreHorario.Text = "";

            // Limpiar horarios temporales
            horariosTemporales.Clear();

            // Limpiar conflictos
            conflictosDetectados.Clear();

            // Cancelar modo relleno si está activo
            if (modoRelleno)
            {
                CancelarModoRelleno();
            }

            // Limpiar grid
            if (gridHorario != null)
            {
                foreach (var child in gridHorario.Children)
                {
                    if (child is Border border && border.Tag != null)
                    {
                        border.Child = null;
                        dynamic tag = border.Tag;
                        border.Background = tag.EsNocturno ? Brushes.WhiteSmoke : Brushes.White;
                        border.BorderBrush = Brushes.LightGray;
                        border.BorderThickness = new Thickness(0.5);
                        border.ToolTip = null;
                    }
                }
            }

            // Limpiar selecciones de listas
            lstMaterias.SelectedItem = null;
            lstGrupos.SelectedItem = null;
            elementoArrastrado = null;
            modoRelleno = false;
            horarioParaRellenar = null;

            // ✅ ACTUALIZAR CONTADOR A CERO
            ActualizarContadorHoras();
        }

        private void CmbProfesores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProfesores.SelectedItem is Profesor profesor)
            {
                if (txtNombreHorario != null)
                    txtNombreHorario.Text = profesor.NombreCompleto;

                // ✅ ACTUALIZAR CONTADOR (solo horas temporales actuales)
                ActualizarContadorHoras();
            }
            else
            {
                if (txtNombreHorario != null)
                    txtNombreHorario.Text = "";
                if (txtInfoHoras != null)
                    txtInfoHoras.Text = "Horas asignadas: 0/30";
            }
        }

        // Métodos de estructura del horario (deben estar implementados)
        private void CrearEstructuraHorario()
        {
            if (gridHorario == null) return;

            gridHorario.Children.Clear();
            gridHorario.RowDefinitions.Clear();
            gridHorario.ColumnDefinitions.Clear();

            string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };

            // Columnas
            gridHorario.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
            foreach (var dia in dias)
            {
                gridHorario.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Encabezados
            gridHorario.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            for (int i = 0; i < dias.Length; i++)
            {
                var label = new Label
                {
                    Content = dias[i],
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F")),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetColumn(label, i + 1);
                Grid.SetRow(label, 0);
                gridHorario.Children.Add(label);
            }

            int fila = 1;

            // Turno diurno (7:30-20:00)
            TimeSpan hora = new TimeSpan(7, 30, 0);
            while (hora < new TimeSpan(20, 0, 0))
            {
                AgregarFilaHorario(hora, hora.Add(TimeSpan.FromMinutes(50)), fila++, dias, false);
                hora = hora.Add(TimeSpan.FromMinutes(50));
            }

            // Turno nocturno (18:30-23:00)
            hora = new TimeSpan(18, 30, 0);
            while (hora < new TimeSpan(23, 0, 0))
            {
                AgregarFilaHorario(hora, hora.Add(TimeSpan.FromMinutes(45)), fila++, dias, true);
                hora = hora.Add(TimeSpan.FromMinutes(45));
            }
        }

        private void AgregarFilaHorario(TimeSpan inicio, TimeSpan fin, int fila, string[] dias, bool esNocturno)
        {
            gridHorario.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45) });

            // Label de hora
            var labelHora = new Label
            {
                Content = $"{inicio:hh\\:mm} - {fin:hh\\:mm}",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = esNocturno ? Brushes.LightGray : Brushes.LightBlue,
                FontWeight = FontWeights.Bold,
                FontSize = 10
            };
            Grid.SetColumn(labelHora, 0);
            Grid.SetRow(labelHora, fila);
            gridHorario.Children.Add(labelHora);

            // Celdas
            for (int col = 1; col <= dias.Length; col++)
            {
                var border = CrearCeldaHorario(dias[col - 1], fila, inicio, fin, esNocturno);
                Grid.SetColumn(border, col);
                Grid.SetRow(border, fila);
                gridHorario.Children.Add(border);
            }
        }

        private Border CrearCeldaHorario(string dia, int fila, TimeSpan inicio, TimeSpan fin, bool esNocturno)
        {
            var border = new Border
            {
                Background = esNocturno ? Brushes.WhiteSmoke : Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5),
                Tag = new { Dia = dia, Fila = fila, HoraInicio = inicio, HoraFin = fin, EsNocturno = esNocturno }
            };

            border.AllowDrop = true;
            border.Drop += CeldaHorario_Drop;
            border.MouseEnter += CeldaHorario_MouseEnter;
            border.MouseLeave += CeldaHorario_MouseLeave;
            border.MouseRightButtonUp += CeldaHorario_MouseRightButtonUp;

            // NUEVOS EVENTOS PARA MODO RELLENO
            border.PreviewMouseLeftButtonDown += CeldaHorario_PreviewMouseLeftButtonDown;
            border.MouseMove += CeldaHorario_MouseMove;

            return border;
        }

        // Métodos auxiliares para eventos de mouse
        private void CeldaHorario_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && elementoArrastrado != null)
            {
                var border = sender as Border;
                if (border.Child == null)
                    border.Background = Brushes.LightYellow;
            }
        }

        private void CeldaHorario_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border.Child == null)
            {
                dynamic tag = border.Tag;
                border.Background = tag.EsNocturno ? Brushes.WhiteSmoke : Brushes.White;
            }
        }

        private void ActualizarContadorHoras()
        {
            if (cmbProfesores.SelectedItem == null || cmbPeriodo.SelectedItem == null)
                return;

            // ✅ SOLO CONTAR HORAS TEMPORALES DEL HORARIO ACTUAL
            double horasTemporales = horariosTemporales.Values
                .Where(h => !h.EsNP)
                .Sum(h => h.Horas);

            double totalHoras = horasTemporales;

            if (txtInfoHoras != null)
                txtInfoHoras.Text = $"Horas asignadas: {totalHoras:F1}/30";

            // Cambiar color según las horas
            if (totalHoras >= 30)
                txtInfoHoras.Foreground = Brushes.Red;
            else if (totalHoras >= 25)
                txtInfoHoras.Foreground = Brushes.Orange;
            else
                txtInfoHoras.Foreground = Brushes.Gray;
        }

        private bool ValidarHorasAntesDeAgregar(Horario nuevoHorario)
        {
            if (cmbProfesores.SelectedItem == null || cmbPeriodo.SelectedItem == null)
                return false;

            // ✅ SOLO CONTAR HORAS TEMPORALES DEL HORARIO ACTUAL
            double horasTemporales = horariosTemporales.Values
                .Where(h => !h.EsNP)
                .Sum(h => h.Horas);

            double totalHoras = horasTemporales + nuevoHorario.Horas;

            if (totalHoras > 30 && !nuevoHorario.EsNP)
            {
                MessageBox.Show($"❌ El profesor excede las 30 horas permitidas.\nHoras actuales en este horario: {horasTemporales:F1}\nHoras nuevas: {nuevoHorario.Horas:F1}\nTotal: {totalHoras:F1}",
                    "Límite de Horas Excedido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private double CalcularHorasPorClase(TimeSpan horaInicio, TimeSpan horaFin, bool esNocturno)
        {
            if (esNocturno)
            {
                // Turno nocturno: 45 minutos = 0.75 horas
                return 1.0
                    ;
            }
            else
            {
                // Turno diurno: 50 minutos = 0.833 horas
                return 1.0;
            }
        }

        private void MostrarInfoDebug()
        {
            if (elementoArrastrado is Materia materia)
            {
                Console.WriteLine($"Materia arrastrada: ID={materia.IdMateria}, Nombre={materia.Nombre}");
            }
            else if (elementoArrastrado is Grupo grupo)
            {
                Console.WriteLine($"Grupo arrastrado: ID={grupo.IdGrupo}, Nombre={grupo.ClaveCompleta}");
            }
        }

        public void CargarHorarioParaEdicion(Horario horarioSeleccionado)
        {
            try
            {
                // Limpiar horario actual
                LimpiarHorarioActual();

                // Configurar profesor
                var profesor = cmbProfesores.Items.OfType<Profesor>()
                    .FirstOrDefault(p => p.NumeroTrabajador == horarioSeleccionado.IdProfesor);

                if (profesor != null)
                {
                    cmbProfesores.SelectedItem = profesor;
                }

                // Configurar periodo
                var periodoItem = cmbPeriodo.Items.OfType<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == horarioSeleccionado.Periodo);

                if (periodoItem != null)
                {
                    cmbPeriodo.SelectedItem = periodoItem;
                }

                // Cargar todos los horarios del profesor en ese periodo
                CargarHorariosCompletosParaEdicion(horarioSeleccionado.IdProfesor, horarioSeleccionado.Periodo);

                MessageBox.Show($"Horario cargado para edición: {horarioSeleccionado.NombreProfesor} - {horarioSeleccionado.Periodo}",
                               "Edición", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar para edición: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarHorariosCompletosParaEdicion(int idProfesor, string periodo)
        {
            try
            {
                // Obtener todos los horarios del profesor en el periodo
                var horariosCompletos = horariosService.ObtenerHorariosPorProfesorYPeriodo(idProfesor, periodo);

                foreach (var horario in horariosCompletos)
                {
                    string claveCelda = $"{horario.DiaSemana}_{horario.HoraInicio}";

                    // Buscar la celda en el grid
                    var border = EncontrarCeldaPorDiaYHora(horario.DiaSemana, horario.HoraInicio);

                    if (border != null)
                    {
                        // Configurar la información para mostrar
                        if (horario.IdMateria > 0)
                        {
                            // Es materia
                            var materia = materiasService.ObtenerMateriaPorId(horario.IdMateria);
                            if (materia != null)
                            {
                                horario.TextoMostrado = materia.Nombre;
                                horario.EsMateria = true;
                                horario.TipoElemento = "Materia";
                            }
                        }
                        else if (horario.IdGrupo > 0)
                        {
                            // Es grupo
                            var grupo = horariosService.ObtenerGrupoPorId(horario.IdGrupo);
                            if (grupo != null)
                            {
                                // ✅ INCLUIR EL TURNO Y MODALIDAD EN EL TEXTO MOSTRADO
                                string turnoAbreviado = grupo.NombreTurno?.Substring(0, 1) ?? "";
                                string modalidadAbreviada = (!string.IsNullOrEmpty(grupo.Modalidad) && grupo.Modalidad.ToUpper() == "EN LÍNEA")
                                    ? "EL"
                                    : "PR";

                                string textoMostrado = $"{grupo.ClaveCarrera} {grupo.Grado}{grupo.Seccion ?? ""} {turnoAbreviado} {modalidadAbreviada}".Trim();

                                horario.TextoMostrado = textoMostrado;
                                horario.EsMateria = false;
                                horario.TipoElemento = "Grupo";
                            }
                        }

                        // Manejar NP
                        if (horario.EsNP)
                        {
                            horario.TextoMostrado += " - NP";
                        }

                        // Mostrar en celda
                        MostrarHorarioEnCelda(border, horario);

                        // Agregar a horarios temporales
                        horariosTemporales[claveCelda] = horario;
                    }
                }

                // Actualizar contador
                ActualizarContadorHoras();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar horarios completos: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border EncontrarCeldaPorDiaYHora(string dia, TimeSpan horaInicio)
        {
            foreach (var child in gridHorario.Children)
            {
                if (child is Border border && border.Tag != null)
                {
                    try
                    {
                        dynamic tag = border.Tag;
                        if (tag.Dia == dia && tag.HoraInicio == horaInicio)
                        {
                            return border;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return null;
        }

        // Manejar doble click en materias y grupos
        // Manejar doble click en materias y grupos
        private void ListViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var listViewItem = sender as ListViewItem;
                if (listViewItem?.Content != null)
                {
                    elementoArrastrado = listViewItem.Content;
                    modoRelleno = true;
                    horarioParaRellenar = CrearHorarioDesdeElemento(elementoArrastrado);

                    if (horarioParaRellenar != null)
                    {
                        // Cambiar cursor para indicar modo relleno
                        Mouse.OverrideCursor = Cursors.Cross;

                        // Mostrar mensaje informativo
                        string tipoElemento = horarioParaRellenar.EsMateria ? "Materia" : "Grupo";
                        MessageBox.Show($"Modo relleno activado para: {horarioParaRellenar.TextoMostrado}\n\n" +
                                       $"Haz clic en una celda para colocar, o arrastra sobre múltiples celdas.\n" +
                                       $"Presiona ESC o Click Derecho para cancelar.",
                                      $"Modo Relleno - {tipoElemento}",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);

                        Console.WriteLine($"Modo relleno activado: {horarioParaRellenar.TextoMostrado}");
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al activar modo relleno: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Horario CrearHorarioDesdeElemento(object elemento)
        {
            if (cmbProfesores.SelectedItem == null || cmbPeriodo.SelectedItem == null)
            {
                MessageBox.Show("Primero selecciona un profesor y periodo", "Datos incompletos",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                var profesor = cmbProfesores.SelectedItem as Profesor;
                string periodo = (cmbPeriodo.SelectedItem as ComboBoxItem).Content.ToString();

                Horario horario = new Horario
                {
                    IdProfesor = profesor.NumeroTrabajador,
                    Periodo = periodo,
                    NombreProfesor = profesor.NombreCompleto,
                    EsNP = false
                };

                if (elemento is Materia materia)
                {
                    horario.TextoMostrado = materia.Nombre;
                    horario.IdMateria = materia.IdMateria;
                    horario.ClaveMateria = materia.Clave;
                    horario.EsMateria = true;
                    horario.TipoElemento = "Materia";
                }
                else if (elemento is Grupo grupo)
                {
                    // ✅ INCLUIR EL TURNO Y MODALIDAD EN EL TEXTO MOSTRADO
                    string turnoAbreviado = grupo.NombreTurno?.Substring(0, 1) ?? "";
                    string modalidadAbreviada = (!string.IsNullOrEmpty(grupo.Modalidad) && grupo.Modalidad.ToUpper() == "EN LÍNEA")
                        ? "EL"
                        : "PR";

                    string textoMostrado = $"{grupo.ClaveCarrera} {grupo.Grado}{grupo.Seccion ?? ""} {turnoAbreviado} {modalidadAbreviada}".Trim();

                    horario.TextoMostrado = textoMostrado;
                    horario.IdGrupo = grupo.IdGrupo;
                    horario.ClaveCarrera = grupo.ClaveCarrera;
                    horario.Grado = grupo.Grado;
                    horario.Seccion = grupo.Seccion;
                    horario.Turno = turnoAbreviado;
                    horario.Modalidad = grupo.Modalidad; // ✅ GUARDAR LA MODALIDAD COMPLETA
                    horario.EsMateria = false;
                    horario.TipoElemento = "Grupo";
                }
                else
                {
                    return null;
                }

                return horario;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear horario: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void CeldaHorario_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (modoRelleno && horarioParaRellenar != null)
            {
                // Iniciar modo relleno
                var border = sender as Border;
                if (border?.Tag != null)
                {
                    posicionInicialRelleno = e.GetPosition(gridHorario);
                    RellenarCeldaIndividual(border);
                }
                e.Handled = true;
            }
        }

        private void RellenarCeldaIndividual(Border border)
        {
            if (border?.Tag == null || horarioParaRellenar == null) return;

            dynamic tag = border.Tag;
            string claveCelda = $"{tag.Dia}_{tag.HoraInicio}";

            // Verificar si ya existe un horario en esta celda
            if (horariosTemporales.ContainsKey(claveCelda))
            {
                return; // No sobreescribir celdas existentes
            }

            // Crear copia del horario para esta celda
            Horario horario = new Horario
            {
                IdProfesor = horarioParaRellenar.IdProfesor,
                DiaSemana = tag.Dia,
                HoraInicio = tag.HoraInicio,
                HoraFin = tag.HoraFin,
                Periodo = horarioParaRellenar.Periodo,
                NombreProfesor = horarioParaRellenar.NombreProfesor,
                EsNP = horarioParaRellenar.EsNP,
                Horas = CalcularHorasPorClase(tag.HoraInicio, tag.HoraFin, tag.EsNocturno),

                // Copiar información específica del elemento
                TextoMostrado = horarioParaRellenar.TextoMostrado,
                IdMateria = horarioParaRellenar.IdMateria,
                ClaveMateria = horarioParaRellenar.ClaveMateria,
                IdGrupo = horarioParaRellenar.IdGrupo,
                ClaveCarrera = horarioParaRellenar.ClaveCarrera,
                Grado = horarioParaRellenar.Grado,
                Seccion = horarioParaRellenar.Seccion,
                Turno = horarioParaRellenar.Turno,
                EsMateria = horarioParaRellenar.EsMateria,
                TipoElemento = horarioParaRellenar.TipoElemento
            };

            // Validar antes de agregar
            string mensajeError = ValidarHorarioAntesDeAgregar(horario);

            if (!string.IsNullOrEmpty(mensajeError))
            {
                MostrarCeldaConConflicto(border, horario, mensajeError);
                conflictosDetectados[claveCelda] = mensajeError;
                return;
            }

            // Mostrar y guardar horario
            MostrarHorarioEnCelda(border, horario);
            horariosTemporales[claveCelda] = horario;

            if (conflictosDetectados.ContainsKey(claveCelda))
                conflictosDetectados.Remove(claveCelda);

            ActualizarContadorHoras();
        }

        // Método para rellenar múltiples celdas durante el arrastre
        private void CeldaHorario_MouseMove(object sender, MouseEventArgs e)
        {
            if (modoRelleno && e.LeftButton == MouseButtonState.Pressed && horarioParaRellenar != null)
            {
                var border = sender as Border;
                if (border?.Tag != null)
                {
                    RellenarCeldaIndividual(border);
                }
            }
        }

        private void CancelarModoRelleno()
        {
            modoRelleno = false;
            horarioParaRellenar = null;
            Mouse.OverrideCursor = null;
        }

        // Cancelar modo relleno con ESC o click derecho
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && modoRelleno)
            {
                CancelarModoRelleno();
                MessageBox.Show("Modo relleno cancelado.", "Información",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GridHorario_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (modoRelleno)
            {
                CancelarModoRelleno();
                e.Handled = true;
            }
        }

        private void VerificarConexionEventos()
        {
            Console.WriteLine("Verificando eventos...");

            // Verificar si los ListView tienen items
            if (lstMaterias.Items.Count > 0)
                Console.WriteLine($"lstMaterias tiene {lstMaterias.Items.Count} items");
            else
                Console.WriteLine("lstMaterias está vacío");

            if (lstGrupos.Items.Count > 0)
                Console.WriteLine($"lstGrupos tiene {lstGrupos.Items.Count} items");
            else
                Console.WriteLine("lstGrupos está vacío");
        }
    }
}
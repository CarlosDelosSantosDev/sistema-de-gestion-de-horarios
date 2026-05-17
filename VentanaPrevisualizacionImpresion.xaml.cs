using SistemaHorarios.Models;
using SistemaHorarios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SistemaHorarios
{
    public partial class VentanaPrevisualizacionImpresion : Window
    {
        private HorariosGuardadosView.HorarioTradicional _horarioSeleccionado;
        private string _periodoSeleccionado;
        public Border BorderParaImpresion => borderImprimible;

        public VentanaPrevisualizacionImpresion(HorariosGuardadosView.HorarioTradicional horario, string periodo)
        {
            InitializeComponent();
            _horarioSeleccionado = horario;
            _periodoSeleccionado = periodo;

            this.Width = 1056;
            this.Height = 816;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                GenerarFormatoCartaHorizontal();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void GenerarFormatoCartaHorizontal()
        {
            try
            {
                gridContenidoPrincipal.Children.Clear();
                gridContenidoPrincipal.RowDefinitions.Clear();
                gridContenidoPrincipal.ColumnDefinitions.Clear();

                // CONFIGURACIÓN MEJORADA - ESTRUCTURA VERTICAL COMPLETA
                gridContenidoPrincipal.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Encabezado
                gridContenidoPrincipal.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Títulos
                gridContenidoPrincipal.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Contenedor principal (horario + grupos)
                gridContenidoPrincipal.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Espacio final

                // SOLO 1 COLUMNA - ESTRUCTURA VERTICAL
                gridContenidoPrincipal.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                int currentRow = 0;

                // ENCABEZADO
                var encabezado = CrearEncabezadoConLogos();
                Grid.SetRow(encabezado, currentRow);
                Grid.SetColumn(encabezado, 0);
                gridContenidoPrincipal.Children.Add(encabezado);
                currentRow++;

                // TÍTULOS
                var titulos = CrearTitulos();
                Grid.SetRow(titulos, currentRow);
                Grid.SetColumn(titulos, 0);
                gridContenidoPrincipal.Children.Add(titulos);
                currentRow++;

                // CONTENEDOR PRINCIPAL QUE SE EXPANDE (horario + grupos + firmas)
                var contenedorPrincipal = CrearContenedorPrincipal();
                Grid.SetRow(contenedorPrincipal, currentRow);
                Grid.SetColumn(contenedorPrincipal, 0);
                gridContenidoPrincipal.Children.Add(contenedorPrincipal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar formato: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Grid CrearContenedorPrincipal()
        {
            var gridContenedor = new Grid();

            // 2 COLUMNAS: 70% horario + grupos, 30% firmas
            gridContenedor.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Star) });
            gridContenedor.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30, GridUnitType.Star) });

            // 2 FILAS: horario y grupos
            gridContenedor.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Horario (auto)
            gridContenedor.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Grupos (auto)

            // HORARIO (fila 0, columna 0)
            var horarioGrid = CrearTablaHorarioCompleta();
            Grid.SetRow(horarioGrid, 0);
            Grid.SetColumn(horarioGrid, 0);
            gridContenedor.Children.Add(horarioGrid);

            // TABLA DE GRUPOS (fila 1, columna 0) - EXACTAMENTE a 5px del horario
            var tablaGrupos = CrearTablaGrupos();
            if (tablaGrupos != null)
            {
                tablaGrupos.Margin = new Thickness(30, 5, 30, 0); // 5px de separación arriba
                tablaGrupos.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(tablaGrupos, 1);
                Grid.SetColumn(tablaGrupos, 0);
                gridContenedor.Children.Add(tablaGrupos);
            }

            // FIRMAS (columna 1, span 2 filas) - alineada al inicio
            var firmas = CrearSeccionFirmas();
            firmas.VerticalAlignment = VerticalAlignment.Top;
            firmas.Margin = new Thickness(20, 0, 20, 0);
            Grid.SetRow(firmas, 0);
            Grid.SetColumn(firmas, 1);
            Grid.SetRowSpan(firmas, 2);
            gridContenedor.Children.Add(firmas);

            return gridContenedor;
        }
        private Grid CrearEncabezadoConLogos()
        {
            var gridEncabezado = new Grid();
            gridEncabezado.Margin = new Thickness(40, 5, 40, 10); // Reducir márgenes verticales
            gridEncabezado.Height = 110; // Reducir altura

            gridEncabezado.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(160) });
            gridEncabezado.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            gridEncabezado.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(160) });

            // Logo izquierda
            try
            {
                var imagenIzquierda = new Image
                {
                    Source = CargarImagenDesdeRecursos("logo_izquierda.jpg"),
                    Width = 140, // Reducir tamaño
                    Height = 110, // Reducir tamaño
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 10, 0)
                };
                Grid.SetColumn(imagenIzquierda, 0);
                gridEncabezado.Children.Add(imagenIzquierda);
            }
            catch
            {
                var placeholder = CrearTextBlock("[LOGO IZQ]", FontWeights.Normal, 18, TextAlignment.Center);
                placeholder.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(placeholder, 0);
                gridEncabezado.Children.Add(placeholder);
            }

            // Título - más compacto
            var titulo = CrearTextBlock("FORMATO DE ENTREGA DE HORARIO A DOCENTES", FontWeights.Bold, 18, TextAlignment.Center); // Reducir tamaño
            titulo.VerticalAlignment = VerticalAlignment.Center;
            titulo.Margin = new Thickness(15, 0, 15, 0); // Reducir márgenes
            Grid.SetColumn(titulo, 1);
            gridEncabezado.Children.Add(titulo);

            // Logo derecha
            try
            {
                var imagenDerecha = new Image
                {
                    Source = CargarImagenDesdeRecursos("logo_derecha.jpg"),
                    Width = 140, // Reducir tamaño
                    Height = 110, // Reducir tamaño
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 10, 0)
                };
                Grid.SetColumn(imagenDerecha, 2);
                gridEncabezado.Children.Add(imagenDerecha);
            }
            catch
            {
                var placeholder = CrearTextBlock("[LOGO DER]", FontWeights.Normal, 18, TextAlignment.Center);
                placeholder.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(placeholder, 2);
                gridEncabezado.Children.Add(placeholder);
            }

            return gridEncabezado;
        }

        private StackPanel CrearTitulos()
        {
            var stackTitulos = new StackPanel();
            stackTitulos.Margin = new Thickness(0, 0, 0, 5); // Reducir margen inferior
            stackTitulos.HorizontalAlignment = HorizontalAlignment.Center;

            string periodo = !string.IsNullOrEmpty(_periodoSeleccionado) ? _periodoSeleccionado.ToUpper() : "SEPTIEMBRE - DICIEMBRE 2025";
            var txtPeriodo = CrearTextBlock(periodo, FontWeights.Bold, 14, TextAlignment.Center); // Reducir tamaño
            txtPeriodo.Margin = new Thickness(0, 0, 0, 2); // Reducir margen
            stackTitulos.Children.Add(txtPeriodo);

            return stackTitulos;
        }

        private Grid CrearTablaHorarioCompleta()
        {
            string[] dias = { "LUNES", "MARTES", "MIÉRCOLES", "JUEVES", "VIERNES", "SÁBADO" };
            string[] horariosMostrar = ObtenerHorariosPorTurnosUsados();

            var gridTabla = new Grid();
            gridTabla.Margin = new Thickness(20, 0, 10, 5); // Menos margen derecho para más espacio
            gridTabla.HorizontalAlignment = HorizontalAlignment.Left;
            gridTabla.VerticalAlignment = VerticalAlignment.Top;

            // COLUMNAS MÁS EQUILIBRADAS
            gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) }); // Horas
            for (int i = 0; i < dias.Length; i++)
            {
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(95) }); // Días
            }

            // CALCULAR ANCHO TOTAL APROXIMADO PARA VER SI QUEPA
            double anchoTotalAprox = 100 + (95 * dias.Length) + 40; // 40px de márgenes
                                                                    // Si es muy ancho, ajustar automáticamente
            if (anchoTotalAprox > 700) // 700px es aproximadamente el 70% de 1000px
            {
                // Reducir proporcionalmente
                double factorReduccion = 700 / anchoTotalAprox;
                gridTabla.ColumnDefinitions[0].Width = new GridLength(100 * factorReduccion);
                for (int i = 1; i <= dias.Length; i++)
                {
                    gridTabla.ColumnDefinitions[i].Width = new GridLength(95 * factorReduccion);
                }
            }

            // FILAS MANTENIENDO TAMAÑO GRANDE
            int totalFilas = 2 + horariosMostrar.Length;

            gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28) });
            for (int i = 0; i < horariosMostrar.Length; i++)
            {
                gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(26) });
            }

            // NOMBRE DEL MAESTRO
            string nombreProfesor = _horarioSeleccionado?.NombreProfesor?.ToUpper() ?? "NOMBRE DEL PROFESOR";
            var celdaNombreMaestro = CrearCeldaTablaAjustable(nombreProfesor, Brushes.LightBlue, FontWeights.Bold, 13);
            Grid.SetRow(celdaNombreMaestro, 0);
            Grid.SetColumn(celdaNombreMaestro, 1);
            Grid.SetColumnSpan(celdaNombreMaestro, dias.Length);
            gridTabla.Children.Add(celdaNombreMaestro);

            // ENCABEZADOS DE DÍAS
            for (int col = 0; col < dias.Length; col++)
            {
                var celda = CrearCeldaTablaAjustable(dias[col], Brushes.LightGray, FontWeights.Bold, 11);
                Grid.SetRow(celda, 1);
                Grid.SetColumn(celda, col + 1);
                gridTabla.Children.Add(celda);
            }

            // HORARIOS
            for (int row = 0; row < horariosMostrar.Length; row++)
            {
                var celdaHora = CrearCeldaTablaAjustable(horariosMostrar[row], Brushes.White, FontWeights.Bold, 10);
                Grid.SetRow(celdaHora, row + 2);
                Grid.SetColumn(celdaHora, 0);
                gridTabla.Children.Add(celdaHora);

                for (int col = 0; col < dias.Length; col++)
                {
                    string contenido = ObtenerContenidoCelda(dias[col].ToLower(), horariosMostrar[row]);
                    var celda = CrearCeldaTablaAjustable(contenido, Brushes.White, FontWeights.Normal, 9);
                    Grid.SetRow(celda, row + 2);
                    Grid.SetColumn(celda, col + 1);
                    gridTabla.Children.Add(celda);
                }
            }

            return gridTabla;
        }

        private Border CrearCeldaTablaAjustable(string texto, Brush fondo, FontWeight fontWeight, double fontSizeBase)
        {
            var textBlock = new TextBlock
            {
                Text = texto,
                FontWeight = fontWeight,
                FontSize = fontSizeBase,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // ✅ SOLO DIVIDIR SI ES UNA CELDA DE CLASE (grupo/materia) Y NO ESTÁ VACÍA
            if (!string.IsNullOrEmpty(texto) && !texto.Contains(":") && !texto.Contains("NOMBRE DEL PROFESOR"))
            {
                var clase = ObtenerClaseParaTexto(texto);
                if (clase != null)
                {
                    // ✅ CREAR GRID SOLO PARA CELDAS DE CLASE
                    var gridInterno = new Grid();
                    gridInterno.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    gridInterno.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                    // Parte superior: Grupo/Materia
                    var textBlockSuperior = new TextBlock
                    {
                        Text = texto,
                        FontWeight = FontWeights.Bold,
                        FontSize = fontSizeBase - 1,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetRow(textBlockSuperior, 0);
                    gridInterno.Children.Add(textBlockSuperior);

                    // Parte inferior: Modalidad
                    string modalidad = "PRESENCIAL";
                    if (!string.IsNullOrEmpty(clase.Modalidad) && clase.Modalidad.ToUpper() == "EN LÍNEA")
                        modalidad = "EN LÍNEA";

                    var textBlockInferior = new TextBlock
                    {
                        Text = modalidad,
                        FontWeight = FontWeights.Normal,
                        FontSize = fontSizeBase - 2,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlockInferior, 1);
                    gridInterno.Children.Add(textBlockInferior);

                    return new Border
                    {
                        Child = gridInterno,
                        Background = fondo,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.8),
                        Padding = new Thickness(2)
                    };
                }
            }

            // ✅ PARA TODAS LAS DEMÁS CELDAS (horas, nombre, vacías) - NORMAL
            return new Border
            {
                Child = textBlock,
                Background = fondo,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.8),
                Padding = new Thickness(2)
            };
        }

        // ✅ MÉTODO PARA OBTENER LA CLASE CORRESPONDIENTE AL TEXTO
        private Horario ObtenerClaseParaTexto(string texto)
        {
            if (_horarioSeleccionado?.Clases == null) return null;

            try
            {
                // Buscar por clave de materia
                if (texto.Contains(" ") && !texto.Contains("-"))
                {
                    var partes = texto.Split(' ');
                    if (partes.Length >= 2)
                    {
                        return _horarioSeleccionado.Clases.FirstOrDefault(c =>
                            (!string.IsNullOrEmpty(c.ClaveMateria) && c.ClaveMateria == texto) ||
                            (!string.IsNullOrEmpty(c.ClaveCarrera) &&
                             $"{c.ClaveCarrera} {c.Grado}{c.Seccion} {c.Turno}".Trim() == texto));
                    }
                }

                // Buscar directo
                return _horarioSeleccionado.Clases.FirstOrDefault(c =>
                    (!string.IsNullOrEmpty(c.ClaveMateria) && c.ClaveMateria == texto) ||
                    (!string.IsNullOrEmpty(c.ClaveCarrera) &&
                     $"{c.ClaveCarrera} {c.Grado}{c.Seccion} {c.Turno}".Trim() == texto));
            }
            catch
            {
                return null;
            }
        }

        // ✅ MÉTODO PARA TEXTO SUPERIOR EN IMPRESIÓN
        private string ObtenerTextoSuperiorImpresion(string textoOriginal, Horario clase)
        {
            if (clase != null)
            {
                if (!string.IsNullOrEmpty(clase.ClaveMateria))
                {
                    return clase.ClaveMateria + (clase.EsNP ? " - NP" : "");
                }
                else if (!string.IsNullOrEmpty(clase.ClaveCarrera))
                {
                    string texto = $"{clase.ClaveCarrera} {clase.Grado}{clase.Seccion} {clase.Turno}".Trim();
                    return texto + (clase.EsNP ? " - NP" : "");
                }
            }

            // Fallback al texto original
            return textoOriginal;
        }

        // ✅ MÉTODO PARA TEXTO MODALIDAD EN IMPRESIÓN
        private string ObtenerTextoModalidadImpresion(Horario clase)
        {
            if (clase != null && !string.IsNullOrEmpty(clase.Modalidad))
            {
                return clase.Modalidad.ToUpper() == "EN LÍNEA" ? "EN LÍNEA" : "PRESENCIAL";
            }
            return "PRESENCIAL"; // Valor por defecto
        }

        private string[] ObtenerHorariosPorTurnosUsados()
        {
            if (_horarioSeleccionado?.Clases == null) return new string[0];

            var turnos = new Dictionary<string, string[]>
    {
        {
            "Matutino", new[] {
                "7:30 - 8:20", "8:20 - 9:10", "9:10 - 10:00", "10:00 - 10:50",
                "10:50 - 11:40", "11:40 - 12:30", "12:30 - 13:20", "13:20 - 14:10"
            }
        },
        {
            "Intermedio", new[] {
                "11:40 - 12:30", "12:30 - 13:20", "13:20 - 14:10", "14:10 - 15:00",
                "15:00 - 15:50", "15:50 - 16:40", "16:40 - 17:30", "17:30 - 18:20"
            }
        },
        {
            "Vespertino", new[] {
                "13:20 - 14:10", "14:10 - 15:00", "15:00 - 15:50", "15:50 - 16:40",
                "16:40 - 17:30", "17:30 - 18:20", "18:20 - 19:10", "19:10 - 20:00"
            }
        },
        {
            "Nocturno", new[] {
                "18:30 - 19:15", "19:15 - 20:00", "20:00 - 20:45", "20:45 - 21:30",
                "21:30 - 22:15", "22:15 - 23:00"
            }
        }
    };

            // Obtener TODAS las horas (inicio y fin) de todas las clases
            var todasLasHoras = _horarioSeleccionado.Clases
                .SelectMany(c => new[] { c.HoraInicio, c.HoraFin })
                .Distinct()
                .OrderBy(h => h)
                .ToList();

            if (todasLasHoras.Count == 0) return new string[0];

            // Encontrar la hora mínima y máxima
            var horaMinima = todasLasHoras.Min();
            var horaMaxima = todasLasHoras.Max();

            // Determinar turnos necesarios basado en el rango completo
            var turnosNecesarios = new HashSet<string>();

            if (horaMinima >= new TimeSpan(7, 30, 0) && horaMaxima <= new TimeSpan(14, 10, 0))
            {
                turnosNecesarios.Add("Matutino");
            }
            else if (horaMinima >= new TimeSpan(11, 40, 0) && horaMaxima <= new TimeSpan(18, 20, 0))
            {
                turnosNecesarios.Add("Intermedio");
            }
            else if (horaMinima >= new TimeSpan(13, 20, 0) && horaMaxima <= new TimeSpan(20, 0, 0))
            {
                turnosNecesarios.Add("Vespertino");
            }
            else if (horaMinima >= new TimeSpan(18, 30, 0) && horaMaxima <= new TimeSpan(23, 0, 0))
            {
                turnosNecesarios.Add("Nocturno");
            }
            else
            {
                // Si el rango cruza múltiples turnos, determinar combinaciones
                if (horaMinima <= new TimeSpan(14, 10, 0) && horaMaxima >= new TimeSpan(11, 40, 0))
                {
                    turnosNecesarios.Add("Matutino");
                    turnosNecesarios.Add("Intermedio");
                }
                if (horaMinima <= new TimeSpan(18, 20, 0) && horaMaxima >= new TimeSpan(13, 20, 0))
                {
                    turnosNecesarios.Add("Intermedio");
                    turnosNecesarios.Add("Vespertino");
                }
                if (horaMinima <= new TimeSpan(20, 0, 0) && horaMaxima >= new TimeSpan(18, 30, 0))
                {
                    turnosNecesarios.Add("Vespertino");
                    turnosNecesarios.Add("Nocturno");
                }
            }

            // Agregar horarios
            var horariosUsados = new HashSet<string>();
            foreach (var turno in turnosNecesarios)
            {
                if (turnos.ContainsKey(turno))
                    horariosUsados.UnionWith(turnos[turno]);
            }

            // DEBUG
            Console.WriteLine($"=== DETECCIÓN POR RANGO ===");
            Console.WriteLine($"Hora mínima: {horaMinima}");
            Console.WriteLine($"Hora máxima: {horaMaxima}");
            Console.WriteLine($"Turnos necesarios: {string.Join(" + ", turnosNecesarios)}");

            return horariosUsados.OrderBy(h => TimeSpan.Parse(h.Split('-')[0].Trim())).ToArray();
        }

        private string ObtenerContenidoCelda(string dia, string horario)
        {
            if (_horarioSeleccionado?.Clases == null) return "";

            try
            {
                string horaInicioStr = horario.Split('-')[0].Trim();
                TimeSpan horaInicio = TimeSpan.Parse(horaInicioStr);

                var clase = _horarioSeleccionado.Clases.FirstOrDefault(c =>
                    c.DiaSemana.Equals(dia, StringComparison.OrdinalIgnoreCase) && c.HoraInicio == horaInicio);

                if (clase != null)
                {
                    string contenido = "";
                    if (!string.IsNullOrEmpty(clase.ClaveMateria))
                        contenido = clase.ClaveMateria;
                    else if (!string.IsNullOrEmpty(clase.ClaveCarrera))
                        contenido = $"{clase.ClaveCarrera} {clase.Grado}{clase.Seccion} {clase.Turno}".Trim();

                    if (clase.EsNP && !string.IsNullOrEmpty(contenido))
                        contenido += " - NP";

                    return contenido;
                }
            }
            catch { }

            return "";
        }
        private StackPanel CrearSeccionFirmas()
        {
            var stackFirmas = new StackPanel();
            stackFirmas.Margin = new Thickness(20, 5, 20, 0);
            stackFirmas.VerticalAlignment = VerticalAlignment.Top;
            stackFirmas.HorizontalAlignment = HorizontalAlignment.Center;

            // "Acepto horario" más arriba y más grande
            var txtAcepto = CrearTextBlock("Acepto horario de conformidad", FontWeights.Normal, 12, TextAlignment.Right);
            txtAcepto.Margin = new Thickness(0, 0, 80, 40);
            stackFirmas.Children.Add(txtAcepto);

            var stackHuella = new StackPanel();
            stackHuella.HorizontalAlignment = HorizontalAlignment.Right;
            stackHuella.Margin = new Thickness(0, 0, 0, 60);

            var lineaHuella = new Border { Height = 1, Background = Brushes.Black, Margin = new Thickness(0, 30, 0, 5), Width = 100 };
            stackHuella.Children.Add(lineaHuella);

            var txtHuella = CrearTextBlock("HUELLA", FontWeights.Normal, 12, TextAlignment.Right);
            txtHuella.Margin = new Thickness(0, 2, 25, 0);
            stackHuella.Children.Add(txtHuella);
            stackFirmas.Children.Add(stackHuella);

            string nombreDocente = _horarioSeleccionado?.NombreProfesor?.ToUpper() ?? "NOMBRE DEL PROFESOR";
            var txtNombreDocente = CrearTextBlock(nombreDocente, FontWeights.Normal, 12, TextAlignment.Left);
            txtNombreDocente.Margin = new Thickness(25, 0, 0, 5);
            stackFirmas.Children.Add(txtNombreDocente);

            var lineaFirmaDocente = new Border { Height = 1, Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 5), Width = 220, HorizontalAlignment = HorizontalAlignment.Left };
            stackFirmas.Children.Add(lineaFirmaDocente);

            var txtFirmaDocente = CrearTextBlock("NOMBRE Y FIRMA DEL DOCENTE", FontWeights.Normal, 12, TextAlignment.Left);
            txtFirmaDocente.Margin = new Thickness(25, 0, 0, 30);
            stackFirmas.Children.Add(txtFirmaDocente);

            var txtGrace = CrearTextBlock("GRACE LIZBETH QUINTANA JUÁREZ", FontWeights.Normal, 12, TextAlignment.Left);
            txtGrace.Margin = new Thickness(25, 50, 0, 5);
            stackFirmas.Children.Add(txtGrace);

            var lineaFirmaJefe = new Border { Height = 1, Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 5), Width = 220, HorizontalAlignment = HorizontalAlignment.Left };
            stackFirmas.Children.Add(lineaFirmaJefe);

            var txtFirmaJefe = CrearTextBlock("NOMBRE Y FIRMA JEFE DE IDIOMAS", FontWeights.Normal, 12, TextAlignment.Left);
            txtFirmaJefe.Margin = new Thickness(25, 0, 0, 30);
            stackFirmas.Children.Add(txtFirmaJefe);

            return stackFirmas;
        }

        private Grid CrearTablaGrupos()
        {
            try
            {
                var elementosUnicos = ObtenerElementosUnicos();
                if (elementosUnicos == null || elementosUnicos.Count == 0) return CrearTablaVacia();

                var gridTabla = new Grid();
                gridTabla.Margin = new Thickness(30, 5, 30, 10);
                gridTabla.HorizontalAlignment = HorizontalAlignment.Left;
                gridTabla.VerticalAlignment = VerticalAlignment.Top;

                // COLUMNAS MÁS ANCHAS - AGREGAR COLUMNA MODALIDAD
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(130) });
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(180) }); // Reducir materia
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) }); // Modalidad
                gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70) });  // Horas

                int totalFilas = 1 + elementosUnicos.Count + 1;
                for (int i = 0; i < totalFilas; i++)
                {
                    gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                }

                // ✅ AGREGAR ENCABEZADO MODALIDAD
                string[] encabezados = { "GRUPO", "CLAVE", "MATERIA", "MODALIDAD", "HORAS" };
                for (int col = 0; col < encabezados.Length; col++)
                {
                    var celda = CrearCeldaTabla(encabezados[col], Brushes.LightGray, FontWeights.Bold, 11);
                    Grid.SetRow(celda, 0);
                    Grid.SetColumn(celda, col);
                    gridTabla.Children.Add(celda);
                }

                double totalHoras = 0;
                for (int i = 0; i < elementosUnicos.Count; i++)
                {
                    var elemento = elementosUnicos[i];
                    string grupo = DeterminarGrupo(elemento);
                    string clave = ObtenerClaveCompleta(elemento);
                    string materia = ObtenerMateriaFormateada(elemento);
                    string modalidad = ObtenerTextoModalidadImpresion(elemento); // ✅ NUEVA COLUMNA
                    double horas = CalcularHoras(elemento);
                    totalHoras += horas;

                    var celdaGrupo = CrearCeldaTabla(grupo, Brushes.White, FontWeights.Normal, 10);
                    Grid.SetRow(celdaGrupo, i + 1); Grid.SetColumn(celdaGrupo, 0); gridTabla.Children.Add(celdaGrupo);

                    var celdaClave = CrearCeldaTabla(clave, Brushes.White, FontWeights.Normal, 10);
                    Grid.SetRow(celdaClave, i + 1); Grid.SetColumn(celdaClave, 1); gridTabla.Children.Add(celdaClave);

                    var celdaMateria = CrearCeldaTabla(materia, Brushes.White, FontWeights.Normal, 10);
                    Grid.SetRow(celdaMateria, i + 1); Grid.SetColumn(celdaMateria, 2); gridTabla.Children.Add(celdaMateria);

                    var celdaModalidad = CrearCeldaTabla(modalidad, Brushes.White, FontWeights.Normal, 10); // ✅ NUEVA CELDA
                    Grid.SetRow(celdaModalidad, i + 1); Grid.SetColumn(celdaModalidad, 3); gridTabla.Children.Add(celdaModalidad);

                    var celdaHoras = CrearCeldaTabla(horas.ToString(), Brushes.White, FontWeights.Normal, 10);
                    Grid.SetRow(celdaHoras, i + 1); Grid.SetColumn(celdaHoras, 4); gridTabla.Children.Add(celdaHoras);
                }

                // ✅ ACTUALIZAR TOTALES CON NUEVA COLUMNA
                var celdaTotalLabel = CrearCeldaTabla("TOTAL HORAS", Brushes.LightGray, FontWeights.Bold, 11);
                Grid.SetRow(celdaTotalLabel, totalFilas - 1); Grid.SetColumn(celdaTotalLabel, 3); gridTabla.Children.Add(celdaTotalLabel);

                var celdaTotalHoras = CrearCeldaTabla(totalHoras.ToString(), Brushes.LightGray, FontWeights.Bold, 11);
                Grid.SetRow(celdaTotalHoras, totalFilas - 1); Grid.SetColumn(celdaTotalHoras, 4); gridTabla.Children.Add(celdaTotalHoras);

                return gridTabla;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear tabla de grupos: {ex.Message}");
                return CrearTablaVacia();
            }
        }

        private List<Horario> ObtenerElementosUnicos()
        {
            if (_horarioSeleccionado?.Clases == null) return new List<Horario>();

            var elementosUnicos = new List<Horario>();

            // Agrupar por materia (cuando tiene ClaveMateria)
            var materias = _horarioSeleccionado.Clases
                .Where(c => !string.IsNullOrEmpty(c.ClaveMateria))
                .GroupBy(c => c.ClaveMateria)
                .Select(g => g.First())
                .ToList();

            // Agrupar por grupo (cuando no tiene ClaveMateria pero sí ClaveCarrera)
            var grupos = _horarioSeleccionado.Clases
                .Where(c => string.IsNullOrEmpty(c.ClaveMateria) && !string.IsNullOrEmpty(c.ClaveCarrera))
                .GroupBy(c => $"{c.ClaveCarrera}-{c.Grado}-{c.Seccion ?? ""}-{c.Turno ?? ""}")
                .Select(g => g.First())
                .ToList();

            elementosUnicos.AddRange(materias);
            elementosUnicos.AddRange(grupos);

            return elementosUnicos;
        }

        private string DeterminarGrupo(Horario elemento)
        {
            // Si es una materia individual (tiene ClaveMateria)
            if (!string.IsNullOrEmpty(elemento.ClaveMateria))
            {
                // Buscar en tu tabla de materias para obtener el grupo real
                try
                {
                    var materiasService = new MateriasService();
                    var materia = materiasService.ObtenerMateriaPorClave(elemento.ClaveMateria);
                    if (materia != null && !string.IsNullOrEmpty(materia.Grupo))
                    {
                        return materia.Grupo.ToUpper(); // "IDIOMAS", "TUTORÍAS", etc. de tu BD
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al buscar grupo para {elemento.ClaveMateria}: {ex.Message}");
                    // Si hay error, usar lógica de respaldo
                }

                // Lógica de respaldo para inglés
                var numeros = new string(elemento.ClaveMateria.Where(char.IsDigit).ToArray());
                if (int.TryParse(numeros, out int numero))
                {
                    if (numero >= 1 && numero <= 3) return "BÁSICO";
                    if (numero >= 4 && numero <= 6) return "INTERMEDIO";
                    if (numero >= 7 && numero <= 9) return "AVANZADO";
                }

                return "IDIOMAS"; // Valor por defecto para otras materias
            }

            // Para grupos de carrera (inglés por niveles)
            if (elemento.Grado >= 1 && elemento.Grado <= 3) return "BÁSICO";
            if (elemento.Grado >= 4 && elemento.Grado <= 6) return "INTERMEDIO";
            if (elemento.Grado >= 7 && elemento.Grado <= 9) return "AVANZADO";

            return "GENERAL";
        }

        private string ObtenerClaveCompleta(Horario elemento)
        {
            if (!string.IsNullOrEmpty(elemento.ClaveMateria)) return elemento.ClaveMateria.Trim();
            if (!string.IsNullOrEmpty(elemento.ClaveCarrera)) return $"{elemento.ClaveCarrera} {elemento.Grado}{elemento.Seccion} {elemento.Turno}".Trim();
            return "SIN CLAVE";
        }

        private string ObtenerMateriaFormateada(Horario elemento)
        {
            // Si es una materia individual (tiene ClaveMateria)
            if (!string.IsNullOrEmpty(elemento.ClaveMateria))
            {
                // PRIORIDAD 1: Si ya tiene nombre de materia, usarlo
                if (!string.IsNullOrEmpty(elemento.NombreMateria))
                {
                    return elemento.NombreMateria.ToUpper();
                }

                // PRIORIDAD 2: Buscar el nombre real de la materia
                // (aquí debes conectar con tu base de datos de materias)
                string nombreReal = ObtenerNombreRealMateria(elemento.ClaveMateria);
                if (!string.IsNullOrEmpty(nombreReal))
                {
                    return nombreReal.ToUpper(); // "ITALIANO", "ADMINISTRATIVO", etc.
                }

                // PRIORIDAD 3: Solo si no se encuentra, usar la lógica de inglés
                if (elemento.ClaveMateria.ToUpper().Contains("TUT"))
                    return "TUTORÍAS";

                var numeros = new string(elemento.ClaveMateria.Where(char.IsDigit).ToArray());
                if (int.TryParse(numeros, out int numero) && numero >= 1 && numero <= 9)
                {
                    return $"INGLÉS {ConvertirARomano(numero)}";
                }

                return "MATERIA"; // Valor por defecto
            }

            // Si es un grupo (CIA 4A M, etc.) - DEJAR COMO ESTABA
            if (!string.IsNullOrEmpty(elemento.ClaveCarrera))
            {
                if (elemento.Grado >= 1 && elemento.Grado <= 9)
                {
                    return $"INGLÉS {ConvertirARomano(elemento.Grado)}";
                }
                return "INGLÉS";
            }

            return "SIN ESPECIFICAR";
        }

        private string ObtenerNombreRealMateria(string claveMateria)
        {
            try
            {
                var materiasService = new MateriasService();
                var materia = materiasService.ObtenerMateriaPorClave(claveMateria);

                if (materia != null && !string.IsNullOrEmpty(materia.Nombre))
                {
                    return materia.Nombre; // Retorna el nombre real de tu BD: "ITALIANO", "ADMINISTRATIVO", etc.
                }

                // Si no encuentra en BD, usar el mapeo temporal
                return MapearClaveANombreMateria(claveMateria);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar materia {claveMateria}: {ex.Message}");
                // Si hay error de conexión, usar mapeo temporal
                return MapearClaveANombreMateria(claveMateria);
            }
        }

        private double CalcularHoras(Horario elemento)
        {
            if (_horarioSeleccionado?.Clases == null) return 0;

            if (!string.IsNullOrEmpty(elemento.ClaveMateria))
            {
                return _horarioSeleccionado.Clases.Count(c => c.ClaveMateria == elemento.ClaveMateria);
            }
            else if (!string.IsNullOrEmpty(elemento.ClaveCarrera))
            {
                return _horarioSeleccionado.Clases.Count(c => c.ClaveCarrera == elemento.ClaveCarrera && c.Grado == elemento.Grado && c.Seccion == elemento.Seccion);
            }
            return 0;
        }

        private Grid CrearTablaVacia()
        {
            var gridTabla = new Grid();
            gridTabla.Margin = new Thickness(20, 10, 0, 10);
            gridTabla.HorizontalAlignment = HorizontalAlignment.Left;

            gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
            gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
            gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
            gridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });

            gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25) });
            gridTabla.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25) });

            string[] encabezados = { "GRUPO", "CLAVE", "MATERIA", "HORAS" };
            for (int col = 0; col < encabezados.Length; col++)
            {
                var celda = CrearCeldaTabla(encabezados[col], Brushes.LightGray, FontWeights.Bold, 10);
                Grid.SetRow(celda, 0); Grid.SetColumn(celda, col); gridTabla.Children.Add(celda);
            }

            var celdaMensaje = CrearCeldaTabla("No hay materias/grupos en este horario", Brushes.White, FontWeights.Normal, 9);
            Grid.SetRow(celdaMensaje, 1); Grid.SetColumnSpan(celdaMensaje, 4); gridTabla.Children.Add(celdaMensaje);

            return gridTabla;
        }

        private string MapearClaveANombreMateria(string claveMateria)
        {
            var mapeoMaterias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Inglés
                {"ING1", "INGLÉS I"}, {"ING2", "INGLÉS II"}, {"ING3", "INGLÉS III"},
                {"ING4", "INGLÉS IV"}, {"ING5", "INGLÉS V"}, {"ING6", "INGLÉS VI"},
                {"ING7", "INGLÉS VII"}, {"ING8", "INGLÉS VIII"}, {"ING9", "INGLÉS IX"},
        
                
                
            };

            // Buscar coincidencia exacta
            if (mapeoMaterias.ContainsKey(claveMateria))
                return mapeoMaterias[claveMateria];

            // Buscar coincidencia parcial
            foreach (var mapeo in mapeoMaterias)
            {
                if (claveMateria.ToUpper().Contains(mapeo.Key))
                    return mapeo.Value;
            }

            return null;
        }

        // MÉTODOS AUXILIARES BÁSICOS
        private Border CrearCeldaTabla(string texto, Brush fondo, FontWeight fontWeight, double fontSize)
        {
            var textBlock = new TextBlock { Text = texto, FontWeight = fontWeight, FontSize = fontSize, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            return new Border { Child = textBlock, Background = fondo, BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5), Padding = new Thickness(3) };
        }

        private TextBlock CrearTextBlock(string texto, FontWeight fontWeight, double fontSize, TextAlignment alignment)
        {
            return new TextBlock { Text = texto, FontWeight = fontWeight, FontSize = fontSize, TextAlignment = alignment, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = alignment == TextAlignment.Center ? HorizontalAlignment.Center : alignment == TextAlignment.Right ? HorizontalAlignment.Right : HorizontalAlignment.Left };
        }

        private string ConvertirARomano(int numero)
        {
            switch (numero) { case 1: return "I"; case 2: return "II"; case 3: return "III"; case 4: return "IV"; case 5: return "V"; case 6: return "VI"; case 7: return "VII"; case 8: return "VIII"; case 9: return "IX"; default: return numero.ToString(); }
        }

        private BitmapImage CargarImagenDesdeRecursos(string nombreArchivo)
        {
            try { string ruta = $"pack://application:,,,/Imagenes/{nombreArchivo}"; return new BitmapImage(new Uri(ruta, UriKind.RelativeOrAbsolute)); }
            catch { try { string ruta = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagenes", nombreArchivo); if (System.IO.File.Exists(ruta)) return new BitmapImage(new Uri(ruta)); } catch { } return null; }
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try { PrintDialog printDialog = new PrintDialog(); if (printDialog.ShowDialog() == true) { printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape; printDialog.PrintVisual(borderImprimible, "Horario Docente"); MessageBox.Show("Horario impreso correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information); } }
            catch (Exception ex) { MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) 
        {
            this.Close(); 
        }

    }
}
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Asociaciones;

public partial class CambioMiembrosAsociacion : ComponentBase
{
    [Parameter] public int id { get; set; }
    
    // Servicios
    [Inject] private IRegistroAsociacion AsociacionService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IArchivoLegalService ArchivoService { get; set; } = default!;
    [Inject] private IHistorialRegistro HistorialService { get; set; } = default!;

    // Modelos y estado
    private AsociacionModel? asociacionActual;
    private AsociacionModel nuevosDatos = new();
    private bool cargando = true;
    private bool procesando = false;
    private string mensajeExito = "";
    private string mensajeError = "";
    private string comentarioCambio = "";
    private DateTime fechaCambio = DateTime.Today;
    private string numeroResolucionCambio = "";
    private List<IBrowserFile> documentosCambio = new();
    private string? usuarioId;
    
    // Controles de tipo de cambio
    private bool cambiarRepresentante = true;
    private bool cambiarApoderado = false;

    protected override async Task OnInitializedAsync()
    {
        await CargarAsociacion();
        await ObtenerUsuarioActual();
        fechaCambio = DateTime.Today;
    }

    private async Task CargarAsociacion()
    {
        cargando = true;
        StateHasChanged();

        try
        {
            asociacionActual = await AsociacionService.ObtenerPorId(id);
            if (asociacionActual != null)
            {
                // Inicializar nuevos datos con valores por defecto
                nuevosDatos.CargoRepLegal = "Presidente";
                nuevosDatos.Folio = asociacionActual.Folio;
                nuevosDatos.NombreAsociacion = asociacionActual.NombreAsociacion;
                
                // Si ya existe apoderado, cargar sus datos para referencia
                if (!string.IsNullOrEmpty(asociacionActual.NombreApoAbogado))
                {
                    nuevosDatos.NombreApoAbogado = asociacionActual.NombreApoAbogado;
                    nuevosDatos.ApellidoApoAbogado = asociacionActual.ApellidoApoAbogado;
                    nuevosDatos.CedulaApoAbogado = asociacionActual.CedulaApoAbogado;
                    nuevosDatos.TelefonoApoAbogado = asociacionActual.TelefonoApoAbogado;
                    nuevosDatos.CorreoApoAbogado = asociacionActual.CorreoApoAbogado;
                    nuevosDatos.DireccionApoAbogado = asociacionActual.DireccionApoAbogado;
                }
            }
            else
            {
                mensajeError = "No se pudo encontrar la asociación solicitada.";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar la asociación: {ex.Message}";
            Console.WriteLine($"[ERROR] Error cargando asociación: {ex}");
        }
        finally
        {
            cargando = false;
            StateHasChanged();
        }
    }

    private async Task ObtenerUsuarioActual()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                usuarioId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                           ?? user.Identity.Name;
                
                nuevosDatos.CreadaPor = user.Identity.Name ?? "Sistema";
                nuevosDatos.UsuarioId = usuarioId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Obteniendo usuario: {ex.Message}");
        }
    }

    private async Task CargarDocumentosCambio(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            if (Path.GetExtension(file.Name).ToLower() == ".pdf" && file.Size <= 50 * 1024 * 1024)
            {
                documentosCambio.Add(file);
            }
            else
            {
                if (Path.GetExtension(file.Name).ToLower() != ".pdf")
                {
                    mensajeError = $"El archivo '{file.Name}' no es un PDF válido.";
                }
                else
                {
                    mensajeError = $"El archivo '{file.Name}' excede el tamaño máximo de 50MB.";
                }
                StateHasChanged();
            }
        }
        StateHasChanged();
    }

    private void RemoverDocumentoCambio(IBrowserFile archivo)
    {
        documentosCambio.Remove(archivo);
        StateHasChanged();
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task ProcesarCambio()
    {
        if (asociacionActual == null) return;

        // Validar que se haya seleccionado al menos un tipo de cambio
        if (!cambiarRepresentante && !cambiarApoderado)
        {
            mensajeError = "Debe seleccionar al menos un tipo de cambio a realizar (Representante Legal y/o Apoderado Legal).";
            return;
        }

        procesando = true;
        mensajeError = "";
        mensajeExito = "";
        StateHasChanged();

        try
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(comentarioCambio))
            {
                mensajeError = "Debe especificar el motivo del cambio.";
                procesando = false;
                StateHasChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(nuevosDatos.CreadaPor))
            {
                mensajeError = "Usuario no autenticado. Por favor, inicie sesión.";
                procesando = false;
                StateHasChanged();
                return;
            }

            Console.WriteLine($"[INFO] Iniciando cambio de miembros para asociación ID: {asociacionActual.AsociacionId}");

            // 1. Preparar modelo para actualización
            var modeloActualizado = new AsociacionModel
            {
                AsociacionId = asociacionActual.AsociacionId,
                CreadaPor = nuevosDatos.CreadaPor,
                UsuarioId = nuevosDatos.UsuarioId
            };

            // Solo actualizar los campos que se están cambiando
            if (cambiarRepresentante)
            {
                modeloActualizado.NombreRepLegal = nuevosDatos.NombreRepLegal?.Trim();
                modeloActualizado.ApellidoRepLegal = nuevosDatos.ApellidoRepLegal?.Trim();
                modeloActualizado.CedulaRepLegal = nuevosDatos.CedulaRepLegal?.Trim();
                modeloActualizado.CargoRepLegal = nuevosDatos.CargoRepLegal?.Trim();
                modeloActualizado.TelefonoRepLegal = nuevosDatos.TelefonoRepLegal?.Trim();
                modeloActualizado.DireccionRepLegal = nuevosDatos.DireccionRepLegal?.Trim();
            }

            if (cambiarApoderado)
            {
                modeloActualizado.NombreApoAbogado = nuevosDatos.NombreApoAbogado?.Trim();
                modeloActualizado.ApellidoApoAbogado = nuevosDatos.ApellidoApoAbogado?.Trim();
                modeloActualizado.CedulaApoAbogado = nuevosDatos.CedulaApoAbogado?.Trim();
                modeloActualizado.TelefonoApoAbogado = nuevosDatos.TelefonoApoAbogado?.Trim();
                modeloActualizado.CorreoApoAbogado = nuevosDatos.CorreoApoAbogado?.Trim();
                modeloActualizado.DireccionApoAbogado = nuevosDatos.DireccionApoAbogado?.Trim();
                modeloActualizado.PerteneceAFirma = nuevosDatos.PerteneceAFirma;
                modeloActualizado.NombreFirma = nuevosDatos.NombreFirma?.Trim();
                modeloActualizado.TelefonoFirma = nuevosDatos.TelefonoFirma?.Trim();
                modeloActualizado.CorreoFirma = nuevosDatos.CorreoFirma?.Trim();
                modeloActualizado.DireccionFirma = nuevosDatos.DireccionFirma?.Trim();
            }

            var resultado = await AsociacionService.ActualizarAsociacion(modeloActualizado);

            if (resultado.Success && resultado.RegistroId > 0)
            {
                Console.WriteLine($"[SUCCESS] Asociación actualizada. Registro ID: {resultado.RegistroId}");

                // 2. Registrar en el historial específico de cambios de miembros
                var historialComentario = ConstruirComentarioHistorial();
                
                await HistorialService.RegistrarHistorialAsociacionAsync(
                    detRegAsociacionId: resultado.RegistroId,
                    asociacionId: asociacionActual.AsociacionId,
                    accion: "CAMBIOS_MIEMBROS",
                    comentario: historialComentario,
                    usuarioId: nuevosDatos.CreadaPor
                );

                Console.WriteLine($"[SUCCESS] Historial registrado para asociación {asociacionActual.AsociacionId}");

                // 3. Subir documentos de soporte si los hay
                if (documentosCambio.Any())
                {
                    Console.WriteLine($"[INFO] Subiendo {documentosCambio.Count} documentos de soporte");
                    
                    foreach (var archivo in documentosCambio)
                    {
                        try
                        {
                            var resultadoArchivo = await ArchivoService.GuardarArchivoAsociacionAsync(
                                asociacionActual.AsociacionId,
                                archivo,
                                "CAMBIOS_MIEMBROS"
                            );

                            if (resultadoArchivo?.Success == true)
                            {
                                Console.WriteLine($"[SUCCESS] Documento '{archivo.Name}' subido exitosamente");
                            }
                            else
                            {
                                Console.WriteLine($"[WARNING] Error al subir documento '{archivo.Name}': {resultadoArchivo?.Message}");
                            }
                        }
                        catch (Exception exArchivo)
                        {
                            Console.WriteLine($"[ERROR] Error subiendo archivo '{archivo.Name}': {exArchivo.Message}");
                        }
                    }
                }

                mensajeExito = "✅ Cambios de miembros registrados exitosamente. " +
                              "El historial ha sido guardado correctamente.";
                
                // Esperar y redirigir
                await Task.Delay(3000);
                Navigation.NavigateTo("/admin/listado");
            }
            else
            {
                mensajeError = $"❌ Error al actualizar la asociación: {resultado.Message}";
                Console.WriteLine($"[ERROR] Error actualizando asociación: {resultado.Message}");
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"❌ Error inesperado al procesar el cambio: {ex.Message}";
            Console.WriteLine($"[ERROR] Excepción en ProcesarCambio: {ex}");
        }
        finally
        {
            procesando = false;
            StateHasChanged();
        }
    }
    private string ConstruirComentarioHistorial()
    {
        var cambios = new List<string>();
        
        if (cambiarRepresentante)
        {
            cambios.Add($"REPRESENTANTE LEGAL - ANTERIOR: {asociacionActual.NombreRepLegal} {asociacionActual.ApellidoRepLegal} (Cédula: {asociacionActual.CedulaRepLegal}). " +
                       $"NUEVO: {nuevosDatos.NombreRepLegal} {nuevosDatos.ApellidoRepLegal} (Cédula: {nuevosDatos.CedulaRepLegal})");
        }
        if (cambiarApoderado)
        {
            var apoderadoAnterior = !string.IsNullOrEmpty(asociacionActual.NombreApoAbogado) 
                ? $"{asociacionActual.NombreApoAbogado} {asociacionActual.ApellidoApoAbogado} (Cédula: {asociacionActual.CedulaApoAbogado})"
                : "No registrado";
                
            cambios.Add($"APODERADO LEGAL - ANTERIOR: {apoderadoAnterior}. " +
                       $"NUEVO: {nuevosDatos.NombreApoAbogado} {nuevosDatos.ApellidoApoAbogado} (Cédula: {nuevosDatos.CedulaApoAbogado})");
        }
        return $"CAMBIOS REALIZADOS - " +
               $"Motivo: {comentarioCambio}. " +
               $"Fecha del cambio: {fechaCambio:dd/MM/yyyy}. " +
               $"Resolución: {(!string.IsNullOrEmpty(numeroResolucionCambio) ? numeroResolucionCambio : "No especificada")}. " +
               string.Join(" | ", cambios);
    }
    
    private EditForm? formulario;

// Agregar este método para verificar el estado de validación
    private bool FormularioEsValido()
    {
        return formulario?.EditContext?.Validate() == true;
    }

// Modificar el método PuedeGuardar
    private bool PuedeGuardar()
    {
        if (!cambiarRepresentante && !cambiarApoderado)
            return false;

        // Verificar validación del formulario
        if (!FormularioEsValido())
            return false;

        // Verificar campos adicionales que no están en el modelo
        if (string.IsNullOrWhiteSpace(comentarioCambio) || fechaCambio == default)
            return false;

        return true;
    }
    
    private void Volver()
    {
        Navigation.NavigateTo($"/admin/listado");
    }
}
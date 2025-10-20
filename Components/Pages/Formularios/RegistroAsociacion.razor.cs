using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Formularios;

public partial class RegistroAsociacion : ComponentBase
{
    [Inject] private IRegistroAsociacion _RegistroAsociacionService { get; set; } = default!;
    [Inject] private ICommon _Commonservice { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IArchivoLegalService ArchivoLegalService { get; set; } = default!;

    private const long MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024; // 50MB en bytes
    private readonly string[] _extensionesPermitidas = { ".pdf" };

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

    private AsociacionModel AModel { get; set; } = new()
    {
        DocumentosSubir = new List<IBrowserFile>()
    };

    private bool IsSubmitting = false;
    private string MensajeExito = "";
    private string MensajeError = "";

    protected override void OnInitialized()
    {
        AModel.CargoRepLegal = "Presidente";
    }

    private async Task CargarDocumentos(InputFileChangeEventArgs e)
    {
        MensajeError = "";

        foreach (var file in e.GetMultipleFiles())
        {
            if (file.Size > MAX_FILE_SIZE_BYTES)
            {
                MensajeError += $"❌ '{file.Name}' excede 50MB. Tamaño: {FormatFileSize(file.Size)}\n";
                continue;
            }

            var extension = Path.GetExtension(file.Name).ToLower();
            if (extension != ".pdf")
            {
                MensajeError += $"❌ '{file.Name}' no es PDF.\n";
                continue;
            }

            AModel.DocumentosSubir.Add(file);
            Console.WriteLine($"✅ '{file.Name}' listo para subir. Tamaño: {FormatFileSize(file.Size)}");
        }

        StateHasChanged();
    }

    private void RemoverDocumento(int index)
    {
        if (index >= 0 && index < AModel.DocumentosSubir.Count)
            AModel.DocumentosSubir.RemoveAt(index);
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        MensajeError = "";
        MensajeExito = "";

        try
        {
            Console.WriteLine($"📋 Iniciando proceso con {AModel.DocumentosSubir.Count} archivos");

            // 1. Crear asociación
            var resultado = await _RegistroAsociacionService.CrearAsociacion(AModel);
            if (!resultado.Success)
            {
                MensajeError = $"❌ Error al crear asociación: {resultado.Message}";
                IsSubmitting = false;
                StateHasChanged();
                return;
            }

            AModel.AsociacionId = resultado.AsociacionId;
            Console.WriteLine($"✅ Asociación creada ID: {AModel.AsociacionId}");

            // 2. Subir archivos si existen
            var erroresArchivos = new List<string>();

            if (AModel.DocumentosSubir?.Any() == true)
            {
                foreach (var archivo in AModel.DocumentosSubir)
                {
                    Console.WriteLine($"⬆️ Subiendo: {archivo.Name}");

                    var archivoRes = await ArchivoLegalService.GuardarArchivoAsociacionAsync(
                        AModel.AsociacionId,
                        archivo,
                        "DocumentosAsociacion"
                    );

                    if (archivoRes == null || !archivoRes.Success)
                    {
                        var errorMsg = $"❌ {archivo.Name}: {archivoRes?.Message ?? "Error desconocido"}";
                        erroresArchivos.Add(errorMsg);
                    }
                    else
                    {
                        Console.WriteLine($"✅ {archivo.Name} subido exitosamente");
                    }
                }
            }

            // 3. Mostrar resultados y redirigir
            if (erroresArchivos.Any())
            {
                MensajeError = string.Join("\n", erroresArchivos);
                IsSubmitting = false;
                StateHasChanged();
            }
            else
            {
                MensajeExito = "✅ Registro completado exitosamente!";
                StateHasChanged();

                // Esperar un momento para mostrar el mensaje y luego redirigir
                await Task.Delay(1500);
                Navigation.NavigateTo("/admin/listado");
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"💥 Error: {ex.Message}";
            Console.WriteLine($"💥 Error crítico: {ex}");
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void Cancelar() => Navigation.NavigateTo("/admin/listado");
}
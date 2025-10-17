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
            if (file.Size > 40 * 1024 * 1024)
            {
                MensajeError += $"El archivo {file.Name} excede 40 MB.\n";
                continue;
            }

            var extension = Path.GetExtension(file.Name).ToLower();
            var permitidas = new[] { ".pdf", ".docx", ".jpg", ".jpeg", ".png" };
            if (!permitidas.Contains(extension))
            {
                MensajeError += $"El archivo {file.Name} no tiene formato permitido.\n";
                continue;
            }

            AModel.DocumentosSubir.Add(file);
        }
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
            // // Validación de firma si aplica
            // if (AModel.PerteneceAFirma)
            // {
            //     if (string.IsNullOrWhiteSpace(AModel.NombreFirma) ||
            //         string.IsNullOrWhiteSpace(AModel.CorreoFirma) ||
            //         string.IsNullOrWhiteSpace(AModel.TelefonoFirma) ||
            //         string.IsNullOrWhiteSpace(AModel.DireccionFirma))
            //     {
            //         MensajeError = "Complete todos los datos de la firma de abogados.";
            //         return;
            //     }
            // }

            var resultado = await _RegistroAsociacionService.CrearAsociacion(AModel);

            if (resultado.Success)
            {
                AModel.AsociacionId = resultado.AsociacionId;

                // Guardar archivos asociados
                foreach (var archivo in AModel.DocumentosSubir)
                {
                    try
                    {
                        var guardado = await ArchivoLegalService.GuardarArchivoAsociacionAsync(
                            resultado.AsociacionId,
                            archivo,
                            categoria: "DocumentosAsociacion"
                        );

                        if (guardado == null)
                            MensajeError += $"No se pudo guardar el archivo {archivo.Name}.\n";
                    }
                    catch (Exception ex)
                    {
                        MensajeError += $"Error al guardar {archivo.Name}: {ex.Message}\n";
                    }
                }

                if (string.IsNullOrEmpty(MensajeError))
                {
                    MensajeExito = "Asociación registrada con éxito.";
                    await Task.Delay(1500);
                    Navigation.NavigateTo("/admin/listado");
                }
            }
            else
            {
                MensajeError = resultado.Message;
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private void Cancelar() => Navigation.NavigateTo("/admin/listado");
}

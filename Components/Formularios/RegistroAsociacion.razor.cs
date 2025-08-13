using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Formularios;

public partial class RegistroAsociacion : ComponentBase
{
    [Inject] private ICommon _Commonservice { get; set; }

    
    [Inject] private DbContextLegal _context { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private AsociacionModel AModel = new();
    private bool IsSubmitting = false;
    private string MensajeExito = "";
    private string MensajeError = "";

    protected override void OnInitialized()
    {
        // El cargo del representante legal siempre es Presidente
        AModel.CargoRepLegal = "Presidente";
    }

    private async Task CargarDocumentos(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            // Validar tamaño (máximo 10 MB)
            if (file.Size > 10 * 1024 * 1024)
            {
                MensajeError = $"El archivo {file.Name} excede el tamaño máximo de 10 MB.";
                continue;
            }

            // Validar tipo
            var extension = Path.GetExtension(file.Name).ToLower();
            var permitidas = new[] { ".pdf", ".docx", ".jpg", ".png", ".jpeg" };
            if (!permitidas.Contains(extension))
            {
                MensajeError = $"El archivo {file.Name} no tiene un formato permitido.";
                continue;
            }

            // AModel.DocumentosSubir.Add(file);
        }
    }

    private void RemoverDocumento(int index)
    {
        // if (index >= 0 && index < AModel.DocumentosSubir.Count)
        // {
        //     AModel.DocumentosSubir.RemoveAt(index);
        // }
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        MensajeError = "";
        MensajeExito = "";

        // try
        // {
        //     // Validar que si pertenece a firma, todos los campos de firma estén completos
        //     if (AModel.PerteneceAFirma)
        //     {
        //         if (string.IsNullOrWhiteSpace(AModel.NombreFirma))
        //         {
        //             MensajeError = "Debe ingresar el nombre de la firma de abogados.";
        //             return;
        //         }
        //
        //         if (string.IsNullOrWhiteSpace(AModel.CorreoFirma))
        //         {
        //             MensajeError = "Debe ingresar el correo de la firma de abogados.";
        //             return;
        //         }
        //
        //         if (string.IsNullOrWhiteSpace(AModel.TelefonoFirma))
        //         {
        //             MensajeError = "Debe ingresar el teléfono de la firma de abogados.";
        //             return;
        //         }
        //
        //         if (string.IsNullOrWhiteSpace(AModel.DireccionFirma))
        //         {
        //             MensajeError = "Debe ingresar la dirección de la firma de abogados.";
        //             return;
        //         }
        //     }
        // }
        // ✅ CORRECTO: Usar la instancia inyectada del servicio
        //     var resultado = await CommonServices.as(AModel);
        //
        //     if (resultado)
        //     {
        //         MensajeExito = "Asociación registrada con éxito.";
        //         // ✅ CORRECTO: Usar NavigationManager inyectado
        //         Navigation.NavigateTo("/asociaciones");
        //     }
        //     else
        //     {
        //         MensajeError = "Error al registrar la asociación. Por favor, intente nuevamente.";
        //     }
        // }
        // catch (Exception ex)
        // {
        //     MensajeError = $"Error inesperado: {ex.Message}";
        // }
        // finally
        // {
        //     IsSubmitting = false;
        // }
    }

// ✅ CORRECTO: Usar NavigationManager inyectado
    private void Cancelar() => Navigation.NavigateTo("/asociaciones");
}

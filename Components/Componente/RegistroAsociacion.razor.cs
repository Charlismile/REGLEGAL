
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Componente;

public partial class RegistroAsociacion : ComponentBase
{
    [Inject]
    private IRegistroService RegistroService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private RegistroDto Model = new();
    private bool IsSubmitting = false;
    private string MensajeExito = "";
    private string MensajeError = "";

    protected override void OnInitialized()
    {
        // El cargo del representante legal siempre es Presidente
        Model.CargoRepLegal = "Presidente";
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

            Model.DocumentosSubir.Add(file);
        }
    }

    private void RemoverDocumento(int index)
    {
        if (index >= 0 && index < Model.DocumentosSubir.Count)
        {
            Model.DocumentosSubir.RemoveAt(index);
        }
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        MensajeError = "";
        MensajeExito = "";

        try
        {
            // Validar que si pertenece a firma, todos los campos de firma estén completos
            if (Model.PerteneceAFirma)
            {
                if (string.IsNullOrWhiteSpace(Model.NombreFirma))
                {
                    MensajeError = "Debe ingresar el nombre de la firma de abogados.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Model.CorreoFirma))
                {
                    MensajeError = "Debe ingresar el correo de la firma de abogados.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Model.TelefonoFirma))
                {
                    MensajeError = "Debe ingresar el teléfono de la firma de abogados.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Model.DireccionFirma))
                {
                    MensajeError = "Debe ingresar la dirección de la firma de abogados.";
                    return;
                }
            }

            // ✅ CORRECTO: Usar la instancia inyectada del servicio
            var resultado = await RegistroService.CrearAsociacionAsync(Model);
        
            if (resultado)
            {
                MensajeExito = "Asociación registrada con éxito.";
                // ✅ CORRECTO: Usar NavigationManager inyectado
                Navigation.NavigateTo("/asociaciones");
            }
            else
            {
                MensajeError = "Error al registrar la asociación. Por favor, intente nuevamente.";
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

// ✅ CORRECTO: Usar NavigationManager inyectado
    private void Cancelar() => Navigation.NavigateTo("/asociaciones");
}
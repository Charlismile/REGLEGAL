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
        MensajeError = "";
        foreach (var file in e.GetMultipleFiles())
        {
            if (file.Size > 10 * 1024 * 1024)
            {
                MensajeError += $"El archivo {file.Name} excede 10 MB.\n";
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
        // Validación de firma de abogados
        if (AModel.PerteneceAFirma)
        {
            if (string.IsNullOrWhiteSpace(AModel.NombreFirma) ||
                string.IsNullOrWhiteSpace(AModel.CorreoFirma) ||
                string.IsNullOrWhiteSpace(AModel.TelefonoFirma) ||
                string.IsNullOrWhiteSpace(AModel.DireccionFirma))
            {
                MensajeError = "Complete todos los datos de la firma de abogados.";
                return;
            }
        }

        // Guardar Asociación en base de datos
        var firma = new TbApoderadoFirma
        {
            NombreFirma = AModel.NombreFirma,
            CorreoFirma = AModel.CorreoFirma,
            TelefonoFirma = AModel.TelefonoFirma,
            DireccionFirma = AModel.DireccionFirma
        };

        var apoderado = new TbApoderadoLegal
        {
            NombreApoAbogado = AModel.NombreApoAbogado,
            CedulaApoAbogado = AModel.CedulaApoAbogado,
            TelefonoApoAbogado = AModel.TelefonoApoAbogado,
            CorreoApoAbogado = AModel.CorreoApoAbogado,
            DireccionApoAbogado = AModel.DireccionApoAbogado,
            ApoderadoFirma = firma
        };

        var asociacion = new TbAsociacion
        {
            NombreAsociacion = AModel.NombreAsociacion,
            Actividad = AModel.Actividad,
            Folio = AModel.Folio,
            RepresentanteLegal = new TbRepresentanteLegal
            {
                NombreRepLegal = AModel.NombreRepLegal,
                CedulaRepLegal = AModel.CedulaRepLegal,
                CargoRepLegal = AModel.CargoRepLegal,
                TelefonoRepLegal = AModel.TelefonoRepLegal,
                DireccionRepLegal = AModel.DireccionRepLegal
            },
            ApoderadoLegal = apoderado
        };

        _context.TbAsociacion.Add(asociacion);
        await _context.SaveChangesAsync();


        // Guardar archivos
        // foreach (var archivo in AModel.DocumentosSubir)
        // {
        //     var resultado = await _Commonservice.GuardarArchivoAsync(
        //         archivo,
        //         categoria: "DocumentosAsociacion",
        //         asociacionId: _context.TbAsociacion.OrderByDescending(a => a.AsociacionId).First().AsociacionId
        //     );
        //
        //     if (!resultado.ok)
        //     {
        //         MensajeError += resultado.mensaje + "\n";
        //     }
        // }

        if (string.IsNullOrEmpty(MensajeError))
        {
            MensajeExito = "Asociación registrada con éxito.";
            Navigation.NavigateTo("/asociaciones");
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

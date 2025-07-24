using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Components.Componente;

public partial class RegistroAsociacion : ComponentBase
{
    private RegistroAsociacionDto Model = new();
    private List<TbApoderadoFirma> Firmas = new();
    private bool IsSubmitting = false;
    private string MensajeExito = "";
    private string MensajeError = "";

    protected override async Task OnInitializedAsync()
    {
        Firmas = await FirmaService.ObtenerFirmasAsync();
    }

    private async Task CargarDocumentos(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
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
            var resultado = await RegistroService.RegistrarAsociacionAsync(Model);
            
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Model = new RegistroAsociacionDto();
                Navigation.NavigateTo("/asociaciones");
            }
            else
            {
                MensajeError = resultado.Mensaje;
            }
        }
        catch (Exception ex)
        {
            MensajeError = "Error inesperado: " + ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private void Cancelar() => Navigation.NavigateTo("/asociaciones");
}
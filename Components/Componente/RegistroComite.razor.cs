using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;
using SISTEMALEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Componente;

public partial class RegistroComite : ComponentBase
{
    [Inject] private IRegistroComiteService RegistroService  { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Parameter] public int? Id { get; set; }

    private RegistroComiteDTO registroComite = new();
    private List<TbTipoTramite> tramites = new();
    private List<TbRegionSalud> regiones = new();
    private List<TbProvincia> provincias = new();
    private List<TbDistrito> distritos = new();
    private List<TbCorregimiento> corregimientos = new();
    private List<TbCargosMiembrosComite> cargos = new();
    
    protected override async Task OnInitializedAsync()
    {
        tramites = await RegistroService.GetTramitesAsync();
        regiones = await RegistroService.GetRegionesAsync();
        cargos = await RegistroService.GetCargosAsync();

        if (Id.HasValue)
        {
            registroComite = await RegistroService.GetComitePorIdAsync(Id.Value);
            if (registroComite != null)
            {
                // Cargar datos en cascada
                if (registroComite.RegionSaludId > 0)
                    provincias = await RegistroService.GetProvinciasAsync(registroComite.RegionSaludId);

                if (registroComite.ProvinciaId > 0)
                    distritos = await RegistroService.GetDistritosAsync(registroComite.ProvinciaId);

                if (registroComite.DistritoId > 0)
                    corregimientos = await RegistroService.GetCorregimientosAsync(registroComite.DistritoId);
            }
        }
        else
        {
            AddMiembro(); // Añadir un miembro por defecto
        }
    }

    private void OnTramiteChanged(ChangeEventArgs e)
    {
        // Limpiar miembros al cambiar trámite
        registroComite.Miembros.Clear();
        AddMiembro(); // Añadir uno por defecto
    }

    private void AddMiembro()
    {
        if (registroComite.Miembros.Count < GetMaxMiembros())
        {
            registroComite.Miembros.Add(new MiembroComiteDTO());
        }
    }

    private int GetMaxMiembros()
    {
        return registroComite.TramiteId == 3 ? 2 : 7;
    }

    private bool IsMaxMiembrosAlcanzado()
    {
        return registroComite.Miembros.Count >= GetMaxMiembros();
    }

    private List<TbCargosMiembrosComite> GetCargosPorTramite()
    {
        if (registroComite.TramiteId == 3)
        {
            // Presidente (1) y Tesorero (4)
            return cargos.Where(c => c.CargoId == 1 || c.CargoId == 4).ToList();
        }
        return cargos;
    }

    private async Task CargarProvincias(ChangeEventArgs e)
    {
        var regionId = int.Parse(e.Value.ToString());
        provincias = await RegistroService.GetProvinciasAsync(regionId);
        registroComite.ProvinciaId = 0;
        registroComite.DistritoId = 0;
        registroComite.CorregimientoId = 0;
    }
    
    private async Task CargarDistritos(ChangeEventArgs e)
    {
        var provinciaId = int.Parse(e.Value.ToString());
        distritos = await RegistroService.GetDistritosAsync(provinciaId);
        registroComite.DistritoId = 0;
        registroComite.CorregimientoId = 0;
    }

    private async Task CargarCorregimientos(ChangeEventArgs e)
    {
        var distritoId = int.Parse(e.Value.ToString());
        corregimientos = await RegistroService.GetCorregimientosAsync(distritoId);
        registroComite.CorregimientoId = 0;
    }

    private async Task HandleValidSubmit()
    {
        var result = await RegistroService.GuardarComiteAsync(registroComite);
        if (result)
        {
            Navigation.NavigateTo("/comites");
        }
        else
        {
            // Mostrar mensaje de error
        }
    }
    
    private async Task CargarArchivos(InputFileChangeEventArgs e)
    {
        foreach (var archivo in e.GetMultipleFiles(10)) // Máximo 10 archivos
        {
            var maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (archivo.Size > maxFileSize)
            {
                // Manejar error: archivo demasiado grande
                continue;
            }

            var extension = Path.GetExtension(archivo.Name).ToLower();
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                // Manejar error: tipo no permitido
                continue;
            }

            var nombreGuardado = $"{Guid.NewGuid()}{extension}";
            var rutaRelativa = $"/uploads/comites/{nombreGuardado}";
            var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comites", nombreGuardado);

            await using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.OpenReadStream(maxFileSize).CopyToAsync(stream);

            registroComite.Archivos.Add(new ArchivoDTO
            {
                Archivo = archivo.Name,
                Url = rutaRelativa,
                Categoria = "Documento General"
            });
        }
    }
}

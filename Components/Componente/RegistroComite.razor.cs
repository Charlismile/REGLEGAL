using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Components.Componente;

public partial class RegistroComite : ComponentBase
{
    private readonly DbContextLegal _context;
    private RegistroComiteDto Model = new();
    private List<TbRegionSalud> Regiones = new();
    private List<TbProvincia> Provincias = new();
    private List<TbDistrito> Distritos = new();
    private List<TbCorregimiento> Corregimientos = new();
    private List<TbTipoTramite> TipoTramites = new();
    private List<TbCargosMiembrosComite> Cargos = new();
    private bool IsSubmitting = false;
    private bool IsLoading = false;
    private string MensajeExito = "";
    private string MensajeError = "";
    private InputFile fileInput;

    private List<TbCargosMiembrosComite> CargosDisponibles => 
        Model.TipoTramiteId == 3 ? 
            Cargos.Where(c => c.NombreCargo == "Presidente" || c.NombreCargo == "Tesorero").ToList() : 
            Cargos;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            await Task.WhenAll(
                CargarRegiones(),
                CargarTipoTramites(),
                CargarCargos()
            );
        }
        catch (Exception ex)
        {
            MensajeError = "Error al cargar datos: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarRegiones()
    {
        Regiones = await UbicacionService.ObtenerRegionesAsync();
    }

    private async Task CargarTipoTramites()
    {
        TipoTramites = await _context.TbTipoTramite.Where(t => t.IsActivo).ToListAsync();
    }

    private async Task CargarCargos()
    {
        Cargos = await _context.TbCargosMiembrosComite.Where(c => c.IsActivo).ToListAsync();
    }

    private async Task CargarProvincias(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int regionId) && regionId > 0)
        {
            IsLoading = true;
            try
            {
                Provincias = await UbicacionService.ObtenerProvinciasPorRegionAsync(regionId);
                Distritos.Clear(); Corregimientos.Clear();
                Model.ProvinciaId = 0; Model.DistritoId = 0; Model.CorregimientoId = 0;
            }
            catch (Exception ex)
            {
                MensajeError = "Error al cargar provincias: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        else
        {
            Provincias.Clear(); Distritos.Clear(); Corregimientos.Clear();
        }
    }

    private async Task CargarDistritos(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int provinciaId) && provinciaId > 0)
        {
            IsLoading = true;
            try
            {
                Distritos = await UbicacionService.ObtenerDistritosPorProvinciaAsync(provinciaId);
                Corregimientos.Clear();
                Model.DistritoId = 0; Model.CorregimientoId = 0;
            }
            catch (Exception ex)
            {
                MensajeError = "Error al cargar distritos: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        else
        {
            Distritos.Clear(); Corregimientos.Clear();
        }
    }

    private async Task CargarCorregimientos(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int distritoId) && distritoId > 0)
        {
            IsLoading = true;
            try
            {
                Corregimientos = await UbicacionService.ObtenerCorregimientosPorDistritoAsync(distritoId);
                Model.CorregimientoId = 0;
            }
            catch (Exception ex)
            {
                MensajeError = "Error al cargar corregimientos: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        else
        {
            Corregimientos.Clear();
        }
    }

    private async Task OnTramiteChanged(ChangeEventArgs e)
    {
        Model.Miembros.Clear();
        if (Model.TipoTramiteId == 3)
        {
            AddMiembro(); AddMiembro();
        }
    }

    private bool IsMaxMiembros() => Model.Miembros.Count >= 7;

    private void AddMiembro()
    {
        if (IsMaxMiembros()) return;
        Model.Miembros.Add(new MiembroComiteDTO());
    }

    private async Task CargarDocumentos(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            // Validaciones
            if (file.Size > 10 * 1024 * 1024)
            {
                MensajeError = $"El archivo {file.Name} excede el tamaño máximo de 10 MB.";
                continue;
            }

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
            if (Model.Miembros.Count == 0)
            {
                MensajeError = "Debe agregar al menos un miembro.";
                return;
            }

            if (!ValidarCargosUnicos())
            {
                MensajeError = "No se pueden duplicar cargos clave como Presidente o Tesorero.";
                return;
            }

            var resultado = await RegistroService.RegistrarComiteAsync(Model);
            
            if (resultado.Exitoso)
            {
                MensajeExito = resultado.Mensaje;
                Model = new RegistroComiteDto();
                Navigation.NavigateTo("/comites");
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

    private bool ValidarCargosUnicos()
    {
        var cargosClave = new[] { 1, 2, 4, 5 };
        var duplicados = Model.Miembros
            .Where(m => cargosClave.Contains(m.CargoId))
            .GroupBy(m => m.CargoId)
            .Any(g => g.Count() > 1);
        return !duplicados;
    }

    private void Cancelar() => Navigation.NavigateTo("/comites");
}
// Services/IRegistroService.cs

using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroService
{
    #region Numeracion

    Task<string> GenerarNumeroAsociacion();
    Task<string> GenerarNumeroComite();

        #endregion
        
    #region Comites
    Task<List<RegistroDto>> ObtenerComiteAsync();
    Task<RegistroDto> ObtenerComitePorIdAsync(int id);
    Task<bool> CrearComiteAsync(RegistroDto dto);
    Task<bool> ActualizarComiteAsync(int id, RegistroDto dto);
    Task<bool> EliminarComiteAsync(int id);
    
    Task<List<TbTipoTramite>> ObtenerTipoTramitesAsync();
    Task<List<TbCargosMiembrosComite>> ObtenerCargosMiembrosAsync();
    
    #endregion
    
    #region Asociaciones

    Task<List<RegistroDto>> ObtenerAsociacionAsync();
    Task<RegistroDto> ObtenerAsociacionPorIdAsync(int id);
    Task<bool> CrearAsociacionAsync(RegistroDto dto);
    Task<bool> ActualizarAsociacionAsync(int id, RegistroDto dto);
    Task<bool> EliminarAsociacionAsync(int id);

    #endregion

    #region FirmaAbogados

    Task<List<TbApoderadoFirma>> ObtenerFirmasAsync();
    Task<TbApoderadoFirma?> ObtenerFirmaPorIdAsync(int firmaId);

        #endregion

    #region Ubicacion

        Task<List<TbRegionSalud>> ObtenerRegionesAsync();
        Task<List<TbProvincia>> ObtenerProvinciasPorRegionAsync(int regionId);
        Task<List<TbDistrito>> ObtenerDistritosPorProvinciaAsync(int provinciaId);
        Task<List<TbCorregimiento>> ObtenerCorregimientosPorDistritoAsync(int distritoId);

            #endregion
        
    #region Documentos

    Task<List<string>> GuardarArchivosAsync(List<IBrowserFile> archivos, string categoria, int? detAsociacionId = null, int? detComiteId = null);

    Task<List<TbArchivos>> ObtenerDocumentosPorAsociacionAsync(int asociacionId);
    Task<List<TbArchivos>> ObtenerDocumentosPorComiteAsync(int comiteId);

    #endregion
    
}


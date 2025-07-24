// Services/Implementations/RegistroService.cs

using Mapster;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroService : IRegistroService
{
    private readonly DbContextLegal _context;
    private readonly IRegistroNumeracionService _numeracionService;
    private readonly IDocumentoService _documentoService;

    public RegistroService(
        DbContextLegal context,
        IRegistroNumeracionService numeracionService,
        IDocumentoService documentoService)
    {
        _context = context;
        _numeracionService = numeracionService;
        _documentoService = documentoService;
    }

    public async Task<ResultadoRegistro> RegistrarAsociacionAsync(RegistroAsociacionDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. TbRepresentanteLegal
            var representante = dto.Adapt<TbRepresentanteLegal>();
            _context.TbRepresentanteLegal.Add(representante);
            await _context.SaveChangesAsync();

            // 2. TbApoderadoLegal
            var apoderado = dto.Adapt<TbApoderadoLegal>();
            _context.TbApoderadoLegal.Add(apoderado);
            await _context.SaveChangesAsync();

            // 3. TbAsociacion
            var asociacion = dto.Adapt<TbAsociacion>();
            asociacion.RepresentanteLegalId = representante.RepLegalId;
            asociacion.ApoderadoLegalId = apoderado.ApoAbogadoId;
            _context.TbAsociacion.Add(asociacion);
            await _context.SaveChangesAsync();

            // 4. TbDetalleRegAsociacion
            var numRegistro = await _numeracionService.GenerarNumeroAsociacion();
            var partes = numRegistro.Split('/');
            int secuencia = int.Parse(partes[0]);
            int anio = int.Parse(partes[1]);
            int mes = int.Parse(partes[2]);

            var detalle = new TbDetalleRegAsociacion
            {
                AsociacionId = asociacion.AsociacionId,
                NumRegASecuencia = secuencia,
                NomRegAAnio = anio,
                NumRegAMes = mes,
                CreadaPor = "UsuarioActual" // Reemplaza con Identity
            };
            _context.TbDetalleRegAsociacion.Add(detalle);

            // 5. Documentos
            if (dto.DocumentosSubir?.Count > 0)
            {
                await _documentoService.GuardarArchivoAsync(
                    dto.DocumentosSubir,
                    "Documento Legal",
                    detAsociacionId: asociacion.AsociacionId);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultadoRegistro
            {
                Exitoso = true,
                Mensaje = "Asociación registrada con éxito.",
                IdGenerado = asociacion.AsociacionId
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResultadoRegistro
            {
                Exitoso = false,
                Mensaje = "Error al registrar la asociación: " + ex.Message
            };
        }
    }

    public async Task<ResultadoRegistro> RegistrarComiteAsync(RegistroComiteDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. TbDatosComite
            var datosComite = dto.Adapt<TbDatosComite>();
            _context.TbDatosComite.Add(datosComite);
            await _context.SaveChangesAsync();

            // 2. TbDetalleRegComite
            var numRegistro = await _numeracionService.GenerarNumeroComite();
            var partes = numRegistro.Split('/');
            int secuencia = int.Parse(partes[0]);
            int anio = int.Parse(partes[1]);
            int mes = int.Parse(partes[2]);

            var detalle = new TbDetalleRegComite
            {
                ComiteId = datosComite.DcomiteId,
                TipoTramiteId = dto.TipoTramiteId,
                NumRegCoSecuencia = secuencia,
                NomRegCoAnio = anio,
                NumRegCoMes = mes,
                CreadaPor = "UsuarioActual"
            };
            _context.TbDetalleRegComite.Add(detalle);

            // 3. Miembros
            foreach (var miembroDto in dto.Miembros)
            {
                var miembro = miembroDto.Adapt<TbDatosMiembros>();
                miembro.DcomiteId = datosComite.DcomiteId;
                _context.TbDatosMiembros.Add(miembro);
            }

            // 4. Documentos
            if (dto.DocumentosSubir?.Count > 0)
            {
                await _documentoService.GuardarArchivosAsync(
                    dto.DocumentosSubir,
                    "Documento Legal",
                    detComiteId: datosComite.DcomiteId);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultadoRegistro
            {
                Exitoso = true,
                Mensaje = "Comité registrado con éxito.",
                IdGenerado = datosComite.DcomiteId
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResultadoRegistro
            {
                Exitoso = false,
                Mensaje = "Error al registrar el comité: " + ex.Message
            };
        }
    }
}
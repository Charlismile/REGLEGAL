using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroComiteService : IRegistroComite
    {
        private readonly IDbContextFactory<DbContextLegal> _contextFactory;
        private readonly ILogger<RegistroComiteService> _logger;
        private readonly IHistorialRegistro _historialRegistro;

        public RegistroComiteService(IDbContextFactory<DbContextLegal> contextFactory,
            ILogger<RegistroComiteService> logger, IHistorialRegistro historialRegistro)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _historialRegistro = historialRegistro;
        }

        // ===============================
        // 🔹 CREAR PERSONERÍA
        // ===============================
        public async Task<ResultModel> CrearPersoneria(PersoneriaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CreadaPor))
            {
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Database.SetCommandTimeout(120);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Crear comité de Personería
                var comite = new TbComite
                {
                    TipoTramite = (int)model.TipoTramiteEnum,
                    CreadaPor = model.CreadaPor,
                    FechaRegistro = DateTime.UtcNow,
                    NumeroResolucion = model.NumeroResolucion?.Trim(),
                    FechaResolucion = model.FechaResolucion,
                    NombreComiteSalud = model.NombreComiteSalud?.Trim() ?? string.Empty,
                    Comunidad = model.Comunidad?.Trim() ?? string.Empty,
                    NumeroNota = model.NumeroNota?.Trim() ?? string.Empty,
                    FechaEleccion = model.FechaCreacion ?? DateTime.UtcNow, 
                    RegionSaludId = model.RegionSaludId,
                    ProvinciaId = model.ProvinciaId,
                    DistritoId = model.DistritoId,
                    CorregimientoId = model.CorregimientoId
                };

                context.TbComite.Add(comite);
                await context.SaveChangesAsync();

                // Crear miembros
                if (model.Miembros?.Any() == true)
                {
                    var miembrosEntities = model.Miembros.Select(miembro => new TbMiembrosComite
                    {
                        ComiteId = comite.ComiteId,
                        NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                        ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                        CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                        CargoId = miembro.CargoId
                    }).ToList();

                    context.TbMiembrosComite.AddRange(miembrosEntities);
                    await context.SaveChangesAsync();
                }

                // Crear registro formal
                var detalleRegistro = new TbDetalleRegComite
                {
                    ComiteId = comite.ComiteId,
                    CreadaPor = model.CreadaPor,
                    CreadaEn = DateTime.UtcNow,
                    NumeroRegistro = model.NumeroResolucion?.Trim(),
                    CoEstadoSolicitudId = 1,
                    FechaResolucion = model.FechaResolucion
                };

                context.TbDetalleRegComite.Add(detalleRegistro);
                await context.SaveChangesAsync();

                // Historial en segundo plano
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await using var histContext = await _contextFactory.CreateDbContextAsync();
                        histContext.Database.SetCommandTimeout(30);

                        histContext.TbDetalleRegComiteHistorial.Add(new TbDetalleRegComiteHistorial
                        {
                            DetRegComiteId = detalleRegistro.DetRegComiteId,
                            ComiteId = comite.ComiteId,
                            CoEstadoSolicitudId = 1,
                            ComentarioCo = "Personería creada",
                            UsuarioRevisorCo = model.CreadaPor,
                            FechaCambioCo = DateTime.UtcNow
                        });

                        await histContext.SaveChangesAsync();
                    }
                    catch (Exception histEx)
                    {
                        _logger.LogWarning(histEx, "No se pudo registrar historial para personería {ComiteId}", comite.ComiteId);
                    }
                });

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = comite.ComiteId,
                    Message = "Personería registrada correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear personería para usuario {Usuario}", model.CreadaPor);
                return new ResultModel { Success = false, Message = $"Error al crear personería: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 REGISTRAR CAMBIO DE DIRECTIVA
        // ===============================
        public async Task<ResultModel> RegistrarCambioDirectiva(CambioDirectivaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CreadaPor))
            {
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Database.SetCommandTimeout(120);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el comité base existe
                var comiteBase = await context.TbComite.FindAsync(model.ComiteBaseId);
                if (comiteBase == null)
                    return new ResultModel { Success = false, Message = "Comité base no encontrado." };

                // Crear registro de Cambio de Directiva
                var cambioDirectiva = new TbComite
                {
                    TipoTramite = (int)model.TipoTramiteEnum,
                    CreadaPor = model.CreadaPor,
                    FechaRegistro = DateTime.UtcNow,
                    NumeroResolucion = model.NumeroResolucion?.Trim(),
                    FechaResolucion = model.FechaResolucion,
                    FechaEleccion = model.FechaEleccion ?? DateTime.UtcNow, // Fixed: Now using nullable DateTime?
                    
                    // Heredar datos del comité base
                    NombreComiteSalud = comiteBase.NombreComiteSalud,
                    Comunidad = comiteBase.Comunidad,
                    RegionSaludId = comiteBase.RegionSaludId,
                    ProvinciaId = comiteBase.ProvinciaId,
                    DistritoId = comiteBase.DistritoId,
                    CorregimientoId = comiteBase.CorregimientoId,
                    
                    NumeroNota = "N/A" // No aplica
                };

                context.TbComite.Add(cambioDirectiva);
                await context.SaveChangesAsync();

                // Crear nuevos miembros
                if (model.Miembros?.Any() == true)
                {
                    var miembrosEntities = model.Miembros.Select(miembro => new TbMiembrosComite
                    {
                        ComiteId = cambioDirectiva.ComiteId,
                        NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                        ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                        CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                        CargoId = miembro.CargoId
                    }).ToList();

                    context.TbMiembrosComite.AddRange(miembrosEntities);
                    await context.SaveChangesAsync();
                }

                // Crear registro formal
                var detalleRegistro = new TbDetalleRegComite
                {
                    ComiteId = cambioDirectiva.ComiteId,
                    CreadaPor = model.CreadaPor,
                    CreadaEn = DateTime.UtcNow,
                    NumeroRegistro = model.NumeroResolucion?.Trim(),
                    CoEstadoSolicitudId = 1,
                    FechaResolucion = model.FechaResolucion
                };

                context.TbDetalleRegComite.Add(detalleRegistro);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = cambioDirectiva.ComiteId,
                    Message = "Cambio de directiva registrado correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar cambio de directiva para comité base {ComiteBaseId}", model.ComiteBaseId);
                return new ResultModel { Success = false, Message = $"Error al registrar cambio de directiva: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 REGISTRAR JUNTA INTERVENTORA
        // ===============================
        public async Task<ResultModel> RegistrarJuntaInterventora(JuntaInterventoraModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CreadaPor))
            {
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Database.SetCommandTimeout(120);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el comité base existe
                var comiteBase = await context.TbComite.FindAsync(model.ComiteBaseId);
                if (comiteBase == null)
                    return new ResultModel { Success = false, Message = "Comité base no encontrado." };

                // Crear registro de Junta Interventora
                var juntaInterventora = new TbComite
                {
                    TipoTramite = (int)model.TipoTramiteEnum,
                    CreadaPor = model.CreadaPor,
                    FechaRegistro = DateTime.UtcNow,
                    NumeroResolucion = model.NumeroResolucion?.Trim(),
                    FechaResolucion = model.FechaResolucion,
                    FechaEleccion = model.FechaEleccion ?? DateTime.UtcNow, // Fixed: Now using nullable DateTime?
                    
                    // Heredar datos del comité base
                    NombreComiteSalud = comiteBase.NombreComiteSalud,
                    Comunidad = comiteBase.Comunidad,
                    RegionSaludId = comiteBase.RegionSaludId,
                    ProvinciaId = comiteBase.ProvinciaId,
                    DistritoId = comiteBase.DistritoId,
                    CorregimientoId = comiteBase.CorregimientoId,
                    
                    NumeroNota = "N/A" // No aplica
                };

                context.TbComite.Add(juntaInterventora);
                await context.SaveChangesAsync();

                // Crear interventores
                if (model.Interventores?.Any() == true)
                {
                    var interventoresEntities = model.Interventores.Select(miembro => new TbMiembrosComite
                    {
                        ComiteId = juntaInterventora.ComiteId,
                        NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                        ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                        CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                        CargoId = miembro.CargoId
                    }).ToList();

                    context.TbMiembrosComite.AddRange(interventoresEntities);
                    await context.SaveChangesAsync();
                }

                // Crear registro formal
                var detalleRegistro = new TbDetalleRegComite
                {
                    ComiteId = juntaInterventora.ComiteId,
                    CreadaPor = model.CreadaPor,
                    CreadaEn = DateTime.UtcNow,
                    NumeroRegistro = model.NumeroResolucion?.Trim(),
                    CoEstadoSolicitudId = 1,
                    FechaResolucion = model.FechaResolucion
                };

                context.TbDetalleRegComite.Add(detalleRegistro);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = juntaInterventora.ComiteId,
                    Message = "Junta interventora registrada correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar junta interventora para comité base {ComiteBaseId}", model.ComiteBaseId);
                return new ResultModel { Success = false, Message = $"Error al registrar junta interventora: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 ACTUALIZAR COMITÉ (para compatibilidad)
        // ===============================
        public async Task<ResultModel> ActualizarComite(ComiteModel model)
        {
            if (model.ComiteId <= 0)
                return new ResultModel { Success = false, Message = "ID de comité inválido." };

            if (string.IsNullOrWhiteSpace(model.CreadaPor))
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };

            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Cargar comité con miembros
                var comite = await context.TbComite
                    .Include(c => c.TbMiembrosComite)
                    .FirstOrDefaultAsync(c => c.ComiteId == model.ComiteId);

                if (comite == null)
                    return new ResultModel { Success = false, Message = "Comité no encontrado." };

                // Cargar registro formal
                var detalleRegistro = await context.TbDetalleRegComite
                    .FirstOrDefaultAsync(d => d.ComiteId == model.ComiteId);

                if (detalleRegistro == null)
                    return new ResultModel { Success = false, Message = "Registro formal no encontrado." };

                // Actualizar comité
                comite.NombreComiteSalud = model.NombreComiteSalud?.Trim() ?? comite.NombreComiteSalud;
                comite.Comunidad = model.Comunidad?.Trim() ?? comite.Comunidad;
                comite.NumeroResolucion = model.NumeroResolucion?.Trim() ?? comite.NumeroResolucion;
                comite.NumeroNota = model.NumeroNota?.Trim() ?? comite.NumeroNota;
                comite.FechaResolucion = model.FechaResolucion;
                comite.FechaEleccion = model.FechaEleccion;
                comite.RegionSaludId = model.RegionSaludId;
                comite.ProvinciaId = model.ProvinciaId;
                comite.DistritoId = model.DistritoId;
                comite.CorregimientoId = model.CorregimientoId;

                // Actualizar miembros (reemplazar)
                context.TbMiembrosComite.RemoveRange(comite.TbMiembrosComite);
                if (model.Miembros?.Any() == true)
                {
                    foreach (var miembro in model.Miembros)
                    {
                        context.TbMiembrosComite.Add(new TbMiembrosComite
                        {
                            ComiteId = comite.ComiteId,
                            NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                            ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                            CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                            CargoId = miembro.CargoId
                        });
                    }
                }

                // Actualizar registro formal
                detalleRegistro.ModificadaPor = model.UsuarioId;
                detalleRegistro.ModificadaEn = DateTime.UtcNow;
                detalleRegistro.CoEstadoSolicitudId = model.EstadoId;

                context.TbComite.Update(comite);
                context.TbDetalleRegComite.Update(detalleRegistro);

                await context.SaveChangesAsync();

                // Registrar en historial
                await _historialRegistro.RegistrarHistorialComiteAsync(
                    detRegComiteId: detalleRegistro.DetRegComiteId,
                    comiteId: comite.ComiteId,
                    estadoId: 1,
                    comentario: "Comité actualizado",
                    usuarioId: model.CreadaPor
                );

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = comite.ComiteId,
                    Message = "Comité actualizado correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comité");
                await transaction.RollbackAsync();
                return new ResultModel { Success = false, Message = $"Error al actualizar comité: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 ELIMINAR COMITÉ
        // ===============================
        public async Task<ResultModel> EliminarComite(int comiteId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();

            try
            {
                var entity = await context.TbComite.FindAsync(comiteId);

                if (entity == null)
                {
                    result.Message = "Comité no encontrado.";
                    return result;
                }

                context.TbComite.Remove(entity);
                await context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Comité eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comité");
                result.Message = $"Error al eliminar comité: {ex.Message}";
            }

            return result;
        }

        // ===============================
        // 🔹 OBTENER TODOS LOS COMITÉS
        // ===============================
        public async Task<List<ComiteModel>> ObtenerComites()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TbComite
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    TipoTramiteEnum = (TipoTramite)c.TipoTramite
                })
                .OrderBy(c => c.NombreComiteSalud)
                .ToListAsync();
        }

        // ===============================
        // 🔹 OBTENER COMITÉS POR TIPO DE TRÁMITE
        // ===============================
        public async Task<List<ComiteModel>> ObtenerComitesPorTipo(TipoTramite tipoTramite)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TbComite
                .Where(c => c.TipoTramite == (int)tipoTramite)
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    TipoTramiteEnum = (TipoTramite)c.TipoTramite
                })
                .OrderBy(c => c.NombreComiteSalud)
                .ToListAsync();
        }

        // ===============================
        // 🔹 OBTENER COMITÉ COMPLETO
        // ===============================
        public async Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var entity = await context.TbComite
                .Include(c => c.TbMiembrosComite)
                .Include(c => c.TbArchivosComite)
                .FirstOrDefaultAsync(c => c.ComiteId == comiteId);

            if (entity == null) return null;

            return new ComiteModel
            {
                ComiteId = entity.ComiteId,
                TipoTramiteEnum = (TipoTramite)entity.TipoTramite,
                NombreComiteSalud = entity.NombreComiteSalud,
                Comunidad = entity.Comunidad,
                FechaCreacion = entity.FechaRegistro,
                FechaEleccion = entity.FechaEleccion,
                NumeroResolucion = entity.NumeroResolucion,
                NumeroNota = entity.NumeroNota,
                FechaResolucion = entity.FechaResolucion,
                RegionSaludId = entity.RegionSaludId,
                ProvinciaId = entity.ProvinciaId,
                DistritoId = entity.DistritoId,
                CorregimientoId = entity.CorregimientoId,
                Miembros = entity.TbMiembrosComite.Select(m => new MiembroComiteModel
                {
                    MiembroId = m.MiembroId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId
                }).ToList()
            };
        }

        // ===============================
        // 🔹 OBTENER HISTORIAL DE COMITÉ
        // ===============================
        public async Task<List<HistorialComiteModel>> ObtenerHistorialComite(int comiteId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var historial = await context.TbDetalleRegComiteHistorial
                .Where(h => h.ComiteId == comiteId)
                .OrderByDescending(h => h.FechaCambioCo)
                .Select(h => new HistorialComiteModel
                {
                    HistorialId = h.RegComiteSolId,
                    ComiteId = h.ComiteId,
                    Accion = h.CoEstadoSolicitudId.ToString(),
                    Comentario = h.ComentarioCo,
                    Usuario = h.UsuarioRevisorCo,
                    Fecha = h.FechaCambioCo ?? DateTime.Today,
                })
                .ToListAsync();

            foreach (var item in historial)
            {
                item.Accion = MapEstadoAccion(int.Parse(item.Accion));
            }

            return historial;
        }

        // ===============================
        // 🔹 GUARDAR ARCHIVO DE RESOLUCIÓN
        // ===============================
        public async Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();
            try
            {
                var entity = await context.TbComite.FindAsync(comiteId);
                if (entity == null)
                {
                    result.Message = "Comité no encontrado.";
                    return result;
                }

                var nombreArchivo = $"{Guid.NewGuid()}_{archivo.Name}";
                var ruta = Path.Combine("wwwroot", "uploads", nombreArchivo);

                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

                await using (var stream = File.Create(ruta))
                {
                    await archivo.OpenReadStream().CopyToAsync(stream);
                }

                var nuevoArchivo = new TbArchivosComite
                {
                    ComiteId = comiteId,
                    NombreArchivoGuardado = archivo.Name,
                    Url = ruta,
                    FechaSubida = DateTime.Now
                };
                context.TbArchivosComite.Add(nuevoArchivo);
                await context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Archivo guardado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo");
                result.Message = $"Error al guardar archivo: {ex.Message}";
            }

            return result;
        }

        // ===============================
        // 🔹 MÉTODO AUXILIAR PARA MAPEAR ESTADOS
        // ===============================
        private string MapEstadoAccion(int estadoId)
        {
            return estadoId switch
            {
                1 => "Creado",
                2 => "En revisión",
                3 => "Aprobado",
                4 => "Rechazado",
                5 => "Actualizado",
                _ => $"Estado {estadoId}"
            };
        }

        // ===============================
        // 🔹 MÉTODOS OBSOLETOS (para compatibilidad temporal)
        // ===============================
        
        [Obsolete("Usar CrearPersoneria en su lugar")]
        public async Task<ResultModel> CrearComite(ComiteModel model)
        {
            // Redirigir al método específico según el tipo
            return model.TipoTramiteEnum switch
            {
                TipoTramite.Personeria => await CrearPersoneria(ConvertToPersoneria(model)),
                TipoTramite.CambioDirectiva => await RegistrarCambioDirectiva(ConvertToCambioDirectiva(model)),
                TipoTramite.JuntaInterventora => await RegistrarJuntaInterventora(ConvertToJuntaInterventora(model)),
                _ => new ResultModel { Success = false, Message = "Tipo de trámite no válido" }
            };
        }

        [Obsolete("Usar RegistrarCambioDirectiva en su lugar")]
        public async Task<ResultModel> RegistrarCambioDirectiva(ComiteModel model, int comiteBaseId)
        {
            var cambioDirectivaModel = ConvertToCambioDirectiva(model);
            cambioDirectivaModel.ComiteBaseId = comiteBaseId;
            return await RegistrarCambioDirectiva(cambioDirectivaModel);
        }

        [Obsolete("Usar RegistrarJuntaInterventora en su lugar")]
        public async Task<ResultModel> RegistrarJuntaInterventora(ComiteModel model, int comiteBaseId)
        {
            var juntaInterventoraModel = ConvertToJuntaInterventora(model);
            juntaInterventoraModel.ComiteBaseId = comiteBaseId;
            return await RegistrarJuntaInterventora(juntaInterventoraModel);
        }

        // ===============================
        // 🔹 MÉTODOS DE CONVERSIÓN (para compatibilidad)
        // ===============================
        private PersoneriaModel ConvertToPersoneria(ComiteModel model)
        {
            return new PersoneriaModel
            {
                ComiteId = model.ComiteId,
                UsuarioId = model.UsuarioId,
                CreadaPor = model.CreadaPor,
                EstadoId = model.EstadoId,
                NumeroResolucion = model.NumeroResolucion,
                FechaResolucion = model.FechaResolucion,
                DocumentosSubir = model.DocumentosSubir,
                Archivos = model.Archivos,
                TipoTramiteEnum = model.TipoTramiteEnum,
                NombreComiteSalud = model.NombreComiteSalud,
                Comunidad = model.Comunidad,
                FechaCreacion = model.FechaCreacion ?? DateTime.Now, // Fixed: Handle nullable DateTime?
                FechaEleccion = model.FechaEleccion,
                NumeroNota = model.NumeroNota,
                RegionSaludId = model.RegionSaludId,
                ProvinciaId = model.ProvinciaId,
                DistritoId = model.DistritoId,
                CorregimientoId = model.CorregimientoId,
                Miembros = model.Miembros,
                MiembrosInterventores = model.MiembrosInterventores,
                Historial = model.Historial,
                CedulaFile = model.CedulaFile,
                CedulaPreviewUrl = model.CedulaPreviewUrl,
                PasaporteFile = model.PasaporteFile,
                PasaportePreviewUrl = model.PasaportePreviewUrl,
                ComiteBaseId = model.ComiteBaseId
            };
        }

        private CambioDirectivaModel ConvertToCambioDirectiva(ComiteModel model)
        {
            return new CambioDirectivaModel
            {
                ComiteId = model.ComiteId,
                UsuarioId = model.UsuarioId,
                CreadaPor = model.CreadaPor,
                EstadoId = model.EstadoId,
                NumeroResolucion = model.NumeroResolucion,
                FechaResolucion = model.FechaResolucion,
                DocumentosSubir = model.DocumentosSubir,
                Archivos = model.Archivos,
                TipoTramiteEnum = model.TipoTramiteEnum,
                NombreComiteSalud = model.NombreComiteSalud,
                Comunidad = model.Comunidad,
                FechaCreacion = model.FechaCreacion,
                FechaEleccion = model.FechaEleccion ?? DateTime.Now, // Fixed: Handle nullable DateTime?
                NumeroNota = model.NumeroNota,
                RegionSaludId = model.RegionSaludId,
                ProvinciaId = model.ProvinciaId,
                DistritoId = model.DistritoId,
                CorregimientoId = model.CorregimientoId,
                Miembros = model.Miembros,
                MiembrosInterventores = model.MiembrosInterventores,
                Historial = model.Historial,
                CedulaFile = model.CedulaFile,
                CedulaPreviewUrl = model.CedulaPreviewUrl,
                PasaporteFile = model.PasaporteFile,
                PasaportePreviewUrl = model.PasaportePreviewUrl,
                ComiteBaseId = model.ComiteBaseId ?? 0
            };
        }

        private JuntaInterventoraModel ConvertToJuntaInterventora(ComiteModel model)
        {
            return new JuntaInterventoraModel
            {
                ComiteId = model.ComiteId,
                UsuarioId = model.UsuarioId,
                CreadaPor = model.CreadaPor,
                EstadoId = model.EstadoId,
                NumeroResolucion = model.NumeroResolucion,
                FechaResolucion = model.FechaResolucion,
                DocumentosSubir = model.DocumentosSubir,
                Archivos = model.Archivos,
                TipoTramiteEnum = model.TipoTramiteEnum,
                NombreComiteSalud = model.NombreComiteSalud,
                Comunidad = model.Comunidad,
                FechaCreacion = model.FechaCreacion,
                FechaEleccion = model.FechaEleccion ?? DateTime.Now, // Fixed: Handle nullable DateTime?
                NumeroNota = model.NumeroNota,
                RegionSaludId = model.RegionSaludId,
                ProvinciaId = model.ProvinciaId,
                DistritoId = model.DistritoId,
                CorregimientoId = model.CorregimientoId,
                Miembros = model.Miembros,
                MiembrosInterventores = model.MiembrosInterventores,
                Historial = model.Historial,
                CedulaFile = model.CedulaFile,
                CedulaPreviewUrl = model.CedulaPreviewUrl,
                PasaporteFile = model.PasaporteFile,
                PasaportePreviewUrl = model.PasaportePreviewUrl,
                ComiteBaseId = model.ComiteBaseId ?? 0,
                Interventores = model.MiembrosInterventores
            };
        }
    }
}
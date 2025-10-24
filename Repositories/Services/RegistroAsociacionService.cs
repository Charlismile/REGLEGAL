﻿using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroAsociacionService : IRegistroAsociacion
    {
        private readonly DbContextLegal _context;
        private readonly IHistorialRegistro _historialRegistro;

        public RegistroAsociacionService(DbContextLegal context, IHistorialRegistro historialRegistro)
        {
            _context = context;
            _historialRegistro = historialRegistro;
        }

        // ========================
        // CRUD Básico
        // ========================
        public async Task<ResultModel> CrearAsociacion(AsociacionModel model)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(model.NombreAsociacion))
                return new ResultModel { Success = false, Message = "El nombre de la asociación es requerido." };

            if (string.IsNullOrWhiteSpace(model.UsuarioId))
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // =============== 1. Crear la asociación ===============
                var asociacion = new TbAsociacion
                {
                    NombreAsociacion = model.NombreAsociacion.Trim(),
                    FechaResolucion = model.FechaResolucion ?? DateTime.Now,
                    Folio = model.Folio,
                    Actividad = model.Actividad?.Trim(),
                    NumeroResolucion = model.NumeroResolucion?.Trim()
                };

                // Representante Legal
                if (!string.IsNullOrWhiteSpace(model.NombreRepLegal))
                {
                    asociacion.RepLegal = new TbRepresentanteLegal
                    {
                        NombreRepLegal = model.NombreRepLegal.Trim(),
                        ApellidoRepLegal = model.ApellidoRepLegal?.Trim() ?? string.Empty,
                        CedulaRepLegal = model.CedulaRepLegal?.Trim() ?? string.Empty,
                        CargoRepLegal = model.CargoRepLegal?.Trim() ?? string.Empty,
                        TelefonoRepLegal = model.TelefonoRepLegal?.Trim(),
                        DireccionRepLegal = model.DireccionRepLegal?.Trim()
                    };
                }

                // Apoderado Legal
                if (!string.IsNullOrWhiteSpace(model.NombreApoAbogado))
                {
                    asociacion.ApoAbogado = new TbApoderadoLegal
                    {
                        NombreApoAbogado = model.NombreApoAbogado.Trim(),
                        ApellidoApoAbogado = model.ApellidoApoAbogado?.Trim() ?? string.Empty,
                        CedulaApoAbogado = model.CedulaApoAbogado?.Trim() ?? string.Empty,
                        TelefonoApoAbogado = model.TelefonoApoAbogado?.Trim(),
                        DireccionApoAbogado = model.DireccionApoAbogado?.Trim(),
                        CorreoApoAbogado = model.CorreoApoAbogado?.Trim()
                    };
                }

                _context.TbAsociacion.Add(asociacion);
                await _context.SaveChangesAsync(); // Obtiene AsociacionId

                // =============== 2. Obtener o crear secuencia de registro ===============
                var ahora = DateTime.UtcNow;
                var secuencia = await _context.TbRegSecuencia
                    .Where(s => s.EntidadId == 1 && s.Activo) // Supón que EntidadId=1 → Asociaciones
                    .FirstOrDefaultAsync();

                if (secuencia == null)
                {
                    secuencia = new TbRegSecuencia
                    {
                        EntidadId = 1,
                        Anio = ahora.Year,
                        Numeracion = 1,
                        Activo = true
                    };
                    _context.TbRegSecuencia.Add(secuencia);
                }
                else
                {
                    if (secuencia.Anio < ahora.Year)
                    {
                        secuencia.Anio = ahora.Year;
                        secuencia.Numeracion = 1;
                    }
                    else
                    {
                        secuencia.Numeracion++;
                    }
                }

                await _context.SaveChangesAsync(); // Guarda secuencia actualizada

                // =============== 3. Crear el registro formal ===============
                var detalleRegistro = new TbDetalleRegAsociacion
                {
                    AsociacionId = asociacion.AsociacionId,
                    CreadaPor = model.UsuarioId,
                    CreadaEn = ahora,
                    NumRegAsecuencia = secuencia.Numeracion,
                    NomRegAanio = secuencia.Anio,
                    NumRegAmes = ahora.Month,
                    NumeroResolucion = model.NumeroResolucion?.Trim(),
                    FechaResolucion = model.FechaResolucion
                };

                _context.TbDetalleRegAsociacion.Add(detalleRegistro);
                await _context.SaveChangesAsync(); // Obtiene DetRegAsociacionId

                // =============== 4. Registrar en historial ===============
                await _historialRegistro.RegistrarHistorialAsociacionAsync(
                    detRegAsociacionId: detalleRegistro.DetRegAsociacionId,
                    asociacionId: asociacion.AsociacionId,
                    accion: "Creada",
                    comentario: "Solicitud de registro inicial enviada",
                    usuarioId: model.UsuarioId
                );

                // =============== 5. Guardar archivos ===============
                if (model.Archivos != null && model.Archivos.Any())
                {
                    foreach (var archivo in model.Archivos)
                    {
                        if (string.IsNullOrWhiteSpace(archivo.NombreArchivoGuardado))
                        {
                            archivo.NombreArchivoGuardado = GenerateStoredFileName(archivo.NombreArchivo);
                        }

                        if (string.IsNullOrWhiteSpace(archivo.Categoria))
                            archivo.Categoria = "General";

                        var archivoEntity = new TbArchivosAsociacion
                        {
                            AsociacionId = asociacion.AsociacionId,
                            Categoria = archivo.Categoria,
                            NombreOriginal = archivo.NombreArchivo ?? "archivo",
                            NombreArchivoGuardado = archivo.NombreArchivoGuardado,
                            Url = archivo.RutaArchivo ?? string.Empty,
                            FechaSubida = DateTime.UtcNow,
                            Version = 1,
                            IsActivo = true
                        };

                        _context.TbArchivosAsociacion.Add(archivoEntity);
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Message = $"Asociación creada correctamente. Número de registro: {detalleRegistro.NumRegAcompleta}",
                    AsociacionId = asociacion.AsociacionId,
                    RegistroId = detalleRegistro.DetRegAsociacionId,
                    NumeroRegistro = detalleRegistro.NumRegAcompleta
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultModel
                {
                    Success = false,
                    Message = $"Error al crear la asociación: {ex.Message}"
                };
            }
        }

        public async Task<ResultModel> ActualizarAsociacion(AsociacionModel model)
        {
            if (model.AsociacionId <= 0)
                return new ResultModel { Success = false, Message = "ID de asociación inválido." };

            if (string.IsNullOrWhiteSpace(model.UsuarioId))
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // =============== 1. Cargar asociación con relaciones ===============
                var asociacion = await _context.TbAsociacion
                    .Include(a => a.RepLegal)
                    .Include(a => a.ApoAbogado)
                    .Include(a => a.TbArchivosAsociacion)
                    .FirstOrDefaultAsync(a => a.AsociacionId == model.AsociacionId);

                if (asociacion == null)
                    return new ResultModel { Success = false, Message = "Asociación no encontrada." };

                // =============== 2. Cargar el registro formal (detalle) ===============
                var detalleRegistro = await _context.TbDetalleRegAsociacion
                    .FirstOrDefaultAsync(d => d.AsociacionId == model.AsociacionId);

                if (detalleRegistro == null)
                    return new ResultModel { Success = false, Message = "Registro formal no encontrado." };

                // =============== 3. Actualizar datos de la asociación ===============
                asociacion.NombreAsociacion = model.NombreAsociacion?.Trim() ?? asociacion.NombreAsociacion;
                asociacion.FechaResolucion = model.FechaResolucion ?? asociacion.FechaResolucion;
                asociacion.Folio = model.Folio != 0 ? model.Folio : asociacion.Folio;
                asociacion.Actividad = model.Actividad?.Trim() ?? asociacion.Actividad;
                asociacion.NumeroResolucion = model.NumeroResolucion?.Trim() ?? asociacion.NumeroResolucion;

                // =============== 4. Actualizar representante legal ===============
                if (asociacion.RepLegal != null)
                {
                    asociacion.RepLegal.NombreRepLegal =
                        model.NombreRepLegal?.Trim() ?? asociacion.RepLegal.NombreRepLegal;
                    asociacion.RepLegal.ApellidoRepLegal =
                        model.ApellidoRepLegal?.Trim() ?? asociacion.RepLegal.ApellidoRepLegal;
                    asociacion.RepLegal.CedulaRepLegal =
                        model.CedulaRepLegal?.Trim() ?? asociacion.RepLegal.CedulaRepLegal;
                    asociacion.RepLegal.CargoRepLegal =
                        model.CargoRepLegal?.Trim() ?? asociacion.RepLegal.CargoRepLegal;
                    asociacion.RepLegal.TelefonoRepLegal =
                        model.TelefonoRepLegal?.Trim() ?? asociacion.RepLegal.TelefonoRepLegal;
                    asociacion.RepLegal.DireccionRepLegal =
                        model.DireccionRepLegal?.Trim() ?? asociacion.RepLegal.DireccionRepLegal;
                }
                else if (!string.IsNullOrWhiteSpace(model.NombreRepLegal))
                {
                    asociacion.RepLegal = new TbRepresentanteLegal
                    {
                        NombreRepLegal = model.NombreRepLegal.Trim(),
                        ApellidoRepLegal = model.ApellidoRepLegal?.Trim() ?? string.Empty,
                        CedulaRepLegal = model.CedulaRepLegal?.Trim() ?? string.Empty,
                        CargoRepLegal = model.CargoRepLegal?.Trim() ?? string.Empty,
                        TelefonoRepLegal = model.TelefonoRepLegal?.Trim(),
                        DireccionRepLegal = model.DireccionRepLegal?.Trim()
                    };
                }

                // =============== 5. Actualizar apoderado legal ===============
                if (asociacion.ApoAbogado != null)
                {
                    asociacion.ApoAbogado.NombreApoAbogado =
                        model.NombreApoAbogado?.Trim() ?? asociacion.ApoAbogado.NombreApoAbogado;
                    asociacion.ApoAbogado.ApellidoApoAbogado =
                        model.ApellidoApoAbogado?.Trim() ?? asociacion.ApoAbogado.ApellidoApoAbogado;
                    asociacion.ApoAbogado.CedulaApoAbogado =
                        model.CedulaApoAbogado?.Trim() ?? asociacion.ApoAbogado.CedulaApoAbogado;
                    asociacion.ApoAbogado.TelefonoApoAbogado =
                        model.TelefonoApoAbogado?.Trim() ?? asociacion.ApoAbogado.TelefonoApoAbogado;
                    asociacion.ApoAbogado.DireccionApoAbogado = model.DireccionApoAbogado?.Trim() ??
                                                                asociacion.ApoAbogado.DireccionApoAbogado;
                    asociacion.ApoAbogado.CorreoApoAbogado =
                        model.CorreoApoAbogado?.Trim() ?? asociacion.ApoAbogado.CorreoApoAbogado;
                }
                else if (!string.IsNullOrWhiteSpace(model.NombreApoAbogado))
                {
                    asociacion.ApoAbogado = new TbApoderadoLegal
                    {
                        NombreApoAbogado = model.NombreApoAbogado.Trim(),
                        ApellidoApoAbogado = model.ApellidoApoAbogado?.Trim() ?? string.Empty,
                        CedulaApoAbogado = model.CedulaApoAbogado?.Trim() ?? string.Empty,
                        TelefonoApoAbogado = model.TelefonoApoAbogado?.Trim(),
                        DireccionApoAbogado = model.DireccionApoAbogado?.Trim(),
                        CorreoApoAbogado = model.CorreoApoAbogado?.Trim()
                    };
                }

                // =============== 6. Actualizar el registro formal (si hay cambios relevantes) ===============
                bool registroModificado = false;
                if (model.NumeroResolucion != null)
                {
                    detalleRegistro.NumeroResolucion = model.NumeroResolucion.Trim();
                    detalleRegistro.FechaResolucion = model.FechaResolucion;
                    detalleRegistro.CreadaPor = model.UsuarioId;
                    detalleRegistro.CreadaEn = DateTime.UtcNow;
                    registroModificado = true;
                }

                // Guardar cambios principales
                _context.TbAsociacion.Update(asociacion);
                if (registroModificado)
                    _context.TbDetalleRegAsociacion.Update(detalleRegistro);

                await _context.SaveChangesAsync();

                // =============== 7. Registrar en historial ===============
                await _historialRegistro.RegistrarHistorialAsociacionAsync(
                    detRegAsociacionId: detalleRegistro.DetRegAsociacionId,
                    asociacionId: asociacion.AsociacionId,
                    accion: "Actualizada",
                    comentario: "Solicitud actualizada por el usuario",
                    usuarioId: model.UsuarioId
                );

                // =============== 8. Manejo de nuevos archivos (solo agregar, no reemplazar) ===============
                if (model.Archivos != null && model.Archivos.Any())
                {
                    foreach (var archivo in model.Archivos)
                    {
                        // Solo procesar archivos nuevos (sin ID)
                        if (archivo.AsociacionArchivoId == 0)
                        {
                            var nombreGuardado = string.IsNullOrWhiteSpace(archivo.NombreArchivoGuardado)
                                ? GenerateStoredFileName(archivo.NombreArchivo)
                                : archivo.NombreArchivoGuardado;

                            var archivoEntity = new TbArchivosAsociacion
                            {
                                AsociacionId = asociacion.AsociacionId,
                                Categoria =
                                    string.IsNullOrWhiteSpace(archivo.Categoria) ? "General" : archivo.Categoria,
                                NombreOriginal = archivo.NombreArchivo ?? "archivo",
                                NombreArchivoGuardado = nombreGuardado,
                                Url = archivo.RutaArchivo ?? string.Empty,
                                FechaSubida = DateTime.UtcNow,
                                Version = 1,
                                IsActivo = true
                            };

                            _context.TbArchivosAsociacion.Add(archivoEntity);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Message = "Asociación actualizada correctamente.",
                    AsociacionId = asociacion.AsociacionId,
                    RegistroId = detalleRegistro.DetRegAsociacionId,
                    NumeroRegistro = detalleRegistro.NumRegAcompleta
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultModel
                {
                    Success = false,
                    Message = $"Error al actualizar la asociación: {ex.Message}"
                };
            }
        }

        public async Task<ResultModel> EliminarAsociacion(int id)
        {
            try
            {
                var entity = await _context.TbAsociacion.FindAsync(id);
                if (entity == null)
                    return new ResultModel { Success = false, Message = "Asociación no encontrada." };

                _context.TbAsociacion.Remove(entity);
                await _context.SaveChangesAsync();
                return new ResultModel { Success = true, Message = "Asociación eliminada correctamente." };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<AsociacionModel?> ObtenerPorId(int id)
        {
            // NOMBRES CORRECTOS en los Include
            var a = await _context.TbAsociacion
                .Include(a => a.RepLegal) // ← CORRECTO
                .Include(a => a.ApoAbogado) // ← CORRECTO
                .Include(a => a.TbArchivosAsociacion)
                .FirstOrDefaultAsync(a => a.AsociacionId == id);

            if (a == null) return null;

            return new AsociacionModel
            {
                AsociacionId = a.AsociacionId,
                NombreAsociacion = a.NombreAsociacion,
                // NOMBRES CORRECTOS de navegación
                NombreRepLegal = a.RepLegal?.NombreRepLegal,
                ApellidoRepLegal = a.RepLegal?.ApellidoRepLegal,
                CedulaRepLegal = a.RepLegal?.CedulaRepLegal,
                CargoRepLegal = a.RepLegal?.CargoRepLegal,
                NombreApoAbogado = a.ApoAbogado?.NombreApoAbogado,
                ApellidoApoAbogado = a.ApoAbogado?.ApellidoApoAbogado,
                CedulaApoAbogado = a.ApoAbogado?.CedulaApoAbogado,
                FechaResolucion = a.FechaResolucion ?? DateTime.Now,
                Folio = a.Folio,
                Actividad = a.Actividad,
                NumeroResolucion = a.NumeroResolucion,
                Archivos = a.TbArchivosAsociacion?.Select(f => new AArchivoModel
                {
                    AsociacionArchivoId = f.ArchivoId,
                    NombreArchivo = f.NombreOriginal,
                    NombreArchivoGuardado = f.NombreArchivoGuardado,
                    RutaArchivo = f.Url,
                    Categoria = f.Categoria,
                    SubidoEn = f.FechaSubida
                }).ToList() ?? new List<AArchivoModel>()
            };
        }

        public async Task<List<AsociacionModel>> ObtenerTodas()
        {
            return await _context.TbAsociacion
                .Include(a => a.RepLegal) // ← CORRECTO
                .Include(a => a.ApoAbogado) // ← CORRECTO
                .Select(a => new AsociacionModel
                {
                    AsociacionId = a.AsociacionId,
                    NombreAsociacion = a.NombreAsociacion,
                    NombreRepLegal = a.RepLegal != null ? a.RepLegal.NombreRepLegal : null,
                    ApellidoRepLegal = a.RepLegal != null ? a.RepLegal.ApellidoRepLegal : null,
                    CedulaRepLegal = a.RepLegal != null ? a.RepLegal.CedulaRepLegal : null,
                    CargoRepLegal = a.RepLegal != null ? a.RepLegal.CargoRepLegal : null,
                    NombreApoAbogado = a.ApoAbogado != null ? a.ApoAbogado.NombreApoAbogado : null,
                    ApellidoApoAbogado = a.ApoAbogado != null ? a.ApoAbogado.ApellidoApoAbogado : null,
                    CedulaApoAbogado = a.ApoAbogado != null ? a.ApoAbogado.CedulaApoAbogado : null,
                    FechaResolucion = a.FechaResolucion ?? DateTime.Now,
                    Folio = a.Folio,
                    Actividad = a.Actividad,
                    NumeroResolucion = a.NumeroResolucion
                }).ToListAsync();
        }

        // ========================
        // Archivos
        // ========================
        public async Task<ResultModel> AgregarArchivo(int asociacionId, AArchivoModel archivo)
        {
            var nombreOriginal = archivo.NombreArchivo ?? "archivo";
            var nombreGuardado = archivo.NombreArchivoGuardado ?? GenerateStoredFileName(nombreOriginal);
            var categoria = string.IsNullOrWhiteSpace(archivo.Categoria) ? "General" : archivo.Categoria;

            var entity = new TbArchivosAsociacion
            {
                AsociacionId = asociacionId,
                Categoria = categoria,
                NombreOriginal = nombreOriginal,
                NombreArchivoGuardado = nombreGuardado,
                Url = archivo.RutaArchivo ?? string.Empty,
                FechaSubida = archivo.SubidoEn,
                Version = archivo.Version != 0 ? archivo.Version : 1,
                IsActivo = archivo.IsActivo
            };

            _context.TbArchivosAsociacion.Add(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo agregado correctamente." };
        }

        public async Task<ResultModel> EliminarArchivo(int archivoId)
        {
            var entity = await _context.TbArchivosAsociacion.FindAsync(archivoId);
            if (entity == null) return new ResultModel { Success = false, Message = "Archivo no encontrado." };

            _context.TbArchivosAsociacion.Remove(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo eliminado correctamente." };
        }

        public async Task<List<AArchivoModel>> ObtenerArchivos(int asociacionId)
        {
            return await _context.TbArchivosAsociacion
                .Where(a => a.AsociacionId == asociacionId)
                .Select(a => new AArchivoModel
                {
                    AsociacionArchivoId = a.ArchivoId,
                    NombreArchivo = a.NombreOriginal,
                    NombreArchivoGuardado = a.NombreArchivoGuardado,
                    RutaArchivo = a.Url,
                    Categoria = a.Categoria,
                    SubidoEn = a.FechaSubida,
                    Version = a.Version,
                    IsActivo = a.IsActivo
                }).ToListAsync();
        }

        // ========================
        // Historial
        // ========================
        public async Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId)
        {
            return await _context.TbDetalleRegAsociacionHistorial
                .Where(h => h.AsociacionId == asociacionId)
                .Select(h => new DetalleRegAsociacionModel
                {
                    DetalleRegAsociacionId = h.HistorialId,
                    CreadaEn = h.FechaModificacion ?? DateTime.Now,
                    CreadaPor = h.UsuarioId,
                    NumRegAsecuencia = h.FechaResolucion.HasValue ? h.FechaResolucion.Value.DayOfYear : 0,
                    NumRegAcompleta = h.NumeroResolucion ?? string.Empty,
                    Accion = h.Accion,
                    Comentario = h.Comentario
                })
                .ToListAsync();
        }

        // ------------------------
        // Helpers
        // ------------------------
        private static string GenerateStoredFileName(string originalName)
        {
            var safe = Path.GetFileNameWithoutExtension(originalName)
                .Replace(' ', '_')
                .Replace("ñ", "n")
                .Replace("Ñ", "N");
            var ext = Path.GetExtension(originalName);
            return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{safe}{ext}";
        }
    }
}
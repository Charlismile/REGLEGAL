// using Microsoft.EntityFrameworkCore;
// using REGISTROLEGAL.Models.Entities.BdSisLegal;
// using SISTEMALEGAL.DTOs;
// using SISTEMALEGAL.Repositories.Interfaces;
//
// namespace SISTEMALEGAL.Repositories.Services
// {
//     public class RegistroAsociacionService : IRegistroAsociacionService
//     {
//         private readonly DbContextLegal _context;
//
//         public RegistroAsociacionService(DbContextLegal context)
//         {
//             _context = context;
//         }
//
//         public async Task<ResultadoOperacion<int>> RegistrarAsociacion(RegistroAsociacionDTO dto, string usuarioId)
//         {
//             var resultado = new ResultadoOperacion<int>();
//
//             using var transaction = await _context.Database.BeginTransactionAsync();
//
//             try
//             {
//                 // Registrar apoderado legal
//                 var apoderado = new TbApoderadoLegal
//                 {
//                     NombreApoAbogado = dto.Apoderado.NombreApoAbogado,
//                     CedulaApoAbogado = dto.Apoderado.CedulaApoAbogado,
//                     TelefonoApoAbogado = dto.Apoderado.TelefonoApoAbogado,
//                     CorreoApoAbogado = dto.Apoderado.CorreoApoAbogado,
//                     DireccionApoAbogado = dto.Apoderado.DireccionApoAbogado
//                 };
//                 await _context.TbApoderadoLegal.AddAsync(apoderado);
//                 await _context.SaveChangesAsync();
//
//                 // Registrar representante legal
//                 var representante = new TbRepresentanteLegal
//                 {
//                     NombreRepLegal = dto.Representante.NombreRepLegal,
//                     CedulaRepLegal = dto.Representante.CedulaRepLegal,
//                     CargoRepLegal = dto.Representante.CargoRepLegal,
//                     TelefonoRepLegal = dto.Representante.TelefonoRepLegal,
//                     DireccionRepLegal = dto.Representante.DireccionRepLegal
//                 };
//                 await _context.TbRepresentanteLegal.AddAsync(representante);
//                 await _context.SaveChangesAsync();
//
//                 // Registrar asociación
//                 var asociacion = new TbAsociacion
//                 {
//                     NombreAsociacion = dto.NombreAsociacion,
//                     Folio = dto.Folio,
//                     Actividad = dto.Actividad,
//                     ApoderadoLegalId = apoderado.ApoAbogadoId,
//                     RepresentanteLegalId = representante.RepLegalId
//                 };
//                 await _context.TbAsociacion.AddAsync(asociacion);
//                 await _context.SaveChangesAsync();
//
//                 // Registrar detalle del registro
//                 const int TIPO_TRAMITE_ID = 4; // Asegúrate de tener un servicio o DTO para esto
//
//                 var detalle = new TbDetalleRegAsociacion
//                 {
//                     AsociacionId = asociacion.AsociacionId,
//                     CreadaPor = usuarioId,
//                     NumRegAsecuencia = await GetNextSecuencia(), // Correcto: minúscula
//                     NomRegAanio = DateTime.Now.Year,            // Correcto: minúscula
//                     NumRegAmes = DateTime.Now.Month             // Correcto: minúscula
//                 };
//
//                 await _context.TbDetalleRegAsociacion.AddAsync(detalle);
//                 await _context.SaveChangesAsync();
//
//                 await transaction.CommitAsync();
//
//                 resultado.Exito = true;
//                 resultado.Data = asociacion.AsociacionId;
//             }
//             catch (Exception ex)
//             {
//                 await transaction.RollbackAsync();
//                 resultado.Exito = false;
//                 resultado.Errores.Add(ex.Message);
//             }
//
//             return resultado;
//         }
//
//         private async Task<int> GetNextSecuencia()
//         {
//             var anio = DateTime.Now.Year;
//             var secuencia = await _context.TbRegSecuencia
//                 .Where(s => s.Anio == anio && s.Activo)
//                 .OrderByDescending(s => s.Numeracion)
//                 .FirstOrDefaultAsync();
//
//             return secuencia?.Numeracion + 1 ?? 1;
//         }
//     }
// }
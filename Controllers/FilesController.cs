using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly DbContextLegal _context;
        private readonly IArchivoLegalService _archivoService;

        public FilesController(DbContextLegal context, IArchivoLegalService archivoService)
        {
            _context = context;
            _archivoService = archivoService;
        }

        // GET: api/files/{tipoEntidad}/{id}
        [HttpGet("{tipoEntidad}/{id:int}")]
        public async Task<IActionResult> GetArchivo(string tipoEntidad, int id)
        {
            var doc = await _context.Documentos
                .Where(d => d.EntidadTipo.ToUpper() == tipoEntidad.ToUpper() && d.EntidadId == id && d.IsActivo)
                .OrderByDescending(d => d.SubidoEn)
                .FirstOrDefaultAsync();

            if (doc == null) return NotFound("Archivo no encontrado.");
            if (doc.FileContent == null || doc.FileContent.Length == 0)
                return NotFound("El archivo no tiene contenido almacenado.");

            return File(doc.FileContent, doc.ContentType, doc.NombreArchivo);
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file,
                                                    [FromForm] string tipoEntidad,
                                                    [FromForm] int entidadId,
                                                    [FromForm] string categoria = "GENERAL")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se recibió ningún archivo.");

            try
            {
                var bytes = await _archivoService.ValidateAndReadPdfAsync(file);

                var doc = await _archivoService.GuardarDocumentoAsync(tipoEntidad, entidadId, bytes, file.FileName, categoria);

                return Ok(new
                {
                    message = "Archivo subido con éxito",
                    id = doc.DocumentoId,
                    url = $"/api/files/{tipoEntidad}/{entidadId}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

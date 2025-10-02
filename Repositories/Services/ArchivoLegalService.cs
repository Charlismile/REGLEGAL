using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class ArchivoLegalService : IArchivoLegalService
{
    private readonly IDbContextFactory<DbContextLegal> _contextFactory;
    private readonly ILogger<ArchivoLegalService> _logger;
    private readonly FileUploadConfig _config;
    private readonly IWebHostEnvironment _env;
    private long MaxBytes => _config.MaxFileSizeMB * 1024 * 1024;

    public ArchivoLegalService(IDbContextFactory<DbContextLegal> contextFactory,
        ILogger<ArchivoLegalService> logger,
        IOptions<FileUploadConfig> config,
        IWebHostEnvironment env)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _config = config.Value;
        _env = env;
    }

    // Validación básica de PDF
    public async Task<byte[]> ValidateAndReadPdfAsync(IBrowserFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (!_config.AllowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Solo se permiten archivos PDF");

        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        if (ext != ".pdf") throw new InvalidOperationException("Extensión inválida");

        if (file.Size == 0) throw new InvalidOperationException("Archivo vacío");
        if (file.Size > MaxBytes) throw new InvalidOperationException($"Tamaño máximo {_config.MaxFileSizeMB} MB");

        await using var ms = new MemoryStream();
        await file.OpenReadStream(MaxBytes).CopyToAsync(ms);
        var bytes = ms.ToArray();

        if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
            throw new InvalidOperationException("No es un PDF válido.");

        return bytes;
    }

    // Guarda archivo en DB (desactiva versiones anteriores)
    public async Task<DocumentoModel> GuardarDocumentoAsync(string entidadTipo, int entidadId, byte[] bytes, string nombreArchivo, string categoria)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var anteriores = await context.Documentos
            .Where(d => d.EntidadTipo.ToUpper() == entidadTipo.ToUpper()
                        && d.EntidadId == entidadId
                        && d.Categoria.ToUpper() == categoria.ToUpper()
                        && d.IsActivo)
            .ToListAsync();

        anteriores.ForEach(a => a.IsActivo = false);

        var nuevo = new DocumentoModel
        {
            EntidadTipo = entidadTipo.ToUpper(),
            EntidadId = entidadId,
            NombreArchivo = nombreArchivo,
            FileContent = bytes,
            ContentType = "application/pdf",
            FileSize = bytes.Length,
            SubidoEn = DateTime.UtcNow,
            Categoria = categoria.ToUpper(),
            IsActivo = true,
            RutaArchivo = ""
        };

        context.Documentos.Add(nuevo);
        await context.SaveChangesAsync();

        return nuevo;
    }
}
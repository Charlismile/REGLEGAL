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
    private readonly string _rutaBaseComite;
    private readonly string _rutaBaseAsociacion;

    public ArchivoLegalService(
        IDbContextFactory<DbContextLegal> contextFactory,
        ILogger<ArchivoLegalService> logger,
        IOptions<FileUploadConfig> config,
        IWebHostEnvironment env)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _config = config.Value;
        _env = env;
        _rutaBaseComite = Path.Combine(_env.WebRootPath, "uploads", "documentos-comite");
        _rutaBaseAsociacion = Path.Combine(_env.WebRootPath, "uploads", "documentos-asociacion");
    }

    #region Validación PDF

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

    #endregion

    #region Comites

    // Guardar archivo físico + registro en BD con historial/versiones
    public async Task<CArchivoModel> GuardarArchivoComiteAsync(int comiteId, IBrowserFile archivo, string categoria)
    {
        _logger.LogInformation("Archivo recibido: {Nombre}, Tamaño: {Size}", archivo?.Name, archivo?.Size);

        var bytes = await ValidateAndReadPdfAsync(archivo);

        // Carpeta física por comité y categoría
        var rutaCategoria = Path.Combine(_rutaBaseComite, categoria, comiteId.ToString());
        if (!Directory.Exists(rutaCategoria))
            Directory.CreateDirectory(rutaCategoria);

        var nombreArchivoGuardado = Guid.NewGuid() + Path.GetExtension(archivo.Name);
        var filePath = Path.Combine(rutaCategoria, nombreArchivoGuardado);

        // Guardar físicamente
        await using (var stream = archivo.OpenReadStream(MaxBytes))
        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream);
        }

        // Guardar en BD y desactivar versiones anteriores
        await using var context = await _contextFactory.CreateDbContextAsync();

        var anteriores = await context.TbArchivosComite
            .Where(d => d.ComiteId == comiteId
                        && d.Categoria.ToUpper() == categoria.ToUpper()
                        && d.IsActivo)
            .ToListAsync();

        anteriores.ForEach(a => a.IsActivo = false);

        var nuevo = new TbArchivosComite
        {
            ComiteId = comiteId,
            Categoria = categoria.ToUpper(),
            NombreOriginal = archivo.Name,
            NombreArchivoGuardado = nombreArchivoGuardado,
            Url = $"/uploads/documentos-comite/{categoria}/{comiteId}/{nombreArchivoGuardado}",
            FechaSubida = DateTime.UtcNow,
            Version = (anteriores.Count > 0 ? anteriores.Max(a => a.Version) + 1 : 1),
            IsActivo = true
        };

        context.TbArchivosComite.Add(nuevo);
        await context.SaveChangesAsync();

        // Mapear a DTO
        return new CArchivoModel
        {
            ComiteArchivoId = nuevo.ArchivoId,
            NombreArchivo = nuevo.NombreOriginal,
            RutaArchivo = nuevo.Url,
            SubidoEn = nuevo.FechaSubida
        };
    }

    // Obtener archivos activos de un comité
    public async Task<List<CArchivoModel>> ObtenerArchivosComiteAsync(int comiteId, string categoria)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TbArchivosComite
            .Where(d => d.ComiteId == comiteId
                        && d.Categoria.ToUpper() == categoria.ToUpper()
                        && d.IsActivo)
            .OrderByDescending(d => d.FechaSubida)
            .Select(d => new CArchivoModel
            {
                ComiteArchivoId = d.ArchivoId,
                NombreArchivo = d.NombreOriginal,
                RutaArchivo = d.Url,
                SubidoEn = d.FechaSubida
            })
            .ToListAsync();
    }

    // Desactivar archivo (historial)
    public async Task<bool> DesactivarArchivoComiteAsync(int archivoId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var archivo = await context.TbArchivosComite.FindAsync(archivoId);
        if (archivo == null) return false;

        archivo.IsActivo = false;
        await context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Asociaciones

    public async Task<AArchivoModel> GuardarArchivoAsociacionAsync(int asociacionId, IBrowserFile file, string categoria)
    {
        try
        {
            Console.WriteLine($"📁 Iniciando guardado de: {file.Name} ({file.Size} bytes)");

            // 1. Crear directorio
            var rutaCategoria = Path.Combine(_rutaBaseAsociacion, categoria, asociacionId.ToString());
            if (!Directory.Exists(rutaCategoria))
                Directory.CreateDirectory(rutaCategoria);

            // 2. Generar nombre único y ruta
            var nombreArchivoGuardado = Guid.NewGuid() + Path.GetExtension(file.Name);
            var filePath = Path.Combine(rutaCategoria, nombreArchivoGuardado);

            // 3. Guardar archivo físico (SIMPLIFICADO)
            await using (var stream = file.OpenReadStream(50 * 1024 * 1024)) // 50MB
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            Console.WriteLine($"💾 Archivo físico guardado: {filePath}");

            // 4. Guardar en base de datos
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Desactivar versiones anteriores
            var anteriores = await context.TbArchivosAsociacion
                .Where(d => d.AsociacionId == asociacionId
                            && d.Categoria.ToUpper() == categoria.ToUpper()
                            && d.IsActivo)
                .ToListAsync();

            anteriores.ForEach(a => a.IsActivo = false);

            var nuevo = new TbArchivosAsociacion
            {
                AsociacionId = asociacionId,
                Categoria = categoria.ToUpper(),
                NombreOriginal = file.Name,
                NombreArchivoGuardado = nombreArchivoGuardado,
                Url = $"/uploads/documentos-asociacion/{categoria}/{asociacionId}/{nombreArchivoGuardado}",
                FechaSubida = DateTime.UtcNow,
                Version = (anteriores.Count > 0 ? anteriores.Max(a => a.Version) + 1 : 1),
                IsActivo = true
            };

            context.TbArchivosAsociacion.Add(nuevo);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Archivo registrado en BD con ID: {nuevo.ArchivoId}");

            return new AArchivoModel
            {
                Success = true, // AGREGAR ESTA PROPIEDAD
                AsociacionArchivoId = nuevo.ArchivoId,
                NombreArchivo = nuevo.NombreOriginal,
                RutaArchivo = nuevo.Url,
                SubidoEn = nuevo.FechaSubida
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR en GuardarArchivoAsociacionAsync: {ex.Message}");
            Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");

            return new AArchivoModel
            {
                Success = false, // AGREGAR ESTA PROPIEDAD
                Message = ex.Message
            };
        }
    }

    public async Task<List<AArchivoModel>> ObtenerArchivosAsociacionAsync(int asociacionId, string categoria)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TbArchivosAsociacion
            .Where(d => d.AsociacionId == asociacionId
                        && d.Categoria.ToUpper() == categoria.ToUpper()
                        && d.IsActivo)
            .OrderByDescending(d => d.FechaSubida)
            .Select(d => new AArchivoModel
            {
                AsociacionArchivoId = d.ArchivoId,
                NombreArchivo = d.NombreOriginal,
                RutaArchivo = d.Url,
                SubidoEn = d.FechaSubida
            })
            .ToListAsync();
    }

    public async Task<bool> DesactivarArchivoAsociacionAsync(int archivoId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var archivo = await context.TbArchivosAsociacion.FindAsync(archivoId);
        if (archivo == null) return false;

        archivo.IsActivo = false;
        await context.SaveChangesAsync();
        return true;
    }

    #endregion
}
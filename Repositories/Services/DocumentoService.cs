using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class DocumentoService : IDocumentoService
{
    private readonly DbContextLegal _context;
    private readonly string _basePath;

    public DocumentoService(DbContextLegal context, IWebHostEnvironment env)
    {
        _context = context;
        _basePath = Path.Combine(env.WebRootPath, "uploads", "documentos-legales");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> GuardarArchivoAsync(IBrowserFile archivo, string categoria, int? detAsociacionId = null, int? detComiteId = null)
    {
        var fileName = $"{Guid.NewGuid()}_{archivo.Name}";
        var filePath = Path.Combine(_basePath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archivo.OpenReadStream().CopyToAsync(stream);
        }

        var documento = new TbArchivos
        {
            Categoria = categoria,
            NombreOriginal = archivo.Name,
            NombreArchivoGuardado = fileName,
            Url = $"/uploads/documentos-legales/{fileName}",
            FechaSubida = DateTime.Now,
            Version = 1,
            IsActivo = true,
            DetRegAsociacionId = detAsociacionId,
            DetRegComiteId = detComiteId
        };

        _context.TbArchivos.Add(documento);
        await _context.SaveChangesAsync();

        return documento.Url;
    }

    public async Task<List<TbArchivos>> ObtenerDocumentosPorAsociacionAsync(int asociacionId)
    {
        return await _context.TbArchivos
            .Where(a => a.DetRegAsociacionId == asociacionId)
            .ToListAsync();
    }

    public async Task<List<TbArchivos>> ObtenerDocumentosPorComiteAsync(int comiteId)
    {
        return await _context.TbArchivos
            .Where(a => a.DetRegComiteId == comiteId)
            .ToListAsync();
    }
}
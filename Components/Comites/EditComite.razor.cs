using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Components.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    private ComiteModel? cModel;
    private TbComite? entidadDb;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine($"Cargando comité con id: {id}");

        entidadDb = await _context.TbComite
            .FirstOrDefaultAsync(e => e.DcomiteId == id);

        if (entidadDb == null)
        {
            Console.WriteLine($"No se encontró el comité con id: {id}");
        }

        if (entidadDb != null)
        {
            cModel = new ComiteModel
            {
                ComiteId = entidadDb.ComiteId,
                NombreComiteSalud = entidadDb.NombreComiteSalud ?? "",
                Comunidad = entidadDb.Comunidad ?? "",
                NumeroResolucion = entidadDb.NumeroResolucion ?? "",
                FechaResolucion = entidadDb.FechaResolucion ?? DateTime.Now,
                FechaCreacion    = entidadDb.FechaRegistro ?? DateTime.Now,
                FechaEleccion    = entidadDb.FechaEleccion ?? DateTime.Now,
                CreadaPor = entidadDb.CreadaPor ?? ""
            };
        }
    }


    private async Task Guardar()
    {
        if (entidadDb != null && cModel != null)
        {
            entidadDb.NombreComiteSalud = cModel.NombreComiteSalud;
            entidadDb.Comunidad = cModel.Comunidad;
            entidadDb.NumeroResolucion = cModel.NumeroResolucion;
            entidadDb.FechaResolucion = cModel.FechaResolucion;
            entidadDb.FechaEleccion = cModel.FechaEleccion;

            await _context.SaveChangesAsync();
            Navigation.NavigateTo("/comites");
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/comites");
    }
}
using Microsoft.AspNetCore.Components;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Components.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    private ComiteModel? cModel;
    private TbDetalleRegComite? entidadDb;

    protected override async Task OnInitializedAsync()
    {
        entidadDb = await _context.TbDetalleRegComite.FindAsync(id);
        if (entidadDb != null)
        {
            cModel = new ComiteModel
            {
                ComiteId = entidadDb.DetalleRegComiteId,
                NombreComiteSalud = entidadDb.NombreComiteSalud ?? "",
                NumeroResolucion = entidadDb.NumeroResolucion ?? "",
                FechaResolucion = entidadDb.FechaResolucion ?? DateTime.Now,
                FechaCreacion = entidadDb.FechaRegistro ?? DateTime.Now,
                FechaEleccion = entidadDb.FechaEleccion ?? DateTime.Now,
                CreadaPor = entidadDb.CreadaPor ?? ""
            };
        }
    }

    private async Task Guardar()
    {
        if (entidadDb != null && cModel != null)
        {
            entidadDb.NombreComiteSalud = cModel.NombreComiteSalud;
            entidadDb.NumeroResolucion = cModel.NumeroResolucion;
            entidadDb.FechaResolucion = cModel.FechaResolucion;
            entidadDb.FechaEleccion = cModel.FechaEleccion;

            _context.Update(entidadDb);
            await _context.SaveChangesAsync();
            Navigation.NavigateTo("/comites");
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/comites");
    }
}
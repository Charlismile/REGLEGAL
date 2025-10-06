using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Components.Comites;

public partial class Comites : ComponentBase
{
    private List<TbComite>? comites;

    protected override async Task OnInitializedAsync()
    {
        comites = await _context.TbComite
            .OrderByDescending(c => c.FechaRegistro)
            .ToListAsync();
    }

    private void Editar(int id)
    {
        Navigation.NavigateTo($"/comite/editar/{id}");
    }
}
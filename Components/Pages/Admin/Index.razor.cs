using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace REGISTROLEGAL.Components.Pages.Admin;

public partial class Index : ComponentBase
{
    private int totalComites;
    private int totalAsociaciones;

    protected override async Task OnInitializedAsync()
    {
        totalComites = await _context.TbComite.CountAsync();
        totalAsociaciones = await _context.TbAsociacion.CountAsync();
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Core;

namespace MachineTrading.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(Logger logger) : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        var exFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exFeature is not null)
        {
            var ex = exFeature.Error;
            logger.Error(exFeature.Error, $"Error Message: {ex.Message}, Time of occurrence {DateTime.UtcNow}");
        }
        
        
    }
}


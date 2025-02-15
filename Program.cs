using System.Net.Mime;
using Hangfire;
using MachineTrading.BackgroundJob;
using MachineTrading.Common;
using MachineTrading.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.AddDependencies();
builder.AddHangfire();

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<MachineTradingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MachineTradingContext")));


var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // using static System.Net.Mime.MediaTypeNames;
        context.Response.ContentType = MediaTypeNames.Text.Plain;
        
        var exFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exFeature is null)
        {
            await context.Response.WriteAsync("An exception was thrown.");
            return;
        }

        var ex = exFeature.Error;
        if (app.Environment.IsProduction())
        {
            await context.Response.WriteAsync($"An exception was thrown. ${ex.Message}");
            return;
        }
        
        await context.Response.WriteAsync(ex.ToString());
        Log.Error(ex, $"Error Message: {ex.Message}");
    });
});
app.UseHangfireDashboard();
if (app.Environment.IsDevelopment())
{

    
}
else
{
    app.UseHsts();
}


// HangfireHelper.SetUpdateArticleJobId();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
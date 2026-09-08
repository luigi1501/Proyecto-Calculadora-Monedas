using Microsoft.Extensions.FileProviders;
using System.IO;
using TasaPlus.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TasasDataService>();
builder.Services.AddOpenApi();

// Política de CORS permitiendo acceso al Frontend y App Móvil
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

// Configuración para servir la Landing Page (landing/index.html) como página principal por defecto en /
var rootLandingPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "landing"));
if (Directory.Exists(rootLandingPath))
{
    var fileProvider = new PhysicalFileProvider(rootLandingPath);
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    provider.Mappings[".apk"] = "application/vnd.android.package-archive";
    provider.Mappings[".exe"] = "application/vnd.microsoft.portable-executable";

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider,
        DefaultFileNames = new List<string> { "index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider,
        RequestPath = "",
        ContentTypeProvider = provider
    });
}

app.MapGet("/downloads/{filename}", (string filename) =>
{
    var filePath = Path.GetFullPath(Path.Combine(rootLandingPath, "downloads", filename));
    if (File.Exists(filePath))
    {
        var contentType = filename.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.android.package-archive"
            : "application/octet-stream";
        return Results.File(filePath, contentType: contentType, fileDownloadName: filename);
    }
    return Results.NotFound();
});

app.MapControllers();

app.Run();

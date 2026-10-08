using ApiForge.Application.Interfaces;
using ApiForge.Generator;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;
using Asp.Versioning;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})  
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddScoped<IOpenApiParser, OpenApiParser>();
builder.Services.AddScoped<ICodeGenerator, CodeGenerator>();
builder.Services.AddScoped<IApiForgeGenerator, ApiForgeGenerator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
using ApiForge.Application.Interfaces;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IOpenApiParser, OpenApiParser>();
builder.Services.AddScoped<ICodeGenerator, CodeGenerator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
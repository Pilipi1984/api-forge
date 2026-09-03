using ApiForge.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IOpenApiParser, ApiForge.Infrastructure.Parser.OpenApiParser>();
builder.Services.AddScoped<ICodeGenerator, ApiForge.Infrastructure.Generator.CodeGenerator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
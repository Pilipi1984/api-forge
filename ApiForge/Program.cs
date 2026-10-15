using ApiForge.Application.Interfaces;
using ApiForge.Generator;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;
using Asp.Versioning;

namespace ApiForge.Api
{
    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddApiVersioning(options =>
            {
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
        }
    }
}
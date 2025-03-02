using PfeProjet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PfeProjet.Controllers;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "azure devops api's .NET",
                Version = "v1",
                Description = "Documentation Swagger azure devops api's .NET"
            });
        });

        builder.Services.AddHttpClient<ProjectService>(client =>
        {
            client.DefaultRequestHeaders.Add("Authorization", "Basic YOUR_ENCODED_TOKEN");
        });

        builder.Services.AddHttpClient<PipelinesService>(client =>
        {
            client.DefaultRequestHeaders.Add("Authorization", "Basic YOUR_ENCODED_TOKEN");
        });

        builder.Services.AddHttpClient<ReleasesService>(client =>
        {
            client.DefaultRequestHeaders.Add("Authorization", "Basic YOUR_ENCODED_TOKEN");
        });

        builder.Services.AddHttpClient<AgentPoolsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Authorization", "Basic YOUR_ENCODED_TOKEN");
        });
        

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
         
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

}

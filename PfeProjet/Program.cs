using PfeProjet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PfeProjet.Controllers;
using MongoDB.Driver;
using PfeProjet;
using Microsoft.Extensions.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


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
        // MongoDB configuration
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

        // Register IMongoClient
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            if (string.IsNullOrEmpty(settings.ConnectionUrl))
            {
                throw new InvalidOperationException("MongoDB ConnectionUrl is not configured in appsettings.json");
            }
            Console.WriteLine($"MongoDB Connection URL: {settings.ConnectionUrl}");
            return new MongoClient(settings.ConnectionUrl);
        });

        // Register MongoDbContext
        builder.Services.AddSingleton<MongoDbContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            if (string.IsNullOrEmpty(settings.DatabaseName))
            {
                throw new InvalidOperationException("MongoDB DatabaseName is not configured in appsettings.json");
            }

            Console.WriteLine($"MongoDB Database Name: {settings.DatabaseName}");
            return new MongoDbContext(client, settings.DatabaseName);
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

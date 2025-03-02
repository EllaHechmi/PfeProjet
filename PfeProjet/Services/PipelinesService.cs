using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using PfeProjet.Models;
using System.Net.Http.Headers;
using System.Text;

namespace PfeProjet.Services
{

    public class PipelinesService
    {
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Pipeline> _pipelinesCollection;


        public PipelinesService(HttpClient httpClient, MongoDbContext mongoDbContext) 
        {
            _httpClient = httpClient;
            _pipelinesCollection = mongoDbContext.Pipelines;

        }

        public async Task<string> GetPipelinesAsync(string organisation, string pat, string project)
        {

            var apiUrl = $"https://dev.azure.com/{organisation}/{project}/_apis/pipelines?api-version=7.1";

            // Configuration de l'authentification et des en-têtes
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                Console.WriteLine($"Envoi de la requête GET à : {apiUrl}");
                 
                // Envoi de la requête GET
                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Affichage de la réponse
                Console.WriteLine($"Statut HTTP : {response.StatusCode}, Réponse : {responseContent}");


                if (response.IsSuccessStatusCode)
                {
                    // Désérialiser la réponse JSON
                    var pipelines = ParsePipelinesResponse(responseContent, organisation, project);

                    // Insérer les pipelines dans MongoDB
                    await InsertPipelinesAsync(pipelines);

                    return "Pipelines récupérés et stockés avec succès dans MongoDB.";
                }
                else
                {
                    return $"Erreur lors de la récupération des pipelines : {responseContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return $"Erreur : {ex.Message}";
            }
        }

        private List<Pipeline> ParsePipelinesResponse(string responseContent, string organisation, string project)
        {
            var pipelines = new List<Pipeline>();
            var jsonResponse = JObject.Parse(responseContent);

            // Parcourir les pipelines dans la réponse JSON
            foreach (var item in jsonResponse["value"])
            {
                var pipeline = new Pipeline
                {
                    Id = item["id"].ToObject<int>(),              
                    Name = item["name"].ToString(),            
                    Url = item["url"].ToString(),                
                    Folder = item["folder"]?.ToString(),          
                    Project = project,                            
                    Organization = organisation,                  
                    CreatedDate = DateTime.UtcNow                 
                };

                pipelines.Add(pipeline);
            }

            return pipelines;
        }

        private async Task InsertPipelinesAsync(List<Pipeline> pipelines)
        {
            if (pipelines.Count > 0)
            {
                await _pipelinesCollection.InsertManyAsync(pipelines);
                Console.WriteLine($"{pipelines.Count} pipelines insérés dans MongoDB.");
            }
            else
            {
                Console.WriteLine("Aucun pipeline à insérer.");
            }
        }

        public async Task<string> GetPipelineByIdAsync(string organisation, string pat, string project, int pipelineId)
        {
            // Corrected API URL to fetch a specific pipeline by its ID
            var apiUrl = $"https://dev.azure.com/{organisation}/{project}/_apis/pipelines/{pipelineId}?api-version=7.1";

            // Configuration de l'authentification et des en-têtes
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                Console.WriteLine($"Sending GET request to: {apiUrl}");


                // Send GET request
                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log response
                Console.WriteLine($"HTTP Status: {response.StatusCode}, Response: {responseContent}");

                // Return response content if successful
                return response.IsSuccessStatusCode
                    ? responseContent
                    : $"Error retrieving pipeline: {responseContent}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

    }
}

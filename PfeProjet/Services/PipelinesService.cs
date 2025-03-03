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

                    // Insérer les pipelines dans MongoDB (avec remplacement des doublons)
                    await UpsertPipelinesAsync(pipelines);

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

        // Nouvelle méthode pour remplacer les pipelines existants ou en insérer de nouveaux
        private async Task UpsertPipelinesAsync(List<Pipeline> pipelines)
        {
            if (pipelines.Count > 0)
            {
                var bulkOps = new List<WriteModel<Pipeline>>();

                foreach (var pipeline in pipelines)
                {
                    var filter = Builders<Pipeline>.Filter.And(
                        Builders<Pipeline>.Filter.Eq(p => p.Id, pipeline.Id),
                        Builders<Pipeline>.Filter.Eq(p => p.Organization, pipeline.Organization),
                        Builders<Pipeline>.Filter.Eq(p => p.Project, pipeline.Project)
                    );

                    var upsert = new ReplaceOneModel<Pipeline>(filter, pipeline) { IsUpsert = true };
                    bulkOps.Add(upsert);
                }

                if (bulkOps.Count > 0)
                {
                    await _pipelinesCollection.BulkWriteAsync(bulkOps);
                    Console.WriteLine($"{pipelines.Count} pipelines traités dans MongoDB.");
                }
            }
            else
            {
                Console.WriteLine("Aucun pipeline à insérer.");
            }
        }

        // Méthode pour récupérer tous les pipelines de MongoDB
        public async Task<List<Pipeline>> GetAllPipelinesAsync(string organization = null, string project = null)
        {
            try
            {
                var filter = Builders<Pipeline>.Filter.Empty;

                if (!string.IsNullOrEmpty(organization))
                {
                    filter = Builders<Pipeline>.Filter.Eq(p => p.Organization, organization);
                }

                if (!string.IsNullOrEmpty(project))
                {
                    var projectFilter = Builders<Pipeline>.Filter.Eq(p => p.Project, project);
                    filter = filter == Builders<Pipeline>.Filter.Empty
                        ? projectFilter
                        : Builders<Pipeline>.Filter.And(filter, projectFilter);
                }

                return await _pipelinesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des pipelines depuis MongoDB: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetPipelineByIdAsync(string organisation, string pat, string project, int pipelineId)
        {
            // API URL to fetch a specific pipeline by its ID
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
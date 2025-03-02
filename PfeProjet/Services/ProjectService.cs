using PfeProjet.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace PfeProjet.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }




        public async Task<string> GetProjectAsync(string organisation, string pat)
        {

            var apiUrl = $"https://dev.azure.com/{organisation}/_apis/projects?api-version=7.1";

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

                // Retourne le contenu si succès
                return response.IsSuccessStatusCode
                    ? responseContent
                    : $"Erreur lors de la récupération des projets : {responseContent}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return $"Erreur : {ex.Message}";
            }
        }
    

      
    }
}
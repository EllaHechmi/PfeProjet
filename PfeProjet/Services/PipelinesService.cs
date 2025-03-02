﻿using System.Net.Http.Headers;
using System.Text;

namespace PfeProjet.Services
{

    public class PipelinesService
    {
        private readonly HttpClient _httpClient;

        public PipelinesService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
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

                // Retourne le contenu si succès
                return response.IsSuccessStatusCode
                    ? responseContent
                    : $"Erreur lors de la récupération des pipelines : {responseContent}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return $"Erreur : {ex.Message}";
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

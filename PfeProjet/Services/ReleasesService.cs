using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using PfeProjet.Models;
using System.Net.Http.Headers;
using System.Text;

namespace PfeProjet.Services
{
    public class ReleasesService
    {
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Release> _releasesCollection;

        public ReleasesService(HttpClient httpClient, MongoDbContext mongoDbContext)
        {
            _httpClient = httpClient;
            _releasesCollection = mongoDbContext.Releases;
        }

        public async Task<string> GetReleasesAsync(string organisation, string pat, string project)
        {
            var apiUrl = $"https://vsrm.dev.azure.com/{organisation}/{project}/_apis/release/releases?api-version=7.1";

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                Console.WriteLine($"Sending GET request to: {apiUrl}");

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"HTTP Status: {response.StatusCode}, Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var releases = ParseReleasesResponse(responseContent, organisation, project);

                    // Ensure releases are being parsed correctly
                    Console.WriteLine($"Parsed {releases.Count} releases.");

                    // Upsert releases into MongoDB
                    await UpsertReleasesAsync(releases);

                    return "Releases retrieved and stored successfully in MongoDB.";
                }
                else
                {
                    return $"Error retrieving releases: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
        private List<Release> ParseReleasesResponse(string responseContent, string organisation, string project)
        {
            var releases = new List<Release>();
            var jsonResponse = JObject.Parse(responseContent);

            foreach (var item in jsonResponse["value"])
            {
                var release = new Release
                {
                    ReleaseId = item["id"].ToObject<int>(),
                    Name = item["name"].ToString(),
                    Status = item["status"].ToString(),
                    CreatedOn = item["createdOn"].ToObject<DateTime>(),
                    ModifiedOn = item["modifiedOn"].ToObject<DateTime>(),
                    ModifiedBy = ParseUser(item["modifiedBy"]),
                    CreatedBy = ParseUser(item["createdBy"]),
                    CreatedFor = ParseUser(item["createdFor"]),
                    Variables = item["variables"].ToObject<Dictionary<string, object>>(),
                    VariableGroups = item["variableGroups"].ToObject<List<object>>(),
                    ReleaseDefinition = ParseReleaseDefinition(item["releaseDefinition"]),
                    ReleaseDefinitionRevision = item["releaseDefinitionRevision"].ToObject<int>(),
                    Description = item["description"].ToString(),
                    Reason = item["reason"].ToString(),
                    ReleaseNameFormat = item["releaseNameFormat"].ToString(),
                    KeepForever = item["keepForever"].ToObject<bool>(),
                    DefinitionSnapshotRevision = item["definitionSnapshotRevision"].ToObject<int>(),
                    LogsContainerUrl = item["logsContainerUrl"].ToString(),
                    Url = item["url"].ToString(),
                    Links = ParseLinks(item["_links"]),
                    Tags = item["tags"].ToObject<List<string>>(),
                    TriggeringArtifactAlias = item["triggeringArtifactAlias"]?.ToString(),
                    ProjectReference = ParseProjectReference(item["projectReference"]),
                    Properties = item["properties"].ToObject<Dictionary<string, object>>(),
                    Organization = organisation,
                    Project = project
                };

                releases.Add(release);
            }

            return releases;
        }

        private User ParseUser(JToken userToken)
        {
            if (userToken == null) return null;

            return new User
            {
                DisplayName = userToken["displayName"]?.ToString(), // Null-conditional operator
                Url = userToken["url"]?.ToString(), // Null-conditional operator
                Links = ParseAvatarLinks(userToken["_links"]), // Already handles null
                Id = userToken["id"]?.ToString(), // Null-conditional operator
                UniqueName = userToken["uniqueName"]?.ToString(), // Null-conditional operator
                ImageUrl = userToken["imageUrl"]?.ToString(), // Null-conditional operator
                Descriptor = userToken["descriptor"]?.ToString() // Null-conditional operator
            };
        }

        private Dictionary<string, AvatarLink> ParseAvatarLinks(JToken linksToken)
        {
            if (linksToken == null) return null;

            return new Dictionary<string, AvatarLink>
            {
                { "avatar", new AvatarLink { Href = linksToken["avatar"]["href"].ToString() } }
            };
        }

        private ReleaseDefinition ParseReleaseDefinition(JToken definitionToken)
        {
            if (definitionToken == null) return null;

            return new ReleaseDefinition
            {
                Id = definitionToken["id"].ToObject<int>(),
                Name = definitionToken["name"].ToString(),
                Path = definitionToken["path"].ToString(),
                ProjectReference = definitionToken["projectReference"]?.ToString(),
                Url = definitionToken["url"].ToString(),
                Links = ParseLinks(definitionToken["_links"])
            };
        }

        private Dictionary<string, Link> ParseLinks(JToken linksToken)
        {
            if (linksToken == null) return null;

            return new Dictionary<string, Link>
            {
                { "self", new Link { Href = linksToken["self"]["href"].ToString() } },
                { "web", new Link { Href = linksToken["web"]["href"].ToString() } }
            };
        }

        private ProjectReference ParseProjectReference(JToken projectToken)
        {
            if (projectToken == null) return null;

            return new ProjectReference
            {
                Id = projectToken["id"].ToString(),
                Name = projectToken["name"].ToString()
            };
        }

        // Upsert releases into MongoDB
        private async Task UpsertReleasesAsync(List<Release> releases)
        {
            if (releases.Count > 0)
            {
                var bulkOps = new List<WriteModel<Release>>();

                foreach (var release in releases)
                {
                    var filter = Builders<Release>.Filter.Eq(r => r.ReleaseId, release.ReleaseId);
                    var upsert = new ReplaceOneModel<Release>(filter, release) { IsUpsert = true };
                    bulkOps.Add(upsert);
                }

                if (bulkOps.Count > 0)
                {
                    await _releasesCollection.BulkWriteAsync(bulkOps);
                    Console.WriteLine($"{releases.Count} releases processed in MongoDB.");
                }
            }
            else
            {
                Console.WriteLine("No releases to insert.");
            }
        }

        // Get all releases from MongoDB
        public async Task<List<Release>> GetAllReleasesFromMongoAsync()
        {
            try
            {
                var releases = await _releasesCollection.Find(_ => true).ToListAsync();

               

                if (releases.Count == 0)
                {
                    Console.WriteLine("No valid releases found in MongoDB.");
                    return new List<Release>();
                }

                Console.WriteLine($"Fetched {releases.Count} valid releases from MongoDB.");
                return releases;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching releases from MongoDB: {ex.Message}");
                throw;
            }
        }

        // Get a release by ID from MongoDB
        public async Task<Release> GetReleaseByIdFromDbAsync(int releaseId)
        {
            try
            {
                var filter = Builders<Release>.Filter.Eq(r => r.ReleaseId, releaseId);
                return await _releasesCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching release from MongoDB: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetReleasesByIdAsync(string organisation, string pat, string project, int releasesId)
        {
            // Corrected API URL to fetch a specific  by its ID
            var apiUrl = $"https://vsrm.dev.azure.com/{organisation}/{project}/_apis/release/releases/{releasesId}?api-version=7.1";

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
                    : $"Error retrieving releases: {responseContent}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

    }
}
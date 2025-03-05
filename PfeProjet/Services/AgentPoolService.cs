using MongoDB.Driver;
using PfeProjet.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PfeProjet.Services
{
    public class AgentPoolsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<AgentPool> _agentPoolsCollection;

        public AgentPoolsService(HttpClient httpClient, MongoDbContext mongoDbContext)
        {
            _httpClient = httpClient;
            _agentPoolsCollection = mongoDbContext.AgentPools;
        }

        public async Task<string> GetAgentPoolAsync(string organisation, string pat)
        {
            var apiUrl = $"https://dev.azure.com/{organisation}/_apis/distributedtask/pools?api-version=7.1";

            // Configure authentication and headers
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

                if (response.IsSuccessStatusCode)
                {
                    var agentPools = ParseAgentPoolsResponse(responseContent);
                    await UpsertAgentPoolsAsync(agentPools);
                    return "Agent pools retrieved and stored successfully in MongoDB.";
                }
                else
                {
                    return $"Error retrieving agent pools: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

      
        private User ParseUser(JsonElement userElement)
        {
            return new User
            {
                DisplayName = userElement.GetProperty("displayName").GetString(),
                Url = userElement.GetProperty("url").GetString(),
                Links = ParseAvatarLinks(userElement.GetProperty("_links")),
                Id = userElement.GetProperty("id").GetString(),
                UniqueName = userElement.GetProperty("uniqueName").GetString(),
                ImageUrl = userElement.GetProperty("imageUrl").GetString(),
                Descriptor = userElement.GetProperty("descriptor").GetString()
            };
        }

        private Dictionary<string, AvatarLink> ParseAvatarLinks(JsonElement linksElement)
        {
            return new Dictionary<string, AvatarLink>
            {
                { "avatar", new AvatarLink { Href = linksElement.GetProperty("avatar").GetProperty("href").GetString() } }
            };
        }

        private async Task UpsertAgentPoolsAsync(List<AgentPool> agentPools)
        {
            if (agentPools.Count > 0)
            {
                var bulkOps = new List<WriteModel<AgentPool>>();

                foreach (var agentPool in agentPools)
                {
                    var filter = Builders<AgentPool>.Filter.Eq(a => a.Id, agentPool.Id);
                    var upsert = new ReplaceOneModel<AgentPool>(filter, agentPool) { IsUpsert = true };
                    bulkOps.Add(upsert);
                }

                if (bulkOps.Count > 0)
                {
                    await _agentPoolsCollection.BulkWriteAsync(bulkOps);
                    Console.WriteLine($"{agentPools.Count} agent pools processed in MongoDB.");
                }
            }
            else
            {
                Console.WriteLine("No agent pools to insert.");
            }
        }



        private List<AgentPool> ParseAgentPoolsResponse(string responseContent)
        {
            var agentPools = new List<AgentPool>();
            var jsonResponse = JsonDocument.Parse(responseContent).RootElement;

            foreach (var item in jsonResponse.GetProperty("value").EnumerateArray())
            {
                var agentPool = new AgentPool
                {
                    Id = item.GetProperty("id").GetInt32(),
                    CreatedOn = item.GetProperty("createdOn").GetDateTime(),

                    // Use safe parsing for boolean properties
                    AutoProvision = item.TryGetProperty("autoProvision", out var autoProvisionElement)
                        ? autoProvisionElement.GetBoolean()
                        : false,

                    AutoUpdate = item.TryGetProperty("autoUpdate", out var autoUpdateElement)
                        ? autoUpdateElement.GetBoolean()
                        : false,

                    AutoSize = item.TryGetProperty("autoSize", out var autoSizeElement)
                        ? autoSizeElement.GetBoolean()
                        : false,

                    // Safe parsing for nullable int properties
                    TargetSize = item.TryGetProperty("targetSize", out var targetSize)
                        ? (targetSize.ValueKind != JsonValueKind.Null ? targetSize.GetInt32() : (int?)null)
                        : null,

                    AgentCloudId = item.TryGetProperty("agentCloudId", out var agentCloudId)
                        ? (agentCloudId.ValueKind != JsonValueKind.Null ? agentCloudId.GetInt32() : (int?)null)
                        : null,

                    // Safe parsing for user objects
                    CreatedBy = item.TryGetProperty("createdBy", out var createdByElement)
                        ? ParseUser(createdByElement)
                        : null,

                    Owner = item.TryGetProperty("owner", out var ownerElement)
                        ? ParseUser(ownerElement)
                        : null,

                    // Safe parsing for string properties
                    Scope = item.TryGetProperty("scope", out var scopeElement)
                        ? scopeElement.GetString()
                        : string.Empty,

                    Name = item.TryGetProperty("name", out var nameElement)
                        ? nameElement.GetString()
                        : string.Empty,

                    // Safe parsing for boolean properties
                    IsHosted = item.TryGetProperty("isHosted", out var isHostedElement)
                        ? isHostedElement.GetBoolean()
                        : false,

                    PoolType = item.TryGetProperty("poolType", out var poolTypeElement)
                        ? poolTypeElement.GetString()
                        : string.Empty,

                    // Safe parsing for int properties
                    Size = item.TryGetProperty("size", out var sizeElement)
                        ? sizeElement.GetInt32()
                        : 0,

                    IsLegacy = item.TryGetProperty("isLegacy", out var isLegacyElement)
                        ? isLegacyElement.GetBoolean()
                        : false,

                    Options = item.TryGetProperty("options", out var optionsElement)
                        ? optionsElement.GetString()
                        : string.Empty
                };

                agentPools.Add(agentPool);
            }

            return agentPools;
        }

        private async Task UpsertAgentPoolAsync(AgentPool agentPool)
        {
            var filter = Builders<AgentPool>.Filter.Eq(a => a.Id, agentPool.Id);
            var options = new ReplaceOptions { IsUpsert = true };
            await _agentPoolsCollection.ReplaceOneAsync(filter, agentPool, options);

            Console.WriteLine($"Agent pool {agentPool.Id} processed in MongoDB.");
        }


        // get all agent pools from mongo db
        public async Task<List<AgentPool>> GetAllAgentPoolsFromMongoAsync()
        {
            try
            {
                // Fetch all agent pools from the MongoDB collection
                var agentPools = await _agentPoolsCollection.Find(_ => true).ToListAsync();

                if (agentPools == null || agentPools.Count == 0)
                {
                    Console.WriteLine("No agent pools found in MongoDB.");
                    return new List<AgentPool>();
                }

                Console.WriteLine($"Fetched {agentPools.Count} agent pools from MongoDB.");
                return agentPools;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching agent pools from MongoDB: {ex.Message}");
                throw;
            }
        }
    }
}
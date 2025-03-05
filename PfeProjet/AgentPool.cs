using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace PfeProjet.Models
{
    public class AgentPool
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public bool AutoProvision { get; set; }
        public bool AutoUpdate { get; set; }
        public bool AutoSize { get; set; }
        public int? TargetSize { get; set; } // Nullable int
        public int? AgentCloudId { get; set; } // Nullable int
        public User CreatedBy { get; set; }
        public User Owner { get; set; }
        public string Scope { get; set; }
        public string Name { get; set; }
        public bool IsHosted { get; set; }
        public string PoolType { get; set; }
        public int Size { get; set; }
        public bool IsLegacy { get; set; }
        public string Options { get; set; }
    }
}
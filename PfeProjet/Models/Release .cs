using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace PfeProjet.Models
{
    public class Release
    {
        [BsonId]
        [BsonElement("ReleaseId")]
       // [BsonElement("_id")]
        public int? ReleaseId { get; set; }

        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public User ModifiedBy { get; set; }
        public User CreatedBy { get; set; }
        public User CreatedFor { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public List<object> VariableGroups { get; set; }
        public ReleaseDefinition ReleaseDefinition { get; set; }
        public int ReleaseDefinitionRevision { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public string ReleaseNameFormat { get; set; }
        public bool KeepForever { get; set; }
        public int DefinitionSnapshotRevision { get; set; }
        public string LogsContainerUrl { get; set; }
        public string Url { get; set; }
        public Dictionary<string, Link> Links { get; set; }
        public List<string> Tags { get; set; }
        public string TriggeringArtifactAlias { get; set; }
        public ProjectReference ProjectReference { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public string Organization { get; set; }
        public string Project { get; set; }
    }

    public class User
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public Dictionary<string, AvatarLink> Links { get; set; }
        public string Id { get; set; }
        public string UniqueName { get; set; }
        public string ImageUrl { get; set; }
        public string Descriptor { get; set; }
    }

    public class AvatarLink
    {
        public string Href { get; set; }
    }

    public class ReleaseDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public object ProjectReference { get; set; }
        public string Url { get; set; }
        public Dictionary<string, Link> Links { get; set; }
    }

    public class Link
    {
        public string Href { get; set; }
    }

    public class ProjectReference
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
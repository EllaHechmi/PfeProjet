using MongoDB.Bson;

namespace PfeProjet.Models
{
    public class PipelineTask
    {
        public ObjectId Id { get; set; } // MongoDB's default identifier
        public int PipelineId { get; set; } // The ID of the pipeline the task belongs to
        public string Name { get; set; } // Task name
        public string Status { get; set; } // Task status (e.g., "Completed", "In Progress")
        public DateTime CreatedDate { get; set; } // Date the task was created
        public string Url { get; set; } // URL for the task (optional)
    }
}

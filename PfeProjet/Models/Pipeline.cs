namespace PfeProjet.Models
{
    public class Pipeline
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
        public string Project { get; set; }
        public string Organization { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

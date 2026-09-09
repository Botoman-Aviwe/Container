namespace PrescribingSystem.Models
{
    public class MovedContainer
    {
        public int Id { get; set; }

        public int Sequence { get; set; }

        public string Container { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        // When the container movement was created
        public DateTime CreatedDate { get; set; }

        // When the container was received
        public DateTime? ReceivedDate { get; set; }
    }
}

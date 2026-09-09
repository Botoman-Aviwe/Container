using System.ComponentModel.DataAnnotations;

namespace PrescribingSystem.Models
{
    public class COPIED
    {
        [Key]
        public int Id { get; set; }

        public int Sequence { get; set; }

        [Required]
        [StringLength(50)]
        public string Container { get; set; }

        [Required]
        [StringLength(20)]
        public string KindMove { get; set; } // Load / Discharge

        [StringLength(50)]
        public string FromLocation { get; set; }

        [StringLength(50)]
        public string ToLocation { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

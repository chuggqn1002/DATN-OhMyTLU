using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OhMyTLU.Data
{
    public class ActionAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserSessionId { get; set; }

        [ForeignKey("UserSessionId")]
        public UserSession UserSession { get; set; }

        [Required]
        public string Action { get; set; }

        public string Details { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}

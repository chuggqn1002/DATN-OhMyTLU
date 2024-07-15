using System.ComponentModel.DataAnnotations;

namespace OhMyTLU.Data
{
    public class Message
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string SenderId { get; set; }
        public User Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}

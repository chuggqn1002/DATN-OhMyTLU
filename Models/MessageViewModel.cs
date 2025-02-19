﻿using OhMyTLU.Data;

namespace OhMyTLU.Models
{
    public class MessageViewModel
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}

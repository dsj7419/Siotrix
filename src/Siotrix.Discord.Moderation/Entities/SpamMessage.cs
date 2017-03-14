using Discord.WebSocket;
using System;
using System.ComponentModel.DataAnnotations;

namespace Siotrix.Discord.Moderation
{
    public class SpamMessage
    {
        [Required]
        public ulong ChannelId { get; set; }
        [Required]
        public ulong MessageId { get; set; }
        [Required]
        public ulong UserId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public string Content { get; set; }

        public SpamMessage(SocketMessage s)
        {
            ChannelId = s.Channel.Id;
            MessageId = s.Id;
            UserId = s.Author.Id;
            CreatedAt = s.CreatedAt.UtcDateTime;
            Content = s.Content;
        }
    }
}

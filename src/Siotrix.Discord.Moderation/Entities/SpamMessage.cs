using System;
using System.ComponentModel.DataAnnotations;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    public class SpamMessage
    {
        public SpamMessage(SocketMessage s)
        {
            ChannelId = s.Channel.Id;
            MessageId = s.Id;
            UserId = s.Author.Id;
            CreatedAt = s.CreatedAt.UtcDateTime;
            Content = s.Content;
        }

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
    }
}
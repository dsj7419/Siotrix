using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class MessageParser
    {
        public static string ParseStringPrefix(IUserMessage msg, string spec)
        {
            var text = msg.Content;
            string value = text.Substring(spec.Length, text.Length - spec.Length);
            return value;
        }

        public static string ParseMentionPrefix(IUserMessage msg)
        {
            var text = msg.Content;
            if (text.Length <= 3 || text[0] != '<' || text[1] != '@') return null;

            int endPos = text.IndexOf('>');
            if (endPos == -1) return null;
            if (text.Length < endPos + 2 || text[endPos + 1] != ' ') return null;

            if (!MentionUtils.TryParseUser(text.Substring(0, endPos + 1), out ulong userId)) return null;
            string value = text.Substring(endPos + 2, text.Length - text.Substring(0, endPos + 1).Length - 1);
            return value;
        }
    }
}

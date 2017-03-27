using Discord;

namespace Siotrix.Discord
{
    public static class MessageExtensions
    {
        public static bool HasStringPrefix(this IUserMessage msg, string str, ref int argPos)
        {
            var text = msg.Content;
            if (text.StartsWith(str))
            {
                argPos = str.Length;
                return true;
            }
            return false;
        }
        public static bool HasMentionPrefix(this IUserMessage msg, IUser user, ref int argPos)
        {
            var text = msg.Content;
            if (text.Length <= 3 || text[0] != '<' || text[1] != '@') return false;

            int endPos = text.IndexOf('>');
            if (endPos == -1) return false;
            if (text.Length < endPos + 2 || text[endPos + 1] != ' ') return false; //Must end in "> "

            if (!MentionUtils.TryParseUser(text.Substring(0, endPos + 1), out ulong userId)) return false;
            if (userId == user.Id)
            {
                argPos = endPos + 2;
                return true;
            }
            return false;
        }
    }
}

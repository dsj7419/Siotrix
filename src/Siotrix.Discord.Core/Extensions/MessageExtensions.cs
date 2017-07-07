using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public static async Task<IUserMessage> SendMessageSafeAsync(this IMessageChannel channel, string text, Func<Exception, Task> handler, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                if (text.Length < 2000)
                    return await channel.SendMessageAsync(text, isTTS, embed, options);

                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sw.Write(text);
                        sw.Flush();
                    }
                    ms.Position = 0;
                    return await channel.SendFileAsync(ms, "Output.txt", $"I tried to send a message that was too long!", isTTS, options);
                }
            }
            catch (HttpException ex)
            {
                await (handler?.Invoke(ex) ?? Task.CompletedTask);
                return null;
            }
        }

        public static async Task<IUserMessage> SendMessageSafeAsync(this IMessageChannel channel, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            return await channel.SendMessageSafeAsync(text, null, isTTS, embed, options);
        }

        public static async Task ModifySafeAsync(this IUserMessage msg, Action<MessageProperties> func, Func<Exception, Task> handler, RequestOptions options = null)
        {
            try
            {
                await msg.ModifyAsync(func, options);
            }
            catch (Exception ex)
            {
                await (handler?.Invoke(ex) ?? Task.CompletedTask);
            }
        }

        public static async Task ModifySafeAsync(this IUserMessage msg, Action<MessageProperties> func, RequestOptions options = null)
        {
            await msg.ModifySafeAsync(func, null, options);
        }

        public static async Task SendToAll(this IEnumerable<IMessageChannel> channels, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            foreach (var channel in channels)
            {
                await SendMessageSafeAsync(channel, text, isTTS, embed, options);
            }
        }

        public static bool UserHasPermission(this IGuildChannel channel, IGuildUser user, ChannelPermission permission)
            => user.GetPermissions(channel).Has(permission);

        public static int number_of_messages = 0;
        public static ulong userId = 0;
        public static async Task NumberOfCleanupMessages(int count, ulong user_id)
        {
            number_of_messages = count;
            userId = user_id;
            await Task.Delay(0);
        }
    }
}

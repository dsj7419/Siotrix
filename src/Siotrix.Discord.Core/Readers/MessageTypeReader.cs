﻿using Discord.Commands;
using Discord;
using System.Globalization;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    public class MessageTypeReader<T> : TypeReader
        where T : class, IMessage
    {
        public override async Task<TypeReaderResult> Read(ICommandContext c, string input)
        {
            var context = c as CommandContext;
            ulong id;

            //By Id (1.0)
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                var msg = await context.Channel.GetMessageAsync(id, CacheMode.CacheOnly).ConfigureAwait(false) as T;
                if (msg != null)
                    return TypeReaderResult.FromSuccess(msg);
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class PrefixExtensions
    {
        public static async Task<DiscordGuildPrefix> GetGuildPrefixAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildPrefix();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gprefixs.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildPrefixAsync(context, SiotrixConstants.BotPrefix);
                        val = await db.Gprefixs.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildPrefixAsync(SocketCommandContext context, string prefix)
        {
            var val = new DiscordGuildPrefix(context.Guild.Id.ToLong(), prefix);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gprefixs.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildPrefix(DiscordGuildPrefix discordGuildPrefix, string prefix)
        {
            discordGuildPrefix.SetGuildPrefix(prefix);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gprefixs.Update(discordGuildPrefix);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}

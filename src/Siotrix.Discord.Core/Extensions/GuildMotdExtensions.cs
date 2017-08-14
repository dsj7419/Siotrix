using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildMotdExtensions
    {
        public static async Task<DiscordGuildMotd> GetGuildMotdAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildMotd();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gmotds.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildMotdAsync(context, "");
                        val = await db.Gmotds.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildMotdAsync(SocketCommandContext context, string motd)
        {
            var val = new DiscordGuildMotd(context.Guild.Id.ToLong(), motd);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gmotds.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildMotd(DiscordGuildMotd discordGuildMotd, string motd)
        {
            discordGuildMotd.SetGuildMotd(motd);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gmotds.Update(discordGuildMotd);
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

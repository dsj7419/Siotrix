using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedName
    {
        public static async Task<DiscordGuildName> GetGuildNameAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildName();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gnames.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildNameAsync(SocketCommandContext context, string name)
        {
            var val = new DiscordGuildName(context.Guild.Id.ToLong(), name);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gnames.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildName(DiscordGuildName discordGuildName, string name)
        {
            discordGuildName.SetGuildName(name);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gnames.Update(discordGuildName);
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
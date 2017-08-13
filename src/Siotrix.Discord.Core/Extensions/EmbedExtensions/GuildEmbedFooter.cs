using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedFooter
    {
        public static async Task<DiscordGuildFooter> GetGuildFooterAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildFooter();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gfooters.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordFooterAsync(context, SiotrixConstants.BotFooterText, SiotrixConstants.BotFooterIcon);
                        val = await db.Gfooters.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordFooterAsync(SocketCommandContext context, string footerText, string footerIcon)
        {
            var val = new DiscordGuildFooter(context.Guild.Id.ToLong(), footerText, footerIcon);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gfooters.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildFooterText(DiscordGuildFooter discordGuildFooter, string guildFooter)
        {
            discordGuildFooter.SetFooterText(guildFooter);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gfooters.Update(discordGuildFooter);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildFooterIcon(DiscordGuildFooter discordGuildFooter, string guildFooterIcon)
        {
            discordGuildFooter.SetFooterIcon(guildFooterIcon);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gfooters.Update(discordGuildFooter);
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
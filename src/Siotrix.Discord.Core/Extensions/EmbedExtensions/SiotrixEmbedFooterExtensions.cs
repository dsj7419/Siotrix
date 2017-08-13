using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class SiotrixEmbedFooterExtensions
    {
        public static async Task<DiscordSiotrixFooter> GetSiotrixFooterAsync()
        {
            var val = new DiscordSiotrixFooter();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Bfooters.FirstOrDefaultAsync();
                    if (val == null)
                    {
                        await CreateSiotrixFooterAsync(SiotrixConstants.BotFooterText, SiotrixConstants.BotFooterIcon);
                        val = await db.Bfooters.FirstOrDefaultAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateSiotrixFooterAsync(string footerText, string footerIcon)
        {
            var val = new DiscordSiotrixFooter(footerText, footerIcon);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Bfooters.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixFooterText(DiscordSiotrixFooter discordSiotrixFooter, string siotrixFooter)
        {
            discordSiotrixFooter.SetFooterText(siotrixFooter);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Bfooters.Update(discordSiotrixFooter);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixFooterIcon(DiscordSiotrixFooter discordSiotrixFooter, string siotrixFooterIcon)
        {
            discordSiotrixFooter.SetFooterIcon(siotrixFooterIcon);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Bfooters.Update(discordSiotrixFooter);
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

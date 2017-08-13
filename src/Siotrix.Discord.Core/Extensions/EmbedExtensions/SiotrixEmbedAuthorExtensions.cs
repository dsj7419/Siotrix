using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class SiotrixEmbedAuthorExtensions
    {
        public static async Task<DiscordAuthor> GetSiotrixAuthorAsync()
        {
            var val = new DiscordAuthor();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Authors.FirstOrDefaultAsync();
                    if (val == null)
                    {
                        await CreateSiotrixAuthorAsync(SiotrixConstants.BotAuthorIcon, SiotrixConstants.BotName, SiotrixConstants.DiscordInv);
                        val = await db.Authors.FirstOrDefaultAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateSiotrixAuthorAsync(string authorIcon, string authorName, string authorUrl)
        {
            var val = new DiscordAuthor(authorIcon, authorName, authorUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Authors.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixAuthorIcon(DiscordAuthor discordSiotrixAuthorIcon, string siotrixAuthorIcon)
        {
            discordSiotrixAuthorIcon.SetAuthorIcon(siotrixAuthorIcon);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Authors.Update(discordSiotrixAuthorIcon);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixAuthorName(DiscordAuthor discordSiotrixAuthorName, string siotrixAuthorName)
        {
            discordSiotrixAuthorName.SetAuthorName(siotrixAuthorName);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Authors.Update(discordSiotrixAuthorName);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixAuthorUrl(DiscordAuthor discordSiotrixAuthorUrl, string siotrixAuthorUrl)
        {
            discordSiotrixAuthorUrl.SetAuthorIcon(siotrixAuthorUrl);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Authors.Update(discordSiotrixAuthorUrl);
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

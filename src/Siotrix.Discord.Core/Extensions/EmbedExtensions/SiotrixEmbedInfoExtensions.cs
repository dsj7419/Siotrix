using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class SiotrixEmbedInfoExtensions
    {
        public static async Task<DiscordSiotrixInfo> GetSiotrixInfoAsync()
        {
            var val = new DiscordSiotrixInfo();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Binfos.FirstOrDefaultAsync();
                    if (val == null)
                    {
                        await CreateSiotrixInfo(SiotrixConstants.BotDesc);
                        val = await db.Binfos.FirstOrDefaultAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateSiotrixInfo(string botInfo)
        {
            var val = new DiscordSiotrixInfo(botInfo);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Binfos.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetSiotrixInfo(DiscordSiotrixInfo discordSiotrixInfo, string botInfo)
        {
            discordSiotrixInfo.SetSiotrixInfo(botInfo);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Binfos.Update(discordSiotrixInfo);
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

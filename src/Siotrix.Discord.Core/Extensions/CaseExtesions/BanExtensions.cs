using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class BanExtensions
    {
        public static async Task<DiscordGuildBanList> GetGuildBanListUserAsync(ulong caseId)
        {
            var val = new DiscordGuildBanList();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gbanlists.FirstOrDefaultAsync(
                        x => x.CaseId.Equals(caseId.ToLong()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateGuildBanListUserAsync(ulong caseId, ulong guildId, ulong userId, int banTime)
        {

            var bannedUser = new DiscordGuildBanList(caseId.ToLong(), guildId.ToLong(), userId.ToLong(), banTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gbanlists.AddAsync(bannedUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyBanListUserMinutesAsync(DiscordGuildBanList bannedUser, int banTime)
        {
            bannedUser.SetBanTime(banTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gbanlists.Update(bannedUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<IEnumerable<DiscordGuildBanList>> GetGuildBanListAsync(ulong guildId)
        {
            IEnumerable<DiscordGuildBanList> val = new List<DiscordGuildBanList>();

            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gbanlists.Where(x => x.GuildId == guildId.ToLong() && x.BanTime > 0).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

    }
}

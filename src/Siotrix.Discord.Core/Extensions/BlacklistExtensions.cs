using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    public static class BlacklistExtensions
    {
        public static async Task<IEnumerable<DiscordGuildBlacklist>> GetBlacklistUsersAsync(IGuild guild)
        {
            IEnumerable<DiscordGuildBlacklist> val = new List<DiscordGuildBlacklist>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Blacklist.Where(x => x.GuildId == guild.Id.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task SetBlacklistNameAsync(DiscordGuildBlacklist blacklist, SocketGuildUser user)
        {
            blacklist.SetBlacklistName(user.Username);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Blacklist.Update(blacklist);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<DiscordGuildBlacklist> GetBlacklistAsync(ulong guildId, ulong userId)
        {
            DiscordGuildBlacklist val = new DiscordGuildBlacklist();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Blacklist.FirstOrDefaultAsync(x => x.GuildId == guildId.ToLong() && x.UserId == userId.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateBlacklistUserAsync(SocketCommandContext context, SocketGuildUser user)
        {
            var blacklist = new DiscordGuildBlacklist(context.Guild.Id.ToLong(), user.Id.ToLong(), user.Username);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Blacklist.AddAsync(blacklist);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task DeleteBlacklistUserAsync(DiscordGuildBlacklist blacklistUser)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Blacklist.Remove(blacklistUser);
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

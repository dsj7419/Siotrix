using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class LogsToggleExtensions
    {
        public static async Task<IEnumerable<DiscordGuildLogsToggle>> GetLogsToggleAsync(IGuild guild)
        {
            IEnumerable<DiscordGuildLogsToggle> val = new List<DiscordGuildLogsToggle>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.LogsToggle.Where(x => x.GuildId == guild.Id.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<DiscordGuildLogsToggle> GetLogToggleAsync(ulong guildId, string logName)
        {
            var val = new DiscordGuildLogsToggle();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.LogsToggle.FirstOrDefaultAsync(
                        x => x.GuildId == guildId.ToLong() && x.LogName == logName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateLogToggleAsync(ulong guildId, string logName)
        {
            var newLog = new DiscordGuildLogsToggle(guildId.ToLong(), logName);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.LogsToggle.AddAsync(newLog);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task DeleteLogToggleAsync(DiscordGuildLogsToggle logToggle)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    db.LogsToggle.Remove(logToggle);
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

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
        #region normal logs

        public static async Task<DiscordGuildLogChannel> GetLogChannelAsync(ulong id)
        {
            var val = new DiscordGuildLogChannel();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Glogchannels.FirstOrDefaultAsync(x => x.GuildId == id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateLogChannelAsync(SocketCommandContext context, SocketChannel channel)
        {
            var logChannel = new DiscordGuildLogChannel(context.Guild.Id.ToLong(), channel.Id.ToLong(), false);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Glogchannels.AddAsync(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetLogChannel(DiscordGuildLogChannel logChannel, SocketChannel channel)
        {
            logChannel.SetLogChannel(channel.Id.ToLong());

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Glogchannels.Update(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }            
        }

        public static async Task SetLogChannelIsActive(DiscordGuildLogChannel logChannel, bool isActive)
        {
            logChannel.SetLogIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Glogchannels.Update(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion

        #region Moderation logs

        public static async Task<DiscordGuildModLogChannel> GetModLogChannelAsync(ulong id)
        {
            var val = new DiscordGuildModLogChannel();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gmodlogchannels.FirstOrDefaultAsync(x => x.GuildId == id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateModLogChannelAsync(SocketCommandContext context, SocketChannel channel)
        {
            var logChannel = new DiscordGuildModLogChannel(context.Guild.Id.ToLong(), channel.Id.ToLong(), false);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gmodlogchannels.AddAsync(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetModLogChannel(DiscordGuildModLogChannel logChannel, SocketChannel channel)
        {
            logChannel.SetModLogChannel(channel.Id.ToLong());

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gmodlogchannels.Update(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetModLogChannelIsActive(DiscordGuildModLogChannel logChannel, bool isActive)
        {
            logChannel.SetModLogIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gmodlogchannels.Update(logChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion

        #region Individual logs
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
#endregion
    }
}

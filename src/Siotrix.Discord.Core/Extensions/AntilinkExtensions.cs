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
    public static class AntilinkExtensions
    {
        /// <summary> Get antilink record by Id  </summary>
        public static async Task<DiscordGuildAntilink> GetAntilinkAsync(ulong id)
        {
            DiscordGuildAntilink val = new DiscordGuildAntilink();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Antilink.FirstOrDefaultAsync(x => x.GuildId == id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateAntilinkAsync(SocketCommandContext context)
        {
            var antilink = new DiscordGuildAntilink(context.Guild.Id.ToLong(), false, false, $"{context.Guild.Name.ToUpper()} does not allow that link in the channel.");

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Antilink.AddAsync(antilink);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetIsActiveAsync(DiscordGuildAntilink antilink, bool isActive)
        {
            antilink.SetIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Antilink.Update(antilink);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetIsDmAsync(DiscordGuildAntilink antilink, bool isOn)
        {
            antilink.SetIsDmMessage(isOn);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Antilink.Update(antilink);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetDmMessageAsync(DiscordGuildAntilink antilink, string message)
        {
            antilink.SetDmMessage(message);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Antilink.Update(antilink);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        // Get channel Guild stored DiscordGuildAntilinkChannelList, and return object
        public static async Task<DiscordGuildAntilinkChannelList> GetAntilinkChanneListAsync(ulong id, SocketChannel channel)
        {
            DiscordGuildAntilinkChannelList val = new DiscordGuildAntilinkChannelList();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.AntilinkChannels.FirstOrDefaultAsync(x => x.GuildId == id.ToLong() && x.ChannelId == channel.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateAntilinkChannelAsync(SocketCommandContext context, DiscordGuildAntilink antilink, SocketChannel channel)
        {
            var antilinkChannel = new DiscordGuildAntilinkChannelList(antilink.Id, context.Guild.Id.ToLong(), channel.Id.ToLong(), false, false);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.AntilinkChannels.AddAsync(antilinkChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetIsActiveChannelAsync(DiscordGuildAntilinkChannelList antilinkChannel, bool isActive)
        {
            antilinkChannel.SetIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.AntilinkChannels.Update(antilinkChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetIsStrictChannelAsync(DiscordGuildAntilinkChannelList antilinkChannel, bool isStrict)
        {
            antilinkChannel.SetIsStrict(isStrict);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.AntilinkChannels.Update(antilinkChannel);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        // Get channel Guild stored DiscordGuildAntilinkChannelList, and return object
        public static async Task<DiscordGuildAntilinkUserList> GetAntilinkUserListAsync(ulong guildId, ulong userId, ulong channelId)
        {
            DiscordGuildAntilinkUserList val = new DiscordGuildAntilinkUserList();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Antilinkusers.FirstOrDefaultAsync(x => x.GuildId == guildId.ToLong() && x.ChannelId.Equals(channelId.ToLong()) && x.UserId.Equals(userId.ToLong()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateUserAntilinkAsync(SocketCommandContext context, DiscordGuildAntilink antilink, SocketChannel channel, SocketGuildUser user, bool isOneTime)
        {
            var antilinkUser = new DiscordGuildAntilinkUserList(antilink.Id, context.Guild.Id.ToLong(), channel.Id.ToLong(), user.Id.ToLong(), isOneTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Antilinkusers.AddAsync(antilinkUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetIsOneTimeAsync(DiscordGuildAntilinkUserList antilinkUser, bool isOneTime)
        {
            antilinkUser.SetIsOneTime(isOneTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Antilinkusers.Update(antilinkUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task DeleteAntilinkUserAsync(DiscordGuildAntilinkUserList antilinkUser)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Antilinkusers.Remove(antilinkUser);
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

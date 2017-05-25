using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Siotrix.Discord
{
    public static class MuteExtensions
    {
        public static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        public static ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> unmuteTimers { get; }
        = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();
        public static event Action<IGuildUser, MuteType, SocketCommandContext, int, bool> UserMuted = delegate { };
        public static event Action<IGuildUser, MuteType, SocketCommandContext, bool> UserUnmuted = delegate { };

        public enum MuteType
        {
            Voice,
            Chat,
            All
        }



        public static async Task<IRole> GetMuteRole(IGuild guild)
        {
            var role_name = GetMuteRoleName(guild.Id.ToLong());

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == role_name);
            if (muteRole == null)
            {
                try
                {
                    muteRole = await guild.CreateRoleAsync(role_name, GuildPermissions.None).ConfigureAwait(false);
                }
                catch
                {
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == role_name) ?? await guild.CreateRoleAsync(role_name, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Select(x => x.Permissions).Contains(denyOverwrite))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, denyOverwrite).ConfigureAwait(false);
                        await Task.Delay(200).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return muteRole;
        }

        public static string GetMuteRoleName(long guild_id)
        {
            string name = null;
            if (guild_id <= 0) return null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmuteroles.FirstOrDefault(x => x.GuildId == guild_id);
                    if (result != null)
                    {
                        name = result.MuteRoleName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        public static bool SaveMuteUser(IGuildUser user, int minutes)
        {
            bool is_save = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmutelists.Where(x => x.GuildId == user.Guild.Id.ToLong() && x.UserId == user.Id.ToLong());
                    if (!result.Any())
                    {
                        var record = new DiscordGuildMuteList();
                        record.GuildId = user.Guild.Id.ToLong();
                        record.UserId = user.Id.ToLong();
                        record.MuteTime = minutes;
                        db.Gmutelists.Add(record);
                        is_save = true;
                    }
                    else
                    {
                        var data = result.First();
                        data.MuteTime = minutes;
                        db.Gmutelists.Update(data);
                        is_save = true;
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_save;
        }

        public static bool RemoveMuteUser(IGuildUser user)
        {
            bool is_remove = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmutelists.Where(x => x.GuildId == user.Guild.Id.ToLong() && x.UserId == user.Id.ToLong());
                    if (result.Any())
                    {
                        db.Gmutelists.RemoveRange(result);
                        is_remove = true;
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_remove;
        }

        public static async Task TimedMute(IGuildUser user, TimeSpan after, int minutes, SocketCommandContext context, bool is_auto)
        {
            await MuteUser(user, minutes, context, is_auto).ConfigureAwait(false); // mute the user. This will also remove any previous unmute timers

            StartUnmuteTimer(user.GuildId, user.Id, after, context); // start the timer
        }

        public static void StopUnmuteTimer(ulong guildId, ulong userId)
        {
            ConcurrentDictionary<ulong, Timer> userUnmuteTimers;
            if (!unmuteTimers.TryGetValue(guildId, out userUnmuteTimers)) return;

            Timer removed;
            if (userUnmuteTimers.TryRemove(userId, out removed))
            {
                removed.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public static async Task MuteUser(IGuildUser usr, int minutes, SocketCommandContext context, bool is_auto)
        {
            await usr.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var muteRole = await GetMuteRole(usr.Guild);
            if (!usr.RoleIds.Contains(muteRole.Id))
                await usr.AddRoleAsync(muteRole).ConfigureAwait(false);
            StopUnmuteTimer(usr.GuildId, usr.Id);
            UserMuted(usr, MuteType.All, context, minutes, is_auto);
        }

        public static void StartUnmuteTimer(ulong guildId, ulong userId, TimeSpan after, SocketCommandContext context)
        {
            //load the unmute timers for this guild
            var userUnmuteTimers = unmuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());

            //unmute timer to be added
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    await UnmuteUser(context.Guild.GetUser(userId), true, context).ConfigureAwait(false);
                    var case_id = CaseExtensions.GetCaseNumber(context);
                    CaseExtensions.SaveCaseDataAsync("unmute", case_id, userId.ToLong(), context.Guild.Id.ToLong(), "auto"); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            //add it, or stop the old one and add this one
            userUnmuteTimers.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        public static async Task UnmuteUser(this IGuildUser usr, bool is_auto, SocketCommandContext context)
        {
            
                var is_remove = RemoveMuteUser(usr);
                if (is_remove)
                {
                    StopUnmuteTimer(usr.GuildId, usr.Id);
                    try { await usr.ModifyAsync(x => x.Mute = false).ConfigureAwait(false); } catch { }
                    try { await usr.RemoveRoleAsync(await GetMuteRole(usr.Guild)).ConfigureAwait(false); } catch { /*ignore*/ }

                    UserUnmuted(usr, MuteType.All, context, is_auto);
                }
        }
    }
}

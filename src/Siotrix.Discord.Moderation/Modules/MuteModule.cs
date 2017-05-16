using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class MuteModule : ModuleBase<SocketCommandContext>
    {
        private readonly OverwritePermissions denyOverwrite = new OverwritePermissions(sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> unmuteTimers { get; }
        = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();
        private event Action<IGuildUser, MuteType> UserMuted = delegate { };
        private event Action<IGuildUser, MuteType> UserUnmuted = delegate { };

        public enum MuteType
        {
            Voice,
            Chat,
            All
        }

        [Command("mute")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task Mute(IGuildUser user)
        {
            try
            {
                await MuteUser(user).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync("Muted User Name : " + Format.Bold(user.ToString())).ConfigureAwait(false);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        private void StopUnmuteTimer(ulong guildId, ulong userId)
        {
            ConcurrentDictionary<ulong, Timer> userUnmuteTimers;
            if (!unmuteTimers.TryGetValue(guildId, out userUnmuteTimers)) return;

            Timer removed;
            if (userUnmuteTimers.TryRemove(userId, out removed))
            {
                removed.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private async Task MuteUser(IGuildUser usr)
        {
            await usr.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var muteRole = await GetMuteRole(usr.Guild);
            if (!usr.RoleIds.Contains(muteRole.Id))
                await usr.AddRoleAsync(muteRole).ConfigureAwait(false);
            StopUnmuteTimer(usr.GuildId, usr.Id);
            UserMuted(usr, MuteType.All);
        }

        private async Task<IRole> GetMuteRole(IGuild guild)
        {
            var role_name = GetMuteRoleName(guild.Id.ToLong());

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == role_name);
            if (muteRole == null)
            {
                try { muteRole = await guild.CreateRoleAsync(role_name, GuildPermissions.None).ConfigureAwait(false); }
                catch
                {
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == role_name) ??
                        await guild.CreateRoleAsync(role_name, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Select(x => x.Permissions).Contains(denyOverwrite))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, denyOverwrite)
                                .ConfigureAwait(false);

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

        private string GetMuteRoleName(long guild_id)
        {
            string name = null;
            if (guild_id <= 0) return null;
            using(var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmuteroles.FirstOrDefault(x => x.GuildId == guild_id);
                    if (result != null)
                    {
                        name = result.MuteRoleName;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        [Command("muterole")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task SetMuteRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gmuteroles.Where(x => x.GuildId == Context.Guild.Id.ToLong());
                    if (!list.Any())
                    {
                        var record = new DiscordGuildMuteRole();
                        record.GuildId = Context.Guild.Id.ToLong();
                        record.MuteRoleName = name;
                        db.Gmuteroles.Add(record);
                    }
                    else
                    {
                        var data = list.First();
                        data.MuteRoleName = name;
                        db.Gmuteroles.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }
    }
}

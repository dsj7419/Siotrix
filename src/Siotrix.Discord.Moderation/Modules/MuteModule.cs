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
        /*
        private readonly OverwritePermissions denyOverwrite = new OverwritePermissions(sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> unmuteTimers { get; }
        = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();
        public static event Action<IGuildUser, MuteType, SocketCommandContext, int> UserMuted = delegate { };
        public static event Action<IGuildUser, MuteType, SocketCommandContext, bool> UserUnmuted = delegate { };

        public enum MuteType
        {
            Voice,
            Chat,
            All
        }

   

        private async Task<IRole> GetMuteRole(IGuild guild)
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

        private bool SaveMuteUser(IGuildUser user, int minutes)
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

        private bool RemoveMuteUser(IGuildUser user)
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

        private async Task TimedMute(IGuildUser user, TimeSpan after, int minutes)
        {
            await MuteUser(user, minutes).ConfigureAwait(false); // mute the user. This will also remove any previous unmute timers
           
            StartUnmuteTimer(user.GuildId, user.Id, after); // start the timer
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

        private async Task MuteUser(IGuildUser usr, int minutes)
        {
            await usr.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var muteRole = await GetMuteRole(usr.Guild);
            if (!usr.RoleIds.Contains(muteRole.Id))
                await usr.AddRoleAsync(muteRole).ConfigureAwait(false);
            StopUnmuteTimer(usr.GuildId, usr.Id);
            UserMuted(usr, MuteType.All, Context, minutes);
        }

        private void StartUnmuteTimer(ulong guildId, ulong userId, TimeSpan after)
        {
            //load the unmute timers for this guild
            var userUnmuteTimers = unmuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());

            //unmute timer to be added
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    await UnmuteUser(Context.Guild.GetUser(userId), true).ConfigureAwait(false);
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

        private async Task UnmuteUser(IGuildUser usr, bool is_auto)
        {
            var is_remove = RemoveMuteUser(usr);
            if (is_remove)
            {
                StopUnmuteTimer(usr.GuildId, usr.Id);
                try { await usr.ModifyAsync(x => x.Mute = false).ConfigureAwait(false); } catch { }
                try { await usr.RemoveRoleAsync(await GetMuteRole(usr.Guild)).ConfigureAwait(false); } catch {  }

                UserUnmuted(usr, MuteType.All, Context, is_auto);
            }
        }
        

        [Command("mute")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task Mute(IGuildUser user, int minutes)
        {
            try
            {
                //await MuteUser(user).ConfigureAwait(false); // no parameter
                await TimedMute(user, TimeSpan.FromMinutes(minutes), minutes).ConfigureAwait(false);
                var is_save = SaveMuteUser(user, minutes);
                if (is_save)
                {
                    var case_id = CaseExtensions.GetCaseNumber(Context);
                    await Context.Channel.SendMessageAsync("What is reason? Case #" + case_id.ToString());
                }
            }
            catch
            {
                await Context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        [Command("unmute")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task UnMute(IGuildUser user)
        {
            try
            {
                await UnmuteUser(user, false).ConfigureAwait(false);
                var case_id = CaseExtensions.GetCaseNumber(Context);
                await Context.Channel.SendMessageAsync("What is reason? Case #" + case_id.ToString());
            }
            catch
            {
                await Context.Channel.SendMessageAsync("unmute_error").ConfigureAwait(false);
            }
        }

        [Command("mute")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task SetMuteRole()
        {
            string name = "siotrix_mute";
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
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("mute list")]
        [Summary("====")]
        [Remarks("==")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task MuteList()
        {
            string users = null;
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);

            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmutelists.Where(x => x.GuildId == Context.Guild.Id.ToLong());
                    if (result.Any())
                    {
                        foreach(var data in result)
                        {
                            users += "**User** : " + Context.Guild.GetUser(data.UserId.ToUlong()).Mention + "  " + " **TimeLength** : " + data.MuteTime.ToString() + " minutes" + "\n"; 
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (users == null)
                users = "No mute users";

            builder
            .AddField(x =>
            {
                x.Name = "Muted Users";
                x.Value = users;
            });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
        */

        //[Command("mute")]
        //[Summary("====")]
        //[Remarks("==")]
        //[RequireContext(ContextType.Guild)]
        //[MinPermissions(AccessLevel.GuildMod)]
        //private async Task Mute(IGuildUser user, int minutes)
        //{
        //    try
        //    {
        //        //await MuteUser(user).ConfigureAwait(false); // no parameter
        //        await MuteExtensions.TimedMute(user, TimeSpan.FromMinutes(minutes), minutes, Context, false).ConfigureAwait(false);
        //        var is_save = MuteExtensions.SaveMuteUser(user, minutes);
        //        if (is_save)
        //        {
        //            var case_id = CaseExtensions.GetCaseNumber(Context, "mute");
        //            await Context.Channel.SendMessageAsync("What is reason? Case #" + case_id.ToString());
        //        }
        //    }
        //    catch
        //    {
        //        await Context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
        //    }
        //}


        [Command("mute")]
        [Summary("Mutes a user from being able to type in any channels for a period of time.")]
        [Remarks(" @username (time) - can be set as 2d, 2 days, or 3d 2h 3m.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task Mute(IGuildUser user, [Remainder]TimeSpan time)
        {
            try
            {
                var minutes = time.TotalMinutes;
                //await MuteUser(user).ConfigureAwait(false); // no parameter
                await MuteExtensions.TimedMute(user, TimeSpan.FromMinutes(minutes), (int)minutes, Context, false).ConfigureAwait(false);
                var is_save = MuteExtensions.SaveMuteUser(user, (int)minutes);
                if (is_save)
                {
                    var case_id = CaseExtensions.GetCaseNumber(Context);
                    await Context.Channel.SendMessageAsync("What is the reason for the mute? Case #" + case_id.ToString());

                    CaseExtensions.SaveCaseDataAsync("mute", case_id, user.Id.ToLong(), Context.Guild.Id.ToLong(), ""); // add save in db
                   // Console.WriteLine("mute ========================={0}", case_id);
                }
            }
            catch
            {
                await Context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        [Command("unmute")]
        [Summary("Unmute a muted user.")]
        [Remarks(" @username")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task UnMute(IGuildUser user)
        {
            try
            {
                await MuteExtensions.UnmuteUser(user, false, Context).ConfigureAwait(false);
                var case_id = CaseExtensions.GetCaseNumber(Context);
                await Context.Channel.SendMessageAsync("What is the reason for the unmute? Case #" + case_id.ToString());

                    CaseExtensions.SaveCaseDataAsync("unmute", case_id, user.Id.ToLong(), Context.Guild.Id.ToLong(), ""); // add save in db
               // Console.WriteLine("unmute ========================={0}", case_id);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("unmute_error").ConfigureAwait(false);
            }
        }

        [Command("mute")]
        [Summary("Using with no args will create and/or verify a proper mute role is in the guild.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task SetMuteRole()
        {
            string name = "siotrix_mute";
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
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Command("mute list")]
        [Summary("List all members currently muted in this guild.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task MuteList()
        {
            string users = null;
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);

            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmutelists.Where(x => x.GuildId == Context.Guild.Id.ToLong());
                    if (result.Any())
                    {
                        foreach (var data in result)
                        {
                            users += "**User** : " + Context.Guild.GetUser(data.UserId.ToUlong()).Mention + "  " + " **TimeLength** : " + data.MuteTime.ToString() + " minutes" + "\n";
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (users == null)
                users = "No muted users";

            builder
            .AddField(x =>
            {
                x.Name = "Muted Users";
                x.Value = users;
            });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}

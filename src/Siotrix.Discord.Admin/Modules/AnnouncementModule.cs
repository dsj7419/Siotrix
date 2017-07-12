using Discord;
using Discord.Commands;
using Discord.Addons.EmojiTools;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Discord.WebSocket;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [Group("announcement")]
    [RequireContext(ContextType.Guild)]
    public class AnnouncementModule : ModuleBase<SocketCommandContext>
    {
        private bool GetToggleOrDMStatus(int cmd_id, int option)
        {
            bool status = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.CommandId == cmd_id && p.GuildId == Context.Guild.Id.ToLong() && p.Option == option);
                    if (list.Any())
                    {
                        db.Gannouncetoggles.RemoveRange(list);
                        status = true;
                    }
                    else
                    {
                        var val = new DiscordGuildAnnounceToggleOrDM();
                        val.CommandId = cmd_id;
                        val.GuildId = Context.Guild.Id.ToLong();
                        val.Option = option;
                        db.Gannouncetoggles.Add(val);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return status;
        }

        private bool CheckUsableCommand(int cmd_id)
        {
            bool is_usable = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.CommandId == cmd_id && p.GuildId == Context.Guild.Id.ToLong());
                    if (!list.Any())
                        is_usable = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_usable;
        }

        private bool SaveAndUpdateAnnounceMessage(long guild_id, int msgId, string text)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncemessages.Where(p => p.MessageId == msgId && p.GuildId == Context.Guild.Id.ToLong());
                    if (list.Any())
                    {
                        var data = list.First();
                        data.Message = text;
                        db.Gannouncemessages.Update(data);
                    }
                    else
                    {
                        var val = new DiscordGuildAnnounceMessage();
                        val.MessageId = msgId;
                        val.Message = text;
                        val.GuildId = Context.Guild.Id.ToLong();
                        db.Gannouncemessages.Add(val);
                    }
                    db.SaveChanges();
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        private string[] GetAnnouncementInfo(long guild_id)
        {
            string[] data = new string[]{ "Welcome ***On***", "Leave ***On***", "Return ***On***" };
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.GuildId == guild_id && p.Option == 0);
                    if (list.Any())
                    {
                        foreach(var item in list)
                        {
                            if (item.CommandId == 1)
                                data[0] = "Welcome ***Off***";
                            else if (item.CommandId == 2)
                                data[1] = "Leave ***Off***";
                            else if (item.CommandId == 3)
                                data[2] = "Return ***Off***";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return data;
        }

        private bool SetAnnounceChannelPerGuild(long guild_id, long channel_id)
        {
            bool isSuccess = false;
            using(var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guild_id);
                    if (list.Any())
                    {
                        var data = list.First();
                        data.ChannelId = channel_id;
                        db.Gannouncechannels.Update(data);
                    }
                    else
                    {
                        var record = new DiscordGuildAnnounceChannel();
                        record.ChannelId = channel_id;
                        record.GuildId = guild_id;
                        db.Gannouncechannels.Add(record);
                    }
                    db.SaveChanges();
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        }

        private string GetAnnounceChannel(long guild_id)
        {
            string channel_name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guild_id);
                    if (list.Any())
                    {
                        channel_name = " : ***#" + Context.Guild.GetChannel(list.First().ChannelId.ToUlong()) + "***";
                    }
                        
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return channel_name;
        }

        private string[] GetDMStatusValues(long guild_id)
        {
            string[] data = new string[3];
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(x => x.GuildId == guild_id && x.Option == 1);
                    if (list.Any())
                    {
                        foreach (var item in list)
                        {
                            if (item.CommandId == 1)
                                data[0] = " : ***Direct Message***";
                            else if (item.CommandId == 2)
                                data[1] = " : ***Direct Message***";
                            else if (item.CommandId == 3)
                                data[2] = " : ***Direct Message***";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return data;
        }

        [Command]
        [Summary("- welcome")]
        [Remarks(" - need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task AnnouncementAsync()
        {
            var info = GetAnnouncementInfo(Context.Guild.Id.ToLong());
            var channel_name = GetAnnounceChannel(Context.Guild.Id.ToLong());
            var dm_list = GetDMStatusValues(Context.Guild.Id.ToLong());
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
            builder
           .AddField(x =>
           {
               x.Name = "Announcements Informations";
               x.Value =
                        info[0] + "\n" + "Welcome Channel" + ((dm_list[0] != null) ? dm_list[0] : channel_name) + "\n" +
                        info[1] + "\n" + "Leave Channel" + ((dm_list[1] != null) ? dm_list[1] : channel_name) + "\n" +
                        info[2] + "\n" + "Return Channel" + ((dm_list[2] != null) ? dm_list[2] : channel_name);
           });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("welcome")]
        [Summary("- welcome")]
        [Remarks(" - need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string param, [Remainder]string text = null)
        {
            
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleOrDMStatus(1, 0);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var is_usable_cmd = CheckUsableCommand(1);
                if (is_usable_cmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 1, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
            else if (param.Equals("dm"))
            {
                var dm_on = GetToggleOrDMStatus(1, 1);
                if (dm_on)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("leave")]
        [Summary("- leave")]
        [Remarks(" - need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LeaveAsync(string param, [Remainder]string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleOrDMStatus(2, 0);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***leave*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***leave*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var is_usable_cmd = CheckUsableCommand(2);
                if (is_usable_cmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 2, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
            else if (param.Equals("dm"))
            {
                var dm_on = GetToggleOrDMStatus(2, 1);
                if (dm_on)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("return")]
        [Summary("- return")]
        [Remarks(" - need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ReturnAsync(string param, [Remainder]string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleOrDMStatus(3, 0);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***return*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***return*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var is_usable_cmd = CheckUsableCommand(3);
                if (is_usable_cmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 3, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
            else if (param.Equals("dm"))
            {
                var dm_on = GetToggleOrDMStatus(3, 1);
                if (dm_on)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("mount")]
        [Summary("- mount")]
        [Remarks(" - need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task MountAsync(string channel_name)
        {
            long channel_id = 0;
            bool is_setting = false;
            channel_id = ChannelNameExtensions.GetChannelIdFromName(channel_name, Context);
            if (channel_id <= 0)
                await ReplyAsync("📣 : Not exists like that channel!");
            is_setting = SetAnnounceChannelPerGuild(Context.Guild.Id.ToLong(), channel_id);
            if (is_setting)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
            else
                await ReplyAsync("📣 : Not founded announcement channel!");
        }
    }
}
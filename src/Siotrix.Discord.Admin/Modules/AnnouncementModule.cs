using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [Group("announcement")]
    [Summary("Set Welcome, Return, and Leave messages for guild members.\nVariable keywords are: ")]
    [RequireContext(ContextType.Guild)]
    public class AnnouncementModule : ModuleBase<SocketCommandContext>
    {
        private bool GetToggleOrDmStatus(int cmdId, int option)
        {
            var status = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(
                        p => p.CommandId == cmdId && p.GuildId == Context.Guild.Id.ToLong() && p.Option == option);
                    if (list.Any())
                    {
                        db.Gannouncetoggles.RemoveRange(list);
                        status = true;
                    }
                    else
                    {
                        var val = new DiscordGuildAnnounceToggleOrDm();
                        val.CommandId = cmdId;
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

        private bool CheckUsableCommand(int cmdId)
        {
            var isUsable = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(
                        p => p.CommandId == cmdId && p.GuildId == Context.Guild.Id.ToLong());
                    if (!list.Any())
                        isUsable = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isUsable;
        }

        private bool SaveAndUpdateAnnounceMessage(long guildId, int msgId, string text)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncemessages.Where(
                        p => p.MessageId == msgId && p.GuildId == Context.Guild.Id.ToLong());
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
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        }

        private string[] GetAnnouncementInfo(long guildId)
        {
            string[] data = {"Welcome ***On***", "Leave ***On***", "Return ***On***"};
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.GuildId == guildId && p.Option == 0);
                    if (list.Any())
                        foreach (var item in list)
                            if (item.CommandId == 1)
                                data[0] = "Welcome ***Off***";
                            else if (item.CommandId == 2)
                                data[1] = "Leave ***Off***";
                            else if (item.CommandId == 3)
                                data[2] = "Return ***Off***";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return data;
        }

        private bool SetAnnounceChannelPerGuild(long guildId, long channelId)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guildId);
                    if (list.Any())
                    {
                        var data = list.First();
                        data.ChannelId = channelId;
                        db.Gannouncechannels.Update(data);
                    }
                    else
                    {
                        var record = new DiscordGuildAnnounceChannel();
                        record.ChannelId = channelId;
                        record.GuildId = guildId;
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

        private string GetAnnounceChannel(long guildId)
        {
            string channelName = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncechannels.Where(p => p.GuildId == guildId);
                    if (list.Any())
                        channelName = " : ***#" + Context.Guild.GetChannel(list.First().ChannelId.ToUlong()) + "***";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return channelName;
        }

        private string[] GetDmStatusValues(long guildId)
        {
            var data = new string[3];
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(x => x.GuildId == guildId && x.Option == 1);
                    if (list.Any())
                        foreach (var item in list)
                            if (item.CommandId == 1)
                                data[0] = " : ***Direct Message***";
                            else if (item.CommandId == 2)
                                data[1] = " : ***Direct Message***";
                            else if (item.CommandId == 3)
                                data[2] = " : ***Direct Message***";
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
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task AnnouncementAsync()
        {
            var info = GetAnnouncementInfo(Context.Guild.Id.ToLong());
            var channelName = GetAnnounceChannel(Context.Guild.Id.ToLong());
            var dmList = GetDmStatusValues(Context.Guild.Id.ToLong());
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = Context.GetGuildPrefix();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(x =>
                {
                    x.Name = "Announcements Informations";
                    x.Value =
                        info[0] + "\n" + "Welcome Channel" + (dmList[0] != null ? dmList[0] : channelName) + "\n" +
                        info[1] + "\n" + "Leave Channel" + (dmList[1] != null ? dmList[1] : channelName) + "\n" +
                        info[2] + "\n" + "Return Channel" + (dmList[2] != null ? dmList[2] : channelName);
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("welcome")]
        [Summary("Welcome message annoucement for new members")]
        [Remarks(
            "[set|toggle|dm] - Set to set message, toggle to turn on and off, and dm to set to direct message user instead of a channel.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string param, [Remainder] string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggleOn = GetToggleOrDmStatus(1, 0);
                if (toggleOn)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var isUsableCmd = CheckUsableCommand(1);
                if (isUsableCmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 1, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BotSuccess);
                }
                else
                {
                    await ReplyAsync(
                        "📣 : You can not use this command because it has been toggled off in this guild.");
                }
            }
            else if (param.Equals("dm"))
            {
                var dmOn = GetToggleOrDmStatus(1, 1);
                if (dmOn)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("leave")]
        [Summary("Leave message annoucement for when users leave the guild.")]
        [Remarks(
            "[set|toggle|dm] - Set to set message, toggle to turn on and off, and dm to set to direct message user instead of a channel.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LeaveAsync(string param, [Remainder] string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggleOn = GetToggleOrDmStatus(2, 0);
                if (toggleOn)
                    await ReplyAsync($"✅ : Toggled ***leave*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***leave*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var isUsableCmd = CheckUsableCommand(2);
                if (isUsableCmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 2, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BotSuccess);
                }
                else
                {
                    await ReplyAsync(
                        "📣 : You can not use this command because it has been toggled off in this guild.");
                }
            }
            else if (param.Equals("dm"))
            {
                var dmOn = GetToggleOrDmStatus(2, 1);
                if (dmOn)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("return")]
        [Summary("Welcome message annoucement for returning members")]
        [Remarks(
            "[set|toggle|dm] - Set to set message, toggle to turn on and off, and dm to set to direct message user instead of a channel.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ReturnAsync(string param, [Remainder] string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggleOn = GetToggleOrDmStatus(3, 0);
                if (toggleOn)
                    await ReplyAsync($"✅ : Toggled ***return*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***return*** command **off** in this guild !");
            }
            else if (param.Equals("set"))
            {
                var isUsableCmd = CheckUsableCommand(3);
                if (isUsableCmd)
                {
                    if (text == null)
                    {
                        await ReplyAsync("📣 : Please input your message!");
                        return;
                    }
                    var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 3, text);
                    if (success)
                        await ReplyAsync(SiotrixConstants.BotSuccess);
                }
                else
                {
                    await ReplyAsync(
                        "📣 : You can not use this command because it has been toggled off in this guild.");
                }
            }
            else if (param.Equals("dm"))
            {
                var dmOn = GetToggleOrDmStatus(3, 1);
                if (dmOn)
                    await ReplyAsync($"✖️ : DM unset!");
                else
                    await ReplyAsync($"✅ : DM set!");
            }
        }

        [Command("announcechannel")]
        [Summary("Set the announcement channel for welcome/leave/return messages.")]
        [Remarks("(Channel Name) - name of channel you'd like to have all announcements sent.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task MountAsync(string channelName)
        {
            long channelId = 0;
            var isSetting = false;
            channelId = channelName.GetChannelIdFromName(Context);
            if (channelId <= 0)
                await ReplyAsync("📣 : Cannot find valid announcement channel!!");
            isSetting = SetAnnounceChannelPerGuild(Context.Guild.Id.ToLong(), channelId);
            if (isSetting)
                await ReplyAsync(SiotrixConstants.BotSuccess);
            else
                await ReplyAsync("📣 : Cannot find announcement channel!");
        }
    }
}
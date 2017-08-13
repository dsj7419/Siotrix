using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildAdmin)]
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _map;
        private readonly CommandService _service;

        public LogModule(CommandService service, IServiceProvider map)
        {
            _service = service;
            _map = map;
        }

        private string[] GetLogChannelPerGuild(long guildId, string cmd)
        {
            var result = new string[9];
            var isModLog = -1;
            if (cmd.Equals("modlogchannel"))
                isModLog = 1;
            if (cmd.Equals("logchannel"))
                isModLog = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (isModLog == 0)
                    {
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() > 0 || list.Any())
                        {
                            result[0] = "LogChannel Information :";
                            result[1] = Context.Client.GetUser(list.First().UserId.ToUlong()).Username;
                            result[2] = list.First().UserId.ToString();
                            result[3] = Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(list.First().ChannelId))
                                .First().Name;
                            result[4] = list.First().ChannelId.ToString();
                            result[5] = list.First().GuildId.ToString();
                            result[6] = list.First().IsActive ? "ON" : "OFF";
                            result[7] = null;
                            result[8] = null;
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() > 0 || list.Any())
                        {
                            result[0] = "ModLogChannel Information :";
                            result[1] = Context.Client.GetUser(list.First().UserId.ToUlong()).Username;
                            result[2] = list.First().UserId.ToString();
                            result[3] = Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(list.First().ChannelId))
                                .First().Name;
                            result[4] = list.First().ChannelId.ToString();
                            result[5] = list.First().GuildId.ToString();
                            result[6] = list.First().IsActive ? "ON" : "OFF";
                            result[7] = Context.Client.GetUser(list.First().ModeratorId.ToUlong()).Username;
                            result[8] = list.First().ModeratorId.ToString();
                        }
                        else
                        {
                            result = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return result;
        }

        private bool SetLogChannelPerGuild(long guildId, string command, long channelId)
        {
            var isSuccess = false;
            var result = new string[9];
            var isModLog = -1;
            if (command.Equals("modlogchannel"))
                isModLog = 1;
            if (command.Equals("logchannel"))
                isModLog = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (isModLog == 0)
                    {
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() > 0 || list.Any())
                        {
                            var data = list.First();
                            data.ChannelId = channelId;
                            data.GuildId = guildId;
                            data.IsActive = false;
                            data.UserId = Context.User.Id.ToLong();
                            db.Glogchannels.Update(data);
                        }
                        else
                        {
                            var record = new DiscordGuildLogChannel();
                            record.ChannelId = channelId;
                            record.GuildId = guildId;
                            record.IsActive = false;
                            record.UserId = Context.User.Id.ToLong();
                            db.Glogchannels.Add(record);
                        }
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() > 0 || list.Any())
                        {
                            var data = list.First();
                            data.ChannelId = channelId;
                            data.GuildId = guildId;
                            data.IsActive = false;
                            data.UserId = Context.User.Id.ToLong();
                            data.ModeratorId = Context.User.Id.ToLong();
                            db.Gmodlogchannels.Update(data);
                        }
                        else
                        {
                            var record = new DiscordGuildModLogChannel();
                            record.ChannelId = channelId;
                            record.GuildId = guildId;
                            record.IsActive = false;
                            record.UserId = Context.User.Id.ToLong();
                            record.ModeratorId = Context.User.Id.ToLong();
                            db.Gmodlogchannels.Add(record);
                        }
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

        private int CheckToggleStatus(long guildId, string command)
        {
            var isActive = 0;
            var isModLog = -1;
            if (command.Equals("modlogchannel"))
                isModLog = 1;
            if (command.Equals("logchannel"))
                isModLog = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (isModLog == 0)
                    {
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() <= 0 || !list.Any()) return 0;

                        var data = list.First();
                        if (data.IsActive)
                        {
                            data.IsActive = false;
                            isActive = 1;
                        }
                        else
                        {
                            data.IsActive = true;
                            isActive = 2;
                        }
                        db.Glogchannels.Update(data);
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guildId));
                        if (list.Count() <= 0 || !list.Any()) return 0;

                        var data = list.First();
                        if (data.IsActive)
                        {
                            data.IsActive = false;
                            isActive = 3;
                        }
                        else
                        {
                            data.IsActive = true;
                            isActive = 4;
                        }
                        db.Gmodlogchannels.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isActive;
        }

        private string PrintToggleStatus(int activeValue)
        {
            string status = null;
            switch (activeValue)
            {
                case 1:
                    status = "✖️ : ``logchannel`` has been **Toggled off** !";
                    break;
                case 2:
                    status = "✅ : ``logchannel`` has been **Toggled on** !";
                    break;
                case 3:
                    status = "✖️ : ``modlogchannel`` has been **Toggled off** !";
                    break;
                case 4:
                    status = "✅ : ``modlogchannel`` has been **Toggled on** !";
                    break;
                default:
                    break;
            }
            return status;
        }

        [Command("logs")]
        [Summary("Check status of logchannel and modlogchannel")]
        [Remarks("(logchannel or modlogchannel) - use ")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LogAsync(string command)
        {
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gColor = await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = Context.GetGuildPrefix();
            var data = GetLogChannelPerGuild(Context.Guild.Id.ToLong(), command);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(gColor)
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTitle(data[0])
                .WithTimestamp(DateTime.UtcNow);

            builder
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("User : "),
                    Value = data[1] + "(" + data[2] + ")"
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Channel : "),
                    Value = "#" + data[3] + "(" + data[4] + ")"
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Guild Id : "),
                    Value = data[5]
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Active Status "),
                    Value = data[6]
                });
            if (data[7] != null)
                builder
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Moderator : "),
                        Value = data[7] + "(" + data[8] + ")"
                    });

            await ReplyAsync("", embed: builder);
        }

        [Command("logs")]
        [Summary("Turn on and off the log and modlog channels.")]
        [Remarks("(logchannel/modlogchannel) toggle - must use the word toggle at end to turn on or off.")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LogAsync(string command, [Remainder] string name)
        {
            long channelId = 0;
            var isSetting = false;

            if (name.Equals("toggle"))
            {
                var activeValue = CheckToggleStatus(Context.Guild.Id.ToLong(), command);
                var status = PrintToggleStatus(activeValue);
                await ReplyAsync(status);
            }
            else
            {
                channelId = name.GetChannelIdFromName(Context);
                if (channelId <= 0) return;
                isSetting = SetLogChannelPerGuild(Context.Guild.Id.ToLong(), command, channelId);
                if (isSetting)
                    await ReplyAsync(SiotrixConstants.BotSuccess);
            }
        }
    }
}
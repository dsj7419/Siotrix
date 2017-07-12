using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]    
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildAdmin)]
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _map;
        
        public LogModule(CommandService service, IServiceProvider map)
        {
            _service = service;
            _map = map;
        }

        private string[] GetLogChannelPerGuild(long guild_id, string cmd)
        {
            string[] result = new string[9];
            int isModLog = -1;
            if (cmd.Equals("modlogchannel"))
                isModLog = 1;
            if (cmd.Equals("logchannel"))
                isModLog = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    if(isModLog == 0)
                    {
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() > 0 || list.Any())
                        {
                            result[0] = "LogChannel Information :";
                            result[1] = Context.Client.GetUser(list.First().UserId.ToUlong()).Username;
                            result[2] = list.First().UserId.ToString();
                            result[3] = Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(list.First().ChannelId)).First().Name;
                            result[4] = list.First().ChannelId.ToString();
                            result[5] = list.First().GuildId.ToString();
                            result[6] = list.First().IsActive ? "ON" : "OFF";
                            result[7] = null;
                            result[8] = null;
                        }
                        else
                            result = null;
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() > 0 || list.Any())
                        {
                            result[0] = "ModLogChannel Information :";
                            result[1] = Context.Client.GetUser(list.First().UserId.ToUlong()).Username;
                            result[2] = list.First().UserId.ToString();
                            result[3] = Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(list.First().ChannelId)).First().Name;
                            result[4] = list.First().ChannelId.ToString();
                            result[5] = list.First().GuildId.ToString();
                            result[6] = list.First().IsActive ? "ON" : "OFF";
                            result[7] = Context.Client.GetUser(list.First().ModeratorId.ToUlong()).Username;
                            result[8] = list.First().ModeratorId.ToString();
                        }
                        else
                            result = null;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return result;
        }

        private bool SetLogChannelPerGuild(long guild_id, string command, long channel_id)
        {
            bool isSuccess = false;
            string[] result = new string[9];
            int isModLog = -1;
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
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() > 0 || list.Any())
                        {

                            var data = list.First();
                            data.ChannelId = channel_id;
                            data.GuildId = guild_id;
                            data.IsActive = false;
                            data.UserId = Context.User.Id.ToLong();
                            db.Glogchannels.Update(data);
                        }
                        else
                        {
                            var record = new DiscordGuildLogChannel();
                            record.ChannelId = channel_id;
                            record.GuildId = guild_id;
                            record.IsActive = false;
                            record.UserId = Context.User.Id.ToLong();
                            db.Glogchannels.Add(record);
                        }
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() > 0 || list.Any())
                        {

                            var data = list.First();
                            data.ChannelId = channel_id;
                            data.GuildId = guild_id;
                            data.IsActive = false;
                            data.UserId = Context.User.Id.ToLong();
                            data.ModeratorId = Context.User.Id.ToLong();
                            db.Gmodlogchannels.Update(data);
                        }
                        else
                        {
                            var record = new DiscordGuildModLogChannel();
                            record.ChannelId = channel_id;
                            record.GuildId = guild_id;
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

        private int CheckToggleStatus(long guild_id, string command)
        {
            int is_active = 0;
            int isModLog = -1;
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
                        var list = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() <= 0 || !list.Any()) return 0;

                        var data = list.First();
                        if (data.IsActive)
                        {
                            data.IsActive = false;
                            is_active = 1;
                        }
                        else
                        {
                            data.IsActive = true;
                            is_active = 2;
                        }
                        db.Glogchannels.Update(data);
                    }
                    if (isModLog == 1)
                    {
                        var list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guild_id));
                        if (list.Count() <= 0 || !list.Any()) return 0;

                        var data = list.First();
                        if (data.IsActive)
                        {
                            data.IsActive = false;
                            is_active = 3;
                        }
                        else
                        {
                            data.IsActive = true;
                            is_active = 4;
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
            return is_active;
        }

        private string PrintToggleStatus(int active_value)
        {
            string status = null;
            switch (active_value)
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
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            var data = GetLogChannelPerGuild(Context.Guild.Id.ToLong(), command);
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithColor(g_color)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTitle(data[0])
                .WithTimestamp(DateTime.UtcNow);

            builder
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("User : "), Value = data[1] + "(" + data[2] + ")" })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Channel : "), Value = "#" + data[3] + "(" + data[4] + ")" })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Guild Id : "), Value = data[5] })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Active Status "), Value = data[6] });
            if(data[7] != null)
            {
                builder
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Moderator : "), Value = data[7] + "(" + data[8] + ")" });
            }
                
            await ReplyAsync("", embed: builder);
        }

        [Command("logs")]
        [Summary("Turn on and off the log and modlog channels.")]
        [Remarks("(logchannel/modlogchannel toggle - must use the word toggle at end to turn on or off.")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LogAsync(string command, [Remainder] string name)
        {
            long channel_id = 0;
            bool is_setting = false;
            
            if (name.Equals("toggle"))
            {
                int active_value = CheckToggleStatus(Context.Guild.Id.ToLong(), command);
                string status = PrintToggleStatus(active_value);
                await ReplyAsync(status);
            }
            else
            {
                channel_id = ChannelNameExtensions.GetChannelIdFromName(name, Context);
                if (channel_id <= 0) return;
                is_setting = SetLogChannelPerGuild(Context.Guild.Id.ToLong(), command, channel_id);
                if(is_setting)
                    await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
            }
        }
    }
}
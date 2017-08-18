using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [Group("logs")]
    [Summary("Command to turn on and off various logging information, and set specific reporting channels.")]
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


        [Command("info")]
        [Summary("Get current guild information on all log settings.")]
        [Remarks("- No additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LogInfoAsync()
        {
            var gColor = await Context.GetGuildColorAsync();
            var prefix = await Context.GetGuildPrefixAsync();
            var description = "";
            var title = $"Logs Information for {Context.Guild.Name}";

            var modLogChannel = await LogsToggleExtensions.GetModLogChannelAsync(Context.Guild.Id);
            description = $"**Current moderation log information:**\n\n";

            if (modLogChannel != null)
            {
                bool isFound = false;
                string isActive = modLogChannel.IsActive ? isActive = "ON" : isActive = "OFF";
                foreach (var channel in Context.Guild.Channels)
                    if (channel.Id.ToLong() == modLogChannel.ChannelId)
                    {
                        description +=
                            $"Current channel: **{channel.Name}** Moderation logging is currently set to: **{isActive}**\n\n";
                        isFound = true;
                    }

                if (isFound == false)
                {
                    await LogsToggleExtensions.DeleteModLogChannelAsync(modLogChannel);                 
                }

            }
            else
            {
                description +=
                    $"Moderation channel has not yet been set.\n Please use: {prefix.Prefix}logs modlogchannel #channelname\n\n";
            }

            var logChannel = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);
            description += $"**Current general log information:**\n\n";

            if (logChannel != null)
            {
                bool isFound = false;
                string isActive = logChannel.IsActive ? isActive = "ON" : isActive = "OFF";
                foreach (var channel in Context.Guild.Channels)
                    if (channel.Id.ToLong() == logChannel.ChannelId)
                    {
                        var val = await LogsToggleExtensions.GetLogsToggleAsync(Context.Guild);

                        description +=
                            $"Current channel: **{channel.Name}** general logging is currently set to: **{isActive}**\n\n";
                        isFound = true;

                        if (val.Count() > 0)
                        {
                            description +=
                                $"Current activated logs: {string.Join(", ", val.Select(x => x.LogName.ToString()))}";
                        }
                        else
                        {
                            description +=
                                $"Currently no activated logs in the logging channel.\n Please use {prefix.Prefix}logs togglelog (logname) to add to the active channel.\n\n";
                        }

                    }
                if (isFound == false)
                {
                    await LogsToggleExtensions.DeleteLogChannelAsync(logChannel);
                }
            } 
            else
            {
                description +=
                    $"General logging channel has not yet been set.\n Please use: {prefix.Prefix}logs logchannel #channelname\n\n";
            }            

            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithDescription(description);
            await ReplyAsync("", embed: builder);
        }

        [Command("logchannel")]
        [Summary("Set what guild channel is to receive general log activity.")]
        [Remarks("#channel")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LogAsync(SocketGuildChannel channel)
        {
            var logChannel = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);

            if (logChannel == null)
            {
                await LogsToggleExtensions.CreateLogChannelAsync(Context, channel);
                var subLogChannel = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);
                await LogsToggleExtensions.SetLogChannelIsActive(subLogChannel, true);
                await ReplyAsync($"{channel.Name} has been set as the active logging channel.");
                return;
            }

            await LogsToggleExtensions.SetLogChannel(logChannel, channel);
            await ReplyAsync($"{channel.Name} has been set as the active logging channel.");
        }

        [Command("modlogchannel")]
        [Summary("Set what guild channel is to receive moderator log activity.")]
        [Remarks("#channel")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ModLogAsync(SocketGuildChannel channel)
        {
            var modLogChannel = await LogsToggleExtensions.GetModLogChannelAsync(Context.Guild.Id);

            if (modLogChannel == null)
            {
                await LogsToggleExtensions.CreateModLogChannelAsync(Context, channel);
                var subModLogChannel = await LogsToggleExtensions.GetModLogChannelAsync(Context.Guild.Id);
                await LogsToggleExtensions.SetModLogChannelIsActive(subModLogChannel, true);
                await ReplyAsync($"{channel.Name} has been set as the active moderator logging channel.");
                return;
            }

            await LogsToggleExtensions.SetModLogChannel(modLogChannel, channel);
            await ReplyAsync($"{channel.Name} has been set as the active moderator logging channel.");
        }

        [Command("togglelog")]
        [Summary("toggle various logs on and off, including toggling on and off logchannel and modlogchannel.")]
        [Remarks(
            "(logchannel | modlogchannel | specific log) logchannel or modlogchannel will toggle master channel on/off.")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ToggleLogsAsync([Remainder] string name)
        {
            if (name.Equals("logchannel"))
            {
                var logChannel = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);

                if (logChannel == null)
                {
                    var prefix = await Context.GetGuildPrefixAsync();
                    await ReplyAsync(
                        $"Channel has not been set. Please use {prefix}logs logchannel #channelname to set your channel and activate general logging.");
                    return;
                }
                var isActive = logChannel.IsActive;
                if (isActive)
                {
                    await LogsToggleExtensions.SetLogChannelIsActive(logChannel, false);
                    await ReplyAsync("All general logging has been disabled for this guild.");
                    return;
                }
                await LogsToggleExtensions.SetLogChannelIsActive(logChannel, true);
                await ReplyAsync("All general logging has been enabled for this guild.");
                return;
            }

            if (name.Equals("modlogchannel"))
            {
                var modLogChannel = await LogsToggleExtensions.GetModLogChannelAsync(Context.Guild.Id);

                if (modLogChannel == null)
                {
                    var prefix = await Context.GetGuildPrefixAsync();
                    await ReplyAsync(
                        $"Channel has not been set. Please use {prefix}logs modlogchannel #channelname to set your channel and activate moderator logging.");
                    return;
                }
                var isActive = modLogChannel.IsActive;
                if (isActive)
                {
                    await LogsToggleExtensions.SetModLogChannelIsActive(modLogChannel, false);
                    await ReplyAsync("All moderator logging has been disabled for this guild.");
                    return;
                }
                await LogsToggleExtensions.SetModLogChannelIsActive(modLogChannel, true);
                await ReplyAsync("All moderator logging has been enabled for this guild.");
                return;
            }

        //    var logsToggled = LogsToggleExtensions.GetLogsToggleAsync(Context.Guild);

            if (SiotrixConstants.LogNamesCommandList.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                var logNameGuild = await LogsToggleExtensions.GetLogToggleAsync(Context.Guild.Id, name.ToLower());

                if (logNameGuild == null)
                {
                    await LogsToggleExtensions.CreateLogToggleAsync(Context.Guild.Id, name);
                    await ReplyAsync($"{name.ToLower()} has been **activated** for reporting.");
                    return;
                }
                else
                {
                    await LogsToggleExtensions.DeleteLogToggleAsync(logNameGuild);
                    await ReplyAsync($"{name.ToLower()} has been **de-activated** for reporting.");
                    return;
                }
            }
            else
            {
                var gColor = await Context.GetGuildColorAsync();
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.Guild.IconUrl)
                    .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                    .WithTitle($"Available log names to turn on/off in {Context.Guild}")
                    .WithDescription(string.Join(", ", SiotrixConstants.LogNamesCommandList));

                await ReplyAsync("", embed: builder);
            }
        }



















        /*   private string[] GetLogChannelPerGuild(long guildId, string cmd)
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
               var gPrefix = await Context.GetGuildPrefixAsync();
               var data = GetLogChannelPerGuild(Context.Guild.Id.ToLong(), command);
               var builder = new EmbedBuilder()
                   .WithAuthor(new EmbedAuthorBuilder()
                       .WithIconUrl(gIconUrl.Avatar)
                       .WithName(gName.GuildName)
                       .WithUrl(gUrl.SiteUrl))
                   .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
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
           } */
    }
}
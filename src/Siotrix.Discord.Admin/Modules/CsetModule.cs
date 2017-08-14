using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildAdmin)]
    public class CsetModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _map;
        private readonly CommandService _service;

        public CsetModule(CommandService service, IServiceProvider map)
        {
            _service = service;
            _map = map;
        }

        /* private long GetChannelIdFromName(string name)
        {
            long id = 0;
            if (name == null)
                return -1;
            foreach (var compare_channel in Context.Guild.Channels)
            {
                string compare_channel_name = "<#" + compare_channel.Id + ">";
                if (compare_channel_name.Equals(name))
                {
                    id = compare_channel.Id.ToLong();
                }
            }
            return id;
        }*/

        private bool GetToggleStatus(string cmd)
        {
            var status = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gtoggles.Where(p => p.CommandName.Equals(cmd) &&
                                                      p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    Console.WriteLine("------{0}++++++{1}", list.Count(), list.Any());
                    if (list.Count() > 0 || list.Any())
                    {
                        db.Gtoggles.RemoveRange(list);
                        status = true;
                    }
                    else
                    {
                        var val = new DiscordGuildToggle();
                        val.CommandName = cmd;
                        val.GuildId = Context.Guild.Id.ToLong();
                        db.Gtoggles.Add(val);
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

        private int GetToggleChannelStatus(string channelName, string command)
        {
            var status = 0;
            var getChannelId = channelName.GetChannelIdFromName(Context);
            if (getChannelId < 0) return -1;
            using (var db = new LogDatabase())
            {
                try
                {
                    var isFoundCmd = db.Gtoggles
                        .Where(p => p.CommandName.Equals(command) && p.GuildId.Equals(Context.Guild.Id.ToLong())).Any();
                    if (isFoundCmd)
                    {
                        status = 1;
                    }
                    else
                    {
                        var result =
                            db.Gtogglechannels.Where(p => p.ChannelId.Equals(getChannelId) &&
                                                          p.CommandName.Equals(command) &&
                                                          p.GuildId.Equals(Context.Guild.Id.ToLong()));
                        Console.WriteLine("------{0}++++++{1}", result.Count(), result.Any());
                        if (result.Any() || result.Count() > 0)
                        {
                            db.Gtogglechannels.RemoveRange(result);
                            status = 2;
                        }
                        else
                        {
                            var record = new DiscordGuildToggleChannel();
                            record.ChannelId = getChannelId;
                            record.GuildId = Context.Guild.Id.ToLong();
                            record.CommandName = command;
                            db.Gtogglechannels.Add(record);
                            status = 3;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return status;
        }

        private string GetDisableCommmands()
        {
            string cmdList = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var guildData = db.Gtoggles.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    var channelData = db.Gtogglechannels.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    Console.WriteLine("----{0}++++++{1}", guildData.Count(), guildData.Any());
                    if (guildData.Count() > 0 || guildData.Any())
                    {
                        cmdList = "- " + Format.Underline("In this guild\n");
                        foreach (var item in guildData)
                            cmdList += $"``{item.CommandName}``" + " , ";
                        if (cmdList.TrimEnd().EndsWith(","))
                            cmdList = cmdList.Truncate(2);
                    }
                    if (channelData.Count() > 0 || channelData.Any())
                    {
                        var channelList = channelData.GroupBy(p => p.ChannelId);
                        foreach (var channel in channelList)
                        {
                            cmdList += "\n- " + Format.Underline("In #" + Context.Guild.Channels
                                                                      .Where(p => p.Id.ToLong().Equals(channel.First()
                                                                          .ChannelId)).First().Name + " channel\n");
                            foreach (var element in channel)
                                cmdList += $"``{element.CommandName}``" + " , ";
                            if (cmdList.TrimEnd().EndsWith(","))
                                cmdList = cmdList.Truncate(2);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return cmdList;
        }

        private string GetToggledCommands()
        {
            string cmdList = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var guildData = db.Gtoggles.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    var channelData = db.Gtogglechannels.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    //ArrayList myArr = new ArrayList();
                    var arr = new List<string>();
                    Console.WriteLine("----{0}++++++{1}", guildData.Count(), guildData.Any());
                    foreach (var a in channelData)
                        arr.Add(a.CommandName);
                    var distinct = arr.Distinct().ToList();
                    Console.WriteLine("++++++{0}---{1}==={2}", arr.Count, guildData.Count(), guildData.Any());
                    if (guildData.Count() > 0 || guildData.Any())
                    {
                        foreach (var item in guildData)
                            cmdList += $"``{item.CommandName}``" + " , ";
                        foreach (var i in guildData.ToList())
                        foreach (var j in channelData.ToList())
                            if (i.CommandName.Equals(j.CommandName))
                                distinct.Remove(j.CommandName);
                    }
                    Console.WriteLine("======={0}*********{1}", distinct.Count, cmdList);

                    if (distinct != null || distinct.Count > 0)
                        foreach (var element in distinct)
                            cmdList += $"``{element}``" + " , ";
                    if (cmdList.TrimEnd().EndsWith(","))
                        cmdList = cmdList.Truncate(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return cmdList;
        }

        private string GetGroupCommandContents(string cmd, bool isGroupCommand)
        {
            string toggleFound = null;
            string summary = null;
            string channels = null;
            if (isGroupCommand)
                summary = _service.Modules.Where(p => p.Aliases.First().Equals(cmd)).First().Summary;
            else
                summary = _service.Commands.Where(p => p.Name.Equals(cmd)).First().Summary;

            toggleFound = "***- Summary*** : " + summary + "\n";

            using (var db = new LogDatabase())
            {
                try
                {
                    var isFoundCmd = db.Gtoggles
                        .Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(Context.Guild.Id.ToLong())).Any();
                    if (isFoundCmd)
                    {
                        toggleFound += "***- Status*** : " + "**Toggled off** in this guild";
                    }
                    else
                    {
                        var list = db.Gtogglechannels.Where(
                            p => p.CommandName.Equals(cmd) && p.GuildId.Equals(Context.Guild.Id.ToLong()));
                        if (list.Any() || list.Count() > 0)
                        {
                            foreach (var item in list)
                                channels += "``#" + Context.Guild.Channels
                                                .Where(p => p.Id.ToLong().Equals(item.ChannelId)).First().Name +
                                            "`` and ";
                            if (channels.TrimEnd().EndsWith("and"))
                                channels = channels.Truncate(4);
                            toggleFound += "***- Status*** : " + "**Toggled off** in " + channels;
                        }
                        else
                        {
                            toggleFound += "***- Status*** : " + "**Toggled on** in this guild";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return toggleFound;
        }

        [Command("cset")]
        [Summary("Turn a command on or off globally or for a specific channel.")]
        [Remarks("<command> toggle (optional:#Channelname)")]
        public async Task Cset(string command, [Remainder] string str)
        {
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gColor =  await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();

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
                .WithTimestamp(DateTime.UtcNow);
            var isFound = _service.Commands.Where(p => p.Name.Equals(command)).Any();
            var isGroupFound = _service.Modules.Where(p => p.Aliases.First().Equals(command)).Any();
            Console.WriteLine("********{0}^^^^^^^{1}", isFound, isGroupFound);
            if (!isFound && !isGroupFound) return;
            if (command == "cset")
            {
                await ReplyAsync($"I'm sorry {Context.User.Mention}, but I cannot turn off cset for obvious reasons.");
                return;
            }
            var toggle = str.Substring(0, 6);
            if (str.Length < 7)
            {
                if (toggle.Equals("toggle"))
                {
                    var toggleOn = GetToggleStatus(command);
                    Console.WriteLine("========{0}", toggleOn);
                    if (toggleOn)
                        await ReplyAsync($"✅ : Toggled ***{command}*** command **on** in this guild !");
                    else
                        await ReplyAsync($"✖️ : Toggled ***{command}*** command **off** in this guild !");
                }
            }
            else
            {
                if (str.Length <= 13)
                    return;
                var toggleChannel = str.Substring(0, 13);
                if (toggleChannel.Equals("togglechannel") && str.Substring(13, 1).Equals(" ")
                ) // at least, must include one backspace after togglechannel
                {
                    var channelName = str.Substring(13, str.Length - 13).Trim();
                    Console.WriteLine("-----{0}====={1}++++{2}", str.Length, channelName.Length,
                        str.Length - toggleChannel.Length - channelName.Length);

                    if (str.Length - toggleChannel.Length - channelName.Length > 0) // must include backspace
                    {
                        var toggleChannelStatus = GetToggleChannelStatus(channelName, command);
                        Console.WriteLine("========{0}", toggleChannelStatus);
                        if (toggleChannelStatus == 1)
                            await ReplyAsync(
                                $"📣 : ***{command}*** command **toggled off** in this guild. Please toggle on globally first!");
                        else if (toggleChannelStatus == 2)
                            await ReplyAsync($"✅ : Toggled ***{command}*** command **on** in {channelName}!");
                        else if (toggleChannelStatus == 3)
                            await ReplyAsync($"✖️ : Toggled ***{command}*** command **off** in {channelName}!");
                        //await ReplyAsync("✅✖️✔️📪📣🔥🌿🍃🌱⚡❄☁💧💦⭕🐛🌟💫✨💢🎵🔇🔊🐼");
                    }
                }
            }
        }

        [Command("cset")]
        [Summary("List current status of commands turned off in this guild.")]
        [Remarks("toggled - must be that word exactly.")]
        public async Task Cset(string param)
        {
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gColor = await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();
            string result = null;
            EmbedBuilder builder = null;

            var isFound = _service.Commands.Where(p => p.Name.Equals(param)).Any();
            var isGroupFound = _service.Modules.Where(p => p.Aliases.First().Equals(param)).Any();
            Console.WriteLine("______{0}_ _ _ _ _{1}", isFound, isGroupFound);
            if (!isFound && !isGroupFound)
            {
                if (param.Equals("toggled"))
                {
                    var list = GetDisableCommmands();
                    if (list == null) list = "None";
                    builder = new EmbedBuilder();
                    builder
                        .AddField(x =>
                        {
                            x.Name = "Commands Disabled:";
                            x.Value = $"{list}";
                        });
                    builder
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(gIconUrl.Avatar)
                            .WithName(gName.GuildName)
                            .WithUrl(gUrl.SiteUrl))
                        .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                        .WithThumbnailUrl(gThumbnail.ThumbNail)
                        .WithFooter(new EmbedFooterBuilder()
                            .WithIconUrl(gFooter.FooterIcon)
                            .WithText(gFooter.FooterText))
                        .WithTimestamp(DateTime.UtcNow);
                }
            }
            else
            {
                if (!isFound && !isGroupFound) return;
                if (!isFound && isGroupFound)
                    result = GetGroupCommandContents(param, true);
                else if (isFound && !isGroupFound)
                    result = GetGroupCommandContents(param, false);
                builder = new EmbedBuilder();
                builder
                    .AddField(x =>
                    {
                        x.Name = Format.Underline("Commands Description:");
                        x.Value = $"{result}";
                    });
                builder
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(gIconUrl.Avatar)
                        .WithName(gName.GuildName)
                        .WithUrl(gUrl.SiteUrl))
                    .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                    .WithThumbnailUrl(gThumbnail.ThumbNail)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow);
            }
            await ReplyAsync("", embed: builder);
        }
    }
}
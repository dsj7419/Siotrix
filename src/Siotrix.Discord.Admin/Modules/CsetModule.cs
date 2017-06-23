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
    public class CsetModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _map;

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
            bool status = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gtoggles.Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(Context.Guild.Id.ToLong()));
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

        private int GetToggleChannelStatus(string channel_name, string command)
        {
            int status = 0;
            long get_channel_id = ChannelNameExtensions.GetChannelIdFromName(channel_name, Context);
            if (get_channel_id < 0) return -1;
            using (var db = new LogDatabase())
            {
                try
                {
                    var is_found_cmd = db.Gtoggles.Where(p => p.CommandName.Equals(command) && p.GuildId.Equals(Context.Guild.Id.ToLong())).Any();
                    if (is_found_cmd)
                    {
                        status = 1;
                    }
                    else
                    {
                        var result = db.Gtogglechannels.Where(p => p.ChannelId.Equals(get_channel_id) && p.CommandName.Equals(command) && p.GuildId.Equals(Context.Guild.Id.ToLong()));
                        Console.WriteLine("------{0}++++++{1}", result.Count(), result.Any());
                        if (result.Any() || result.Count() > 0)
                        {
                            db.Gtogglechannels.RemoveRange(result);
                            status = 2;
                        }
                        else
                        {
                            var record = new DiscordGuildToggleChannel();
                            record.ChannelId = get_channel_id;
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
            string cmd_list = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var guild_data = db.Gtoggles.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    var channel_data = db.Gtogglechannels.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    Console.WriteLine("----{0}++++++{1}", guild_data.Count(), guild_data.Any());
                    if (guild_data.Count() > 0 || guild_data.Any())
                    {
                        cmd_list = "- " + Format.Underline("In this guild\n");
                        foreach (var item in guild_data)
                        {
                            cmd_list += $"``{item.CommandName}``" + " , ";
                        }
                        if (cmd_list.TrimEnd().EndsWith(","))
                            cmd_list = cmd_list.Truncate(2);
                    }
                    if(channel_data.Count() > 0 || channel_data.Any())
                    {
                        var channel_list = channel_data.GroupBy(p => p.ChannelId);
                        foreach(var channel in channel_list)
                        {
                            cmd_list += "\n- " + Format.Underline("In #" + Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(channel.First().ChannelId)).First().Name + " channel\n");
                            foreach(var element in channel)
                            {
                                cmd_list += $"``{element.CommandName}``" + " , ";
                            }
                            if (cmd_list.TrimEnd().EndsWith(","))
                                cmd_list = cmd_list.Truncate(2);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return cmd_list;
        }

        private string GetToggledCommands()
        {
            string cmd_list = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var guild_data = db.Gtoggles.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    var channel_data = db.Gtogglechannels.Where(p => p.GuildId.Equals(Context.Guild.Id.ToLong()));
                    //ArrayList myArr = new ArrayList();
                    List<string> arr = new List<string>();
                    Console.WriteLine("----{0}++++++{1}", guild_data.Count(), guild_data.Any());
                    foreach (var a in channel_data)
                    {
                        arr.Add(a.CommandName);
                    }
                    List<string> distinct = arr.Distinct().ToList();
                    Console.WriteLine("++++++{0}---{1}==={2}", arr.Count, guild_data.Count(), guild_data.Any());
                    if (guild_data.Count() > 0 || guild_data.Any())
                    {
                        foreach (var item in guild_data)
                        {
                            cmd_list += $"``{item.CommandName}``" + " , ";
                        }
                        foreach (var i in guild_data.ToList())
                        {
                            foreach (var j in channel_data.ToList())
                            {
                                if (i.CommandName.Equals(j.CommandName))
                                {
                                    distinct.Remove(j.CommandName);
                                }
                            }
                        }
                    }
                    Console.WriteLine("======={0}*********{1}", distinct.Count, cmd_list);

                    if(distinct != null || distinct.Count > 0)
                    {
                        foreach (var element in distinct)
                        {
                            cmd_list += $"``{element}``" + " , ";
                        }
                    }
                    if (cmd_list.TrimEnd().EndsWith(","))
                        cmd_list = cmd_list.Truncate(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return cmd_list;
        }

        private string GetGroupCommandContents(string cmd, bool is_group_command)
        {
            string toggle_found = null;
            string summary = null;
            string channels = null;
            if (is_group_command)
            {
                summary = _service.Modules.Where(p => p.Aliases.First().Equals(cmd)).First().Summary;
            }
            else
            {
                summary = _service.Commands.Where(p => p.Name.Equals(cmd)).First().Summary;
            }

            toggle_found = "***- Summary*** : " + summary + "\n";

            using (var db = new LogDatabase())
            {
                try
                {
                    var is_found_cmd = db.Gtoggles.Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(Context.Guild.Id.ToLong())).Any();
                    if (is_found_cmd)
                    {
                        toggle_found += "***- Status*** : " + "**Toggled off** in this guild";
                    }
                    else
                    {
                        var list = db.Gtogglechannels.Where(p => p.CommandName.Equals(cmd) && p.GuildId.Equals(Context.Guild.Id.ToLong()));
                        if (list.Any() || list.Count() > 0)
                        {
                            foreach(var item in list)
                            {
                                 channels += "``#" + Context.Guild.Channels.Where(p => p.Id.ToLong().Equals(item.ChannelId)).First().Name + "`` and ";
                            }
                            if (channels.TrimEnd().EndsWith("and"))
                                channels = channels.Truncate(4);
                            toggle_found += "***- Status*** : " + "**Toggled off** in " + channels;
                        }
                        else
                        {
                            toggle_found += "***- Status*** : " + "**Toggled on** in this guild";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return toggle_found;
        }

        [Command("cset")]
        [Summary("Turn a command on or off globally or for a specific channel.")]
        [Remarks("<command> toggle (optional:#Channelname)")]
        public async Task Cset(string command, [Remainder]string str)
        {
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            
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
                .WithTimestamp(DateTime.UtcNow);
            bool is_found = _service.Commands.Where(p => p.Name.Equals(command)).Any();
            bool is_group_found = _service.Modules.Where(p => p.Aliases.First().Equals(command)).Any();
            Console.WriteLine("********{0}^^^^^^^{1}", is_found, is_group_found);
            if (!is_found && !is_group_found) return;
            if (command == "cset")
            {
                await ReplyAsync($"I'm sorry {Context.User.Mention}, but I cannot turn off cset for obvious reasons.");
                return;
            }
            string toggle = str.Substring(0, 6);
            if (str.Length < 7)
            {
                if (toggle.Equals("toggle"))
                {
                    bool toggle_on = GetToggleStatus(command);
                    Console.WriteLine("========{0}", toggle_on);
                    if (toggle_on)
                    {
                        await ReplyAsync($"✅ : Toggled ***{command}*** command **on** in this guild !");
                    }
                    else
                    {
                        await ReplyAsync($"✖️ : Toggled ***{command}*** command **off** in this guild !");
                    }
                }
            }
            else
            {
                if (str.Length <= 13) return;
                else
                {
                    string toggle_channel = str.Substring(0, 13);
                    if (toggle_channel.Equals("togglechannel") && str.Substring(13, 1).Equals(" ")) // at least, must include one backspace after togglechannel
                    {
                        string channel_name = str.Substring(13, str.Length - 13).Trim();
                        System.Console.WriteLine("-----{0}====={1}++++{2}", str.Length, channel_name.Length, str.Length - toggle_channel.Length - channel_name.Length);

                        if (str.Length - toggle_channel.Length - channel_name.Length > 0) // must include backspace
                        {
                            int toggle_channel_status = GetToggleChannelStatus(channel_name, command);
                            Console.WriteLine("========{0}", toggle_channel_status);
                            if (toggle_channel_status == 1)
                            {
                                await ReplyAsync($"📣 : ***{command}*** command **toggled off** in this guild. Please toggle on globally first!");
                            }
                            else if (toggle_channel_status == 2)
                            {
                                await ReplyAsync($"✅ : Toggled ***{command}*** command **on** in {channel_name}!");
                            }
                            else if (toggle_channel_status == 3)
                            {
                                await ReplyAsync($"✖️ : Toggled ***{command}*** command **off** in {channel_name}!");
                            }
                            //await ReplyAsync("✅✖️✔️📪📣🔥🌿🍃🌱⚡❄☁💧💦⭕🐛🌟💫✨💢🎵🔇🔊🐼");
                        }
                    }
                }
            }
        }

        [Command("cset")]
        [Summary("List current status of commands turned off in this guild.")]
        [Remarks("toggled - must be that word exactly.")]
        public async Task Cset(string param)
        {
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            string result = null;
            EmbedBuilder builder = null;
                
            bool is_found = _service.Commands.Where(p => p.Name.Equals(param)).Any();
            bool is_group_found = _service.Modules.Where(p => p.Aliases.First().Equals(param)).Any();
            Console.WriteLine("______{0}_ _ _ _ _{1}", is_found, is_group_found);
            if (!is_found && !is_group_found)
            {
                if (param.Equals("toggled"))
                {
                    string list = GetDisableCommmands();
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
                        .WithIconUrl(g_icon_url)
                        .WithName(g_name)
                        .WithUrl(g_url))
                        .WithColor(g_color)
                        .WithThumbnailUrl(g_thumbnail)
                        .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(g_footer[0])
                        .WithText(g_footer[1]))
                        .WithTimestamp(DateTime.UtcNow);
                }
            }
            else
            {
                if (!is_found && !is_group_found) return;
                if (!is_found && is_group_found)
                {
                    result = GetGroupCommandContents(param, true);
                }
                else if (is_found && !is_group_found)
                {
                    result = GetGroupCommandContents(param, false);
                }
                builder = new EmbedBuilder();
                builder
                        .AddField(x =>
                        {
                            x.Name = Format.Underline("Commands Description:");
                            x.Value = $"{result}";
                        });
                builder
                        .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(g_icon_url)
                        .WithName(g_name)
                        .WithUrl(g_url))
                        .WithColor(g_color)
                        .WithThumbnailUrl(g_thumbnail)
                        .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(g_footer[0])
                        .WithText(g_footer[1]))
                        .WithTimestamp(DateTime.UtcNow);
            }
            await ReplyAsync("", embed: builder);
        }
    }
}
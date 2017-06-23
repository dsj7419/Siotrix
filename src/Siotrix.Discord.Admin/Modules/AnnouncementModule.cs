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
        private bool GetToggleStatus(int cmd_id)
        {
            bool status = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.CommandId == cmd_id && p.GuildId == Context.Guild.Id.ToLong());
                    if (list.Any())
                    {
                        db.Gannouncetoggles.RemoveRange(list);
                        status = true;
                    }
                    else
                    {
                        var val = new DiscordGuildAnnounceToggle();
                        val.CommandId = cmd_id;
                        val.GuildId = Context.Guild.Id.ToLong();
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

<<<<<<< HEAD
        private string GetAnnouncementInfo(long guild_id)
=======
        [Command("welcome")]
        [Summary("Turn welcome message on or off when user joins your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string toggle)
>>>>>>> 8c63bae45c609a7713571acb77b39c751a727a3a
        {
            string info = null;
            string welcome_data = "Welcome Off";
            string leave_data = "Leave Off";
            string return_data = "Return Off";
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Gannouncetoggles.Where(p => p.GuildId == guild_id);
                    if (list.Any())
                    {
                        foreach(var item in list)
                        {
                            if (item.CommandId == 1)
                                welcome_data = "Welcome On";
                            else if (item.CommandId == 2)
                                leave_data = "Welcome On";
                            else if (item.CommandId == 3)
                                return_data = "Return On";
                        }
                        info = welcome_data + "\n" + leave_data + "\n" + return_data;
                    }
                    else
                        info = "No Informations";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return info;
        }

<<<<<<< HEAD
        [Command]
        [Summary("- welcome")]
        [Remarks(" - need some arguments")]
=======
        [Command("leave")]
        [Summary("Turn the leave message on or off when user leaves your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
>>>>>>> 8c63bae45c609a7713571acb77b39c751a727a3a
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task AnnouncementAsync()
        {
            var info = GetAnnouncementInfo(Context.Guild.Id.ToLong());
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
               x.Value = info;
           });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

<<<<<<< HEAD
        [Command("welcome")]
        [Summary("- welcome")]
        [Remarks(" - need some arguments")]
=======
        [Command("return")]
        [Summary("Turn the return message on or off when user rejoins your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
>>>>>>> 8c63bae45c609a7713571acb77b39c751a727a3a
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string param, [Remainder]string text = null)
        {
            
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(1);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **off** in this guild !");
            }
<<<<<<< HEAD
            else if (param.Equals("set"))
=======
        }

        [Command("welcome")]
        [Summary("- welcome")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string set, [Remainder]string text)
        {
            if (set.Equals("set"))
>>>>>>> 8c63bae45c609a7713571acb77b39c751a727a3a
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
                        await ReplyAsync("👍");
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
        }

        [Command("leave")]
        [Summary("- leave")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LeaveAsync(string param, [Remainder]string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(2);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **off** in this guild !");
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
                        await ReplyAsync("👍");
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
        }

        [Command("return")]
        [Summary("- return")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ReturnAsync(string param, [Remainder]string text = null)
        {
            if (param.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(3);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **off** in this guild !");
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
                        await ReplyAsync("👍");
                }
                else
                    await ReplyAsync("📣 : You can not use this command because it has been toggled off in this guild.");
            }
        }
    }
}
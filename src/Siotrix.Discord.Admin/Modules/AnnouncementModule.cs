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

        [Command("welcome")]
        [Summary("Turn welcome message on or off when user joins your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string toggle)
        {
            if (toggle.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(1);
                if(toggle_on)
                    await ReplyAsync($"✅ : Toggled ***welcome*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***welcome*** command **on** in this guild !");
            }
        }

        [Command("leave")]
        [Summary("Turn the leave message on or off when user leaves your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LeaveAsync(string toggle)
        {
            if (toggle.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(2);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***leave*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***leave*** command **on** in this guild !");
            }
        }

        [Command("return")]
        [Summary("Turn the return message on or off when user rejoins your guild.")]
        [Remarks("toggle - use keyword toggle to turn on or off.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ReturnAsync(string toggle)
        {
            if (toggle.Equals("toggle"))
            {
                var toggle_on = GetToggleStatus(3);
                if (toggle_on)
                    await ReplyAsync($"✅ : Toggled ***return*** command **on** in this guild !");
                else
                    await ReplyAsync($"✖️ : Toggled ***return*** command **on** in this guild !");
            }
        }

        [Command("welcome")]
        [Summary("- welcome")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WelcomeAsync(string set, [Remainder]string text)
        {
            if (set.Equals("set"))
            {
                var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 1, text);
                if(success)
                    await ReplyAsync("👍");
            }
        }

        [Command("leave")]
        [Summary("- leave")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task LeaveAsync(string set, [Remainder]string text)
        {
            if (set.Equals("set"))
            {
                var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 2, text);
                if (success)
                    await ReplyAsync("👍");
            }
        }

        [Command("return")]
        [Summary("- return")]
        [Remarks("- need some arguments")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task ReturnAsync(string set, [Remainder]string text)
        {
            if (set.Equals("set"))
            {
                var success = SaveAndUpdateAnnounceMessage(Context.Guild.Id.ToLong(), 3, text);
                if (success)
                    await ReplyAsync("👍");
            }
        }
    }
}
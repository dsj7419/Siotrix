using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Statistics
{
    [Name("Information")]
    [Summary("General information command with detailed reporting.")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute(CommandInfo info)
        {
            _process = Process.GetCurrentProcess();
        }


        private async Task<string> GetGuildIconUrl(int id)
        {
            string iconurl;

            if (id == 2)
            {
                var val = await Context.GetGuildIconUrlAsync();
                iconurl = val.Avatar;
            }
            else
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                iconurl = val.AuthorIcon;
            }
            return iconurl;
        }

        private async Task<string> GetGuildName(int id)
        {
            string name;

            if (id == 2)
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                name = val.AuthorName;
            }
            else
            {
                var val = await Context.GetGuildNameAsync();
                name = val.GuildName;
            }

            return name;
        }

        private async Task<string> GetGuildUrl(int id)
        {
            string url = null;

            if (id == 2)
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                url = val.AuthorUrl;
            }
            else
            {
                var val = await Context.GetGuildUrlAsync();
                url = val.SiteUrl;
            }
            return url;
        }

        private async Task<string> GetGuildDescription(int id)
        {
            string description = null;

            if (id == 2)
            {
                var val = await SiotrixEmbedInfoExtensions.GetSiotrixInfoAsync();
                description = val.BotInfo;
            }
            else
            {
                var val = await Context.GetGuildDescriptionAsync();
                description = val.Description;
            }
            return description;
        }

        private async Task<string> GetGuildFooterIcon(int id)
        {
            string footerIcon;

            if (id == 2)
            {
                var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
                footerIcon = val.FooterIcon;
            }
            else
            {
                var val = await Context.GetGuildFooterAsync();
                footerIcon = val.FooterIcon;
            }
            return footerIcon;
        }

        private async Task<string> GetGuildFooterText(int id)
        {
            string footerText;

            if (id == 2)
            {
                var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
                footerText = val.FooterText;
            }
            else
            {
                var val = await Context.GetGuildFooterAsync();
                footerText = val.FooterText;
            }
            return footerText;
        }

        private bool CheckBot(SocketUser user)
        {
            if (user.IsBot)
                return true;
            return false;
        }

        private string GetUptime()
        {
            var uptime = DateTime.Now - _process.StartTime;
            string dayname = null;
            if (uptime.Days == 1)
                dayname = "day";
            else
                dayname = "days";
            return $"{uptime.Days} {dayname} {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private string[] GetLifeTimeMessages(SocketUser user)
        {
            var cnt = 0;
            var arr = new string[2];
            var today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    cnt = db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).ToList().Count;
                    var lifeTime = (today - db.Messages.Where(p => p.AuthorId.Equals(user.Id.ToLong()) && !p.IsBot)
                                        .Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = Math.Round(cnt / lifeTime, 2).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return arr;
        }

        private string GetLastMessageTime(SocketUser user)
        {
            var last = "-";
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).ToList().Count > 0)
                    {
                        var date = db.Messages.Where(p => !p.IsBot && p.AuthorId == user.Id.ToLong()).Last().CreatedAt;
                        last = string.Format("{0:dddd, MMMM d, yyyy}", date);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return last;
        }

        private string GetActiveGuildName()
        {
            var cnt = 0;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.GroupBy(p => p.GuildId).ToList();
                    foreach (var item in list)
                        if (db.Messages.Where(p => !p.IsBot && p.GuildId == item.First().GuildId).ToList().Count > cnt)
                        {
                            cnt = db.Messages.Where(p => !p.IsBot && p.GuildId == item.First().GuildId).ToList().Count;
                            name = item.First().GuildName;
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        [Command("info")]
        [Alias("information")]
        [Summary("Retrieves general information about guild")]
        [Remarks(" - no additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public async Task InfoAsync()
        {
            var gIconUrl = await GetGuildIconUrl(0);
            var gName = await GetGuildName(0);
            var gUrl = await GetGuildUrl(0);
            var gColor = await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gDescription = await GetGuildDescription(0);
            var gFooterIcon = await GetGuildFooterIcon(0);
            var gFooterText = await GetGuildFooterText(0);
            var gPrefix = await Context.GetGuildPrefixAsync();

            var dt = new DateTime(Context.Guild.CreatedAt.Year, Context.Guild.CreatedAt.Month,
                Context.Guild.CreatedAt.Day, 0, 0, 0, 0);
            var establishedDate = string.Format("{0:dddd, MMMM d, yyyy}", dt) + " | " +
                                   Math.Round((DateTime.Now - Context.Guild.CreatedAt.DateTime).TotalDays, 0) +
                                   " Days Ago!";


            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithDescription(gDescription)
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithTitle("General Information sheet for " + gName)
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooterIcon)
                    .WithText(gFooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Server Owner"),
                    Value = Context.Guild.Owner
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Guild Established"),
                    Value = establishedDate
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Roles"),
                    Value = Context.Guild.Roles.Count()
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Member Count"),
                    Value = Context.Guild.Users.Count()
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Bot Count"),
                    Value = Context.Guild.Users.Where(b => b.IsBot).Count()
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Channel Count"),
                    Value = Context.Guild.Channels.Count()
                })
                .AddField(x =>
                {
                    x.Name = Format.Underline("Avatar URL");
                    x.Value = GetGuildIconUrl(0);
                })
                .AddField(x =>
                {
                    x.Name = $"For guild statistical data type {gPrefix.Prefix}stats";
                    x.Value = $"Uptime: {GetUptime()}";
                });

            await ReplyAsync("", embed: builder);
        }

        [Command("info")]
        [Alias("information")]
        [Summary("Retrieves general information about a specific user or bot in the guild")]
        [Remarks("<@username> - any @user in guild. May also user it for the bot as well!")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public async Task InfoAsync(SocketUser user)
        {
            var id = 0;
            var bot = CheckBot(user);
            var person = user as IGuildUser;
            if (!bot)
                id = 1;
            else
                id = 2;
            var gIconUrl = await GetGuildIconUrl(id);
            var gName = await GetGuildName(id);
            var gUrl = await GetGuildUrl(id);
            var gColor = await Context.GetGuildColorAsync();
            var gDescription = await GetGuildDescription(id);
            var gFooterIcon = await GetGuildFooterIcon(id);
            var gFooterText = await GetGuildFooterText(id);
            var mCount = GetLifeTimeMessages(user);
            var gPrefix = await Context.GetGuildPrefixAsync();

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooterIcon)
                    .WithText(gFooterText))
                .WithTimestamp(DateTime.UtcNow);
            if (id == 1)
            {
                var joined = (DateTime.Now - person.JoinedAt)?.TotalDays ?? 0;
                var joinDate = string.Format("{0:dddd, MMMM d, yyyy}", person.JoinedAt?.DateTime ?? DateTime.Now);
                var daysOld = Math.Round((DateTime.Now - (person.JoinedAt?.DateTime ?? DateTime.Now)).TotalDays, 0);
                string daysOldConcat = null;
                if (daysOld < 1) daysOld = 1;
                if (daysOld == 1)
                    daysOldConcat = joinDate + " | " + daysOld + " day ago.";
                else
                    daysOldConcat = joinDate + " | " + daysOld + " days ago.";

                var lastSeen = GetLastMessageTime(user);
                builder
                    .WithTitle("General Information sheet for " + Context.Guild.GetUser(user.Id).Username)
                    .WithThumbnailUrl(person.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("ID : "),
                        Value = person.Id
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Nickname : "),
                        Value = person.Nickname ?? "None"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Past Names : "),
                        Value = person.Nickname ?? "None"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Avatar : "),
                        Value = person.GetAvatarUrl()
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Playing : "),
                        Value = person.Game.ToString() != "" ? person.Game.ToString() : "Nothing Listed"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Status : "),
                        Value = person.Status.ToString()
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Joined Server : "),
                        Value = daysOldConcat
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("lifetime messages : "),
                        Value = mCount[0]
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Messages / hour : "),
                        Value = mCount[1]
                    })
                    //    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Joined Server : "), Value = $" {Math.Round(joined, 0) - 1} Days Ago!" })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Last Seen : "),
                        Value = lastSeen
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Last Spoke : "),
                        Value = lastSeen
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Roles : "),
                        Value = person.Guild.Roles.Count
                    });
            }
            else if (id == 2)
            {
                builder
                    .WithTitle("Information for Siotrix")
                    .WithDescription(gDescription)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Owner : "),
                        Value = Context.User.Id
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Bot Version : "),
                        Value = SiotrixConstants.BotVersion
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Guilds : "),
                        Value = Context.Client.Guilds.Count
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Users : "),
                        Value = Context.Guild.Users.Count
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("invite to guild : "),
                        Value = SiotrixConstants.BotInvite
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Siotrix Guild : "),
                        Value = SiotrixConstants.DiscordInv
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Donate : "),
                        Value = SiotrixConstants.BotDonate
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Created : "),
                        Value = Math.Round((DateTime.Now - Context.Client.CurrentUser.CreatedAt.DateTime).TotalDays,
                                    0) + " Days ago."
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Icon : "),
                        Value = Context.Client.CurrentUser.GetAvatarUrl()
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Most Active Guild This Week : "),
                        Value = GetActiveGuildName()
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Uptime : "),
                        Value = GetUptime()
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Contributors : "),
                        Value = Format.Bold($"Dan Johnson. See {gPrefix.Prefix}about for more info!")
                    });
            }
            await ReplyAsync("", embed: builder);
        }
    }
}
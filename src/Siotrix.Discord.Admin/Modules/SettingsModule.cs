using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Siotrix.Discord.Attributes.Preconditions;
using Discord.Addons.EmojiTools;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [Summary("Various settings for guild to customize Siotrix with.")]
    [Group("settings"), Alias("set")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        [Summary("Will list bots current avatar.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task AvatarAsync()
            => ReplyAsync(Context.Client.CurrentUser.GetAvatarUrl());

        [Command("avatar")]
        [Summary("Will set bots avatar.")]
        [Remarks("<url> - url of picture to assign as bot avatar **note** using keyword reset will reset to Siotrix avatar.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AvatarAsync(Uri url)
        {
            if (url.ToString().Equals("reset"))
            {
                url = new Uri("https://s27.postimg.org/hgn3yw4gz/Siotrix_Logo_Side_Alt1_No_Text.png");
            }
            var request = new HttpRequestMessage(new HttpMethod("GET"), url);

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var stream = await response.Content.ReadAsStreamAsync();

                var self = Context.Client.CurrentUser;
                await self.ModifyAsync(x =>
                {
                    x.Avatar = new Image(stream);
                });
                await ReplyAsync("👍");
            }
        }

        [Command("authoricon")]
        [Summary("Will list bots current author icon.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorIconAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        url = db.Authors.First().AuthorIcon;
                        if (url == null || url == "")
                        {
                            url = "http://img04.imgland.net/WyZ5FoM.png";
                            var data = db.Authors.First();
                            data.AuthorIcon = url;
                            db.Authors.Update(data);
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("authoricon")]
        [Summary("Will set bots author icon.")]
        [Remarks("<url> - url of picture to assign as bot author icon **note** using keyword reset will reset to Siotrix icon.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorIconAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                if (url.ToString().Equals("reset"))
                {
                    val.AuthorIcon = "http://img04.imgland.net/WyZ5FoM.png";
                }
                else
                {
                    val.AuthorIcon = url.ToString();
                }
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        db.Authors.Add(val);
                    }
                    else
                    {
                        var data = db.Authors.First();
                        data.AuthorIcon = val.AuthorIcon;
                        db.Authors.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("authorurl")]
        [Summary("Will list bots current author url.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorUrlAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        url = "No Url";
                    }
                    else
                    {
                        url = db.Authors.First().AuthorUrl;
                        if (url == null || url.ToString() == "")
                        {
                            url = "No Url";
                            var data = db.Authors.First();
                            data.AuthorUrl = "";
                            db.Authors.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }
        
        [Command("authorurl")]
        [Summary("Will set bots author url.")]
        [Remarks("<url> - This links author name as a hyperlink. **note** using keyword reset will reset to Siotrix url.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorUrlAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                if (url.ToString().Equals("reset"))
                {
                    val.AuthorUrl = "";
                }
                else
                {
                    val.AuthorUrl = url.ToString();
                }
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        db.Authors.Add(val);
                    }
                    else
                    {
                        var data = db.Authors.First();
                        data.AuthorUrl = val.AuthorUrl;
                        db.Authors.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("authorname")]
        [Summary("Will set bots current author name.")]
        [Remarks("<name> - This defaults to your guild name. **note** You can use reset as the parameter to reset back to your guild name.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorNameAsync([Remainder] string txt)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                if (txt.Equals("reset"))
                {
                    val.AuthorName = Context.Guild.Name;
                }
                else
                {
                    val.AuthorName = txt;
                }
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        db.Authors.Add(val);
                    }
                    else
                    {
                        var data = db.Authors.First();
                        data.AuthorName = val.AuthorName;
                        db.Authors.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("authorname")]
        [Summary("Will list bots current author name.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorNameAsync()
        {
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Authors == null || db.Authors.ToList().Count <= 0)
                    {
                        txt = Context.Guild.Name;
                    }
                    else
                    {
                        txt = db.Authors.First().AuthorName;
                        if (txt == null || txt == "")
                        {
                            txt = Context.Guild.Name;
                            var data = db.Authors.First();
                            data.AuthorName = txt;
                            db.Authors.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(txt);
        }

        [Command("gfootericon")]
        [Summary("Will list bots current footer icon.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task FooterIconAsync()
        {
            CheckGuildFooters();
            var guild_id = Context.Guild.Id;
            string url = null;
            using(var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        url = val.First().FooterIcon;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("gfootericon")]
        [Summary("Will set bots footer icon.")]
        [Remarks("<url> - url of picture to assign as bot footer icon **note** using keyword reset will reset to Siotrix icon.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task FooterIconAsync(Uri url)
        {
            CheckGuildFooters();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                if (url.ToString().Equals("reset"))
                {
                    val.FooterIcon = "http://img04.imgland.net/WyZ5FoM.png";
                }
                else
                {
                    val.FooterIcon = url.ToString();
                }
                try
                {
                    var arr = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.FooterIcon = val.FooterIcon;
                        db.Gfooters.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildFooters()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gfooters.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildFooter();
                        instance.GuildId = id;
                        instance.FooterIcon = "http://img04.imgland.net/WyZ5FoM.png";
                        instance.FooterText = "Siotrix Footer";
                        db.Gfooters.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("gfootertext")]
        [Summary("Will set bots footer text.")]
        [Remarks("<text> - text you would like to use on embed footers. **note** using keyword reset will reset to Siotrix footer text.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task FooterTextAsync([Remainder] string txt)
        {
            CheckGuildFooters();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                if (txt.ToString().Equals("reset"))
                {
                    val.FooterText = "Siotrix Footer";
                }
                else
                {
                    val.FooterText = txt;
                }
                try
                {
                    var arr = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.FooterText = val.FooterText;
                        db.Gfooters.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("gfootertext")]
        [Summary("Will list bots current footer text.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task FooterTextAsync()
        {
            CheckGuildFooters();
            var guild_id = Context.Guild.Id;
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        txt = "Siotrix Footer";
                    }
                    else
                    {
                        txt = val.First().FooterText;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(txt);
        }

        [Command("gthumbnail")]
        [Summary("Will list bots current thumbnail image link.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildThumbNailAsync()
        {
            CheckGuildThumbNails();
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gthumbnails.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        url = val.First().ThumbNail;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("gthumbnail")]
        [Summary("Will set bots thumbnail image.")]
        [Remarks("<url> - url of picture to assign as bot thumbnail **note** using keyword reset will reset to Siotrix image.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildThumbNailAsync(Uri url)
        {
            CheckGuildThumbNails();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildThumbNail();
                if (url.ToString().Equals("reset"))
                {
                    val.ThumbNail = "http://img04.imgland.net/WyZ5FoM.png";
                }
                else
                {
                    val.ThumbNail = url.ToString();
                }
                try
                {
                    var arr = db.Gthumbnails.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gthumbnails.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.ThumbNail = val.ThumbNail;
                        db.Gthumbnails.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildThumbNails()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gthumbnails.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildThumbNail();
                        instance.GuildId = id;
                        instance.ThumbNail = "http://img04.imgland.net/WyZ5FoM.png";
                        db.Gthumbnails.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("gwebsite"), Alias("gweb")]
        [Summary("Will list bots current website.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildWebSiteAsync()
        {
            CheckGuildWebSites();
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gwebsiteurls.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        url = "https://dsj7419.github.io/Siotrix/";
                    }
                    else
                    {
                        url = val.First().SiteUrl;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("gwebsite"), Alias("gweb")]
        [Summary("Will set bots website.")]
        [Remarks("<url> - url of bots website (guild website maybe?) **note** using keyword reset will reset to Siotrix website.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildWebSiteAsync(Uri url)
        {
            CheckGuildWebSites();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildSiteUrl();
                if (url.ToString().Equals("reset"))
                {
                    val.SiteUrl = "https://dsj7419.github.io/Siotrix/";
                }
                else
                {
                    val.SiteUrl = url.ToString();
                }
                try
                {
                    var arr = db.Gwebsiteurls.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gwebsiteurls.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.SiteUrl = val.SiteUrl;
                        db.Gwebsiteurls.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildWebSites()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gwebsiteurls.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildSiteUrl();
                        instance.GuildId = id;
                        instance.SiteUrl = "https://dsj7419.github.io/Siotrix/";
                        db.Gwebsiteurls.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("gdescription"), Alias("gdesc")]
        [Summary("Will list bots current description.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildDescriptionAsync()
        {
            CheckGuildDescriptions();
            var guild_id = Context.Guild.Id;
            string desc = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gdescriptions.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        desc = "Siotrix Bot";
                    }
                    else
                    {
                        desc = val.First().Description;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(desc);
        }

        [Command("gdescription"), Alias("gdesc")]
        [Summary("Will set bots description for your guild.")]
        [Remarks("<text> - text you would like to use as a guild description.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildDescriptionAsync([Remainder] string str)
        {
            CheckGuildDescriptions();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildDescription();
                val.Description = str;
                try
                {
                    var arr = db.Gdescriptions.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gdescriptions.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.Description = val.Description;
                        db.Gdescriptions.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildDescriptions()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gdescriptions.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildDescription();
                        instance.GuildId = id;
                        instance.Description = "Siotrix Bot";
                        db.Gdescriptions.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("username")]
        [Summary("Lists Siotrix's username.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task UsernameAsync()
            => ReplyAsync(Context.Client.CurrentUser.ToString());

        [Name("no-help")]
        [Command("username")]
        [Summary("Sets Siotrix's username.")]
        [Remarks("<name> - new name to change Siotrix too, but why??.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task UsernameAsync([Remainder]string name)
        {
            var self = Context.Client.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Username = name;
            });
            await ReplyAsync("👍");
        }

        [Command("nickname")]
        [Summary("Lists Siotrix's nickname.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task NicknameAsync()
               => await ReplyAsync(Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.ToString());

        [Name("no-help")]
        [Command("nickname")]
        [Summary("Sets Siotrix's nickname.")]
        [Remarks("<name> - Set a nickname for Siotrix just for your guild. **note** reset will change it back to Siotrx.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task NicknameAsync([Remainder]string name)
        {
            var self = Context.Guild.CurrentUser;
            if (name.Equals("reset"))
            {
                name = "Siotrix";
            }
            await self.ModifyAsync(x =>
            {
                x.Nickname = name;
            });
            await ReplyAsync("👍");

        }

        [Command("activity")]
        [Summary("Lists Siotrix's current activity.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task ActivityAsync()
            => ReplyAsync($"Playing: {Context.Client.CurrentUser.Game.ToString()}");

        [Name("no-help")]
        [Command("activity")]
        [Summary("Sets Siotrix's activity.")]
        [Remarks("<activity> - Whatever activity you want to set Siotrix as playing.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ActivityAsync([Remainder]string activity)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(activity);
            await ReplyAsync("👍");
        }

        [Command("status")]
        [Summary("Lists Siotrix's current status.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task StatusAsync()
            => ReplyAsync(Context.Client.CurrentUser.Status.ToString());

        [Name("no-help")]
        [Command("status")]
        [Summary("Sets Siotrix's status.")]
        [Remarks("<status> - Sets status of Siotrix(Offline, Online, Idle, Afk, etc, etc..).")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task UsernameAsync(UserStatus status)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetStatusAsync(status);
            await ReplyAsync("👍");
        }

        [Command("color")]
        [Summary("Lists your guilds current embed color choice.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildColorAsync()
        {
            string colorName = null;
            var guild_id = Context.Guild.Id;
            CheckGuildColorGuilds();
            using (var db = new LogDatabase())
            {
                var col = (from t1 in db.Gcolors
                                   join t2 in db.Colorinfos on t1.ColorId equals t2.Id
                                   where t1.GuildId == guild_id.ToLong()
                                   select new { r = t2.RedParam, g = t2.GreenParam, b = t2.BlueParam }).First();
                if (col.r == 255 && col.g == 0 && col.b == 0)
                {
                    colorName = "Red";
                }
                else if (col.r == 255 && col.g == 0 && col.b == 127)
                {
                    colorName = "Rose";
                }
                else if (col.r == 255 && col.g == 0 && col.b == 255)
                {
                    colorName = "Magenta";
                }
                else if (col.r == 127 && col.g == 0 && col.b == 255)
                {
                    colorName = "Violet";
                }
                else if (col.r == 0 && col.g == 0 && col.b == 255)
                {
                    colorName = "Blue";
                }
                else if (col.r == 0 && col.g == 127 && col.b == 255)
                {
                    colorName = "Azure";
                }
                else if (col.r == 0 && col.g == 255 && col.b == 255)
                {
                    colorName = "Cyan";
                }
                else if (col.r == 0 && col.g == 255 && col.b == 127)
                {
                    colorName = "Aquamarine";
                }
                else if (col.r == 0 && col.g == 255 && col.b == 0)
                {
                    colorName = "Green";
                }
                else if (col.r == 127 && col.g == 255 && col.b == 0)
                {
                    colorName = "Chartreuse";
                }
                else if (col.r == 255 && col.g == 255 && col.b == 0)
                {
                    colorName = "Yellow";
                }
                else if (col.r == 255 && col.g == 127 && col.b == 0)
                {
                    colorName = "Orange";
                }
                else if (col.r == 1 && col.g == 1 && col.b == 1)
                {
                    colorName = "Black";
                }
                else if (col.r == 49 && col.g == 79 && col.b == 79)
                {
                    colorName = "Dark-Gray";
                }
                else if (col.r == 127 && col.g == 127 && col.b == 127)
                {
                    colorName = "Gray";
                }
                else if (col.r == 255 && col.g == 255 && col.b == 255)
                {
                    colorName = "None";
                }
            }
            await ReplyAsync(colorName);
        }

        public async Task GuildColorListAsync()
        {
            string colors = null;
            var guild_id = Context.Guild.Id;
            CheckGuildColorGuilds();
            using (var db = new LogDatabase())
            {
                var list = db.Colorinfos.ToList();
                var val = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong()).First();
                foreach(var col in list)
                {
                    if (col.RedParam == 255 && col.GreenParam == 0 && col.BlueParam == 0)
                    {
                        if(val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Red") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Red" + "\n";
                        }
                    }
                    else if (col.RedParam == 255 && col.GreenParam == 0 && col.BlueParam == 127)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Rose") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Rose" + "\n";
                        }
                    }
                    else if (col.RedParam == 255 && col.GreenParam == 0 && col.BlueParam == 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Magenta") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Magenta" + "\n";
                        }
                    }
                    else if (col.RedParam == 127 && col.GreenParam == 0 && col.BlueParam == 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Violet") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Violet" + "\n";
                        }
                    }
                    else if (col.RedParam == 0 && col.GreenParam == 0 && col.BlueParam== 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Blue") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Blue" + "\n";
                        }
                    }
                    else if (col.RedParam == 0 && col.GreenParam == 127 && col.BlueParam == 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Azure") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Azure" + "\n";
                        }
                    }
                    else if (col.RedParam == 0 && col.GreenParam == 255 && col.BlueParam == 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Cyan") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Cyan" + "\n";
                        }
                    }
                    else if (col.RedParam == 0 && col.GreenParam == 255 && col.BlueParam == 127)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Aquamarine") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Aquamarine" + "\n";
                        }
                    }
                    else if (col.RedParam == 0 && col.GreenParam == 255 && col.BlueParam == 0)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Green") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Green" + "\n";
                        }
                    }
                    else if (col.RedParam == 127 && col.GreenParam == 255 && col.BlueParam == 0)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Chartreuse") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Chartreuse" + "\n";
                        }
                    }
                    else if (col.RedParam == 255 && col.GreenParam == 255 && col.BlueParam == 0)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Yellow") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Yellow" + "\n";
                        }
                    }
                    else if (col.RedParam == 255 && col.GreenParam == 127 && col.BlueParam == 0)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Orange") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Orange" + "\n";
                        }
                    }
                    else if (col.RedParam == 1 && col.GreenParam == 1 && col.BlueParam == 1)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Black") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Black" + "\n";
                        }
                    }
                    else if (col.RedParam == 49 && col.GreenParam == 79 && col.BlueParam == 79)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Dark-Gray") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Dark-Gray" + "\n";
                        }
                    }
                    else if (col.RedParam == 127 && col.GreenParam == 127 && col.BlueParam == 127)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("Gray") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "Gray" + "\n";
                        }
                    }
                    else if (col.RedParam == 255 && col.GreenParam == 255 && col.BlueParam == 255)
                    {
                        if (val.ColorId == col.Id)
                        {
                            colors += Format.Bold("None") + " is color of this guild \n";
                        }
                        else
                        {
                            colors += "None" + "\n";
                        }
                    }
                }
            }
            await ReplyAsync(colors);
        }

        [Command("color")]
        [Summary("Sets guild embed color.")]
        [Remarks("<color> - Sets your guilds embet color. **note** using list instead of color will list all possibilities.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildColorAsync(string name)
        {
            int id = 0;
            switch (name)
            {
                case "list":
                    id = -1;
                    break;
                case "Red":
                    id = 1;
                    break;
                case "Rose":
                    id = 2;
                    break;
                case "Magenta":
                    id = 3;
                    break;
                case "Violet":
                    id = 4;
                    break;
                case "Blue":
                    id = 5;
                    break;
                case "Azure":
                    id = 6;
                    break;
                case "Cyan":
                    id = 7;
                    break;
                case "Aquamarine":
                    id = 8;
                    break;
                case "Green":
                    id = 9;
                    break;
                case "Chartreuse":
                    id = 10;
                    break;
                case "Yellow":
                    id = 11;
                    break;
                case "Orange":
                    id = 12;
                    break;
                case "Black":
                    id = 13;
                    break;
                case "Dark-Gray":
                    id = 14;
                    break;
                case "None":
                    id = 16;
                    break;
                default :
                    id = 15;
                    break;
            }
            if(id < 0)
            {
                await GuildColorListAsync();
            }
            else
            {
                var guild_id = Context.Guild.Id;
                CheckGuildColorGuilds();
                using (var db = new LogDatabase())
                {
                    var arr = db.Gcolors.Where(x => x.GuildId == guild_id.ToLong());
                    try
                    {
                        var data = arr.First();
                        data.ColorId = id;
                        db.Gcolors.Update(data);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                await ReplyAsync("👍");
            }
        }

        private void CheckGuildColorGuilds()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gcolors.ToList();
                foreach(var item in list)
                {
                    if(id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordColor();
                        instance.ColorId = 15;
                        instance.GuildId = id;
                        db.Gcolors.Add(instance);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("gname")]
        [Summary("Sets guild name.")]
        [Remarks("<name> - Name of guild you want to use in embeds. **note** reset will reset to your actual guild name.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildNameAsync([Remainder] string txt)
        {
            CheckGuildNames();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildName();
                if (txt.Equals("reset"))
                {
                    val.GuildName = Context.Guild.Name;
                }
                else
                {
                    val.GuildName = txt;
                }
                try
                {
                    var arr = db.Gnames.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gnames.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.GuildName = val.GuildName;
                        db.Gnames.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("gname")]
        [Summary("Lists your guilds current name thats been set for embeds.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildNameAsync()
        {
            CheckGuildNames();
            var guild_id = Context.Guild.Id;
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        txt = Context.Guild.Name;
                    }
                    else
                    {
                        txt = val.First().GuildName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(txt);
        }

        private void CheckGuildNames()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gnames.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildName();
                        instance.GuildId = id;
                        instance.GuildName = Context.Guild.Name;
                        db.Gnames.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("gavatar")]
        [Summary("Will list bots current guild avatar for embeds.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildAvatarAsync()
        {
            CheckGuildAvatars();
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        url = "http://www.clipartkid.com/images/47/clipart-information-image-information-gif-anim-information-2noIRl-clipart.png";
                    }
                    else
                    {
                        url = val.First().Avatar;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("gavatar")]
        [Summary("Will set bots guild avatar for embeds.")]
        [Remarks("<url> - url of picture to assign as bot avatar **note** using keyword reset will reset to Siotrix embed avatar.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildAvatarAsync(Uri url)
        {
            CheckGuildAvatars();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildAvatar();
                if (url.ToString().Equals("reset"))
                {
                    val.Avatar = "http://www.clipartkid.com/images/47/clipart-information-image-information-gif-anim-information-2noIRl-clipart.png";
                }
                else
                {
                    val.Avatar = url.ToString();
                }
                var arr = db.Gavatars.Where(p => p.GuildId == guild_id.ToLong());
                try
                {
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gavatars.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.Avatar = val.Avatar;
                        db.Gavatars.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildAvatars()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gavatars.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildAvatar();
                        instance.GuildId = id;
                        instance.Avatar = "http://www.clipartkid.com/images/47/clipart-information-image-information-gif-anim-information-2noIRl-clipart.png";
                        db.Gavatars.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        [Command("prefix")]
        [Summary("Will set bot prefix for your guild.")]
        [Remarks("<prefix> - Any prefix you'd like, up to 10 characters for your guild. **note** reset will change prefix to !.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task PrefixAsync([Remainder] string txt)
        {
           // string str = CheckEmojiText(txt);
            CheckPrefixs();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildPrefix();
                if (txt.Equals("reset"))
                {
                    val.Prefix = "!";
                }
                else
                {
                    val.Prefix = txt;
                }
                try
                {
                    var arr = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gprefixs.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.Prefix = val.Prefix;
                        db.Gprefixs.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        [Command("prefix")]
        [Summary("Will list bots current guild prefix.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task PrefixAsync()
        {
            CheckPrefixs();
            var guild_id = Context.Guild.Id;
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gprefixs.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        txt = "!";
                    }
                    else
                    {
                        txt = val.First().Prefix;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(txt);
        }

        private bool CheckString(string str)
        {
            bool isFounded = false;
            string[] spec = new string[] {"!", "~", "#", "$", "%", "&", "*", ":", "/", "|", "+", "=", "-", "_", ">", "<", "reset"};
            foreach(var element in spec)
            {
                if (str.Equals(element))
                {
                    isFounded = true;
                    break;
                }
            }
            return isFounded;
        }

        private void CheckPrefixs()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gprefixs.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildPrefix();
                        instance.GuildId = id;
                        instance.Prefix = "!";
                        db.Gprefixs.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public static string FromText(string text)
        {
            text = text.Trim(':');

            var unicode = default(string);
            if (EmojiMap.Map.TryGetValue(text, out unicode))
                return unicode;
            throw new ArgumentException("The given alias could not be matched to a Unicode Emoji.", nameof(text));
        }

        public string CheckEmojiText(string str)
        {
            string new_str = str.Trim(':');
            if (new_str.Equals(str))
            {
                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>{0}", new_str);
                return new_str;
            }
            else
            {
                
                var unicode = default(string);
                if (EmojiMap.Map.TryGetValue(new_str, out unicode))
                {
                    Console.WriteLine("==========================={0}", unicode);
                }
                return unicode;
            }
        }

        [Command("gmotd")]
        [Summary("Lists guild current motd.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildMotdAsync()
        {
            CheckGuildMotds();
            var guild_id = Context.Guild.Id;
            string str = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gmotds.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0)
                    {
                        str = "Welcome to .";
                    }
                    else
                    {
                        str = val.First().Message;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(str);
        }

        [Command("gmotd")]
        [Summary("Sets guild motd.")]
        [Remarks("<motd> - Motd text for guild. **note** reset will revert back to default motd.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildMotdAsync([Remainder] string str)
        {
            CheckGuildMotds();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildMotd();
                if (str.Equals("reset"))
                {
                    val.Message = "Welcome to .";
                }
                else
                {
                    val.Message = str;
                }
                var arr = db.Gmotds.Where(p => p.GuildId == guild_id.ToLong());
                try
                {
                    if (arr == null || arr.ToList().Count <= 0)
                    {
                        db.Gmotds.Add(val);
                    }
                    else
                    {
                        var data = arr.First();
                        data.Message = val.Message;
                        db.Gmotds.Update(data);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync("👍");
        }

        private void CheckGuildMotds()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Gmotds.ToList();
                foreach (var item in list)
                {
                    if (id == item.GuildId)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (!isFounded)
                {
                    try
                    {
                        var instance = new DiscordGuildMotd();
                        instance.GuildId = id;
                        instance.Message = "Welcome to .";
                        db.Gmotds.Add(instance);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}

using Discord;
using Siotrix;
using Siotrix.Commands;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Group("settings"), Alias("set")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        [Command("gavatar")]
        public Task AvatarAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.GetAvatarUrl());

        [Name("no-help")]
        [Command("gavatar"), RequireOwner]
        public async Task AvatarAsync(Uri url)
        {
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
                await Context.ReplyAsync("👍");
            }
        }

        [Name("no-help")]
        [Command("gavatar reset"), RequireOwner]
        public async Task AvatarResetAsync()
        {
            Uri url = new Uri("https://s27.postimg.org/hgn3yw4gz/Siotrix_Logo_Side_Alt1_No_Text.png");
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
                await Context.ReplyAsync("👍");
            }
        }

        [Command("gfootericon"), RequireOwner]
        public async Task FooterIconAsync()
        {
            string url = null;
            using(var db = new LogDatabase())
            {
                try
                {
                    if (db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        url = "No url";
                    }
                    else
                    {
                        url = db.Gfooters.First().FooterIcon;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync(url);
        }

        [Command("gfootericon reset"),RequireOwner]
        public async Task FooterIconResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                val.FooterIcon = "http://img04.imgland.net/WyZ5FoM.png";
                try
                {
                    if (db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Gfooters.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gfootericon"), RequireOwner]
        public async Task FooterIconAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                val.FooterIcon = url.ToString();
                try
                {
                    if(db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Gfooters.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gfootertext"), RequireOwner]
        public async Task FooterTextAsync([Remainder] string txt)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                val.FooterText = txt;
                try
                {
                    if (db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Gfooters.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gfootertext"), RequireOwner]
        public async Task FooterTextAsync()
        {
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        txt = "Siotrix Footer";
                    }
                    else
                    {
                        txt = db.Gfooters.First().FooterText;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync(txt);
        }

        [Command("gfootertext reset"), RequireOwner]
        public async Task FooterTextResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildFooter();
                val.FooterText = "Siotrix Footer";
                try
                {
                    if (db.Gfooters == null || db.Gfooters.ToList().Count <= 0)
                    {
                        db.Gfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Gfooters.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthoricon"), RequireOwner]
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
                        if(url == null || url == "")
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
            await Context.ReplyAsync(url);
        }

        [Command("gauthoricon reset"), RequireOwner]
        public async Task AuthorIconResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorIcon = "http://img04.imgland.net/WyZ5FoM.png";
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthoricon"), RequireOwner]
        public async Task AuthorIconAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorIcon = url.ToString();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthorurl"), RequireOwner]
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
                        if (url == null || url == "")
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
            await Context.ReplyAsync(url);
        }

        [Command("gauthorurl reset"), RequireOwner]
        public async Task AuthorUrlResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorUrl = "";
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthorurl"), RequireOwner]
        public async Task AuthorUrlAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorUrl = url.ToString();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthorname"), RequireOwner]
        public async Task AuthorNameAsync([Remainder] string txt)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorName = txt;
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
            await Context.ReplyAsync("👍");
        }

        [Command("gauthorname"), RequireOwner]
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
                        if(txt == null || txt == "")
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
            await Context.ReplyAsync(txt);
        }

        [Command("gauthorname reset"), RequireOwner]
        public async Task AuthorNameResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordAuthor();
                val.AuthorName = Context.Guild.Name;
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
            await Context.ReplyAsync("👍");
        }

        [Command("gthumbnail"), RequireOwner]
        public async Task ThumbNailAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Gthumbnails == null || db.Gthumbnails.ToList().Count <= 0)
                    {
                        url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        url = db.Gthumbnails.First().ThumbNail;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync(url);
        }

        [Command("gthumbnail reset"), RequireOwner]
        public async Task ThumbNailResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordThumbNail();
                val.ThumbNail = "http://img04.imgland.net/WyZ5FoM.png";
                try
                {
                    if (db.Gthumbnails == null || db.Gthumbnails.ToList().Count <= 0)
                    {
                        db.Gthumbnails.Add(val);
                    }
                    else
                    {
                        var data = db.Gthumbnails.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gthumbnail"), RequireOwner]
        public async Task ThumbNailAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordThumbNail();
                val.ThumbNail = url.ToString();
                try
                {
                    if (db.Gthumbnails == null || db.Gthumbnails.ToList().Count <= 0)
                    {
                        db.Gthumbnails.Add(val);
                    }
                    else
                    {
                        var data = db.Gthumbnails.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gwebsite"), Alias("gweb"), RequireOwner]
        public async Task WebSiteAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Gwebsiteurls == null || db.Gwebsiteurls.ToList().Count <= 0)
                    {
                        url = "https://dsj7419.github.io/Siotrix/";
                    }
                    else
                    {
                        url = db.Gwebsiteurls.First().SiteUrl;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync(url);
        }

        [Command("gwebsite reset"), Alias("gweb"), RequireOwner]
        public async Task WebSiteResetAsync()
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildSiteUrl();
                val.SiteUrl = "https://dsj7419.github.io/Siotrix/";
                try
                {
                    if (db.Gwebsiteurls == null || db.Gwebsiteurls.ToList().Count <= 0)
                    {
                        db.Gwebsiteurls.Add(val);
                    }
                    else
                    {
                        var data = db.Gwebsiteurls.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gwebsite"), Alias("gweb"), RequireOwner]
        public async Task WebSiteAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildSiteUrl();
                val.SiteUrl = url.ToString();
                try
                {
                    if (db.Gwebsiteurls == null || db.Gwebsiteurls.ToList().Count <= 0)
                    {
                        db.Gwebsiteurls.Add(val);
                    }
                    else
                    {
                        var data = db.Gwebsiteurls.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("gdescription"), Alias("gdesc"), RequireOwner]
        public async Task DescriptionAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Gdescriptions == null || db.Gdescriptions.ToList().Count <= 0)
                    {
                        url = "Siotrix Bot";
                    }
                    else
                    {
                        url = db.Gdescriptions.First().Description;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync(url);
        }

        [Command("gdescription"), Alias("gdesc"), RequireOwner]
        public async Task DescriptionAsync([Remainder] string str)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordGuildDescription();
                val.Description = str;
                try
                {
                    if (db.Gdescriptions == null || db.Gdescriptions.ToList().Count <= 0)
                    {
                        db.Gdescriptions.Add(val);
                    }
                    else
                    {
                        var data = db.Gdescriptions.First();
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
            await Context.ReplyAsync("👍");
        }

        [Command("username")]
        public Task UsernameAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.ToString());

        [Name("no-help")]
        [Command("username"), RequireOwner]
        public async Task UsernameAsync([Remainder]string name)
        {
            var self = Context.Client.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Username = name;
            });
            await Context.ReplyAsync("👍");
        }

        [Command("nickname")]
        public Task NicknameAsync()
            => Context.ReplyAsync(Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.ToString());

        [Name("no-help")]
        [Command("nickname"), RequireOwner]
        public async Task NicknameAsync([Remainder]string name)
        {
            var self = Context.Guild.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Nickname = name;
            });
            await Context.ReplyAsync("👍");
        }

        [Name("no-help")]
        [Command("nickname reset"), RequireOwner]
        public async Task NicknameResetAsync()
        {
            var self = Context.Guild.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Nickname = "Siotrix";
            });
            await Context.ReplyAsync("👍");
        }

        [Command("activity")]
        public Task ActivityAsync()
            => Context.ReplyAsync($"Playing: {Context.Client.CurrentUser.Game.ToString()}");

        [Name("no-help")]
        [Command("activity"), RequireOwner]
        public async Task ActivityAsync([Remainder]string activity)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetGameAsync(activity);
            await Context.ReplyAsync("👍");
        }

        [Command("status")]
        public Task StatusAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.Status.ToString());

        [Name("no-help")]
        [Command("status"), RequireOwner]
        public async Task UsernameAsync(UserStatus status)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetStatusAsync(status);
            await Context.ReplyAsync("👍");
        }

       [Command("color")]
        public async Task ColorAsync()
        {
            string colorName = null;
            var guild_id = Context.Guild.Id;
            CheckColorGuilds();
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
            await Context.ReplyAsync(colorName);
        }

        [Name("no-help")]
        [Command("color list")]
        public async Task ColorListAsync()
        {
            string colors = null;
            var guild_id = Context.Guild.Id;
            CheckColorGuilds();
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
            await Context.ReplyAsync(colors);
        }

        [Name("no-help")]
        [Command("color")]
        public async Task ColorAsync(string name)
        {
            int id = 0;
            switch (name)
            {
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
            var guild_id = Context.Guild.Id;
            CheckColorGuilds();
            using (var db = new LogDatabase())
            {
                var arr = db.Gcolors.Where(x => x.GuildId == guild_id.ToLong()).First();
                arr.ColorId = id;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync("👍");
        }

        private void CheckColorGuilds()
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

        [Command("gname"), RequireOwner]
        public async Task GuildNameAsync([Remainder] string txt)
        {
            CheckGuildNames();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var arr = db.Gnames.Where(x => x.GuildId == guild_id.ToLong()).First();
                arr.GuildName = txt;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync("👍");
        }

        [Command("gname"), RequireOwner]
        public async Task GuildNameAsync()
        {
            CheckGuildNames();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong()).First();
                await Context.ReplyAsync(val.GuildName);
            }
        }

        [Command("gname reset"), RequireOwner]
        public async Task GuildNameResetAsync()
        {
            CheckGuildNames();
            var guild_id = Context.Guild.Id;
            using (var db = new LogDatabase())
            {
                var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong()).First();
                try
                {
                    val.GuildName = Context.Guild.Name;
                    db.Gnames.Update(val);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await Context.ReplyAsync("👍");
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
    }
}

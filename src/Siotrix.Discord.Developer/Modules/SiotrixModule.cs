﻿using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Summary("Various Siotrix property Settings.")]
    [Group("siotrix"), Alias("sio")]
    public class SiotrixModule : ModuleBase<SocketCommandContext>
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

        [Command("info")]
        [Summary("Will list Siotrix current info description.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotInfoAsync()
        {
            string str = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Binfos == null || db.Binfos.ToList().Count <= 0)
                    {
                        str = "Siotrix Bot";
                    }
                    else
                    {
                        str = db.Binfos.First().BotInfo;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(str);
        }

        [Command("info")]
        [Summary("Will set Siotrix info description.")]
        [Remarks("<text> - Add text for Siotrix information.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotInfoAsync([Remainder] string str)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordSiotrixInfo();
                val.BotInfo = str;
                try
                {
                    if (db.Binfos == null || db.Binfos.ToList().Count <= 0)
                    {
                        db.Binfos.Add(val);
                    }
                    else
                    {
                        var data = db.Binfos.First();
                        data.BotInfo = val.BotInfo;
                        db.Binfos.Update(data);
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

        [Command("website")]
        [Summary("Will list Siotrix current website.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task WebSiteAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Bwebsiteurls == null || db.Bwebsiteurls.ToList().Count <= 0)
                    {
                        url = "https://dsj7419.github.io/Siotrix/";
                    }
                    else
                    {
                        url = db.Bwebsiteurls.First().SiteUrl;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("website")]
        [Summary("Will set Siotrix website.")]
        [Remarks("<url> - Update main website URL for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task WebSiteAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordSiotrixSiteUrl();
                val.SiteUrl = url.ToString();
                try
                {
                    if (db.Bwebsiteurls == null || db.Bwebsiteurls.ToList().Count <= 0)
                    {
                        db.Bwebsiteurls.Add(val);
                    }
                    else
                    {
                        var data = db.Bwebsiteurls.First();
                        data.SiteUrl = val.SiteUrl;
                        db.Bwebsiteurls.Update(data);
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

        [Command("footericon")]
        [Summary("Will list Siotrix current footer icon.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterIconAsync()
        {
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Bfooters == null || db.Bfooters.ToList().Count <= 0)
                    {
                        url = "http://img04.imgland.net/WyZ5FoM.png";
                    }
                    else
                    {
                        url = db.Bfooters.First().FooterIcon;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(url);
        }

        [Command("footericon")]
        [Summary("Will set Siotrix footer icon.")]
        [Remarks("<url> - Update main footer icon for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterIconAsync(Uri url)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordSiotrixFooter();
                val.FooterIcon = url.ToString();
                try
                {
                    if (db.Bfooters == null || db.Bfooters.ToList().Count <= 0)
                    {
                        db.Bfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Bfooters.First();
                        data.FooterIcon = val.FooterIcon;
                        db.Bfooters.Update(data);
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

        [Command("footertext")]
        [Summary("Will set Siotrix footer text.")]
        [Remarks("<text> - Update main footer text for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterTextAsync([Remainder] string txt)
        {
            using (var db = new LogDatabase())
            {
                var val = new DiscordSiotrixFooter();
                val.FooterText = txt;
                try
                {
                    if (db.Bfooters == null || db.Bfooters.ToList().Count <= 0)
                    {
                        db.Bfooters.Add(val);
                    }
                    else
                    {
                        var data = db.Bfooters.First();
                        data.FooterText = val.FooterText;
                        db.Bfooters.Update(data);
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

        [Command("footertext")]
        [Summary("Will list Siotrix current footer text.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterTextAsync()
        {
            string txt = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Bfooters == null || db.Bfooters.ToList().Count <= 0)
                    {
                        txt = "Siotrix Footer";
                    }
                    else
                    {
                        txt = db.Bfooters.First().FooterText;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await ReplyAsync(txt);
        }

        [Command("username")]
        [Summary("Lists Siotrix's username.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task UsernameAsync()
            => ReplyAsync(Context.Client.CurrentUser.ToString());

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

        [Command("activity")]
        [Summary("Lists Siotrix's current activity.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task ActivityAsync()
            => ReplyAsync($"Playing: {Context.Client.CurrentUser.Game.ToString()}");

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
    }
}

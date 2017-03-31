using Discord;
using Discord.WebSocket;
using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Group("siotrix"), Alias("sio")]
    public class SiotrixModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
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
            await Context.ReplyAsync(str);
        }

        [Command("info")]
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
            await Context.ReplyAsync("👍");
        }

        [Command("website")]
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
            await Context.ReplyAsync(url);
        }

        [Command("website")]
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
            await Context.ReplyAsync("👍");
        }

        [Command("footericon")]
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
            await Context.ReplyAsync(url);
        }

        [Command("footericon")]
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
            await Context.ReplyAsync("👍");
        }

        [Command("footertext")]
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
            await Context.ReplyAsync("👍");
        }

        [Command("footertext")]
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
            await Context.ReplyAsync(txt);
        }
    }
}

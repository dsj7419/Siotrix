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
            CheckGuilds();
            using (var db = new LogDatabase())
            {
                var col = (from t1 in db.Infos
                                   join t2 in db.Colors on t1.ColorId equals t2.Id
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
            CheckGuilds();
            using (var db = new LogDatabase())
            {
                var list = db.Colors.ToList();
                var val = db.Infos.Where(p => p.GuildId == guild_id.ToLong()).First();
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
            CheckGuilds();
            using (var db = new LogDatabase())
            {
                var arr = db.Infos.Where(x => x.GuildId == guild_id.ToLong()).First();
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

        private void CheckGuilds()
        {
            var id = Context.Guild.Id.ToLong();
            bool isFounded = false;
            using (var db = new LogDatabase())
            {
                var list = db.Infos.ToList();
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
                        var instance = new DiscordInfo();
                        instance.ColorId = 15;
                        instance.GuildId = id;
                        db.Infos.Add(instance);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}

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

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]    
    [Group("settings"), Alias("set")]
    [Summary("Various settings for guild to customize Siotrix with.")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        private InteractiveService Interactive;

        public SettingsModule(InteractiveService Inter)
        {
            Interactive = Inter;
        }

        private Stopwatch _timer = new Stopwatch();

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

        [Command("nickname")]
        [Summary("Lists Siotrix's nickname.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task NicknameAsync()
               => await ReplyAsync(Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.ToString());

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
       
        [Command("color")]
        [Summary("Set or list your guilds official embed color.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task GuildColorAsync()
        {
            var regexColorCode = new Regex("^#[a-fA-F0-9]{6}$");
            var regexRGBCode = new Regex("^\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*$");
            Color currentGColor = GuildEmbedColorExtensions.GetGuildColor(Context);
            string currentStringColor = currentGColor.ToString();
            string currentHexColor = currentStringColor.Replace("0x", "#").ToUpper();
            var guild_id = Context.Guild.Id;

            await ReplyAsync($"Give me any value of color (Hex, RGB, or a name) to set your guild color.\nYour Current Guild Hex Code is:{Format.Bold(currentGColor.ToString())} {Format.Bold(currentStringColor)} {Format.Bold(currentHexColor)}. Type list for a breakdown of your current color.");
            var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
            if (response.Content == "cancel") return;

            _timer.Start();

            if (response.Content == "list")
            {
                var colornamelower = HexColorDict.ColorName(currentStringColor); //look up hex in dictionary

                Console.WriteLine($"1. {colornamelower}");

                if (colornamelower == null)
                    colornamelower = "no name found";

                Console.WriteLine($"2. {colornamelower}");

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colornamelower);

                Console.WriteLine($"3. {colornamelower}");

                HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(currentHexColor); // convert hex to an RGB value
                var red = Convert.ToString(rgbvalue.R); // Red Property
                var green = Convert.ToString(rgbvalue.G); // Green property
                var blue = Convert.ToString(rgbvalue.B); // Blue property  

                if (colorname == "Black")
                {
                    colorname = "Black (For Discord)";
                    currentHexColor = "#010101";
                    rgbvalue.R = 1;
                    rgbvalue.G = 1;
                    rgbvalue.B = 1;
                }

                Console.WriteLine($" {currentHexColor} {colorname} {currentHexColor}");

                var embed = GetEmbed(currentHexColor, colorname, currentHexColor, rgbvalue);
                await ReplyAsync("", embed: embed);

                return;
            }

            string colorchoice = response.Content.ToLower();

            if (regexColorCode.IsMatch(colorchoice.Trim()))
            {
                string cleanHex = colorchoice.Replace("#", "0x").ToLower(); //strip # and add 0x for dictionary search
                var colornamelower = HexColorDict.ColorName(cleanHex); //look up hex in dictionary

                if (colornamelower == null)
                    colornamelower = "no name found";

                using (var db = new LogDatabase())
                {
                    var guildColor = new DiscordColor();
                    guildColor.ColorHex = cleanHex;
                    try
                    {
                        var arr = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong());
                        if (arr == null || arr.ToList().Count <= 0)
                        {
                            db.Gcolors.Add(guildColor);
                        }
                        else
                        {
                            var data = arr.First();
                            data.ColorHex = cleanHex;
                            db.Gcolors.Update(data);
                        }
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colornamelower);

                var hexcolor = colorchoice.ToUpper();

                HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(colorchoice); // convert hex to an RGB value
                var red = Convert.ToString(rgbvalue.R); // Red Property
                var green = Convert.ToString(rgbvalue.G); // Green property
                var blue = Convert.ToString(rgbvalue.B); // Blue property  

                if (colorname == "Black")
                {
                    colorname = "Black (For Discord)";
                    hexcolor = "#010101";
                    rgbvalue.R = 1;
                    rgbvalue.G = 1;
                    rgbvalue.B = 1;
                }

                var embed = GetEmbed(hexcolor, colorname, hexcolor, rgbvalue);
                await ReplyAsync("", embed: embed);
            }
            else if (HexColorDict.colorHex.ContainsKey(colorchoice))
            {
                var colorhex = HexColorDict.ColorHex(colorchoice); // look up hex in color name Dictionary

                using (var db = new LogDatabase())
                {
                    var guildColor = new DiscordColor();
                    guildColor.ColorHex = colorhex;
                    try
                    {
                        var arr = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong());
                        if (arr == null || arr.ToList().Count <= 0)
                        {
                            db.Gcolors.Add(guildColor);
                        }
                        else
                        {
                            var data = arr.First();
                            data.ColorHex = colorhex;
                            db.Gcolors.Update(data);
                        }
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                string cleanhex = colorhex.Replace("0x", "#").ToUpper(); // convert the hex back to #FFFFFF format

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colorchoice);

                var rgbvalue = HextoRGB.HexadecimalToRGB(cleanhex); // convert hex to RGB value

                if (colorname == "Black")
                {
                    colorname = "Black (For Discord)";
                    cleanhex = "#010101";
                    rgbvalue.R = 1;
                    rgbvalue.G = 1;
                    rgbvalue.B = 1;
                }

                var embed = GetEmbed(colorname, colorname, cleanhex, rgbvalue);
                await ReplyAsync("", embed: embed);
            }
            else if (regexRGBCode.IsMatch(colorchoice))
            {
                string[] str = colorchoice.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries); // Split up string of numbers into Red/Green/Blue
                int red = int.Parse(str[0]); //convert to red int
                int green = int.Parse(str[1]); //convert to green int
                int blue = int.Parse(str[2]); // convery to blue int
                RGBtoHex.RGB data = new RGBtoHex.RGB((byte)red, (byte)green, (byte)blue); // convert broken out ints into a data struct - prep for conversion

                var colorhex = RGBtoHex.RGBToHexadecimal(data); // convert RGB input into hex           

                if (!regexColorCode.IsMatch(colorhex))
                {
                    var colorname = "No Name Found";
                    var hexcolorcaps = "Hex Not Available";
                    byte r = (byte)red;
                    byte g = (byte)green;
                    byte b = (byte)blue;
                    var rgbvalue = new HextoRGB.RGB(r, g, b);
                    var embed = GetEmbed(colorchoice, colorname, hexcolorcaps, rgbvalue);
                    await ReplyAsync("", embed: embed);
                }
                else
                {

                    string cleanHex = colorhex.Replace("#", "0x"); // replace # with 0x for dictionary lookup
                    var hexcolorcaps = colorhex.ToUpper();

                    var cleanHexLower = cleanHex.ToLower();
                    var colornamelower = HexColorDict.ColorName(cleanHexLower); // get color name

                    using (var db = new LogDatabase())
                    {
                        var guildColor = new DiscordColor();
                        guildColor.ColorHex = cleanHexLower;
                        try
                        {
                            var arr = db.Gcolors.Where(p => p.GuildId == guild_id.ToLong());
                            if (arr == null || arr.ToList().Count <= 0)
                            {
                                db.Gcolors.Add(guildColor);
                            }
                            else
                            {
                                var cdata = arr.First();
                                cdata.ColorHex = cleanHexLower;
                                db.Gcolors.Update(cdata);
                            }
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }


                    if (colornamelower == null)
                        colornamelower = "no name found";

                    TextInfo text = new CultureInfo("en-US").TextInfo;
                    var colorname = text.ToTitleCase(colornamelower);

                    HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(colorhex);

                    if (colorname == "Black")
                    {
                        colorname = "Black (For Discord)";
                        hexcolorcaps = "#010101";
                        rgbvalue.R = 1;
                        rgbvalue.G = 1;
                        rgbvalue.B = 1;
                    }

                    var embed = GetEmbed(colorchoice, colorname, hexcolorcaps, rgbvalue);
                    await ReplyAsync("", embed: embed);
                }
            }
            else
            {
                await ReplyAsync("You did not input a correct hex, color name, or RGB value.");
            }
        }

            private EmbedBuilder GetEmbed(string colorchoice, string colorname, string colorhex, HextoRGB.RGB rgbvalue)
        {

            _timer.Stop();
            string g_icon_url = GuildEmbedIconUrl.GetGuildIconUrl(Context);
            string g_name = GuildEmbedName.GetGuildName(Context);
            string g_url = GuildEmbedUrl.GetGuildUrl(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GuildEmbedFooter.GetGuildFooter(Context);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            var red = Convert.ToString(rgbvalue.R); // Red Property
            var green = Convert.ToString(rgbvalue.G); // Green property
            var blue = Convert.ToString(rgbvalue.B); // Blue property
            Color color = new Color(rgbvalue.R, rgbvalue.G, rgbvalue.B);

            if (colorname == null)
                colorname = "No Name Found";

            if (colorchoice == null)
                colorchoice = "Colorchoice is empty";

            if (colorhex == null)
                colorhex = "No hex equivalent found.";

            var builder = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            builder.Title = $"Color Embed Information for: {colorchoice}";
            builder.Color = color;
            builder.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Color Name: ";
                x.Value = Format.Bold(colorname);
            });

            builder.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Hex Color Code: ";
                x.Value = Format.Bold(colorhex);
            });

            builder.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "RGB Value: ";
                x.Value = $"RED:{Format.Bold(red)} GREEN:{Format.Bold(green)} BLUE:{Format.Bold(blue)}";
            });
            builder.WithDescription($"In {_timer.ElapsedMilliseconds}ms");

            return builder;
        }

        /*
        //TODO: Color needs rework to include custom colors - also can do pre-set colors to static out of this module.

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
        } */

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
            //TODO: Prefix needs more work
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

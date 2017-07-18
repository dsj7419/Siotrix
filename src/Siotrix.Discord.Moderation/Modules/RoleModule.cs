using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using System.Runtime.CompilerServices;
using static Siotrix.Discord.RoleExtensions;
using System.Globalization;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("role")]
    [Summary("Various role commands.")]
    public class RoleModule : InteractiveModuleBase<SocketCommandContext>
    {

        private InteractiveService Interactive;

        public RoleModule(InteractiveService Inter)
        {
            Interactive = Inter;
        }


        [Command("giverole")]
        [Summary("Give a role to a user.")]
        [Remarks(" <username> <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task GiveRoleAsync(SocketGuildUser user, string rolename)
        {
            IRole role = user.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            if(role == null)
            {
                await ReplyAsync("Please enter a valid role name.");
                return;
            }
            await GiveRole(user, role);
            await ReplyAsync(String.Format("Successfully gave the following role to `{0}`: `{1}`.", user.FormatUser(), role), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("takerole")]
        [Summary("Take a role to a user.")]
        [Remarks(" <username> <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task TakeRoleAsync(SocketGuildUser user, string rolename)
        {
            IRole role = user.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            await TakeRole(user, role);
            await ReplyAsync(String.Format("Successfully took the following role from `{0}`: `{1}`.", user.FormatUser(), role), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("createrole")]
        [Summary("Create a new role.")]
        [Remarks(" <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task CreateNewRoleAsync(string rolename)
        {
            var guild = Context.Guild as IGuild;
            await guild.CreateRoleAsync(rolename, GuildPermissions.None).ConfigureAwait(false);
            await ReplyAsync(String.Format("Successfully created the role `{0}`. No permissions have been set.", rolename), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("softdeleterole")]
        [Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
        [Remarks(" <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task SoftDeleteRoleAsync(string rolename)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            var guild = Context.Guild as IGuild;
            var name = role.Name;
            var color = role.Color;
            var position = role.Position;

            await role.DeleteAsync();
            var newRole = await guild.CreateRoleAsync(name, GuildPermissions.None, color).ConfigureAwait(false);
            await ModifyRolePosition(newRole, position);
            await ReplyAsync(String.Format("Successfully removed all permissions from the role `{0}` and removed the role from all users on the guild.", role.Name), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("deleterole")]
        [Summary("Permenently deletes role.")]
        [Remarks(" <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task DeleteRoleAsync(string rolename)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            await role.DeleteAsync().ConfigureAwait(false);
            await ReplyAsync(String.Format("Successfully deleted `{0}`.", role.FormatRole()), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("changeroleposition"), Alias("crp")]
        [Summary("If only a role is input its position will be listed, else moves the role to the given position. " + SiotrixConstants.FAKE_EVERYONE + " is the first position and starts at zero.")]
        [Remarks(" <rolename> [position] - position is optional")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task ChangeRolePositionAsync(string rolename, int position = -1)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            if (position == -1)
            {
                await ReplyAsync($"{role.Name.ToUpper()}'s current position on the roles list is {role.Position}.");
            }
            else
            {                
                var newPos = await ModifyRolePosition(role, position);
                if (newPos != -1)
                {
                    await ReplyAsync(String.Format("Successfully gave `{0}` the position `{1}`.", role.FormatRole(), newPos), deleteAfter: SiotrixConstants.WAIT_TIME);
                }
                else
                {
                    await ReplyAsync(String.Format("Failed to give `{0}` the position `{1}`.", role.FormatRole(), position), deleteAfter: SiotrixConstants.WAIT_TIME);
                }
            }
        }

        [Command("displayrolepositions"), Alias("drp")]
        [Summary("Lists the positions of each role on the guild.")]
        [Remarks(" - No additional argument needed.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task DisplayRolePositionsAsync()
        {
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);

            var desc = String.Join("\n", Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x =>
            {
                if (x.Id == Context.Guild.EveryoneRole.Id)
                {
                    return String.Format("`{0}.` {1}", x.Position.ToString("00"), SiotrixConstants.FAKE_EVERYONE);
                }
                else
                {
                    return String.Format("`{0}.` {1}", x.Position.ToString("00"), x.Name);
                }
            }));


            var eb = new EmbedBuilder()
            {
                Color = g_color,
                Title = $"Roles in {Context.Guild.Name}",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Description = desc
            };        

            await Context.Channel.SendMessageAsync("", false, eb);           
        }

        [Command("changerolename")]
        [Summary("Changes the name of a given role to a new name.")]
        [Remarks(" <rolename> <new_rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task ChangeRoleNameAsync(string rolename, [Remainder]string newname)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            await role.ModifyAsync(x => x.Name = newname).ConfigureAwait(false);
            await ReplyAsync(String.Format("Successfully changed the name of `{0}` to `{1}`.", role.FormatRole(), newname), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("changerolecolor")]
        [Summary("Changes color of role to inputted color.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task ChangeRoleColorAsync(string rolename)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            Color currentColor = role.Color;
            var regexColorCode = new Regex("^#[A-Fa-f0-9]{6}$");
            var regexRGBCode = new Regex("^\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*$");
            string currentHexColor = currentColor.ToString();

            if (currentHexColor.Length != 7)
            {
                currentHexColor = currentHexColor.Substring(1);
                currentHexColor = "#" + currentHexColor.PadLeft(6, '0');
            }

            await ReplyAsync($"Give me any value of color (Hex, RGB, or a name) to set the color for the role: {role.Name} (Example Hex: #FF43A4).\nYour Current Hex Code is: {Format.Bold(currentHexColor)}. Type list for a breakdown of your current color.");
            var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));

            if (response.Content == "cancel")
            {
                await ReplyAsync("I have cancelled your request. You will keep your current color.");
                return;
            }

            if (response.Content == "list")
            {
                string cleanHex = currentHexColor.Replace("#", "0x").ToLower(); //strip # and add 0x for dictionary search
                var colornamelower = HexColorDict.ColorName(cleanHex); //look up hex in dictionary


                if (colornamelower == null)
                    colornamelower = "no name found";

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colornamelower);

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

                var hexcolor = currentHexColor.ToUpper();

                var embed = GetEmbed(role, hexcolor, colorname, hexcolor, rgbvalue);
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

                await role.ModifyAsync(x => x.Color = new Color(Convert.ToUInt32(cleanHex, 16)));

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

                var embed = GetEmbed(role, hexcolor, colorname, hexcolor, rgbvalue);
                await ReplyAsync("", embed: embed);
            }
            else if (HexColorDict.colorHex.ContainsKey(colorchoice))
            {
                var colorhex = HexColorDict.ColorHex(colorchoice); // look up hex in color name Dictionary

                await role.ModifyAsync(x => x.Color = new Color(Convert.ToUInt32(colorhex, 16)));

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

                var embed = GetEmbed(role, colorname, colorname, cleanhex, rgbvalue);
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
                    var embed = GetEmbed(role, colorchoice, colorname, hexcolorcaps, rgbvalue);
                    await ReplyAsync("", embed: embed);
                }
                else
                {

                    string cleanHex = colorhex.Replace("#", "0x"); // replace # with 0x for dictionary lookup
                    var hexcolorcaps = colorhex.ToUpper();

                    var cleanHexLower = cleanHex.ToLower();
                    var colornamelower = HexColorDict.ColorName(cleanHexLower); // get color name

                    await role.ModifyAsync(x => x.Color = new Color(Convert.ToUInt32(cleanHexLower, 16)));

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

                    var embed = GetEmbed(role, colorchoice, colorname, hexcolorcaps, rgbvalue);
                    await ReplyAsync("", embed: embed);
                }
            }
            else
            {
                await ReplyAsync("You did not input a correct hex, color name, or RGB value.");
            }
        }

        [Command("changerolehoist")]
        [Summary("Displays a role separately from others on the user list. (toggles on and off)")]
        [Remarks(" <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task ChangeRoleHoistAsync(string rolename)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            await role.ModifyAsync(x => x.Hoist = !role.IsHoisted).ConfigureAwait(false);
            await ReplyAsync(String.Format("Successfully {0} `{1}`.", (role.IsHoisted ? "de-hoisted" : "hoisted"), role.FormatRole()), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        [Command("changerolemention")]
        [Summary("Allows the role to be mentioned/unmetioned (toggles on and off)")]
        [Remarks(" <rolename>")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task ChangeRoleMentionAsync(string rolename)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable).ConfigureAwait(false);
            await ReplyAsync(String.Format("Successfully made `{0}` {1}.", role.FormatRole(), (role.IsMentionable ? "unmentionable" : "mentionable")), deleteAfter: SiotrixConstants.WAIT_TIME);
        }

        private EmbedBuilder GetEmbed(IRole role, string colorchoice, string colorname, string colorhex, HextoRGB.RGB rgbvalue)
        {
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

            var builder = new EmbedBuilder();
            builder.Title = $"Color Role Information for: **{role.Name}**";
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

            return builder;
        }
    }
}

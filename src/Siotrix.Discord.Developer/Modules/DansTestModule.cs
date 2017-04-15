using Discord.Commands;
using Discord.Addons.InteractiveCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using System.Diagnostics;
using System.Globalization;

namespace Siotrix.Discord.Developer
{
    public class DansTestModule : ModuleBase<SocketCommandContext>
    {
          private InteractiveService Interactive;

          public DansTestModule(InteractiveService Inter)
          {
              Interactive = Inter;
          }

        private Stopwatch _timer = new Stopwatch();

      /* protected override void BeforeExecute()
        {
            _timer.Start();
        } */

        [Command("dantest")]
        public async Task Danstest()
        {
            var regexColorCode = new Regex("^#[a-fA-F0-9]{6}$");
            var regexRGBCode = new Regex("^\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*$");

            await ReplyAsync("Give me any value of color (Hex, RGB, or a name) and i'll return the others.");
                 var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
                 if (response.Content == "cancel") return;            

                 string colorchoice = response.Content;           
        
                 _timer.Start();
        

            if (regexColorCode.IsMatch(colorchoice.Trim()))
            {
                string cleanHex = colorchoice.Replace("#", "0x").ToLower(); //strip # and add 0x for dictionary search
                var colornamelower = HexColorDict.ColorName(cleanHex); //look up hex in dictionary

                if (colornamelower == null)
                    colornamelower = "no name found";

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colornamelower);

                var hexcolor = colorchoice.ToUpper();

                HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(colorchoice); // convery hex to an RGB value
                var red = Convert.ToString(rgbvalue.R); // Red Property
                var green = Convert.ToString(rgbvalue.G); // Green property
                var blue = Convert.ToString(rgbvalue.B); // Blue property                

                var embed = GetEmbed(hexcolor, colorname, hexcolor, rgbvalue);
                await ReplyAsync("", embed: embed);                
            }
            else if (HexColorDict.colorHex.ContainsKey(colorchoice))
            {               
                var colorHex = HexColorDict.ColorHex(colorchoice).ToLower(); // look up hex in color name Dictionary
                string cleanHex = colorHex.Replace("0x", "#").ToUpper(); // convert the hex back to #FFFFFF format

                TextInfo text = new CultureInfo("en-US").TextInfo;
                var colorname = text.ToTitleCase(colorchoice);

                var rgbvalue = HextoRGB.HexadecimalToRGB(cleanHex); // convert hex to RGB value

                var embed = GetEmbed(colorname, colorname, cleanHex, rgbvalue);
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
                    /*  var username = Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.Username;
                      await ReplyAsync($"The color hex did not process correctly {username}, try a different RGB value.");
                      return; */
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

                    if (colornamelower == null)
                        colornamelower = "no name found";

                    TextInfo text = new CultureInfo("en-US").TextInfo;
                    var colorname = text.ToTitleCase(colornamelower);

                    HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(colorhex);

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
            var red = Convert.ToString(rgbvalue.R); // Red Property
            var green = Convert.ToString(rgbvalue.G); // Green property
            var blue = Convert.ToString(rgbvalue.B); // Blue property

            if (colorname == null)
                colorname = "No Name Found";

            if (colorchoice == null)
                colorchoice = "Colorchoice is empty";

            if (colorhex == null)
                colorhex = "No hex equivalent found.";

            var builder = new EmbedBuilder();
            builder.Title = $"Color Information for your inputted text of: {colorchoice}";
            builder.Color = new Color(rgbvalue.R, rgbvalue.G, rgbvalue.B);
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
            builder.WithFooter(x =>
            {
                x.Text = $"In {_timer.ElapsedMilliseconds}ms";
            });

            return builder;
        }
    }
}

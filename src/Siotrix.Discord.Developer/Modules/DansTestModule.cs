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
using System.Net;
using System.Net.Http;
using System.IO;
using imgshp = ImageSharp;

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

        [Command("dantest")]
        public async Task Danstest()
        {
            var regexColorCode = new Regex("^#[a-fA-F0-9]{6}$");
            var regexRGBCode = new Regex("^\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])\\s*$");

            await ReplyAsync("Give me any value of color (Hex, RGB, or a name) and i'll return the others.");
                 var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
                 if (response.Content == "cancel") return;            

                 string colorchoice = response.Content.ToLower();           
        
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

                HextoRGB.RGB rgbvalue = HextoRGB.HexadecimalToRGB(colorchoice); // convert hex to an RGB value
                var red = Convert.ToString(rgbvalue.R); // Red Property
                var green = Convert.ToString(rgbvalue.G); // Green property
                var blue = Convert.ToString(rgbvalue.B); // Blue property  
                
                if(colorname == "Black")
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

        [Command("imagetest")]
        public async Task ImageTest()
        {
            await ReplyAsync("Give me the url of the image you would like to modify.");
            var response = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
            if (response.Content == "cancel") return;

            string url = response.Content;
            Uri myUri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri))
            {
            await ReplyAsync("Please enter a valid URL image");
                return;
            }

            if (!url.EndsWith(".jpg") && !url.EndsWith(".png") && !url.EndsWith(".gif") && !url.EndsWith(".jpeg"))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You must link an Image!");
                return;
            }


            var filename = $"{Context.User.Id}.jpg";
            var stream = DownloadAsync(myUri, filename);
            await ReplyAsync($"Thank you, {Context.User.Username}! Now, let me know how you would like to manipulate this image (polaroid sepia blackwhite");
            var manipulate = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));        
            if (manipulate.Content == "cancel") return;

            if (manipulate.Content == "blackwhite")
            {
                await ReplyAsync("Great! You'd like that image in Black and White, give me just a brief moment");
                using (var input = imgshp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"BW.{filename}"))
                    {
                        var image = new imgshp.Image(input);
                        imgshp.ImageExtensions.BlackWhite(image);
                        image.Save(output);
                        output.Dispose();
                    }

                    if (File.Exists($"BW.{filename}"))
                    {
                        using (FileStream fs = File.OpenRead($"BW.{filename}"))
                        {
                            await Task.Delay(2000);
                            await ReplyAsync("Here is the image in Black and White:");
                            await Task.Delay(2000);
                            await Context.Channel.SendFileAsync(fs, $"BW.{filename}").ConfigureAwait(false);
                            await Task.Delay(2000);
                            input.Dispose();
                            await fs.FlushAsync();
                            fs.Dispose();
                            File.Delete($"{Context.User.Id}.jpg");
                            File.Delete($"BW.{filename}");
                          
                        }
                    }
                    else
                    {
                        await ReplyAsync("There was a problem finding your file...");
                    }

                }
            }

            if (manipulate.Content == "sepia")
            {
                await ReplyAsync("Great! You'd like that image with sepia processing, give me just a brief moment");
                using (var input = imgshp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"SEPIA.{filename}"))
                    {
                        var image = new imgshp.Image(input);
                        imgshp.ImageExtensions.Sepia(image);
                        image.Save(output);
                        output.Dispose();
                    }

                    if (File.Exists($"SEPIA.{filename}"))
                    {
                        using (FileStream fs = File.OpenRead($"SEPIA.{filename}"))
                        {
                            await Task.Delay(2000);
                            await ReplyAsync("Here is the image with sepia processing:");
                            await Task.Delay(2000);
                            await Context.Channel.SendFileAsync(fs, $"SEPIA.{filename}").ConfigureAwait(false);
                            await Task.Delay(2000);
                            input.Dispose();
                            await fs.FlushAsync();
                            fs.Dispose();
                            File.Delete($"{Context.User.Id}.jpg");
                            File.Delete($"SEPIA.{filename}");

                        }
                    }
                    else
                    {
                        await ReplyAsync("There was a problem finding your file...");
                    }

                }
            }

            if (manipulate.Content == "polaroid")
            {
                await ReplyAsync("Great! You'd like that image with polaroid processing, give me just a brief moment");
                using (var input = imgshp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"POL.{filename}"))
                    {
                        var image = new imgshp.Image(input);
                        imgshp.ImageExtensions.Polaroid(image);
                        image.Save(output);
                        output.Dispose();
                    }

                    if (File.Exists($"POL.{filename}"))
                    {
                        using (FileStream fs = File.OpenRead($"POL.{filename}"))
                        {
                            await Task.Delay(2000);
                            await ReplyAsync("Here is the image with polaroid processing:");
                            await Task.Delay(2000);
                            await Context.Channel.SendFileAsync(fs, $"POL.{filename}").ConfigureAwait(false);
                            await Task.Delay(2000);
                            input.Dispose();
                            await fs.FlushAsync();
                            fs.Dispose();
                            File.Delete($"{Context.User.Id}.jpg");
                            File.Delete($"POL.{filename}");

                        }
                    }
                    else
                    {
                        await ReplyAsync("There was a problem finding your file...");
                    }

                }
            }
        }

        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
                await contentStream.FlushAsync();
                contentStream.Dispose();
                await stream.FlushAsync();
                stream.Dispose();
            }
        }

        private EmbedBuilder GetEmbed(string colorchoice, string colorname, string colorhex, HextoRGB.RGB rgbvalue)
        {

            _timer.Stop();
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
            builder.Title = $"Color Information for your inputted text of: {colorchoice}";
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
            builder.WithFooter(x =>
            {
                x.Text = $"In {_timer.ElapsedMilliseconds}ms";
            });

            return builder;
        }
    }
}

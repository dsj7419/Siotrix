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
using ImageSharp;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Summary("Will do different stuff... really no need to use this unless you are Dan")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class DansTestModule : ModuleBase<SocketCommandContext>
    {
          private InteractiveService Interactive;

          public DansTestModule(InteractiveService Inter)
          {
              Interactive = Inter;
          }

        private Stopwatch _timer = new Stopwatch();       

        [Command("imagetest")]
        [Summary("Testing different image manipulations...")]
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
                using (var input = ImageSharp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"BW.{filename}"))
                    {
                        var image = new Image<Rgba32>(input);
                        ImageExtensions.BlackWhite(image);
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
                using (var input = ImageSharp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"SEPIA.{filename}"))
                    {
                        var image = new Image<Rgba32>(input);
                        ImageExtensions.Sepia(image);
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
                using (var input = ImageSharp.Image.Load(filename))
                {
                    using (var output = File.OpenWrite($"POL.{filename}"))
                    {
                        var image = new Image<Rgba32>(input);
                        ImageExtensions.Polaroid(image);
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

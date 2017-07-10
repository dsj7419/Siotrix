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
using SixLabors.Primitives;
using SixLabors.Shapes;
using System.Numerics;
using SixLabors.Fonts;

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
        private static FontCollection _fontCollection;
        private static FontFamily _titleFont;

        private static Image<Rgba32> _bgMaskImage;
        private static Image<Rgba32> _noBgMask;
        private static Image<Rgba32> _noBgMaskOverlay;


       /* public static void Initialize()
        {
            _fontCollection = new FontCollection();
            _titleFont = _fontCollection.Install("fonts/Lato-Bold.ttf");
            _bgMaskImage = ImageSharp.Image.Load("moreBGtemp.png");
            _noBgMask = ImageSharp.Image.Load("profilecardtemplate.png");
            _noBgMaskOverlay = ImageSharp.Image.Load("ProfileMASK.png");
        } */



       /*ontCollection _fontCollection = new FontCollection();
        FontFamily _titleFont = _fontCollection.Install("fonts/Walkway_Black.ttf"); */

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
           
            var filename = $"{Context.User.Id}.png";
            var stream = DownloadAsync(myUri, filename);
            await ReplyAsync($"Thank you, {Context.User.Username}! Now, let me know how you would like to manipulate this image (polaroid sepia blackwhite avatar");
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
                        image.BlackWhite();
                        image.Save(output, ImageFormats.Png);
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
                            File.Delete($"{Context.User.Id}.png");
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
                        image.Sepia();
                        image.Save(output, ImageFormats.Png);
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
                            File.Delete($"{Context.User.Id}.png");
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
                        image.Polaroid();
                        image.Save(output, ImageFormats.Png);
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
                            File.Delete($"{Context.User.Id}.png");
                            File.Delete($"POL.{filename}");

                        }
                    }
                    else
                    {
                        await ReplyAsync("There was a problem finding your file...");
                    }

                }
            }

            if (manipulate.Content == "avatar")
            {
                await ReplyAsync("Great! I'll turn this into an avatar, give me just a brief moment");
                GenerateAvatar(filename, $"AVT.{filename}", new Size(200, 200), 100);
                await Task.Delay(2000);


                if (File.Exists($"AVT.{filename}"))
                    {
                        using (FileStream fs = File.OpenRead($"AVT.{filename}"))
                        {
                            await Task.Delay(2000);
                            await ReplyAsync("Here is your new avatar:");
                            await Task.Delay(2000);
                            await Context.Channel.SendFileAsync(fs, $"AVT.{filename}").ConfigureAwait(false);
                            await Task.Delay(2000);
                          //  input.Dispose();
                            await fs.FlushAsync();
                            fs.Dispose();
                            File.Delete($"{Context.User.Id}.png");
                            File.Delete($"AVT.{filename}");

                        }
                    }
                    else
                    {
                        await ReplyAsync("There was a problem finding your file...");
                    }

               // }
            }

            if (manipulate.Content == "avatarsmall")
            {
                await ReplyAsync("Great! I'll turn this into an avatar, give me just a brief moment");
                GenerateAvatar(filename, $"AVT.{filename}", new Size(100, 100), 50);
                await Task.Delay(2000);


                if (File.Exists($"AVT.{filename}"))
                {
                    using (FileStream fs = File.OpenRead($"AVT.{filename}"))
                    {
                        await Task.Delay(2000);
                        await ReplyAsync("Here is your new avatar:");
                        await Task.Delay(2000);
                        await Context.Channel.SendFileAsync(fs, $"AVT.{filename}").ConfigureAwait(false);
                        await Task.Delay(2000);
                        //  input.Dispose();
                        await fs.FlushAsync();
                        fs.Dispose();
                        File.Delete($"{Context.User.Id}.png");
                        File.Delete($"AVT.{filename}");

                    }
                }
                else
                {
                    await ReplyAsync("There was a problem finding your file...");
                }

                // }
            }

        }  

        private static void GenerateAvatar(string source, string destination, Size size, float cornerRadius)
        {
            using (var image = ImageSharp.Image.Load(source))
            {
                image.Resize(new ImageSharp.Processing.ResizeOptions
                {
                    Size = size,
                    Mode = ImageSharp.Processing.ResizeMode.Crop
                });

                ApplyRoundedCourners(image, cornerRadius);
                image.Save(destination);
            }
        }

        public static void ApplyRoundedCourners(Image<Rgba32> img, float cornerRadius)
        {
            var corners = BuildCorners(img.Width, img.Height, cornerRadius);
            // now we have our corners time to draw them
            img.Fill(Rgba32.Transparent, corners, new GraphicsOptions(true)
            {
                BlenderMode = ImageSharp.PixelFormats.PixelBlenderMode.Src // enforces that any part of this shape that has color is punched out of the background
            });
        }

        public static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new SixLabors.Shapes.RectangularePolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            var cornerToptLeft = rect.Clip(new SixLabors.Shapes.EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we cando that by translating the orgional artound the center of the image
            var center = new Vector2(imageWidth / 2, imageHeight / 2);
            var angle = Math.PI / 2f;

            float rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            // move it across the widthof the image - the width of the shape
            var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        public static void GenerateProfile(string avatarUrl, string name, int rank, int level, int ep,
            string outputPath)
        {
            using (var output = new Image<Rgba32>(890, 150))
            {
              //  DrawMask(_noBgMask, output, new Size(1000, 150));

              //  DrawAvatar(avatarUrl, output, new Rectangle(26, 15, 121, 121));

              //  DrawMask(_noBgMaskOverlay, output, new Size(1000, 150));

              //  DrawStats(rank, level, ep, output, new System.Numerics.Vector2(200, 92), new System.Numerics.Vector2(480, 92), new System.Numerics.Vector2(830, 92), Rgba32.Black);

                DrawTitle(name, output, new System.Numerics.Vector2(200, -5), Rgba32.FromHex("#2398e1"));



                output.Save(outputPath);
            }//dispose of output to help save memory
        }


        private static void DrawMask(Image<Rgba32> mask, Image<Rgba32> output, Size size)
        {

            output.DrawImage(mask, 1, size, new Point(0, 0));

        }

        private static void DrawBackground(string backgroundUrl, Image<Rgba32> output, Size size)
        {
            using (Image<Rgba32> background = ImageSharp.Image.Load(backgroundUrl))//900x500
            {
                //draw on the background
                output.DrawImage(background, 1, size, new Point(0, 0));
            }//once draw it can be disposed as its no onger needed in memory
        }

        private static void DrawTitle(string name, Image<Rgba32> output, Vector2 pos, Rgba32 color)
        {
            output.DrawText(name, new Font(_titleFont, 60, FontStyle.Bold), color, pos);
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

using Discord;
using Siotrix.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    public class GlobalStatisticsModule : ModuleBase<SocketCommandContext>
    {
        [Command("statistics")]
        public Task GlobalDataAsync()
        {
            System.Console.WriteLine("\nSuccessfully in database table!!!!");

            using (var db = new LogDatabase())
            {
                var query = (from data in db.Messages where !data.IsBot
                             group data by data.Name into stats
                             select new
                             {
                                 AuthorId = stats.Select(m => m.AuthorId).First(),
                                 Name = stats.Select(t => t.Name).First(),
                                 Count = stats.Count()
                             }).ToList();
                var numberOfMessags = db.Messages.Where(x => !x.IsBot).Count();
                var deleteMessages = db.Messages.Where(x => !x.IsBot && x.DeletedAt != null).Count();
                var guild_query = (from guild in db.Messages where !guild.IsBot
                                   group guild by guild.GuildId into g
                                   select new
                                   {
                                       GuildName = g.Select(t => t.GuildName).First(),
                                       GuildMessages = g.Count(),
                                       GuildId = g.Select(t => t.GuildId).First()
                                  }).ToList();
                var active_query = (from active in db.Messages where !active.IsBot
                              group active by active.ChannelId into x
                              select new
                              {
                                  Count = x.Count(),
                                  GuildName = x.Select(y => y.GuildName).First(),
                                  GuildId = x.Select(y => y.GuildId).First(),
                                  ChannelName = x.Select(y => y.ChannelName).First()
                              });

                var builder = new EmbedBuilder()
                    .WithTitle("Statistics Data")
                    .WithColor(new Color(114, 137, 218))
                    .WithDescription($"Siotrix Bot")
                    .WithThumbnailUrl("http://img04.imgland.net/WyZ5FoM.png")
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages done in all channels of all guilds:";
                        x.Value = $"\t{numberOfMessags}\t{"messages"}";
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages deleted in all channels of all guilds:";
                        x.Value = $"\t{deleteMessages}\t{"messages"}";
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Per Guild:";
                        int index = 0;
                        foreach (var t in guild_query)
                        {
                            int z = 0;
                            string k = null;
                            string aa = null;
                            foreach (var m in active_query)
                            {
                                if (m.GuildId == t.GuildId)
                                {
                                    if (m.Count > z)
                                    {
                                        z = m.Count;
                                        k = m.ChannelName;
                                    }
                                }
                                aa = k;
                            }
                            index++;
                            x.Value += $"{index}{")"}\t{t.GuildName}\n\t\t{Format.Italics("Total Number of Guild Messages (lifetime):")}\t{t.GuildMessages}\t{"messages"}" +
                            $"\n\t\t{Format.Italics("Most Active Channel:")}\t\t\t{"#"}{aa}\n";
                        };
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Per User:";
                        int index = 0;
                        foreach (var t in query)
                        {
                            DateTime current = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt);
                            DateTime monthago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-30);
                            DateTime weekago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-7);
                            DateTime dayago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-1);
                            double time = (current - db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Min(m => m.CreatedAt)).TotalHours;
                            int monthCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= monthago && !p.IsBot && p.Name.Equals(t.Name)).Count();
                            int weekCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= weekago && !p.IsBot && p.Name.Equals(t.Name)).Count();
                            int dayCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= dayago && !p.IsBot && p.Name.Equals(t.Name)).Count();
                            if (time != 0)
                            {
                                index++;
                                x.Value += $"{index}{")"}\t{t.Name}\n\t\t{Format.Italics("Total number of messages:")}\t{t.Count}\t{"messages"}\n\t\t" +
                                $"{Format.Italics("Messages / hour lifetime:")}\t{Math.Round((t.Count / time), 2)}\t{"messages/hour"}\n\t\t" +
                                $"{Format.Italics("Messages this D/W/M:")}\t{dayCount}/{weekCount}/{monthCount}\t{"messages"}\n";
                            }
                            System.Console.WriteLine("This is time==={0}\n This is Max===={1}\n This is Min==={2}\n" +
                                "This is day=={3}\n This is week=={4}\n This is month=={5}\n", time,
                                db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt),
                                db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Min(m => m.CreatedAt), dayago, weekago, monthago);
                        };
                    });
                    
                
                return Context.ReplyAsync("", embed: builder);
            }
        }
    }
}

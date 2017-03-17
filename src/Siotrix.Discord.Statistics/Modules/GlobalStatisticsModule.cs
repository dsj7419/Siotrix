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
        [Command("leaderboard")]
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
                var guild_query = (from guild in db.Messages
                                   group guild by guild.GuildId into g
                                   select new
                                   {
                                       GuildName = g.Select(t => t.GuildName).First(),
                                       GuildMessages = g.Count(),
                                       ActiveChannels = g.GroupBy(a => a.ChannelId).Select(n => n.Count()).ToList(),
                                       ActiveChannelName = g.GroupBy(y => y.ChannelId).Select(v => v.First().ChannelName).ToList()
                                  }).ToList();

                var builder = new EmbedBuilder()
                    .WithTitle("Statistics Data")
                    .WithColor(new Color(114, 137, 218))
                    .WithDescription($"Siotrix Bot")
                    .WithThumbnailUrl("http://img04.imgland.net/WyZ5FoM.png")
                   .AddField(x =>
                   {
                       x.Name = $"{"Owners ####### "}" + $"{"Number of Messages"}\n";
                       int index = 0;
                       foreach (var t in query)
                       {
                           index++;
                           x.Value += $"{index}{")"}\t{t.Name}\t\t\t{t.Count}\t{"messages"}\n";
                       };
                   })
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages done in all channels of all guilds since bot started:";
                        x.Value = $"{numberOfMessags}\t{"messages"}";
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages deleted in all channels of all guilds since bot started:";
                        x.Value = $"{deleteMessages}\t{"messages"}";
                    });
                
                    /*.AddField(x =>
                    {
                        x.Name = $"Per Guild:";
                        int index = 0;
                        int firstElement = 0;
                        string mostActiveChannel = null;
                        int i = 0;
                        foreach (var t in guild_query)
                        {
                            foreach(var y in t.ActiveChannels)
                            {
                                firstElement = t.ActiveChannels[0];
                                if(y > firstElement)
                                {
                                    firstElement = y;
                                    mostActiveChannel = t.ActiveChannelName[i];
                                }
                                i++;
                            }
                            index++;
                            x.Value += $"{index}{")"}\t{t.GuildName}{"(lifetime)"}\t\t\t{t.GuildMessages}\t{"messages"}\t\t{"Most Active Channel"}\t\t{firstElement} {mostActiveChannel}\n";
                        };
                    });*/

                return Context.ReplyAsync("", embed: builder);
            }
        }
    }
}

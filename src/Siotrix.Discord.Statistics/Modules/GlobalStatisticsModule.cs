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
            System.Console.WriteLine("\nSuccessfully in daabase table!!!!");

            using (var db = new LogDatabase())
            {
                var query = (from data in db.Messages
                             group data by data.Name into stats
                             select new
                             {
                                 AuthorId = stats.Select(m => m.AuthorId).First(),
                                 Name = stats.Select(t => t.Name).First(),
                                 Count = stats.Count()
                             }).ToList();

                var builder = new EmbedBuilder()
                    .WithTitle("Current Statistics Data")
                    .WithColor(new Color(114, 137, 218))
                    .WithDescription($"Statistics Data for the last: 30 Days")
                   .AddField(x =>
                   {
                       x.Name = $"{"Owners ####### "}" + $"{"Number of Messages"}\n";
                       int index = 0;
                       foreach (var t in query)
                       {
                           index++;
                           x.Value += $"{index}{")"}\t{t.Name}\t\t\t{t.Count}\t{"messages"}\n";
                       };
                   });

                return Context.ReplyAsync("", embed: builder);
            }
        }
    }
}

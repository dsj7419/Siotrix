using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class CaseExtensions
    {
        public static long GetCaseNumber(this SocketCommandContext context)
        {
            long case_num = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Casenums.Where(x => x.GuildId.Equals(context.Guild.Id.ToLong()));
                    if (!list.Any())
                        case_num = 1;
                    else
                        case_num = list.Last().GCaseNum + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return case_num;
        }
    }
}

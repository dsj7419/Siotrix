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
        public static long GetCaseNumber(this string cmdName, SocketCommandContext context, SocketGuildUser user)
        {
            long case_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var record = new DiscordCaseNum();
                    record.GuildId = context.Guild.Id.ToLong();
                    record.UserId = user.Id.ToLong();
                    record.UserName = user.Username;
                    record.CmdName = cmdName;
                    db.Casenums.Add(record);
                    db.SaveChanges();
                    case_id = db.Casenums.Last().Id;
                    ActionResult.CaseId = case_id;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return case_id;
        }
    }
}

using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async void SaveCaseDataAsync(string cmd_name, long case_num, long user_id, long guild_id, string reason)
        {
            Console.WriteLine("================================================{0}", case_num);
            using (var db = new LogDatabase())
            {
                try
                {
                    var exist_data = db.Casenums.Where(x => x.GuildId.Equals(guild_id) && x.GCaseNum.Equals(case_num) && x.UserId.Equals(user_id) && x.CmdName.Equals(cmd_name));
                    if (exist_data.Any())
                    {
                        var data = exist_data.First();
                        data.Reason = reason;
                        db.Casenums.Update(data);
                    }
                    else
                    {
                        var record = new DiscordCaseNum();
                        record.GCaseNum = case_num;
                        record.GuildId = guild_id;
                        record.UserId = user_id;
                        record.CmdName = cmd_name;
                        record.Reason = reason;
                        db.Casenums.Add(record);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}

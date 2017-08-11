using System;
using System.Linq;
using Discord.Commands;

namespace Siotrix.Discord
{
    public static class CaseExtensions
    {
        public static long GetCaseNumber(this SocketCommandContext context)
        {
            long caseNum = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Casenums.Where(x => x.GuildId.Equals(context.Guild.Id.ToLong()));
                    if (!list.Any())
                        caseNum = 1;
                    else
                        caseNum = list.Last().GCaseNum + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return caseNum;
        }

        public static async void SaveCaseDataAsync(string cmdName, long caseNum, long userId, long guildId,
            string reason)
        {
            Console.WriteLine("================================================{0}", caseNum);
            using (var db = new LogDatabase())
            {
                try
                {
                    var existData =
                        db.Casenums.Where(x => x.GuildId.Equals(guildId) && x.GCaseNum.Equals(caseNum) &&
                                               x.UserId.Equals(userId) && x.CmdName.Equals(cmdName));
                    if (existData.Any())
                    {
                        var data = existData.First();
                        data.Reason = reason;
                        db.Casenums.Update(data);
                    }
                    else
                    {
                        var record = new DiscordCaseNum();
                        record.GCaseNum = caseNum;
                        record.GuildId = guildId;
                        record.UserId = userId;
                        record.CmdName = cmdName;
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
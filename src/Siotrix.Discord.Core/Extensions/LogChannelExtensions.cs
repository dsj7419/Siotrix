using System;
using System.Collections.Generic;
using System.Linq;

namespace Siotrix.Discord
{
    public static class LogChannelExtensions
    {
        public static long ModlogchannelId;
        public static long LogchannelId;
        public static bool IsToggledLog;
        public static bool IsToggledModlog;

        public static void IsUsableLogChannel(long guildId)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    var logList = db.Glogchannels.Where(p => p.GuildId.Equals(guildId));
                    var modlogList = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guildId));
                    if (logList.Count() > 0 || logList.Any())
                    {
                        var data = logList.First();
                        LogchannelId = data.ChannelId;
                        if (!data.IsActive)
                            IsToggledLog = true;
                        else
                            IsToggledLog = false;
                    }
                    if (modlogList.Count() > 0 || modlogList.Any())
                    {
                        var modData = modlogList.First();
                        ModlogchannelId = modData.ChannelId;
                        if (!modData.IsActive)
                            IsToggledModlog = true;
                        else
                            IsToggledModlog = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static Dictionary<int, string> GetFilterWords(long guildId)
        {
            var dictionary = new Dictionary<int, string>();
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId);
                    if (result.Any())
                    {
                        var i = 0;
                        foreach (var item in result)
                        {
                            dictionary.Add(i, item.Word);
                            i++;
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return dictionary;
        }

        public static string ParseMessages(string[] msg, Dictionary<int, string> dictionary)
        {
            string badword = null;
            foreach (var msgItem in msg)
            foreach (var dicItem in dictionary)
                if (msgItem.ToLower().Equals(dicItem.Value.ToLower()))
                {
                    badword = msgItem;
                    break;
                }
            return badword;
        }
    }
}
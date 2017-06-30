using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class LogChannelExtensions
    {
        public static long modlogchannel_id = 0;
        public static long logchannel_id = 0;
        public static bool is_toggled_log = false;
        public static bool is_toggled_modlog = false;

        public static void IsUsableLogChannel(long guild_id)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    var log_list = db.Glogchannels.Where(p => p.GuildId.Equals(guild_id));
                    var modlog_list = db.Gmodlogchannels.Where(p => p.GuildId.Equals(guild_id));
                    if (log_list.Count() > 0 || log_list.Any())
                    {
                        var data = log_list.First();
                        logchannel_id = data.ChannelId;
                        if (!data.IsActive)
                            is_toggled_log = true;
                        else
                            is_toggled_log = false;
                    }
                    if (modlog_list.Count() > 0 || modlog_list.Any())
                    {
                        var mod_data = modlog_list.First();
                        modlogchannel_id = mod_data.ChannelId;
                        if (!mod_data.IsActive)
                            is_toggled_modlog = true;
                        else
                            is_toggled_modlog = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static Dictionary<int, string> GetFilterWords(long guild_id)
        {
            var dictionary = new Dictionary<int, string>();
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guild_id);
                    if (result.Any())
                    {
                        int i = 0;
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
            foreach (var msg_item in msg)
            {
                foreach (var dic_item in dictionary)
                {
                    if (msg_item.ToLower().Equals(dic_item.Value.ToLower()))
                    {
                        badword = msg_item;
                        break;
                    }
                }
            }
            return badword;
        }
    }
}

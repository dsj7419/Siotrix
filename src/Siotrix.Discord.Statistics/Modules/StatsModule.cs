using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    [Name("Information")]
    [Summary("Statistical information for users, bot, and guild.")]
    public class StatsModule : ModuleBase<SocketCommandContext>
    {
        private Process _process;

        protected override void BeforeExecute(CommandInfo info)
        {
            _process = Process.GetCurrentProcess();
          //  Console.WriteLine(info.Summary);
        } 

        private string GetAuthorIconUrl()
        {
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    iconurl = db.Authors.First().AuthorIcon;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }

        private string GetGuildIconUrl(int id)
        {
            var guild_id = Context.Guild.Id;
            string iconurl = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gavatars.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0 || id == 2)
                    {
                        iconurl = db.Authors.First().AuthorIcon;
                    }
                    else
                    {
                        iconurl = val.First().Avatar;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return iconurl;
        }

        private string GetGuildName(int id)
        {
            var guild_id = Context.Guild.Id;
            string name = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (id == 0 || id == 1)
                    {
                        var val = db.Gnames.Where(p => p.GuildId == guild_id.ToLong());
                        if (val == null || val.ToList().Count <= 0)
                        {
                            name = Context.Guild.Name;
                        }
                        else
                        {
                            name = val.First().GuildName;
                        }
                    }
                    else
                    {
                        name = db.Authors.First().AuthorName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return name;
        }

        private string GetGuildUrl(int id)
        {
            var guild_id = Context.Guild.Id;
            string url = null;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Gwebsiteurls.Where(p => p.GuildId == guild_id.ToLong());
                    if (val == null || val.ToList().Count <= 0 || id == 2)
                    {
                        url = db.Authors.First().AuthorUrl;
                    }
                    else
                    {
                        url = val.First().SiteUrl;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return url;
        }

        private string[] GetGuildFooter(int id)
        {
            var guild_id = Context.Guild.Id;
            string[] footer = new string[2];
            using (var db = new LogDatabase())
            {
                try
                {
                    if (id == 2)
                    {
                        footer[0] = db.Bfooters.First().FooterIcon;
                        footer[1] = db.Bfooters.First().FooterText;
                    }
                    else
                    {
                        var val = db.Gfooters.Where(p => p.GuildId == guild_id.ToLong());
                        if (val == null || val.ToList().Count <= 0)
                        {
                            footer[0] = db.Bfooters.First().FooterIcon;
                            footer[1] = db.Bfooters.First().FooterText;
                        }
                        else
                        {
                            footer[0] = val.First().FooterIcon;
                            footer[1] = val.First().FooterText;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return footer;
        }

        private string GetUptime()
        {
            var uptime = (DateTime.Now - _process.StartTime);
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private string[] GetLifeTimeMessagesPerGuild()
        {
            int cnt = 0;
            string[] arr = new string[2];
            DateTime today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    cnt = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList().Count;
                    double lifeTime = (today - db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = (Math.Round((cnt / lifeTime), 2)).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return arr;
        }

        private long GetActivityChannelPerGuild()
        {
            int cnt = 0;
            //string active_channel = null;
            long active_channel_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var arr = list.GroupBy(p => p.ChannelId).ToList();
                    foreach(var item in arr)
                    {
                        if (list.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count > cnt)
                        {
                            cnt = list.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count;
                            //active_channel = item.First().ChannelName;
                            active_channel_id = item.First().ChannelId ?? 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return active_channel_id;
            //if(active_channel == null || active_channel == "")
            //{
            //    return "None";
            //}
            //else
            //{
            //    return "#" + active_channel;
            //}
        }

        private int GetDeleteMessagesPerGuild()
        {
            int count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    count = list.Where(p => p.DeletedAt != null).ToList().Count;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        private int[] GetStatsMessagesPerGuild()
        {
            int[] count = new int[3];
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
            DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
            DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    count[0] = list.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth).Count();
                    count[1] = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).Count();
                    count[2] = list.Where(p => p.CreatedAt.Date == today.Date).Count();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        private long GetActivityChannelOfWeekPerGuild()
        {
            int channel_cnt = 0;
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today); ;
            long active_channel_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();

                    var arr = list.GroupBy(p => p.ChannelId).ToList();
                    foreach (var item in arr)
                    {
                        if (list.Where(p => item.First().ChannelId == p.ChannelId && p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count > channel_cnt)
                        {
                            channel_cnt = list.Where(p => item.First().ChannelId == p.ChannelId && p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            active_channel_id = item.First().ChannelId ?? 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return active_channel_id;
        }

        private string GetActiveDataOfWeekPerGuild()
        {
            string active_data = null;
            int date_cnt = 0;
            int hour_cnt = 0;
            int active_day = 0;
            int active_hour = 0;
            long? active_channel_id = 0;
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today); ;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    active_channel_id = GetActivityChannelOfWeekPerGuild();

                    var data1 = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && p.ChannelId == active_channel_id).ToList();
                    var data2 = data1.GroupBy(p => p.CreatedAt.Day);
                    foreach (var i in data2)
                    {
                        if (i.ToList().Count > date_cnt)
                        {
                            date_cnt = i.ToList().Count;
                            active_day = i.First().CreatedAt.Day;
                        }
                    }

                    var data3 = data1.Where(p => p.CreatedAt.Day == active_day).ToList();
                    var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
                    foreach (var ii in data4)
                    {
                        if (ii.ToList().Count > hour_cnt)
                        {
                            hour_cnt = ii.ToList().Count;
                            active_hour = ii.First().CreatedAt.Hour;
                        }
                    }
                   
                    if (active_hour <= 12)
                    {
                        active_data = active_hour.ToString() + " : 00 AM";
                    }
                    else
                    {
                        active_data = (active_hour - 12).ToString() + " : 00 PM";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return active_data;
        }
        //private string[] GetActiveDataOfWeekPerGuild()
        //{
        //    string[] active_data = new string[2]; 
        //    int channel_cnt = 0;
        //    int date_cnt = 0;
        //    int hour_cnt = 0;
        //    int active_day = 0;
        //    int active_hour = 0;
        //    string active_channel_of_week = null;
        //    long? active_channel_id = 0;
        //    DateTime today = DateTime.Now;
        //    DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
        //    DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today); ;
        //    using (var db = new LogDatabase())
        //    {
        //        try
        //        {
        //            var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();

        //            var arr = list.GroupBy(p => p.ChannelId).ToList();
        //            foreach (var item in arr)
        //            {
        //                if (list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count > channel_cnt)
        //                {
        //                    channel_cnt = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count;
        //                    active_channel_of_week = item.First().ChannelName;
        //                    active_channel_id = item.First().ChannelId;
        //                }
        //            }

        //            var data1 = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && p.ChannelId == active_channel_id).ToList();
        //            var data2 = data1.GroupBy(p => p.CreatedAt.Day);
        //            foreach(var i in data2)
        //            {
        //                if(i.ToList().Count > date_cnt)
        //                {
        //                    date_cnt = i.ToList().Count;
        //                    active_day = i.First().CreatedAt.Day;
        //                }
        //            }

        //            var data3 = data1.Where(p => p.CreatedAt.Day == active_day).ToList();
        //            var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
        //            foreach(var ii in data4)
        //            {
        //                if(ii.ToList().Count > hour_cnt)
        //                {
        //                    hour_cnt = ii.ToList().Count;
        //                    active_hour = ii.First().CreatedAt.Hour;
        //                }
        //            }
        //            if(active_channel_of_week == null || active_channel_of_week == "")
        //            {
        //                active_data[0] = "None";
        //            }
        //            else
        //            {
        //                active_data[0] = "#" + active_channel_of_week;
        //            }
        //            if (active_hour <= 12)
        //            {
        //                active_data[1] = active_hour.ToString() + " : 00 AM";
        //            }
        //            else
        //            {
        //                active_data[1] = (active_hour - 12).ToString() + " : 00 PM";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //    return active_data;
        //}

        private bool CheckBot(SocketUser user)
        {
            if (user.IsBot)
            {
                return true;
            }
            return false;
        }

        private string[] GetLifeTimeMessagesPerUser(IGuildUser user)
        {
            int cnt = 0;
            string[] arr = new string[2];
            DateTime today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Messages.Where(p => p.AuthorId == user.Id.ToLong());
                    cnt = val.ToList().Count;
                    double lifeTime = (today - val.Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = (Math.Round((cnt / lifeTime), 2)).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return arr;
        }

        private int[] GetStatsMessagesPerUser(IGuildUser user)
        {
            int[] count = new int[3];
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
            DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
            DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    count[0] = list.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth).Count();
                    count[1] = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).Count();
                    count[2] = list.Where(p => p.CreatedAt.Date == today.Date).Count();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        //private string GetActivityChannelPerUser(IGuildUser user)
        //{
        //    int cnt = 0;
        //    string guild_name = null;
        //    string active_channel = null;
        //    using (var db = new LogDatabase())
        //    {
        //        try
        //        {
        //            var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
        //            var guild_group = list.GroupBy(p => p.GuildId).ToList();
        //            foreach (var a in guild_group)
        //            {
        //                var arr = a.GroupBy(p => p.ChannelId).ToList();
        //                foreach (var item in arr)
        //                {
        //                    int c_count = a.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count;
        //                    if (c_count > cnt)
        //                    {
        //                        cnt = c_count;
        //                        active_channel = item.First().ChannelName;
        //                        guild_name = item.First().GuildName;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //    if(active_channel == "" || guild_name == "" || active_channel == null || guild_name == null)
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        return "#" + active_channel + " in " + guild_name;
        //    }
        //}

        private long[] GetActivityChannelPerUser(IGuildUser user)
        {
            int cnt = 0;
            long[] ids = new long[2];
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guild_group = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guild_group)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            int c_count = a.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count;
                            if (c_count > cnt)
                            {
                                cnt = c_count;
                                ids[0] = item.First().GuildId ?? 0;
                                ids[1] = item.First().ChannelId ?? 0;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return ids;
        }

        private long[] GetActiveChannelIdOfWeekPerUser(IGuildUser user)
        {
            int channel_cnt = 0;
            long[] ids = new long[2];
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
            using(var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guild_group = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guild_group)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            int c_count = a.Where(p => p.ChannelId == item.First().ChannelId && p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            if (c_count > channel_cnt)
                            {
                                channel_cnt = c_count;
                                ids[0] = item.First().GuildId ?? 0;
                                ids[1] = item.First().ChannelId ?? 0;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return ids;
        }

        private string[] GetActiveDataOfWeekPerUser(IGuildUser user)
        {
            string[] active_data = new string[2];
            int channel_cnt = 0;
            int date_cnt = 0;
            int hour_cnt = 0;
            int active_day = 0;
            int active_hour = 0;
            long? active_channel_id = 0;
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today); 
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guild_group = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guild_group)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            int c_count = a.Where(p => p.ChannelId == item.First().ChannelId && p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            if (c_count > channel_cnt)
                            {
                                active_channel_id = item.First().ChannelId ?? 0;
                            }
                        }

                        var data1 = a.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && p.ChannelId == active_channel_id).ToList();
                        var data2 = data1.GroupBy(p => p.CreatedAt.Day);
                        foreach (var i in data2)
                        {
                            if (i.ToList().Count > date_cnt)
                            {
                                date_cnt = i.ToList().Count;
                                active_day = i.First().CreatedAt.Day;
                            }
                        }
                        if (active_day != 0)
                        {
                            var data3 = data1.Where(p => p.CreatedAt.Day == active_day).ToList();
                            var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
                            foreach (var ii in data4)
                            {
                                if (ii.ToList().Count > hour_cnt)
                                {
                                    hour_cnt = ii.ToList().Count;
                                    active_hour = ii.First().CreatedAt.Hour;
                                }
                            }
                        }
                    }

                    if (active_day == 0)
                    {
                        active_data[0] = "-";
                        active_data[1] = "-";
                    }
                    else
                    {
                        DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, active_day, 0, 0, 0, 0);
                        active_data[0] = String.Format("{0:dddd, MMMM d, yyyy}", dt);
                        if (active_hour <= 12)
                        {
                            active_data[1] = active_hour.ToString() + " : 00 AM";
                        }
                        else
                        {
                            active_data[1] = (active_hour - 12).ToString() + " : 00 PM";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return active_data;
        }

        //private string[] GetActiveDataOfWeekPerUser(IGuildUser user)
        //{
        //    string[] active_data = new string[3];
        //    int channel_cnt = 0;
        //    int date_cnt = 0;
        //    int hour_cnt = 0;
        //    int active_day = 0;
        //    int active_hour = 0;
        //    string active_channel_of_week = null;
        //    string guild_name = null;
        //    long? active_channel_id = 0;
        //    DateTime today = DateTime.Now;
        //    DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
        //    DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today); ;
        //    using (var db = new LogDatabase())
        //    {
        //        try
        //        {
        //            var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
        //            var guild_group = list.GroupBy(p => p.GuildId).ToList();
        //            foreach(var a in guild_group)
        //            {
        //                var arr = a.GroupBy(p => p.ChannelId).ToList();
        //                foreach (var item in arr)
        //                {
        //                    int c_count = a.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).ToList().Count;
        //                    if (c_count > channel_cnt)
        //                    {
        //                        channel_cnt = c_count;
        //                        active_channel_of_week = item.First().ChannelName;
        //                        guild_name = item.First().GuildName;
        //                        active_channel_id = item.First().ChannelId;
        //                    }
        //                }

        //                var data1 = a.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && p.ChannelId == active_channel_id).ToList();
        //                var data2 = data1.GroupBy(p => p.CreatedAt.Day);
        //                foreach (var i in data2)
        //                {
        //                    if (i.ToList().Count > date_cnt)
        //                    {
        //                        date_cnt = i.ToList().Count;
        //                        active_day = i.First().CreatedAt.Day;
        //                    }
        //                }
        //                if(active_day != 0)
        //                {
        //                    var data3 = data1.Where(p => p.CreatedAt.Day == active_day).ToList();
        //                    var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
        //                    foreach (var ii in data4)
        //                    {
        //                        if (ii.ToList().Count > hour_cnt)
        //                        {
        //                            hour_cnt = ii.ToList().Count;
        //                            active_hour = ii.First().CreatedAt.Hour;
        //                        }
        //                    }
        //                }
        //            }

        //            if(active_channel_of_week == null || active_channel_of_week == "" || guild_name == null || guild_name == "")
        //            {
        //                active_data[0] = "None";
        //            }
        //            else
        //            {
        //                active_data[0] = "#" + active_channel_of_week + " in " + guild_name;
        //            }
        //            if(active_day == 0)
        //            {
        //                active_data[1] = "-";
        //                active_data[2] = "-";
        //            }
        //            else
        //            {
        //                //active_data[1] = active_day.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
        //                DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, active_day, 0, 0, 0, 0);
        //                active_data[1] = String.Format("{0:dddd, MMMM d, yyyy}", dt);
        //                if (active_hour <= 12)
        //                {
        //                    active_data[2] = active_hour.ToString() + " : 00 AM";
        //                }
        //                else
        //                {
        //                    active_data[2] = (active_hour - 12).ToString() + " : 00 PM";
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //    return active_data;
        //}

        private string[] GetGlobalLifeTimeMessages()
        {
            int cnt = 0;
            string[] arr = new string[2];
            DateTime today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Messages.Where(p => !p.IsBot);
                    cnt = val.ToList().Count;
                    double lifeTime = (today - val.Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = (Math.Round((cnt / lifeTime), 2)).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return arr;
        }

        private int[] GetGlobalStatsMessages()
        {
            int[] count = new int[3];
            DateTime today = DateTime.Now;
            DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
            DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
            DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot).ToList();
                    count[0] = list.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth).Count();
                    count[1] = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).Count();
                    count[2] = list.Where(p => p.CreatedAt.Date == today.Date).Count();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        private int GetGlobalUsers()
        {
            int count = 0;
            foreach(var a in Context.Client.Guilds)
            {
                foreach (var i in Context.Guild.Users)
                {
                    if(a.Users.Where(p => p.Id == i.Id).ToList().Count > 0)
                    {
                        count--;
                    }
                    count++;
                }
            }
            return count;
        }

        private long GetGlobalActivityGuild()
        {
            int cnt = 0;
            long guild_id = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot).ToList();
                    var guild_group = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guild_group)
                    {
                        if (a.ToList().Count > cnt)
                        {
                            cnt = a.ToList().Count;
                            guild_id = a.First().GuildId ?? 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return guild_id;
        }

        //private string GetGlobalActivityGuild()
        //{
        //    int cnt = 0;
        //    string guild_name = null;
        //    using (var db = new LogDatabase())
        //    {
        //        try
        //        {
        //            var list = db.Messages.Where(p => !p.IsBot).ToList();
        //            var guild_group = list.GroupBy(p => p.GuildId).ToList();
        //            foreach (var a in guild_group)
        //            {
        //                if(a.ToList().Count > cnt)
        //                {
        //                    cnt = a.ToList().Count;
        //                    guild_name = a.First().GuildName;
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //    if(guild_name == null || guild_name == "")
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        return guild_name;
        //    }
        //}

        [Command("stats"), Alias("statistics")]
        [Summary("Statistical command to display guild information data.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync()
        {
            string g_icon_url = GetGuildIconUrl(0);
            string g_name = GetGuildName(0);
            string g_url = GetGuildUrl(0);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GetGuildFooter(0);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            string[] m_count = GetLifeTimeMessagesPerGuild();
            string active_channel = "None";
            if (GetActivityChannelPerGuild() > 0)
                active_channel = "#" + Context.Guild.GetChannel(GetActivityChannelPerGuild().ToUlong()).Name;
            int delete_msg_per_guild = GetDeleteMessagesPerGuild();
            int[] m_stats_count = GetStatsMessagesPerGuild();
            string active_channel_of_week = "None";
            if (GetActivityChannelOfWeekPerGuild() > 0)
                active_channel_of_week = "#" + Context.Guild.GetChannel(GetActivityChannelOfWeekPerGuild().ToUlong()).Name;
            string active_data_of_week = GetActiveDataOfWeekPerGuild();

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(g_icon_url)
                .WithName(g_name)
                .WithUrl(g_url))
                .WithDescription($"for individual information or bot information please use {g_prefix}stats @username")
                .WithColor(g_color)
                .WithTitle("Statistical Information sheet for " + g_name)
                .WithThumbnailUrl(g_thumbnail)
                .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(g_footer[0])
                .WithText(g_footer[1]))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Number of Members"), Value = Context.Guild.Users.Count() })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("lifetime messages : "), Value = m_count[0] })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages / hour : "), Value = m_count[1] + " messages/hour" })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Number of channels : "), Value = Context.Guild.Channels.Count })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Channel : "), Value = active_channel })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Lifetime message deletes : "), Value = delete_msg_per_guild })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages this D/W/M : "), Value = m_stats_count[2] + "/" + m_stats_count[1] + "/" + m_stats_count[0] + "messages" })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Channel this week : "), Value = active_channel_of_week })
                .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Time most active on that day : "), Value = active_data_of_week });

            return ReplyAsync("", embed: builder);
        }

        [Command("stats"), Alias("statistics")]
        [Summary("Statistical command to display user or bot information data.")]
        [Remarks("<@username> - any @user or bot.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public Task StatsAsync(SocketUser user)
        {
            int id = 0;
            bool bot = CheckBot(user);
            var person = user as IGuildUser;
            if (!bot)
            {
                id = 1;
            }
            else
            {
                id = 2;
            }
            string g_icon_url = GetGuildIconUrl(id);
            string g_name = GetGuildName(id);
            string g_url = GetGuildUrl(id);
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            string g_thumbnail = GuildEmbedThumbnail.GetGuildThumbNail(Context);
            string[] g_footer = GetGuildFooter(id);
            string g_prefix = PrefixExtensions.GetGuildPrefix(Context);
            string[] m_count = GetLifeTimeMessagesPerUser(person);
            int[] m_stats_count = GetStatsMessagesPerUser(person);
            //string active_channel = GetActivityChannelPerUser(person);
            string active_channel = "None";
            if (GetActivityChannelPerUser(person)[0] > 0 && GetActivityChannelPerUser(person)[1] > 0)
                active_channel = "#" + Context.Client.GetGuild(GetActivityChannelPerUser(person)[0].ToUlong()).GetChannel(GetActivityChannelPerUser(person)[1].ToUlong()).Name + 
                    " in " + Context.Client.GetGuild(GetActivityChannelPerUser(person)[0].ToUlong()).Name;
            string active_channel_of_week = "None";
            if (GetActiveChannelIdOfWeekPerUser(person)[0] > 0 && GetActiveChannelIdOfWeekPerUser(person)[1] > 0)
                active_channel_of_week = "#" + Context.Client.GetGuild(GetActiveChannelIdOfWeekPerUser(person)[0].ToUlong()).GetChannel(GetActiveChannelIdOfWeekPerUser(person)[1].ToUlong()).Name + 
                    " in " + Context.Client.GetGuild(GetActiveChannelIdOfWeekPerUser(person)[0].ToUlong()).Name;
            string[] active_data_of_week = GetActiveDataOfWeekPerUser(person);
            string[] b_count = GetGlobalLifeTimeMessages();
            int[] b_stats_count = GetGlobalStatsMessages();
            int b_user_count = GetGlobalUsers();
            //string b_active_guild = GetGlobalActivityGuild();
            string b_active_guild = Context.Client.GetGuild(GetGlobalActivityGuild().ToUlong()).Name;

            var builder = new EmbedBuilder()
                 .WithAuthor(new EmbedAuthorBuilder()
                 .WithIconUrl(g_icon_url)
                 .WithName(g_name)
                 .WithUrl(g_url))
                 .WithColor(g_color)
                 .WithFooter(new EmbedFooterBuilder()
                 .WithIconUrl(g_footer[0])
                 .WithText(g_footer[1]))
                 .WithTimestamp(DateTime.UtcNow);
            if (id == 1)
            {
                double joined = (DateTime.Now - person.JoinedAt)?.TotalDays ?? 0;
                string join_date = String.Format("{0:dddd, MMMM d, yyyy}", person.JoinedAt?.DateTime ?? DateTime.Now);
                builder
                    .WithTitle("Statistical Information sheet for " + Context.Guild.GetUser(user.Id).Username)
                    .WithDescription($"for general guild information information please use {g_prefix}stats")
                    .WithThumbnailUrl(person.GetAvatarUrl().ToString())
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Joined Server : "), Value = join_date })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("lifetime messages : "), Value = m_count[0] })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages / hour : "), Value = m_count[1] + " messages/hour" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages this D/W/M : "), Value = m_stats_count[2] + "/" + m_stats_count[1] + "/" + m_stats_count[0] + "messages" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Channel : "), Value = active_channel })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Channel this week : "), Value = active_channel_of_week })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Day this week : "), Value = active_data_of_week[0] })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Time most active on that day : "), Value = active_data_of_week[1] });
            }
            else if(id == 2)
            {
                builder
                    .WithTitle("Statistics for Siotrix Bot")
                    .WithDescription($"for general guild information information please use {g_prefix}stats")
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Number of Guilds : "), Value = Context.Client.Guilds.Count })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("lifetime messages : "), Value = b_count[0] })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages / hour : "), Value = b_count[1] + " messages/hour" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Messages this D/W/M : "), Value = b_stats_count[2] + "/" + b_stats_count[1] + "/" + b_stats_count[0] + "messages" })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Number of Channels : "), Value = Context.Guild.Channels.Count * Context.Client.Guilds.Count })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Number of Users : "), Value = b_user_count })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Most Active Guild This Week : "), Value = b_active_guild })
                    .AddField(new EmbedFieldBuilder() { IsInline = true, Name = Format.Underline("Uptime : "), Value = GetUptime() });
            }

            return ReplyAsync("", embed: builder);
        }
    }
}

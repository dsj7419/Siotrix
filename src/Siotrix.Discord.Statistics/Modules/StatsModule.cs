using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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

        private async Task<string> GetGuildIconUrl(int id)
        {
            string iconurl;

            if (id == 2)
            {
                var val = await Context.GetGuildIconUrlAsync();
                iconurl = val.Avatar;
            }
            else
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                iconurl = val.AuthorIcon;
            }
            return iconurl;
        }

        private async Task<string> GetGuildName(int id)
        {
            string name;

            if (id == 2)
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                name = val.AuthorName;
            }
            else
            {
                var val = await Context.GetGuildNameAsync();
                name = val.GuildName;
            }

            return name;
        }

        private async Task<string> GetGuildUrl(int id)
        {
            string url = null;

            if (id == 2)
            {
                var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
                url = val.AuthorUrl;
            }
            else
            {
                var val = await Context.GetGuildUrlAsync();
                url = val.SiteUrl;
            }           
            return url;
        }

        private async Task<string> GetGuildFooterIcon(int id)
        {
            string footerIcon;

            if (id == 2)
            {
                var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
                footerIcon = val.FooterIcon;
            }
            else
            {
                var val = await Context.GetGuildFooterAsync();
                footerIcon = val.FooterIcon;
            }           
            return footerIcon;
        }

        private async Task<string> GetGuildFooterText(int id)
        {
            string footerText;

            if (id == 2)
            {
                var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
                footerText = val.FooterText;
            }
            else
            {
                var val = await Context.GetGuildFooterAsync();
                footerText = val.FooterText;
            }
            return footerText;
        }

        private string GetUptime()
        {
            var uptime = DateTime.Now - _process.StartTime;
            return $"{uptime.Days} day {uptime.Hours} hr {uptime.Minutes} min {uptime.Seconds} sec";
        }

        private string[] GetLifeTimeMessagesPerGuild()
        {
            var cnt = 0;
            var arr = new string[2];
            var today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    cnt = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList().Count;
                    var lifeTime = (today - db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong())
                                        .Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = Math.Round(cnt / lifeTime, 2).ToString();
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
            var cnt = 0;
            //string active_channel = null;
            long activeChannelId = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    var arr = list.GroupBy(p => p.ChannelId).ToList();
                    foreach (var item in arr)
                        if (list.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count > cnt)
                        {
                            cnt = list.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count;
                            //active_channel = item.First().ChannelName;
                            activeChannelId = item.First().ChannelId ?? 0;
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return activeChannelId;
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
            var count = 0;
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
            var count = new int[3];
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            var firstDayOfMonth = today.FirstDayOfMonth();
            var lastDayOfMonth = today.LastDayOfMonth();
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
            var channelCnt = 0;
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            ;
            long activeChannelId = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();

                    var arr = list.GroupBy(p => p.ChannelId).ToList();
                    foreach (var item in arr)
                        if (list.Where(p => item.First().ChannelId == p.ChannelId && p.CreatedAt <= lastDayOfWeek &&
                                            p.CreatedAt >= firstDayOfWeek).ToList().Count > channelCnt)
                        {
                            channelCnt = list
                                .Where(p => item.First().ChannelId == p.ChannelId && p.CreatedAt <= lastDayOfWeek &&
                                            p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            activeChannelId = item.First().ChannelId ?? 0;
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return activeChannelId;
        }

        private string GetActiveDataOfWeekPerGuild()
        {
            string activeData = null;
            var dateCnt = 0;
            var hourCnt = 0;
            var activeDay = 0;
            var activeHour = 0;
            long? activeChannelId = 0;
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            ;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot && p.GuildId == Context.Guild.Id.ToLong()).ToList();
                    activeChannelId = GetActivityChannelOfWeekPerGuild();

                    var data1 = list.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek &&
                                                p.ChannelId == activeChannelId).ToList();
                    var data2 = data1.GroupBy(p => p.CreatedAt.Day);
                    foreach (var i in data2)
                        if (i.ToList().Count > dateCnt)
                        {
                            dateCnt = i.ToList().Count;
                            activeDay = i.First().CreatedAt.Day;
                        }

                    var data3 = data1.Where(p => p.CreatedAt.Day == activeDay).ToList();
                    var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
                    foreach (var ii in data4)
                        if (ii.ToList().Count > hourCnt)
                        {
                            hourCnt = ii.ToList().Count;
                            activeHour = ii.First().CreatedAt.Hour;
                        }

                    if (activeHour <= 12)
                        activeData = activeHour + " : 00 AM";
                    else
                        activeData = activeHour - 12 + " : 00 PM";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return activeData;
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
                return true;
            return false;
        }

        private string[] GetLifeTimeMessagesPerUser(IGuildUser user)
        {
            var cnt = 0;
            var arr = new string[2];
            var today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Messages.Where(p => p.AuthorId == user.Id.ToLong());
                    cnt = val.ToList().Count;
                    var lifeTime = (today - val.Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = Math.Round(cnt / lifeTime, 2).ToString();
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
            var count = new int[3];
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            var firstDayOfMonth = today.FirstDayOfMonth();
            var lastDayOfMonth = today.LastDayOfMonth();
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
            var cnt = 0;
            var ids = new long[2];
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guildGroup = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guildGroup)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            var cCount = a.Where(p => p.ChannelId == item.First().ChannelId).ToList().Count;
                            if (cCount > cnt)
                            {
                                cnt = cCount;
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
            var channelCnt = 0;
            var ids = new long[2];
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guildGroup = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guildGroup)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            var cCount = a
                                .Where(p => p.ChannelId == item.First().ChannelId && p.CreatedAt <= lastDayOfWeek &&
                                            p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            if (cCount > channelCnt)
                            {
                                channelCnt = cCount;
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
            var activeData = new string[2];
            var channelCnt = 0;
            var dateCnt = 0;
            var hourCnt = 0;
            var activeDay = 0;
            var activeHour = 0;
            long? activeChannelId = 0;
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => p.AuthorId == user.Id.ToLong()).ToList();
                    var guildGroup = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guildGroup)
                    {
                        var arr = a.GroupBy(p => p.ChannelId).ToList();
                        foreach (var item in arr)
                        {
                            var cCount = a
                                .Where(p => p.ChannelId == item.First().ChannelId && p.CreatedAt <= lastDayOfWeek &&
                                            p.CreatedAt >= firstDayOfWeek).ToList().Count;
                            if (cCount > channelCnt)
                                activeChannelId = item.First().ChannelId ?? 0;
                        }

                        var data1 = a.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek &&
                                                 p.ChannelId == activeChannelId).ToList();
                        var data2 = data1.GroupBy(p => p.CreatedAt.Day);
                        foreach (var i in data2)
                            if (i.ToList().Count > dateCnt)
                            {
                                dateCnt = i.ToList().Count;
                                activeDay = i.First().CreatedAt.Day;
                            }
                        if (activeDay != 0)
                        {
                            var data3 = data1.Where(p => p.CreatedAt.Day == activeDay).ToList();
                            var data4 = data3.GroupBy(p => p.CreatedAt.Hour);
                            foreach (var ii in data4)
                                if (ii.ToList().Count > hourCnt)
                                {
                                    hourCnt = ii.ToList().Count;
                                    activeHour = ii.First().CreatedAt.Hour;
                                }
                        }
                    }

                    if (activeDay == 0)
                    {
                        activeData[0] = "-";
                        activeData[1] = "-";
                    }
                    else
                    {
                        var dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, activeDay, 0, 0, 0, 0);
                        activeData[0] = string.Format("{0:dddd, MMMM d, yyyy}", dt);
                        if (activeHour <= 12)
                            activeData[1] = activeHour + " : 00 AM";
                        else
                            activeData[1] = activeHour - 12 + " : 00 PM";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return activeData;
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
            var cnt = 0;
            var arr = new string[2];
            var today = DateTime.Now;
            using (var db = new LogDatabase())
            {
                try
                {
                    var val = db.Messages.Where(p => !p.IsBot);
                    cnt = val.ToList().Count;
                    var lifeTime = (today - val.Min(m => m.CreatedAt)).TotalHours;
                    arr[0] = cnt.ToString();
                    arr[1] = Math.Round(cnt / lifeTime, 2).ToString();
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
            var count = new int[3];
            var today = DateTime.Now;
            var firstDayOfWeek = today.FirstDayOfWeek();
            var lastDayOfWeek = today.LastDayOfWeek();
            var firstDayOfMonth = today.FirstDayOfMonth();
            var lastDayOfMonth = today.LastDayOfMonth();
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
            var count = 0;
            foreach (var a in Context.Client.Guilds)
            foreach (var i in Context.Guild.Users)
            {
                if (a.Users.Where(p => p.Id == i.Id).ToList().Count > 0)
                    count--;
                count++;
            }
            return count;
        }

        private long GetGlobalActivityGuild()
        {
            var cnt = 0;
            long guildId = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    var list = db.Messages.Where(p => !p.IsBot).ToList();
                    var guildGroup = list.GroupBy(p => p.GuildId).ToList();
                    foreach (var a in guildGroup)
                        if (a.ToList().Count > cnt)
                        {
                            cnt = a.ToList().Count;
                            guildId = a.First().GuildId ?? 0;
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return guildId;
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

        [Command("stats")]
        [Alias("statistics")]
        [Summary("Statistical command to display guild information data.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public async Task StatsAsync()
        {
            var gIconUrl = await GetGuildIconUrl(0);
            var gName = await GetGuildName(0);
            var gUrl = await GetGuildUrl(0);
            var gColor = await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooterIcon = await GetGuildFooterIcon(0);
            var gFooterText = await GetGuildFooterText(0);
            var gPrefix = await Context.GetGuildPrefixAsync();            
            var mCount = GetLifeTimeMessagesPerGuild();
            var activeChannel = "None";
            if (GetActivityChannelPerGuild() > 0)
                activeChannel = "#" + Context.Guild.GetChannel(GetActivityChannelPerGuild().ToUlong()).Name;
            var deleteMsgPerGuild = GetDeleteMessagesPerGuild();
            var mStatsCount = GetStatsMessagesPerGuild();
            var activeChannelOfWeek = "None";
            if (GetActivityChannelOfWeekPerGuild() > 0)
                activeChannelOfWeek = "#" + Context.Guild.GetChannel(GetActivityChannelOfWeekPerGuild().ToUlong())
                                             .Name;
            var activeDataOfWeek = GetActiveDataOfWeekPerGuild();

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithDescription($"for individual information or bot information please use {gPrefix.Prefix}stats @username")
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithTitle("Statistical Information sheet for " + gName)
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooterIcon)
                    .WithText(gFooterText))
                .WithTimestamp(DateTime.UtcNow);
            builder
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Number of Members"),
                    Value = Context.Guild.Users.Count()
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("lifetime messages : "),
                    Value = mCount[0]
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Messages / hour : "),
                    Value = mCount[1] + " messages/hour"
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Number of channels : "),
                    Value = Context.Guild.Channels.Count
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Most Active Channel : "),
                    Value = activeChannel
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Lifetime message deletes : "),
                    Value = deleteMsgPerGuild
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Messages this D/W/M : "),
                    Value = mStatsCount[2] + "/" + mStatsCount[1] + "/" + mStatsCount[0] + "messages"
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Most Active Channel this week : "),
                    Value = activeChannelOfWeek
                })
                .AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = Format.Underline("Time most active on that day : "),
                    Value = activeDataOfWeek
                });

            await ReplyAsync("", embed: builder);
        }

        [Command("stats")]
        [Alias("statistics")]
        [Summary("Statistical command to display user or bot information data.")]
        [Remarks("(@username)")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public async Task StatsAsync(SocketGuildUser user)
        {
            var id = 0;
            var bot = CheckBot(user);
            if (bot && user.Id != SiotrixConstants.BotId)
            {
                await ReplyAsync("It's a bot, and its not me..not much more you need to know!");
                return;
            }

            var person = user as IGuildUser;
            if (!bot)
                id = 1;
            else
                id = 2;
            var gIconUrl = await GetGuildIconUrl(id);
            var gName = await GetGuildName(id);
            var gUrl = await GetGuildUrl(id);
            var gColor = await Context.GetGuildColorAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooterIcon = await GetGuildFooterIcon(id);
            var gFooterText = await GetGuildFooterText(id);
            var gPrefix = await Context.GetGuildPrefixAsync();
            var mCount = GetLifeTimeMessagesPerUser(person);
            var mStatsCount = GetStatsMessagesPerUser(person);
            //string active_channel = GetActivityChannelPerUser(person);
            var activeChannel = "None";
            if (GetActivityChannelPerUser(person)[0] > 0 && GetActivityChannelPerUser(person)[1] > 0)
                activeChannel = "#" + Context.Client.GetGuild(GetActivityChannelPerUser(person)[0].ToUlong())
                                     .GetChannel(GetActivityChannelPerUser(person)[1].ToUlong()).Name +
                                 " in " + Context.Client.GetGuild(GetActivityChannelPerUser(person)[0].ToUlong()).Name;
            var activeChannelOfWeek = "None";
            if (GetActiveChannelIdOfWeekPerUser(person)[0] > 0 && GetActiveChannelIdOfWeekPerUser(person)[1] > 0)
                activeChannelOfWeek = "#" + Context.Client
                                             .GetGuild(GetActiveChannelIdOfWeekPerUser(person)[0].ToUlong())
                                             .GetChannel(GetActiveChannelIdOfWeekPerUser(person)[1].ToUlong()).Name +
                                         " in " + Context.Client
                                             .GetGuild(GetActiveChannelIdOfWeekPerUser(person)[0].ToUlong()).Name;
            var activeDataOfWeek = GetActiveDataOfWeekPerUser(person);
            var bCount = GetGlobalLifeTimeMessages();
            var bStatsCount = GetGlobalStatsMessages();
            var bUserCount = GetGlobalUsers();
            //string b_active_guild = GetGlobalActivityGuild();
            var bActiveGuild = Context.Client.GetGuild(GetGlobalActivityGuild().ToUlong()).Name;

            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl)
                    .WithName(gName)
                    .WithUrl(gUrl))
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooterIcon)
                    .WithText(gFooterText))
                .WithTimestamp(DateTime.UtcNow);
            if (id == 1)
            {
                var joined = (DateTime.Now - person.JoinedAt)?.TotalDays ?? 0;
                var joinDate = string.Format("{0:dddd, MMMM d, yyyy}", person.JoinedAt?.DateTime ?? DateTime.Now);
                builder
                    .WithTitle("Statistical Information sheet for " + Context.Guild.GetUser(user.Id).Username)
                    .WithDescription($"for general guild information information please use {gPrefix.Prefix}stats")
                    .WithThumbnailUrl(person.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Joined Server : "),
                        Value = joinDate
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("lifetime messages : "),
                        Value = mCount[0]
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Messages / hour : "),
                        Value = mCount[1] + " messages/hour"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Messages this D/W/M : "),
                        Value = mStatsCount[2] + "/" + mStatsCount[1] + "/" + mStatsCount[0] + "messages"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Most Active Channel : "),
                        Value = activeChannel
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Most Active Channel this week : "),
                        Value = activeChannelOfWeek
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Most Active Day this week : "),
                        Value = activeDataOfWeek[0]
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Time most active on that day : "),
                        Value = activeDataOfWeek[1]
                    });
            }
            else if (id == 2)
            {
                builder
                    .WithTitle("Statistics for Siotrix Bot")
                    .WithDescription($"for general guild information information please use {gPrefix.Prefix}stats")
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Number of Guilds : "),
                        Value = Context.Client.Guilds.Count
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("lifetime messages : "),
                        Value = bCount[0]
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Messages / hour : "),
                        Value = bCount[1] + " messages/hour"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Messages this D/W/M : "),
                        Value = bStatsCount[2] + "/" + bStatsCount[1] + "/" + bStatsCount[0] + "messages"
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Number of Channels : "),
                        Value = Context.Guild.Channels.Count * Context.Client.Guilds.Count
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Number of Users : "),
                        Value = bUserCount
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Most Active Guild This Week : "),
                        Value = bActiveGuild
                    })
                    .AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = Format.Underline("Uptime : "),
                        Value = GetUptime()
                    });
            }

            await ReplyAsync("", embed: builder);
        }
    }
}
﻿using Discord;
using Siotrix.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Siotrix.Utility;

namespace Siotrix.Discord.Statistics
{
    public class GlobalStatisticsModule : ModuleBase<SocketCommandContext>
    {
        [Command("statistics")]
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
                var guild_query = (from guild in db.Messages where !guild.IsBot
                                   group guild by guild.GuildId into g
                                   select new
                                   {
                                       GuildName = g.Select(t => t.GuildName).First(),
                                       GuildMessages = g.Count(),
                                       GuildId = g.Select(t => t.GuildId).First()
                                  }).ToList();
                var active_query = (from active in db.Messages where !active.IsBot
                              group active by active.ChannelId into x
                              select new
                              {
                                  Count = x.Count(),
                                  GuildName = x.Select(y => y.GuildName).First(),
                                  GuildId = x.Select(y => y.GuildId).First(),
                                  ChannelName = x.Select(y => y.ChannelName).First()
                              });

                var builder = new EmbedBuilder()
                    .WithTitle("Statistics Data")
                    .WithColor(new Color(114, 137, 218))
                    .WithDescription($"Siotrix Bot")
                    .WithThumbnailUrl("http://img04.imgland.net/WyZ5FoM.png")
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages done in all channels of all guilds:";
                        x.Value = $"\t{numberOfMessags}\t{"messages"}";
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Number of messages deleted in all channels of all guilds:";
                        x.Value = $"\t{deleteMessages}\t{"messages"}";
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Per Guild:";
                        int index = 0;
                        foreach (var t in guild_query)
                        {
                            int z = 0;
                            string k = null;
                            string aa = null;
                            foreach (var m in active_query)
                            {
                                if (m.GuildId == t.GuildId)
                                {
                                    if (m.Count > z)
                                    {
                                        z = m.Count;
                                        k = m.ChannelName;
                                    }
                                }
                                aa = k;
                            }
                            index++;
                            x.Value += $"{index}{")"}\t{Format.Underline(t.GuildName)}\n\t\t{Format.Italics("Total Number of Guild Messages (lifetime):")}\t{t.GuildMessages}\t{"messages"}" +
                            $"\n\t\t{Format.Italics("Most Active Channel:")}\t\t\t{"#"}{aa}\n";
                        };
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Per User:";
                        int index = 0;
                        foreach (var t in query)
                        {
                            DateTime current = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt);
                            double time = (current - db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Min(m => m.CreatedAt)).TotalHours;
                            DateTime monthago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-30);
                            DateTime weekago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-7);
                            DateTime dayago = db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Max(m => m.CreatedAt).AddDays(-1);
                            int monthCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= monthago && !p.IsBot && p.Name.Equals(t.Name)).Count();
                            int weekCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= weekago && !p.IsBot && p.Name.Equals(t.Name)).Count();
                            int dayCount = db.Messages.Where(p => p.CreatedAt <= current && p.CreatedAt >= dayago && !p.IsBot && p.Name.Equals(t.Name)).Count();

                            if (time != 0)
                            {
                                index++;
                                x.Value += $"{index}{")"}\t{Format.Underline(t.Name)}\n\t\t{Format.Italics("Total number of messages:")}\t{t.Count}\t{"messages"}\n\t\t" +
                                $"{Format.Italics("Messages / hour lifetime:")}\t{Math.Round((t.Count / time), 2)}\t{"messages/hour"}\n\t\t" +
                                $"{Format.Italics("Messages this D/W/M:")}\t{dayCount}/{weekCount}/{monthCount}\t{"messages"}\n";
                            }
                        };
                    })
                    .AddField(x =>
                    {
                        x.Name = $"Per Guild Per Users:";
                        int guild_index = 0;
                     
                        foreach (var t in guild_query)
                        {
                            var list = db.Messages.Where(o => o.GuildId == t.GuildId && !o.IsBot).ToList();
                            if(list != null)
                            {
                                var users = from w in list group w by w.Name into y select new { Name = y.Select(p => p.Name).First() };
                                guild_index++;
                                x.Value += $"{Format.Italics("Guild Name :")}\t{guild_index}{")"}\t{Format.Underline(t.GuildName)}\n";
                                foreach (var user in users)
                                {
                                    int i = 0;
                                    string str = null;
                                    var details = list.Where(z => z.Name == user.Name);
                                    DateTime current = list.Where(p => p.Name.Equals(user.Name)).Max(m => m.CreatedAt);
                                    DateTime monthago = list.Where(p => p.Name.Equals(user.Name)).Max(m => m.CreatedAt).AddDays(-30);
                                    DateTime weekago = list.Where(p => p.Name.Equals(user.Name)).Max(m => m.CreatedAt).AddDays(-7);
                                    DateTime dayago = list.Where(p => p.Name.Equals(user.Name)).Max(m => m.CreatedAt).AddDays(-1);
                                    int monthCount = details.Where(p => p.CreatedAt <= current && p.CreatedAt >= monthago).Count();
                                    int weekCount = details.Where(p => p.CreatedAt <= current && p.CreatedAt >= weekago).Count();
                                    int dayCount = details.Where(p => p.CreatedAt <= current && p.CreatedAt >= dayago).Count();

                                    var week_info = details.Where(p => p.CreatedAt <= current && p.CreatedAt >= weekago);
                                    var act = (from info in week_info
                                                  group info by info.ChannelId into k
                                                  select new
                                                  {
                                                      Count = k.Count(),
                                                      ChannelName = k.Select(y => y.ChannelName).First()
                                                  });
                                    foreach(var a in act)
                                    {
                                        if(a.Count > i)
                                        {
                                            i = a.Count;
                                            str = a.ChannelName;
                                        }
                                    }

                                    var day_info = week_info.Where(p => p.ChannelName == str);
                                    var act_day = (from h in day_info
                                                    group h by h.CreatedAt.Day into yy
                                                    select new
                                                    {
                                                        Day_Count = yy.Count(),
                                                        ActiveDay = yy.Select(uu => uu.CreatedAt).First()
                                                    });
                                    int d_count = 0;
                                    var init_day = DateTime.Today.Day;
                                    var init_month = DateTime.Today.Month;
                                    var init_year = DateTime.Today.Year;
                                    var init_dayofweek = DateTime.Today.DayOfWeek;
                                    foreach(var bb in act_day)
                                    {
                                        if(bb.Day_Count > d_count)
                                        {
                                            d_count = bb.Day_Count;
                                            init_day = bb.ActiveDay.Day;
                                            init_month = bb.ActiveDay.Month;
                                            init_year = bb.ActiveDay.Year;
                                            init_dayofweek = bb.ActiveDay.DayOfWeek;
                                        }
                                    }
                                    var act_time = (from v in day_info
                                                    where v.CreatedAt.Year == init_year && v.CreatedAt.Month == init_month && v.CreatedAt.Day == init_day
                                                    group v by v.CreatedAt.Hour into ii
                                                    select new
                                                    {
                                                        Time_Count = ii.Count(),
                                                        ActiveTime = ii.Select(rr => rr.CreatedAt).First()
                                                    });
                                    int t_count = 0;
                                    var init_hour = DateTime.Today.Hour;
                                    var init_minute = DateTime.Today.Minute;
                                    string kind = null;
                                    foreach(var q in act_time)
                                    {
                                        if(q.Time_Count > t_count)
                                        {
                                            t_count = q.Time_Count;
                                            init_hour = q.ActiveTime.Hour;
                                            init_minute = q.ActiveTime.Minute;
                                            kind = q.ActiveTime.ToString("tt");
                                        }
                                    }
                                    x.Value += 
                                    $"\t\t{"+"}\t{Format.Underline(user.Name)}{" "}{Format.Italics("user information :")}\n" +
                                    $"\t\t\t{"-"}\t{Math.Round((monthCount / 720.0), 2)}\t{"messages/hour"}{" "}{Format.Italics("(this Month)")}\n" +
                                    $"\t\t\t{"-"}\t{monthCount}\t{"messages"}{" "}{Format.Italics("(this Month)")}\n" +
                                    $"\t\t\t{"-"}\t{weekCount}\t{"messages"}{" "}{Format.Italics("(this Week)")}\n" +
                                    $"\t\t\t{"-"}\t{dayCount}\t{"messages"}{" "}{Format.Italics("(Today)")}\n" +
                                    $"\t\t\t{"-"}\t{"#"}{str}{" "}{Format.Italics("(Most Active Channel)")}\n" +
                                    $"\t\t\t{"-"}\t{init_dayofweek}{" ["}{init_day}-{init_month}-{init_year}{"] "}{Format.Italics("(Most Active Day on this week)")}\n" +
                                    $"\t\t\t{"-"}\t{init_hour}:{init_minute}{kind}{" "}{Format.Italics("(Most Active Hour on")}{" "}{Format.Underline(init_dayofweek.ToString())}{Format.Italics(")")}\n"; 
                                }
                            }
                            
                        };
                    }); 
                    
                
                return Context.ReplyAsync("", embed: builder);
            }
        }
    }
}
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    [Name("Developer")]
    [Summary("Testing module for Frank")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class GlobalStatisticsModule : ModuleBase<SocketCommandContext>
    {
  /*      [Command("testinfo")]
        [Summary("Unless you are Frank, you have no reason to use this.")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public Task GlobalDataAsync()
        {
            System.Console.WriteLine("\nSuccessfully in database table!!!!");

            using (var db = new LogDatabase())
            {
                var user_query = (from data in db.Messages
                             where !data.IsBot
                             group data by data.Name into stats
                             select new
                             {
                                 AuthorId = stats.Select(m => m.AuthorId).First(),
                                 Name = stats.Select(t => t.Name).First(),
                                 Count = stats.Count()
                             }).ToList();
                var numberOfMessags = db.Messages.Where(x => !x.IsBot).Count();
                var deleteMessages = db.Messages.Where(x => !x.IsBot && x.DeletedAt != null).Count();
                var guild_query = (from guild in db.Messages
                                   where !guild.IsBot
                                   group guild by guild.GuildId into g
                                   select new
                                   {
                                       GuildName = g.Select(t => t.GuildName).First(),
                                       GuildMessages = g.Count(),
                                       GuildId = g.Select(t => t.GuildId).First()
                                   }).ToList();
                var active_query = (from active in db.Messages
                                    where !active.IsBot
                                    group active by active.ChannelId into x
                                    select new
                                    {
                                        Count = x.Count(),
                                        GuildName = x.Select(y => y.GuildName).First(),
                                        GuildId = x.Select(y => y.GuildId).First(),
                                        ChannelName = x.Select(y => y.ChannelName).First()
                                    });
                var guild_id = Context.Guild.Id;
      //          var color_query = (from t1 in db.Gcolors join t2 in db.Colorinfos on t1.ColorId equals t2.Id where t1.GuildId == guild_id.ToLong()
        //                           select new { r = t2.RedParam, g = t2.GreenParam, b = t2.BlueParam });
                byte rColor = 0;
                byte gColor = 0;
                byte bColor = 0;
                if (color_query == null || color_query.ToList().Count <= 0)
                {
                    rColor = Convert.ToByte(127);
                    gColor = Convert.ToByte(127);
                    bColor = Convert.ToByte(127);
                }
                else
                {
                    rColor = Convert.ToByte(color_query.First().r);
                    gColor = Convert.ToByte(color_query.First().g);
                    bColor = Convert.ToByte(color_query.First().b);
                }
                var footer_query = db.Gfooters.First();
                var author_query = db.Authors.First();
                var thumbnail_query = db.Gthumbnails.First();
                var desc_query = db.Gdescriptions.First();

                /* var builder = new EmbedBuilder()
                     .WithTitle("Statistics Data")
                     .WithColor(new Color(rColor, gColor, bColor))
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
                         foreach (var t in user_query)
                         {
                             DateTime today = DateTime.Now;
                             double lifeTime = (today - db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Min(m => m.CreatedAt)).TotalHours;
                             DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
                             DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
                             DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
                             DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
                             int mCount = db.Messages.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth && !p.IsBot && p.Name.Equals(t.Name)).Count();
                             int wCount = db.Messages.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && !p.IsBot && p.Name.Equals(t.Name)).Count();
                             int dCount = db.Messages.Where(p => p.CreatedAt.Date == today.Date && !p.IsBot && p.Name.Equals(t.Name)).Count();
                             if (lifeTime != 0)
                             {
                                 index++;
                                 x.Value += $"{index}{")"}\t{Format.Underline(t.Name)}\n\t\t{Format.Italics("Total number of messages:")}\t{t.Count}\t{"messages"}\n\t\t" +
                                 $"{Format.Italics("Messages / hour lifetime:")}\t{Math.Round((t.Count / lifeTime), 2)}\t{"messages/hour"}\n\t\t" +
                                 $"{Format.Italics("Messages this D/W/M:")}\t{dCount}/{wCount}/{mCount}\t{"messages"}\n";
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
                             if (list != null)
                             {
                                 var users = from w in list group w by w.Name into y select new { Name = y.Select(p => p.Name).First() };
                                 guild_index++;
                                 x.Value += $"{Format.Italics("Guild Name :")}\t{guild_index}{")"}\t{Format.Underline(t.GuildName)}\n";
                                 foreach (var user in users)
                                 {
                                     int i = 0;
                                     string str = null;
                                     var details = list.Where(z => z.Name == user.Name);

                                     DateTime today = DateTime.Now;
                                     DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
                                     DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
                                     DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
                                     DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
                                     int mCnt = details.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth).Count();
                                     int wCnt = details.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).Count();
                                     int dCnt = details.Where(p => p.CreatedAt.Date == today.Date).Count();
                                     double time = (lastDayOfMonth - firstDayOfMonth).TotalHours;

                                     var week_info = details.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek);
                                     var act = (from info in week_info
                                                group info by info.ChannelId into k
                                                select new
                                                {
                                                    Count = k.Count(),
                                                    ChannelName = k.Select(y => y.ChannelName).First()
                                                });
                                     foreach (var a in act)
                                     {
                                         if (a.Count > i)
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
                                     foreach (var bb in act_day)
                                     {
                                         if (bb.Day_Count > d_count)
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
                                     foreach (var q in act_time)
                                     {
                                         if (q.Time_Count > t_count)
                                         {
                                             t_count = q.Time_Count;
                                             init_hour = q.ActiveTime.Hour;
                                             init_minute = q.ActiveTime.Minute;
                                             kind = q.ActiveTime.ToString("tt");
                                         }
                                     }
                                     System.Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>> {0}", Math.Round((mCnt / time), 2));
                                     x.Value +=
                                     $"\t\t{"+"}\t{Format.Underline(user.Name)}{" "}{Format.Italics("user information :")}\n" +
                                     //$"\t\t\t{"-"}\t{Math.Round((mCnt / time), 2)}\t{"messages/hour"}{" "}{Format.Italics("(this Month)")}\n" +
                                     $"\t\t\t{"-"}\t{mCnt}\t{"messages"}{" "}{Format.Italics("(this Month)")}\n" +
                                     $"\t\t\t{"-"}\t{wCnt}\t{"messages"}{" "}{Format.Italics("(this Week)")}\n" +
                                     $"\t\t\t{"-"}\t{dCnt}\t{"messages"}{" "}{Format.Italics("(Today)")}\n" +
                                     $"\t\t\t{"-"}\t{"#"}{str}{" "}{Format.Italics("(Most Active Channel)")}\n" +
                                     $"\t\t\t{"-"}\t{init_dayofweek}{" ["}{init_day}-{init_month}-{init_year}{"] "}{Format.Italics("(Most Active Day on this week)")}\n" +
                                     $"\t\t\t{"-"}\t{init_hour}:{init_minute}{kind}{" "}{Format.Italics("(Most Active Hour on")}{" "}{Format.Underline(init_dayofweek.ToString())}{Format.Italics(")")}\n";
                                 }
                             }

                         };
                     });*/

          /*      var builder = new EmbedBuilder()
                    .WithTitle("Statistics Data")
                    .WithColor(new Color(rColor, gColor, bColor))
                    .WithDescription(desc_query.Description)
                    .WithThumbnailUrl(thumbnail_query.ThumbNail)
                     .AddField(x =>
                     {
                         x.Name = $"-   Number of messages done in all channels of all guilds:";
                         x.Value = $"\t{numberOfMessags}\t{"messages"}";
                     })
                     .AddField(x =>
                     {
                         x.Name = $"-   Number of messages deleted in all channels of all guilds:";
                         x.Value = $"\t{deleteMessages}\t{"messages"}";
                     });
                builder.WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(author_query.AuthorIcon)
                .WithName(author_query.AuthorName)
                .WithUrl(author_query.AuthorUrl));
                // per guild
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
                    builder.AddField((f) => f.WithName("-" + "\t" + t.GuildName + "\t" + "Guild")
                    .WithValue(Format.Italics("Total Number of Guild Messages (lifetime):") + "\t" + t.GuildMessages + "\t" +
                    "messages\n" + Format.Italics("Most Active Channel:") + "\t" + "#" + aa));
                };
                // per user
                foreach (var t in user_query)
                {
                    DateTime today = DateTime.Now;
                    double lifeTime = (today - db.Messages.Where(p => p.Name.Equals(t.Name) && !p.IsBot).Min(m => m.CreatedAt)).TotalHours;
                    DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
                    DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
                    DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
                    DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
                    int mCount = db.Messages.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth && !p.IsBot && p.Name.Equals(t.Name)).Count();
                    int wCount = db.Messages.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek && !p.IsBot && p.Name.Equals(t.Name)).Count();
                    int dCount = db.Messages.Where(p => p.CreatedAt.Date == today.Date && !p.IsBot && p.Name.Equals(t.Name)).Count();
                    if (lifeTime != 0)
                    {
                        builder.AddField((f) => f.WithName("-" + "\t" + t.Name + "\t" + "User")
                        .WithValue(
                            Format.Italics("Total number of messages:") + "\t" + t.Count + "\t" + "messages\n" +
                            Format.Italics("Messages / hour lifetime:") + "\t" + Math.Round((t.Count / lifeTime), 2) + "\t" + "messages/hour\n" +
                            Format.Italics("Messages this D/W/M:") + "\t" + dCount + "/" + wCount + "/" + mCount + "\t" + "messages"
                            ));
                    }
                };
                //per guild per user
                builder.AddField(x =>
                 {
                     x.Name = $"Per Guild Per Users:";
                     int guild_index = 0;

                     foreach (var t in guild_query)
                     {
                         var list = db.Messages.Where(o => o.GuildId == t.GuildId && !o.IsBot).ToList();
                         if (list != null)
                         {
                             var users = from w in list group w by w.Name into y select new { Name = y.Select(p => p.Name).First() };
                             guild_index++;
                             x.Value += $"{Format.Italics("Guild Name :")}\t{guild_index}{")"}\t{Format.Underline(t.GuildName)}\n";
                             foreach (var user in users)
                             {
                                 int i = 0;
                                 string str = null;
                                 var details = list.Where(z => z.Name == user.Name);

                                 DateTime today = DateTime.Now;
                                 DateTime firstDayOfWeek = DateTimeExtensions.FirstDayOfWeek(today);
                                 DateTime lastDayOfWeek = DateTimeExtensions.LastDayOfWeek(today);
                                 DateTime firstDayOfMonth = DateTimeExtensions.FirstDayOfMonth(today);
                                 DateTime lastDayOfMonth = DateTimeExtensions.LastDayOfMonth(today);
                                 int mCnt = details.Where(p => p.CreatedAt <= lastDayOfMonth && p.CreatedAt >= firstDayOfMonth).Count();
                                 int wCnt = details.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek).Count();
                                 int dCnt = details.Where(p => p.CreatedAt.Date == today.Date).Count();
                                 double time = (lastDayOfMonth - firstDayOfMonth).TotalHours;

                                 var week_info = details.Where(p => p.CreatedAt <= lastDayOfWeek && p.CreatedAt >= firstDayOfWeek);
                                 var act = (from info in week_info
                                            group info by info.ChannelId into k
                                            select new
                                            {
                                                Count = k.Count(),
                                                ChannelName = k.Select(y => y.ChannelName).First()
                                            });
                                 foreach (var a in act)
                                 {
                                     if (a.Count > i)
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
                                 foreach (var bb in act_day)
                                 {
                                     if (bb.Day_Count > d_count)
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
                                 foreach (var q in act_time)
                                 {
                                     if (q.Time_Count > t_count)
                                     {
                                         t_count = q.Time_Count;
                                         init_hour = q.ActiveTime.Hour;
                                         init_minute = q.ActiveTime.Minute;
                                         kind = q.ActiveTime.ToString("tt");
                                     }
                                 }
                                 Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>> {0}", Math.Round((mCnt / time), 2));
                                 x.Value +=
                                 $"\t\t{"+"}\t{Format.Underline(user.Name)}{" "}{Format.Italics("user information :")}\n" +
                                 $"\t\t\t{"-"}\t{Math.Round((mCnt / time), 2)}\t{"messages/hour"}{" "}{Format.Italics("(this Month)")}\n" +
                                 $"\t\t\t{"-"}\t{mCnt}\t{"messages"}{" "}{Format.Italics("(this Month)")}\n" +
                                 $"\t\t\t{"-"}\t{wCnt}\t{"messages"}{" "}{Format.Italics("(this Week)")}\n" +
                                 $"\t\t\t{"-"}\t{dCnt}\t{"messages"}{" "}{Format.Italics("(Today)")}\n" +
                                 $"\t\t\t{"-"}\t{"#"}{str}{" "}{Format.Italics("(Most Active Channel)")}\n" +
                                 $"\t\t\t{"-"}\t{init_dayofweek}{" ["}{init_day}-{init_month}-{init_year}{"] "}{Format.Italics("(Most Active Day on this week)")}\n" +
                                 $"\t\t\t{"-"}\t{init_hour}:{init_minute}{kind}{" "}{Format.Italics("(Most Active Hour on")}{" "}{Format.Underline(init_dayofweek.ToString())}{Format.Italics(")")}\n";
                             }
                         }

                     };
                 });
                builder.WithFooter(new EmbedFooterBuilder().WithIconUrl(footer_query.FooterIcon).WithText(footer_query.FooterText)).WithTimestamp(DateTime.UtcNow);
                return ReplyAsync("", embed: builder);
            }
        } */
    } 
} 

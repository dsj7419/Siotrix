using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class SeenModule : ModuleBase<SocketCommandContext>
    {

        [Command("seen")]
        [Summary("Shows information about the when the inputted user was last seen.")]
        [Remarks("(Username)")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.User)]
        public async Task SeenAsync(SocketGuildUser target)
        {
            if(target == null || target.IsBot)
            {
                await ReplyAsync("Please enter a valid user.");
                    return; 
            }
            
            string last_seen = GetLastMessageTime(target);
            var name = $"{target.Username} was last seen on {last_seen}";

            if (last_seen == "No recorded message from user in this guild.")
            {
                name = $"{target.Username} - {last_seen}";
            }


            string join_date = String.Format("{0:dddd, MMMM d, yyyy}", target.JoinedAt?.DateTime ?? DateTime.Now);
            var days_old = Math.Round((DateTime.Now - (target.JoinedAt?.DateTime ?? DateTime.Now)).TotalDays, 0);
            string days_old_concat = null;
            if (days_old < 1) days_old = 1;
            if (days_old == 1)
            {
                days_old_concat = join_date + " | " + days_old + " day ago.";
            }
            else
            {
                days_old_concat = join_date + " | " + days_old + " days ago.";
            }
            var serverjoined = days_old_concat;

            if (IsSameRoleLevelOrHigher(Context.User as SocketGuildUser, target) && last_seen != "No recorded message from user in this guild.")
            {
                string lastmessage = GetLastMessage(target);
                var channelindex = GetLastChannel(target);
                var channelname = Context.Guild.Channels.FirstOrDefault(x => x.Id == (ulong)channelindex).Name;
                var description = $"Text: {lastmessage}\nChannel: #{channelname}";

                var embed = SeenEmbed(Context, target.GetAvatarUrl(), name, serverjoined, description);
                await ReplyAsync("", embed: embed);
            }
            else
            {
                var embed = SeenEmbed(Context, target.GetAvatarUrl(), name, serverjoined);
                await ReplyAsync("", embed: embed);
            }            

        }


        private string GetLastMessageTime(SocketUser target)
        {
            string last = "-";
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).ToList().Count > 0)
                    {
                        DateTime date = db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).Last().CreatedAt;
                        last = String.Format("{0:dddd, MMMM d, yyyy}", date);
                    }
                    else
                    {
                        last = "No recorded message from user in this guild.";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return last;
        }

        private string GetLastMessage(SocketUser target)
        {
            string lastmessage = "";
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).ToList().Count > 0)
                    {
                        lastmessage = db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).Last().Content;
                    } else
                    {
                        lastmessage = "I have not yet seen this person say anything.";
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return lastmessage;
        }

        private long GetLastChannel(SocketUser target)
        {
            long lastchannel = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    if (db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).ToList().Count > 0)
                    {
                        lastchannel = db.Messages.Where(p => !p.IsBot && p.AuthorId == target.Id.ToLong()).Last().ChannelId.Value;
                    }
                    else
                    {
                        lastchannel = 0;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }   
            
            return lastchannel;
        }

        private bool IsSameRoleLevelOrHigher(SocketGuildUser user, SocketGuildUser target)
        {
            if (user.Hierarchy >= target.Hierarchy)
                return true;
            else
                return false;
        }

        private static EmbedBuilder SeenEmbed(SocketCommandContext Context, string avatar, string name, string serverjoined, string description = null)
        {
            //Make the embed builder
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(avatar)
                .WithName(name));
            embed.WithColor(g_color);

            if (description != null)
            {
                embed.WithDescription(description);
            }

            embed.AddField(new EmbedFieldBuilder() { IsInline = true, Name = "Joined Server: ", Value = serverjoined });

            return embed;
        }
    }
}

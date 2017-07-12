using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Summary("Ban a user")]
        [Remarks(" @username [optional days] - bans a user permenantly if no argument given.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task BanAsync(SocketUser user, int prunedays = -1)
        {
            int prune = prunedays == -1 ? 0 : prunedays;
            await Context.Guild.AddBanAsync(user, prune);
            var case_id = CaseExtensions.GetCaseNumber(Context);
            await Context.Channel.SendMessageAsync("What is the reason for ban? Case #" + case_id.ToString());
        }

        [Command("tempban")]
        [Summary("Ban a user for X amount of time/days")]
        [Remarks(" @username <time> - eg. 1h or 2d - can use weeks days hours minutes.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task TempBanAsync(SocketUser user, [Remainder]TimeSpan duration)
        {
            var minutes = duration.TotalMinutes;
            await Context.Guild.AddBanAsync(user, (int)minutes);
            var case_id = CaseExtensions.GetCaseNumber(Context);
            await Context.Channel.SendMessageAsync("What is the reason for ban? Case #" + case_id.ToString());
        }

        [Command("unban")]
        [Summary("Unban a user if they have been banned from the guild.")]
        [Remarks(" @username#descriminator - Must use full user name with numbers after.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task UnBanAsync()
        {
            //await Context.Guild.RemoveBanAsync(user);
            var banned_data = Context.Guild.GetBansAsync() as IBan;
            Console.WriteLine(banned_data.User.Username);
            await Context.Channel.SendMessageAsync(banned_data.User.Username);
        }
    }
}

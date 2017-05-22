﻿using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("kick"), Alias("ki")]
    [Summary("Kicks user from guild. May add a time amount before they are allowed back.")]
    public class KickModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Summary("Will instantly kick user from guild.")]
        [Remarks("<@username> - @mention user name of user you want to kick.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task KickAsync(SocketGuildUser user)
        {
            await user.KickAsync();
            var case_id = CaseExtensions.GetCaseNumber(Context, "kick");
            await Context.Channel.SendMessageAsync("What is reason? Case #" + case_id.ToString());
        }

        [Command]
        [Summary("Will kick user for specified amount of time/days.")]
        [Remarks("<@username> <time> - can specify any time fram 1d, 2d, 1w, 1m, etc. **note** default is normal kick")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task KickAsync(SocketUser user, int prunedays = -1)
        {
            int prune = prunedays == -1 ? 0 : prunedays;
            await Context.Guild.AddBanAsync(user, prune);
            await Context.Guild.RemoveBanAsync(user);
            var case_id = CaseExtensions.GetCaseNumber(Context, "kick");
            await Context.Channel.SendMessageAsync("What is reason? Case #" + case_id.ToString());
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GynBot.Common.Attributes;
using GynBot.Common.Enums;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GynBot.Modules.Public
{
    [Name("Moderator Commands")]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("leave")]
        [Remarks("Instructs the bot to leave this Guild.")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in a server."); return; }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("say"), Alias("s")]
        [Remarks("Repeats what the user says.")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task Say([Remainder] string input)
        {
            await ReplyAsync(input);
        }

        [Command("kick")]
        [Remarks("Kick the specified user.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Kick([Remainder]SocketGuildUser user)
        {
            await ReplyAsync($"bye bye {user.Mention} :wave:");
            await user.KickAsync();
        }
    }
}
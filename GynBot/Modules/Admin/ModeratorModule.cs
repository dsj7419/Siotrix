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
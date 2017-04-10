using Discord;
using Discord.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    [Group("leave")]
    public class LeaveModule : ModuleBase<SocketCommandContext>
    {

        [Command]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in your guild."); return; }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Name("no-help")]
        [Command]
        [Summary("Instructs the bot to leave a Guild specified by a developer.")]
        [Remarks("<GuildID> <text> - include a brief reason why you are making the bot quit that guild.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task Leave(ulong ID, [Remainder] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");
            var client = Context.Client;
            var gld = Context.Client.GetGuild(ID) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();
            var embed = new EmbedBuilder();
            embed.Description = $"Hello, I've been instructed by my developers to leave this guild..\n**Reason: **{msg}";
            embed.Color = new Color(0, 0, 255);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = "http://img04.imgland.net/WyZ5FoM.png",
                Url = "https://discord.gg/e6sku22"
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync($"Message has been sent and I've left the guild! {gld.Name}");
        }
    }
}



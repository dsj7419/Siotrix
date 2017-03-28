using Discord;
using Discord.WebSocket;
using Siotrix.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;


namespace Siotrix.Discord.Utility.Modules
{
    [Name("Guild Owner Commands")]
    class GuildOwnerModule : ModuleBase<SocketCommandContext>
    {

        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [Remarks("leave")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await Context.ReplyAsync("This command can only be ran in your guild."); return; }
            await Context.ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("echo"), Alias("say")]
        [Summary("Echo's input into a specified channel.")]
        [Remarks("say #general I am alive!")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public void Say([Summary("Target channel")] ITextChannel channel, [Remainder, Summary("Text to echo")] string text)
        {
            (Context.Client.GetChannel(channel.Id) as SocketTextChannel)?.SendMessageAsync(text);
        }

        [Command("requesthelp"), Alias("summonowner", "reportbug"), RequireContext(ContextType.Guild)]
        [Summary("Gives bot developer an alert that something is wrong, and an invite to the guild to provide assitance. Please use this only as emergency"), Remarks("summonowner")]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task RequestOwnerAsync()
        {
            // var owner = Context.Client.GetUser(Configuration.Load().PConfigs.Owners[0]);
            await Context.ReplyAsync("Summoning a Siotrix Developer...");
            var loggingChannel = Context.Client.GetChannel(290397586798542848) as ITextChannel;
            var invite = await (Context.Channel as IGuildChannel).CreateInviteAsync(maxUses: 1);
            var summonMsg = "@" + $"developer :eye: You're being summoned by {Context.User} to {Context.Guild.Name}. \n\n{invite.Url}";
            await loggingChannel.SendMessageAsync(summonMsg);
        }
    }
}

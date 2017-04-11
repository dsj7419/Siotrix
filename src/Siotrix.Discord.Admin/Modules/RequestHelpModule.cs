using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;

namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class RequestHelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("requesthelp"), Alias("reportbug")]
        [Summary("Gives bot developer an alert that something is wrong. Please use this only as emergency.")]
        [Remarks("<text> - include a brief report")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task RequestOwnerAsync([Remainder] string report)
        {
            if (report == null) throw new Exception("Please include a summary report. Only for emergencies please.");
            // var owner = Context.Client.GetUser(Configuration.Load().PConfigs.Owners[0]);
            var devGuild = Context.Client.GetGuild(290395992107253761) as IGuild;
            var devChannel = Context.Client.GetChannel(290397586798542848) as ITextChannel;
            var devRole = devGuild.Roles.First(x => x.Name == "developer");
            var invite = await (Context.Channel as IGuildChannel).CreateInviteAsync(maxUses: 1);

            await ReplyAsync("Notifying a Siotrix Developer...An invite has been sent in case they need to speak with you.");

            var summonMsg = $"{devRole.Mention} :eye: {Context.User} from {Context.Guild.Name} submitted a report.\n\nReport: {report.Cap(2000)} \n\n{invite.Url}";
            await devChannel.SendMessageAsync(summonMsg);
        }
    }
}

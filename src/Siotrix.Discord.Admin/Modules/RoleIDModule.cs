using Discord.Commands;
using Siotrix.Discord.Attributes.Preconditions;
using System.Threading.Tasks;


namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class RoleIDModule : ModuleBase<SocketCommandContext>
    {
        [Command("RoleIDs")]
        [Summary("Gets the ID of all roles in the guild.")]
        [Remarks("RoleIDs")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task RoleIDs()
        {
            string message = null;
            foreach (var role in Context.Guild.Roles)
                message += $"{role.Name}: {role.Id}\n";
            var channel = await Context.User.CreateDMChannelAsync();
            await channel.SendMessageAsync(message);
            await ReplyAsync($"{Context.User.Mention}, all Role IDs have been DMed to you!");
        }
    }
}

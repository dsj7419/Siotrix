using Discord.Commands;
using System.Threading.Tasks;


namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class RoleIDModule : ModuleBase<SocketCommandContext>
    {
        [Command("RoleIDs")]
        [Summary("Gets the ID of all roles in the guild.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
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

using Discord;
using Siotrix.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord.Github
{
    [Group("git"), Alias("github")]
    [RequireUserPermission(ChannelPermission.ManageChannel)]
    [Remarks("Manage repositories configured to be used in the source and issue commands")]
    public class GitModule : ModuleBase<SocketCommandContext>
    {
        [Name("no-help")]
        [Command]
        [Remarks("View a list of all github repositories configured for this guild or channel")]
        public async Task GitAsync()
        {
            await Task.Delay(0);
        }

        [Command("addrepo"), Alias("addrepository")]
        [Remarks("Add a single repository to this guild or channel")]
        public async Task AddRepoAsync(string name)
        {
            await Task.Delay(0);
        }

        [Command("addrepos"), Alias("addrepositories")]
        [Remarks("Add multiple repositories to this guild or channel")]
        public async Task AddReposAsync(params string[] names)
        {
            await Task.Delay(0);
        }

        [Command("removerepo"), Alias("removerepository")]
        [Remarks("Remove a single repository from this guild or channel")]
        public async Task RemoveRepoAsync(string name)
        {
            await Task.Delay(0);
        }

        [Command("removerepos"), Alias("removerepositories")]
        [Remarks("Remove multiple repositories from this guild or channel")]
        public async Task RemoveReposAsync(params string[] names)
        {
            await Task.Delay(0);
        }
    }
}

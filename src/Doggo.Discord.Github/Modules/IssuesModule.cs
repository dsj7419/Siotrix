using Doggo.Commands;
using Octokit;
using System.Threading.Tasks;

namespace Doggo.Discord.Github
{
    [Group("issues")]
    public class IssuesModule : ModuleBase<SocketCommandContext>
    {
        private GitHubClient _client;

        protected override void BeforeExecute()
        {
            _client = new GitHubClient(new ProductHeaderValue("Doggo"));
            _client.Credentials = new Credentials(Configuration.Load().Tokens.Github);
        }

        [Command]
        public async Task IssueAsync(GithubEntity entity, string query)
        {
            await Task.Delay(0);
        }
    }
}

using Siotrix.Commands;
using Octokit;
using System.Threading.Tasks;

namespace Siotrix.Discord.Github
{
    [Group("issues")]
    public class IssuesModule : ModuleBase<SocketCommandContext>
    {
        private GitHubClient _client;

        protected override void BeforeExecute()
        {
            _client = new GitHubClient(new ProductHeaderValue("Siotrix"));
            _client.Credentials = new Credentials(Configuration.Load().Tokens.Github);
        }

        [Command]
        public async Task IssueAsync(GithubEntity entity, string query)
        {
            await Task.Delay(0);
        }
    }
}

using Discord;
using Doggo.Commands;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doggo.Discord.Github
{
    [Group("source")]
    public class OldSourceModule : ModuleBase<SocketCommandContext>
    {
        private const string _githubUrl = "https://github.com/Doggo";
        private GitHubClient _client;

        protected override void BeforeExecute()
        {
            _client = new GitHubClient(new ProductHeaderValue("Doggo"));
            _client.Credentials = new Credentials(Configuration.Load().Tokens.Github);
        }
        
        [Command]
        public Task SourceAsync()
            => Context.ReplyAsync(_githubUrl);

        [Command]
        public async Task SourceAsync(string query)
        {
            var request = new SearchCodeRequest(query)
            {
                Extension = "cs",
                Repos = new RepositoryCollection()
                {
                    "Doggo/Discord",
                    "Doggo/Core"
                }
            };

            var result = await _client.Search.SearchCode(request);

            var embed = CreateEmbed(result.Items.First(), result.Items.Skip(1));
            await Context.ReplyAsync("", embed);
        }

        private Embed CreateEmbed(SearchCode first, IEnumerable<SearchCode> remaining)
        {
            var builder = new EmbedBuilder();

            builder.Title = "Found";
            builder.Description = $"[{first.Path}]({first.HtmlUrl})";
            
            if (remaining.Count() > 0)
            {
                builder.AddField(x =>
                {
                    x.Name = "Related:";

                    var sbuilder = new StringBuilder();
                    foreach (var result in remaining.Take(3))
                        sbuilder.AppendLine($"[{result.Name}]({result.HtmlUrl})");

                    x.Value = sbuilder.ToString();
                });
            }

            return builder;
        }
    }
}

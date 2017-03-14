using Siotrix.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Github
{
    public class GithubEntityTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var slashCount = input.Count(x => x == '/' || x == '\\');

            if (slashCount == 0)
                return Task.FromResult(TypeReaderResult.FromSuccess(new GithubEntity(input, true)));
            else if (slashCount == 1)
                return Task.FromResult(TypeReaderResult.FromSuccess(new GithubEntity(input, false)));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "A repo can't have multiple `/`."));
        }
    }
}

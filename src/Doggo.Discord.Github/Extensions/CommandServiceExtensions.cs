using Doggo.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Github
{
    public static class CommandServiceExtensions
    {
        public static Task LoadGithubAsync(this CommandService service)
        {
            service.AddTypeReader(typeof(GithubEntity), new GithubEntityTypeReader());
            return service.AddModulesAsync(typeof(OldSourceModule).GetTypeInfo().Assembly);
        }
    }
}

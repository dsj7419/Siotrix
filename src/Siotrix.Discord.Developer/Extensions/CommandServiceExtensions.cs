using Siotrix.Commands;
using Siotrix.Discord.Developer.Modules;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Developer
{
    public static class CommandServiceExtensions
    {
        public static Task LoadDeveloperAsync(this CommandService service)
            => service.AddModulesAsync(typeof(DeveloperModule).GetTypeInfo().Assembly);
    }
}

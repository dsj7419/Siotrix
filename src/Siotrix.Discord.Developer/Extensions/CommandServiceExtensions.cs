using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Developer
{
    public static class CommandServiceExtensions
    {
        public static Task LoadDeveloperAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(GetInviteModule).GetTypeInfo().Assembly);
        }
    }
}
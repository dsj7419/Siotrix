using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Events
{
    public static class CommandServiceExtensions
    {
        public static Task LoadEventsAsync(this CommandService service)
        {
            return service.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
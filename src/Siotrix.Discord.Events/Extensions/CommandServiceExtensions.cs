using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Events
{
    public static class CommandServiceExtensions
    {
        public static Task LoadEventsAsync(this CommandService service)
            => service.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}

using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Utility
{
    public static class CommandServiceExtensions
    {
        public static Task LoadUtilityAsync(this CommandService service)
            => service.AddModulesAsync(typeof(PerformanceModule).GetTypeInfo().Assembly);
    }
}
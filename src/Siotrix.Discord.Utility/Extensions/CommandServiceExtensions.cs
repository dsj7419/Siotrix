using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Utility
{
    public static class CommandServiceExtensions
    {
        public static Task LoadUtilityAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(PerformanceModule).GetTypeInfo().Assembly);
        }
    }
}
using Siotrix.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Statistics
{
    public static class CommandServiceExtensions
    {
        public static Task LoadStatisticsAsync(this CommandService service)
            => service.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}

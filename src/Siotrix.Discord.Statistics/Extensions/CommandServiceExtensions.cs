using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Statistics
{
    public static class CommandServiceExtensions
    {
        public static Task LoadStatisticsAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(InfoModule).GetTypeInfo().Assembly);
        }
    }
}
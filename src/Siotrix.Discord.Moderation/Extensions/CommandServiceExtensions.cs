using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Moderation
{
    public static class CommandServiceExtensions
    {
        public static Task LoadModerationAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(BanModule).GetTypeInfo().Assembly);
        }
    }
}
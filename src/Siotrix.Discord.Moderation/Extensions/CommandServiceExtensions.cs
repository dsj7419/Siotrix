using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public static class CommandServiceExtensions
    {
        public static Task LoadModerationAsync(this CommandService service)
            => service.AddModulesAsync(typeof(BanModule).GetTypeInfo().Assembly);
    }
}

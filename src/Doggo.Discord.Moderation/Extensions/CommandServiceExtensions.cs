using Doggo.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Moderation
{
    public static class CommandServiceExtensions
    {
        public static Task LoadModerationAsync(this CommandService service)
            => service.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}

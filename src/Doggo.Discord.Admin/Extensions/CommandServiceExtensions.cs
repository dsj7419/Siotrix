using Doggo.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Admin
{
    public static class CommandServiceExtensions
    {
        public static Task LoadAdminAsync(this CommandService service)
            => service.AddModulesAsync(typeof(PerformanceModule).GetTypeInfo().Assembly);
    }
}

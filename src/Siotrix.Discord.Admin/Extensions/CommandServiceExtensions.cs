using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Admin
{
    public static class CommandServiceExtensions
    {
        public static Task LoadAdminAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(RequestHelpModule).GetTypeInfo().Assembly);
        }

        //=> service.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}
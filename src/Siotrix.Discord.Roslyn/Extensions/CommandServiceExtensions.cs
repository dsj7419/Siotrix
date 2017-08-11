using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Roslyn
{
    public static class CommandServiceExtensions
    {
        public static Task LoadRoslynAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(EvalModule).GetTypeInfo().Assembly);
        }
    }
}
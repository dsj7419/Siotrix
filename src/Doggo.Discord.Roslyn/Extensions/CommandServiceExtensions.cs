using Doggo.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Roslyn
{
    public static class CommandServiceExtensions
    {
        public static Task LoadRoslynAsync(this CommandService service)
            => service.AddModulesAsync(typeof(EvalModule).GetTypeInfo().Assembly);
    }
}

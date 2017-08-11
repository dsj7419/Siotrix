using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Audio
{
    public static class CommandServiceExtensions
    {
        public static Task LoadAudioAsync(this CommandService service)
        {
            return service.AddModulesAsync(typeof(AudioService).GetTypeInfo().Assembly);
        }
    }
}
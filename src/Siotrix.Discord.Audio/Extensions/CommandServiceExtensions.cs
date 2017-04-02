using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Audio
{
    public static class CommandServiceExtensions
    {
        public static Task LoadAudioAsync(this CommandService service)
            => service.AddModulesAsync(typeof(AudioService).GetTypeInfo().Assembly);
    }
}

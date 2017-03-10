using Doggo.Commands;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Audio
{
    public static class CommandServiceExtensions
    {
        public static Task LoadAudioAsync(this CommandService service)
            => service.AddModulesAsync(typeof(AudioService).GetTypeInfo().Assembly);
    }
}

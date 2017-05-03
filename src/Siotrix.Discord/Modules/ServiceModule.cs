using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
 /*   [Name("Developer")]
    [Group("services")]    
    [Summary("module manipulation and info for Siotrix (DEPRECIATED)")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class ServiceModule : ModuleBase
    {
        private ServiceManager _manager;
        private Configuration _config;

        public ServiceModule(ServiceManager manager)
        {
            _manager = manager;
            _config = Configuration.Load();
        }

        [Command("instance"), Priority(0)]
        [Summary("list description of a specific instance")]
        [Remarks(" (instance name) - name of a specific instance")]
        public Task ServicesAsync(string instance)
        {
            if (_config.Instance.ToString() != instance.ToLower())
                return Task.CompletedTask;

            var builder = new EmbedBuilder()
            {
                Title = $"Module configuration for {_config.Instance}",
                Description = _config.Modules.ToString()
            };
            return ReplyAsync("", true, builder);
        }
    } */
}

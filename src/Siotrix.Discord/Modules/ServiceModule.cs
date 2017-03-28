using Discord;
using Siotrix.Commands;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    [Name("no-help")]
    [Group("services"), RequireOwner]
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        private ServiceManager _manager;
        private Configuration _config;

        public ServiceModule(ServiceManager manager)
        {
            _manager = manager;
            _config = Configuration.Load();
        }

        [Name("no-help")]
        [Command, Priority(0)]
        public Task ServicesAsync(string instance)
        {
            if (_config.Instance.ToString() != instance.ToLower())
                return Task.CompletedTask;

            var builder = new EmbedBuilder()
            {
                Title = $"Module configuration for {_config.Instance}",
                Description = _config.Modules.ToString()
            };
            return Context.ReplyAsync("", builder);
        }
    }
}

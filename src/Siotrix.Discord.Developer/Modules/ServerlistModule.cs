using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Siotrix.Discord.Attributes.Preconditions;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    public class ServerlistModule : ModuleBase<SocketCommandContext>
    {
        [Command("serverlist")]
        [Summary("Lists all guilds Siotrix is servicing")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ServerlistAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var embed = new EmbedBuilder();
            foreach (SocketGuild guild in client.Guilds)
            {
                embed.AddField(x =>
                {
                    x.Name = $"{guild.Name} || {guild.Id}";
                    x.Value = $"Guild Owner: { guild.Owner} || { guild.OwnerId}\nGuild Members: {guild.MemberCount}";
                    x.IsInline = true;
                });
            }
            embed.Title = "=== Server List ===";
            embed.Color = new Color(0, 0, 255);
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = $"Total Guilds: {client.Guilds.Count.ToString()}",
                IconUrl = "http://img04.imgland.net/WyZ5FoM.png"
            };

            await ReplyAsync("", embed: embed);
        }
    }
}
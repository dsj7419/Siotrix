using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    public class ServerlistModule : ModuleBase<SocketCommandContext>
    {
        [Command("serverlist")]
        [Summary("Lists all guilds Siotrix is servicing")]
        [Remarks(" - no additional arguments needed")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ServerlistAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var embed = new EmbedBuilder();
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
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
            embed.Color = g_color;
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = $"Total Guilds: {client.Guilds.Count.ToString()}",
                IconUrl = SiotrixConstants.BOT_AVATAR
            };

            await ReplyAsync("", embed: embed);
        }
    }
}
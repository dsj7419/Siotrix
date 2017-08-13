using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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
            var client = Context.Client;
            var embed = new EmbedBuilder();
            var gColor = await Context.GetGuildColorAsync();
            foreach (var guild in client.Guilds)
                embed.AddField(x =>
                {
                    x.Name = $"{guild.Name} || {guild.Id}";
                    x.Value = $"Guild Owner: {guild.Owner} || {guild.OwnerId}\nGuild Members: {guild.MemberCount}";
                    x.IsInline = true;
                });
            embed.Title = "=== Server List ===";
            embed.Color = gColor;
            embed.Footer = new EmbedFooterBuilder
            {
                Text = $"Total Guilds: {client.Guilds.Count}",
                IconUrl = SiotrixConstants.BotAvatar
            };

            await ReplyAsync("", embed: embed);
        }
    }
}
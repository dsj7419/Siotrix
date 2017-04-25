using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Group("leaveguild")]
    [Summary("Instructs the bot to leave a Guild specified by a developer. This could could be temporary or permanent.")]
    [MinPermissions(AccessLevel.BotOwner)]
    public class LeaveGuildModule : ModuleBase<SocketCommandContext>
    {
        [Command("guildkick")]
        [Summary("Instructs the bot to leave a Guild specified by a developer.")]
        [Remarks("<GuildID> <text> - include a brief reason why you are making the bot quit that guild.")]
        public async Task LeaveGuildAsync(ulong ID, [Remainder] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(ID) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description = $"Hello, I've been instructed by my developers to leave this guild..\n**Reason: **{msg}";
            embed.Color = new Color(0, 0, 255);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = "http://img04.imgland.net/WyZ5FoM.png",
                Url = "https://discord.gg/e6sku22"
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync($"Message has been sent and I've left the guild! {gld.Name}");
        }

        [Command("banlist")]
        [Summary("Lists all guilds banned from Siotrix joining.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildOwner)]
        public async Task BanGuildAsync()
        {
            //TODO: develop guildbans list
            await ReplyAsync("Needs to be developed still");
        }

        [Command("guildban")]
        [Summary("Bans bot from guild, and makes it quit immediately.")]
        [Remarks("<GuildID> <text> - brief description why the ban is happening.")]
        public async Task BanGuildAsync(ulong ID, [Remainder] string msg)
        {
            //TODO: Must do ban code for leaveguild command
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");

            var client = Context.Client;
            var gld = Context.Client.GetGuild(ID) as IGuild;
            var ch = await gld.GetDefaultChannelAsync();

            var embed = new EmbedBuilder();
            embed.Description = $"The developers have banned me from this guild..\n**Reason: **{msg}";
            embed.Color = new Color(255, 0, 0);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Siotrix - click to join my discord!",
                IconUrl = "http://img04.imgland.net/WyZ5FoM.png",
                Url = "https://discord.gg/e6sku22"
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync($"Message has been sent and I've left the guild forever! {gld.Name}");
        }

        [Command("guildunban")]
        [Summary("Un-bans bot from guild, allowing guild to re-invite Siotrix.")]
        [Remarks("<Id> - ID number in the ban list of the guild you want to unban.")]
        public async Task UnbanGuildAsync(int ID)
        {
            //TODO: Must develop unban code for leaveguild
            await ReplyAsync("Needs to be developed still.");
        }
    }
}
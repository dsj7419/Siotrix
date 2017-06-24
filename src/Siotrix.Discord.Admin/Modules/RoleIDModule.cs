using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Siotrix.Discord.Admin
{
    [Name("Admin")]
    public class RoleIDModule : ModuleBase<SocketCommandContext>
    {
        [Command("RoleIDs")]
        [Summary("Gets the ID of all roles in the guild.")]
        [Remarks(" - no additional arguments needed")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task RoleIDs()
        {
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            /*  string message = null;
              foreach (var role in Context.Guild.Roles)
                  message += $"{role.Name}: {role.Id}\n";
              var channel = await Context.User.GetOrCreateDMChannelAsync();
              await channel.SendMessageAsync(message);
              await ReplyAsync($"{Context.User.Mention}, all Role IDs have been DMed to you!"); */

            try
            {
                var eb = new EmbedBuilder()
                {
                    Color = g_color,
                    Title = $"Roles in {Context.Guild.Name}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Description = ""
                };

                foreach (var r in Context.Guild.Roles.OrderByDescending(r => r.Position))
                {
                    eb.Description += $"{r.Position}. {r.Name}: {r.Id}\n";
                }
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}

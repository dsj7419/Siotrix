using Discord;
using Discord.Commands;
using Discord.Addons.EmojiTools;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    public class ForgiveModule : ModuleBase<SocketCommandContext>
    {
        private bool DeleteWarningUser(long userId, long guildId)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gwarningusers.Where(x => x.UserId == userId && x.GuildId == guildId);
                    if (result.Any())
                    {
                        db.Gwarningusers.RemoveRange(result);
                        is_success = true;
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        [Command("Forgive")]
        [Summary("Forgive a user from ALL of their previous warnings.")]
        [Remarks(" - No additional arguments needed.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task ForgiveAsync(SocketGuildUser user)
        {
            var success = DeleteWarningUser(user.Id.ToLong(), Context.Guild.Id.ToLong());
            if (success)
                await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
            else
                await ReplyAsync("That person is not found!");
        }
    }
}
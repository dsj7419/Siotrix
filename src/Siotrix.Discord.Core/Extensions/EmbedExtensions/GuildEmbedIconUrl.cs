using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedIconUrl
    {
        public static async Task<DiscordGuildAvatar> GetGuildIconUrlAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildAvatar();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gavatars.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildAvatarAsync(context, SiotrixConstants.BotAvatar);
                        val = await db.Gavatars.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildAvatarAsync(SocketCommandContext context, string avatar)
        {
            var val = new DiscordGuildAvatar(context.Guild.Id.ToLong(), avatar);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gavatars.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildAvatar(DiscordGuildAvatar discordGuildAvatar, string avatar)
        {
            discordGuildAvatar.SetGuildAvatar(avatar);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gavatars.Update(discordGuildAvatar);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class GuildEmbedDescription
    {
        public static async Task<DiscordGuildDescription> GetGuildDescriptionAsync(this SocketCommandContext context)
        {
            var val = new DiscordGuildDescription();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gdescriptions.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    if (val == null)
                    {
                        await CreateDiscordGuildDescriptionAsync(context, context.Guild.Name);
                        val = await db.Gdescriptions.FirstOrDefaultAsync(p => p.GuildId == context.Guild.Id.ToLong());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task CreateDiscordGuildDescriptionAsync(SocketCommandContext context, string desc)
        {
            var val = new DiscordGuildDescription(context.Guild.Id.ToLong(), desc);
            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gdescriptions.AddAsync(val);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task SetGuildDescription(DiscordGuildDescription discordGuildDescription, string desc)
        {
            discordGuildDescription.SetGuildDescription(desc);
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gdescriptions.Update(discordGuildDescription);
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

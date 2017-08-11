using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class TagExtensions
    {
        /// <summary> Get a tag by it's id </summary>
        public static async Task<Tag> GetTagAsync(ulong id)
        {
            var val = new Tag();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Tags.FirstOrDefaultAsync(x => x.Id == id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        /// <summary> Get a tag in the specified guild by name </summary>
        public static async Task<Tag> GetTagAsync(string name, IGuild guild)
        {
            var val = new Tag();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Tags.FirstOrDefaultAsync(
                        x => x.Name.Contains(name.ToLower()) && x.GuildId == guild.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        /// <summary> Get all tags associated with the specified guild </summary>
        public static async Task<IEnumerable<Tag>> GetTagsAsync(IGuild guild)
        {
            IEnumerable<Tag> val = new List<Tag>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Tags.Where(x => x.GuildId == guild.Id.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        /// <summary> Get all tags associated with the specified guild and user </summary>
        public static async Task<IEnumerable<Tag>> GetTagsAsync(IGuild guild, IUser user)
        {
            IEnumerable<Tag> val = new List<Tag>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Tags.Where(x => x.GuildId == guild.Id.ToLong() && x.OwnerId == user.Id.ToLong())
                        .ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        /// <summary> Get the total usage of a tag </summary>
        public static async Task<int> CountLogsAsync(ulong id)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(x => x.TagId == id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Get the total usage of a tag for a user </summary>
        public static async Task<int> CountLogsAsync(ulong id, IUser user)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(x => x.TagId == id.ToLong() && x.UserId == user.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Get the total usage of a tag for a channel </summary>
        public static async Task<int> CountLogsAsync(ulong id, IChannel channel)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(
                        x => x.TagId == id.ToLong() && x.ChannelId == channel.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Get the total usage of all tags for a user </summary>
        public static async Task<int> CountLogsAsync(IUser user)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(x => x.UserId == user.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Get the total usage of all tags for a channel </summary>
        public static async Task<int> CountLogsAsync(IChannel channel)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(x => x.ChannelId == channel.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Get the total usage of all tags for a guild </summary>
        public static async Task<int> CountLogsAsync(IGuild guild)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.TagsLogs.CountAsync(x => x.GuildId == guild.Id.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        /// <summary> Check if the specified tag has been executed recently </summary>
        public static async Task<bool> IsDupeExecutionAsync(ulong id)
        {
            var isDupe = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    isDupe = await db.TagsLogs.AnyAsync(
                        x => x.TagId == id.ToLong() && x.Timestamp.AddSeconds(30) >= DateTime.UtcNow);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return isDupe;
        }


        /// <summary> Find tags similar to the specified name </summary>
        public static async Task<IEnumerable<Tag>> FindTagsAsync(string name, IGuild guild, int stop = 3,
            int tolerance = 5)
        {
            return (await GetTagsAsync(guild))
                .ToDictionary(x => x, x => x.Name
                    .Select(y => MathHelper.GetStringDistance(x.Name, name))
                    .Sum())
                .OrderBy(x => x.Value)
                .Select(x => x.Key)
                .Take(stop);
        }

        /// <summary> Add a new log for the specified tag </summary>
        public static async Task AddLogAsync(Tag tag, SocketCommandContext context)
        {
            var log = new TagLog(tag.Id, context.Guild.Id.ToLong(), context.Channel.Id.ToLong(),
                context.User.Id.ToLong());

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.TagsLogs.AddAsync(log);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary> Create a new tag </summary>
        public static async Task CreateTagAsync(string name, string content, SocketCommandContext context)
        {
            var tag = new Tag(name, content, context.Guild.Id.ToLong(), context.User.Id.ToLong());

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Tags.AddAsync(tag);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary> Delete an existing tag </summary>
        public static async Task DeleteTagAsync(Tag tag)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Tags.Remove(tag);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary> Modify the content of an existing tag </summary>
        public static async Task ModifyTagAsync(Tag tag, string content)
        {
            tag.SetContent(content);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Tags.Update(tag);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary> Change the owner of a tag </summary>
        public static async Task SetOwnerAsync(Tag tag, IUser user)
        {
            tag.SetOwnerId(user.Id.ToLong());

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Tags.Update(tag);
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
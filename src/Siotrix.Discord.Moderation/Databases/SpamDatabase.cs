using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    public class SpamDatabase : DbContext
    {
        public DbSet<SpamMessage> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!Directory.Exists("cache"))
                Directory.CreateDirectory("cache");

            string datadir = Path.Combine(AppContext.BaseDirectory, "cache/spam.sqlite.db");
            optionsBuilder.UseSqlite($"Filename={datadir}");
        }

        public async Task<IEnumerable<SpamMessage>> GetMessagesLike(SpamMessage msg, int distance, int span)
        {
            var channelMessages = Messages.Where(x => x.ChannelId == msg.ChannelId);
            var similarMessages = channelMessages.Where(x => Levenshtein.GetDistance(x.Content, msg.Content) < distance);
            var chainedMessages = similarMessages.Where(x => similarMessages.Any(y => y.CreatedAt >= x.CreatedAt.AddSeconds(-span)));
            
            return await chainedMessages.ToListAsync();
        }
    }
}

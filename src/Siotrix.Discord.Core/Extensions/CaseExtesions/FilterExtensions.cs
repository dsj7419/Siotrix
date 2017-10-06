using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class FilterExtensions
    {
        public static async Task<DiscordGuildFilterList> GetFilteredWordAsync(ulong guildId, string name)
        {
            var val = new DiscordGuildFilterList();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gfilterlists.FirstOrDefaultAsync(
                        x => x.Word.Equals(name.ToLower()) && x.GuildId == guildId.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateFilteredWordAsync(ulong guildId, string name, int warnPoints)
        {
            var filteredWord = new DiscordGuildFilterList(guildId.ToLong(), name, warnPoints);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gfilterlists.AddAsync(filteredWord);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task DeleteFilteredWordAsync(DiscordGuildFilterList filteredWord)
        {
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gfilterlists.Remove(filteredWord);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<IEnumerable<DiscordGuildFilterList>> GetFilteredWordsAsync(ulong guildId)
        {
            IEnumerable<DiscordGuildFilterList> val = new List<DiscordGuildFilterList>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gfilterlists.Where(x => x.GuildId == guildId.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task ModifyFilterPointsAsync(DiscordGuildFilterList filteredWord, int warnPoints)
        {
            filteredWord.SetWarnPoints(warnPoints);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gfilterlists.Update(filteredWord);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<int> CountFilteredWordsAsync(ulong guildId)
        {
            var count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.Gfilterlists.CountAsync(x => x.GuildId == guildId.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        public static async Task ImportFilteredWords(string[] words, ulong guildId)
        {
            var val = await GetFilteredWordsAsync(guildId);

            if (val != null)
            {
                foreach (var filteredWord in val)
                {
                    await DeleteFilteredWordAsync(filteredWord);
                }
            }
            foreach (var word in words)
            {
                await CreateFilteredWordAsync(guildId, word, 0);
            }
        }

        public static async Task DeleteAllFilteredWords(ulong guildId)
        {
            var val = await GetFilteredWordsAsync(guildId);

            if (val != null)
            {
                foreach (var filteredWord in val)
                {
                    await DeleteFilteredWordAsync(filteredWord);
                }
            }
        }
    }
}
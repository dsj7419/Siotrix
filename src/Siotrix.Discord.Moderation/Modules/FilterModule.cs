using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("filter")]
    [Summary("A special guild-specific word filter.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class FilterModule : InteractiveModuleBase<SocketCommandContext>
    {        
      /*  private bool SaveAndUpdateFilterWord(string word, long guildId)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId && x.Word.Equals(word));
                    if (!result.Any())
                    {
                        var record = new DiscordGuildFilterList();
                        record.GuildId = guildId;
                        record.Word = word;
                        db.Gfilterlists.Add(record);
                    }
                    else
                    {
                        var data = result.First();
                        data.Word = word;
                        db.Gfilterlists.Update(data);
                    }
                    db.SaveChanges();
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        } */

 /*       private bool ImportFilterWord(string[] words, long guildId)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId);
                    if (result.Any())
                        db.Gfilterlists.RemoveRange(result);
                    foreach (var word in words)
                    {
                        var record = new DiscordGuildFilterList();
                        record.GuildId = guildId;
                        record.Word = word;
                        db.Gfilterlists.Add(record);
                    }
                    db.SaveChanges();
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        }

        private bool RemoveFilterWord(string word, long guildId)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId && x.Word.Equals(word));
                    if (result.Any())
                    {
                        db.Gfilterlists.RemoveRange(result);
                        isSuccess = true;
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        }

        private bool DeleteAllFilterWords(long guildId)
        {
            var isSuccess = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId);
                    if (result.Any())
                    {
                        db.Gfilterlists.RemoveRange(result);
                        isSuccess = true;
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return isSuccess;
        } */

        [Command("list")]
        [Summary("Receive a private message listing words filtered from this channel.")]
        [Remarks("- No additional arguments needed.")]
        public async Task FilterAsync()
        {
            var gColor = await Context.GetGuildColorAsync();           
            var filteredWords = await FilterExtensions.GetFilteredWordsAsync(Context.Guild.Id);

            if (!HasWords(Context.Guild, filteredWords)) return;

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex))
                .WithTitle($"Filtered Words for {Context.Guild}")
                .WithDescription(string.Join(", ", filteredWords.Select(x => $"{x.Word.ToString()}({x.WarnPoints})")));

            await MessageExtensions.DmUser(Context.User, embed: builder);
            await Task.Delay(1);
        }

        [Command("add")]
        [Summary("Add a new word to this channel's filter with the option to assign how many warning points are issued if a user breaks the filter rule (defaults to 0).")]
        [Remarks("(word) [number]")]
        public async Task AddAsync(string word, int warnPoints = 0)
        {
            var filteredWord = await FilterExtensions.GetFilteredWordAsync(Context.Guild.Id, word);
            if (Exists(filteredWord, word)) return;

            await FilterExtensions.CreateFilteredWordAsync(Context.Guild.Id, word, warnPoints);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("remove")]
        [Summary("Remove an existing word from this channel's filter.")]
        [Remarks("(word)")]
        public async Task RemoveAsync(string word)
        {
            var filteredWord = await FilterExtensions.GetFilteredWordAsync(Context.Guild.Id, word);

            if (NotExists(filteredWord, word)) return;

            await FilterExtensions.DeleteFilteredWordAsync(filteredWord);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("import")]
        [Summary("This will import a default list for your guild and remove any other words you have saved.")]
        [Remarks("- No additional arguments needed.")]
        public async Task ImportAsync()
        {
            await ReplyAsync(
                "📣 : **WARNING**! You are about to import the siotrix default filter! This will delete any filters you have added for your guild, are you sure you want to do this? (Yes or No)");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel);
            if (response.Content.ToUpper().Equals("YES") || response.Content.ToUpper().Equals("Y"))
            {
                await FilterExtensions.ImportFilteredWords(SiotrixConstants.BadWords, Context.Guild.Id);
                await ReplyAsync("📣 : Import is complete.");
            }
        }

        [Command("reset")]
        [Summary("This will move all words from your guild filter.")]
        [Remarks("- No additional arguments needed.")]
        public async Task ResetAsync()
        {
            await ReplyAsync(
                "📣 : **WARNING**! This will delete all filters you have added for your guild, are you sure you want to do this? (Yes or No)");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel);
            if (response.Content.ToUpper().Equals("YES") || response.Content.ToUpper().Equals("Y"))
            {
                await FilterExtensions.DeleteAllFilteredWords(Context.Guild.Id);
                await ReplyAsync("📣 : All filters have been deleted.");
            }
        }

        [Command("modify")]
        [Summary("Modify inputted words warning point value.")]
        [Remarks("(word) [number]")]
        public async Task ModifyAsync(string word, int warnPoints = 0)
        {
            var filteredWord = await FilterExtensions.GetFilteredWordAsync(Context.Guild.Id, word);

            if (NotExists(filteredWord, word)) return;

            await FilterExtensions.ModifyFilterPointsAsync(filteredWord, warnPoints);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("modifyall")]
        [Summary("Modify inputted words warning point value.")]
        [Remarks("(word) [number]")]
        public async Task ModifyAllAsync(int warnPoints)
        {
            var filteredWords = await FilterExtensions.GetFilteredWordsAsync(Context.Guild.Id);

            if (!HasWords(Context.Guild, filteredWords)) return;

            foreach (var filteredWord in filteredWords)
            {
                await FilterExtensions.ModifyFilterPointsAsync(filteredWord, warnPoints);
            }
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        private bool Exists(DiscordGuildFilterList filteredWord, string name)
        {
            if (filteredWord != null)
            {
                var _ = ReplyAsync($"The filter, `{name}` already exists..");
                return true;
            }
            return false;
        }

        private bool NotExists(DiscordGuildFilterList filteredWord, string name)
        {
            if (filteredWord == null)
            {
                var _ = ReplyAsync($"The filter, `{name}` does not exist..");
                return true;
            }
            return false;
        }

        private bool HasWords(object obj, IEnumerable<DiscordGuildFilterList> filteredWords)
        {
            if (filteredWords.Count() == 0)
            {
                var _ = ReplyAsync($"{obj} currently has no filtered words.");
                return false;
            }
            return true;
        }
    }
}
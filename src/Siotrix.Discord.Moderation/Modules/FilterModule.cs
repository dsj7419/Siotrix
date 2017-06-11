﻿using Discord;
using Discord.Commands;
using Discord.Addons.InteractiveCommands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("filter")]
    [Summary("A special guild-specific word and phrase filter.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class FilterModule : InteractiveModuleBase<SocketCommandContext>
    {
        string[] bad_words = new string[] {"shit", "fuck", "nigger", "rape", "sex", "coon", "pig" };

        private bool SaveAndUpdateFilterWord(string word, long guild_id)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guild_id && x.Word.Equals(word));
                    if (!result.Any())
                    {
                        var record = new DiscordGuildFilterList();
                        record.GuildId = guild_id;
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
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        private bool ImportFilterWord(string[] words, long guild_id)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guild_id);
                    if (result.Any())
                    {
                        db.Gfilterlists.RemoveRange(result);
                    }
                    foreach (var word in words)
                    {
                        var record = new DiscordGuildFilterList();
                        record.GuildId = guild_id;
                        record.Word = word;
                        db.Gfilterlists.Add(record);
                    }
                    db.SaveChanges();
                    is_success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return is_success;
        }

        private bool RemoveFilterWord(string word, long guildId)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guildId && x.Word.Equals(word));
                    if (result.Any())
                    {
                        db.Gfilterlists.RemoveRange(result);
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

        private bool DeleteAllFilterWords(long guild_id)
        {
            bool is_success = false;
            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gfilterlists.Where(x => x.GuildId == guild_id);
                    if (result.Any())
                    {
                        db.Gfilterlists.RemoveRange(result);
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

        [Command]
        [Remarks("Receive a private message listing words filtered from this channel.")]
        public async Task FilterAsync()
        {
            await Task.Delay(1);
        }

        [Command("add")]
        [Remarks("Add a new word to this channel's filter.")]
        public async Task AddAsync(string word)
        {
            var success = SaveAndUpdateFilterWord(word, Context.Guild.Id.ToLong());
            if (success)
                await ReplyAsync("👍");
        }

        [Command("remove")]
        [Remarks("Remove an existing word from this channel's filter.")]
        public async Task RemoveAsync(string word)
        {
            var success = RemoveFilterWord(word, Context.Guild.Id.ToLong());
            if (success)
                await ReplyAsync("👍");
            else
                await ReplyAsync("📣 : Not Found like that word!");
        }

        [Command("import")]
        [Remarks("========")]
        public async Task ImportAsync()
        {
            await ReplyAsync("📣 : **WARNING**! You are about to import the siotrix default filter! This will delete any filters you have added for your guild, are you sure you want to do this? (Yes or No)");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel);
            if(response.Content.ToUpper().Equals("YES") || response.Content.ToUpper().Equals("Y"))
            {
                var success = ImportFilterWord(bad_words, Context.Guild.Id.ToLong());
                if (success)
                    await ReplyAsync("📣 : Siotrix imports filter and confirms with user.");
            }
        }

        [Command("reset")]
        [Remarks("========")]
        public async Task ResetAsync()
        {
            await ReplyAsync("📣 : **WARNING**! This will delete all filters you have added for your guild, are you sure you want to do this? (Yes or No)");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel);
            if (response.Content.ToUpper().Equals("YES") || response.Content.ToUpper().Equals("Y"))
            {
                var success = DeleteAllFilterWords(Context.Guild.Id.ToLong());
                if (success)
                    await ReplyAsync("📣 : All filters has been deleted.");
            }
        }
    }
}

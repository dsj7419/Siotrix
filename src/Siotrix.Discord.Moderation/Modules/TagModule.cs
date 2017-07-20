using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("tag")]
    [Summary("Create and manage tags for this guild")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        private readonly TagManager _manager;

        public TagModule(TagManager manager)
        {
            _manager = manager;
        }

        [Command("name"), Priority(0)]
        [Summary("Show the tag associated with the specified name")]
        [Remarks("<tagname> - Name of tag to show.")]
        public async Task TagAsync([Remainder]string name)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (await _manager.IsDupeExecutionAsync((ulong)tag.Id)) return;
            if (NotExists(tag, name)) return;

            var _ = _manager.AddLogAsync(tag, Context);
            await ReplyAsync($"{tag.Aliases.First()}: {tag.Content}");
        }

        [Priority(10)]
        [Command("create"), Alias("new", "add")]
        [Summary("Create a new tag for this guild")]
        [Remarks("<tagname> <content for tag>")]
        public async Task CreateAsync(string name, [Remainder]string content)
        {
            Console.WriteLine("first", name, content);

            var tag = await _manager.GetTagAsync(name, Context.Guild);

            Console.WriteLine("second");

            if (Exists(tag, name)) return;

            Console.WriteLine("third");

            await _manager.CreateTagAsync(name, content, Context);

            Console.WriteLine("fourth");

            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("delete"), Alias("remove")]
        [Summary("Delete an existing tag from this guild")]
        [Remarks("<tagname> - Name of tag to delete.")]
        public async Task DeleteAsync(string name)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name)) return;

            await _manager.DeleteTagAsync(tag);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("modify"), Alias("edit", "change")]
        [Summary("Modify an existing tag from this guild")]
        [Remarks("<tagname> <edited content for tag>")]
        public async Task ModifyAsync(string name, [Remainder]string content)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name) || !IsOwner(tag)) return;

            await _manager.ModifyTagAsync(tag, content);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("setowner"), Alias("donate", "give")]
        [Summary("Change the owner of a tag in this guild")]
        [Remarks("<tagname> <usernamae>")]
        public async Task SetOwnerAsync(string name, [Remainder]SocketUser user)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name) || !IsOwner(tag)) return;

            await _manager.SetOwnerAsync(tag, user);
            await ReplyAsync(":thumbsup:");
        }

        [Priority(10)]
        [Command("alias"), Alias("addalias")]
        [Summary("Add a new alias to the specified tag")]
        [Remarks("<tagname> <alias name to add for tag>")]
        public async Task AddAliasAsync(string name, params string[] aliases)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name)) return;

            await _manager.AddAliasesAsync(tag, aliases);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("unalias"), Alias("removealias")]
        [Summary("Remove an existing alias from the specified tag")]
        [Remarks("<tagname> <alias name to remove from tag>")]
        public async Task RemoveAliasAsync(string name, params string[] aliases)
        {
            var tag = await _manager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name) || !IsOwner(tag)) return;

            await _manager.RemoveAliasesAsync(tag, aliases);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        private bool IsOwner(Tag tag)
        {
            if (tag.OwnerId != Context.User.Id)
            {
                var _ = ReplyAsync("You are not the owner of this tag");
                return false;
            }
            return true;
        }

        private bool Exists(Tag tag, string name)
        {
            if (tag != null)
            {
                var _ = ReplyAsync($"The tag `{name}` already exists");
                return true;
            }
            return false;
        }

        private bool NotExists(Tag tag, string name)
        {
            if (tag == null)
            {
                var _ = ReplySuggestionsAsync(name);
                return true;
            }
            return false;
        }

        private async Task ReplySuggestionsAsync(string name)
        {
            string msg = $"The tag `{name}` does not exist";
            var tags = await _manager.FindTagsAsync(name, Context.Guild);

            if (tags.Count() != 0)
                msg += $"\nDid you mean: {string.Join(", ", tags.Select(x => x.Aliases.First()))}";

            await ReplyAsync(msg);
        }
    }
}
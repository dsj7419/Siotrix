using Discord;
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

        [Command, Priority(0)]
        [Summary("Show the tag associated with the specified name")]
        [Remarks("<tagname> - Name of tag to show.")]
        public async Task TagAsync([Remainder]string name)
        {
            var tag = await TagManager.GetTagAsync(name, Context.Guild as IGuild);
            if (await TagManager.IsDupeExecutionAsync((ulong)tag.Id)) return;
            if (NotExists(tag, name)) return;
            
            var _ = TagManager.AddLogAsync(tag, Context);

            await ReplyAsync($"{tag.Name}: {tag.Content}");
        }

        [Priority(10)]
        [Command("create"), Alias("new", "add")]
        [Summary("Create a new tag for this guild")]
        [Remarks("<tagname> <content for tag>")]
        public async Task CreateAsync(string name, [Remainder]string content)
        {

            var tag = await TagManager.GetTagAsync(name, Context.Guild);
            if (Exists(tag, name)) return;

            await TagManager.CreateTagAsync(name, content, Context);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("delete"), Alias("remove")]
        [Summary("Delete an existing tag from this guild")]
        [Remarks("<tagname> - Name of tag to delete.")]
        public async Task DeleteAsync(string name)
        {
            var tag = await TagManager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name)) return;

            await TagManager.DeleteTagAsync(tag);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("modify"), Alias("edit", "change")]
        [Summary("Modify an existing tag from this guild")]
        [Remarks("<tagname> <edited content for tag>")]
        public async Task ModifyAsync(string name, [Remainder]string content)
        {
            var tag = await TagManager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name) || !IsOwner(tag)) return;

            await TagManager.ModifyTagAsync(tag, content);
            await ReplyAsync(SiotrixConstants.BOT_SUCCESS);
        }

        [Priority(10)]
        [Command("setowner"), Alias("donate", "give")]
        [Summary("Change the owner of a tag in this guild")]
        [Remarks("<tagname> <usernamae>")]
        public async Task SetOwnerAsync(string name, [Remainder]SocketUser user)
        {
            var tag = await TagManager.GetTagAsync(name, Context.Guild);

            if (NotExists(tag, name) || !IsOwner(tag)) return;

            await TagManager.SetOwnerAsync(tag, user);
            await ReplyAsync(":thumbsup:");
        }

        private bool IsOwner(Tag tag)
        {
            if (tag.OwnerId.ToUlong() != Context.User.Id)
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
            var tags = await TagManager.FindTagsAsync(name, Context.Guild);

            if (tags.Count() != 0)
                msg += $"\nDid you mean: {string.Join(", ", tags.Select(x => x.Name.First()))}";

            await ReplyAsync(msg);
        }
    }
}
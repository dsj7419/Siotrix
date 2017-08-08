using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [Group("tags")]
    [Summary("Search and view information about available tags.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class TagsModule : ModuleBase<SocketCommandContext>
    {
        private readonly Random _random;

        public TagsModule(Random random)
        {
            _random = random;
        }

        [Command("listtags")]
        [Summary("View all available tags for this guild")]
        [Remarks("- No additional input required.")]
        public async Task TagsAsync()
        {
            Color g_color = GuildEmbedColorExtensions.GetGuildColor(Context);
            var tags = await TagExtensions.GetTagsAsync(Context.Guild);

            if (!HasTags(Context.Guild, tags)) return;

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithColor(g_color)
                .WithTitle($"Tags for {Context.Guild}")
                .WithDescription(string.Join(", ", tags.Select(x => x.Name.ToString())));

            await ReplyAsync("", embed: builder);
        }

        [Command("usertags")]
        [Summary("View all available tags for the specified user")]
        [Remarks("<username>")]
        public async Task TagsAsync([Remainder]SocketUser user)
        {
            var tags = await TagExtensions.GetTagsAsync(Context.Guild, user);

            if (!HasTags(user, tags)) return;

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithTitle($"Tags for {user}")
                .WithDescription(string.Join(", ", tags.Select(x => x.Name.ToString())));

            await ReplyAsync("", embed: builder);
        }

        [Command("random")]
        [Summary("Show a random tag from this guild")]
        [Remarks("- No additional input required.")]
        public async Task RandomAsync()
        {
            var tags = await TagExtensions.GetTagsAsync(Context.Guild);

            if (!HasTags(Context.Guild, tags)) return;

            var selected = SelectRandom(tags);
            var _ = TagExtensions.AddLogAsync(selected, Context);
            await ReplyAsync($"{selected.Name.ToString()}: {selected.Content}");
        }

        [Command("randomusertag")]
        [Summary("Show a random tag from the specified user")]
        [Remarks("<username>")]
        public async Task RandomAsync([Remainder]SocketUser user)
        {
            var tags = await TagExtensions.GetTagsAsync(Context.Guild, user);

            if (!HasTags(Context.Guild, tags)) return;

            var selected = SelectRandom(tags);
            var _ = TagExtensions.AddLogAsync(selected, Context);
            await ReplyAsync($"{selected.Name.ToString()}: {selected.Content}");
        }

        [Command("taginfo"), Priority(0)]
        [Summary("Get information about the specified tag")]
        [Remarks("<tagname>")]
        public async Task InfoAsync([Remainder]string name)
        {
            var tag = await TagExtensions.GetTagAsync(name, Context.Guild);
            var author = Context.Guild.GetUser(tag.OwnerId.ToUlong());
            var count = await TagExtensions.CountLogsAsync((ulong)tag.Id);

            var builder = new EmbedBuilder()
                .WithFooter(x => x.Text = "Last Updated")
                .WithTimestamp(tag.UpdatedAt)
                .AddInlineField("Owner", author.Mention)
                .AddInlineField("Name", string.Join(", ", tag.Name))
                .AddInlineField("Uses", count);

            builder.WithAuthor(x =>
            {
                x.Name = author.ToString();
                x.IconUrl = author.GetAvatarUrl();
            });

            await ReplyAsync("", embed: builder);
        }

        [Command("usertaginfo"), Priority(10)]
        [Summary("Get information about the specified tag in relation to the specified user")]
        [Remarks("<tagname> <username>")]
        public async Task InfoAsync(string name, [Remainder]SocketUser user)
        {
            var tag = await TagExtensions.GetTagAsync(name, Context.Guild);
            var count = await TagExtensions.CountLogsAsync((ulong)tag.Id, user);

            await ReplyAsync($"{tag.Name.ToString()} has been used {count} time(s)");
        }

        [Command("channeltaginfo"), Priority(10)]
        [Summary("Get information about the specified tag in relation to the specified channel")]
        [Remarks("<tagname> <channelname>")]
        public async Task InfoAsync(string name, [Remainder]SocketChannel channel)
        {
            var tag = await TagExtensions.GetTagAsync(name, Context.Guild);
            var count = await TagExtensions.CountLogsAsync((ulong)tag.Id, channel);

            await ReplyAsync($"{tag.Name.ToString()} has been used {count} time(s)");
        }

        [Command("userinfo"), Priority(5)]
        [Summary("Get information about tags for the specified user")]
        [Remarks("<username>")]
        public async Task InfoAsync([Remainder]SocketUser user)
        {
            var count = await TagExtensions.CountLogsAsync(user);
            await ReplyAsync($"{user} executed tags {count} time(s)");
        }

        [Command("channelinfo"), Priority(5)]
        [Summary("Get information about tags for the specified channel")]
        [Remarks("<channelname>")]
        public async Task InfoAsync([Remainder]SocketChannel channel)
        {
            var count = await TagExtensions.CountLogsAsync(channel);
            await ReplyAsync($"{channel} executed tags {count} time(s)");
        }

        private bool HasTags(object obj, IEnumerable<Tag> tags)
        {
            if (tags.Count() == 0)
            {
                var _ = ReplyAsync($"{obj} currently has no tags.");
                return false;
            }
            return true;
        }

        private Tag SelectRandom(IEnumerable<Tag> tags)
        {
            var index = _random.Next(0, tags.Count());
            var selected = tags.ElementAt(index);
            return selected;
        }
    }
}
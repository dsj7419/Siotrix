using Discord;
using Doggo.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Doggo.Discord.Admin
{
    [Group("settings"), Alias("set")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        public Task AvatarAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.GetAvatarUrl());

        [Command("avatar"), RequireOwner]
        public async Task AvatarAsync(Uri url)
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), url);

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var stream = await response.Content.ReadAsStreamAsync();

                var self = Context.Client.CurrentUser;
                await self.ModifyAsync(x =>
                {
                    x.Avatar = new Image(stream);
                });
                await Context.ReplyAsync("👍");
            }
        }

        [Command("username")]
        public Task UsernameAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.ToString());

        [Command("username"), RequireOwner]
        public async Task UsernameAsync([Remainder]string name)
        {
            var self = Context.Client.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Username = name;
            });
            await Context.ReplyAsync("👍");
        }

        [Command("nickname")]
        public Task NicknameAsync()
            => Context.ReplyAsync(Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.ToString());

        [Command("nickname"), RequireOwner]
        public async Task NicknameAsync([Remainder]string name)
        {
            var self = Context.Guild.CurrentUser;
            await self.ModifyAsync(x =>
            {
                x.Nickname = name;
            });
            await Context.ReplyAsync("👍");
        }

        [Command("activity")]
        public Task ActivityAsync()
            => Context.ReplyAsync($"Playing: {Context.Client.CurrentUser.Game.ToString()}");

        [Command("activity"), RequireOwner]
        public async Task ActivityAsync([Remainder]string activity)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetGameAsync(activity);
            await Context.ReplyAsync("👍");
        }

        [Command("status")]
        public Task StatusAsync()
            => Context.ReplyAsync(Context.Client.CurrentUser.Status.ToString());

        [Command("status"), RequireOwner]
        public async Task UsernameAsync(UserStatus status)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetStatusAsync(status);
            await Context.ReplyAsync("👍");
        }
    }
}

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Siotrix.Discord.Developer
{
    [Name("Developer")]
    [Group("siotrix")]
    [Alias("sio")]
    [Summary("Various Siotrix property Settings.")]
    public class SiotrixModule : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        [Summary("Will list bots current avatar.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task AvatarAsync()
        {
            return ReplyAsync(Context.Client.CurrentUser.GetAvatarUrl());
        }

        [Command("avatar")]
        [Summary("Will set bots avatar.")]
        [Remarks(
            "<url> - url of picture to assign as bot avatar **note** using keyword reset will reset to Siotrix avatar.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AvatarAsync(Uri url)
        {
            if (url.ToString().Equals("reset"))
                url = new Uri(SiotrixConstants.BotAvatar);
            var request = new HttpRequestMessage(new HttpMethod("GET"), url);

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var stream = await response.Content.ReadAsStreamAsync();

                var self = Context.Client.CurrentUser;
                await self.ModifyAsync(x => { x.Avatar = new Image(stream); });
                await ReplyAsync(SiotrixConstants.BotSuccess);
            }
        }

        [Command("authoricon")]
        [Summary("Will list bots current author icon.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorIconAsync()
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
            await ReplyAsync(val.AuthorIcon);
        }

        [Command("authoricon")]
        [Summary("Will set bots author icon.")]
        [Remarks(
            "<url> - url of picture to assign as bot author icon **note** using keyword reset will reset to Siotrix icon.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorIconAsync(Uri url)
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();

            if (url.ToString().Trim().Equals("reset"))
            {
                await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorIcon(val, SiotrixConstants.BotAuthorIcon);
                await ReplyAsync(SiotrixConstants.BotSuccess);
                return;
            }

            await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorIcon(val, url.ToString());
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("authorurl")]
        [Summary("Will list bots current author url.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorUrlAsync()
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
            await ReplyAsync(val.AuthorUrl);
        }

        [Command("authorurl")]
        [Summary("Will set bots author url.")]
        [Remarks(
            "<url> - This links author name as a hyperlink. **note** using keyword reset will reset to Siotrix url.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorUrlAsync(Uri url)
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();

            if (url.ToString().Trim().Equals("reset"))
            {
                await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorUrl(val, SiotrixConstants.DiscordInv);
                await ReplyAsync(SiotrixConstants.BotSuccess);
                return;
            }

            await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorUrl(val, url.ToString());
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("authorname")]
        [Summary("Will list bots current author name.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorNameAsync()
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();
            await ReplyAsync(val.AuthorName);
        }

        [Command("authorname")]
        [Summary("Will set bots current author name.")]
        [Remarks(
            "<name> - This defaults to your guild name. **note** You can use reset as the parameter to reset back to your guild name.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task AuthorNameAsync([Remainder] string txt)
        {
            var val = await SiotrixEmbedAuthorExtensions.GetSiotrixAuthorAsync();

            if (txt.Equals("reset"))
            {
                await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorName(val, SiotrixConstants.BotName);
                await ReplyAsync(SiotrixConstants.BotSuccess);
                return;
            }

            await SiotrixEmbedAuthorExtensions.SetSiotrixAuthorName(val, txt);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }      

        [Command("description")]
        [Summary("Will list Siotrix current description.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotInfoAsync()
        {
            var val = await SiotrixEmbedInfoExtensions.GetSiotrixInfoAsync();
            await ReplyAsync(val.BotInfo);
        }

        [Command("description")]
        [Summary("Will set Siotrix description.")]
        [Remarks("<text> - Add text for Siotrix information.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotInfoAsync([Remainder] string str)
        {
            var val = await SiotrixEmbedInfoExtensions.GetSiotrixInfoAsync();

            if (str.Equals("reset"))
            {
                await SiotrixEmbedInfoExtensions.SetSiotrixInfo(val, SiotrixConstants.BotDesc);
                await ReplyAsync(SiotrixConstants.BotSuccess);
                return;
            }

            await SiotrixEmbedInfoExtensions.SetSiotrixInfo(val, str);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("website")]
        [Summary("Will list Siotrix current website.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task WebSiteAsync()
        {
            var val = await SiotrixEmbedWebsiteExtensions.GetSiotrixSiteUrlAsync();
            await ReplyAsync(val.SiteUrl);
        }

        [Command("website")]
        [Summary("Will set Siotrix website.")]
        [Remarks("<url> - Update main website URL for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task WebSiteAsync(Uri url)
        {
            var val = await SiotrixEmbedWebsiteExtensions.GetSiotrixSiteUrlAsync();
            await SiotrixEmbedWebsiteExtensions.SetSiotrixSiteUrl(val, url.ToString());
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("footericon")]
        [Summary("Will list Siotrix current footer icon.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterIconAsync()
        {
            var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();           
            await ReplyAsync(val.FooterIcon);
        }

        [Command("footericon")]
        [Summary("Will set Siotrix footer icon.")]
        [Remarks("<url> - Update main footer icon for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterIconAsync(Uri url)
        {
            var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
            await SiotrixEmbedFooterExtensions.SetSiotrixFooterIcon(val, url.ToString());
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("footertext")]
        [Summary("Will list Siotrix current footer text.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterTextAsync()
        {
            var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
            await ReplyAsync(val.FooterText);
        }

        [Command("footertext")]
        [Summary("Will set Siotrix footer text.")]
        [Remarks("<text> - Update main footer text for Siotrix.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task FooterTextAsync([Remainder] string txt)
        {
            var val = await SiotrixEmbedFooterExtensions.GetSiotrixFooterAsync();
            await SiotrixEmbedFooterExtensions.SetSiotrixFooterText(val, txt);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("username")]
        [Summary("Lists Siotrix's username.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task UsernameAsync()
        {
            return ReplyAsync(Context.Client.CurrentUser.ToString());
        }

        [Command("username")]
        [Summary("Sets Siotrix's username.")]
        [Remarks("<name> - new name to change Siotrix too, but why??.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task UsernameAsync([Remainder] string name)
        {
            var self = Context.Client.CurrentUser;
            await self.ModifyAsync(x => { x.Username = name; });
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("activity")]
        [Summary("Lists Siotrix's current activity.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task ActivityAsync()
        {
            return ReplyAsync($"Playing: {Context.Client.CurrentUser.Game}");
        }

        [Command("activity")]
        [Summary("Sets Siotrix's activity.")]
        [Remarks("<activity> - Whatever activity you want to set Siotrix as playing.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ActivityAsync([Remainder] string activity)
        {
            await Context.Client.SetGameAsync(activity);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }

        [Command("status")]
        [Summary("Lists Siotrix's current status.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task StatusAsync()
        {
            return ReplyAsync(Context.Client.CurrentUser.Status.ToString());
        }

        [Command("status")]
        [Summary("Sets Siotrix's status.")]
        [Remarks("<status> - Sets status of Siotrix(Offline, Online, Idle, Afk, etc, etc..).")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task UsernameAsync(UserStatus status)
        {
            var self = Context.Client.CurrentUser;
            await Context.Client.SetStatusAsync(status);
            await ReplyAsync(SiotrixConstants.BotSuccess);
        }
    }
}
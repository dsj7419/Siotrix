using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord.Moderation
{
    [Name("Moderator")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class MuteModule : ModuleBase<SocketCommandContext>
    {

        private readonly InteractiveService _interactive;

        public MuteModule(InteractiveService inter)
        {
            _interactive = inter;
        }

        [Command("mute")]
        [Summary("Using with no args will create and/or verify a proper mute role is in the guild.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetMuteRole()
        {
            var name = SiotrixConstants.BotMuteRoleName;
            if (string.IsNullOrWhiteSpace(name))
                return;

            await ReplyAsync("Please hold while I lookup your current settings...");
            var muteRoleDb = await MuteExtensions.GetGuildMuteRoleNameAsync(Context.Guild.Id);

            var guildMuteRole = await MuteExtensions.GetMuteRole(Context.Guild);

            await Task.Delay(2000);

            await ReplyAsync($"OK, i've retreived your current mute role and name information. The mute role name is: {muteRoleDb.MuteRoleName}\n" +
                             $"Would you like to change the name of your muted role? (Y/N)");

            var responseNameChange =
                await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));

            if (responseNameChange.Content.ToLower() == "y" || responseNameChange.Content.ToLower() == "yes")
            {
                await ReplyAsync("Great, what would you like me to change that role name to for you?");
                var newName = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));

                if (Context.Guild.Roles.OrderByDescending(r => r.Position).Any(r => newName.Content == r.Name))
                {
                    await ReplyAsync(
                        "I don't want to change the role to a name of another role already in the guild. Try again.");
                    return;
                }

                if (newName.Content.ToLower() == "cancel")
                    return;

                IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == muteRoleDb.MuteRoleName);
                await role.ModifyAsync(x => x.Name = newName.Content).ConfigureAwait(false);
                await ReplyAsync(SiotrixConstants.BotSuccess);
            }          
        }

        [Command("mute")]
        [Summary("Mutes a user from being able to type in any channels for a period of time.")]
        [Remarks(" @username (time) - can be set as 2d, 2 days, or 3d 2h 3m.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task Mute(SocketGuildUser user, [Remainder] TimeSpan time)
        {
            var guildWarnInfo = await WarningExtensions.GetWarnSettingsAsync(Context.Guild.Id);
            var guildUserWarnTracking = await UserCaseExtensions.GetUserCaseTrackingAsync(Context.Guild.Id, user.Id);
            var gPrefix = await Context.GetGuildPrefixAsync();
            var minutes = time.TotalMinutes;

            if (user.IsBot && user.Id == SiotrixConstants.BotId)
            {
                await ReplyAsync("Unable to mute me, sorry!");
                return;
            }

            // lets not go down the warning bots rabbit hole
            if (user.IsBot)
            {
                await ReplyAsync("Let's not start muting bots..just kick them if you dont like them!");
                return;
            }

            // retreive the moderation log channel. If this isnt set up it could break other stuff..easier to just exit.
            var modLogChannel = await LogsToggleExtensions.GetModLogChannelAsync(Context.Guild.Id);
            if (modLogChannel == null || !modLogChannel.IsActive)
            {
                await ReplyAsync(
                    $"The moderation logging channel must be both set up and active to warn people ({gPrefix.Prefix}help modlogchannel)");
                return;
            }

            // set up the guilds Moderation channel as an ISocketMessageChannel
            var modChannel =
                Context.Guild.GetChannel(LogChannelExtensions.ModlogchannelId.ToUlong()) as ISocketMessageChannel;

            // if the channel is no longer valid tell them to re-set it up.
            if (modChannel == null)
            {
                await ReplyAsync(
                    $"The current moderation logging channel is invalid. Please re-set this up with {gPrefix.Prefix}logs modlogchannel #channelname.");
                return;
            }

            // Make sure both user and bot are higher rank than the person they are warning
            var aboveResponse =
                UserCaseExtensions.CheckIfUserCanBeWarnKickBanMute(Context, user, Context.User as SocketGuildUser);

            if (aboveResponse == UserCaseExtensions.AboveRoleResponseType.TargetAboveUser)
            {
                await ReplyAsync("You can't mute someone higher rank than you.");
                return;
            }

            if (aboveResponse == UserCaseExtensions.AboveRoleResponseType.TargetAboveBot)
            {
                await ReplyAsync("I can't mute someone higher rank than me.");
                return;
            }


            // If the guild have no parameters set up, use defaults in SiotrixConstants.cs
            if (guildWarnInfo == null)
            {
                await WarningExtensions.CreateWarnSettingsAsync(Context.Guild.Id, SiotrixConstants.TimesBeforeMute,
                    SiotrixConstants.MuteTimeLengthMinutes, SiotrixConstants.TimesBeforeBan,
                    SiotrixConstants.BanTimeLengthMinutes, SiotrixConstants.SrsInfractionsBeforePermBan,
                    SiotrixConstants.WarningFalloffMinutes);
                guildWarnInfo = await WarningExtensions.GetWarnSettingsAsync(Context.Guild.Id);
            }

            // If user has no record, set it up
            if (guildUserWarnTracking == null)
            {
                await UserCaseExtensions.CreateUserCaseTrackingAsync(Context.Guild.Id, user.Id);
                guildUserWarnTracking = await UserCaseExtensions.GetUserCaseTrackingAsync(Context.Guild.Id, user.Id);
            }            

            //time to log the mute to the moderation logging channel.
            var mutedUser =
                await UserCaseExtensions.GetWarningMuteBannedEmbed(Context, user, guildWarnInfo, UserCaseExtensions.CaseResponseType.Muted, 0, false, (long)minutes);

            // send the embed to the channel, and grab the message sent so it can be stored in the database.
            var msg = await modChannel.SendMessageAsync("", embed: mutedUser.Item1);

            // lets get a consistent string to store in database for what type of thing this is (warning, ban, etc)
            var type = UserCaseExtensions.WarnResponseTypeToString(UserCaseExtensions.CaseResponseType.Muted);

            //need to log the case file to the database
            await UserCaseExtensions.CreateUserCaseAsync(mutedUser.Item2, Context.Guild.Id, user.Id,
                0, type, msg.Id, Context.User.Id);

            try
            {
                var response = await UserCaseExtensions.ProcessUserMutesBansPermas(guildWarnInfo, guildUserWarnTracking,
                    Context, user, mutedUser.Item2, UserCaseExtensions.CaseResponseType.Muted, (int)minutes);

                await Context.Channel.SendMessageAsync($"What is the reason for the mute? Case #{mutedUser.Item2}");

            }
            catch
            {
                await Context.Channel.SendMessageAsync("mute_error").ConfigureAwait(false);
            }
        }

        [Command("unmute")]
        [Summary("Unmute a muted user.")]
        [Remarks(" @username")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task UnMute(IGuildUser user)
        {
            try
            {
                await user.UnmuteUser(false, Context).ConfigureAwait(false);
                var caseId = Context.GetCaseNumber();
                await Context.Channel.SendMessageAsync("What is the reason for the unmute? Case #" + caseId);

                CaseExtensions.SaveCaseDataAsync("unmute", caseId, user.Id.ToLong(), Context.Guild.Id.ToLong(),
                    ""); // add save in db
                // Console.WriteLine("unmute ========================={0}", case_id);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("unmute_error").ConfigureAwait(false);
            }
        }        

        [Command("mute list")]
        [Summary("List all members currently muted in this guild.")]
        [Remarks(" - No additional arguments needed.")]
        [RequireContext(ContextType.Guild)]
        [MinPermissions(AccessLevel.GuildMod)]
        private async Task MuteList()
        {
            string users = null;
            var gIconUrl = await Context.GetGuildIconUrlAsync();
            var gName = await Context.GetGuildNameAsync();
            var gUrl = await Context.GetGuildUrlAsync();
            var gThumbnail = await Context.GetGuildThumbNailAsync();
            var gFooter = await Context.GetGuildFooterAsync();
            var gPrefix = await Context.GetGuildPrefixAsync();
            var builder = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(gIconUrl.Avatar)
                    .WithName(gName.GuildName)
                    .WithUrl(gUrl.SiteUrl))
                .WithColor(new Color(255, 127, 0))
                .WithThumbnailUrl(gThumbnail.ThumbNail)
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl(gFooter.FooterIcon)
                    .WithText(gFooter.FooterText))
                .WithTimestamp(DateTime.UtcNow);

            using (var db = new LogDatabase())
            {
                try
                {
                    var result = db.Gmutelists.Where(x => x.GuildId == Context.Guild.Id.ToLong());
                    if (result.Any())
                        foreach (var data in result)
                            users += "**User** : " + Context.Guild.GetUser(data.UserId.ToUlong()).Mention + "  " +
                                     " **TimeLength** : " + data.MuteTime + " minutes" + "\n";
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (users == null)
                users = "No muted users";

            builder
                .AddField(x =>
                {
                    x.Name = "Muted Users";
                    x.Value = users;
                });
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
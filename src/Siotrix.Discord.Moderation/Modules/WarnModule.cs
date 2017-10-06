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
    [Summary("Warn a misbehaving user, and setup warning automation.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.GuildMod)]
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [Summary("issue a manual warning to a user.")]
        [Remarks("(username) [points] [reason]- if no points given will issue 1 point")]
        [MinPermissions(AccessLevel.GuildAdmin)]
        public async Task WarnAsync(SocketGuildUser user, int points = 1, [Remainder] string reason = null)
        {
            var guildWarnInfo = await WarningExtensions.GetWarnSettingsAsync(Context.Guild.Id);
            var guildUserWarnTracking = await UserCaseExtensions.GetUserCaseTrackingAsync(Context.Guild.Id, user.Id);
            var gPrefix = await Context.GetGuildPrefixAsync();

            if (user.IsBot && user.Id == SiotrixConstants.BotId)
            {
                await ReplyAsync("Unable to warn me, sorry!");
                return;
            }

            // lets not go down the warning bots rabbit hole
            if (user.IsBot)
            {
                await ReplyAsync("Let's not start warning bots..just kick them if you dont like them!");
                return;
            }

            // no awarding negative points. user can use forgive for that.
            if (points < 1)
            {
                await ReplyAsync(
                    $"You cannot award less than 1 warning point to a user. Are you trying to forgive them? If so, see {gPrefix.Prefix}help forgive.");
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
                await ReplyAsync("You can't warn someone higher rank than you.");
                return;
            }

            if (aboveResponse == UserCaseExtensions.AboveRoleResponseType.TargetAboveBot)
            {
                await ReplyAsync("I can't warn someone higher rank than me.");
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

            // send the warning points through the process, and see if the warning ended up something worse (mute/ban/permaban)
            var response =
                await UserCaseExtensions.ProcessUserWarnPointsAsync(guildWarnInfo, guildUserWarnTracking, points);

            // lets get a consistent string to store in database for what type of thing this is (warning, ban, etc)
            var type = UserCaseExtensions.WarnResponseTypeToString(response);

            //time to log the warning/mute/ban/permaban to the moderation logging channel.
            var warnedUser =
                await UserCaseExtensions.GetWarningMuteBannedEmbed(Context, user, guildWarnInfo, response, points, false,
                    0,
                    reason);

            // send the embed to the channel, and grab the message sent so it can be stored in the database.
            var msg = await modChannel.SendMessageAsync("", embed: warnedUser.Item1);

            //need to log the case file to the database
            await UserCaseExtensions.CreateUserCaseAsync(warnedUser.Item2, Context.Guild.Id, user.Id,
                points, type, msg.Id, Context.User.Id, reason, true);

            // if user is getting punished at all, lets process this now.
            if (response == UserCaseExtensions.CaseResponseType.Banned ||
                response == UserCaseExtensions.CaseResponseType.Muted ||
                response == UserCaseExtensions.CaseResponseType.MuteWarned ||
                response == UserCaseExtensions.CaseResponseType.PermaBanned)
            {
                var processedResponse = await UserCaseExtensions.ProcessUserMutesBansPermas(guildWarnInfo,
                    guildUserWarnTracking, Context, user, warnedUser.Item2,
                    response);
            }
        }
    }
}
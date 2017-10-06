using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    public static class UserCaseExtensions
    {
        public enum CaseResponseType
        {
            Muted,
            MuteWarned,
            Banned,
            PermaBanned,
            Warned,
            Forgiven,
            Error
        }

        public enum AboveRoleResponseType
        {
            TargetAboveUser,
            TargetAboveBot,
            Success,
            Error
        }

        public static async Task<DiscordGuildUserCases> GetUserCaseAsync(ulong guildId, int caseNum)
        {
            var val = new DiscordGuildUserCases();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercases.FirstOrDefaultAsync(
                        x => x.GuildId == guildId.ToLong() && x.CaseNum.Equals(caseNum));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateUserCaseAsync(int caseNumber, ulong guildId, ulong userId, int warningPoints, string type, ulong messageId, 
                            ulong modId = 0, string reason = null, bool isActive = true)
        {

            var issuedWarning = new DiscordGuildUserCases(caseNumber, guildId.ToLong(), userId.ToLong(), warningPoints, type, messageId.ToLong(),
                            modId.ToLong(), reason, isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gusercases.AddAsync(issuedWarning);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<IEnumerable<DiscordGuildUserCases>> GetUserCasessAsync(ulong guildId)
        {
            IEnumerable<DiscordGuildUserCases> val = new List<DiscordGuildUserCases>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercases.Where(x => x.GuildId == guildId.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<IEnumerable<DiscordGuildUserCases>> GetUserCasessAsync(ulong guildId, ulong userId)
        {
            IEnumerable<DiscordGuildUserCases> val = new List<DiscordGuildUserCases>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercases.Where(x => x.GuildId == guildId.ToLong() && x.UserId == userId.ToLong()).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<IEnumerable<DiscordGuildUserCases>> GetUserCasessAsync(ulong guildId, ulong userId, bool isActive)
        {
            IEnumerable<DiscordGuildUserCases> val = new List<DiscordGuildUserCases>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercases.Where(x => x.GuildId == guildId.ToLong() && x.UserId == userId.ToLong() && x.IsActive == isActive).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<IEnumerable<DiscordGuildUserCases>> GetUserCasessAsync(ulong guildId, ulong userId, DateTime fromDate, DateTime toDate)
        {
            IEnumerable<DiscordGuildUserCases> val = new List<DiscordGuildUserCases>();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercases.Where(x => x.GuildId == guildId.ToLong() && x.UserId == userId.ToLong() && x.CreatedAt >= fromDate &&
                                                            x.CreatedAt <= toDate).OrderBy(x => x.CreatedAt).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<int> CountCaseNumbersAsync(ulong guildId)
        {
            int count = 0;
            using (var db = new LogDatabase())
            {
                try
                {
                    count = await db.Gusercases.CountAsync(x => x.GuildId == guildId.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return count;
        }

        public static async Task ModifyReasonAsync(DiscordGuildUserCases userCase, string reason)
        {
            userCase.SetReason(reason);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercases.Update(userCase);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyIsActiveUserCaseAsync(DiscordGuildUserCases userCase, bool isActive)
        {
            userCase.SetIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercases.Update(userCase);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }        

        public static async Task<DiscordGuilUserCaseTracking> GetUserCaseTrackingAsync(ulong guildId, ulong userId)
        {
            var val = new DiscordGuilUserCaseTracking();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gusercasetracking.FirstOrDefaultAsync(
                        x => x.GuildId == guildId.ToLong() &&
                             x.UserId.Equals(userId.ToLong()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateUserCaseTrackingAsync(ulong guildId, ulong userId, int activeWarningUserPoints = 0, int seriousUserInfractions = 0, int forgivenWarningUserPoints = 0,
            int forgivenSeriousUserInfractions = 0, int totalWarningUserPoints = 0, int numberUserMutes = 0, int numberUserKicks = 0, int numberUserBans = 0)
        {

            var warnTracking = new DiscordGuilUserCaseTracking(guildId.ToLong(), userId.ToLong(), activeWarningUserPoints, seriousUserInfractions, forgivenWarningUserPoints,
            forgivenSeriousUserInfractions, totalWarningUserPoints, numberUserMutes, numberUserKicks, numberUserBans);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gusercasetracking.AddAsync(warnTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyActiveWarningUserPointsAsync(DiscordGuilUserCaseTracking warnCaseTracking, int activeWarningUserPoints)
        {
            warnCaseTracking.SetActiveWarningUserPoints(activeWarningUserPoints);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyForgivenSeriousUserInfractionsAsync(DiscordGuilUserCaseTracking warnCaseTracking, int forgivenSeriousUserInfractions)
        {
            warnCaseTracking.SetForgivenSeriousUserInfractions(forgivenSeriousUserInfractions);            
            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyForgivenWarningUserPointsAsync(DiscordGuilUserCaseTracking warnCaseTracking, int forgivenWarningUserPoints)
        {
            warnCaseTracking.SetForgivenWarningUserPoints(forgivenWarningUserPoints);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyNumberUserMutesAsync(DiscordGuilUserCaseTracking warnCaseTracking, int numberUserMutes)
        {
            warnCaseTracking.SetNumberUserMutes(numberUserMutes);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyNumberUserKicksAsync(DiscordGuilUserCaseTracking warnCaseTracking, int numberUserKicks)
        {
            warnCaseTracking.SetNumberUserKicks(numberUserKicks);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyNumberUserBansAsync(DiscordGuilUserCaseTracking warnCaseTracking, int numberUserBans)
        {
            warnCaseTracking.SetNumberUserBans(numberUserBans);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifySeriousUserInfractionsTrackingAsync(DiscordGuilUserCaseTracking warnCaseTracking, int seriousUserInfractions)
        {
            warnCaseTracking.SetSeriousUserInfraction(seriousUserInfractions);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyTotalWarningUserPointsAsync(DiscordGuilUserCaseTracking warnCaseTracking, int totalWarningUserPoints)
        {
            warnCaseTracking.SetTotalWarningUserPoints(totalWarningUserPoints);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gusercasetracking.Update(warnCaseTracking);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<CaseResponseType> ProcessUserWarnPointsAsync(DiscordGuildWarnSettings guildWarnSettings,
            DiscordGuilUserCaseTracking userCaseWarnTracking, int points)
        {
            int totalActivePoints;
            int userTotalPoints;                        

            if (points < 1)
            {
                return CaseResponseType.Error;
            }

            //cannot possibly process more than the guilds max ban points in 1 warn
            if (points > guildWarnSettings.TimesBeforeBan)
            {
                points = guildWarnSettings.TimesBeforeBan;
            }

            //add points to users total warning points
            userTotalPoints = userCaseWarnTracking.TotalWarningUserPoints + points;
            await ModifyTotalWarningUserPointsAsync(userCaseWarnTracking, userTotalPoints);

            // add inputted points to users current active total
            totalActivePoints = userCaseWarnTracking.ActiveWarningUserPoints + points;

            // has the additional points pushed it based the guilds ban guidelines?
            if (totalActivePoints >= guildWarnSettings.TimesBeforeBan)
            {
                // reset active warning points to 0
                await ModifyActiveWarningUserPointsAsync(userCaseWarnTracking, 0);

                //Since we are resetting all active warning points to 0, we need to set all of users current warns to no longer active.
                var userWarnings = await GetUserCasessAsync(guildWarnSettings.GuildId.ToUlong(), userCaseWarnTracking.UserId.ToUlong(), true);
                foreach(var userWarning in userWarnings)
                {
                    await ModifyIsActiveUserCaseAsync(userWarning, false);
                }

                // since user broke the ban threshold, that is a serious infraction. Increase it by 1
                var response = await ProcessUserSeriousInfraction(guildWarnSettings, userCaseWarnTracking);

                // if user is permabanned return that, if not return that the user was time banned according to guild settings
                return response;                
            }

            // if it hasn't broken the ban threshold, update users currently active points.
            await ModifyActiveWarningUserPointsAsync(userCaseWarnTracking, totalActivePoints);
            //User has not broken ban threshhold, let's check if they have broken the Mute threshold..if not simply return that user has been warned
            return totalActivePoints >= guildWarnSettings.TimesBeforeMute ? CaseResponseType.MuteWarned : CaseResponseType.Warned;
        }

        public static async Task<CaseResponseType> ProcessUserSeriousInfraction(DiscordGuildWarnSettings guildWarnSettings, DiscordGuilUserCaseTracking userCaseWarnTracking)
        {
            int totalSeriousInfractions;

            totalSeriousInfractions = userCaseWarnTracking.SeriousUserInfractions + 1;
            await ModifySeriousUserInfractionsTrackingAsync(userCaseWarnTracking, totalSeriousInfractions);

            // check if there have been enough serious offences according to guild settings to deserve a perma-ban
            if (totalSeriousInfractions >= guildWarnSettings.SrsInfractionsBeforePermBan)
            {
                // yep, perma-boned
                return CaseResponseType.PermaBanned;
            }
            // not perma banned
            return CaseResponseType.Banned;
        }

        public static async Task<CaseResponseType> ProcessUserMutesBansPermas(DiscordGuildWarnSettings guildWarnSettings,
            DiscordGuilUserCaseTracking userCaseWarnTracking, SocketCommandContext context, SocketGuildUser user, int caseId, CaseResponseType response, int minutes = 0)
        {
            if (caseId < 1)
                return CaseResponseType.Error;            

            switch (response)
            {
                case CaseResponseType.Muted:
                    var userMuted = await MuteExtensions.GetGuildMutebyUserAsync(context.Guild.Id, user.Id);
                    var muteRoleDb = await MuteExtensions.GetGuildMuteRoleNameAsync(context.Guild.Id);
                    var guildMuteRole = await MuteExtensions.GetMuteRole(context.Guild);
                    if (minutes == 0)
                        minutes = (int) guildWarnSettings.MuteTimeLengthMinutes;

                    // increase the number of mutes to the players record
                    var numMutes = userCaseWarnTracking.NumberUserMutes;
                    await ModifyNumberUserMutesAsync(userCaseWarnTracking, numMutes + 1);

                    // juuuuust in case lets make sure there's no active mute right now, and if there is lets deactivate that case and set minutes to 0
                    if (userMuted != null && userMuted.MuteTime > 0)
                    {
                        var userCase = await GetUserCaseAsync(context.Guild.Id, userMuted.CaseId);
                        // we want to deactivate a mute casE not a mute-warn since the warn could still be active (and is)
                        if (userCase.IsActive && userCase.Type == "MUTE")
                            await ModifyIsActiveUserCaseAsync(userCase, false);
                        //Update the users active mute time to 0 so we can start a new one.
                        userMuted.SetMuteTime(0);
                    }                       

                    await MuteExtensions.CreateGuildMuteListUserAsync(caseId, context.Guild.Id, user.Id, minutes);

                    await MuteExtensions.TimedMute(user, TimeSpan.FromMinutes(minutes), minutes, context, false)
                        .ConfigureAwait(false);



------------------
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        return CaseResponseType.Muted;                    

                case CaseResponseType.MuteWarned:

                    return CaseResponseType.MuteWarned;
                case CaseResponseType.Banned:

                    return CaseResponseType.Banned;
                case CaseResponseType.PermaBanned:

                    return CaseResponseType.PermaBanned;
                default:
                    return CaseResponseType.Error;
            }
        }

        public static async Task<Tuple<EmbedBuilder, int>> GetWarningMuteBannedEmbed(SocketCommandContext context, SocketGuildUser user, DiscordGuildWarnSettings guildWarnSettings, CaseResponseType responseType, int warningPoints=0, bool isAuto=false, long timeMinutes = 0, [Remainder] string reason = null)
        {
            string typeWarning = null;
            string timedLineBuilder = null;
            string value = null;
            string realReason = null;
            string newTime = null;
            string pluralPoints = (warningPoints == 1) ? "point" : "points";
            string moderator = null;
            uint warnColor = 0;
            DateTime futureDate;
            var gName = await context.GetGuildNameAsync();
            var gFooter = await context.GetGuildFooterAsync();
            var gPrefix = await context.GetGuildPrefixAsync();                       
            var falloff = TimeSpan.FromMinutes(guildWarnSettings.WarningFalloffMinutes);

            // Switch into the type of response we are dealing with (mute, ban, permban or warn)
            switch (responseType)
            {
                //user was muted due to a warning 
                case CaseResponseType.MuteWarned:
                    var mutedLength = TimeSpan.FromMinutes(guildWarnSettings.MuteTimeLengthMinutes);
                    futureDate = DateTime.Now.AddMinutes(guildWarnSettings.MuteTimeLengthMinutes);
                    // Format: "Fri, 09 Mar 2018 16:05:07 GMT"
                    newTime = String.Format($"{0:r}", futureDate);
                    warnColor = 0xFFC400;
                    if (warningPoints > 0)
                    {
                        timedLineBuilder = $"{warningPoints} {pluralPoints} issued resulting in timed mute!\n" +
                                           $"Length: {mutedLength.ToTimespanPrettyFormat()}\n" +
                                           $"User will be un-muted on: {newTime}";
                    }
                    else
                    {
                        timedLineBuilder = $"Length: {mutedLength.ToTimespanPrettyFormat()}\n" +
                                           $"User will be un-muted on: {newTime}";
                    }
                    typeWarning = "Time-Muted";
                    break;

                //user was time muted
                case CaseResponseType.Muted:
                    var muteTimeLength = TimeSpan.FromMinutes(timeMinutes);
                    futureDate = DateTime.Now.AddMinutes(timeMinutes);
                    // Format: "Fri, 09 Mar 2018 16:05:07 GMT"
                    newTime = String.Format($"{0:r}", futureDate);
                    warnColor = 0xFFC400;
                    if (!isAuto)
                    {
                        timedLineBuilder = "No warning points issued for this mute (manual)\n" +
                                           $"Length: {muteTimeLength.ToTimespanPrettyFormat()}\n" +
                                           $"User will be un-muted on: {newTime}";
                    }
                    else
                    {
                        timedLineBuilder = $"No warning points issued for this mute(auto)\n" +
                                           $"Length: {muteTimeLength.ToTimespanPrettyFormat()}\n" +
                                           $"User will be un-muted on: {newTime}";
                    }
                    typeWarning = "Time-Muted";
                    break;

                    //user was time-banned
                case CaseResponseType.Banned:
                    var banTimeLength = TimeSpan.FromMinutes(guildWarnSettings.BanTimeLengthMinutes);
                    futureDate = DateTime.Now.AddMinutes(guildWarnSettings.BanTimeLengthMinutes);
                    newTime = String.Format($"{0:r}", futureDate);
                    warnColor = 0x9100FF;
                    if (warningPoints > 0)
                    {
                        timedLineBuilder = $"{warningPoints} {pluralPoints} issued resulting in timed banned!\n" +
                                           $"Length: {banTimeLength.ToTimespanPrettyFormat()}\n" +
                                           $"User will be un-banned on: {newTime}";
                    }
                    else
                    {
                        timedLineBuilder = $"Length: {banTimeLength.ToTimespanPrettyFormat()}\n" +
                                       $"User will be un-banned on: {newTime}";
                    }
                    typeWarning = "Time-Banned";
                    break;

                    //user perma-boned
                case CaseResponseType.PermaBanned:
                    warnColor = 0xFF0000;
                    timedLineBuilder = $"PERMANENT BAN";
                    typeWarning = "Perma-Banned";
                    break;

                    //user time warned
                case CaseResponseType.Warned:
                    futureDate = DateTime.Now.AddMinutes(guildWarnSettings.WarningFalloffMinutes);
                    newTime = String.Format($"{0:r}", futureDate);
                    warnColor = 0xFFFF00;
                    timedLineBuilder = $"{warningPoints} {pluralPoints} issued.\n" +
                                       $"Falloff Time: {falloff.ToTimespanPrettyFormat()} \n" +
                                       $"Warning will falloff on: {newTime}";
                    typeWarning = "Warning Infraction";
                    break;
            }

            var caseId = await CountCaseNumbersAsync(context.Guild.Id);
            caseId += 1;

            if (!isAuto)
            {
                realReason = reason ?? $"No reason given - type {gPrefix.Prefix}reason {caseId} (text) to update the case.";
                moderator = context.User.Mention;
            }
            else
            {
                realReason = $"AUTOMATIC {typeWarning}";
                moderator = "Bot-Imposed Sanction";
            }
              

            value = $"User : {user.Mention} ({user.Id})\n" +
                    $"Moderator : {moderator}\n" +
                    $"{timedLineBuilder}\n" +
                    $"Reason: {realReason}";            

            var builder = new EmbedBuilder()
                .WithTitle($"{gName.GuildName} Moderation Log")
                .WithColor(warnColor)
                .WithFooter(new EmbedFooterBuilder()
                        .WithIconUrl(gFooter.FooterIcon)
                        .WithText(gFooter.FooterText))
                    .WithTimestamp(DateTime.UtcNow)
                .AddField(x =>
                {
                    x.Name = $"Case # {caseId} | {typeWarning}";
                    x.Value = value;
                });

            return Tuple.Create(builder, caseId);
        }

        public static AboveRoleResponseType CheckIfUserCanBeWarnKickBanMute(SocketCommandContext context, SocketGuildUser user, SocketGuildUser mod=null)
        {
            var bot = context.Guild.GetUser(SiotrixConstants.BotId) as IGuildUser;
            var usersHighestRole = user.Roles.OrderByDescending(r => r.Position).First();

            if (mod != null)
            {
                var modHighestRole = mod.Roles.OrderByDescending(r => r.Position).First();                

                if (usersHighestRole.Position > modHighestRole.Position)
                {
                    return AboveRoleResponseType.TargetAboveUser;
                }
            }

            var botHighestRole = bot.RoleIds.Select(x => context.Guild.GetRole(x))
                .OrderByDescending(x => x.Position)
                .First();

            return usersHighestRole.Position > botHighestRole.Position ? AboveRoleResponseType.TargetAboveBot : AboveRoleResponseType.Success;
        }

        public static string WarnResponseTypeToString(CaseResponseType response)
        {
            string type = null;
            switch(response)
                {
                    case CaseResponseType.Muted:
                        type = "MUTE";
                        break;
                    case CaseResponseType.MuteWarned:
                        type = "MUTEWARN";
                        break;
                    case CaseResponseType.Banned:
                        type = "TIMEBAN";
                        break;
                    case CaseResponseType.PermaBanned:
                        type = "PERMBAN";
                        break;
                    case CaseResponseType.Warned:
                        type = "WARN";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(response), response, null);
                }
            return type;
        }

        public static string ProcessUserCaseFalloffTimeLeft(DiscordGuildUserCases userCase,
            DiscordGuildWarnSettings guildWarnSettings)
        {
            DateTime futureDate;
            var falloff = TimeSpan.FromMinutes(guildWarnSettings.WarningFalloffMinutes);
            string newTime;

            if (userCase.IsForgiven)
                return "This case was forgiven.";
            if (!userCase.IsActive)
                return "This case is no longer active.";

            switch (userCase.Type)
            {
                case "MUTE":
                    futureDate = userCase.CreatedAt + falloff;
                    if (userCase.WarningPoints > 0)
                    { 
                        newTime = String.Format("Points falloff at: {0:r}", futureDate);

                        return
                            $"Was awarded {userCase.WarningPoints} points that resulted in a timed mute.\n" +
                            $"{newTime}";
                    }
                    return "Was a manual mute with no points awarded.";
                case "TIMEBAN":
                    if (userCase.WarningPoints > 0)
                    {
                        return "Received a serious infraction for breaking the guild ban threshold.";
                    }
                    return "Was a manual timed ban with no points awarded.";
                case "PERMBAN":
                    return "No falloff time -- PERMABANNED!";
                case "WARN":
                    futureDate = userCase.CreatedAt + falloff;
                    if (futureDate < DateTime.Now)
                        return "The falloff time has already passed.";
                    newTime = String.Format($"Warning will falloff at: {0:r}", futureDate);
                    return newTime;
                default:
                    return "invalid time";
            }
        }
    }
}

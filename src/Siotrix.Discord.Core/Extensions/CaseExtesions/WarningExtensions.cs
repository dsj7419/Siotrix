using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace Siotrix.Discord
{
    public static class WarningExtensions
    {
        public static async Task<DiscordGuildWarnList> GetGuildWarnListUserAsync(ulong caseId)
        {
            var val = new DiscordGuildWarnList();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gwarnlists.FirstOrDefaultAsync(
                        x => x.CaseId.Equals(caseId.ToLong()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateGuildWarnListUserAsync(ulong caseId, ulong guildId, ulong userId, int warnTime)
        {

            var warnnedUser = new DiscordGuildWarnList(caseId.ToLong(), guildId.ToLong(), userId.ToLong(), warnTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gwarnlists.AddAsync(warnnedUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnListUserMinutesAsync(DiscordGuildWarnList warnnedUser, int warnTime)
        {
            warnnedUser.SetWarnTime(warnTime);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnlists.Update(warnnedUser);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task<IEnumerable<DiscordGuildWarnList>> GetGuildWarnListAsync(ulong guildId)
        {
            IEnumerable<DiscordGuildWarnList> val = new List<DiscordGuildWarnList>();

            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gwarnlists.Where(x => x.GuildId == guildId.ToLong() && x.WarnTime > 0).ToListAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return val;
        }

        public static async Task<DiscordGuildWarnSettings> GetWarnSettingsAsync(ulong guildId)
        {
            var val = new DiscordGuildWarnSettings();
            using (var db = new LogDatabase())
            {
                try
                {
                    val = await db.Gwarnsettings.FirstOrDefaultAsync(
                        x => x.GuildId == guildId.ToLong());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return val;
        }

        public static async Task CreateWarnSettingsAsync(ulong guildId, int timesBeforeMute, long muteTimeLengthMinutes, int timesBeforeBan,
            long banTimeLengthMintues, int srsInfractionsBeforePermBan, long warningFalloffMinutes)
        {
            var warnSettings = new DiscordGuildWarnSettings(guildId.ToLong(), timesBeforeMute, muteTimeLengthMinutes, timesBeforeBan,
                banTimeLengthMintues, srsInfractionsBeforePermBan, warningFalloffMinutes);

            using (var db = new LogDatabase())
            {
                try
                {
                    await db.Gwarnsettings.AddAsync(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnTimesBeforeMute(DiscordGuildWarnSettings warnSettings, int timesBeforeMute)
        {
            warnSettings.SetTimesBeforeMute(timesBeforeMute);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnMuteTimeLengthMinutes(DiscordGuildWarnSettings warnSettings, long muteTimeLengthMinutes)
        {
            warnSettings.SetMuteTimeLengthMinutes(muteTimeLengthMinutes);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnTimesBeforeBan(DiscordGuildWarnSettings warnSettings, int timesBeforeBan)
        {
            warnSettings.SetTimesBeforeBan(timesBeforeBan);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnTimeLengthMinutes(DiscordGuildWarnSettings warnSettings, long banTimeLengthMinutes)
        {
            warnSettings.SetBanTimeLengthMinutes(banTimeLengthMinutes);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnSrsInfractionsBeforePermBan(DiscordGuildWarnSettings warnSettings, int srsInfractionsBeforePermBan)
        {
            warnSettings.SetSrsInfractionsBeforePermBan(srsInfractionsBeforePermBan);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnFalloffMinutes(DiscordGuildWarnSettings warnSettings, long warningFalloffMinutes)
        {
            warnSettings.SetWarningFalloffMinutes(warningFalloffMinutes);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static async Task ModifyWarnIsActive(DiscordGuildWarnSettings warnSettings, bool isActive)
        {
            warnSettings.SetIsActive(isActive);

            using (var db = new LogDatabase())
            {
                try
                {
                    db.Gwarnsettings.Update(warnSettings);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}

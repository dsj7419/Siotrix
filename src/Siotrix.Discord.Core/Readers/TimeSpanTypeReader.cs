using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Siotrix.Discord
{
    public class TimeSpanTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            var times = new Dictionary<string, int>();

            var regex = new Regex(@"(\d+)\s{0,1}([a-zA-Z]*)");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                int value;
                if (!int.TryParse(match.Groups[1].Value, out value))
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid time value specified."));

                string range = match.Groups[2].Value;
                if (string.IsNullOrWhiteSpace(range))
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid time range specified."));

                times.Add(range.Trim(), value);
            }

            var finalTime = new TimeSpan();
            foreach (var range in times)
            {
                switch (range.Key)
                {
                    case "w":
                    case "ws":
                    case "wk":
                    case "wks":
                    case "week":
                    case "weeks":
                        finalTime = finalTime.Add(new TimeSpan(range.Value * 7, 0, 0, 0));
                        break;
                    case "d":
                    case "ds":
                    case "day":
                    case "days":
                        finalTime = finalTime.Add(new TimeSpan(range.Value, 0, 0, 0));
                        break;
                    case "h":
                    case "hs":
                    case "hr":
                    case "hrs":
                    case "hour":
                    case "hours":
                        finalTime = finalTime.Add(new TimeSpan(range.Value, 0, 0));
                        break;
                    case "m":
                    case "ms":
                    case "min":
                    case "mins":
                    case "minute":
                    case "minutes":
                        finalTime = finalTime.Add(new TimeSpan(0, range.Value, 0));
                        break;
                    case "s":
                    case "ss":
                    case "sec":
                    case "secs":
                    case "second":
                    case "seconds":
                        finalTime = finalTime.Add(new TimeSpan(0, 0, range.Value));
                        break;
                    default:
                        return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Unknown time range {range.Key}"));
                }
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(finalTime));
        }
    }
}
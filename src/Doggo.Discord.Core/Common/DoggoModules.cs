﻿using System.Text;

namespace Doggo.Discord
{
    public class DoggoModules
    {
        public bool Admin { get; set; } = false;
        public bool Audio { get; set; } = false;
        public bool Events { get; set; } = false;
        public bool Moderation { get; set; } = false;
        public bool Roslyn { get; set; } = false;
        public bool Github { get; set; } = false;
        public bool Statistics { get; set; } = false;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Admin: {Admin}");
            builder.AppendLine($"Audio: {Audio}");
            builder.AppendLine($"Events: {Events}");
            builder.AppendLine($"Moderation: {Moderation}");
            builder.AppendLine($"Roslyn: {Roslyn}");
            builder.AppendLine($"Github: {Github}");
            builder.AppendLine($"Statistics: {Statistics}");
            return builder.ToString();
        }
    }
}

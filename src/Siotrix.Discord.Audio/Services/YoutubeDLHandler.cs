using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Siotrix.Discord.Audio
{
    public class YoutubeDLHandler
    {
        private readonly string appdir = AppContext.BaseDirectory;

        public Task EnsureUpdatedAsync()
        {
            if (!Directory.Exists("cache/audio"))
                Directory.CreateDirectory("cache/audio");
            
            return Task.CompletedTask;
        }
        
        public Task<string> DownloadAsync(ulong guildId, string url)
        {
            var id = new Random().Next(1000, 1000000);
            string pathpart = Path.Combine(appdir, $"cache/audio/{guildId}");
            string filepart = $"{id}.mp3";

            if (!Directory.Exists(pathpart))
                Directory.CreateDirectory(pathpart);

            string path = Path.Combine(pathpart, filepart);

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(appdir, "youtube-dl.exe"),
                Arguments = $"-x -q --write-info-json --audio-format mp3 -o \"{path}\" \"{url}\"",
                UseShellExecute = false
            });

            process.WaitForExit();
            return Task.FromResult(path);
        }
    }
}

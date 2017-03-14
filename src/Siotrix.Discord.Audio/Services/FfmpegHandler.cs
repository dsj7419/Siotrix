using System;
using System.Threading.Tasks;
using Discord.Audio;
using System.Diagnostics;
using System.IO;

namespace Siotrix.Discord.Audio
{
    public class FfmpegHandler
    {
        public Task EnsureUpdatedAsync()
        {
            return Task.CompletedTask;
        }
        
        public async Task SendAsync(IAudioClient client, string path)
        {
            var process = CreateProcess(path);
            var output = process.StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(AudioApplication.Music, 1920);
            await output.CopyToAsync(stream);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"),
                Arguments = $"-hide_banner -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false
            });
        }
    }
}

using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Siotrix.Discord.Audio
{
    public class AudioService : IService
    {
        private ConcurrentDictionary<ulong, IAudioClient> _clients = new ConcurrentDictionary<ulong, IAudioClient>();
        private YoutubeDLHandler _ytdl;
        private FfmpegHandler _ffmpeg;
        
        public async Task StartAsync()
        {
            _ytdl = new YoutubeDLHandler();
            _ffmpeg = new FfmpegHandler();

            await _ytdl.EnsureUpdatedAsync();
            await _ffmpeg.EnsureUpdatedAsync();
            await PrettyConsole.LogAsync("Info", "Audio", "Service started successfully");
        }

        public async Task StopAsync()
        {
            foreach (var client in _clients.Values)
                await client.StopAsync();
            
            _ytdl = null;
            _ffmpeg = null;
            await PrettyConsole.LogAsync("Info", "Audio", "Service stopped successfully");
        }

        private async Task JoinAsync(SocketVoiceChannel channel)
        {
            if (_clients.TryGetValue(channel.Guild.Id, out IAudioClient a))
                return;
            
            var client = await channel.ConnectAsync().ConfigureAwait(false);

            if (_clients.TryAdd(channel.Guild.Id, client))
                PrettyConsole.Log("Info", "Audio", $"Joined voice at {channel.Guild.Name}/{channel.Name}");
        }

        private async Task LeaveAsync(SocketVoiceChannel channel)
        {
            if (_clients.TryRemove(channel.Guild.Id, out IAudioClient client))
            {
                await client.StopAsync();
                PrettyConsole.Log("Info", "Audio", $"Left voice at {channel.Guild.Name}/{channel.Name}");
            }
        }

        public async Task PlayAsync(SocketVoiceChannel channel, Uri url)
        {
            var user = channel.Guild.CurrentUser;

            if (user.VoiceChannel?.Id != channel.Id)
                await JoinAsync(channel).ConfigureAwait(false);

            if (_clients.TryGetValue(channel.Guild.Id, out IAudioClient client))
            {
                string file = await _ytdl.DownloadAsync(channel.Guild.Id, url.ToString());
                await _ffmpeg.SendAsync(client, file).ConfigureAwait(false);
            }
        }

        public async Task PlayAsync(SocketVoiceChannel channel, string path)
        {
            var user = channel.Guild.CurrentUser;

            if (user.VoiceChannel?.Id != channel.Id)
                await JoinAsync(channel).ConfigureAwait(false);

            if (_clients.TryGetValue(channel.Guild.Id, out IAudioClient client))
                await _ffmpeg.SendAsync(client, path).ConfigureAwait(false);
        }

        public async Task PlayAsync(SocketVoiceChannel channel, long soundId)
        {
            var user = channel.Guild.CurrentUser;

            if (user.VoiceChannel?.Id != channel.Id)
                await JoinAsync(channel);

            if (_clients.TryGetValue(channel.Guild.Id, out IAudioClient client))
            {
                string file = $"cache/sounds/{soundId}.mp3";
                await _ffmpeg.SendAsync(client, file).ConfigureAwait(false);
            }
        }

        public async Task ResumeAsync()
        {
            await Task.Delay(0);
        }

        public object GetCurrentlyPlaying(ulong guildId)
        {
            return null;
        }
    }
}

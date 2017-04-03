﻿using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Siotrix.Discord.Audio
{
    public class PlayModule : ModuleBase<SocketCommandContext>
    {
        private AudioService _audio;

        public PlayModule(AudioService audio)
        {
            _audio = audio;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("Resume playback of a paused song")]
        public Task PlayAsync()
        {
            var user = Context.User as SocketGuildUser;

            if (user.VoiceChannel == null)
                return ReplyAsync("You must be in a voice channel to play music.");

            return _audio.PlayAsync(user.VoiceChannel, "surf.mp3");
        }

        [Command("play", RunMode = RunMode.Async), Priority(10)]
        [Remarks("Search for and then play a song")]
        public Task PlayAsync(string query)
        {
            return ReplyAsync("Search is not supported at this time.");
        }

        [Command("play", RunMode = RunMode.Async), Priority(20)]
        [Remarks("Play a song from a specific url")]
        public Task PlayAsync(Uri url)
        {
            var user = Context.User as SocketGuildUser;

            if (user.VoiceChannel == null)
                return ReplyAsync("You must be in a voice channel to play music.");
            
            return _audio.PlayAsync(user.VoiceChannel, url);
        }

        [Command("playing")]
        [Remarks("View information on the currently playing song")]
        public Task PlayingAsync()
        {
            var playing = _audio.GetCurrentlyPlaying(Context.Guild.Id);

            if (playing == null)
                return ReplyAsync("There aren't any songs playing right now.");
            else
                return ReplyAsync("Song Info");
        }
    }
}

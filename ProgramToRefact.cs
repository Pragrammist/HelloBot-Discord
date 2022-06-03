using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using System.Collections.Generic;
using Discord.Commands;
using YoutubeExplode;
using YoutubeExplode.Videos;
using CliWrap;

namespace Microsoft_Office_Word
{
    class ProgramRefact
    {
        static DiscordSocketClient _client;
        public static string botChanelName = "bot_hellobot";
        static string path = @"C:\Users\F\source\repos\Microsoft Office Word\Microsoft Office Word\Songs\-blin-zachem-ya-syuda-prishel.mp3";
        static string token = "OTU0MTI1NjQ4NTg5MzYxMTgz.YjOkwQ.e3onepUi569wDVGP1_KcqNUownU";
        static bool isPlaying = false;
        static string botName = "HelloBot";
        static string emojiCodeOn = "\uD83D\uDD08";
        static string emojiCodeOff = "\uD83D\uDD07";
        static string emojiSign = "\uD83C\uDFA7";
        static bool isMute = false;
        static string botMessage = "Я вас приветствую, используйте эмодзи, чтобы управлять звуками";
        static string url = "https://www.youtube.com/watch?v=xko3MSJrhXE";
        public static ulong id = 956010641938858014;


        public static List<string> songs = new List<string>();
        public static List<string> ems = new List<string>();
        public static Dictionary<string, string> emotes = new();

        static async Task Main(string[] args)
        {
            //Конфигурация и настройка бота в начале
            _client = new DiscordSocketClient();


            _client.Log += Log;

            CommandService cService = new CommandService();

            Initialize initialize = new Initialize(cService, _client);
            var services = initialize.BuildServiceProvider();

            CommandHandler commandHandler = new CommandHandler(services, cService, _client);
            await commandHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            _client.JoinedGuild += Joined;
            _client.UserVoiceStateUpdated += Connected;
            _client.ReactionAdded += ReactionAdded;
            await Task.Delay(-1);
        }
        static async Task ReactionAdded(Cacheable<IUserMessage, ulong> m1, Cacheable<IMessageChannel, ulong> m2, SocketReaction reaction)
        {
            //
            //Интерфейс для пользователя
            //


            IUserMessage mess;

            if (m1.Value is not null)
                mess = m1.Value;
            else
                mess = await m1.DownloadAsync();


            var userReacted = reaction.User.Value;
            var isBotMessage = IsContainSign(mess);

            //
            //Валидация
            //
            if (userReacted.IsBot || !isBotMessage)
            {
                return;
            }




            if (isBotMessage)
            {
                //
                //Интерфейс для пользователя
                //

                var name = reaction.Emote.Name;
                var deleteEmote = GetDeleteEmote(mess, reaction.Emote);
                if (name == emojiCodeOn)
                {
                    isMute = false;
                }
                else if (name == emojiCodeOff)
                {
                    isMute = true;
                }
                else if (ems.Contains(name))
                {
                    url = emotes[name];
                }
                if (deleteEmote is not null)
                {

                    await mess.RemoveAllReactionsForEmoteAsync(deleteEmote);

                    await mess.AddReactionAsync(deleteEmote);
                }

            }
        }
        public static bool IsContainSign(IUserMessage message)
        {
            //
            //Интерфейс для пользователя
            //


            if (!message.Author.IsBot)
            {
                return false;
            }
            foreach (var react in message.Reactions.Keys)
            {
                if (react.Name == emojiSign)
                {
                    return true;
                }
            }

            return false;
        }
        static IEmote GetDeleteEmote(IUserMessage message, IEmote reacted)
        {
            //
            //Интерфейс для пользователя
            //


            IEmote res = null;
            if (reacted.Name == emojiCodeOff && message.Reactions.FirstOrDefault(r => r.Key.Name == emojiCodeOn).Value.ReactionCount > 1)
            {
                res = message.Reactions.FirstOrDefault(r => r.Key.Name == emojiCodeOn).Key;
            }
            else if (reacted.Name == emojiCodeOn && message.Reactions.FirstOrDefault(r => r.Key.Name == emojiCodeOff).Value.ReactionCount > 1)
            {
                res = message.Reactions.FirstOrDefault(r => r.Key.Name == emojiCodeOff).Key;
            }
            else if (ems.Contains(reacted.Name))
            {
                IEnumerable<string> notAppropriateEmotes = new string[] { reacted.Name, emojiCodeOff, emojiCodeOn };
                res = message.Reactions.FirstOrDefault(r => !notAppropriateEmotes.Contains(r.Key.Name) && r.Value.ReactionCount > 1).Key;
            }

            return res;
        }

        static async Task Log(LogMessage logMessage)
        {
            //
            //Система логирования и записи исключений
            //


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logMessage.Exception);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(logMessage.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(logMessage.Severity);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(logMessage.Source);
        }

        static async Task Connected(SocketUser user, SocketVoiceState v1, SocketVoiceState v2)
        {
            //
            //Воис
            //
            string p = url;
            if (v1.VoiceChannel == null && v2.VoiceChannel != null && !user.IsBot) // connected
            {
                var voiceChanle = v2.VoiceChannel;

                if (!isPlaying && !isMute)
                {


                    var t = ConnectToVoice(voiceChanle, p).ContinueWith(async (t) =>
                    {
                        //isPlaying = false;
                        if (t.IsFaulted)
                        {
                            foreach (var ex in t.Exception.InnerExceptions)
                            {
                                //
                                //Система логирования и записи исключений
                                //


                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.WriteLine(ex.Message);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine(ex.Source);
                                Console.WriteLine(ex.HelpLink);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                        if (t.IsCompleted)
                        {
                            await t.Result.DisconnectAsync();
                        }
                    });

                }

            }
            else if (v1.VoiceChannel != null && v2.VoiceChannel == null) //disconnected
            {

                if (user.IsBot && user.Username == botName)
                {
                    isPlaying = false;
                }
            }
        }

        static async Task Joined(SocketGuild guild)
        {

            //
            //Валидация
            //

            var channel = guild.TextChannels.FirstOrDefault(ch => ch.Name.ToLower() == botChanelName.ToLower());
            if (channel is null)

                return;


            //
            //Формирование сообщений в чат
            //

            string url = "http://memesmix.net/media/created/vyvo4q.jpg";
            EmbedBuilder embedBuilder = new();
            embedBuilder.Title = botMessage;
            embedBuilder.ImageUrl = url;


            var emb = embedBuilder.Build();

            var mess = await channel.SendMessageAsync(embed: emb);


            //
            //Работа с БД
            //



            id = mess.Id;


            //
            //Интерфейс
            //
            Emoji on = Emoji.Parse(emojiCodeOn);
            Emoji off = Emoji.Parse(emojiCodeOff);
            Emoji sign = Emoji.Parse(emojiSign);



            IEmote[] ems = new IEmote[] { on, off, sign };
            await mess.AddReactionsAsync(ems);


        }



        private static async Task<MemoryStream> CreateFromYoutube(string url)
        {

            //
            //Воис
            //
            var codec = "mp4a";
            var youtube = new YoutubeClient();

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(VideoId.Parse(url));

            var streamInfos = streamManifest.GetAudioOnlyStreams();

            var streamInfo = streamInfos.FirstOrDefault(s => s.AudioCodec.ToLower().Contains(codec.ToLower()));

            var input = await youtube.Videos.Streams.GetAsync(streamInfo);

            var memoryStream = new MemoryStream();
            var com = await Cli.Wrap("ffmpeg")
                .WithArguments("-hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(input))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            return memoryStream;

        }

        private static Process CreateStream(string path)
        {
            //
            //Воис
            //

            var moduleName = "ffmpeg.exe";
            var find = Process.GetProcessesByName(moduleName);
            if (find.Length > 0)
            {
                return find[0];
            }

            var proc = new ProcessStartInfo
            {
                FileName = moduleName,
                Arguments = $@"-i ""{path}"" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,

            };


            return Process.Start(proc);
        }

        private static async Task<SocketVoiceChannel> ConnectToVoice(SocketVoiceChannel voiceChannel, string path)
        {

            //
            //Воис
            //

            if (voiceChannel == null)
                return voiceChannel;


            isPlaying = true;
            //Console.WriteLine($"Connecting to channel {voiceChannel.Id}");
            await Task.Delay(500);
            var connection = await voiceChannel.ConnectAsync().ConfigureAwait(false);

            await Task.Delay(500);

            await SendAsync(connection, path).ConfigureAwait(false);



            return voiceChannel;


        }

        private static async Task SendAsync(IAudioClient client, string path)
        {
            //
            //Валидация
            //
            if (client is null)
                return;
            // Create FFmpeg using the previous example





            //
            //Воис
            //

            using (var input = await CreateFromYoutube(path))
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await discord.WriteAsync(input.ToArray(), 0, (int)input.Length); }
                finally { await discord.FlushAsync(); }
            }
        }



    }
}

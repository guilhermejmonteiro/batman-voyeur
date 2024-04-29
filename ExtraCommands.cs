using Telegram.Bot;
using Telegram.Bot.Types;

namespace BatBot
{
    public class ExtraCommands
    {
        private readonly ITelegramBotClient _batBot;

        public ExtraCommands(ITelegramBotClient batBot)
        {
            _batBot = batBot;
        }
        public async Task HandleHiddenCommandsAsync(Message message, string command)
        {
            switch (command){
                case "/acorda":
                await HandleAcordaCommandAsync(message);
                break;
                case "/yamato":
                await HandleYamatoCommandAsync(message);
                break;
                case "/elbigodon":
                await HandleBigodonCommandAsync(message);
                break;
            }
        }

        async Task HandleAcordaCommandAsync(Message message)
        {
            await _batBot.SendVideoAsync(
                    chatId: message.Chat.Id,
                    video: InputFile.FromUri("http://i.imgur.com/ACFiRDK.mp4"),
                    supportsStreaming: true,
                    replyToMessageId: message.MessageId,
                    cancellationToken: default
            );
        }

        async Task HandleYamatoCommandAsync(Message message)
        {
            await _batBot.SendVideoAsync(
                    chatId: message.Chat.Id,
                    video: InputFile.FromUri("https://i.imgur.com/RdR5DWs.mp4"),
                    supportsStreaming: true,
                    replyToMessageId: message.MessageId,
                    cancellationToken: default
            );
        }

        async Task HandleBigodonCommandAsync(Message message)
        {
            await _batBot.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromUri("https://i.imgur.com/d9WzkvL.jpeg"),
                    caption: "el bigodon",
                    replyToMessageId: message.MessageId,
                    cancellationToken: default
                );
            
        }
    }

}

using Telegram.Bot;
using Telegram.Bot.Types;

namespace BatBot
{
    public class ExtraCommands
    {
        private readonly ITelegramBotClient batBot;

        public ExtraCommands(ITelegramBotClient _batBot)
        {
            batBot = _batBot;
        }
        public async Task HandleHiddenCommandsAsync(Message message, string command)
        {
            switch (command){
                case "/acorda":
                    await HandleExtrasCommandAsync(message, ExtraLinks.acorda);
                    break;
                case "/yamato":
                    await HandleExtrasCommandAsync(message, ExtraLinks.yamato);
                    break;
                case "/elbigodon":
                    await HandleExtrasCommandAsync(message, ExtraLinks.bigodon);
                    break;
            }
        }

        async Task HandleExtrasCommandAsync(Message message, string mediaLink)
        {
            await batBot.SendAnimationAsync(
                chatId: message.Chat.Id,
                animation: InputFile.FromUri(mediaLink),
                replyToMessageId: message.MessageId,
                cancellationToken: default
            );
        }
    }
}

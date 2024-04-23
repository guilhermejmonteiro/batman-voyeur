#region Namespace Imports

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

#region Bot Start
    var botClient = new TelegramBotClient(AccessToken.BatToken);
    
    using CancellationTokenSource cts = new ();
    
    ReceiverOptions receiverOptions = new ()
    {
        AllowedUpdates = Array.Empty<UpdateType>()
    };
    
    botClient.StartReceiving(
        updateHandler: HandleUpdateAsync,
        pollingErrorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token);
    
    var me = await botClient.GetMeAsync();
    
    Console.WriteLine($"Start listening for @{me.Username}");
    
    Console.ReadLine();
    
    cts.Cancel();
#endregion

#region Update Handling

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    string command = message.Text.Split(' ')[0].ToLower();

    switch (command)
    {
        case "/help":
            await HandleHelpCommandAsync(message);
            break;
        case "/w":
            await HandleWRCommandAsync(message);
            break;
        case "/kill":
            await HandleKillCommandAsync(message);
            break;
        case "/acorda":
            await HandleAcordaCommandAsync(message);
            break;
    }    
}

#endregion

#region Command Handling

async Task HandleHelpCommandAsync(Message message)
{
    await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "/help - mostra o curriculo do pai\n/kill - desce o vapo em algum muquira\n/w - chama os parça pro w",
            replyToMessageId: message.MessageId,
            cancellationToken: default);
}

async Task HandleWRCommandAsync(Message message)
{
    string[] chamada = {"hora de tomar leite de macho do alistar",
                        "chamando todos os viados para o encontro de bixas \\(wr\\)",
                        "favor comparecer a queda semidiaria de rank",
                        "hora de subir pro mestre \\(ficar preso no esmeralda\\)"};
        
    string randomChamada = chamada[new Random().Next(chamada.Length)];

    await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{UserIDs.felipeUser}, {UserIDs.guilhermeUser}, {UserIDs.luisUser}, {UserIDs.marcosUser} e {UserIDs.batBotUser}, {randomChamada}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: default);
}

async Task HandleKillCommandAsync(Message message)
{
    string[] commandParts = message.Text.Split(' ');
    
    if (commandParts.Length != 2 || !commandParts[1].StartsWith("@"))
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "tá chapando man? tem que marcar alguém assim:\n/kill @otário",
            cancellationToken: default
        );
        return;
    }

    string taggedUser = commandParts[1];

    string killEmoji1 = "\U0001F91C";
    string killEmoji2 = "\U0001F4A5";
    Random rnd = new Random();
    string KillGif = KillResources.KillGifs[rnd.Next(KillResources.KillGifs.Length)];

    if (taggedUser == "@batmanVoyeurBot")
    {
        await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            photo: InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQqG0SCSQgQZS3JXlpS6lGTsgf2r5GxqAoIfvZPMWN0umoUs2C2qeue3fg&s=10"),
            caption: "<b>ROYAL GUARD!</b>",
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Html,
            cancellationToken: default
        );
    }
    else
    {
        string escapedCaption = EscapeMarkdownCharacters(
            $"{message.From.Username} {killEmoji1}{killEmoji2} {taggedUser}");

        await botClient.SendAnimationAsync(
            chatId: message.Chat.Id,
            animation: InputFile.FromUri(KillGif),
            caption: escapedCaption,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Markdown,
            cancellationToken: default
        );
    }
}

async Task HandleAcordaCommandAsync(Message message)
{
    await botClient.SendVideoAsync(
            chatId: message.Chat.Id,
            video: InputFile.FromUri("http://i.imgur.com/ACFiRDK.mp4"),
            supportsStreaming: true,
            replyToMessageId: message.MessageId,
            cancellationToken: default
    );
}

#endregion

#region Error Handling

string EscapeMarkdownCharacters(string text)
{
    return Regex.Replace(text, @"([_\*\[\]\(\)~`>\#\+\-=\|\{\}\.\!])", @"\$1");
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

#endregion
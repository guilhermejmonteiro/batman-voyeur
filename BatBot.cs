#region Namespace Imports

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BatBot;
using System.IO;

#endregion

#region Bot Start
DateTime lastOnline = DateTime.MinValue;
    lastOnline = DateTime.UtcNow;

    string queryData = null;
    
    var batBot = new TelegramBotClient(AccessToken.BatToken);
    var extra = new ExtraCommands(batBot);
    
    using CancellationTokenSource cts = new ();
    
    ReceiverOptions receiverOptions = new ()
    {
        AllowedUpdates = Array.Empty<UpdateType>()
    };
    
    batBot.StartReceiving(
        updateHandler: HandleUpdateAsync,
        pollingErrorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token);
    
    var me = await batBot.GetMeAsync();
    
    Console.WriteLine($"Start listening for @{me.Username}");
    
    await HandleStartMessageAsync();
    
    Console.ReadLine();
    
    cts.Cancel();

#endregion

#region Update Handling

async Task HandleUpdateAsync(ITelegramBotClient batBot, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    
    if (message.Date < lastOnline)
        return;

    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    string _command = message.Text.Split(' ')[0].ToLower();
    string command = _command.Split('@')[0].ToLower();

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
        case "/addgif":
            await HandleAddGifCommandAsync(message, update);
            break;
        default:
            await extra.HandleHiddenCommandsAsync(message, command);
            break;
    }    
}

#endregion

#region Command Handling

async Task HandleHelpCommandAsync(Message message)
{
    await batBot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "/help - mostra o curriculo do pai\n/kill - desce o vapo em algum muquirano\n/w - chama os parça pro w",
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

    await batBot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{UserIDs.felipeUser}, {UserIDs.guilhermeUser}, {UserIDs.luisUser}, {UserIDs.marcosUser} e {UserIDs.batBotUser}, {randomChamada}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: default);
}

async Task HandleKillCommandAsync(Message message)
{
    string[] commandParts = message.Text.Split(' ');
    
    if (commandParts == null || commandParts.Length != 2 || !commandParts[1].StartsWith("@"))
    {
        await batBot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "tá chapando man? tem que marcar alguém assim:\n/kill @algumotário",
            cancellationToken: default
        );
        return;
    }

    string taggedUser = commandParts[1];
    
    if (taggedUser == "@batmanVoyeurBot")
    {
        await batBot.SendPhotoAsync(
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
        string killEmoji1 = "\U0001F91C";
        string killEmoji2 = "\U0001F4A5";
        
        Random rnd = new Random();
        string killGif = GifResources.GetRandomGif("kill_gifs");

        string escapedCaption = EscapeMarkdownCharacters(
            $"{message.From?.Username} {killEmoji1}{killEmoji2} {taggedUser}");

        await batBot.SendAnimationAsync(
            chatId: message.Chat.Id,
            animation: InputFile.FromFileId(killGif),
            caption: escapedCaption,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Markdown,
            cancellationToken: default
        );
    }
}

async Task HandleAddGifCommandAsync(ITelegramBotClient bot, Message message)
{
        // Define callback data for each category
        var categories = new List<string> { "Category1", "Category2", "Category3" };
        var inlineKeyboard = new InlineKeyboardMarkup(categories
            .Select(category => new[] { InlineKeyboardButton.WithCallbackData(category, category) }));

        // Send categories as callback buttons
        var sentMessage = await bot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Choose a category:",
            replyMarkup: inlineKeyboard,
            cancellationToken: default);

        // Store the sent message ID for later editing
        var sentMessageId = sentMessage.MessageId;

        // Store the chat ID for later use
        var chatId = message.Chat.Id;

        // Store the user ID for later use
        var userId = message.From.Id;

        // Store the message ID for later use
        var messageId = message.MessageId;

        // Save the chat ID and user ID for later use
        // (You may want to store this information in a database for more permanent storage)
        SaveUserChoice(chatId, userId, messageId);
    }

    void SaveUserChoice(long chatId, int userId, int messageId)
    {
        // Store the chat ID, user ID, and message ID in memory or a database for later use
        // You can use these values to identify the message to edit and to track the user's choice
    }

    async Task HandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        var category = callbackQuery.Data;

        // Edit the original message to prompt the user to reply with a gif
        await bot.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Reply to this message with the gif you want to add to the category: " + category,
            cancellationToken: default);
    }

    async Task HandleUserReplyAsync(ITelegramBotClient bot, Message message)
    {
        // Ensure that the message is a reply to the bot's message
        if (message.ReplyToMessage == null || message.ReplyToMessage.MessageId != messageId)
            return;

        // Ensure that the message contains a gif
        if (message.Document == null || message.Document.MimeType != "image/gif")
        {
            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please reply with a gif file.",
                cancellationToken: default);
            return;
        }

        // Download the gif
        var gifFile = await bot.GetFileAsync(message.Document.FileId);

        
        // Save the gif to the appropriate category folder
        var category = GetCategoryFromUserChoice(message.Chat.Id, message.From.Id);
        var filePath = Path.Combine("gifs", "misc", category, $"{Guid.NewGuid().ToString()}.gif");
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await bot.DownloadFileAsync(gifFile.FilePath, fileStream);
        }

        // Notify the user that the gif has been added
        await bot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Gif added to category: " + category,
            cancellationToken: default);
    }

    // Helper method to get the category based on user choice
    string GetCategoryFromUserChoice(long chatId, int userId)
    {
        // Retrieve the category based on the stored chat ID and user ID
        // You'll need to implement your logic to retrieve the user's choice from where you stored it
        // For simplicity, you can return a default category or handle errors appropriately
        return "DefaultCategory";
}



async Task HandleDebugStateAsync(Message message)
{
    await batBot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "ta em reforma man",
            replyToMessageId: message.MessageId,
            cancellationToken: default);
}

// async Task HandleStartMessageAsync()
// {
//     string songFilePath = @"/home/guilherme/batmanVoyeur/misc/music/Vordt of the Boreal Valley.mp3";

//     if (System.IO.File.Exists(songFilePath))
//     {
//         using (var stream = new FileStream(songFilePath, FileMode.Open))
//         {
//                 await batBot.SendAudioAsync(
//                     chatId: -1001717718883,
//                     audio: InputFile.FromStream(stream, "Vordt of the Boreal Valley.mp3")
//                 );
//         }
//     }
//     else
//     {
//         Console.WriteLine("nah nah b");
//     }
// }

#endregion

#region Error Handling

string EscapeMarkdownCharacters(string text)
{
    return Regex.Replace(text, @"([_\*\[\]\(\)~`>\#\+\-=\|\{\}\.\!])", @"\$1");
}

Task HandlePollingErrorAsync(ITelegramBotClient batBot, Exception exception, CancellationToken cancellationToken)
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
#region Namespace Imports

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BatBot;
using Newtonsoft.Json;

#endregion

#region Bot Start

string workDir = Environment.CurrentDirectory;
string workDirParent = Directory.GetParent(workDir).ToString();
string projDir;
if (workDirParent != null && workDirParent.EndsWith("Debug"))
{
    projDir = Directory.GetParent(workDir).Parent.Parent.FullName;
}
else
{
    projDir = workDir;
}

var batBot = new TelegramBotClient(AccessToken.BatToken);
var extra = new ExtraCommands(batBot);


bool debugMode = false;

await HandleStartMessageAsync();

long addGifRequesterId = 0;
long addGifRequesterChatId = 0;
int addGifInlineMessageId = 0;
int addGifWaitForGifMessageId = 0;
string addGifCategory = " ";

// Dictionary<long, Dictionary<string, object>> userContexts = new Dictionary<long, Dictionary<string, object>>();

using CancellationTokenSource cts = new ();

ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>(),
    ThrowPendingUpdates =  true
};

batBot.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
    );


var me = await batBot.GetMeAsync();



Console.WriteLine($"Start listening for @{me.Username}");


    
Console.ReadLine();

cts.Cancel();



#endregion

#region Update Handling

async Task HandleUpdateAsync(ITelegramBotClient batBot, Update update, CancellationToken cancellationToken)
{
    if(update.CallbackQuery is { } callbackQuery)
    {
        if(callbackQuery.From.Id == addGifRequesterId)
        {
            await HandleAddGifCategoryAsync(callbackQuery.Data, callbackQuery.From.Username);
        }
        else
        {
            await batBot.SendTextMessageAsync(
                chatId: addGifRequesterChatId,
                text: $"@{callbackQuery.From.Username} espera tua vez porra",
                cancellationToken: default
            );
        
            return;
        }
    };

    if (update.Message is not { } message)
        return;
    
    if (message.Animation is { } gif)
    {
        if (message.ReplyToMessage.MessageId ==  addGifWaitForGifMessageId)
        {
            if (message.From.Id == addGifRequesterId)
            {
                await HandleAddGifStore(gif.FileId);
            }
            else
            {
                await batBot.SendTextMessageAsync(
                    chatId: addGifRequesterChatId,
                    text: $"@{message.From.Username} espera tua vez porra",
                    cancellationToken: default
                );
            }
            
        }
        else
            return;
    }
    
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
            await HandleAddGifCommandAsync(message);
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
            text: "/help - mostra o curriculo do pai\n/kill @alguem - desce o vapo em algum muquirano\n/w - chama os parça pro w\n/addgif - adiciona um gif pra algum comando que manda gifs",
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
                text: $"{UserIDs.felipeUser}, {UserIDs.marcosUser}, {UserIDs.kaikyUser} e {UserIDs.batBotUser}, {randomChamada}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: default);
}

async Task HandleKillCommandAsync(Message message)
{
    string[] commandParts = message.Text.Split(' ');
    
    if (commandParts.Length < 2)
    {
        await batBot.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "tá chapando man? tem que marcar alguém assim:\n/kill @algumotário",
            cancellationToken: default
        );
        return;
    }

    string taggedUser = string.Join(" ", commandParts, 1, commandParts.Length - 1);
    
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
        
        string killGif = GifResources.GetRandomGif(Path.Combine(projDir, $"lists/gifs/kill_gifs.json"));

        if (killGif == "nada")
        {
            await batBot.SendTextMessageAsync(
                message.Chat.Id, "estoque mais vazio que dmc2",
                message.MessageId, default);
            return;
        }

        string escapedCaption = EscapeMarkdownCharacters(
            $"@{message.From?.Username} {killEmoji1}{killEmoji2} {taggedUser}");

        await using Stream stream = System.IO.File.OpenRead(killGif);
        await batBot.SendAnimationAsync(
            chatId: message.Chat.Id,
            animation: InputFile.FromStream(stream, "kill.gif"),
            caption: escapedCaption,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Markdown,
            cancellationToken: default
        );
    }
}

async Task HandleAddGifCommandAsync(Message message)
{    
    var replyMarkup = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("Kill", "kill")
            }
        });
            
    var inlineMessage = await batBot.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: "seleciona uma categoria ae:",
        replyToMessageId: message.MessageId,
        replyMarkup: replyMarkup
        );

    addGifRequesterId = message.From.Id;
    addGifInlineMessageId = inlineMessage.MessageId;
    addGifRequesterChatId = message.Chat.Id;
}

async Task HandleAddGifCategoryAsync(string category, string requesterUsername)
{    
    addGifCategory = category;

    await batBot.DeleteMessageAsync(addGifRequesterChatId, addGifInlineMessageId);
    
    var waitForGifMessage = await batBot.SendTextMessageAsync(
        chatId: addGifRequesterChatId,
        text: $"@{requesterUsername} responde essa mensagem aqui com o gif que tu quer adicionar ao comando {category}"
        );
    addGifWaitForGifMessageId = waitForGifMessage.MessageId;
    
}

async Task HandleAddGifStore(string gifFileId)
{
    string category = addGifCategory;
    string jsonFilePath = Path.Combine(projDir, $"lists/gifs/{category}_gifs.json");
    string gifFilePath = Path.Combine(projDir, $"misc/gifs/{category}/{category}{gifFileId}.gif");

    await using Stream fileStream = System.IO.File.Create(gifFilePath);
    
    await batBot.GetInfoAndDownloadFileAsync(
        fileId: gifFileId,
        destination: fileStream,
        cancellationToken: default
    );

    GifResources.AddGif(gifFilePath, jsonFilePath);

    await batBot.DeleteMessageAsync(addGifRequesterChatId, addGifWaitForGifMessageId);

    await batBot.SendTextMessageAsync(
        chatId: addGifRequesterChatId,
        text: "só sucesso meu querido"
    );
}

async Task HandleStartMessageAsync()
{    
    string jsonLocation = Path.Combine(projDir, "lists/corno_das_trevas.json");
    string jsonRetrieve = System.IO.File.ReadAllText(jsonLocation);
    string jsonDeserialize = JsonConvert.DeserializeObject<string>(jsonRetrieve);
    string[] attributes = jsonDeserialize.Split("@");
    int welcomeMessageId = int.Parse(attributes[0]);
    DateTime welcomeTimeSent = DateTime.Parse(attributes[1]);
    var currentTime = DateTime.UtcNow;
    TimeSpan difference = currentTime - welcomeTimeSent;
    
    if (welcomeMessageId != 0 && difference.Hours < 48)
    {
        await batBot.DeleteMessageAsync(UserIDs.manosChatId, welcomeMessageId);
    }

    await using Stream stream = System.IO.File.OpenRead($"{Path.Combine(projDir, "misc/videos/corno.mp4")}");
    
    var cornoMsg = await batBot.SendVideoAsync(
                chatId: UserIDs.manosChatId,
                video: InputFile.FromStream(stream: stream, fileName: "corno.mp4"),
                caption: "tô on dnv"
                );
    
    string jsonSave = JsonConvert.SerializeObject($"{cornoMsg.MessageId}@{cornoMsg.Date}");
    System.IO.File.WriteAllText(jsonLocation, jsonSave);
}

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
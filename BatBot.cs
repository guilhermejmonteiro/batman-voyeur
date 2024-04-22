#region Namespace Imports

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

// Inicializa uma instância do TelegramBotClient com o token de acesso providenciado
var botClient = new TelegramBotClient(AccessToken.BatToken);

// Declara que o CancellationTokenSource gerencia o cancelamento
using CancellationTokenSource cts = new ();

// Define opções pra receber atualizações
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

// Começa a receber atualizações da API do bot
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token);

// Adquire informações sobre o próprio bot
var me = await botClient.GetMeAsync();

// Printa uma mensagem indicando que o bot tá ouvindo
Console.WriteLine($"Start listening for @{me.Username}");

// Espera input do usuário antes de parar o bot
Console.ReadLine();

// Cancela a fonte do token pra parar de receber atualizações
cts.Cancel();

#region Update Handling

// Define um método assíncrono pra gerenciar atualizações iminentes
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Checa se a atualicação contém uma mensagem
    if (update.Message is not { } message)
        return;
    
    // Checa se a atualização contém texto
    if (message.Text is not { } messageText)
        return;
    
    // Extrai da mensagem o ID de chat
    var chatId = message.Chat.Id;

    // Printa indicando a mensagem recebida e o ID de seu chat
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    // Extrai o comando do texto da mensagem
    string command = message.Text.Split(' ')[0].ToLower();

    // Gerencia diferentes comandos baseado no comando extraído
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
    }    
}

#endregion

#region Command Handling

// Define um método para gerenciar o comando "/help"
async Task HandleHelpCommandAsync(Message message)
{
    // Manda uma mensagem de help contendo os comandos disponíveis
    await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "/help - mostra o curriculo do pai\n/kill - desce o vapo em algum muquira\n/w - chama os parça pro w",
            replyToMessageId: message.MessageId,
            cancellationToken: default);
}

// Define um método para gerenciar o comando "/w"
async Task HandleWRCommandAsync(Message message)
{
    // Define as possíveis mensagens pro comando
    string[] chamada = {"hora de tomar leite de macho do alistar",
                        "chamando todos os viados para o encontro de bixas \\(wr\\)",
                        "favor comparecer a queda semidiaria de rank",
                        "hora de subir pro mestre \\(ficar preso no esmeralda\\)"};
        
    // Seleciona uma mensagem aleatória da lista definida
    string randomChamada = chamada[new Random().Next(chamada.Length)];

    // Manda a mensagem seleciona junto com menções aos usuários específicos
    await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{UserIDs.felipeUser}, {UserIDs.guilhermeUser}, {UserIDs.luisUser}, {UserIDs.marcosUser} e {UserIDs.batBotUser}, {randomChamada}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: default);
}

// Define um método para gerenciar o comando "/kill"
async Task HandleKillCommandAsync(Message message)
{
    // Divide a mensagem de texto pra extrair o usuário mencionado
    string[] commandParts = message.Text.Split(' ');
    
    // Checa se o comando está no formato correto
    if (commandParts.Length != 2 || !commandParts[1].StartsWith("@"))
    {
        // Manda uma mensagem indicando o formato correto
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "tá chapando man? tem que marcar alguém assim:\n/kill @otário",
            cancellationToken: default
        );
        return;
    }

    // Extrai o usuário mencionado do comando
    string taggedUser = commandParts[1];

    // Define emojis e gifs pro comando
    string killEmoji1 = "\U0001F91C";
    string killEmoji2 = "\U0001F4A5";
    Random rnd = new Random();
    string KillGif = KillResources.KillGifs[rnd.Next(KillResources.KillGifs.Length)];

    // Checa se o usuário mencionado é o bot
    if (taggedUser == "@batmanVoyeurBot")
    {
        // Manda uma mensagem e foto específicas 
        await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            photo: InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQqG0SCSQgQZS3JXlpS6lGTsgf2r5GxqAoIfvZPMWN0umoUs2C2qeue3fg&s=10"),
            caption: "<b>ROYAL GUARD!</b>",
            parseMode: ParseMode.Html,
            cancellationToken: default
        );
    }
    else
    {
        // Manda um gif aleatório com a descrição mencionando os usuários envolvidos
        string escapedCaption = EscapeMarkdownCharacters(
            $"{message.From.Username} {killEmoji1}{killEmoji2} {taggedUser}");

        await botClient.SendAnimationAsync(
            chatId: message.Chat.Id,
            animation: InputFile.FromUri(KillGif),
            caption: escapedCaption,
            parseMode: ParseMode.Markdown,
            cancellationToken: default
        );
    }
}
    
#endregion

#region Error Handling

// Método pra exonerar caracteres de redução
string EscapeMarkdownCharacters(string text)
{
    return Regex.Replace(text, @"([_\*\[\]\(\)~`>\#\+\-=\|\{\}\.\!])", @"\$1");
}

// Método pra gerenciar erros que ocorrem durante o polling
Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Determina o tipo de exception e traça uma mensagem de erro apropriada
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    // rinta uma mensagem de erro
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

#endregion

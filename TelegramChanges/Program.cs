using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramChanges
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("742443229:AAEHNs7QYoLI9Aw463v72HbxrVmiKOgBwE8");
        private static List<Telegram.Bot.Types.Message> _messages = new List<Telegram.Bot.Types.Message>();
        static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username; 
            Bot.OnMessageEdited += BotOnMessageEdit;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;  
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.OnUpdate += BotOnUpdateReceived;
            Bot.OnMessage += BotOnMessageReseived;
            Bot.StartReceiving(Array.Empty<UpdateType>()); 
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }
        private static async void BotOnUpdateReceived(object sender, UpdateEventArgs e)
        {
            var message = e.Update.Message;
            if (message == null || message.Type != MessageType.Text) return;
            var text = message.Text; 
            await Bot.SendTextMessageAsync(message.Chat.Id, "_Recieved Update._", ParseMode.Markdown);
        }
        private static async void BotOnMessageReseived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            
            if (message == null || message.Type != MessageType.Text) return;
            _messages.Add(message);
            switch (message.Text.Split(' ').First())
            {
                case "/Работать":
                case "/Работа":
                case "/За_работу":
                    Message essage = await Bot.SendPhotoAsync(
chatId: message.Chat.Id,
photo: "https://i.ytimg.com/vi/ZIozHiT1ehs/hqdefault.jpg",
caption: "Опять работа",
parseMode: ParseMode.Html
);
                    break;
               
            }
            
        }
        private static async void BotOnMessageEdit(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return; 
            Message essage = await Bot.SendPhotoAsync(
  chatId: message.Chat.Id,
  photo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2Yvbr0zgZk8-OxIAshDvSnD_SB53a7-F_ywreqThBd67eIdVo6g",
  caption: "Пане президенте, хтось знову змінив повідомлення!",
  parseMode: ParseMode.Html
); 

            foreach (var m in _messages)
            {
                if(m.MessageId == message.MessageId)
                {
                     
                        await Bot.SendTextMessageAsync(message.Chat.Id, "From:\n " + m.Text + "\nTo:\n " + message.Text);
                     
                    }
            }
        }


        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }
        
        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}

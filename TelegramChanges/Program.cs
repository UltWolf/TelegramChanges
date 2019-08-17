
using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramChanges.Data;
using TelegramChanges.Services;
using File = System.IO.File;


namespace TelegramChanges
{
    class Program
    {
        private static readonly TelegramBotClient Bot =
            new TelegramBotClient("742443229:AAEHNs7QYoLI9Aw463v72HbxrVmiKOgBwE8");
        private static BotService _botService = new BotService(Bot); 
   

        static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Bot.OnMessageEdited += BotOnMessageEdit;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.OnMessage += BotOnMessageReseived;
            Bot.StartReceiving(Array.Empty<UpdateType>()); 
            Clocks();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private async static void Clocks()
        {
            TaskService.Initialize();
            while (true)
            { 
                    var now = DateTime.Now;
                    foreach (var state in TaskService.States.ToList())
                    {
                       
                        if(state.Time.ToString("f").Equals(now.ToString("f")))
                        {
                            var response =  await Bot.SendTextMessageAsync(state.ChatId, 
                                 "We are want remind you about:\n "+state.Message+
                                  "\nYou want take that message at:\n "+state.Time.ToString("F")
                                 +"\nHave a nice day.");

                            await Bot.PinChatMessageAsync(response.Chat.Id, response.MessageId);
                             
                             TaskService.States.Remove(state);
                             TaskService.SerializeTasks();
                             break;
                        }
                        var result = DateTime.Compare(now,state.Time);
                        if (result == 1)
                        {
                            TaskService.States.Remove(state);
                        }
                    }
                
                Thread.Sleep(TimeSpan.FromSeconds(30));
                TaskService.SerializeTasks();
            }
        }
      

        private static async void BotOnMessageReseived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message != null && message.Text!=null)
            {
                _botService.ExecuteCommand(message);
            }
        }

        private static async void BotOnMessageEdit(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;
            Message essage = await Bot.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo:
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2Yvbr0zgZk8-OxIAshDvSnD_SB53a7-F_ywreqThBd67eIdVo6g",
                caption: "Пане президенте, хтось знову змінив повідомлення!",
                parseMode: ParseMode.Html
            );
        }


        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
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
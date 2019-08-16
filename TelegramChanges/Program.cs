
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
using File = System.IO.File;


namespace TelegramChanges
{
    class Program
    {
        private static readonly TelegramBotClient Bot =
            new TelegramBotClient("742443229:AAEHNs7QYoLI9Aw463v72HbxrVmiKOgBwE8");

        private static List<Telegram.Bot.Types.Message> _messages = new List<Telegram.Bot.Types.Message>();
        private static List<ChatMember> _blackListMember = new List<ChatMember>();
        private static List<TaskState> States=new List<TaskState>();
        private static BinaryFormatter bf = new BinaryFormatter();
        private static int IdCount = 0;

        static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Bot.OnUpdate += BotOnUpdate;
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

        private static void Clocks()
        {
            Initialize();
            while (true)
            {
                lock (States)
                {
                    var now = DateTime.Now;
                    foreach (var state in States.ToList())
                    {
                       
                        if(state.Time.ToString("f").Equals(now.ToString("f")))
                        {
                             Bot.SendTextMessageAsync(state.ChatId, 
                                 "We are want remind you about:\n "+state.Message+
                                  "\nYou want take that message at:\n "+state.Time.ToString("F")
                                 +"\nHave a nice day.");
                             
                             
                             States.Remove(state);
                             SerializeTasks();
                             break;
                        }
                        var result = DateTime.Compare(now,state.Time);
                        if (result == 1)
                        {
                            States.Remove(state);
                        }
                    }
                }
               
                Thread.Sleep(TimeSpan.FromSeconds(30));
                SerializeTasks();
            }
        }

        private static void CreateTaskData()
        {
            if (!File.Exists("Tasks.data"))
            {
                File.Create("Tasks.data");
            }
        }

        private static void Initialize()
        {
            CreateTaskData();
            DeserializeTask();
        }
        private static void DeserializeTask()
        {
            lock (States)
            {
                try
                {
                    using (FileStream fs = new FileStream("Tasks.data", FileMode.Open, FileAccess.Read))
                    {
                        States = (List<TaskState>) bf.Deserialize(fs);
                        IdCount = States.Count;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }

        private static async void BotOnUpdate(object sender, UpdateEventArgs updateEventArgs)
        {
            Console.WriteLine(updateEventArgs.Update);
            
        }

        private static async void BotOnMessageReseived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message != null && message.Text!=null)
            {
                switch (message.Text.Split(' ').First().Replace("@ChangeUltWolfBot", ""))
                {
                    case "/work":
                        Message essage = await Bot.SendPhotoAsync(
                            chatId: message.Chat.Id,
                            photo: "https://i.ytimg.com/vi/ZIozHiT1ehs/hqdefault.jpg",
                            caption: "Опять работа",
                            parseMode: ParseMode.Html
                        );
                        break;
                    case "/unban":
                        try
                        {
                            int userId = _blackListMember.Find(m => m.User.Username == message.Text.Split(' ')[1]).User
                                .Id;
                            await Bot.UnbanChatMemberAsync(message.Chat, userId);
                        }
                        catch (Exception ex)
                        {
                            await Bot.SendTextMessageAsync(message.Chat,
                                "К сожалению мы не можем найти, или убрать со списка данного гражданина.");
                        }

                        break;
                    case "/add": 
                        try
                        {
                            var stringsArray = message.Text.Split(" ");
                            if (stringsArray.Length > 1)
                            {
                                StringBuilder stringBuilder = new StringBuilder("");
                                for (var i = 1; i < stringsArray.Length; i++)
                                {
                                    stringBuilder.Append(stringsArray[i] + " ");
                                }
                                string regexPattern = @"\[(.*?)\]";

                                var timeString = Regex.Match(stringBuilder.ToString(),
                                        regexPattern)
                                    .ToString()
                                    .Replace("[",
                                        "")
                                    .Replace("]",
                                        "");
                                var messageText = Regex.Replace(stringBuilder.ToString(), regexPattern,"");
                               
                                var task = new TaskState();
                                lock (States)
                                {

                                    DateTime time = DateTime.ParseExact(timeString,
                                        "MM-dd HH:mm", 
                                        System.Globalization.CultureInfo.InvariantCulture);
                                    if (States.Count > 0)
                                    {  var bytes = new byte[1];
                                        var rng = RandomNumberGenerator.Create();
                                        rng.GetBytes(bytes);
                                        uint random = BitConverter.ToUInt32(bytes, 0) % 1000;
                                        task = new TaskState((int)random, time, messageText,
                                            message.Chat.Id, message.From.Id);
                                        AddTask(task);
                                    }
                                    else
                                    {
                                        task = new TaskState(0, time, messageText,
                                            message.Chat.Id, message.From.Id);
                                        AddTask(task);
                                    }
                                }
                                messageText+= "\nIn time: " + timeString;
                                messageText += "\nTask Id is: " + task.TaskId;
                                await Bot.SendTextMessageAsync(message.Chat, messageText);
                                SerializeTasks();
                            } 
                            

                        }
                        catch (Exception ex)
                        {

                        }

                        break;
                    case "/list":
                        var resultText = new StringBuilder("You have next tasks:\n");
                        foreach (var state in States)
                        {
                            if (state.ChatId == message.Chat.Id)
                            {
                                resultText.Append($"{state.Time.ToString("f")} : {state.Message}\nIt`s have Id:{state.TaskId}\n\n");
                              
                            }
                        }
                        await Bot.SendTextMessageAsync(message.Chat, resultText.ToString());
                        break;
                    case "/remove":
                        var stringArray = message.Text.Split(" ");
                        var id = int.Parse(stringArray[1]);


                        var taskState = States.First(m => m.TaskId == id);
                        if(taskState!=null){
                                if (taskState.UserId == message.From.Id)
                                {
                                    States.Remove(taskState);
                                    SerializeTasks();
                                    await Bot.SendTextMessageAsync(taskState.ChatId, "Task have been successfull removed");
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(taskState.ChatId, "No, u can`t, it`s not your task, sorry");
                                }
                                break;
                            } 
                        break;
                }
            }
        }

        private static void AddTask(TaskState taskState)
        {
            States.Add(taskState);
            SerializeTasks();
        }

        private static void SerializeTasks()
        {
            lock (States)
            {
                using (FileStream fs = new FileStream("Tasks.data",FileMode.Create,FileAccess.Write))
                {
                    bf.Serialize(fs,States);
                }
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
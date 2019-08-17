using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramChanges.Abstracts;
using TelegramChanges.Data;

namespace TelegramChanges.Services
{
    
    public class BotService:IBotService
    {
        private readonly TelegramBotClient Bot; 

        public BotService(TelegramBotClient bot)
        {
            Bot = bot; 
        }
 
        public void ExecuteCommand(Message message)
        {
             
            BotCommands command;
            if (Enum.TryParse(GetCommand(message), out command))
            {
                switch (command)
                {
                    case BotCommands.WORK:
                        Work(message);
                        break;
                    case BotCommands.ADD:
                        Add(message);
                        break;
                    case BotCommands.LIST:
                        List(message);
                        break;
                    case BotCommands.REMOVE:
                        Remove(message);
                        break;
                }
            }
        }

        private string GetCommand(Message message)
        {
            return message.Text.Split(' ').First().Replace("@ChangeUltWolfBot", "").Trim('/').ToUpper();
        }

        public async void Add(Message message)
        {
             try {
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
                                lock (TaskService.States)
                                {

                                    DateTime time = DateTime.ParseExact(timeString,
                                        "MM-dd HH:mm", 
                                        System.Globalization.CultureInfo.InvariantCulture);
                                    if (TaskService.States.Count > 0)
                                    {  var bytes = new byte[1];
                                        var rng = RandomNumberGenerator.Create();
                                        rng.GetBytes(bytes);
                                        uint random = BitConverter.ToUInt32(bytes, 0) % 1000;
                                        task = new TaskState((int)random, time, messageText,
                                            message.Chat.Id, message.From.Id);
                                        TaskService.AddTask(task);
                                    }
                                    else
                                    {
                                        task = new TaskState(0, time, messageText,
                                            message.Chat.Id, message.From.Id);
                                        TaskService.AddTask(task);
                                    }
                                }
                                messageText+= "\nIn time: " + timeString;
                                messageText += "\nTask Id is: " + task.TaskId;
                                await Bot.SendTextMessageAsync(message.Chat, messageText);
                                TaskService.SerializeTasks();
                            } 
                            

                        }
                        catch (Exception ex)
                        {

                        }

        }

        public void Edit(Message message)
        {
            throw new System.NotImplementedException();
        }

        public async void Remove(Message message)
        {
            var stringArray = message.Text.Split(" ");
            var id = int.Parse(stringArray[1]);


            var taskState = TaskService.States.First(m => m.TaskId == id);
            if(taskState!=null){
                if (taskState.UserId == message.From.Id)
                {
                    TaskService.States.Remove(taskState);
                    TaskService.SerializeTasks();
                    await Bot.SendTextMessageAsync(taskState.ChatId, "Task have been successfull removed");
                }
                else
                {
                    await Bot.SendTextMessageAsync(taskState.ChatId, "No, u can`t, it`s not your task, sorry");
                }
            } 
        }

        public async void List(Message message)
        {
            var resultText = new StringBuilder("You have next tasks:\n");
            foreach (var state in TaskService.States)
            {
                if (state.ChatId == message.Chat.Id)
                {
                    resultText.Append($"{state.Time.ToString("f")} : {state.Message}\nIt`s have Id:{state.TaskId}\n\n");
                              
                }
            }
            await Bot.SendTextMessageAsync(message.Chat, resultText.ToString());
            
        }

        public void Update(Message message)
        {
            throw new System.NotImplementedException();
        }

        public async void Work(Message message)
        {
            Message messageResponse = await Bot.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: "https://i.ytimg.com/vi/ZIozHiT1ehs/hqdefault.jpg",
                caption: "Опять работа",
                parseMode: ParseMode.Html
                
            );
        }
       
    }
}
using System;
using Telegram.Bot.Types;

namespace TelegramChanges.Data
{
    [Serializable]
    public class TaskState
    {
        public int TaskId { get; set; }
        public DateTime Time { get; private set; }
        public string Message { get; private set; }
        
        public long ChatId { get; private set; }
        public long UserId { get; set; }

        public TaskState()
        {
            
        }
        public TaskState(int taskId, DateTime time, string message, long chatId, long userId)
        {
            TaskId = taskId;
            Time = time;
            Message = message;
            ChatId = chatId;
            UserId = userId;
        }
    }
}
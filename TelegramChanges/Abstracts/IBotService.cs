using Telegram.Bot.Types;

namespace TelegramChanges.Abstracts
{
    public interface IBotService
    {
        void ExecuteCommand(Message message);
        void Add(Message message);
        void Edit(Message message);
        void Remove(Message message);
        void List(Message message);
        void Update(Message message);
        void Work(Message message);
    }
}
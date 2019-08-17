using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TelegramChanges.Data;

namespace TelegramChanges.Services
{
    public class TaskService
    {
        public static List<TaskState> States=new List<TaskState>();
        
        private static BinaryFormatter bf = new BinaryFormatter();
        public static void AddTask(TaskState taskState)
        {
            States.Add(taskState);
            SerializeTasks();
        }

        public static void SerializeTasks()
        {
            lock (States)
            {
                using (FileStream fs = new FileStream("Tasks.data",FileMode.Create,FileAccess.Write))
                {
                    bf.Serialize(fs,States);
                }
            }
            
            
        }
        private static List<TaskState> DeserializeTask()
        {
            lock (States)
            {
                try
                {
                    using (FileStream fs = new FileStream("Tasks.data", FileMode.Open, FileAccess.Read))
                    {
                        return (List<TaskState>) bf.Deserialize(fs); 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }

            return null;
        }
        public static List<TaskState> Initialize()
        {
            CreateTaskData();
            return DeserializeTask();
        }
        private static void CreateTaskData()
        {
            if (!File.Exists("Tasks.data"))
            {
                File.Create("Tasks.data");
            }
        }
    }
}
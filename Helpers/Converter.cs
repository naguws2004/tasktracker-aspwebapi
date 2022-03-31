using System.Collections.Generic;
using TaskWebAPI.Models;

namespace TaskWebAPI.Helpers
{
    public class Converter
    {
        public List<TaskModel> Convert(List<Task> objList)
        {
            List<TaskModel> list = new List<TaskModel>();

            foreach (var obj in objList)
            {
                list.Add(
                    new TaskModel
                    {
                        TaskId = obj.TaskId,
                        TaskName = obj.TaskName,
                        TaskDateTime = obj.TaskDateTime,
                        Remind = obj.Remind
                    }
                );
            }

            return list;
        }

        public TaskModel ConvertSingle(Task obj)
        {
            return
                new TaskModel
                {
                    TaskId = obj.TaskId,
                    TaskName = obj.TaskName,
                    TaskDateTime = obj.TaskDateTime,
                    Remind = obj.Remind
                };
        }
    }
}

using System;
using System.Text.Json.Serialization;

namespace TaskWebAPI.Models
{
    public class TaskModel
    {
        public int TaskId { get; set; } = 0;

        public string TaskName { get; set; } = string.Empty;

        public DateTime TaskDateTime { get; set; } = DateTime.MinValue;

        public bool Remind { get; set; } = false;
    }
}

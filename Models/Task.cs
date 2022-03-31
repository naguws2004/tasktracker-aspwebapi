using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TaskWebAPI.Models
{
    public enum UpdateMarker
    {
        Default = 0,
        Insert = 1,
        Update = 2,
        Delete = 3
    }

    public class Task
    {
        [IgnoreDataMember]
        public ObjectId Id { get; set; } = ObjectId.Empty;
        public int TaskId { get; set; } = 0;
        public string TaskName { get; set; } =  string.Empty;
        public DateTime TaskDateTime { get; set; } = DateTime.MinValue;
        public bool Remind { get; set; } = false;

        [IgnoreDataMember]
        public UpdateMarker Marker { get; set; } = UpdateMarker.Default; 
    }

    public class TaskCRUD
    {
        private readonly MongoClient _mongoClient;

        public TaskCRUD(MongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public string InsertTask(Task task)
        {
            int maxTaskId = GetMaxTaskId();
            task.TaskId = maxTaskId + 1;
            _mongoClient.GetDatabase("TaskTracker").GetCollection<Task>("Tasks").InsertOne(task);
            return "New Task Added to Database Successfully.";
        }

        public string UpdateTask(Task task)
        {
            var updateFilter = Builders<Task>.Filter.Eq("TaskId", task.TaskId);
            var update = Builders<Task>.Update.Set("Remind", task.Remind);
            var updateResult = _mongoClient.GetDatabase("TaskTracker").GetCollection<Task>("Tasks").UpdateOne(updateFilter, update);
            return UpdateResultMessage(updateResult);
        }

        public string DeleteTask(int taskId)
        {
            var deleteFilter = Builders<Task>.Filter.Eq("TaskId", taskId);
            var deleteResult = _mongoClient.GetDatabase("TaskTracker").GetCollection<Task>("Tasks").DeleteOne(deleteFilter);
            return DeleteResultMessage(deleteResult);
        }

        public List<Task> GetTasks()
        {
            var dbList = _mongoClient.GetDatabase("TaskTracker").GetCollection<Task>("Tasks").AsQueryable();
            return dbList.ToList();
        }

        public string UpdateTasksSuccessMessage()
        {
            var dbList = GetTasks();
            int taskCounter = dbList.Count();
            return taskCounter.ToString() + " Tasks. Database Successfully Updated";
        }

        public List<Task> UpdateTasksMarker(List<Task> tasks)
        {
            var dbList = GetTasks();

            foreach (var task in tasks)
            {
                if (!dbList.Any(x => x.TaskId == task.TaskId))
                {
                    // Mark for Insert
                    task.Marker = UpdateMarker.Insert;
                }
                else if (dbList.Any(x => x.TaskId == task.TaskId))
                {
                    // Mark for Update
                    task.Marker = UpdateMarker.Update;
                }
            }

            foreach (var dbTask in dbList)
            {
                if (!tasks.Any(x => x.TaskId == dbTask.TaskId))
                {
                    // Add and Mark for Delete
                    var task = dbTask;
                    task.Marker = UpdateMarker.Delete;
                    tasks.Add(task);
                }
            }

            return tasks;
        }

        private int GetMaxTaskId()
        {
            var dbList = GetTasks();
            return dbList.Max(x => x.TaskId);
        }

        private string UpdateResultMessage(UpdateResult updateResult)
        {
            if (updateResult.IsAcknowledged)
            {
                if (updateResult.MatchedCount > 0)
                {
                    if (updateResult.ModifiedCount > 0)
                    {
                        return "Task Updated in Database Successfully.";
                    }
                    else
                    {
                        return "Unable to Update Task.";
                    }
                }
                else
                {
                    return "No matching Task found.";
                }
            }
            else
            {
                return "Not Acknowledged by Database.";
            }
        }

        private string DeleteResultMessage(DeleteResult deleteResult)
        {
            if (deleteResult.IsAcknowledged)
            {
                if (deleteResult.DeletedCount > 0)
                {
                    return "Task Deleted from Database Successfully.";
                }
                else
                {
                    return "No matching Task found.";
                }
            }
            else
            {
                return "Not Acknowledged by Database.";
            }
        }
    }
}

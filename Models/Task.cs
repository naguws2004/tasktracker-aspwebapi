using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TaskWebAPI.Helpers;

namespace TaskWebAPI.Models
{
    public class Task
    {
        [JsonIgnore]
        public ObjectId Id { get; set; } = ObjectId.Empty;

        public int TaskId { get; set; } = 0;

        public string TaskName { get; set; } =  string.Empty;

        public DateTime TaskDateTime { get; set; } = DateTime.MinValue;

        public bool Remind { get; set; } = false;
    }

    public class TaskCRUD
    {
        private readonly MongoClient _mongoClient;

        public TaskCRUD(MongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public string InsertTask(TaskModel task)
        {
            int maxTaskId = GetMaxTaskId();
            task.TaskId = maxTaskId + 1;
            _mongoClient.GetDatabase("TaskTracker").GetCollection<TaskModel>("Tasks").InsertOne(task);
            return "New Task Added to Database Successfully.";
        }

        public string UpdateTask(TaskModel task)
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

        public List<TaskModel> GetTasks()
        {
            var dbList = _mongoClient.GetDatabase("TaskTracker").GetCollection<Task>("Tasks").AsQueryable();
            List<TaskModel> list = new Converter().Convert(dbList.ToList());
            return list;
        }

        public void UpdateTasks(List<TaskModel> tasks)
        {
            List<UpdateMarker> markers = UpdateTaskMarker(tasks);
            var task = new TaskModel();

            foreach (var updateMarker in markers)
            {
                switch (updateMarker.Marker)
                {
                    case MarkerType.Insert:
                        task = tasks.FirstOrDefault(x => x.TaskId == updateMarker.TaskId);
                        InsertTask(task);
                        break;
                    case MarkerType.Update:
                        task = tasks.FirstOrDefault(x => x.TaskId == updateMarker.TaskId);
                        UpdateTask(task);
                        break;
                    case MarkerType.Delete:
                        DeleteTask(updateMarker.TaskId);
                        break;
                    default:
                        break;
                }
            }
        }

        public string UpdateTasksSuccessMessage()
        {
            var dbList = GetTasks();
            int taskCounter = dbList.Count();
            return taskCounter.ToString() + " Tasks. Database Successfully Updated";
        }

        private List<UpdateMarker> UpdateTaskMarker(List<TaskModel> tasks)
        {
            List<UpdateMarker> updateMarkers = new List<UpdateMarker>();

            var dbList = GetTasks();

            UpdateAddAndUpdateMarker(updateMarkers, tasks, dbList);
            
            UpdateDeleteMarker(updateMarkers, tasks, dbList);

            return updateMarkers;
        }

        private List<UpdateMarker> UpdateAddAndUpdateMarker(List<UpdateMarker> updateMarkers, List<TaskModel> tasks, List<TaskModel> dbList)
        {
            foreach (var task in tasks)
            {
                if (!dbList.Any(x => x.TaskId == task.TaskId))
                {
                    // Mark for Insert
                    updateMarkers.Add(new UpdateMarker
                    {
                        TaskId = task.TaskId,
                        Marker = MarkerType.Insert
                    });
                }
                else if (dbList.Any(x => x.TaskId == task.TaskId))
                {
                    // Mark for Update
                    updateMarkers.Add(new UpdateMarker
                    {
                        TaskId = task.TaskId,
                        Marker = MarkerType.Update
                    });
                }
            }

            return updateMarkers;
        }

        private List<UpdateMarker> UpdateDeleteMarker(List<UpdateMarker> updateMarkers, List<TaskModel> tasks, List<TaskModel> dbList)
        {
            foreach (var dbTask in dbList)
            {
                if (!tasks.Any(x => x.TaskId == dbTask.TaskId))
                {
                    // Mark for Delete
                    updateMarkers.Add(new UpdateMarker
                    {
                        TaskId = dbTask.TaskId,
                        Marker = MarkerType.Delete
                    });
                }
            }

            return updateMarkers;
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

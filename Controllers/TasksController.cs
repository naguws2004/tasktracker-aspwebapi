using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using TaskWebAPI.Models;

namespace TaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TaskCRUD _taskCRUD;

        public TasksController(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("TaskTrackerConn"));

            _configuration = configuration;
            _taskCRUD = new TaskCRUD(mongoClient);
        }

        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult(_taskCRUD.GetTasks());
        }

        [HttpPost]
        public JsonResult Post([FromBody] TaskModel task)
        {
            var result = _taskCRUD.InsertTask(task);
            return new JsonResult(result);
        }

        [HttpPut]
        public JsonResult Put([FromBody] TaskModel task)
        {
            var result = _taskCRUD.UpdateTask(task);   
            return new JsonResult(result);
        }

        [HttpDelete("{taskId:int}")]
        public JsonResult Delete(int taskId)
        {
            var result = _taskCRUD.DeleteTask(taskId);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("UpdateTasks")]
        public JsonResult UpdateTasks([FromBody] List<TaskModel> tasks)
        {
            _taskCRUD.UpdateTasks(tasks);
            return new JsonResult(_taskCRUD.UpdateTasksSuccessMessage());
        }
    }
}

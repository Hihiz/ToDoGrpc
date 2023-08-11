using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services
{
    public class ToDoService : ToDoIt.ToDoItBase
    {
        private readonly ApplicationDbContext _db;

        public ToDoService(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST
        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (request.Title == string.Empty || request.Description == string.Empty)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Вы должны указать допустимый объект"));
            }

            ToDoItem toDoItem = new ToDoItem
            {
                Title = request.Title,
                Description = request.Description
            };

            await _db.AddAsync(toDoItem);
            await _db.SaveChangesAsync();

            return await Task.FromResult(new CreateToDoResponse
            {
                Id = toDoItem.Id
            });
        }

        // GET {id}
        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Индекс ресурса должен быть больше 0"));
            }

            ToDoItem toDoItem = await _db.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem != null)
            {
                return await Task.FromResult(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"Нет задачи с идентификатором {request.Id}"));
        }

        // GET all
        public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
        {
            GetAllResponse response = new GetAllResponse();
            List<ToDoItem> toDoItems = await _db.ToDoItems.ToListAsync();

            foreach (var item in toDoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    ToDoStatus = item.ToDoStatus
                });
            }

            return await Task.FromResult(response);
        }

        // PUT
        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Вы должны указать допустимый объект"));
            }

            ToDoItem toDoItem = await _db.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Нет задачи с идентификатором {request.Id}"));
            }

            toDoItem.Title = request.Title;
            toDoItem.Description = request.Description;
            toDoItem.ToDoStatus = request.ToDoStatus;

            await _db.SaveChangesAsync();

            return await Task.FromResult(new UpdateToDoResponse
            {
                Id = toDoItem.Id
            });
        }
        
        // DELETE
        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Индекс ресурса должен быть больше 0"));
            }

            ToDoItem toDoItem = await _db.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Нет задачи с идентификатором {request.Id}"));
            }

            _db.Remove(toDoItem);

            await _db.SaveChangesAsync();

            return await Task.FromResult(new DeleteToDoResponse
            {
                Id = toDoItem.Id
            });
        }
    }
}

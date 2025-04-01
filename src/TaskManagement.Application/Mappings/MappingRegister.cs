using System;
using System.Linq;
using Mapster;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Events;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Mappings
{
    /// <summary>
    /// Configuration for Mapster mappings
    /// </summary>
    public class MappingRegister : IRegister
    {
        /// <summary>
        /// Registers mapping configurations for Mapster
        /// </summary>
        /// <param name="config">Mapster configuration</param>
        public void Register(TypeAdapterConfig config)
        {
            // Domain to Response mapping
            config.NewConfig<TaskItem, TaskResponse>()
                .Map(dest => dest.Status, src => src.Status.ToString());

            // Request to Domain mapping
            config.NewConfig<CreateTaskRequest, TaskItem>()
                .Map(dest => dest.Status, _ => TaskStatus.NotStarted);

            // Domain to Event mapping
            config.NewConfig<TaskItem, TaskCreatedEvent>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.TaskName, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.CreatedAt, _ => DateTimeOffset.UtcNow);

            config.NewConfig<TaskItem, TaskUpdatedEvent>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.TaskName, src => src.Name)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.UpdatedAt, _ => DateTimeOffset.UtcNow);
                
            config.NewConfig<TaskItem, TaskAssignedEvent>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.TaskName, src => src.Name)
                .Map(dest => dest.AssigneeName, src => src.AssignedTo)
                .Map(dest => dest.AssignedAt, _ => DateTimeOffset.UtcNow);
        }
    }
} 
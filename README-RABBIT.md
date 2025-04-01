# Task Management System

A task management application with service bus integration using RabbitMQ.

## Prerequisites

- .NET 8.0 SDK
- SQL Server LocalDB or SQL Server
- Docker Desktop (for RabbitMQ)

## Setting up RabbitMQ

The application uses RabbitMQ as a message broker for service bus functionality. Follow these steps to set it up:

1. Make sure Docker Desktop is installed and running
2. From the root of the project, run:

```
docker-compose up -d
```

This will start RabbitMQ in a Docker container with the following settings:
- Host: localhost
- Port: 5672
- Username: guest
- Password: guest
- Management UI: http://localhost:15672 

## Running the Application

1. Ensure that SQL Server LocalDB is installed and running
2. Ensure that RabbitMQ is running (via Docker)
3. From the TaskManagementSystem directory, run:

```
dotnet run --project src/TaskManagement.Api/TaskManagement.Api.csproj
```

4. Access the API at http://localhost:7192
5. Swagger UI is available at http://localhost:7192

## Message Consumers

The application has consumers for the following event messages:
- TaskCreatedEvent - When a new task is created
- TaskUpdatedEvent - When a task status is updated
- TaskAssignedEvent - When a task is assigned to someone

## Development Notes

- In development mode, if RabbitMQ is not available, the application will use a dummy service bus handler that logs messages instead of sending/receiving them.
- The application seeds the database with sample data if the database is empty.
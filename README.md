# Task Management System with Service Bus Integration

A Web API application built with C# and .NET that simulates a Task Management System with integration to a service bus (RabbitMQ).

## Project Structure

The project follows Clean Architecture and Domain-Driven Design principles:

- **TaskManagement.Api**: Web API controllers and endpoints
- **TaskManagement.Domain**: Contains the core business entities and interfaces
- **TaskManagement.Application**: Contains the business logic and use cases
- **TaskManagement.Infrastructure**: Provides implementations for data access and external services
- **TaskManagement.ServiceBus**: Handles service bus integration with RabbitMQ
- **TaskManagement.Tests**: Contains unit tests

## Features

The application allows users to perform the following tasks:
1. Add new tasks
2. Update task status and assignee
3. Show the list of tasks and their status
4. Delete tasks
5. 

## Setup and Run

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose

### Steps to Run
1. Clone the repository
2. Start RabbitMQ container:
   ```
   docker-compose up -d
   ```
3. Build the solution:
   ```
   dotnet build
   ```
4. Run the API:
   ```
   cd src/TaskManagement.Api
   dotnet run
   ```

## API Endpoints

The following API endpoints are available:
- `POST /api/tasks` - Add a new task
- `PUT /api/tasks/{id}/status` - Update task status
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by id
- `DELETE /api/task/{id}` - Deleted task by id
- `PUT /api/tasks/{id}/assign` - Update task assignee

## Architecture

The application implements:
- Clean Architecture principles
- Entity Framework Core for data access
- RabbitMQ for service bus integration
- Dependency Injection 
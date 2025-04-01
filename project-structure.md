# Task Management System Project Structure

```
TaskManagementSystem/
├── src/
│   ├── TaskManagement.Api/             # Web API layer
│   │   └── Controllers/                # API Controllers
│   │   
│   ├── TaskManagement.Domain/          # Domain layer
│   │   ├── Entities/                   # Domain entities
│   │   ├── Enums/                      # Domain enumerations
│   │   └── Interfaces/                 # Domain interfaces
│   │   
│   ├── TaskManagement.Application/     # Application layer
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   ├── Interfaces/                 # Application interfaces
│   │   ├── Services/                   # Application services
│   │   └── Mapping/                    # Object mapping
│   │   
│   ├── TaskManagement.Infrastructure/  # Infrastructure layer
│   │   ├── Data/                       # Data access
│   │   │   ├── Context/                # Database contexts
│   │   │   ├── Repositories/           # Repository implementations
│   │   │   └── Configuration/          # Entity configurations
│   │   └── Services/                   # External service implementations
│   │   
│   └── TaskManagement.ServiceBus/      # Service Bus integration
│       ├── Handlers/                   # Message handlers
│       ├── Services/                   # Service bus services
│       └── Models/                     # Message models
│   
├── tests/
│   └── TaskManagement.Tests/           # Test project
│       ├── Unit/                       # Unit tests~~~~
│   
├── docker-compose.yml                  # Docker compose for RabbitMQ
├── README.md                           # Project documentation
└── TaskManagementSystem.sln            # Solution file
```

## Dependencies

- **TaskManagement.Domain**: No dependencies
- **TaskManagement.Application**: Depends on Domain
- **TaskManagement.Infrastructure**: Depends on Domain and Application
- **TaskManagement.ServiceBus**: Depends on Domain and Application
- **TaskManagement.Api**: Depends on Domain, Application, Infrastructure, and ServiceBus
- **TaskManagement.Tests**: Depends on all projects for testing

## Technology Stack

- ASP.NET Core 8.0
- Entity Framework Core
- RabbitMQ for service bus integration
- Dependency Injection
- Docker (for RabbitMQ) 
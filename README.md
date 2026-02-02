# PostgreSQL Event Sourcing + Kafka Event-Driven Banking System

A comprehensive Event Sourcing implementation using PostgreSQL as the event store, with Kafka for event-driven architecture and Docker for containerization.

## 🏗️ Architecture Overview

This solution implements a complete event sourcing pattern and event-driven architecture with the following components:

### Core Components

- **EventSourcing** - Core event sourcing logic and interfaces, including aggregate roots, domain events, and event store abstractions
- **EventSourcing.Postgres** - PostgreSQL-based event store implementation
- **EventBus** - Core event bus logic and interfaces, including event consumers, event handlers, and integration event definitions
- **EventBus.Kafka** - Kafka-based event bus for reliable message delivery and processing
- **ESsample.Banking.API** - Web API that handles banking operations and publishes integration events. Features are created using Vertical Slice Architecture and CQRS pattern
- **ESsample.Banking.Shared** - Shared contracts and events between producer and consumers
- **Projections.Banking** - A sample projection logic to build read models from the different integration events consumed
- **Projections.Banking.API** - Web API to query the read models built by the consumer
- **Projections.Banking.Consumer** - Background service that consumes and processes integration events in order to update the read models
- **Projections.Bankisg.Postgres** - PostgreSQL implementation for read models storage


### Infrastructure

- **PostgreSQL** - Event store database
- **Entity Framework Core** - ORM for database interactions
- **Automated Migrations** - Database schema management using Entity Framework Core migrations
- **Apache Kafka** - Message broker for event streaming
- **Kafka UI** - Web interface for monitoring Kafka topics and messages
- **Zookeeper** - Coordination service for Kafka
- **Docker** - Containerization platform
- **Docker Compose** - Orchestration tool for the multi-container Docker application
- **Health Checks** - Built-in health checks for services to monitor their status
- **Logging** - Structured logging using Serilog for better observability
- **Swagger/OpenAPI** - API documentation and testing interface
- **Razor Pages** - Simple web front-end for user interactions 
- **xUnit** - Unit testing framework for .NET
- **Moq** - Mocking library for unit tests
- **Testcontainers** - Integration testing with real dependencies in Docker containers

### Patterns and Practices

- **Event Sourcing** - Storing state changes as a sequence of events
- **CQRS (Command Query Responsibility Segregation)** - Separate models for reading and writing data
- **Domain-Driven Design (DDD)** - Structuring the code around the business domain
- **Vertical Slice Architecture** - Organizing code by feature rather than by layer
- **Event-Driven Architecture** - Decoupling components through events
- **Dependency Injection** - Managing dependencies through constructor injection
- **Asynchronous Programming** - Using async/await for non-blocking operations

## 🚀 Getting Started

### Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK (for local development)
- Git
- pgAdmin or any PostgreSQL client (optional, for database inspection)

### Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PostgresEventSourcing
   ```

2. **Start all services**
   ```bash
   docker-compose up -d --build
   ```

3. **Verify services are running**
   ```bash
   docker-compose ps
   ```

4. **Access the services**
   - Event Sourcing Banking Web service: http://localhost:8082
   - Banking Projections Web service: http://localhost:8084
   - Kafka UI: http://localhost:8080

### Local Development

1. **Start infrastructure services**
   ```bash
   docker-compose up -d postgres kafka zookeeper kafka-ui
   ```

2. **Run the Event Sourcing Banking API**
   ```bash
   cd ESsample.Banking.API
   dotnet run
   ```

3. **Run the Consumer (in another terminal)**
   ```bash
   cd Projections.Banking.Consumer
   dotnet run
   ```

4. **Run theBanking Projections API**
   ```bash
   cd Projections.Banking.API
   dotnet run
   ```

## 📋 API Endpoints

### Account Management

- **POST** `/api/accounts` - Create a new account
- **GET** `/api/accounts/{id}` - Get account details
- **GET** `/api/accounts/{id}/history` - Get account event history
- **POST** `/api/accounts/{id}/deposit` - Deposit money
- **POST** `/api/accounts/{id}/withdraw` - Withdraw money
- **POST** `/api/accounts/{id}/transfer` - Money transfer between accounts

#### Example Usage

**Create Account:**
```bash
curl -X POST "http://localhost:8082/api/accounts" \\
  -H "Content-Type: application/json" \\
  -d '{
    "accountNumber": "ACC001",
    "accountName": "My savings account",
    "ownerName": "John Doe",
    "initialBalance": 1000.00
  }'
```

**Deposit Money:**
```bash
curl -X POST "http://localhost:8082/api/accounts/{account-id}/deposit" \\
  -H "Content-Type: application/json" \\
  -d '{
    "amount": 500.00,
    "description": "Salary deposit"
  }'
```

#### Swagger UI

Access Swagger UI at `http://localhost:8082/swagger` to explore and test the API endpoints.

#### Razor Pages

Access the simple web front-end at `http://localhost:8082` to visit the home page and interact with the banking features, including Swagger UI, Account History, and Account Balance web pages.

### Projections API

- **GET** `/api/reports/balances` - List all account balancess from the reading model
- **GET** `/api/reports/transactions/{id}` - List all transactions for the specified account in the reading model

#### Swagger UI

Access Swagger UI at `http://localhost:8084/swagger` to explore and test the API endpoints.

#### Razor Pages

Access the simple web front-end at `http://localhost:8084` to visit the home page and interact with the banking projections features, including Swagger UI, Account Balances, and Account Transactions web pages.

## 🔄 Event Flow

1. **Command Processing**: Event Sourcing Banking API receives commands (create account, deposit, withdraw, transfer)
2. **Event Generation**: Domain events are generated and stored in the event store
3. **Integration Events**: Integration events are published to Kafka event bus
4. **Event Consumption**: Consumer service processes integration events
5. **Side Effects**: Projections Consumer triggers notifications, updates read models, etc.
6. **Read Models**: Projections API queries the read models built by the consumer and lists account balances and transactions

### Integration Events

- **AccountCreatedEvent** - Published when a new account is created
- **AccountBalanceUpdatedEvent** - Published when account balance changes ( deposits, withdrawals or transfers)

## 🐳 Docker Services

| Service | Port | Description |
|---------|------|-------------|
| banking-producer | 8082 | Banking API (Event Sourcing) |
| banking-consumer | - | Integration Events consumer service |
| banking-projections | 8084 | Projections API (reading models) |
| banking-postgres-db | 5432 | PostgreSQL host for event storage and reading models databases |
| kafka | 9092 | Kafka message broker |
| kafka-ui | 8080 | Kafka monitoring UI |
| zookeeper | 2181 | Kafka coordination service |

## 📊 Monitoring

### Kafka UI

Access Kafka UI at http://localhost:8080 to:
- View topics and messages
- Monitor consumer groups
- Check message throughput
- Debug event processing

### Logs

View service logs:
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f banking-consumer
docker-compose logs -f banking-producer-api
docker-compose logs -f banking-projections-api
```

## 🔧 Configuration

### Environment Variables

**Banking Producer (ESsample.Banking.API):**
- `ConnectionStrings__EventStore` - PostgreSQL connection string for event store
- `Kafka__BootstrapServers` - Kafka bootstrap servers
- `Kafka__TopicPrefix` - Kafka topic prefix for integration events

**Banking Consumer: (Projections.Banking.Consumer)**
- `ConnectionStrings__ReadingModels` - PostgreSQL connection string for ethe reading models database
- `Kafka__BootstrapServers` - Kafka bootstrap servers
- `Kafka__TopicPrefix` - Topic prefix for integration events

**Banking Projections: (Projections.Banking.API)**
- `ConnectionStrings__ReadingModels` - PostgreSQL connection string for ethe reading models database

### Database Schema

## 🗄️ External Database Configuration

This solution connects to external PostgreSQL databases instead of running its own instance:

### EventStore:

- **Database Server**: 127.0.0.1:5432
- **Database Name**: eventstore
- **Connection String**: Host=banking-postgres-db;Port=5432;Database=eventstore;Username=postgres;Password=balrog

### Reading Models:

- **Database Server**: 127.0.0.1:5432
- **Database Name**: banking
- **Connection String**: Host=banking-postgres-db;Port=5432;Database=banking;Username=postgres;Password=balrog

### Database Setup

Before running the application, ensure your external PostgreSQL databases have the required schemas. You can run the migration manually:

### EventStore:

```bash
# Connect to your PostgreSQL instance
psql -h 127.0.0.1 -p 5432 -U postgres -d eventstore

# The application will automatically create the required tables on first run
# Or you can run the migration manually using Entity Framework:
dotnet ef database update --project EventSourcing.Postgres --startup-project ESsample.Banking.API
```

The event store uses a single table `mt_events` with the following structure:
- `seq_id` - Auto-incrementing sequence ID
- `id` - Event ID (UUID)
- `stream_id` - Aggregate ID (UUID)
- `version` - Event version within the stream
- `data` - Event payload (JSONB)
- `type` - Event type name
- `timestamp` - Event creation timestamp
- `tenant_id` - Tenant identifier for multi-tenancy support
- `mt_dotnet_type` - .NET type information for deserialization
- `is_archived` - Flag for soft deletion

### Reading Models:

```bash
# Connect to your PostgreSQL instance
psql -h 127.0.0.1 -p 5432 -U postgres -d banking

# The application will automatically create the required tables on first run
# Or you can run the migration manually using Entity Framework:
dotnet ef database update --project Projections.Banking.Postgres --startup-project Projections.Banking.API
```

The reading models database contains two tables:
- `Balances` - Stores current balances for each account
- `Transactions` - Stores transaction history for each account

The `Balances` table structure is as follows:
- `AccountId` - Primary key (UUID)
- `AccountNumber` - Unique account number
- `AccountName` - Name of the account
- `OwnerName` - Name of the account owner
- `CurrentBalance` - Current account balance
- `LastUpdated` - Timestamp of the last update

And for the `Transactions` table:
- `TransactionId` - Primary key (UUID)
- `AccountId` - Foreign key to the account (UUID)
- `AccountNumber` - Account number
- `AccountName` - Name of the account
- `Amount` - Transaction amount
- `OpeningBalance` - Balance before the transaction
- `ClosingBalance` - Balance after the transaction
- `Description` - Transaction description
- `Date` - Transaction timestamp


### Network Configuration

The Docker containers are configured to access the external database using:
- **extra_hosts** mapping for DNS resolution
- **Direct IP connection** to 127.0.0.1:5432
- **Network bridge** allowing external connectivity

## 🧪 Testing

### Manual Testing

1. **Create an account** using the API
2. **Check Kafka UI** to verify the existence of the `banking-events` topic
3. **Click on the Messages tab** of the topic to see the `AccountCreatedEvent` event
4. **Check consumer logs** to see event processing
5. **Perform deposits/withdrawals/transfers** and observe the event flow
6. **Query the Projections API** to see updated balances and transactions

### Tools to Use:

1. **Use Swagger UI** to interact with the APIs
2. **Use Razor Pages** for a simple web interface
3. **Check logs** for detailed information
4. **Use Kafka UI** to monitor topics and consumer groups
5. **Inspect databases** using pgAdmin or any PostgreSQL client
6. **Run unit tests** using `dotnet test` in the respective test project directories
7. **Run integration tests** using `dotnet test` in the respective integration test project directories

### Health Checks

- Event Sourcing Banking API: http://localhost:8082/health
- Projections Banking API: http://localhost:8082/health
- Kafka UI: http://localhost:8080

## 🛠️ Development

### Project Structure

```
PostgresEventSourcing/
├── ESsample.Banking.API/                           # Banking API (Event Sourcing)
├── ESsample.Banking.Shared/                        # Shared contracts and integrations events
├── EventBus/                                       # Event bus abstractions
├── EventBus.Kafka/                                 # Kafka event bus implementation
├── EventBus.UnitTests/                             # Unit tests for EventBus
├── EventSourcing/                                  # Core event sourcing abstractions
├── EventSourcing.Postgres/                         # PostgreSQL event store implementation
├── EventSourcing.Postgres.IntegrationTests/        # Integration tests for PostgreSQL event store
├── EventSourcing.UnitTests/                        # Unit tests for EventSourcing
├── Projections.Banking/                            # Banking projections logic and use cases
├── Projections.Banking.API/                        # Banking projections API (reading models)
├── Projections.Banking.Consumer/                   # Integration Events consumer service (reading models)
├── Projections.Banking.Consumer.IntegrationTests/  # Integration tests for the consumer service
├── Projections.Banking.Postgres/                   # PostgreSQL implementation for reading models storage
├── Directory.Build.props                           # Shared build settings 
├── docker-compose.yml                              # Docker services configuration file 
├── PostgresEventSourcing.sln                       # Visual Studio solution file
└── README.md                                       # This file
```

### Adding New Events

1. **Define the event** in `ESsample.Banking.Shared/Events/`
2. **Publish the event** in the Event Store Banking API
3. **Create a handler** in `Projections.Banking.Consumer/Handlers/`
4. **Register the handler** (automatic via reflection)

### Extending the Consumer

The consumer automatically discovers and registers event handlers using reflection. Simply implement `IIntegrationEventHandler<TEvent>` and the framework will handle the rest.

## 🚨 Troubleshooting

### Common Issues

**Kafka Connection Issues:**
- Ensure Kafka is healthy: `docker-compose ps`
- Check Kafka logs: `docker-compose logs kafka`
- Verify network connectivity between services

**Database Connection Issues:**
- Check PostgreSQL health: `docker-compose ps`
- Verify connection string configuration
- Check database logs: `docker-compose logs postgres`

**Consumer Not Processing Events:**
- Check consumer logs: `docker-compose logs banking-consumer`
- Verify Kafka topic exists in Kafka UI
- Check consumer group status in Kafka UI

### Useful Commands

```bash
# Restart all services
docker-compose restart

# Rebuild and restart specific service
docker-compose up -d --build banking-consumer

# View real-time logs
docker-compose logs -f banking-consumer banking-producer-api

# Clean up everything
docker-compose down -v
docker system prune -f
```

## 📚 Key Concepts

### Event Sourcing
- Events are the source of truth
- Current state is derived from events
- Complete audit trail of all changes
- Temporal queries and time travel

### CQRS (Command Query Responsibility Segregation)
- Separate models for reads and writes
- Commands modify state via events
- Queries read from projections/read models

### Event-Driven Architecture
- Loose coupling between services
- Asynchronous processing
- Scalable and resilient system design
- Integration via events

### Projections
- Build read models from events
- Optimized for querying
- Materialized views of event data
- Event handlers update projections
- Supports multiple read models
- Event replay to rebuild projections

### Domain-Driven Design (DDD)
- Focus on the business domain
- Rich domain models
- Ubiquitous language
- Bounded contexts
- Aggregates and entities
- Value objects
- Domain events
- Repositories

### Vertical Slice Architecture
- Organize code by feature
- Self-contained modules
- Reduced dependencies
- Easier to understand and maintain
- Focused testing
- Enhanced modularity
- Improved collaboration

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

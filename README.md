# Car Management API

A .NET 6.0 API for managing cars, garages, and maintenance records, designed to run with Docker and SQL Server. This project demonstrates an entity-based structure with seamless database migrations and containerization.

## Features

- Manage cars, garages, and maintenance records.
- Entity Framework Core for data persistence.
- Automatic database migrations on startup.
- Dockerized application and SQL Server.
- Swagger UI for API documentation.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [SQL Server Management Studio (optional)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/car-management-api.git
cd car-management-api
```

### Build and Run with Docker

1. **Generate Initial Migrations** (if needed):

   ```bash
   dotnet ef migrations add InitialCreate
   ```

2. **Start the Application**:

   ```bash
   docker-compose up --build
   ```

3. The API will be available at [http://localhost:8088](http://localhost:8088).

### API Documentation

- Swagger UI is available at: [http://localhost:8088/swagger](http://localhost:8088/swagger)

### Database Access

The SQL Server container is exposed on port `1433`. You can connect to the database using a client like SSMS or DBeaver with the following credentials:

- **Server**: `localhost,1433`
- **Database**: `CarManagment`
- **User**: `sa`
- **Password**: `carManagment12345*`

## Project Structure

```plaintext
CarManagment/
├── CarManagment.Data/       # Data layer with DbContext and entity models
├── CarManagment.Models/     # Domain models
├── Program.cs               # Application entry point
├── Dockerfile               # Dockerfile for building the API container
├── docker-compose.yml       # Docker Compose configuration
└── README.md                # Project documentation
```

## Running Locally (Without Docker)

1. **Update the Connection String**:
   Modify `appsettings.json` with your local SQL Server configuration.

2. **Run the Application**:

   ```bash
   dotnet run
   ```

3. The API will be available at [http://localhost:8088](http://localhost:8088).

## Troubleshooting

### Common Issues

- **Cannot connect to the database**:

  - Ensure the SQL Server container is running (`docker ps`).
  - Verify the connection string uses `Data Source=database,1433` in Docker or `localhost` for local development.

- **Database not created**:
  - Ensure migrations have been applied:
    ```bash
    docker-compose exec api dotnet ef database update
    ```

## License

This project is licensed under the [MIT License](LICENSE).

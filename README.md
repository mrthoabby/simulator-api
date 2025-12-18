# Product Management System API

Backend API for [Simulator UI](https://github.com/mrthoabby/simulator-ui).

## Tech Stack

- .NET 9.0
- MongoDB
- JWT Authentication

## Run Locally

```bash
cd ProductManagementSystem.Application
dotnet run
```

API available at `http://localhost:5000`

## Run with Docker

```bash
# Build
docker build -t product-management-api .

# Run
docker run -d -p 8080:8080 \
  -e MongoDB__ConnectionString="mongodb://your-mongo-host:27017" \
  -e Jwt__Key="your-secret-key-at-least-32-characters" \
  product-management-api
```

API available at `http://localhost:8080`

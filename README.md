# Product Management System API

A .NET 9.0 Web API for managing products, pricing simulations, and user subscriptions.

## Prerequisites

- .NET 9.0 SDK
- MongoDB (local or Atlas)

## Quick Start

### Development

1. Clone the repository
2. Configure MongoDB connection in `appsettings.Development.json`
3. Run the application:

```bash
cd ProductManagementSystem.Application
dotnet run
```

4. Access Swagger UI at `http://localhost:5000`

### Production Deployment

The application uses environment variables for sensitive configuration in production.

#### Required Environment Variables

| Variable | Description |
|----------|-------------|
| `MONGODB_CONNECTION_STRING` | MongoDB connection string |
| `MONGODB_DATABASE_NAME` | Database name |
| `JWT_SECRET_KEY` | JWT signing key (min 32 characters) |
| `JWT_ISSUER` | JWT issuer identifier |
| `JWT_AUDIENCE` | JWT audience identifier |
| `SECURITY_PASSWORD_PEPPER` | Password hashing pepper |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |

#### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

#### Run in Production

```bash
cd publish
ASPNETCORE_ENVIRONMENT=Production dotnet ProductManagementSystem.Application.dll
```

## Project Structure

```
ProductManagementSystem.Application/
├── AppEntities/          # Domain entities and features
│   ├── Auth/             # Authentication and authorization
│   ├── Products/         # Product management
│   ├── Users/            # User management
│   ├── Subscriptions/    # Subscription plans
│   └── ...
├── Common/               # Shared utilities
│   ├── Domain/           # Error handling and base types
│   ├── Helpers/          # Utility classes
│   └── Middleware/       # Request pipeline middleware
├── Infrastructure/       # External service integrations
└── Jobs/                 # Background tasks
```

## Architecture

The project follows Clean Architecture principles:

- **Controllers**: HTTP request handling
- **Services**: Business logic
- **Repositories**: Data access
- **Models**: Domain entities
- **DTOs**: Data transfer objects

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - User logout

### Products
- `GET /api/product` - List products (paginated)
- `GET /api/product/{id}` - Get product by ID
- `POST /api/product` - Create product
- `PUT /api/product/{id}` - Update product
- `DELETE /api/product/{id}` - Delete product

### Users
- `POST /api/user` - Create user
- `GET /api/user/{id}` - Get user by ID
- `GET /api/user` - List users (paginated)

### Subscriptions
- `GET /api/subscription` - List subscriptions
- `POST /api/subscription` - Create subscription

## Testing

Run all tests:

```bash
dotnet test
```

## Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production configuration (uses env vars)

## Security Notes

- Never commit sensitive credentials to version control
- Use environment variables for all secrets in production
- JWT tokens expire after 60 minutes by default
- Refresh tokens expire after 7 days

## License

Proprietary - All rights reserved


# TiHoMo Identity API

Một API đơn giản để xác thực người dùng thông qua social login (Google, Facebook) và quản lý API keys.

## Tính năng chính

- **Social Login**: Xác thực qua Google và Facebook
- **JWT Authentication**: Tạo và validate JWT tokens  
- **User Management**: Quản lý thông tin người dùng
- **API Key Management**: Tạo và quản lý API keys cho programmatic access

## Cấu trúc dự án

```
Identity.Api/
├── Controllers/         # API Controllers
│   ├── AuthController.cs       # Authentication endpoints
│   ├── UsersController.cs      # User management  
│   └── ApiKeysController.cs    # API key management
├── Services/           # Business logic services
│   ├── TokenVerificationService.cs  # Social token verification
│   ├── UserService.cs              # User operations
│   ├── ApiKeyService.cs            # API key operations
│   └── JwtService.cs               # JWT token operations
├── Models/             # Data models and DTOs
│   ├── User.cs         # User entity
│   ├── UserLogin.cs    # Social login mapping  
│   ├── ApiKey.cs       # API key entity
│   └── DTOs.cs         # Request/Response models
├── Configuration/      # Configuration and setup
│   └── ApiKeyAuthenticationMiddleware.cs # API key middleware
└── Migrations/         # EF Core migrations
```

## API Endpoints

### Authentication
- `POST /api/auth/social-login` - Xác thực qua social provider
- `POST /api/auth/validate-token` - Validate JWT token
- `POST /api/auth/refresh-token` - Làm mới access token
- `POST /api/auth/logout` - Đăng xuất

### User Management
- `GET /api/users/me` - Lấy thông tin user hiện tại
- `GET /api/users/{userId}` - Lấy thông tin user theo ID (admin)
- `PUT /api/users/me` - Cập nhật thông tin user
- `DELETE /api/users/me` - Deactivate tài khoản

### API Key Management
- `POST /api/apikeys` - Tạo API key mới
- `GET /api/apikeys` - Lấy danh sách API keys
- `GET /api/apikeys/{keyId}` - Lấy thông tin API key
- `DELETE /api/apikeys/{keyId}` - Thu hồi API key
- `POST /api/apikeys/validate` - Validate API key (internal)

## Cấu hình

### appsettings.json
```json
{
  "ConnectionStrings": {
    "IdentityDb": "Server=(localdb)\\mssqllocaldb;Database=TiHoMo_Identity;Trusted_Connection=true"
  },
  "JWT": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "TiHoMo.Identity", 
    "Audience": "TiHoMo.Clients",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "30"
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id"
  },
  "FacebookAuth": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret"
  }
}
```

## Chạy dự án

1. **Cấu hình database connection**:
   ```bash
   # Update connection string trong appsettings.json
   ```

2. **Chạy migrations**:
   ```bash
   dotnet ef database update
   ```

3. **Cấu hình Social Login**:
   - Google: Tạo OAuth 2.0 client trong Google Cloud Console
   - Facebook: Tạo app trong Facebook for Developers
   - Cập nhật ClientId/AppId trong appsettings

4. **Chạy ứng dụng**:
   ```bash
   dotnet run
   ```

5. **Truy cập Swagger UI**:
   - Development: `https://localhost:{port}/swagger`

## Authentication Methods

### 1. JWT Bearer Token (cho users)
```
Authorization: Bearer <jwt-token>
```

### 2. API Key (cho services)
```
# Header
Authorization: Bearer <api-key>
# hoặc
X-API-Key: <api-key>
# hoặc query parameter
?api_key=<api-key>
```

## Example Usage

### Social Login Flow
```javascript
// 1. User authenticates với Google/Facebook trên frontend
// 2. Frontend gửi token đến API
POST /api/auth/social-login
Content-Type: application/json

{
  "provider": "Google",
  "token": "google-jwt-token-here"
}

// 3. API trả về JWT token
{
  "user": {
    "id": "user-guid",
    "email": "user@example.com",
    "name": "User Name",
    "providers": ["Google"]
  },
  "accessToken": "jwt-token",
  "refreshToken": "refresh-token", 
  "expiresAt": "2023-12-01T10:00:00Z"
}
```

### API Key Creation
```javascript
// 1. User đăng nhập và tạo API key
POST /api/apikeys
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "name": "My API Key",
  "description": "For accessing TiHoMo APIs",
  "expiresAt": "2024-12-01T00:00:00Z"
}

// 2. API trả về key (chỉ hiển thị 1 lần)
{
  "id": "key-guid",
  "name": "My API Key", 
  "apiKey": "actual-api-key-here",
  "keyPrefix": "tk_12345",
  "createdAt": "2023-12-01T10:00:00Z",
  "expiresAt": "2024-12-01T00:00:00Z"
}
```

## Database Schema

### Users Table
- `Id` (uniqueidentifier, PK)
- `Email` (nvarchar(100), unique)
- `Name` (nvarchar(100))
- `PictureUrl` (nvarchar(200), nullable)
- `IsActive` (bit)
- `CreatedAt` (datetime2)
- `UpdatedAt` (datetime2, nullable)

### UserLogins Table
- `Id` (uniqueidentifier, PK)  
- `UserId` (uniqueidentifier, FK)
- `Provider` (nvarchar(50)) - "Google", "Facebook"
- `ProviderUserId` (nvarchar(100)) - ID từ provider
- `ProviderDisplayName` (nvarchar(200), nullable)
- `CreatedAt` (datetime2)
- `LastLoginAt` (datetime2, nullable)

### ApiKeys Table
- `Id` (uniqueidentifier, PK)
- `UserId` (uniqueidentifier, FK)
- `Name` (nvarchar(100)) - User-friendly name
- `KeyHash` (nvarchar(64)) - Hashed key
- `KeyPrefix` (nvarchar(32)) - First 8 chars for identification
- `IsActive` (bit)
- `CreatedAt` (datetime2)
- `ExpiresAt` (datetime2, nullable)
- `LastUsedAt` (datetime2, nullable)
- `Description` (nvarchar(500), nullable)
- `Scopes` (nvarchar(1000), nullable) - Comma-separated permissions

## Lưu ý bảo mật

- API keys được hash trước khi lưu database
- JWT tokens có thời gian expire
- API key có thể set expiration date
- Social tokens được verify với provider trước khi accept
- User accounts có thể bị deactivate
- CORS được cấu hình cho production

## TODO/Roadmap

- [ ] Rate limiting
- [ ] Refresh token storage và rotation  
- [ ] Role-based authorization
- [ ] Audit logging
- [ ] Health checks
- [ ] Docker support
- [ ] Integration tests

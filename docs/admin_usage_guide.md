# 后台管理系统使用说明文档

## 1. 系统概述

本系统实现了一套完整的基于角色权限的后台管理入口，支持管理员进行用户管理、账户锁定和系统日志审计。系统采用三层架构设计，将权限判断、业务逻辑与数据访问完全解耦，确保系统的可维护性和安全性。

## 2. 架构设计

### 2.1 三层架构

| 层级 | 职责 | 核心组件 |
|------|------|----------|
| **权限判断层** | 验证用户角色权限，防止越权访问 | `IPermissionService`, `RequireAdminAttribute` |
| **业务逻辑层** | 处理核心业务逻辑，封装业务规则 | `IAdminService`, `ISystemLogService` |
| **数据访问层** | 封装数据库操作，提供数据持久化 | `IUserRepository`, `ISystemLogRepository` |

### 2.2 安全机制

- **JWT Token 认证**：所有接口需携带有效的 Bearer Token
- **角色Claim**：JWT Token 中包含用户角色信息（`ClaimTypes.Role`）
- **权限过滤器**：`[RequireAdmin]` 特性自动验证管理员角色
- **会话吊销**：锁定用户时自动吊销所有活跃的 Refresh Token

## 3. 角色权限说明

### 3.1 角色定义

在 [AppConstants.cs](file:///d:/charles/program/ai/apps/02.work session/solo-0601/source code/app-21/src/CraftSwap.API/Common/AppConstants.cs#L93-L104) 中定义：

```csharp
public static class UserRoles
{
    public const string Admin = "Admin";   // 管理员
    public const string User = "User";     // 普通用户
}
```

### 3.2 权限矩阵

| 功能 | 管理员 | 普通用户 |
|------|--------|----------|
| 分页查询用户列表 | ✅ | ❌ |
| 锁定/解锁用户账户 | ✅ | ❌ |
| 查看系统运行日志 | ✅ | ❌ |
| 登录系统 | ✅ | ✅ |
| 修改个人资料 | ✅ | ✅ |

## 4. 快速开始

### 4.1 默认管理员账户

系统初始化时会自动创建默认管理员账户：

- **用户名**：`admin`
- **密码**：`admin123`
- **角色**：`Admin`

> ⚠️  **安全提示**：首次登录后请立即修改默认密码！

### 4.2 数据库迁移

执行以下命令创建数据库表结构：

```powershell
# 添加迁移
Add-Migration AddAdminFeatures

# 更新数据库
Update-Database
```

### 4.3 登录获取Token

调用登录接口获取管理员 Token：

```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "admin123"
}
```

响应示例：

```json
{
    "code": 200,
    "message": "操作成功",
    "data": {
        "token": "eyJhbGciOiJIUzI1NiIs...",
        "refreshToken": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "expiresIn": 3600,
        "user": {
            "id": 1,
            "username": "admin",
            "email": "admin@example.com",
            "role": "Admin"
        }
    }
}
```

## 5. API 接口文档

所有管理接口的基路径为 `/api/admin`，且必须在请求头中携带：

```
Authorization: Bearer {your_admin_token}
```

### 5.1 分页查询用户列表

**接口**：`GET /api/admin/users`

**查询参数**：

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `PageNumber` | int | 否 | 1 | 页码，从1开始 |
| `PageSize` | int | 否 | 10 | 每页条数，最大100 |
| `SearchKeyword` | string | 否 | - | 搜索关键词（用户名/邮箱） |
| `Role` | string | 否 | - | 角色过滤：`Admin` / `User` |
| `IsLocked` | bool | 否 | - | 是否只显示锁定用户 |
| `SortBy` | string | 否 | `CreatedAt` | 排序字段 |
| `SortDirection` | string | 否 | `desc` | 排序方向：`asc` / `desc` |

**请求示例**：

```http
GET /api/admin/users?PageNumber=1&PageSize=20&Role=User&IsLocked=false
Authorization: Bearer {token}
```

**响应示例**：

```json
{
    "code": 200,
    "message": "操作成功",
    "data": {
        "pageNumber": 1,
        "pageSize": 20,
        "totalCount": 156,
        "totalPages": 8,
        "items": [
            {
                "id": 2,
                "username": "user001",
                "email": "user001@example.com",
                "avatar": "https://...",
                "role": "User",
                "isLocked": false,
                "lockEndTime": null,
                "lockReason": null,
                "createdAt": "2024-01-15T10:30:00Z"
            }
        ]
    }
}
```

### 5.2 锁定用户账户

**接口**：`POST /api/admin/users/lock`

**请求体**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `UserId` | int | 是 | 目标用户ID |
| `LockDurationMinutes` | int? | 否 | 锁定时长（分钟），null表示永久锁定 |
| `Reason` | string | 是 | 锁定原因 |

**请求示例 - 临时锁定**（锁定24小时）：

```http
POST /api/admin/users/lock
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": 5,
    "lockDurationMinutes": 1440,
    "reason": "检测到异常登录行为，多次尝试使用弱密码"
}
```

**请求示例 - 永久锁定**：

```http
POST /api/admin/users/lock
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": 5,
    "lockDurationMinutes": null,
    "reason": "发布违规内容，永久封禁"
}
```

**响应示例**：

```json
{
    "code": 200,
    "message": "操作成功",
    "data": {
        "userId": 5,
        "username": "baduser",
        "isLocked": true,
        "lockEndTime": "2024-01-16T10:30:00Z",
        "lockReason": "检测到异常登录行为，多次尝试使用弱密码",
        "sessionsRevoked": 3
    }
}
```

> 💡  **说明**：`sessionsRevoked` 表示已吊销的活跃会话数量。

### 5.3 解锁用户账户

**接口**：`POST /api/admin/users/unlock`

**请求体**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `UserId` | int | 是 | 目标用户ID |
| `Reason` | string | 是 | 解锁原因 |

**请求示例**：

```http
POST /api/admin/users/unlock
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": 5,
    "reason": "用户申诉成功，核实为误判"
}
```

**响应示例**：

```json
{
    "code": 200,
    "message": "操作成功",
    "data": {
        "userId": 5,
        "username": "baduser",
        "isLocked": false,
        "lockEndTime": null,
        "lockReason": null,
        "sessionsRevoked": 0
    }
}
```

### 5.4 按时间范围查询系统日志

**接口**：`GET /api/admin/logs`

**查询参数**：

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `PageNumber` | int | 否 | 1 | 页码 |
| `PageSize` | int | 否 | 10 | 每页条数 |
| `StartTime` | DateTime? | 否 | - | 开始时间（UTC） |
| `EndTime` | DateTime? | 否 | - | 结束时间（UTC） |
| `LogLevel` | string? | 否 | - | 日志级别：`Information` / `Warning` / `Error` / `Critical` |
| `EventType` | string? | 否 | - | 事件类型，见下方说明 |
| `OperatorId` | int? | 否 | - | 操作人用户ID |
| `TargetUserId` | int? | 否 | - | 目标用户ID |
| `SearchKeyword` | string? | 否 | - | 关键词搜索（消息/详情） |
| `SortDirection` | string | 否 | `desc` | 排序方向 |

**事件类型列表**：

| 事件类型 | 说明 |
|----------|------|
| `UserLogin` | 用户登录成功 |
| `UserLoginFailed` | 用户登录失败 |
| `UserRegistered` | 用户注册 |
| `UserProfileUpdated` | 用户资料更新 |
| `UserLocked` | 用户被锁定 |
| `UserUnlocked` | 用户被解锁 |
| `UserListQueried` | 用户列表查询 |
| `SystemLogQueried` | 系统日志查询 |
| `PermissionDenied` | 权限不足访问尝试 |

**请求示例** - 查询2024年1月的错误日志：

```http
GET /api/admin/logs?StartTime=2024-01-01T00:00:00Z&EndTime=2024-01-31T23:59:59Z&LogLevel=Error&PageSize=50
Authorization: Bearer {token}
```

**响应示例**：

```json
{
    "code": 200,
    "message": "操作成功",
    "data": {
        "pageNumber": 1,
        "pageSize": 50,
        "totalCount": 12,
        "totalPages": 1,
        "items": [
            {
                "id": 1024,
                "logLevel": "Error",
                "eventType": "UserLoginFailed",
                "message": "用户登录失败：密码错误",
                "detailedMessage": "用户 baduser 连续5次登录失败，IP: 192.168.1.100",
                "operatorId": null,
                "operatorName": null,
                "targetUserId": 5,
                "targetUserName": "baduser",
                "ipAddress": "192.168.1.100",
                "userAgent": "Mozilla/5.0...",
                "createdAt": "2024-01-15T14:32:10Z"
            }
        ]
    }
}
```

## 6. 核心代码说明

### 6.1 权限判断层 - [IPermissionService](file:///d:/charles/program/ai/apps/02.work session/solo-0601/source code/app-21/src/CraftSwap.API/Services/IPermissionService.cs)

```csharp
public interface IPermissionService
{
    // 检查是否为管理员
    bool IsAdmin(ClaimsPrincipal user);
    
    // 确保是管理员，否则抛出403异常
    void EnsureAdmin(ClaimsPrincipal user);
    
    // 获取当前用户ID
    int? GetCurrentUserId(ClaimsPrincipal user);
    
    // 获取当前用户角色
    string? GetCurrentUserRole(ClaimsPrincipal user);
}
```

### 6.2 业务逻辑层 - [IAdminService](file:///d:/charles/program/ai/apps/02.work session/solo-0601/source code/app-21/src/CraftSwap.API/Services/IAdminService.cs)

- `GetPagedUsersAsync()` - 分页查询用户列表
- `LockUserAsync()` - 锁定用户（自动吊销会话）
- `UnlockUserAsync()` - 解锁用户
- `GetPagedSystemLogsAsync()` - 分页查询系统日志

### 6.3 权限过滤器 - [RequireAdminAttribute](file:///d:/charles/program/ai/apps/02.work session/solo-0601/source code/app-21/src/CraftSwap.API/Filters/RequireAdminAttribute.cs)

使用方式：在控制器或Action上添加 `[RequireAdmin]` 特性，系统会自动：
1. 验证用户JWT Token的有效性
2. 检查用户角色是否为 `Admin`
3. 记录所有权限不足的访问尝试到系统日志

### 6.4 用户实体 - [User.cs](file:///d:/charles/program/ai/apps/02.work session/solo-0601/source code/app-21/src/CraftSwap.API/Entities/User.cs#L46-L67)

扩展字段：

```csharp
public string Role { get; set; } = AppConstants.UserRoles.User;
public bool IsLocked { get; set; }
public DateTime? LockEndTime { get; set; }
public string? LockReason { get; set; }
```

## 7. 账户锁定机制

### 7.1 锁定流程

1. 管理员调用锁定接口，指定用户ID和锁定时长
2. 系统更新用户的 `IsLocked`、`LockEndTime`、`LockReason` 字段
3. 系统自动吊销该用户的所有活跃 Refresh Token
4. 被锁定用户的现有 JWT Token 在过期前仍然有效，但无法刷新
5. 被锁定用户尝试登录时会被拒绝，并提示锁定原因

### 7.2 自动解锁

对于临时锁定的账户，系统在用户登录时会自动检查 `LockEndTime`：

- 如果 `LockEndTime` <= 当前时间，自动解锁账户
- 如果 `LockEndTime` == null（永久锁定），需要管理员手动解锁

## 8. 系统日志审计

### 8.1 自动记录的事件

系统会自动记录以下关键事件：

| 事件 | 触发时机 |
|------|----------|
| 用户登录 | 登录成功/失败时 |
| 用户注册 | 新用户注册时 |
| 资料更新 | 用户修改个人资料时 |
| 用户锁定/解锁 | 管理员执行锁定/解锁时 |
| 用户列表查询 | 管理员查询用户列表时 |
| 系统日志查询 | 管理员查询系统日志时 |
| 权限不足 | 普通用户尝试访问管理接口时 |

### 8.2 日志字段说明

| 字段 | 说明 |
|------|------|
| `LogLevel` | 日志级别：信息/警告/错误/严重 |
| `EventType` | 事件类型（见5.4节） |
| `Message` | 简要消息 |
| `DetailedMessage` | 详细信息（JSON格式） |
| `OperatorId/Name` | 操作人信息 |
| `TargetUserId/Name` | 目标用户信息 |
| `IpAddress` | 客户端IP地址 |
| `UserAgent` | 客户端浏览器信息 |

## 9. 常见问题排查

### 9.1 调用管理接口返回 401 Unauthorized

**原因**：未携带有效的 JWT Token 或 Token 已过期

**解决方案**：
1. 检查请求头是否包含 `Authorization: Bearer {token}`
2. 重新调用登录接口获取新的 Token
3. 检查 Token 的有效期（默认1小时）

### 9.2 调用管理接口返回 403 Forbidden

**原因**：当前用户不是管理员角色

**解决方案**：
1. 确认登录的账户具有 `Admin` 角色
2. 检查数据库中用户的 `Role` 字段值是否为 `"Admin"`
3. 查看系统日志中的 `PermissionDenied` 事件记录

### 9.3 被锁定的用户仍然可以访问

**原因**：JWT Token 是无状态的，已签发的 Token 在过期前仍然有效

**解决方案**：
1. 锁定操作会自动吊销所有 Refresh Token，用户无法续期
2. 可以缩短 JWT Token 的有效期（在 `appsettings.json` 中配置）
3. 紧急情况下可以重启服务，所有 Token 失效

### 9.4 数据库迁移失败

**解决方案**：
```powershell
# 删除现有迁移
Remove-Migration

# 重新创建迁移
Add-Migration AddAdminFeatures -Force

# 应用迁移
Update-Database -Verbose
```

## 10. 配置说明

在 `appsettings.json` 中可以调整相关配置：

```json
{
    "JwtSettings": {
        "Secret": "your-secret-key-here",
        "ExpirationMinutes": 60,        // JWT Token 有效期
        "RefreshTokenExpirationDays": 7 // Refresh Token 有效期
    }
}
```

## 11. 维护建议

1. **定期审查管理员账户**：确保只有授权人员拥有管理员角色
2. **监控系统日志**：定期检查 `PermissionDenied` 和 `UserLoginFailed` 事件
3. **备份数据**：定期备份 `Users` 和 `SystemLogs` 表
4. **更新密码策略**：强制管理员定期更换密码，启用多因素认证
5. **日志轮转**：定期归档历史系统日志，避免数据库过大

---

**文档版本**：v1.0  
**最后更新**：2024-01-15

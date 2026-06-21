# 项目21：手工材料剩余交换（CraftSwap）

## 请帮我从 0 到 1 实现以下小众项目

### 项目概述
手工爱好者常剩下毛线、珠子、布料、颜料等材料。该平台帮助同好互换剩余材料，降低创作成本，减少囤积。

### 创新点 / 小众定位
围绕"剩余手工材料"设计，支持按颜色/材质/克重搜索、交换请求、邮费分摊与作品晒单。

### 目标用户
手工爱好者、DIY 创作者、钩织/串珠社团

## 项目范围说明
- 本项目为纯后端系统开发，不涉及任何前端页面、UI、CSS/JS 改动。
- 所有功能均通过 RESTful API 对外提供服务，可使用 Postman、curl 或任意 HTTP 客户端进行测试与验收。

## 技术栈（必须严格使用）
- **后端框架**: .NET Core 8.0 (ASP.NET Core Web API)
- **数据库**: MySQL 8.0
- **ORM**: Entity Framework Core 8.0 (Pomelo.EntityFrameworkCore.MySql)
- **认证**: JWT Bearer Token
- **API文档**: Swagger / OpenAPI
- **容器化**: Docker + Docker Compose
- **测试**: xUnit + TestContainers（可选）+ Postman 测试集合

## 项目必须包含的交付物
- **Dockerfile**：多阶段构建，基于上述技术栈。
- **docker-compose.yml**：一键启动应用服务 + MySQL 8.0 + 可选管理工具（如 Adminer）。
- **.gitignore**：针对 .NET Core 的标准忽略配置。
- **README.md**：项目简介、目录说明、快速启动、API 文档入口、测试方式。
- **docs/functional_intro.md**：功能说明、ER 图文字描述、核心用例、业务规则。
- **src/**：完整后端源码（Controller / Service / Repository / Entity / DTO / Mapper / Config 等）。
- **tests/**：单元测试 + 集成测试。
- **postman_collection.json**（或同等测试脚本）：覆盖所有接口的功能测试集合。
- **初始化 SQL / Seed Data**：Docker 启动后自动建表并插入示例数据。

## 数据库设计

### 主要数据表
1. **Users** - 用户表
   - Id（主键）
   - Username（用户名，唯一）
   - Email（邮箱，唯一）
   - PasswordHash（密码哈希）
   - Avatar（头像 URL，可选）
   - CreatedAt / UpdatedAt

2. **Materials** - 材料
   - OwnerId
   - Name
   - Category
   - Color
   - Material
   - Quantity
   - Unit
   - Condition
   - Photos
   - Status（Available / Reserved / Swapped / Archived）
   - CreatedAt

3. **SwapRequests** - 交换请求
   - ProposerId
   - ReceiverId
   - OfferedMaterialId
   - RequestedMaterialId
   - Message
   - Status（Pending / Accepted / Rejected / Completed）
   - CreatedAt

4. **SwapReviews** - 交换评价
   - RequestId
   - ReviewerId
   - RevieweeId
   - Rating
   - Content
   - CreatedAt

5. **ProjectShowcases** - 作品展示
   - UserId
   - Title
   - Description
   - UsedMaterials
   - Photos
   - CreatedAt

## 核心功能模块
### 1. 用户认证模块
- 用户注册 / 登录 / JWT 鉴权
- 获取当前登录用户信息

### 2. 材料管理模块
- 材料的增删改查（支持分页、搜索、排序）
- 材料状态/详情/关联操作
- 材料权限控制（仅所有者或管理员可操作）

### 3. 交换请求管理模块
- 交换请求的增删改查（支持分页、搜索、排序）
- 交换请求状态/详情/关联操作
- 交换请求权限控制（仅所有者或管理员可操作）

### 4. 交换评价管理模块
- 交换评价的增删改查（支持分页、搜索、排序）
- 交换评价状态/详情/关联操作
- 交换评价权限控制（仅所有者或管理员可操作）

### 5. 作品管理模块
- 作品的增删改查（支持分页、搜索、排序）
- 作品状态/详情/关联操作
- 作品权限控制（仅所有者或管理员可操作）

### 6. 统计与搜索模块
- 全局搜索与筛选
- 基础数据看板（数量、趋势、排行榜等）
- 导出关键数据（可选）

## API 接口清单
### Auth
- POST /api/auth/register - 用户注册
- POST /api/auth/login - 用户登录
- GET /api/auth/me - 获取当前用户信息
- PUT /api/auth/me - 更新个人信息

### Materials（材料）
- GET /api/materials - 获取材料列表（支持分页、搜索、筛选）
- POST /api/materials - 创建材料
- GET /api/materials/{id} - 获取材料详情
- PUT /api/materials/{id} - 更新材料
- DELETE /api/materials/{id} - 删除材料
- PATCH /api/materials/{id}/status - 修改材料状态
- GET /api/materials/mine - 获取我发布的/关联的材料

### SwapRequests（交换请求）
- GET /api/swaprequests - 获取交换请求列表（支持分页、搜索、筛选）
- POST /api/swaprequests - 创建交换请求
- GET /api/swaprequests/{id} - 获取交换请求详情
- PUT /api/swaprequests/{id} - 更新交换请求
- DELETE /api/swaprequests/{id} - 删除交换请求
- PATCH /api/swaprequests/{id}/status - 修改交换请求状态

### SwapReviews（交换评价）
- GET /api/swapreviews - 获取交换评价列表（支持分页、搜索、筛选）
- POST /api/swapreviews - 创建交换评价
- GET /api/swapreviews/{id} - 获取交换评价详情
- PUT /api/swapreviews/{id} - 更新交换评价
- DELETE /api/swapreviews/{id} - 删除交换评价

### ProjectShowcases（作品）
- GET /api/projectshowcases - 获取作品列表（支持分页、搜索、筛选）
- POST /api/projectshowcases - 创建作品
- GET /api/projectshowcases/{id} - 获取作品详情
- PUT /api/projectshowcases/{id} - 更新作品
- DELETE /api/projectshowcases/{id} - 删除作品
- GET /api/projectshowcases/mine - 获取我发布的/关联的作品

### Statistics
- GET /api/stats/overview - 总览统计
- GET /api/stats/trend - 趋势统计（按时间范围）

## Docker 配置要求

### Dockerfile（.NET Core）
```dockerfile
# 阶段1：构建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 阶段2：运行
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8091
ENV ASPNETCORE_URLS=http://+:8091
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3   CMD curl -f http://localhost:8091/health || exit 1
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

要求：
1. 使用多阶段构建，减少最终镜像体积。
2. 暴露 8091 端口。
3. 添加健康检查接口 `/health`。
4. 通过环境变量读取数据库连接字符串。

### docker-compose.yml 要求
```yaml
version: '3.8'
services:
  app:
    build: .
    container_name: craftswap_app
    ports:
      - "8091:8091"
    environment:
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_NAME=craftswap
      - DB_USER=app_user
      - DB_PASSWORD=app_pass
    depends_on:
      mysql:
        condition: service_healthy
  mysql:
    image: mysql:8.0
    container_name: craftswap_mysql
    environment:
      - MYSQL_ROOT_PASSWORD=root_pass
      - MYSQL_DATABASE=craftswap
      - MYSQL_USER=app_user
      - MYSQL_PASSWORD=app_pass
    ports:
      - "13316:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
  volumes:
    mysql_data:
```

要求：
1. MySQL 使用 8.0 镜像。
2. 应用服务必须等 MySQL healthy 后再启动。
3. 使用 named volume 持久化数据库数据。
4. 环境变量集中管理，禁止在源码中硬编码密码。

## .gitignore 参考
```text
# .NET / ASP.NET Core
bin/
obj/
*.user
*.suo
*.userosscache
*.sln.docstates
.vs/
*.swp
*.log

# Secrets & local config
appsettings.Development.json
appsettings.Local.json
.env
.env.local

# Test results
TestResults/
coverage/

# IDE
.vscode/
.idea/

# OS
Thumbs.db
.DS_Store
```

## 文档要求

### README.md
至少包含：
1. 项目名称与一句话介绍。
2. 功能亮点（3-5 条）。
3. 技术栈说明。
4. 目录结构说明。
5. 快速启动步骤（克隆 → Docker 启动 → 访问接口）。
6. 测试命令与 Postman 集合导入说明。
7. 贡献与许可（可选）。

### docs/functional_intro.md
至少包含：
1. 业务背景与解决的问题。
2. 用户角色与核心用例。
3. 功能模块详细说明。
4. 数据库 ER 图文字描述（表关系）。
5. 关键业务规则（如状态流转、权限规则、时间计算逻辑）。
6. 接口调用示例（至少 3 个）。

## 运行与测试步骤

1. **克隆并进入项目目录**：
   ```bash
   git clone <repo-url>
   cd CraftSwap
   ```

2. **Docker 启动**：
   ```bash
   docker-compose up --build -d
   ```

3. **查看日志**：
   ```bash
   docker-compose logs -f app
   ```

4. **验证服务健康**：
   - .NET：`curl http://localhost:8091/health`
   - Java：`curl http://localhost:8091/actuator/health`

5. **导入并执行 Postman 测试集合**，验证所有接口：
   - 注册 / 登录
   - 各实体的 CRUD
   - 搜索 / 筛选 / 分页
   - 统计接口
   - 权限控制（未登录访问受限资源应返回 401）

6. **执行自动化测试**：
   - .NET：`dotnet test`
   - Java：`./mvnw test` 或 `mvn test`

7. **停止服务**：
   ```bash
   docker-compose down -v
   ```

## 其他质量要求
- 使用 EF Core 操作 MySQL，禁止手写 SQL 进行日常 CRUD（复杂统计可手写）。
- 代码分层清晰，遵循 RESTful API 设计规范。
- 关键代码必须有中文注释，说明业务意图。
- 统一的异常处理与参数校验（.NET FluentValidation / Spring Validation）。
- 使用 JWT 保护敏感接口，未携带 Token 返回 401。
- 数据库连接字符串通过环境变量注入，支持 Docker 内外运行。
- 提供 Seed Data，容器启动后至少有 5-10 条示例数据可用于测试。
- 接口返回统一包装格式（code / message / data）。
- 日志使用框架原生日志（.NET ILogger / SLF4J），记录关键操作与异常。
- 项目必须是小众生活/工作场景，禁止做成通用商城、OA、CMS、ERP。

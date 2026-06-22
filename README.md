# CraftSwap - 手工材料剩余交换平台

## 项目简介

CraftSwap 是一个专门

为手工爱好者打造的剩余材料交换平台。手工创作者常常会剩余毛线、珠子、布料、颜料等材料，本平台帮助同好之间互换剩余材料，降低创作成本，减少囤积浪费。

## 功能亮点

1. **精准材料搜索** - 支持按颜色、材质、分类、克重等多维度筛选和搜索剩余材料
2. **安全交换流程** - 完整的交换请求生命周期管理（发起→接受/拒绝→完成→评价）
3. **作品展示社区** - 晒出你的手工作品，展示材料使用成果，激发创作灵感
4. **信用评价体系** - 基于交换完成后的双向评价，建立可信的社区信用体系
5. **数据统计看板** - 平台总览数据和趋势统计，掌握社区动态

## 技术栈说明

| 类别     | 技术                               | 版本    |
| ------ | -------------------------------- | ----- |
| 后端框架   | ASP.NET Core Web API             | 8.0   |
| ORM    | Entity Framework Core            | 8.0   |
| 数据库    | MySQL                            | 8.0   |
| 数据库驱动  | Pomelo.EntityFrameworkCore.MySql | 8.0   |
| 认证方案   | JWT Bearer Token                 | -     |
| API 文档 | Swagger / OpenAPI                | -     |
| 对象映射   | AutoMapper                       | 13.0  |
| 参数校验   | FluentValidation                 | 11.10 |
| 密码哈希   | BCrypt.Net-Next                  | 4.0   |
| 容器化    | Docker + Docker Compose          | -     |
| 测试框架   | xUnit                            | -     |

## 目录结构说明

```
CraftSwap/
├── src/
│   └── CraftSwap.API/                 # Web API 主项目
│       ├── Common/                     # 通用配置与常量
│       │   ├── AppConstants.cs         # 应用常量定义
│       │   └── JwtSettings.cs          # JWT 配置类
│       ├── Controllers/                # API 控制器层
│       │   ├── AuthController.cs       # 用户认证接口
│       │   ├── MaterialsController.cs  # 材料管理接口
│       │   ├── SwapRequestsController.cs # 交换请求接口
│       │   ├── SwapReviewsController.cs # 交换评价接口
│       │   ├── ProjectShowcasesController.cs # 作品展示接口
│       │   └── StatsController.cs      # 统计接口
│       ├── Data/                       # 数据访问层
│       │   ├── AppDbContext.cs         # 数据库上下文
│       │   └── DbInitializer.cs        # 数据库初始化器
│       ├── DTOs/                       # 数据传输对象
│       │   ├── Auth/                   # 认证相关 DTO
│       │   ├── Common/                 # 通用 DTO（ApiResponse、PagedResponse）
│       │   ├── Materials/              # 材料相关 DTO
│       │   ├── SwapRequests/           # 交换请求 DTO
│       │   ├── SwapReviews/            # 评价相关 DTO
│       │   ├── ProjectShowcases/       # 作品相关 DTO
│       │   └── Stats/                  # 统计相关 DTO
│       ├── Entities/                   # 数据实体
│       │   ├── User.cs                 # 用户实体
│       │   ├── Material.cs             # 材料实体
│       │   ├── SwapRequest.cs          # 交换请求实体
│       │   ├── SwapReview.cs           # 交换评价实体
│       │   └── ProjectShowcase.cs      # 作品展示实体
│       ├── Exceptions/                 # 自定义异常
│       ├── Extensions/                 # 扩展方法
│       │   ├── ServiceCollectionExtensions.cs # 服务注册扩展
│       │   └── ApplicationBuilderExtensions.cs # 中间件扩展
│       ├── Filters/                    # Action 过滤器
│       ├── MappingProfiles/            # AutoMapper 映射配置
│       ├── Middleware/                 # 自定义中间件
│       │   └── ExceptionHandlingMiddleware.cs # 全局异常处理
│       ├── Repositories/               # 仓储层
│       │   ├── IRepository.cs          # 通用仓储接口
│       │   ├── Repository.cs           # 通用仓储实现
│       │   ├── IUserRepository.cs
│       │   ├── UserRepository.cs
│       │   ├── IMaterialRepository.cs
│       │   ├── MaterialRepository.cs
│       │   ├── ISwapRequestRepository.cs
│       │   ├── SwapRequestRepository.cs
│       │   ├── ISwapReviewRepository.cs
│       │   ├── SwapReviewRepository.cs
│       │   ├── IProjectShowcaseRepository.cs
│       │   └── ProjectShowcaseRepository.cs
│       ├── Services/                   # 业务服务层
│       │   ├── IAuthService.cs
│       │   ├── AuthService.cs
│       │   ├── IMaterialService.cs
│       │   ├── MaterialService.cs
│       │   ├── ISwapRequestService.cs
│       │   ├── SwapRequestService.cs
│       │   ├── ISwapReviewService.cs
│       │   ├── SwapReviewService.cs
│       │   ├── IProjectShowcaseService.cs
│       │   ├── ProjectShowcaseService.cs
│       │   ├── IStatsService.cs
│       │   └── StatsService.cs
│       ├── Validators/                 # FluentValidation 验证器
│       ├── Program.cs                  # 应用入口
│       ├── appsettings.json            # 应用配置
│       └── CraftSwap.API.csproj        # 项目文件
├── tests/                              # 测试项目
│   └── CraftSwap.Tests/                # 单元测试 & 集成测试
├── docs/
│   └── functional_intro.md             # 功能说明文档
├── postman_collection.json             # Postman 测试集合
├── Dockerfile                          # Docker 镜像构建文件
├── docker-compose.yml                  # Docker Compose 编排
├── .gitignore                          # Git 忽略配置
└── README.md                           # 本文件
```

## 快速启动步骤

### 前置要求

- Docker 20.10+
- Docker Compose v2+
- 至少 4GB 可用内存

### 一键启动

```bash
# 1. 克隆并进入项目目录
git clone <repo-url>
cd CraftSwap

# 2. 使用 Docker Compose 构建并启动所有服务
docker-compose up --build -d

# 3. 查看服务启动日志
docker-compose logs -f app

# 4. 等待服务健康（首次启动约需 30-60 秒）
docker-compose ps
```

### 服务访问地址

| 服务         | 地址                              | 说明                                                                   |
| ---------- | ------------------------------- | -------------------------------------------------------------------- |
| API 服务     | <http://localhost:8091>         | 后端 API 服务                                                            |
| Swagger 文档 | <http://localhost:8091/swagger> | 交互式 API 文档                                                           |
| 健康检查       | <http://localhost:8091/health>  | 服务健康状态                                                               |
| Adminer    | <http://localhost:8081>         | 数据库管理工具（系统：MySQL，服务器：mysql，用户名：app\_user，密码：app\_pass，数据库：craftswap） |

### 端口占用处理

如果启动时遇到端口被占用错误，可修改 `docker-compose.yml` 中的端口映射：

```yaml
services:
  app:
    ports:
      - "8096:8091"  # 修改左边的本地端口，如 "9091:8091"

  mysql:
    ports:
      - "13321:3306"  # 修改左边的本地端口

  adminer:
    ports:
      - "8082:8080"  # 修改左边的本地端口
```

修改后重新执行 `docker-compose up -d`。

### 停止服务

```bash
# 停止并保留数据
docker-compose down

# 停止并删除所有数据（慎用）
docker-compose down -v
```

## 测试命令与 Postman 集合导入说明

### 自动化测试

```bash
# 运行单元测试和集成测试
cd tests/CraftSwap.Tests
dotnet test

# 带代码覆盖率的测试
dotnet test --collect:"XPlat Code Coverage"
```

### Postman 测试集合

1. 打开 Postman
2. 点击 **Import** → 选择项目根目录下的 `postman_collection.json`
3. 导入后会创建名为 **CraftSwap API** 的集合
4. 集合中包含所有接口的测试用例，涵盖：
   - 用户注册、登录、获取个人信息
   - 材料的 CRUD 及分页搜索
   - 交换请求的完整生命周期
   - 交换评价的创建与查询
   - 作品展示的 CRUD
   - 统计数据接口
   - 权限控制测试（未登录访问受限资源返回 401）
5. **执行顺序建议**：
   - 先运行 `Auth > Register` 注册用户
   - 再运行 `Auth > Login` 获取 Token（会自动设置到集合变量）
   - 然后按目录顺序执行其他接口测试

### 默认种子数据

系统启动后会自动插入以下示例数据（可直接用于测试）：

| 类型   | 数量 | 说明                                       |
| ---- | -- | ---------------------------------------- |
| 用户   | 3  | alice / bob / charlie，密码均为 `Test@123456` |
| 材料   | 10 | 涵盖毛线、珠子、布料等不同分类                          |
| 交换请求 | 3  | 不同状态的示例请求                                |
| 交换评价 | 2  | 已完成交换的评价                                 |
| 作品展示 | 3  | 示例手工作品                                   |

## 贡献与许可

欢迎提交 Issue 和 Pull Request！

### 开发规范

- 所有 API 返回统一的 `ApiResponse<T>` 格式：`{ code, message, data }`
- 关键代码必须有中文注释说明业务意图
- 新增接口需补充对应的 FluentValidation 验证器
- 权限控制使用 `[Authorize]` 特性，公开接口显式标记 `[AllowAnonymous]`

### License

MIT License

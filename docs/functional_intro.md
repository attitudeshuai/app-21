# CraftSwap 功能说明文档

## 1. 业务背景与解决的问题

### 1.1 业务背景

手工创作（钩织、串珠、刺绣、绘画、布艺等）近年来在年轻群体中越来越流行。然而，几乎每个手工爱好者都会遇到以下痛点：

- **材料囤积**：为了一个项目购买的材料（毛线、珠子、布料、颜料等）往往用不完，剩下的被束之高阁，占用储物空间
- **采购成本高**：某些特殊颜色或材质的材料最小起订量大，实际只需少量
- **材料浪费**：长期闲置的材料因老化、变色而被丢弃，造成资源浪费
- **社交孤立**：独自创作缺乏交流，完成作品后无处展示，难以获得反馈

### 1.2 解决的问题

CraftSwap 平台专门围绕"剩余手工材料"这一小众场景，提供以下解决方案：

1. **材料交换机制**：以物换物的方式让闲置材料流动起来，创作者无需额外花钱即可获得需要的新材料
2. **精准匹配搜索**：按颜色、材质、分类、克重等维度搜索，快速找到需要的材料
3. **信用评价体系**：基于交换行为的双向评价，建立可信的社区环境
4. **作品展示社区**：晒出作品、展示材料使用成果，形成正向反馈循环
5. **数据驱动运营**：通过统计看板了解社区动态，辅助运营决策

---

## 2. 用户角色与核心用例

### 2.1 用户角色

| 角色 | 说明 | 权限 |
|------|------|------|
| 游客 | 未登录的访问者 | 浏览公开的材料列表、作品展示、统计数据 |
| 注册用户 | 已注册并登录的用户 | 拥有游客全部权限 + 发布材料、发起交换请求、发布作品、评价等所有写操作 |
| （预留）管理员 | 平台运营人员 | 拥有所有用户权限 + 删除违规内容、封禁账号等管理权限（当前版本未实现管理后台） |

### 2.2 核心用例

#### 用例 1：用户注册与登录

```mermaid
flowchart TD
    A[进入平台] --> B{是否有账号?}
    B -->|否| C[填写注册信息]
    C --> D{信息校验通过?}
    D -->|否| C
    D -->|是| E[创建账号成功]
    E --> F[自动登录获取Token]
    B -->|是| G[填写登录凭证]
    G --> H{校验通过?}
    H -->|否| G
    H -->|是| F
    F --> I[进入平台操作]
```

#### 用例 2：发布材料并发起交换

```mermaid
flowchart TD
    A[用户A登录] --> B[发布剩余材料M1]
    B --> C[填写材料信息<br/>名称/分类/颜色/数量/成色]
    C --> D[材料M1上架 - Available状态]
    E[用户B登录] --> F[搜索浏览材料]
    F --> G[发现材料M1]
    G --> H[选择自己的材料M2]
    H --> I[发起交换请求<br/>提供M2, 求M1]
    I --> J[请求状态 - Pending]
    J --> K[通知用户A]
```

#### 用例 3：交换请求处理流程

```mermaid
flowchart TD
    A[收到交换请求] --> B[查看请求详情]
    B --> C{是否接受?}
    C -->|拒绝| D[状态变为 Rejected<br/>流程结束]
    C -->|接受| E[状态变为 Accepted]
    E --> F[双方线下邮寄交换]
    F --> G[双方确认收到材料]
    G --> H[发起方将请求标记 Completed]
    H --> I[双方材料状态变为 Swapped]
    I --> J[双方互相评价]
```

---

## 3. 功能模块详细说明

### 3.1 用户认证模块（Auth）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 用户注册 | `POST /api/auth/register` | 用户名和邮箱唯一，密码需包含大小写字母和数字 | 公开 |
| 用户登录 | `POST /api/auth/login` | 支持用户名或邮箱登录，返回 JWT Token | 公开 |
| 获取当前用户信息 | `GET /api/auth/me` | 根据 Token 解析当前用户 | 需登录 |
| 更新个人信息 | `PUT /api/auth/me` | 修改头像等个人资料 | 需登录 |

**说明**：
- 密码使用 BCrypt 算法哈希存储，不可逆
- JWT Token 默认有效期 24 小时
- Token 需放在请求头 `Authorization: Bearer <token>` 中

### 3.2 材料管理模块（Materials）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 获取材料列表 | `GET /api/materials` | 支持分页、搜索关键词、分类、颜色、状态筛选、排序 | 公开 |
| 创建材料 | `POST /api/materials` | 发布剩余材料 | 需登录 |
| 获取材料详情 | `GET /api/materials/{id}` | 查看单个材料的完整信息 | 公开 |
| 更新材料 | `PUT /api/materials/{id}` | 修改材料信息 | 仅所有者 |
| 删除材料 | `DELETE /api/materials/{id}` | 删除已发布材料 | 仅所有者 |
| 修改材料状态 | `PATCH /api/materials/{id}/status` | 手动变更材料状态 | 仅所有者 |
| 获取我的材料 | `GET /api/materials/mine` | 当前用户发布的所有材料 | 需登录 |

**状态说明**：
| 状态 | 含义 | 流转条件 |
|------|------|----------|
| Available | 可交换 | 初始状态 |
| Reserved | 被预定中 | 有 Pending 状态的交换请求 |
| Swapped | 已交换 | 关联交换请求变为 Completed |
| Archived | 已归档 | 用户主动下架 |

### 3.3 交换请求管理模块（SwapRequests）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 获取请求列表 | `GET /api/swaprequests` | 分页查询，可按状态、参与用户筛选 | 公开 |
| 创建请求 | `POST /api/swaprequests` | 发起交换请求 | 需登录 |
| 获取请求详情 | `GET /api/swaprequests/{id}` | 查看请求详情 | 参与用户 |
| 更新请求 | `PUT /api/swaprequests/{id}` | 修改消息内容等 | 发起者（仅Pending状态） |
| 删除请求 | `DELETE /api/swaprequests/{id}` | 撤销请求 | 发起者（仅Pending状态） |
| 修改请求状态 | `PATCH /api/swaprequests/{id}/status` | 接受/拒绝/标记完成 | 按规则 |

**状态流转**：
```
Pending → Accepted → Completed
   ↓
Rejected
```

**业务规则**：
1. 不能向自己发起交换请求
2. 提供的材料和请求的材料不能相同
3. 材料状态必须为 Available 才能被请求
4. Accepted 状态变更时，双方材料自动变为 Reserved
5. Completed 状态变更时，双方材料自动变为 Swapped
6. 状态变更权限：Accepted/Rejected 由接收者操作；Completed 由发起者操作

### 3.4 交换评价管理模块（SwapReviews）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 获取评价列表 | `GET /api/swapreviews` | 分页查询，可按用户、请求ID筛选 | 公开 |
| 创建评价 | `POST /api/swapreviews` | 交换完成后评价对方 | 参与用户 |
| 获取评价详情 | `GET /api/swapreviews/{id}` | 查看评价详情 | 公开 |
| 更新评价 | `PUT /api/swapreviews/{id}` | 修改评价内容和评分 | 评价者 |
| 删除评价 | `DELETE /api/swapreviews/{id}` | 删除评价 | 评价者 |

**业务规则**：
1. 仅当交换请求状态为 Completed 时才能创建评价
2. 每个用户对同一个交换请求只能评价一次
3. 评分范围 1-5 星
4. 评价者不能评价自己

### 3.5 作品管理模块（ProjectShowcases）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 获取作品列表 | `GET /api/projectshowcases` | 分页查询，关键词搜索 | 公开 |
| 创建作品 | `POST /api/projectshowcases` | 发布作品晒单 | 需登录 |
| 获取作品详情 | `GET /api/projectshowcases/{id}` | 查看作品详情 | 公开 |
| 更新作品 | `PUT /api/projectshowcases/{id}` | 修改作品信息 | 仅作者 |
| 删除作品 | `DELETE /api/projectshowcases/{id}` | 删除作品 | 仅作者 |
| 获取我的作品 | `GET /api/projectshowcases/mine` | 当前用户发布的作品 | 需登录 |

### 3.6 统计模块（Stats）

| 功能点 | 接口 | 说明 | 权限 |
|--------|------|------|------|
| 总览统计 | `GET /api/stats/overview` | 平台总数据：用户数、材料数、交换请求数、各状态统计、作品数、评价数 | 公开 |
| 趋势统计 | `GET /api/stats/trend` | 指定时间范围内的每日新增数据趋势 | 公开 |

---

## 4. 数据库 ER 图文字描述（表关系）

### 4.1 表清单

| 表名 | 中文名 | 核心字段 |
|------|--------|----------|
| Users | 用户表 | Id, Username(唯一), Email(唯一), PasswordHash, Avatar, CreatedAt, UpdatedAt |
| Materials | 材料表 | Id, OwnerId(FK→Users), Name, Category, Color, MaterialType, Quantity, Unit, Condition, Photos, Status, CreatedAt |
| SwapRequests | 交换请求表 | Id, ProposerId(FK→Users), ReceiverId(FK→Users), OfferedMaterialId(FK→Materials), RequestedMaterialId(FK→Materials), Message, Status, CreatedAt |
| SwapReviews | 交换评价表 | Id, RequestId(FK→SwapRequests), ReviewerId(FK→Users), RevieweeId(FK→Users), Rating, Content, CreatedAt |
| ProjectShowcases | 作品展示表 | Id, UserId(FK→Users), Title, Description, UsedMaterials, Photos, CreatedAt |

### 4.2 关系说明

```
Users (1) ────< (N) Materials
  │                │
  │                │
  │  (N) Proposer  │ (N) Offered
  ├─────< SwapRequests >─────┤
  │  (N) Receiver  │ (N) Requested
  │                │
  │                │
  │ (N) Reviewer   │
  ├─────< SwapReviews >─────┤
  │ (N) Reviewee   │ (1) Request
  │
  └─────< (N) ProjectShowcases
```

**关系详解**：
1. **Users → Materials**：一对多，一个用户可以发布多个材料
2. **Users → SwapRequests**：两对多关系
   - 作为发起者（Proposer）：一个用户可以发起多个交换请求
   - 作为接收者（Receiver）：一个用户可以接收多个交换请求
3. **Materials → SwapRequests**：两对多关系
   - 作为提供材料（OfferedMaterial）：一个材料可以在多个请求中被提供（理论上只有一个成功）
   - 作为被请求材料（RequestedMaterial）：一个材料可以被多个交换请求申请
4. **SwapRequests → SwapReviews**：一对多，一个交换请求可以有0-2条评价（双方各评一次）
5. **Users → SwapReviews**：两对多关系
   - 作为评价者（Reviewer）：一个用户可以做出多条评价
   - 作为被评价者（Reviewee）：一个用户可以收到多条评价
6. **Users → ProjectShowcases**：一对多，一个用户可以发布多个作品

---

## 5. 关键业务规则

### 5.1 状态流转规则

**材料状态流转**：
```
Available (发布)
    │
    ├──→ Reserved (有Pending请求)
    │       │
    │       ├──→ Swapped (关联请求Completed)
    │       └──→ Available (请求被拒绝/删除)
    │
    └──→ Archived (用户主动归档)
```

**交换请求状态流转**：
```
Pending (发起)
    │
    ├──→ Accepted (接收者同意) ──→ Completed (发起者确认完成)
    │
    └──→ Rejected (接收者拒绝)
```

### 5.2 权限规则

| 操作 | 权限条件 |
|------|----------|
| 创建材料、作品、交换请求 | 必须登录（携带有效 JWT） |
| 修改/删除 材料、作品 | 必须是资源所有者（OwnerId/UserId == 当前用户Id） |
| 修改/删除 交换请求（Pending状态） | 必须是发起者（ProposerId == 当前用户Id） |
| 接受/拒绝 交换请求 | 必须是接收者（ReceiverId == 当前用户Id） |
| 标记交换请求 Completed | 必须是发起者（ProposerId == 当前用户Id） |
| 创建评价 | 必须是请求参与者，且请求状态为 Completed，未评价过，不能自评 |
| 修改/删除 评价 | 必须是评价者（ReviewerId == 当前用户Id） |

### 5.3 分页规则

- 默认页码 `pageNumber = 1`，默认每页大小 `pageSize = 10`
- `pageSize` 最大值限制为 100，防止过度请求
- 分页响应统一格式：
  ```json
  {
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "items": [ ... ]
  }
  ```

### 5.4 时间计算逻辑

- 所有时间字段存储为 UTC 时间
- CreatedAt 在实体创建时自动设置为 `DateTime.UtcNow`
- UpdatedAt 在用户资料更新时自动刷新
- 趋势统计按日期分组（本地时区转换）

---

## 6. 接口调用示例

### 示例 1：用户注册 + 登录

**请求 - 注册**：
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "handmade_lover",
  "email": "lover@example.com",
  "password": "MyCraft@2024",
  "confirmPassword": "MyCraft@2024"
}
```

**响应 - 注册成功**：
```json
{
  "code": 200,
  "message": "注册成功",
  "data": {
    "id": 4,
    "username": "handmade_lover",
    "email": "lover@example.com",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

**请求 - 登录**：
```http
POST /api/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "handmade_lover",
  "password": "MyCraft@2024"
}
```

**响应 - 登录成功**：
```json
{
  "code": 200,
  "message": "登录成功",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 1440,
    "user": {
      "id": 4,
      "username": "handmade_lover",
      "email": "lover@example.com",
      "avatar": null
    }
  }
}
```

---

### 示例 2：发布材料

**请求**：
```http
POST /api/materials
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "奶白色纯棉线",
  "category": "毛线",
  "color": "奶白",
  "materialType": "纯棉",
  "quantity": 200,
  "unit": "g",
  "condition": "全新未拆封",
  "photos": "https://example.com/yarn1.jpg,https://example.com/yarn2.jpg"
}
```

**响应**：
```json
{
  "code": 200,
  "message": "材料创建成功",
  "data": {
    "id": 11,
    "ownerId": 4,
    "name": "奶白色纯棉线",
    "category": "毛线",
    "color": "奶白",
    "materialType": "纯棉",
    "quantity": 200,
    "unit": "g",
    "condition": "全新未拆封",
    "photos": "https://example.com/yarn1.jpg,https://example.com/yarn2.jpg",
    "status": "Available",
    "createdAt": "2024-01-15T10:35:00Z",
    "owner": {
      "id": 4,
      "username": "handmade_lover",
      "avatar": null
    }
  }
}
```

---

### 示例 3：发起交换请求

**请求**：
```http
POST /api/swaprequests
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "offeredMaterialId": 11,
  "requestedMaterialId": 5,
  "message": "你好！我用200g奶白棉线换你的藏蓝色刺绣线，我这边邮费自理可以吗？"
}
```

**响应**：
```json
{
  "code": 200,
  "message": "交换请求创建成功",
  "data": {
    "id": 4,
    "proposerId": 4,
    "receiverId": 2,
    "offeredMaterialId": 11,
    "requestedMaterialId": 5,
    "message": "你好！我用200g奶白棉线换你的藏蓝色刺绣线，我这边邮费自理可以吗？",
    "status": "Pending",
    "createdAt": "2024-01-15T10:40:00Z"
  }
}
```

---

## 附录：统一返回格式说明

所有 API 接口返回统一的 JSON 包装格式：

```json
{
  "code": 200,       // 状态码：200成功，400参数错误，401未授权，403无权限，404不存在，500服务器错误
  "message": "操作成功", // 提示信息
  "data": { ... }    // 业务数据（对象/数组/null）
}
```

分页接口的 `data` 字段结构：
```json
{
  "code": 200,
  "message": "获取成功",
  "data": {
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "items": [ ... ]
  }
}
```

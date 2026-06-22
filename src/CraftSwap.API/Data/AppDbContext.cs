using CraftSwap.Entities;
using CraftSwap.Common;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// 用户数据集
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 材料数据集
    /// </summary>
    public DbSet<Material> Materials { get; set; }

    /// <summary>
    /// 交换请求数据集
    /// </summary>
    public DbSet<SwapRequest> SwapRequests { get; set; }

    /// <summary>
    /// 交换评价数据集
    /// </summary>
    public DbSet<SwapReview> SwapReviews { get; set; }

    /// <summary>
    /// 作品展示数据集
    /// </summary>
    public DbSet<ProjectShowcase> ProjectShowcases { get; set; }

    /// <summary>
    /// 刷新令牌（会话）数据集
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// 系统日志数据集
    /// </summary>
    public DbSet<SystemLog> SystemLogs { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文配置选项</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置实体模型和关系
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);
        ConfigureMaterialEntity(modelBuilder);
        ConfigureSwapRequestEntity(modelBuilder);
        ConfigureSwapReviewEntity(modelBuilder);
        ConfigureProjectShowcaseEntity(modelBuilder);
        ConfigureSystemLogEntity(modelBuilder);

        SeedData(modelBuilder);
    }

    /// <summary>
    /// 配置用户实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasMany(u => u.Materials)
                  .WithOne(m => m.Owner)
                  .HasForeignKey(m => m.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ProposedSwapRequests)
                  .WithOne(s => s.Proposer)
                  .HasForeignKey(s => s.ProposerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ReceivedSwapRequests)
                  .WithOne(s => s.Receiver)
                  .HasForeignKey(s => s.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.GivenReviews)
                  .WithOne(r => r.Reviewer)
                  .HasForeignKey(r => r.ReviewerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ReceivedReviews)
                  .WithOne(r => r.Reviewee)
                  .HasForeignKey(r => r.RevieweeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ProjectShowcases)
                  .WithOne(p => p.User)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// 配置刷新令牌（会话）实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.AccessTokenJti).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => rt.ExpiresAt);

            entity.HasOne(rt => rt.User)
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置材料实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureMaterialEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasIndex(m => m.Category);
            entity.HasIndex(m => m.Status);
            entity.HasIndex(m => m.OwnerId);
            entity.HasIndex(m => m.ViewCount);
            entity.HasIndex(m => m.FavoriteCount);
            entity.HasIndex(m => m.CreatedAt);

            entity.HasMany(m => m.OfferedInRequests)
                  .WithOne(s => s.OfferedMaterial)
                  .HasForeignKey(s => s.OfferedMaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.RequestedInRequests)
                  .WithOne(s => s.RequestedMaterial)
                  .HasForeignKey(s => s.RequestedMaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// 配置交换请求实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureSwapRequestEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SwapRequest>(entity =>
        {
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.ProposerId);
            entity.HasIndex(s => s.ReceiverId);
            entity.HasIndex(s => s.CreatedAt);

            entity.HasMany(s => s.Reviews)
                  .WithOne(r => r.Request)
                  .HasForeignKey(r => r.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置交换评价实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureSwapReviewEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SwapReview>(entity =>
        {
            entity.HasIndex(r => r.RequestId);
            entity.HasIndex(r => r.ReviewerId);
            entity.HasIndex(r => r.RevieweeId);
            entity.HasIndex(r => r.Rating);
        });
    }

    /// <summary>
    /// 配置作品展示实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureProjectShowcaseEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectShowcase>(entity =>
        {
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.CreatedAt);
        });
    }

    /// <summary>
    /// 配置系统日志实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void ConfigureSystemLogEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasIndex(s => s.LogLevel);
            entity.HasIndex(s => s.EventType);
            entity.HasIndex(s => s.OperatorId);
            entity.HasIndex(s => s.TargetUserId);
            entity.HasIndex(s => s.CreatedAt);

            entity.HasOne(sl => sl.Operator)
                  .WithMany()
                  .HasForeignKey(sl => sl.OperatorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sl => sl.TargetUser)
                  .WithMany()
                  .HasForeignKey(sl => sl.TargetUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// 种子数据初始化
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var users = new[]
        {
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Avatar = "https://example.com/avatars/admin.jpg",
                Role = AppConstants.UserRoles.Admin,
                IsLocked = false,
                AverageRating = 0m,
                TotalReviewCount = 0,
                CreatedAt = now,
                UpdatedAt = now
            },
            new User
            {
                Id = 2,
                Username = "zhangsan",
                Email = "zhangsan@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/zhangsan.jpg",
                Role = AppConstants.UserRoles.User,
                IsLocked = false,
                AverageRating = 0m,
                TotalReviewCount = 0,
                CreatedAt = now.AddHours(1),
                UpdatedAt = now.AddHours(1)
            },
            new User
            {
                Id = 3,
                Username = "lisi",
                Email = "lisi@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/lisi.jpg",
                Role = AppConstants.UserRoles.User,
                IsLocked = false,
                AverageRating = 4.00m,
                TotalReviewCount = 1,
                CreatedAt = now.AddDays(1),
                UpdatedAt = now.AddDays(1)
            },
            new User
            {
                Id = 4,
                Username = "wangwu",
                Email = "wangwu@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/wangwu.jpg",
                Role = AppConstants.UserRoles.User,
                IsLocked = false,
                AverageRating = 5.00m,
                TotalReviewCount = 1,
                CreatedAt = now.AddDays(2),
                UpdatedAt = now.AddDays(2)
            }
        };

        modelBuilder.Entity<User>().HasData(users);

        var materials = new[]
        {
            new Material
            {
                Id = 1,
                OwnerId = 2,
                Title = "优质红色羊毛线",
                Description = "500克装优质100%纯羊毛线，色泽鲜艳，手感柔软，适合编织围巾、毛衣等冬季保暖用品。全新未拆封，附编织教程一份。",
                Tags = "羊毛,红色,毛线,冬季,保暖",
                Name = "红色羊毛线",
                Category = "毛线",
                Color = "红色",
                MaterialType = "羊毛",
                Quantity = 500,
                Unit = "克",
                Condition = "全新",
                Photos = "https://example.com/materials/1.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 128,
                FavoriteCount = 15,
                CreatedAt = now
            },
            new Material
            {
                Id = 2,
                OwnerId = 2,
                Title = "蓝色纯棉布料",
                Description = "5米长高品质蓝色纯棉布料，幅宽1.5米，质地厚实透气，适合制作衬衫、连衣裙等夏季服装。仅使用过一小段，剩余约4.8米。",
                Tags = "纯棉,蓝色,布料,夏季,透气",
                Name = "蓝色棉布",
                Category = "布料",
                Color = "蓝色",
                MaterialType = "纯棉",
                Quantity = 5,
                Unit = "米",
                Condition = "九成新",
                Photos = "https://example.com/materials/2.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 86,
                FavoriteCount = 9,
                CreatedAt = now.AddHours(2)
            },
            new Material
            {
                Id = 3,
                OwnerId = 3,
                Title = "高档绿色真丝面料",
                Description = "3米重磅真丝面料，16姆米厚度，光泽度极佳，手感丝滑。适合制作旗袍、丝巾等高档服饰。保存完好，无污渍无破损。",
                Tags = "丝绸,绿色,真丝,高档,礼服",
                Name = "绿色丝绸",
                Category = "布料",
                Color = "绿色",
                MaterialType = "丝绸",
                Quantity = 3,
                Unit = "米",
                Condition = "全新",
                Photos = "https://example.com/materials/3.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 256,
                FavoriteCount = 32,
                CreatedAt = now.AddHours(4)
            },
            new Material
            {
                Id = 4,
                OwnerId = 3,
                Title = "黄色树脂纽扣套装",
                Description = "100颗高品质树脂纽扣，直径15mm和20mm各50颗，颜色均匀有光泽。附带精美收纳盒，适合各种手工服装制作。",
                Tags = "纽扣,黄色,配件,树脂,套装",
                Name = "黄色纽扣套装",
                Category = "配件",
                Color = "黄色",
                MaterialType = "树脂",
                Quantity = 100,
                Unit = "颗",
                Condition = "全新",
                Photos = "https://example.com/materials/4.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 45,
                FavoriteCount = 7,
                CreatedAt = now.AddHours(6)
            },
            new Material
            {
                Id = 5,
                OwnerId = 4,
                Title = "紫色精美蕾丝花边",
                Description = "10米长高品质紫色蕾丝花边，宽度5cm，花纹精致典雅。适合装饰连衣裙、婚纱、家纺等，弹性好不易变形。",
                Tags = "蕾丝,紫色,配件,花边,装饰",
                Name = "紫色蕾丝边",
                Category = "配件",
                Color = "紫色",
                MaterialType = "尼龙",
                Quantity = 10,
                Unit = "米",
                Condition = "全新",
                Photos = "https://example.com/materials/5.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 178,
                FavoriteCount = 23,
                CreatedAt = now.AddHours(8)
            },
            new Material
            {
                Id = 6,
                OwnerId = 4,
                Title = "白色腈纶毛线",
                Description = "300克白色腈纶毛线，不起球不褪色，易护理。适合初学者练手，可编织小挂件、玩偶、围巾等。剩余八成新，约250克可用。",
                Tags = "腈纶,白色,毛线,新手,玩偶",
                Name = "白色毛线",
                Category = "毛线",
                Color = "白色",
                MaterialType = "腈纶",
                Quantity = 300,
                Unit = "克",
                Condition = "八成新",
                Photos = "https://example.com/materials/6.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 62,
                FavoriteCount = 5,
                CreatedAt = now.AddHours(10)
            },
            new Material
            {
                Id = 7,
                OwnerId = 2,
                Title = "黑色头层牛皮皮革",
                Description = "2平方尺头层牛皮，厚度1.2-1.4mm，质地柔软有韧性。适合制作钱包、卡包、表带等手工皮具。皮面光滑无瑕疵。",
                Tags = "皮革,黑色,牛皮,手工,皮具",
                Name = "黑色皮革",
                Category = "皮革",
                Color = "黑色",
                MaterialType = "牛皮",
                Quantity = 2,
                Unit = "平方尺",
                Condition = "全新",
                Photos = "https://example.com/materials/7.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 312,
                FavoriteCount = 41,
                CreatedAt = now.AddDays(1)
            },
            new Material
            {
                Id = 8,
                OwnerId = 3,
                Title = "粉色缎面绸缎",
                Description = "4米粉色绸缎面料，光泽柔和，垂感好，适合制作汉服、襦裙、伴娘服等。已使用约0.5米，剩余3.5米，边角略有裁剪痕迹。",
                Tags = "绸缎,粉色,汉服,礼服,光泽",
                Name = "粉色绸缎",
                Category = "布料",
                Color = "粉色",
                MaterialType = "绸缎",
                Quantity = 4,
                Unit = "米",
                Condition = "九成新",
                Photos = "https://example.com/materials/8.jpg",
                Status = AppConstants.MaterialStatus.Swapped,
                ViewCount = 198,
                FavoriteCount = 28,
                CreatedAt = now.AddDays(1).AddHours(2)
            },
            new Material
            {
                Id = 9,
                OwnerId = 4,
                Title = "金色金属拉链",
                Description = "50条高档金色金属拉链，长度20cm，顺滑不卡齿。YKK品质，适合制作外套、包包、裙子等。独立包装，全新未使用。",
                Tags = "拉链,金色,金属,配件,YKK",
                Name = "金色拉链",
                Category = "配件",
                Color = "金色",
                MaterialType = "金属",
                Quantity = 50,
                Unit = "条",
                Condition = "全新",
                Photos = "https://example.com/materials/9.jpg",
                Status = AppConstants.MaterialStatus.Available,
                ViewCount = 73,
                FavoriteCount = 11,
                CreatedAt = now.AddDays(1).AddHours(4)
            },
            new Material
            {
                Id = 10,
                OwnerId = 2,
                Title = "灰色加厚帆布",
                Description = "6米加厚灰色帆布，380g重磅面料，耐磨耐用，适合制作背包、帆布鞋、坐垫等。因项目剩余，约六成可用，边角有少量污渍。",
                Tags = "帆布,灰色,加厚,耐磨,背包",
                Name = "灰色帆布",
                Category = "布料",
                Color = "灰色",
                MaterialType = "帆布",
                Quantity = 6,
                Unit = "米",
                Condition = "七成新",
                Photos = "https://example.com/materials/10.jpg",
                Status = AppConstants.MaterialStatus.Offline,
                ViewCount = 41,
                FavoriteCount = 3,
                CreatedAt = now.AddDays(2)
            }
        };

        modelBuilder.Entity<Material>().HasData(materials);

        var swapRequests = new[]
        {
            new SwapRequest
            {
                Id = 1,
                ProposerId = 2,
                ReceiverId = 3,
                OfferedMaterialId = 1,
                RequestedMaterialId = 3,
                Message = "我想用红色羊毛线换你的绿色丝绸，可以吗？",
                Status = AppConstants.SwapRequestStatus.Pending,
                CreatedAt = now.AddDays(3)
            },
            new SwapRequest
            {
                Id = 2,
                ProposerId = 3,
                ReceiverId = 4,
                OfferedMaterialId = 4,
                RequestedMaterialId = 5,
                Message = "想用黄色纽扣换紫色蕾丝边~",
                Status = AppConstants.SwapRequestStatus.Completed,
                CreatedAt = now.AddDays(4)
            },
            new SwapRequest
            {
                Id = 3,
                ProposerId = 4,
                ReceiverId = 2,
                OfferedMaterialId = 6,
                RequestedMaterialId = 2,
                Message = "白色毛线换蓝色棉布怎么样？",
                Status = AppConstants.SwapRequestStatus.Accepted,
                CreatedAt = now.AddDays(5)
            }
        };

        modelBuilder.Entity<SwapRequest>().HasData(swapRequests);

        var swapReviews = new[]
        {
            new SwapReview
            {
                Id = 1,
                RequestId = 2,
                ReviewerId = 3,
                RevieweeId = 4,
                Rating = 5,
                Content = "交换非常顺利，对方很友善，材料质量也很好！",
                CreatedAt = now.AddDays(6)
            },
            new SwapReview
            {
                Id = 2,
                RequestId = 2,
                ReviewerId = 4,
                RevieweeId = 3,
                Rating = 4,
                Content = "整体不错，材料符合描述，沟通顺畅。",
                CreatedAt = now.AddDays(6).AddHours(2)
            }
        };

        modelBuilder.Entity<SwapReview>().HasData(swapReviews);

        var projectShowcases = new[]
        {
            new ProjectShowcase
            {
                Id = 1,
                UserId = 2,
                Title = "手工编织红色围巾",
                Description = "用红色羊毛线编织的围巾，温暖又时尚，适合冬天佩戴。针法采用了基础的平针，适合初学者学习。",
                UsedMaterials = "红色羊毛线500克",
                Photos = "https://example.com/projects/1.jpg",
                CreatedAt = now.AddDays(7)
            },
            new ProjectShowcase
            {
                Id = 2,
                UserId = 3,
                Title = "丝绸手工艺品",
                Description = "用绿色丝绸制作的手工花束，色彩鲜艳，造型精美，可以作为家居装饰。",
                UsedMaterials = "绿色丝绸2米",
                Photos = "https://example.com/projects/2.jpg",
                CreatedAt = now.AddDays(8)
            },
            new ProjectShowcase
            {
                Id = 3,
                UserId = 4,
                Title = "蕾丝装饰小布袋",
                Description = "用紫色蕾丝边装饰的棉麻小布袋，精致可爱，可以用来装小物件。",
                UsedMaterials = "紫色蕾丝边2米，棉麻布1米",
                Photos = "https://example.com/projects/3.jpg",
                CreatedAt = now.AddDays(9)
            }
        };

        modelBuilder.Entity<ProjectShowcase>().HasData(projectShowcases);
    }
}

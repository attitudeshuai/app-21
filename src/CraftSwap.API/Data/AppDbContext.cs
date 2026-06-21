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
                Username = "zhangsan",
                Email = "zhangsan@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/zhangsan.jpg",
                CreatedAt = now,
                UpdatedAt = now
            },
            new User
            {
                Id = 2,
                Username = "lisi",
                Email = "lisi@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/lisi.jpg",
                CreatedAt = now.AddDays(1),
                UpdatedAt = now.AddDays(1)
            },
            new User
            {
                Id = 3,
                Username = "wangwu",
                Email = "wangwu@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Avatar = "https://example.com/avatars/wangwu.jpg",
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
                OwnerId = 1,
                Name = "红色羊毛线",
                Category = "毛线",
                Color = "红色",
                MaterialType = "羊毛",
                Quantity = 500,
                Unit = "克",
                Condition = "全新",
                Photos = "https://example.com/materials/1.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now
            },
            new Material
            {
                Id = 2,
                OwnerId = 1,
                Name = "蓝色棉布",
                Category = "布料",
                Color = "蓝色",
                MaterialType = "纯棉",
                Quantity = 5,
                Unit = "米",
                Condition = "九成新",
                Photos = "https://example.com/materials/2.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddHours(2)
            },
            new Material
            {
                Id = 3,
                OwnerId = 2,
                Name = "绿色丝绸",
                Category = "布料",
                Color = "绿色",
                MaterialType = "丝绸",
                Quantity = 3,
                Unit = "米",
                Condition = "全新",
                Photos = "https://example.com/materials/3.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddHours(4)
            },
            new Material
            {
                Id = 4,
                OwnerId = 2,
                Name = "黄色纽扣套装",
                Category = "配件",
                Color = "黄色",
                MaterialType = "树脂",
                Quantity = 100,
                Unit = "颗",
                Condition = "全新",
                Photos = "https://example.com/materials/4.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddHours(6)
            },
            new Material
            {
                Id = 5,
                OwnerId = 3,
                Name = "紫色蕾丝边",
                Category = "配件",
                Color = "紫色",
                MaterialType = "尼龙",
                Quantity = 10,
                Unit = "米",
                Condition = "全新",
                Photos = "https://example.com/materials/5.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddHours(8)
            },
            new Material
            {
                Id = 6,
                OwnerId = 3,
                Name = "白色毛线",
                Category = "毛线",
                Color = "白色",
                MaterialType = "腈纶",
                Quantity = 300,
                Unit = "克",
                Condition = "八成新",
                Photos = "https://example.com/materials/6.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddHours(10)
            },
            new Material
            {
                Id = 7,
                OwnerId = 1,
                Name = "黑色皮革",
                Category = "皮革",
                Color = "黑色",
                MaterialType = "牛皮",
                Quantity = 2,
                Unit = "平方尺",
                Condition = "全新",
                Photos = "https://example.com/materials/7.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddDays(1)
            },
            new Material
            {
                Id = 8,
                OwnerId = 2,
                Name = "粉色绸缎",
                Category = "布料",
                Color = "粉色",
                MaterialType = "绸缎",
                Quantity = 4,
                Unit = "米",
                Condition = "九成新",
                Photos = "https://example.com/materials/8.jpg",
                Status = AppConstants.MaterialStatus.Swapped,
                CreatedAt = now.AddDays(1).AddHours(2)
            },
            new Material
            {
                Id = 9,
                OwnerId = 3,
                Name = "金色拉链",
                Category = "配件",
                Color = "金色",
                MaterialType = "金属",
                Quantity = 50,
                Unit = "条",
                Condition = "全新",
                Photos = "https://example.com/materials/9.jpg",
                Status = AppConstants.MaterialStatus.Available,
                CreatedAt = now.AddDays(1).AddHours(4)
            },
            new Material
            {
                Id = 10,
                OwnerId = 1,
                Name = "灰色帆布",
                Category = "布料",
                Color = "灰色",
                MaterialType = "帆布",
                Quantity = 6,
                Unit = "米",
                Condition = "七成新",
                Photos = "https://example.com/materials/10.jpg",
                Status = AppConstants.MaterialStatus.Offline,
                CreatedAt = now.AddDays(2)
            }
        };

        modelBuilder.Entity<Material>().HasData(materials);

        var swapRequests = new[]
        {
            new SwapRequest
            {
                Id = 1,
                ProposerId = 1,
                ReceiverId = 2,
                OfferedMaterialId = 1,
                RequestedMaterialId = 3,
                Message = "我想用红色羊毛线换你的绿色丝绸，可以吗？",
                Status = AppConstants.SwapRequestStatus.Pending,
                CreatedAt = now.AddDays(3)
            },
            new SwapRequest
            {
                Id = 2,
                ProposerId = 2,
                ReceiverId = 3,
                OfferedMaterialId = 4,
                RequestedMaterialId = 5,
                Message = "想用黄色纽扣换紫色蕾丝边~",
                Status = AppConstants.SwapRequestStatus.Completed,
                CreatedAt = now.AddDays(4)
            },
            new SwapRequest
            {
                Id = 3,
                ProposerId = 3,
                ReceiverId = 1,
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
                ReviewerId = 2,
                RevieweeId = 3,
                Rating = 5,
                Content = "交换非常顺利，对方很友善，材料质量也很好！",
                CreatedAt = now.AddDays(6)
            },
            new SwapReview
            {
                Id = 2,
                RequestId = 2,
                ReviewerId = 3,
                RevieweeId = 2,
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
                UserId = 1,
                Title = "手工编织红色围巾",
                Description = "用红色羊毛线编织的围巾，温暖又时尚，适合冬天佩戴。针法采用了基础的平针，适合初学者学习。",
                UsedMaterials = "红色羊毛线500克",
                Photos = "https://example.com/projects/1.jpg",
                CreatedAt = now.AddDays(7)
            },
            new ProjectShowcase
            {
                Id = 2,
                UserId = 2,
                Title = "丝绸手工艺品",
                Description = "用绿色丝绸制作的手工花束，色彩鲜艳，造型精美，可以作为家居装饰。",
                UsedMaterials = "绿色丝绸2米",
                Photos = "https://example.com/projects/2.jpg",
                CreatedAt = now.AddDays(8)
            },
            new ProjectShowcase
            {
                Id = 3,
                UserId = 3,
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

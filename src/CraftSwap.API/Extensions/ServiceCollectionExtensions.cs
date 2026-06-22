using System.Reflection;
using System.Text;
using CraftSwap.Common;
using CraftSwap.Data;
using CraftSwap.Repositories;
using CraftSwap.Services;
using CraftSwap.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CraftSwap.Extensions;

/// <summary>
/// 服务集合扩展方法类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加应用程序服务配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddAutoMapperConfig();
        services.AddFluentValidationConfig();
        services.AddSwaggerConfig();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddRepositories();
        services.AddServices();
        services.AddControllers();

        return services;
    }

    /// <summary>
    /// 添加数据库上下文配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    private static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("未找到数据库连接字符串 'DefaultConnection'");

        services.AddDbContext<AppDbContext>(options =>
        {
            var serverVersion = ServerVersion.Parse("8.0.0-mysql");
            options.UseMySql(connectionString, serverVersion, mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });
    }

    /// <summary>
    /// 添加JWT认证配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("未找到 JWT 配置节 'JwtSettings'");

        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey 不能为空");
        }

        var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
    }

    /// <summary>
    /// 添加AutoMapper配置
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void AddAutoMapperConfig(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// 添加FluentValidation配置
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void AddFluentValidationConfig(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
    }

    /// <summary>
    /// 添加Swagger配置
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CraftSwap API",
                Version = "v1",
                Description = "手工材料交换平台 API 接口文档",
                Contact = new OpenApiContact
                {
                    Name = "CraftSwap Team",
                    Email = "support@craftswap.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "请输入 JWT 令牌，格式：Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
    }

    /// <summary>
    /// 注册仓储服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ISwapRequestRepository, SwapRequestRepository>();
        services.AddScoped<ISwapReviewRepository, SwapReviewRepository>();
        services.AddScoped<IProjectShowcaseRepository, ProjectShowcaseRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
    }

    /// <summary>
    /// 注册业务服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenValidatorService, TokenValidatorService>();
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<ISwapRequestService, SwapRequestService>();
        services.AddScoped<ISwapReviewService, SwapReviewService>();
        services.AddScoped<IProjectShowcaseService, ProjectShowcaseService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISystemLogService, SystemLogService>();
        services.AddScoped<ISwapRequestStatusValidator, SwapRequestStatusValidator>();
        services.AddScoped<IMaterialSyncService, MaterialSyncService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IHighlightService, HighlightService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
    }
}

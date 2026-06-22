using AutoMapper;
using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Materials;
using CraftSwap.DTOs.ProjectShowcases;
using CraftSwap.DTOs.SwapRequests;
using CraftSwap.DTOs.SwapReviews;
using CraftSwap.Entities;

namespace CraftSwap.MappingProfiles;

/// <summary>
/// AutoMapper映射配置
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// 构造函数，配置所有映射关系
    /// </summary>
    public MappingProfile()
    {
        CreateUserMaps();
        CreateMaterialMaps();
        CreateSwapRequestMaps();
        CreateSwapReviewMaps();
        CreateProjectShowcaseMaps();
    }

    /// <summary>
    /// 配置用户相关映射
    /// </summary>
    private void CreateUserMaps()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.Bio, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.TotalReviewCount, opt => opt.MapFrom(src => src.TotalReviewCount));

        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.AvatarUrl))
            .ForMember(dest => dest.Materials, opt => opt.Ignore())
            .ForMember(dest => dest.ProposedSwapRequests, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedSwapRequests, opt => opt.Ignore())
            .ForMember(dest => dest.GivenReviews, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedReviews, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectShowcases, opt => opt.Ignore())
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.TotalReviewCount, opt => opt.Ignore());

        CreateMap<UpdateProfileRequest, User>()
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.AvatarUrl))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// 配置材料相关映射
    /// </summary>
    private void CreateMaterialMaps()
    {
        CreateMap<Material, MaterialResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Title) ? src.Title : src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Description) ? src.Description : src.Condition))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Photos) ? new List<string>() : src.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Tags) ? new List<string>() : src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId.ToString()))
            .ForMember(dest => dest.OwnerUsername, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Username : string.Empty))
            .ForMember(dest => dest.OwnerAvatarUrl, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Avatar : null));

        CreateMap<CreateMaterialRequest, Material>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Description) ? (src.Description.Length > 50 ? src.Description.Substring(0, 50) : src.Description) : string.Empty))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null && src.Tags.Count > 0 ? string.Join(',', src.Tags) : null))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.ImageUrls != null && src.ImageUrls.Count > 0 ? string.Join(',', src.ImageUrls) : null))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.OfferedInRequests, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedInRequests, opt => opt.Ignore())
            .ForMember(dest => dest.Color, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialType, opt => opt.Ignore())
            .ForMember(dest => dest.Quantity, opt => opt.Ignore())
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        CreateMap<UpdateMaterialRequest, Material>()
            .ForMember(dest => dest.Title, opt => opt.Condition(src => src.Title != null))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title!))
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Title != null))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title!))
            .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description!))
            .ForMember(dest => dest.Condition, opt => opt.Condition(src => src.Description != null))
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Description!.Length > 50 ? src.Description.Substring(0, 50) : src.Description))
            .ForMember(dest => dest.Tags, opt => opt.Condition(src => src.Tags != null))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null && src.Tags.Count > 0 ? string.Join(',', src.Tags) : null))
            .ForMember(dest => dest.Photos, opt => opt.Condition(src => src.ImageUrls != null))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.ImageUrls != null && src.ImageUrls.Count > 0 ? string.Join(',', src.ImageUrls) : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// 配置交换请求相关映射
    /// </summary>
    private void CreateSwapRequestMaps()
    {
        CreateMap<SwapRequest, SwapRequestResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => $"交换请求 #{src.Id}"))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.RequesterMaterialId, opt => opt.MapFrom(src => src.OfferedMaterialId))
            .ForMember(dest => dest.RequesterMaterialTitle, opt => opt.MapFrom(src => src.OfferedMaterial != null ? src.OfferedMaterial.Name : string.Empty))
            .ForMember(dest => dest.ResponderMaterialId, opt => opt.MapFrom(src => src.RequestedMaterialId))
            .ForMember(dest => dest.ResponderMaterialTitle, opt => opt.MapFrom(src => src.RequestedMaterial != null ? src.RequestedMaterial.Name : string.Empty))
            .ForMember(dest => dest.RequesterId, opt => opt.MapFrom(src => src.ProposerId.ToString()))
            .ForMember(dest => dest.RequesterUsername, opt => opt.MapFrom(src => src.Proposer != null ? src.Proposer.Username : string.Empty))
            .ForMember(dest => dest.RequesterAvatarUrl, opt => opt.MapFrom(src => src.Proposer != null ? src.Proposer.Avatar : null))
            .ForMember(dest => dest.ResponderId, opt => opt.MapFrom(src => src.ReceiverId.ToString()))
            .ForMember(dest => dest.ResponderUsername, opt => opt.MapFrom(src => src.Receiver != null ? src.Receiver.Username : string.Empty))
            .ForMember(dest => dest.ResponderAvatarUrl, opt => opt.MapFrom(src => src.Receiver != null ? src.Receiver.Avatar : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<CreateSwapRequestRequest, SwapRequest>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.OfferedMaterialId, opt => opt.MapFrom(src => src.RequesterMaterialId))
            .ForMember(dest => dest.RequestedMaterialId, opt => opt.MapFrom(src => src.ResponderMaterialId))
            .ForMember(dest => dest.ProposerId, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiverId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Proposer, opt => opt.Ignore())
            .ForMember(dest => dest.Receiver, opt => opt.Ignore())
            .ForMember(dest => dest.OfferedMaterial, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedMaterial, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        CreateMap<UpdateSwapRequestRequest, SwapRequest>()
            .ForMember(dest => dest.Message, opt => opt.Condition(src => src.Description != null))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Description!))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// 配置交换评价相关映射
    /// </summary>
    private void CreateSwapReviewMaps()
    {
        CreateMap<SwapReview, SwapReviewResponse>()
            .ForMember(dest => dest.SwapRequestId, opt => opt.MapFrom(src => src.RequestId))
            .ForMember(dest => dest.ReviewerId, opt => opt.MapFrom(src => src.ReviewerId.ToString()))
            .ForMember(dest => dest.ReviewerUsername, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.Username : string.Empty))
            .ForMember(dest => dest.ReviewerAvatarUrl, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.Avatar : null))
            .ForMember(dest => dest.RevieweeId, opt => opt.MapFrom(src => src.RevieweeId.ToString()))
            .ForMember(dest => dest.RevieweeUsername, opt => opt.MapFrom(src => src.Reviewee != null ? src.Reviewee.Username : string.Empty))
            .ForMember(dest => dest.RevieweeAvatarUrl, opt => opt.MapFrom(src => src.Reviewee != null ? src.Reviewee.Avatar : null));

        CreateMap<CreateSwapReviewRequest, SwapReview>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.SwapRequestId))
            .ForMember(dest => dest.ReviewerId, opt => opt.Ignore())
            .ForMember(dest => dest.RevieweeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Request, opt => opt.Ignore())
            .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
            .ForMember(dest => dest.Reviewee, opt => opt.Ignore());

        CreateMap<UpdateSwapReviewRequest, SwapReview>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// 配置作品展示相关映射
    /// </summary>
    private void CreateProjectShowcaseMaps()
    {
        CreateMap<ProjectShowcase, ProjectShowcaseResponse>()
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Photos) ? new List<string>() : src.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Tags) ? new List<string>() : src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty))
            .ForMember(dest => dest.AuthorAvatarUrl, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : null))
            .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.LikeCount))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.ViewCount))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.FavoriteCount))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

        CreateMap<CreateProjectShowcaseRequest, ProjectShowcase>()
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.ImageUrls != null && src.ImageUrls.Count > 0 ? string.Join(',', src.ImageUrls) : null))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null && src.Tags.Count > 0 ? string.Join(',', src.Tags) : null))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.UsedMaterials, opt => opt.Ignore());

        CreateMap<UpdateProjectShowcaseRequest, ProjectShowcase>()
            .ForMember(dest => dest.Photos, opt => opt.Condition(src => src.ImageUrls != null))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.ImageUrls != null && src.ImageUrls.Count > 0 ? string.Join(',', src.ImageUrls) : null))
            .ForMember(dest => dest.Tags, opt => opt.Condition(src => src.Tags != null))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null && src.Tags.Count > 0 ? string.Join(',', src.Tags) : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.Entities;
using CraftSwap.Repositories;

namespace CraftSwap.Services;

public interface IMaterialSyncService
{
    Task<ApiResponse> ValidateMaterialsForSwapAsync(SwapRequest swapRequest);

    Task SyncMaterialsToSwappedAsync(SwapRequest swapRequest);
}

public class MaterialSyncService : IMaterialSyncService
{
    private readonly IMaterialRepository _materialRepository;

    public MaterialSyncService(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<ApiResponse> ValidateMaterialsForSwapAsync(SwapRequest swapRequest)
    {
        var offeredMaterial = await _materialRepository.GetByIdAsync(swapRequest.OfferedMaterialId);
        if (offeredMaterial == null)
        {
            return ApiResponse.Fail("发起方提供的材料不存在", 404);
        }

        if (offeredMaterial.Status != AppConstants.MaterialStatus.Available)
        {
            return ApiResponse.Fail($"发起方提供的材料状态异常（当前: {GetMaterialStatusDisplayName(offeredMaterial.Status)}），无法完成交换");
        }

        var requestedMaterial = await _materialRepository.GetByIdAsync(swapRequest.RequestedMaterialId);
        if (requestedMaterial == null)
        {
            return ApiResponse.Fail("接收方提供的材料不存在", 404);
        }

        if (requestedMaterial.Status != AppConstants.MaterialStatus.Available)
        {
            return ApiResponse.Fail($"接收方提供的材料状态异常（当前: {GetMaterialStatusDisplayName(requestedMaterial.Status)}），无法完成交换");
        }

        return ApiResponse.Success(null);
    }

    public async Task SyncMaterialsToSwappedAsync(SwapRequest swapRequest)
    {
        var offeredMaterial = await _materialRepository.GetByIdAsync(swapRequest.OfferedMaterialId);
        if (offeredMaterial != null)
        {
            offeredMaterial.Status = AppConstants.MaterialStatus.Swapped;
            offeredMaterial.UpdatedAt = DateTime.UtcNow;
            await _materialRepository.UpdateAsync(offeredMaterial);
        }

        var requestedMaterial = await _materialRepository.GetByIdAsync(swapRequest.RequestedMaterialId);
        if (requestedMaterial != null)
        {
            requestedMaterial.Status = AppConstants.MaterialStatus.Swapped;
            requestedMaterial.UpdatedAt = DateTime.UtcNow;
            await _materialRepository.UpdateAsync(requestedMaterial);
        }
    }

    private static string GetMaterialStatusDisplayName(string status)
    {
        return status switch
        {
            AppConstants.MaterialStatus.Available => "可用",
            AppConstants.MaterialStatus.Swapped => "已交换",
            AppConstants.MaterialStatus.Offline => "已下架",
            AppConstants.MaterialStatus.Pending => "审核中",
            _ => status
        };
    }
}

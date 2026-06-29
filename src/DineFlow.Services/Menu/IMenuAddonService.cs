using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IMenuAddonService
{
    List<MenuAddonGroup> GetAllGroups();
    List<MenuAddonOption> GetAllOptions();
    List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId);
    MenuAddonGroup CreateGroup(MenuAddonGroup group, UserRole role);
    void UpdateGroup(MenuAddonGroup group, UserRole role);
    void HideGroup(int menuAddonGroupId, UserRole role);
    MenuAddonOption CreateOption(MenuAddonOption option, UserRole role);
    void UpdateOption(MenuAddonOption option, UserRole role);
    void HideOption(int menuAddonOptionId, UserRole role);
    MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping, UserRole role);
    void HideGroupFromMenuItem(int menuItemId, int menuAddonGroupId, UserRole role);
    AddonGroupOption AddOptionToGroup(AddonGroupOption mapping, UserRole role);
    void UpdateGroupOption(AddonGroupOption mapping, UserRole role);
    void HideOptionFromGroup(int addonGroupOptionId, UserRole role);
    bool ValidateAddonsForOrder(List<OrderItemRequestDto> items);
    Task<bool> ValidateAddonsForOrderAsync(List<OrderItemRequestDto> items);
    List<AddonSnapshotDto> GetAddonSnapshotsForOrder(List<OrderItemRequestDto> items);
    Task<List<AddonSnapshotDto>> GetAddonSnapshotsForOrderAsync(List<OrderItemRequestDto> items);
}

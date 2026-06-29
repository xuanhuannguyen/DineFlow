using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IChoiceService
{
    List<ChoiceGroup> GetGroups();
    List<MenuItemChoiceGroup> GetMappings(int menuItemId);
    ChoiceGroup CreateGroup(ChoiceGroup group, UserRole role);
    void UpdateGroup(ChoiceGroup group, UserRole role);
    ChoiceItem CreateChoiceItem(ChoiceItem item, UserRole role);
    void UpdateChoiceItem(ChoiceItem item, UserRole role);
    void DeleteChoiceItem(int choiceItemId, UserRole role);
    void DeleteGroup(int choiceGroupId, UserRole role);
    MenuItemChoiceGroup AssignGroup(MenuItemChoiceGroup mapping, UserRole role);
    void RemoveGroup(int menuItemId, int choiceGroupId, UserRole role);
}

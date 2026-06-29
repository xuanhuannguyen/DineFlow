using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IChoiceRepository
{
    List<ChoiceGroup> GetGroups();
    ChoiceGroup? GetGroup(int choiceGroupId);
    ChoiceItem? GetChoiceItem(int choiceItemId);
    List<MenuItemChoiceGroup> GetMappings(int menuItemId);
    ChoiceGroup AddGroup(ChoiceGroup group);
    ChoiceItem AddChoiceItem(ChoiceItem item);
    MenuItemChoiceGroup UpsertMapping(MenuItemChoiceGroup mapping);
    void UpdateGroup(ChoiceGroup group);
    void UpdateChoiceItem(ChoiceItem item);
    void DeleteChoiceItem(int choiceItemId);
    void DeleteGroup(int choiceGroupId);
    void RemoveMapping(int menuItemId, int choiceGroupId);
}

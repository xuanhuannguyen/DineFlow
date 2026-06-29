namespace DineFlow.BusinessObjects.Menu;

public class CustomerMenuDto
{
    public List<Category> Categories { get; set; } = new();
    public List<MenuItem> Items { get; set; } = new();
}

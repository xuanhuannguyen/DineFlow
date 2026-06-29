namespace DineFlow.BusinessObjects.Menu;

public class ChannelMenuDto
{
    public string ChannelCode { get; set; } = string.Empty;
    public List<Category> Categories { get; set; } = new();
    public List<ChannelMenuItemDto> Items { get; set; } = new();
}

public class ChannelMenuItemDto
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ChannelExtraPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsAvailable { get; set; }
}

public class ChannelMenuItemDetailDto : ChannelMenuItemDto
{
    public List<ChannelChoiceGroupDto> ChoiceGroups { get; set; } = new();
}

public class ChannelChoiceGroupDto
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; }
    public int DisplayOrder { get; set; }
    public List<ChannelChoiceItemDto> ChoiceItems { get; set; } = new();
}

public class ChannelChoiceItemDto
{
    public int ChoiceItemId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public decimal ChannelExtraPrice { get; set; }
    public decimal FinalExtraPrice { get; set; }
    public int? LinkedMenuItemId { get; set; }
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
}

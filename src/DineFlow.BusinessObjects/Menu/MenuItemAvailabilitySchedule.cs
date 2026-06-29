using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItemAvailabilitySchedule : BaseEntity
{
    public int MenuItemAvailabilityScheduleId { get; set; }
    public int MenuItemId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }

    public MenuItem? MenuItem { get; set; }
}

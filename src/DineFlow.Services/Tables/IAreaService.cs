using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables
{
    public interface IAreaService
    {
        List<AreaDto> GetAllAreas();
        List<AreaDto> GetActiveAreas();
        AreaDto GetAreaById(int areaId);
        AreaDto CreateArea(CreateAreaRequest request);
        AreaDto UpdateArea(int areaId, UpdateAreaRequest request);
        void DeactivateArea(int areaId);
        void ReactivateArea(int areaId);
    }
}

namespace DineFlow.BusinessObjects.Tables
{
    public class CreateTableRequest
    {
        public string TableName { get; set; } = null!;
        public int AreaId { get; set; }
    }
}

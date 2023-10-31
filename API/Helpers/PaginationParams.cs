namespace API.Helpers;

public class PaginationParams
{
    // This class is used to define the parameters to get from the URL query.
    // An example is: /api/users?pageNumber=1&Size=10
    
    // Private members.
    private const int MaxPageSize = 50;
    private int pageSize = 12;
    
    // Public members
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => this.pageSize;
        set => this.pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}

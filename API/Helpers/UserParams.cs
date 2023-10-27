using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace API.Helpers;

public class UserParams
{
    // This class is used to define the parameters to get from the URL query.
    // An example is: /api/users?pageNumber=1&Size=10
    
    // Private members.
    private const int MaxPageSize = 50;
    private int pageSize = 10;
    
    // Public members
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => this.pageSize;
        set => this.pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string CurrentUserName { get; set; }
    public string Gender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}

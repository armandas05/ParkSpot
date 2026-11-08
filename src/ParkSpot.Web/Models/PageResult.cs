using System.Collections.Generic;
namespace ParkSpot.Web.Models;

public class PageResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; } = 30;
}

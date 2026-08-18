using Ube.Application.Common.Models;
using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Admin.VendorApplications;

public class VendorApplicationsRequest : QueryOptions
{
    public VendorApplicationSortBy? SortOptions { get; set; }
}
using System;
using System.Collections.Generic;

namespace Test_project.Entity;

public partial class PermissionAssignTbl
{
    public int PermissionAssignId { get; set; }

    public int PermissionId { get; set; }

    public int RoleId { get; set; }
}

using System;
using System.Collections.Generic;

namespace Test_project.Entity;

public partial class UserInfoTbl
{
    public int UserInfoId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Phone { get; set; }
}

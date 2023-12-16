using System;
using System.Collections.Generic;

namespace Test_project.Entity;

public partial class UserLogInInfoTbl
{
    public int UserId { get; set; }

    public int UserInfoId { get; set; }

    public string LoginId { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public string TokenSecretKey { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Test_project.SqliteEntity
{
    public class UserLogInfo
    {
        [Key]
        public Guid SessionID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string? Token { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime SessionTime { get; set; }
        public string? Permissions { get; set; }      
    }
}

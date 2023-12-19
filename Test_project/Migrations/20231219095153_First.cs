using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test_project.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLogInfo",
                columns: table => new
                {
                    SessionID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleID = table.Column<int>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    LoginTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SessionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogInfo", x => x.SessionID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLogInfo");
        }
    }
}

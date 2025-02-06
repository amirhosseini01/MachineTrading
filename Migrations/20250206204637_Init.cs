using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MachineTrading.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Link = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParentLink = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Time = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CommentCount = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ReShareCount = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    LikeCount = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Link);
                });

            migrationBuilder.CreateTable(
                name: "Selectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selectors", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Selectors");
        }
    }
}

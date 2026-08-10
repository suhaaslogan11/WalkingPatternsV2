using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalkingPatterns.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectVersionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    projectDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    GrandTotal = table.Column<double>(type: "float", nullable: false),
                    DiscountAmount = table.Column<double>(type: "float", nullable: false),
                    DiscountedTotal = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersionDetails_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionDetails_ClientId",
                table: "ProjectVersionDetails",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectVersionDetails");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");
        }
    }
}

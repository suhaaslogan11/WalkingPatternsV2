using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalkingPatterns.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDetailsAndOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accessories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsAmounts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialTotal = table.Column<double>(type: "float", nullable: false),
                    AccessoriesTotal = table.Column<double>(type: "float", nullable: false),
                    AdditionalItemsTotal = table.Column<double>(type: "float", nullable: false),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrandTotal = table.Column<double>(type: "float", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilityNameOld = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectVersionDetailsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_ProjectVersionDetails_ProjectVersionDetailsId",
                        column: x => x.ProjectVersionDetailsId,
                        principalTable: "ProjectVersionDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Woodwork = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accessories = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Services = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDetails_ProjectVersionDetails_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ProjectVersionDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProjectVersionDetailsId",
                table: "OrderDetails",
                column: "ProjectVersionDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetails_ProjectId",
                table: "ProjectDetails",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "ProjectDetails");
        }
    }
}

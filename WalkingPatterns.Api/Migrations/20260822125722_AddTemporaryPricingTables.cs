using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalkingPatterns.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryPricingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BedromPriceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityNameOld = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsAmounts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialTotal = table.Column<double>(type: "float", nullable: false),
                    AdditionalItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsTotal = table.Column<double>(type: "float", nullable: false),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedromPriceDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HDSPriceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityNameOld = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsAmounts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialTotal = table.Column<double>(type: "float", nullable: false),
                    AdditionalItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsTotal = table.Column<double>(type: "float", nullable: false),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HDSPriceDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KitchenPriceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accessories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityNameOld = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsAmounts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialTotal = table.Column<double>(type: "float", nullable: true),
                    AdditionalItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessoriesTotal = table.Column<double>(type: "float", nullable: true),
                    AdditionalItemsTotal = table.Column<double>(type: "float", nullable: true),
                    TotalPrice = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenPriceDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtherWoodworkPriceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilityNameOld = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsAmounts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialTotal = table.Column<double>(type: "float", nullable: false),
                    AdditionalItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalItemsTotal = table.Column<double>(type: "float", nullable: false),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherWoodworkPriceDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BedromPriceDetails");

            migrationBuilder.DropTable(
                name: "HDSPriceDetails");

            migrationBuilder.DropTable(
                name: "KitchenPriceDetails");

            migrationBuilder.DropTable(
                name: "OtherWoodworkPriceDetails");
        }
    }
}

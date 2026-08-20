using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DulceAtardecer.Migrations
{
    /// <inheritdoc />
    public partial class AddVentaItemExtraPrecio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "VentaItemExtras",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Precio",
                table: "VentaItemExtras");
        }
    }
}

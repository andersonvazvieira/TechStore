using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStore.API.Migrations;

public partial class AddStockProtectionTriggers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TRIGGER TR_Products_PreventNegativeStock_Insert
            BEFORE INSERT ON Products
            FOR EACH ROW
            WHEN NEW.Quantity < 0
            BEGIN
                SELECT RAISE(
                    ABORT,
                    'O estoque nao pode ser negativo.'
                );
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER TR_Products_PreventNegativeStock_Update
            BEFORE UPDATE OF Quantity ON Products
            FOR EACH ROW
            WHEN NEW.Quantity < 0
            BEGIN
                SELECT RAISE(
                    ABORT,
                    'O estoque nao pode ser negativo.'
                );
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS
                TR_Products_PreventNegativeStock_Insert;
            """);

        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS
                TR_Products_PreventNegativeStock_Update;
            """);
    }
}
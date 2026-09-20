using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ube.Infrastructure.Persistence;

namespace Ube.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260912000100_AddVehicleTypeToCarRentalDetails")]
public class AddVehicleTypeToCarRentalDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.AddColumn<string>(
            name: "VehicleType", table: "CarRentalListingDetails", type: "nvarchar(max)", nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn(name: "VehicleType", table: "CarRentalListingDetails");
}

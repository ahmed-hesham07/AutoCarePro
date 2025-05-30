using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCarePro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "CarOwner"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetExpiry = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Make = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    VIN = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentMileage = table.Column<int>(type: "INTEGER", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", nullable: false),
                    FuelType = table.Column<string>(type: "TEXT", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    EngineType = table.Column<string>(type: "TEXT", nullable: true),
                    TransmissionType = table.Column<string>(type: "TEXT", nullable: true),
                    FuelEfficiency = table.Column<double>(type: "REAL", nullable: true),
                    LastOilChange = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OilChangeMileage = table.Column<double>(type: "REAL", nullable: true),
                    LastTireRotation = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TireRotationMileage = table.Column<double>(type: "REAL", nullable: true),
                    LastBrakeService = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BrakeServiceMileage = table.Column<double>(type: "REAL", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    NumberOfDoors = table.Column<int>(type: "INTEGER", nullable: true),
                    BodyStyle = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Component = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    RecommendedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecommendedMileage = table.Column<double>(type: "REAL", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecommendations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaintenanceType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MileageAtMaintenance = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ServiceProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    HasDiagnosisRecommendations = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiagnosedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagnosisRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaintenanceRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiagnosedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Component = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    RecommendedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    RecommendedAction = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosisRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosisRecommendations_MaintenanceRecords_MaintenanceRecordId",
                        column: x => x.MaintenanceRecordId,
                        principalTable: "MaintenanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiagnosisRecommendations_Users_DiagnosedByUserId",
                        column: x => x.DiagnosedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisRecommendations_DiagnosedByUserId",
                table: "DiagnosisRecommendations",
                column: "DiagnosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisRecommendations_MaintenanceRecordId",
                table: "DiagnosisRecommendations",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecommendations_VehicleId",
                table: "MaintenanceRecommendations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_VehicleId",
                table: "MaintenanceRecords",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagnosisRecommendations");

            migrationBuilder.DropTable(
                name: "MaintenanceRecommendations");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

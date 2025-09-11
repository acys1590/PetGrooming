using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetGrooming.Migrations
{
    public partial class SeedServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Turn ON identity insert for explicit Id values
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.Services ON;");

            // Upsert 1..3 (update if exists, insert if missing)
            migrationBuilder.Sql(@"
MERGE dbo.Services AS t
USING (VALUES
    (1, N'Basic Grooming', NULL, 40.00, 60, 1),
    (2, N'Full Grooming',  NULL, 80.00, 90, 1),
    (3, N'Bath Only',      NULL, 25.00, 30, 1)
) AS s(Id, Name, Description, Price, DurationMinutes, IsActive)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE
    SET Name=s.Name, Description=s.Description, Price=s.Price, DurationMinutes=s.DurationMinutes, IsActive=s.IsActive
WHEN NOT MATCHED THEN INSERT (Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (s.Id, s.Name, s.Description, s.Price, s.DurationMinutes, s.IsActive);
");

            // Insert the rest 4..13 only if missing
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=4)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (4, N'Nail Trim', NULL, 15.00, 15, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=5)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (5, N'Vet Consultation', NULL, 60.00, 30, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=6)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (6, N'General Health Check', NULL, 80.00, 45, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=7)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (7, N'Vaccination', NULL, 70.00, 20, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=8)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (8, N'Flea/Tick Treatment', NULL, 50.00, 30, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=9)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (9, N'Minor Wound Care', NULL, 90.00, 45, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=10)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (10, N'Blood Test (Basic)', NULL, 120.00, 45, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=11)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (11, N'Spay/Neuter', N'Base price; final amount may vary', 250.00, 120, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=12)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (12, N'Dental Care', NULL, 50.00, 30, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Services WHERE Id=13)
    INSERT dbo.Services(Id, Name, Description, Price, DurationMinutes, IsActive)
    VALUES (13, N'Other / Custom Request', N'Describe in Special Notes', 0.00, 60, 1);
");

            // Turn OFF identity insert
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.Services OFF;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove only 4..13
            migrationBuilder.Sql(@"DELETE FROM dbo.Services WHERE Id BETWEEN 4 AND 13;");
            // (Optionally revert 1..3 to previous values)
        }
    }
}

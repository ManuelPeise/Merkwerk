using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    SubjectId = table.Column<long>(type: "bigint", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    ContentSource = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    LatestVersion = table.Column<int>(type: "int", nullable: false),
                    HasUnpublishedChanges = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsArchived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exercises_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExerciseVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Content = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseVersions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseVersions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "json", nullable: false),
                    Solution = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_OrganizationId",
                table: "Exercises",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SubjectId",
                table: "Exercises",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseVersions_ExerciseId_Number",
                table: "ExerciseVersions",
                columns: new[] { "ExerciseId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseVersions_OrganizationId",
                table: "ExerciseVersions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExerciseId_Position",
                table: "Questions",
                columns: new[] { "ExerciseId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_OrganizationId",
                table: "Questions",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseVersions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}

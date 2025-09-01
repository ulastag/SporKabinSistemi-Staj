using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace user_panel.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutPalnImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkoutImage",
                table: "WorkoutPlans",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkoutImage",
                table: "WorkoutPlans");
        }
    }
}

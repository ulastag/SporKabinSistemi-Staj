using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace user_panel.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutExerciseadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanID",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_WorkoutPlanID",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "WorkoutPlanID",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "WorkOutPlanId",
                table: "WorkoutPlans",
                newName: "WorkoutPlanId");

            migrationBuilder.CreateTable(
                name: "WorkoutExercises",
                columns: table => new
                {
                    WorkoutPlanId = table.Column<int>(type: "int", nullable: false),
                    ExerciseID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExercises", x => new { x.WorkoutPlanId, x.ExerciseID });
                    table.ForeignKey(
                        name: "FK_WorkoutExercises_Exercises_ExerciseID",
                        column: x => x.ExerciseID,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "WorkoutPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_ExerciseID",
                table: "WorkoutExercises",
                column: "ExerciseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutExercises");

            migrationBuilder.RenameColumn(
                name: "WorkoutPlanId",
                table: "WorkoutPlans",
                newName: "WorkOutPlanId");

            migrationBuilder.AddColumn<int>(
                name: "WorkoutPlanID",
                table: "Exercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_WorkoutPlanID",
                table: "Exercises",
                column: "WorkoutPlanID");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanID",
                table: "Exercises",
                column: "WorkoutPlanID",
                principalTable: "WorkoutPlans",
                principalColumn: "WorkOutPlanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

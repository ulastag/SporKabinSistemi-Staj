using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace user_panel.Migrations
{
    /// <inheritdoc />
    public partial class exerciseAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "cab",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");


            migrationBuilder.CreateTable(
                name: "WorkoutPlans",
                columns: table => new
                {
                    WorkOutPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkoutPreview = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    WorkoutDetails = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlans", x => x.WorkOutPlanId);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExerciseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExerciseDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkoutPlanID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseID);
                    table.ForeignKey(
                        name: "FK_Exercises_WorkoutPlans_WorkoutPlanID",
                        column: x => x.WorkoutPlanID,
                        principalTable: "WorkoutPlans",
                        principalColumn: "WorkOutPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HowToDos",
                columns: table => new
                {
                    HowToDoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HowToDoStep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HowToDos", x => x.HowToDoId);
                    table.ForeignKey(
                        name: "FK_HowToDos_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_WorkoutPlanID",
                table: "Exercises",
                column: "WorkoutPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_HowToDos_ExerciseId",
                table: "HowToDos",
                column: "ExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HowToDos");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "cab");

          
        }
    }
}

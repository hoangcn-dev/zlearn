using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    public partial class AllowDeleteQuizWithEndedExams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam");

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam");

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zlearn.V2.Infas.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIdToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_ExamParticipant_ExamId",
                table: "ExamParticipant");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeadLetter",
                table: "OutboxEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "OutboxEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "OutboxEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizTag_QuizzesId_TagsId",
                table: "QuizTag",
                columns: new[] { "QuizzesId", "TagsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipant_ExamId_UserId",
                table: "ExamParticipant",
                columns: new[] { "ExamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipant_UserId",
                table: "ExamParticipant",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamParticipant_Users_UserId",
                table: "ExamParticipant",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamParticipant_Users_UserId",
                table: "ExamParticipant");

            migrationBuilder.DropIndex(
                name: "IX_QuizTag_QuizzesId_TagsId",
                table: "QuizTag");

            migrationBuilder.DropIndex(
                name: "IX_ExamParticipant_ExamId_UserId",
                table: "ExamParticipant");

            migrationBuilder.DropIndex(
                name: "IX_ExamParticipant_UserId",
                table: "ExamParticipant");

            migrationBuilder.DropColumn(
                name: "IsDeadLetter",
                table: "OutboxEvents");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "OutboxEvents");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "OutboxEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipant_ExamId",
                table: "ExamParticipant",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Quizzes_QuizId",
                table: "Exam",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

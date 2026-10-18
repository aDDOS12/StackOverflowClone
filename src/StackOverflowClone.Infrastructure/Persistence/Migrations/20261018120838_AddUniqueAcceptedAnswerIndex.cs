using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StackOverflowClone.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueAcceptedAnswerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId_AcceptedUnique",
                table: "Answers",
                column: "QuestionId",
                unique: true,
                filter: "[IsAccepted] = 1 AND [DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionId_AcceptedUnique",
                table: "Answers");
        }
    }
}

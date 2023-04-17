using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XMLEdition.Migrations
{
    /// <inheritdoc />
    public partial class subject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<int>(
                name: "CourseSubjectId",
                table: "Courses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

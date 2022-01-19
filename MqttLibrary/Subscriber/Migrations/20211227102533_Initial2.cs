using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MqttSubscriber.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageMqtt",
                table: "MessageMqtt");

            migrationBuilder.RenameTable(
                name: "MessageMqtt",
                newName: "Messages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                table: "Messages",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                table: "Messages");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "MessageMqtt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageMqtt",
                table: "MessageMqtt",
                column: "Id");
        }
    }
}

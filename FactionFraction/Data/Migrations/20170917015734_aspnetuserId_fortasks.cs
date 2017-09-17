using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FactionFraction.Data.Migrations
{
    public partial class aspnetuserId_fortasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "Email",
            //    table: "GroupMembers",
            //    type: "nvarchar(max)",
            //    nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "AssignedTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "AssignedTasks");
        }
    }
}

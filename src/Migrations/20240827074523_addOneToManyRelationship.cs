﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class addOneToManyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TodoItems",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId",
                table: "TodoItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_AspNetUsers_UserId",
                table: "TodoItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_AspNetUsers_UserId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_UserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TodoItems");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietAI.Api.Migrations
{
    public partial class AddDietPlanPreferencesAndHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActivityLevel",
                table: "Diets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "Allergies",
                table: "Diets",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Diets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<List<string>>(
                name: "ExcludedIngredients",
                table: "Diets",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AddColumn<int>(
                name: "GoalType",
                table: "Diets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MealsPerDay",
                table: "Diets",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.CreateIndex(
                name: "IX_Diets_UserId_CreatedAtUtc",
                table: "Diets",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Diets_UserId_CreatedAtUtc",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "ActivityLevel",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "ExcludedIngredients",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "GoalType",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "MealsPerDay",
                table: "Diets");
        }
    }
}

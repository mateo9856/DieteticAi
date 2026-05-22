using System;
using System.Collections.Generic;
using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DietAI.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DietAI.Api.Services.AiPlanSender.Models.Diets", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ActivityLevel")
                        .HasColumnType("integer");

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Allergies")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<decimal>("CaloricValue")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DietType")
                        .HasColumnType("integer");

                    b.Property<List<string>>("ExcludedIngredients")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<decimal>("ForHeight")
                        .HasColumnType("numeric");

                    b.Property<int>("ForSex")
                        .HasColumnType("integer");

                    b.Property<decimal>("ForWeight")
                        .HasColumnType("numeric");

                    b.Property<int>("GoalType")
                        .HasColumnType("integer");

                    b.Property<int>("MealsPerDay")
                        .HasColumnType("integer");

                    b.Property<string>("DietName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("UserId", "CreatedAtUtc");

                    b.ToTable("Diets");
                });
#pragma warning restore 612, 618
        }
    }
}

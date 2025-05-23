﻿// <auto-generated />
using System.Collections.Generic;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeYasnoTelegramBot.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DeYasnoTelegramBot.Domain.Entities.Subscriber", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("BrowserSessionId")
                        .HasColumnType("text");

                    b.Property<int>("InputStep")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDisableNotification")
                        .HasColumnType("boolean");

                    b.Property<List<OutageScheduleDayDto>>("OutageSchedules")
                        .HasColumnType("jsonb");

                    b.Property<string>("UserCity")
                        .HasColumnType("text");

                    b.Property<string>("UserHouseNumber")
                        .HasColumnType("text");

                    b.Property<string>("UserRegion")
                        .HasColumnType("text");

                    b.Property<string>("UserStreet")
                        .HasColumnType("text");

                    b.HasKey("ChatId");

                    b.ToTable("Subscribers");
                });
#pragma warning restore 612, 618
        }
    }
}

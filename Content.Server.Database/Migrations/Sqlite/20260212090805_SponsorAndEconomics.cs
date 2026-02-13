using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class SponsorAndEconomics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "additional_sponsor_data",
                columns: table => new
                {
                    additional_sponsor_data_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tier_id = table.Column<string>(type: "TEXT", nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additional_sponsor_data", x => x.additional_sponsor_data_id);
                });

            migrationBuilder.CreateTable(
                name: "patron_profile_item",
                columns: table => new
                {
                    patron_profile_item_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<int>(type: "INTEGER", nullable: false),
                    item_prototype_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patron_profile_item", x => x.patron_profile_item_id);
                    table.ForeignKey(
                        name: "FK_patron_profile_item_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patron_profile_pet",
                columns: table => new
                {
                    patron_profile_pet_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<int>(type: "INTEGER", nullable: false),
                    pet_id = table.Column<string>(type: "TEXT", nullable: false),
                    pet_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patron_profile_pet", x => x.patron_profile_pet_id);
                    table.ForeignKey(
                        name: "FK_patron_profile_pet_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profile_economics",
                columns: table => new
                {
                    profile_economics_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<int>(type: "INTEGER", nullable: false),
                    balance = table.Column<int>(type: "INTEGER", nullable: false),
                    transactions = table.Column<byte[]>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_economics", x => x.profile_economics_id);
                    table.ForeignKey(
                        name: "FK_profile_economics_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_additional_sponsor_data_user_id",
                table: "additional_sponsor_data",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_additional_sponsor_data_user_id_tier_id",
                table: "additional_sponsor_data",
                columns: new[] { "user_id", "tier_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patron_profile_item_profile_id_item_prototype_id",
                table: "patron_profile_item",
                columns: new[] { "profile_id", "item_prototype_id" });

            migrationBuilder.CreateIndex(
                name: "IX_patron_profile_pet_profile_id",
                table: "patron_profile_pet",
                column: "profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profile_economics_profile_id",
                table: "profile_economics",
                column: "profile_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additional_sponsor_data");

            migrationBuilder.DropTable(
                name: "patron_profile_item");

            migrationBuilder.DropTable(
                name: "patron_profile_pet");

            migrationBuilder.DropTable(
                name: "profile_economics");
        }
    }
}

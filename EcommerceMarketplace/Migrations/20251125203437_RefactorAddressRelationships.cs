using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceMarketplace.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAddressRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== PASSO 1: Remover índice de CustomerId em Addresses (se existir) =====
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AspNetUsers_CustomerId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses");

            // ===== PASSO 2: Remover colunas específicas de cliente da tabela Addresses =====
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Addresses");

            // ===== PASSO 3: Adicionar novos campos à tabela Addresses =====
            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Addresses",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "Addresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Addresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Addresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            // ===== PASSO 4: Adicionar AddressId à tabela Stores (nullable temporariamente) =====
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Stores",
                type: "integer",
                nullable: true);

            // ===== PASSO 5: Migrar dados existentes - Criar endereços para lojas existentes =====
            // Cria uma tabela temporária para mapear Store -> Address
            migrationBuilder.Sql(@"
                CREATE TEMP TABLE temp_store_addresses AS
                SELECT
                    ""Id"" as store_id,
                    COALESCE(""Address"", 'Não informado') as street,
                    'S/N' as number,
                    'Centro' as neighborhood,
                    COALESCE(""City"", 'Não informado') as city,
                    COALESCE(""State"", 'XX') as state,
                    COALESCE(""ZipCode"", '00000-000') as zipcode
                FROM ""Stores"";
            ");

            // Insere endereços únicos na tabela Addresses
            migrationBuilder.Sql(@"
                INSERT INTO ""Addresses"" (""Street"", ""Number"", ""Neighborhood"", ""City"", ""State"", ""ZipCode"", ""CreatedAt"", ""UpdatedAt"")
                SELECT DISTINCT
                    street,
                    number,
                    neighborhood,
                    city,
                    state,
                    zipcode,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                FROM temp_store_addresses;
            ");

            // ===== PASSO 6: Atualizar AddressId nas lojas com base nos endereços criados =====
            migrationBuilder.Sql(@"
                UPDATE ""Stores"" s
                SET ""AddressId"" = a.""Id""
                FROM ""Addresses"" a, temp_store_addresses t
                WHERE s.""Id"" = t.store_id
                  AND a.""Street"" = t.street
                  AND a.""City"" = t.city
                  AND a.""State"" = t.state
                  AND a.""ZipCode"" = t.zipcode;
            ");

            // Remove a tabela temporária
            migrationBuilder.Sql(@"DROP TABLE temp_store_addresses;");

            // ===== PASSO 7: Tornar AddressId obrigatório =====
            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "Stores",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // ===== PASSO 8: Remover campos de endereço direto da tabela Stores =====
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Stores");

            // ===== PASSO 9: Criar tabela CustomerAddresses (junction table) =====
            migrationBuilder.CreateTable(
                name: "CustomerAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddresses_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerAddresses_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ===== PASSO 10: Criar índices para CustomerAddresses =====
            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_AddressId",
                table: "CustomerAddresses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_CustomerId",
                table: "CustomerAddresses",
                column: "CustomerId");

            // ===== PASSO 11: Criar foreign key de Store para Address =====
            migrationBuilder.CreateIndex(
                name: "IX_Stores_AddressId",
                table: "Stores",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Addresses_AddressId",
                table: "Stores",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ===== REVERTER: Remover foreign key de Store para Address =====
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Addresses_AddressId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_AddressId",
                table: "Stores");

            // ===== REVERTER: Deletar tabela CustomerAddresses =====
            migrationBuilder.DropTable(
                name: "CustomerAddresses");

            // ===== REVERTER: Remover AddressId da tabela Stores =====
            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Stores");

            // ===== REVERTER: Adicionar campos de endereço direto à tabela Stores =====
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Stores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Stores",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Stores",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            // ===== REVERTER: Remover novos campos da tabela Addresses =====
            migrationBuilder.DropColumn(
                name: "Street",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Addresses");

            // ===== REVERTER: Adicionar colunas específicas de cliente à tabela Addresses =====
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "Addresses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Addresses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Addresses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Addresses",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Addresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // ===== REVERTER: Criar índice e foreign key de Addresses para AspNetUsers =====
            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AspNetUsers_CustomerId",
                table: "Addresses",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webhooks.Api.Data.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "webhooks");

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            schema: "webhooks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                CreateOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "WebhookDeliveryAttempts",
            schema: "webhooks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WebhookSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false),
                StatusCode = table.Column<int>(type: "integer", nullable: true),
                ErrorMessage = table.Column<string>(type: "text", nullable: true),
                IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebhookDeliveryAttempts", x => x.Id);
                table.ForeignKey(
                    name: "FK_WebhookDeliveryAttempts_Subscriptions_WebhookSubscriptionId",
                    column: x => x.WebhookSubscriptionId,
                    principalSchema: "webhooks",
                    principalTable: "Subscriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveryAttempts_WebhookSubscriptionId",
            schema: "webhooks",
            table: "WebhookDeliveryAttempts",
            column: "WebhookSubscriptionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "WebhookDeliveryAttempts",
            schema: "webhooks");

        migrationBuilder.DropTable(
            name: "Subscriptions",
            schema: "webhooks");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ActiveDirectory_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    correlation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    log_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    user_context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    request_data = table.Column<string>(type: "jsonb", nullable: true),
                    response_data = table.Column<string>(type: "jsonb", nullable: true),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    duration_ms = table.Column<double>(type: "double precision", nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    exception_details = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    http_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ad_operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ad_target = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ad_success = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action_timestamp",
                table: "audit_logs",
                columns: new[] { "action", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_id",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_timestamp",
                table: "audit_logs",
                columns: new[] { "correlation_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_log_type",
                table: "audit_logs",
                column: "log_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_resource",
                table: "audit_logs",
                column: "resource");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_status_code",
                table: "audit_logs",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp_log_type",
                table: "audit_logs",
                columns: new[] { "timestamp", "log_type" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_context",
                table: "audit_logs",
                column: "user_context");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}

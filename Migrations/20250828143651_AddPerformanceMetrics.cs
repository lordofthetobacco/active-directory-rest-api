using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ActiveDirectory_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "performance_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    http_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    response_time_ms = table.Column<double>(type: "double precision", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    request_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    response_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    performance_category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_metrics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_action",
                table: "performance_metrics",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_action_timestamp",
                table: "performance_metrics",
                columns: new[] { "action", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_category_timestamp",
                table: "performance_metrics",
                columns: new[] { "performance_category", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_correlation_id",
                table: "performance_metrics",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_created_at",
                table: "performance_metrics",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_endpoint",
                table: "performance_metrics",
                column: "endpoint");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_endpoint_method_timestamp",
                table: "performance_metrics",
                columns: new[] { "endpoint", "http_method", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_endpoint_timestamp",
                table: "performance_metrics",
                columns: new[] { "endpoint", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_http_method",
                table: "performance_metrics",
                column: "http_method");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_performance_category",
                table: "performance_metrics",
                column: "performance_category");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_status_code",
                table: "performance_metrics",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "ix_performance_metrics_timestamp",
                table: "performance_metrics",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "performance_metrics");
        }
    }
}

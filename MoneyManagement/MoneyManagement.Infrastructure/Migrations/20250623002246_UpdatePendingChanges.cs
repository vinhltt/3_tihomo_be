using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedExpenseParticipants_SharedExpenses_SharedExpenseId",
                table: "SharedExpenseParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jars",
                table: "Jars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SharedExpenses",
                table: "SharedExpenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SharedExpenseParticipants",
                table: "SharedExpenseParticipants");

            migrationBuilder.RenameTable(
                name: "Jars",
                newName: "jars");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "budgets");

            migrationBuilder.RenameTable(
                name: "SharedExpenses",
                newName: "shared_expenses");

            migrationBuilder.RenameTable(
                name: "SharedExpenseParticipants",
                newName: "shared_expense_participants");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "jars",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "jars",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "jars",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "jars",
                newName: "balance");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "jars",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "jars",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "jars",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "jars",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "TargetAmount",
                table: "jars",
                newName: "target_amount");

            migrationBuilder.RenameColumn(
                name: "JarType",
                table: "jars",
                newName: "jar_type");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "jars",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "jars",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "IconName",
                table: "jars",
                newName: "icon_name");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "jars",
                newName: "display_order");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "jars",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "jars",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "ColorCode",
                table: "jars",
                newName: "color_code");

            migrationBuilder.RenameColumn(
                name: "AllocationPercentage",
                table: "jars",
                newName: "allocation_percentage");

            migrationBuilder.RenameIndex(
                name: "IX_Jars_Id",
                table: "jars",
                newName: "ix_jars_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "budgets",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Period",
                table: "budgets",
                newName: "period");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "budgets",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "budgets",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "budgets",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "budgets",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "budgets",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "budgets",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "budgets",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "budgets",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "budgets",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "SpentAmount",
                table: "budgets",
                newName: "spent_amount");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "budgets",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "budgets",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "EnableNotifications",
                table: "budgets",
                newName: "enable_notifications");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "budgets",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "budgets",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "BudgetAmount",
                table: "budgets",
                newName: "budget_amount");

            migrationBuilder.RenameColumn(
                name: "AlertThreshold",
                table: "budgets",
                newName: "alert_threshold");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_Id",
                table: "budgets",
                newName: "ix_budgets_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "shared_expenses",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "shared_expenses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "shared_expenses",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "shared_expenses",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "shared_expenses",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shared_expenses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "shared_expenses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "shared_expenses",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "shared_expenses",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "SettledAmount",
                table: "shared_expenses",
                newName: "settled_amount");

            migrationBuilder.RenameColumn(
                name: "ReceiptImageUrl",
                table: "shared_expenses",
                newName: "receipt_image_url");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "shared_expenses",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                table: "shared_expenses",
                newName: "group_name");

            migrationBuilder.RenameColumn(
                name: "ExpenseDate",
                table: "shared_expenses",
                newName: "expense_date");

            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "shared_expenses",
                newName: "currency_code");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "shared_expenses",
                newName: "created_by_user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "shared_expenses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "shared_expenses",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_SharedExpenses_Id",
                table: "shared_expenses",
                newName: "ix_shared_expenses_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "shared_expense_participants",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "shared_expense_participants",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shared_expense_participants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "shared_expense_participants",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "shared_expense_participants",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "shared_expense_participants",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "SharedExpenseId",
                table: "shared_expense_participants",
                newName: "shared_expense_id");

            migrationBuilder.RenameColumn(
                name: "ShareAmount",
                table: "shared_expense_participants",
                newName: "share_amount");

            migrationBuilder.RenameColumn(
                name: "SettledDate",
                table: "shared_expense_participants",
                newName: "settled_date");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "shared_expense_participants",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "shared_expense_participants",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "ParticipantName",
                table: "shared_expense_participants",
                newName: "participant_name");

            migrationBuilder.RenameColumn(
                name: "PaidAmount",
                table: "shared_expense_participants",
                newName: "paid_amount");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "shared_expense_participants",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "shared_expense_participants",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "shared_expense_participants",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_SharedExpenseParticipants_Id",
                table: "shared_expense_participants",
                newName: "ix_shared_expense_participants_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_jars",
                table: "jars",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_budgets",
                table: "budgets",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shared_expenses",
                table: "shared_expenses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shared_expense_participants",
                table: "shared_expense_participants",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_shared_expense_participants_shared_expenses_shared_expense_",
                table: "shared_expense_participants",
                column: "shared_expense_id",
                principalTable: "shared_expenses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_shared_expense_participants_shared_expenses_shared_expense_",
                table: "shared_expense_participants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_jars",
                table: "jars");

            migrationBuilder.DropPrimaryKey(
                name: "pk_budgets",
                table: "budgets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shared_expenses",
                table: "shared_expenses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shared_expense_participants",
                table: "shared_expense_participants");

            migrationBuilder.RenameTable(
                name: "jars",
                newName: "Jars");

            migrationBuilder.RenameTable(
                name: "budgets",
                newName: "Budgets");

            migrationBuilder.RenameTable(
                name: "shared_expenses",
                newName: "SharedExpenses");

            migrationBuilder.RenameTable(
                name: "shared_expense_participants",
                newName: "SharedExpenseParticipants");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Jars",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Jars",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Jars",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "balance",
                table: "Jars",
                newName: "Balance");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Jars",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Jars",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Jars",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Jars",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "target_amount",
                table: "Jars",
                newName: "TargetAmount");

            migrationBuilder.RenameColumn(
                name: "jar_type",
                table: "Jars",
                newName: "JarType");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Jars",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Jars",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "icon_name",
                table: "Jars",
                newName: "IconName");

            migrationBuilder.RenameColumn(
                name: "display_order",
                table: "Jars",
                newName: "DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Jars",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Jars",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "color_code",
                table: "Jars",
                newName: "ColorCode");

            migrationBuilder.RenameColumn(
                name: "allocation_percentage",
                table: "Jars",
                newName: "AllocationPercentage");

            migrationBuilder.RenameIndex(
                name: "ix_jars_id",
                table: "Jars",
                newName: "IX_Jars_Id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Budgets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "period",
                table: "Budgets",
                newName: "Period");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Budgets",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Budgets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Budgets",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "Budgets",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Budgets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Budgets",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Budgets",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Budgets",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Budgets",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "spent_amount",
                table: "Budgets",
                newName: "SpentAmount");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Budgets",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Budgets",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "enable_notifications",
                table: "Budgets",
                newName: "EnableNotifications");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Budgets",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Budgets",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "budget_amount",
                table: "Budgets",
                newName: "BudgetAmount");

            migrationBuilder.RenameColumn(
                name: "alert_threshold",
                table: "Budgets",
                newName: "AlertThreshold");

            migrationBuilder.RenameIndex(
                name: "ix_budgets_id",
                table: "Budgets",
                newName: "IX_Budgets_Id");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "SharedExpenses",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "SharedExpenses",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "SharedExpenses",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "SharedExpenses",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "SharedExpenses",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SharedExpenses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "SharedExpenses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "SharedExpenses",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "SharedExpenses",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "settled_amount",
                table: "SharedExpenses",
                newName: "SettledAmount");

            migrationBuilder.RenameColumn(
                name: "receipt_image_url",
                table: "SharedExpenses",
                newName: "ReceiptImageUrl");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "SharedExpenses",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "group_name",
                table: "SharedExpenses",
                newName: "GroupName");

            migrationBuilder.RenameColumn(
                name: "expense_date",
                table: "SharedExpenses",
                newName: "ExpenseDate");

            migrationBuilder.RenameColumn(
                name: "currency_code",
                table: "SharedExpenses",
                newName: "CurrencyCode");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                table: "SharedExpenses",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SharedExpenses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "SharedExpenses",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_shared_expenses_id",
                table: "SharedExpenses",
                newName: "IX_SharedExpenses_Id");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "SharedExpenseParticipants",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "SharedExpenseParticipants",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SharedExpenseParticipants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SharedExpenseParticipants",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "SharedExpenseParticipants",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "SharedExpenseParticipants",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "shared_expense_id",
                table: "SharedExpenseParticipants",
                newName: "SharedExpenseId");

            migrationBuilder.RenameColumn(
                name: "share_amount",
                table: "SharedExpenseParticipants",
                newName: "ShareAmount");

            migrationBuilder.RenameColumn(
                name: "settled_date",
                table: "SharedExpenseParticipants",
                newName: "SettledDate");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "SharedExpenseParticipants",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "SharedExpenseParticipants",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "participant_name",
                table: "SharedExpenseParticipants",
                newName: "ParticipantName");

            migrationBuilder.RenameColumn(
                name: "paid_amount",
                table: "SharedExpenseParticipants",
                newName: "PaidAmount");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "SharedExpenseParticipants",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SharedExpenseParticipants",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "SharedExpenseParticipants",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_shared_expense_participants_id",
                table: "SharedExpenseParticipants",
                newName: "IX_SharedExpenseParticipants_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jars",
                table: "Jars",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SharedExpenses",
                table: "SharedExpenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SharedExpenseParticipants",
                table: "SharedExpenseParticipants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedExpenseParticipants_SharedExpenses_SharedExpenseId",
                table: "SharedExpenseParticipants",
                column: "SharedExpenseId",
                principalTable: "SharedExpenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

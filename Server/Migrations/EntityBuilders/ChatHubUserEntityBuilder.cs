using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubUserEntityBuilder : AuditableBaseEntityBuilder<ChatHubUserEntityBuilder>
    {

        private const string _entityTableName = "ChatHubUser";
        private readonly PrimaryKey<ChatHubUserEntityBuilder> _primaryKey = new("PK_ChatHubUser", x => x.UserId);

        public ChatHubUserEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override ChatHubUserEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UserId = AddIntegerColumn(table, "UserId");
            FrameworkUserId = AddIntegerColumn(table, "FrameworkUserId", true);
            UserType = AddStringColumn(table, "UserType", 256, false, true);
            return this;
        }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }
        public OperationBuilder<AddColumnOperation> FrameworkUserId { get; set; }
        public OperationBuilder<AddColumnOperation> UserType { get; set; }

    }
}
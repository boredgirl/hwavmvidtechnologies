using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubInvitationEntityBuilder : AuditableBaseEntityBuilder<ChatHubInvitationEntityBuilder>
    {

        private const string _entityTableName = "ChatHubInvitation";
        private readonly PrimaryKey<ChatHubInvitationEntityBuilder> _primaryKey = new("PK_ChatHubInvitation", x => x.Id);
        private readonly ForeignKey<ChatHubInvitationEntityBuilder> _userForeignKey = new("FK_ChatHubInvitation_User", x => x.ChatHubUserId, "User", "UserId", ReferentialAction.NoAction);

        public ChatHubInvitationEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_userForeignKey);
        }

        protected override ChatHubInvitationEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table, "Id");
            ChatHubUserId = AddIntegerColumn(table, "ChatHubUserId");
            RoomId = AddIntegerColumn(table, "RoomId");
            Hostname = AddStringColumn(table, "Hostname", 256, false, true);

            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }
        public OperationBuilder<AddColumnOperation> ChatHubUserId { get; set; }
        public OperationBuilder<AddColumnOperation> RoomId { get; set; }
        public OperationBuilder<AddColumnOperation> Hostname { get; set; }

    }
}
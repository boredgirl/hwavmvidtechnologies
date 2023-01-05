using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubDeviceEntityBuilder : AuditableBaseEntityBuilder<ChatHubDeviceEntityBuilder>
    {

        private const string _entityTableName = "ChatHubDevice";
        private readonly PrimaryKey<ChatHubDeviceEntityBuilder> _primaryKey = new("PK_ChatHubDevice", x => x.Id);
        private readonly ForeignKey<ChatHubDeviceEntityBuilder> _userForeignKey = new("FK_ChatHubDevice_ChatHubUser", x => x.ChatHubUserId, "ChatHubUser", "UserId", ReferentialAction.NoAction);
        private readonly ForeignKey<ChatHubDeviceEntityBuilder> _roomForeignKey = new("FK_ChatHubDevice_ChatHubRoom", x => x.ChatHubRoomId, "ChatHubRoom", "Id", ReferentialAction.NoAction);

        public ChatHubDeviceEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_userForeignKey);
            ForeignKeys.Add(_roomForeignKey);
        }

        protected override ChatHubDeviceEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table, "Id");
            ChatHubUserId = AddIntegerColumn(table, "ChatHubUserId");
            ChatHubRoomId = AddIntegerColumn(table, "ChatHubRoomId");
            UserAgent = AddStringColumn(table, "UserAgent", 512, false, true);
            Type = AddStringColumn(table, "Type", 32, false, true);
            DefaultDeviceId = AddStringColumn(table, "DefaultDeviceId", 256, false, true);
            DefaultDeviceName = AddStringColumn(table, "DefaultDeviceName", 256, false, true);

            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }
        public OperationBuilder<AddColumnOperation> ChatHubUserId { get; set; }
        public OperationBuilder<AddColumnOperation> ChatHubRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> UserAgent { get; set; }
        public OperationBuilder<AddColumnOperation> Type { get; set; }
        public OperationBuilder<AddColumnOperation> DefaultDeviceId { get; set; }
        public OperationBuilder<AddColumnOperation> DefaultDeviceName { get; set; }

    }
}
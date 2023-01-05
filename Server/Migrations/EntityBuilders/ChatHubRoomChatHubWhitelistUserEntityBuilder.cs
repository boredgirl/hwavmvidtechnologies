﻿using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubRoomChatHubWhitelistUserEntityBuilder : AuditableBaseEntityBuilder<ChatHubRoomChatHubWhitelistUserEntityBuilder>
    {

        private const string _entityTableName = "ChatHubRoomChatHubWhitelistUser";
        private readonly PrimaryKey<ChatHubRoomChatHubWhitelistUserEntityBuilder> _primaryKey = new("PK_ChatHubRoomChatHubWhitelistUser", x => x.Id);
        private readonly ForeignKey<ChatHubRoomChatHubWhitelistUserEntityBuilder> _roomForeignKey = new("FK_ChatHubRoomChatHubWhitelistUser_ChatHubRoom", x => x.ChatHubRoomId, "ChatHubRoom", "Id", ReferentialAction.NoAction);
        private readonly ForeignKey<ChatHubRoomChatHubWhitelistUserEntityBuilder> _whitelistUserForeignKey = new("FK_ChatHubRoomChatHubWhitelistUser_ChatHubWhitelistUser", x => x.ChatHubWhitelistUserId, "ChatHubWhitelistUser", "Id", ReferentialAction.NoAction);

        public ChatHubRoomChatHubWhitelistUserEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_roomForeignKey);
            ForeignKeys.Add(_whitelistUserForeignKey);
        }

        protected override ChatHubRoomChatHubWhitelistUserEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table, "Id");
            ChatHubRoomId = AddIntegerColumn(table, "ChatHubRoomId");
            ChatHubWhitelistUserId = AddIntegerColumn(table, "ChatHubWhitelistUserId");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }

        public OperationBuilder<AddColumnOperation> ChatHubRoomId { get; set; }

        public OperationBuilder<AddColumnOperation> ChatHubWhitelistUserId { get; set; }

    }
}
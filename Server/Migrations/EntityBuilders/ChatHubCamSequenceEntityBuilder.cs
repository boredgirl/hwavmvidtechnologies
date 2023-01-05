using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubCamSequenceEntityBuilder : AuditableBaseEntityBuilder<ChatHubCamSequenceEntityBuilder>
    {

        private const string _entityTableName = "ChatHubCamSequence";
        private readonly PrimaryKey<ChatHubCamSequenceEntityBuilder> _primaryKey = new("PK_ChatHubCamSequence", x => x.Id);
        private readonly ForeignKey<ChatHubCamSequenceEntityBuilder> _camForeignKey = new("FK_ChatHubCamSequence_ChatHubCam", x => x.ChatHubCamId, "ChatHubCam", "Id", ReferentialAction.NoAction);

        public ChatHubCamSequenceEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_camForeignKey);
        }

        protected override ChatHubCamSequenceEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table, "Id");
            ChatHubCamId = AddIntegerColumn(table, "ChatHubCamId");
            Filename = AddStringColumn(table, "Filename", 256, false, true);
            FilenameExtension = AddStringColumn(table, "FilenameExtension", 10, false, true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }
        public OperationBuilder<AddColumnOperation> ChatHubCamId { get; set; }
        public OperationBuilder<AddColumnOperation> Filename { get; set; }
        public OperationBuilder<AddColumnOperation> FilenameExtension { get; set; }

    }
}
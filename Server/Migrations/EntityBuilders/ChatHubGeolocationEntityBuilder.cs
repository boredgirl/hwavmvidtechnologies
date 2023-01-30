using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.ChatHubs.Migrations.EntityBuilders
{
    public class ChatHubGeolocationEntityBuilder : AuditableBaseEntityBuilder<ChatHubGeolocationEntityBuilder>
    {

        private const string _entityTableName = "ChatHubGeolocation";
        private readonly PrimaryKey<ChatHubGeolocationEntityBuilder> _primaryKey = new("PK_ChatHubGeolocation", x => x.Id);
        private readonly ForeignKey<ChatHubGeolocationEntityBuilder> _geolocationForeignKey = new("FK_ChatHubGeolocation_ChatHubConnection", x => x.ChatHubConnectionId, "ChatHubConnection", "Id", ReferentialAction.NoAction);

        public ChatHubGeolocationEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_geolocationForeignKey);
        }

        protected override ChatHubGeolocationEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table, "Id");
            ChatHubConnectionId = AddIntegerColumn(table, "ChatHubConnectionId");
            state = AddStringColumn(table, "state", 41, false, true);
            latitude = AddDecimalColumn(table, "latitude", 24, 14, true);
            longitude = AddDecimalColumn(table, "longitude", 24, 14, true);
            altitude = AddDecimalColumn(table, "altitude", 24, 14, true);
            altitudeaccuracy = AddDecimalColumn(table, "altitudeaccuracy", 24, 14, true);
            accuracy = AddDecimalColumn(table, "accuracy", 24, 14, true);
            heading = AddDecimalColumn(table, "heading", 24, 14, true);
            speed = AddDecimalColumn(table, "speed", 24, 14, true);

            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }
        public OperationBuilder<AddColumnOperation> ChatHubConnectionId { get; set; }
        public OperationBuilder<AddColumnOperation> state { get; set; }
        public OperationBuilder<AddColumnOperation> latitude { get; set; }
        public OperationBuilder<AddColumnOperation> longitude { get; set; }
        public OperationBuilder<AddColumnOperation> altitude { get; set; }
        public OperationBuilder<AddColumnOperation> altitudeaccuracy { get; set; }
        public OperationBuilder<AddColumnOperation> accuracy { get; set; }
        public OperationBuilder<AddColumnOperation> heading { get; set; }
        public OperationBuilder<AddColumnOperation> speed { get; set; }

    }
}
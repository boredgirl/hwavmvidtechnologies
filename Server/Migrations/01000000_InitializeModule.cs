﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.ChatHubs.Migrations.EntityBuilders;
using Oqtane.ChatHubs.Repository;

namespace Oqtane.ChatHubs.Migrations
{
    [DbContext(typeof(ChatHubContext))]
    [Migration("ChatHub.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilderUser = new ChatHubUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderUser.Create();
            var entityBuilderRoom = new ChatHubRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderRoom.Create();
            var entityBuilderRoomUser = new ChatHubRoomChatHubUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderRoomUser.Create();
            var entityBuilderMessage = new ChatHubMessageEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderMessage.Create();
            var entityBuilderConnection = new ChatHubConnectionEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderConnection.Create();
            var entityBuilderPhoto = new ChatHubPhotoEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderPhoto.Create();
            var entityBuilderSetting = new ChatHubSettingEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderSetting.Create();
            var entityBuilderDevice = new ChatHubDeviceEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderDevice.Create();
            var entityBuilderInvitation = new ChatHubInvitationEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderInvitation.Create();
            var entityBuilderCam = new ChatHubCamEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderCam.Create();
            var entityBuilderCamSequence = new ChatHubCamSequenceEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderCamSequence.Create();
            var entityBuilderIgnore = new ChatHubIgnoreEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderIgnore.Create();
            var entityBuilderModerator = new ChatHubModeratorEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderModerator.Create();
            var entityBuilderChatHubRoomChatHubModerator = new ChatHubRoomChatHubModeratorEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubModerator.Create();
            var entityBuilderChatHubWhitelistUser = new ChatHubWhitelistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubWhitelistUser.Create();
            var entityBuilderChatHubRoomChatHubWhitelistUser = new ChatHubRoomChatHubWhitelistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubWhitelistUser.Create();
            var entityBuilderChatHubBlacklistUser = new ChatHubBlacklistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubBlacklistUser.Create();
            var entityBuilderChatHubRoomChatHubBlacklistUser = new ChatHubRoomChatHubBlacklistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubBlacklistUser.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilderUser = new ChatHubUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderUser.Drop();
            var entityBuilderChatHubRoomChatHubBlacklistUser = new ChatHubRoomChatHubBlacklistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubBlacklistUser.Drop();
            var entityBuilderChatHubBlacklistUser = new ChatHubBlacklistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubBlacklistUser.Drop();
            var entityBuilderChatHubRoomChatHubWhitelistUser = new ChatHubRoomChatHubWhitelistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubWhitelistUser.Drop();
            var entityBuilderChatHubWhitelistUser = new ChatHubWhitelistUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubWhitelistUser.Drop();
            var entityBuilderChatHubRoomChatHubModerator = new ChatHubRoomChatHubModeratorEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderChatHubRoomChatHubModerator.Drop();
            var entityBuilderModerator = new ChatHubModeratorEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderModerator.Drop();
            var entityBuilderIgnore = new ChatHubIgnoreEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderIgnore.Drop();
            var entityBuilderCam = new ChatHubCamEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderCam.Drop();
            var entityBuilderCamSequence = new ChatHubCamSequenceEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderCamSequence.Drop();
            var entityBuilderSetting = new ChatHubSettingEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderSetting.Drop();
            var entityBuilderDevice = new ChatHubDeviceEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderDevice.Drop();
            var entityBuilderInvitation = new ChatHubInvitationEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderInvitation.Drop();
            var entityBuilderPhoto = new ChatHubPhotoEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderPhoto.Drop();
            var entityBuilderConnection = new ChatHubConnectionEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderConnection.Drop();
            var entityBuilderMessage = new ChatHubMessageEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderMessage.Drop();
            var entityBuilderRoomUser = new ChatHubRoomChatHubUserEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderRoomUser.Drop();
            var entityBuilderRoom = new ChatHubRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilderRoom.Drop();
        }
    }
}

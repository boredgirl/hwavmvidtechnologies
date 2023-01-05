﻿using Microsoft.AspNetCore.SignalR;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Linq;
using Oqtane.ChatHubs.Repository;
using System;
using System.Collections.Generic;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("whisper", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /whisper | /asmr")]
    public class WhisperCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByDisplayNameAsync(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (!targetUser.Online())
            {
                await context.ChatHub.SendClientNotification("User not online.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (caller.UserId == targetUser.UserId)
            {
                await context.ChatHub.SendClientNotification("Calling user can not be target user.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string msg = null;
            if (args.Length > 1)
            {
                msg = String.Join(" ", args.Skip(1)).Trim();
            }

            var callerRoom = await context.ChatHubRepository.GetRoomById(callerContext.RoomId);

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = callerContext.RoomId,
                ChatHubUserId = caller.UserId,
                Content = msg ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Whisper)
            };
            await context.ChatHubRepository.AddMessage(chatHubMessage);
            var chatHubMessageClientModel = context.ChatHubService.CreateChatHubMessageClientModel(chatHubMessage, caller);

            var users = new List<ChatHubUser>(); users.Add(caller); users.Add(targetUser);

            foreach (var item in users)
            {
                var rooms = context.ChatHubRepository.GetRoomsByUser(item).FilterByModuleId(callerContext.ModuleId).Public().Enabled().ToList();
                var connections = await context.ChatHubRepository.GetConnectionsByUserId(item.UserId).Active().ToListAsync();

                foreach (var room in rooms)
                {
                    foreach (var connection in connections)
                    {
                        await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("AddMessage", chatHubMessageClientModel);
                    }
                }
            }

        }
    }
}
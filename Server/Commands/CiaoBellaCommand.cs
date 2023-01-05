﻿using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Microsoft.AspNetCore.SignalR;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Enums;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("ciaobella", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /ciaobella")]
    public class CiaoBellaCommand : BaseCommand
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

            if (caller.UserId == targetUser.UserId)
            {
                context.ChatHubRepository.DeleteMessages(caller.UserId);
                context.ChatHubRepository.DeleteConnections(caller.UserId);
                context.ChatHubRepository.DeleteRooms(caller.UserId, callerContext.ModuleId);
                context.ChatHubRepository.DeleteUser(caller.UserId);

                throw new HubException(string.Format("Successfully deleted all database entries. System do not know an user named {0} anymore.", caller.DisplayName));
            }
        }
    }
}
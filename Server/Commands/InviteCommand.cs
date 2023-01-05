using Microsoft.AspNetCore.SignalR;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System;
using System.Linq;
using Oqtane.ChatHubs.Repository;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Enums;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("invite", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin } , "Usage: /invite")]
    public class InviteCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByDisplayNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            var targetUserConnections = await context.ChatHubRepository.GetConnectionsByUserId(targetUser.UserId).Active().ToListAsync();
            if (!targetUserConnections.Any())
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
            var oneVsOneRoom = await context.ChatHubService.GetOneVsOneRoomAsync(caller, targetUser, callerRoom.ModuleId);

            if(oneVsOneRoom != null)
            {
                await context.ChatHub.EnterChatRoom(oneVsOneRoom.Id);

                ChatHubInvitation chatHubInvitation = new ChatHubInvitation()
                {
                    ChatHubUserId = targetUser.UserId,
                    RoomId = oneVsOneRoom.Id,
                    Hostname = caller.DisplayName,
                    CreatedBy = targetUser.Username,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = targetUser.Username,
                    ModifiedOn = DateTime.Now,
                };

                context.ChatHubRepository.AddInvitation(chatHubInvitation);

                /*
                var targetUserConnections = await context.ChatHubRepository.GetConnectionsByUserId(targetUser.UserId).Active().ToListAsync();
                foreach (var connection in targetUserConnections)
                {
                    await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("AddInvitation", chatHubInvitation);
                }
                */
            }

        }
    }
}
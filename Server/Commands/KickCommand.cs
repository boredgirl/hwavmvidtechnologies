using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Microsoft.AspNetCore.SignalR;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Repository;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("kick", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /kick | /throw | fly")]
    public class KickCommand : ModeratorCommand
    {
        public override async Task ExecuteModeratorOperation(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
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

            var targetUserConnections = await context.ChatHubRepository.GetConnectionsByUserId(targetUser.UserId).Active().ToListAsync();
            foreach (var connection in targetUserConnections)
            {
                await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("Disconnect", context.ChatHubService.CreateChatHubUserClientModel(targetUser));
            }

        }
    }
}
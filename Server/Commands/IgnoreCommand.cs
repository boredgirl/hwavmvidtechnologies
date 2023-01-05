using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Enums;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("ignore", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /ignore | /block")]
    public class IgnoreCommand : BaseCommand
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
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            await context.ChatHubService.IgnoreUser(caller, targetUser);
            await context.ChatHub.SendClientNotification(string.Concat(targetUser.DisplayName, " ", "has been added to ignore list."), callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);

        }
    }
}
﻿using System.Composition;
using System.Threading.Tasks;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Repository;
using Oqtane.Shared;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("message-color", "[]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin } , "Usage: /message-color")]
    public class MessageColorCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string messageColor = args[0];

            if(!string.IsNullOrEmpty(messageColor))
            {
                var settings = caller.Settings;                
                settings.MessageColor = messageColor;
                await context.ChatHubRepository.UpdateSetting(settings);

                await context.ChatHub.SendClientNotification("Message Color Updated.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
            }

        }
    }
}
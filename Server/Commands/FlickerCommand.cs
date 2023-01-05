﻿using Microsoft.AspNetCore.SignalR;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Text.RegularExpressions;
using System;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Repository;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("flicker", "[message]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin } , "Usage: /flickr | /flicker")]
    public class FlickerCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string msg = String.Join(" ", args).Trim();

            var regex = new Regex(@"[^ bcdfghjklmnpqrstvwxyz_]");
            msg = regex.Replace(msg, "");

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = callerContext.RoomId,
                ChatHubUserId = caller.UserId,
                Content = msg,
                Type = ChatHubMessageType.Guest.ToString()
            };
            await context.ChatHubRepository.AddMessage(chatHubMessage);
            ChatHubMessage chatHubMessageClientModel = context.ChatHubService.CreateChatHubMessageClientModel(chatHubMessage, caller);            
            
            var connectionsIds = context.ChatHubService.GetAllExceptConnectionIds(caller);
            await context.ChatHub.Clients.GroupExcept(callerContext.RoomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);

        }
    }
}
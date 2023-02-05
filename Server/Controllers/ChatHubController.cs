using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Oqtane.ChatHubs.Repository;
using Oqtane.Infrastructure;
using System.Drawing;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Hubs;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Constants;
using Microsoft.EntityFrameworkCore;
using Hwavmvid.Pager;
using System.Collections.Generic;
using FFMpegCore;
using FFMpegCore.Enums;
using BlazorDownload;

namespace Oqtane.ChatHubs.Controllers
{

    [Route("{site}/api/[controller]/[action]")]
    public class ChatHubController : Controller
    {

        private IHttpContextAccessor httpContextAccessor { get; set; }
        private IWebHostEnvironment webHostEnvironment { get; set; }
        private IHubContext<ChatHub> chatHubContext { get; set; }
        private ChatHubRepository chatHubRepository { get; set; }
        private ChatHubService chatHubService { get; set; }

        private int EntityId = -1;
        private readonly ILogManager logger;

        public ChatHubController(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, IHubContext<ChatHub> chatHubContext, ChatHubRepository chatHubRepository, ChatHubService chatHubService, ILogManager logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.webHostEnvironment = webHostEnvironment;
            this.chatHubContext = chatHubContext;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.logger = logger;

            if (httpContextAccessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                EntityId = int.Parse(httpContextAccessor.HttpContext.Request.Query["entityid"]);
            }
        }

        private async Task<ChatHubUser> GetUser(string connectionId)
        {

            if (this.httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var username = this.httpContextAccessor.HttpContext.User.Identity.Name;
                return await this.chatHubRepository.GetUserByUserNameAsync(username);
            }
            else
            {
                ChatHubConnection connection = chatHubRepository.Connections().Include(item => item.User).ByConnectionId(connectionId);
                if (connection != null)
                {
                    var guest = await this.chatHubRepository.GetUserByIdAsync(connection.User.UserId);
                    if (guest.UserType == ChatHubUserType.Guest.ToString())
                    {
                        return guest;
                    }
                }
            }

            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostImageUpload()
        {
            try
            {

                string connectionId = null;
                if (Request.Headers.ContainsKey("connectionId"))
                {
                    connectionId = Request.Headers["connectionId"];
                    if (string.IsNullOrEmpty(connectionId))
                    {
                        return new BadRequestObjectResult(new { Message = "No connection id." });
                    }
                }

                ChatHubUser user = await this.GetUser(connectionId);
                if (user == null)
                {
                    return new BadRequestObjectResult(new { Message = "No user found." });
                }

                string displayName = string.Empty;
                if (Request.Headers.ContainsKey("displayName"))
                {
                    displayName = Request.Headers["displayName"];
                    if (string.IsNullOrEmpty(displayName))
                    {
                        return new BadRequestObjectResult(new { Message = "No display name." });
                    }
                }

                string roomId = string.Empty;
                if (Request.Headers.ContainsKey("roomId"))
                {
                    roomId = Request.Headers["roomId"];
                    if (string.IsNullOrEmpty(roomId))
                    {
                        return new BadRequestObjectResult(new { Message = "No room id." });
                    }
                }

                string moduleId = string.Empty;
                if (Request.Headers.ContainsKey("moduleId"))
                {
                    moduleId = Request.Headers["moduleId"];
                    if (string.IsNullOrEmpty(moduleId))
                    {
                        return new BadRequestObjectResult(new { Message = "No module id." });
                    }
                }

                IFormFileCollection files = Request.Form.Files;
                if (files == null || files.Count <= 0)
                {
                    return new BadRequestObjectResult(new { Message = "No files." });
                }

                string content = string.Concat(files.Count, " ", "Photo(s)");
                ChatHubRoom chatHubRoom = await this.chatHubRepository.GetRoomById(Int32.Parse(roomId));
                if (chatHubRoom == null)
                {
                    return new BadRequestObjectResult(new { Message = "No room found." });
                }

                ChatHubMessage chatHubMessage = new ChatHubMessage()
                {
                    ChatHubRoomId = chatHubRoom.Id,
                    ChatHubUserId = user.UserId,
                    Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Image),
                    Content = content,
                    CreatedBy = user.Username,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = user.Username,
                    ModifiedOn = DateTime.Now,
                };
                chatHubMessage = await this.chatHubRepository.AddMessage(chatHubMessage);

                var maxFileSize = 10;
                var maxFileCount = 3;

                string folderName = ChatHubConstants.UploadImagesPath;
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (files.Count > maxFileCount)
                {
                    return new BadRequestObjectResult(new { Message = "Maximum number of files exceeded." });
                }

                List<ChatHubPhoto> pics = new List<ChatHubPhoto>();
                foreach (IFormFile file in files)
                {

                    if (file.Length > (maxFileSize * 1024 * 1024))
                    {
                        return new BadRequestObjectResult(new { Message = "File size Should Be UpTo " + maxFileSize + "MB" });
                    }

                    var supportedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(file.FileName);
                    if (!supportedFileExtensions.Contains(fileExtension))
                    {
                        return new BadRequestObjectResult(new { Message = "Unknown file type(s)." });
                    }

                    /*
                    ObjectResult result = await this.PostAsync(file);
                    dynamic obj = result.Value;
                    string imageClassification = string.Empty;
                    if (result.StatusCode.Value == 200)
                    {
                        if (obj.predictedLabel == "dickpic")
                        {
                            var percent = string.Format("{0:P2}", Math.Round((float)obj.probability, 3));
                            if ((float)obj.probability >= 0.99)
                            {
                                imageClassification = string.Concat(" | ", "(", "most likely identified as ", obj.predictedLabel, ": ", percent, ")");
                            }
                        }
                    }
                    */

                    int imageWidth, imageHeight;
                    string fileName = string.Concat(Guid.NewGuid().ToString(), fileExtension);
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);

                        FileInfo fileInfo = new FileInfo(file.FileName);
                        var sizeInBytes = file.Length;
                        Bitmap img = new Bitmap(stream);
                        imageWidth = img.Width;
                        imageHeight = img.Height;
                    }

                    ChatHubPhoto chatHubPicture = new ChatHubPhoto()
                    {
                        ChatHubMessageId = chatHubMessage.Id,
                        Source = fileName,
                        Size = file.Length,
                        Thumb = fileName,
                        Caption = string.Concat(user.DisplayName, " | ", Math.Round(Convert.ToDecimal(file.Length / (1024.0m * 1024.0m)), 2), "MB"/*, imageClassification*/),
                        Message = chatHubMessage,
                        Width = imageWidth,
                        Height = imageHeight,
                        CreatedBy = user.Username,
                        CreatedOn = DateTime.Now,
                        ModifiedBy = user.Username,
                        ModifiedOn = DateTime.Now,
                    };

                    chatHubPicture = this.chatHubRepository.AddPhoto(chatHubPicture);
                    pics.Add(chatHubPicture);
                }

                var chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, user, pics);
                await this.chatHubContext.Clients.Group(chatHubMessage.ChatHubRoomId.ToString()).SendAsync("AddMessage", chatHubMessageClientModel);
                return new OkObjectResult(new { Message = "Successfully Uploaded Files." });
            }
            catch
            {
                return new BadRequestObjectResult(new { Message = "Error Uploading Files." });
            }
            
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostRoomImageUpload()
        {
            try
            {
                string roomId = string.Empty;
                if (Request.Headers.ContainsKey("roomid"))
                {
                    roomId = Request.Headers["roomid"];
                    if (string.IsNullOrEmpty(roomId))
                    {
                        return new BadRequestObjectResult(new { Message = "No room id." });
                    }
                }

                IFormFileCollection files = Request.Form.Files;
                if (files == null || files.Count <= 0)
                {
                    return new BadRequestObjectResult(new { Message = "No files." });
                }

                var maxFileSize = 10;
                var maxFileCount = 3;

                string folderName = ChatHubConstants.RoomImagesPath;
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (files.Count > maxFileCount)
                {
                    return new BadRequestObjectResult(new { Message = "Maximum number of files exceeded." });
                }

                foreach (IFormFile file in files)
                {
                    if (file.Length > (maxFileSize * 1024 * 1024))
                    {
                        return new BadRequestObjectResult(new { Message = "File size Should Be UpTo " + maxFileSize + "MB" });
                    }

                    var supportedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(file.FileName);
                    if (!supportedFileExtensions.Contains(fileExtension))
                    {
                        return new BadRequestObjectResult(new { Message = "Unknown file type(s)." });
                    }

                    string fileName = string.Concat(Guid.NewGuid().ToString(), fileExtension);
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var room = await this.chatHubRepository.GetRoomById(Int32.Parse(roomId));
                    if (room != null)
                    {
                        room.ImageUrl = fileName;
                        await this.chatHubRepository.UpdateRoom(room);
                    }
                }

                return new OkObjectResult(new { Message = "Successfully Uploaded Files." });
            }
            catch
            {
                return new BadRequestObjectResult(new { Message = "Error Uploading Files." });
            }
        }

        [HttpDelete("{id}")]
        [ActionName("DeleteRoomImage")]
        [Authorize(Policy = "EditModule")]
        public async Task<IActionResult> DeleteRoomImage(int id, int moduleid)
        {
            try
            {
                var room = await this .chatHubRepository.GetRoomById(id);
                if (room != null)
                {
                    room.ImageUrl = string.Empty;
                    await this.chatHubRepository.UpdateRoom(room);
                    return new OkObjectResult(new { Message = "Successfully Removed Image." });
                }

                return new NotFoundObjectResult(new { Message = "Could not found any requested objects." });
            }
            catch
            {
                return new BadRequestObjectResult(new { Message = "Error Removing Image." });
            }
        }

        [HttpGet("{page}/{items}/{id}")]
        [ActionName("GetRooms")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubRoom>> GetRooms(int page, int items, int id)
        {
            return await this.chatHubService.GetRooms(page, items, id, false);
        }

        [HttpGet("{page}/{items}/{id}")]
        [ActionName("GetUsers")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubUser>> GetUsers(int page, int items, int id)
        {
            return await this.chatHubService.GetUsers(page, items, id, false);
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetArchiveItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubCam>> GetArchiveItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetArchiveItems(page, items, id, true, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetInvitationItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubInvitation>> GetInvitationItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetInvitationItems(page, items, id, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetIgnoreItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubIgnore>> GetIgnoreItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetIgnoreItems(page, items, id, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetIgnoredByItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubIgnoredBy>> GetIgnoredByItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetIgnoredByItems(page, items, id, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetModeratorItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubModerator>> GetModeratorItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetModeratorItems(page, items, id, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetBlacklistUserItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubBlacklistUser>> GetBlacklistUserItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetBlacklistUserItems(page, items, id, user);
            }

            return null;
        }

        [HttpGet("{page}/{items}/{id}/{connectionId}")]
        [ActionName("GetWhitelistUserItems")]
        [AllowAnonymous]
        public async Task<PagerApiItem<ChatHubWhitelistUser>> GetWhitelistUserItems(int page, int items, int id, string connectionId)
        {
            ChatHubUser user = await this.GetUser(connectionId);
            if (user != null)
            {
                return await this.chatHubService.GetWhitelistUserItems(page, items, id, user);
            }

            return null;
        }

        [DisableRequestSizeLimit]
        [HttpGet("{connectionid}/{camid}/{roomid}/{moduleid}/{filetype}")]
        [ActionName("DownloadVideoCapture")]
        [AllowAnonymous]
        public async Task<BlazorDownloadApiItem> DownloadVideoCapture(string connectionId, string camId, string roomId, string moduleId, string fileType)
        {
            try
            {

                var contextUser = await this.GetUser(connectionId);
                if (contextUser != null)
                {

                    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./wwwroot/Modules/Oqtane.ChatHubs", TemporaryFilesFolder = "/tmp" });

                    var contextCam = await this.chatHubRepository.GetCamById(Convert.ToInt32(camId));
                    if (contextCam != null)
                    {

                        var camSequences = await this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == contextCam.Id).ToListAsync();
                        if (camSequences != null)
                        {

                            string tempFileExtension = ".mp4";
                            string tempFileExtensionExtractedAudio = ".mp3";
                            string tempFilename = Guid.NewGuid().ToString();
                            string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                            string tempDirectoryPath = Path.Combine(webRootPath, "Modules", "Oqtane.ChatHubs", "temp");
                            string videosDirecotryPath = Path.Combine(webRootPath, "Modules", "Oqtane.ChatHubs", "videos", contextUser.Username, moduleId, roomId, camId);
                            string tempFullPath = Path.Combine(tempDirectoryPath, tempFilename + tempFileExtension);
                            string tempFullPathExtractedAudio = Path.Combine(tempDirectoryPath, tempFilename + tempFileExtensionExtractedAudio);

                            if (!Directory.Exists(tempDirectoryPath))
                            {
                                Directory.CreateDirectory(tempDirectoryPath);
                            }

                            List<string> videos = new List<string>();
                            foreach (var camSequence in camSequences.Select((item, index) => new { item = item, index = index }))
                            {
                                var tempIndex = camSequence.index;

                                string fileExtension = camSequence.item.FilenameExtension;
                                string filename = camSequence.item.Filename;
                                string fullPath = Path.Combine(videosDirecotryPath, filename + fileExtension);

                                string outputPathFileExtension = ".mp4";
                                string outputPath = Path.Combine(videosDirecotryPath, filename + outputPathFileExtension);

                                if (fileType == ".mp4" || fileType == ".mp3")
                                {
                                    if (Directory.Exists(videosDirecotryPath))
                                    {
                                        if (System.IO.File.Exists(fullPath) &&
                                            !System.IO.File.Exists(outputPath))
                                        {
                                            await FFMpegArguments
                                                .FromFileInput(fullPath)
                                                .OutputToFile(outputPath, false, options => options
                                                .WithVideoCodec(VideoCodec.LibX264)
                                                .WithAudioCodec(AudioCodec.Aac)
                                                //.WithConstantRateFactor(21)
                                                //.WithVariableBitrate(4)
                                                //.WithVideoFilters(filterOptions => filterOptions
                                                //.Scale(VideoSize.Ld))
                                                .WithFastStart())
                                                .ProcessAsynchronously();
                                        }
                                    }
                                }

                                if (Directory.Exists(videosDirecotryPath))
                                {
                                    if (System.IO.File.Exists(fullPath))
                                    {
                                        videos.Add(outputPath);
                                    }
                                }

                                BlazorDownloadApiItem apiItem = new BlazorDownloadApiItem()
                                {
                                    Id = camId,
                                    ProgressTotal = camSequences.Count(),
                                    ProgressCurrent = tempIndex,
                                    DownloadCompleted = false,
                                    Base64Uri = string.Empty,
                                    Message = "Converting video files",
                                };

                                await this.chatHubContext.Clients.Client(connectionId).SendAsync("DownloadVideoCapture", apiItem);
                            }

                            foreach (var video in videos.Select((item, index) => new { item = item, index = index }))
                            {
                                var tempIndex = video.index;

                                if (!System.IO.File.Exists(tempFullPath))
                                {
                                    System.IO.File.Copy(video.item, tempFullPath);
                                    continue;
                                }

                                FFMpeg.Join(
                                    tempFullPath,
                                    tempFullPath,
                                    video.item
                                );                               

                                BlazorDownloadApiItem apiItem = new BlazorDownloadApiItem()
                                {
                                    Id = camId,
                                    ProgressTotal = videos.Count(),
                                    ProgressCurrent = tempIndex,
                                    DownloadCompleted = false,
                                    Base64Uri = string.Empty,
                                    Message = "Joining video files",
                                };

                                await this.chatHubContext.Clients.Client(connectionId).SendAsync("DownloadVideoCapture", apiItem);
                            }

                            if (fileType == ".mp3")
                            {
                                FFMpeg.ExtractAudio(tempFullPath, tempFullPathExtractedAudio);
                                var base64uri = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(tempFullPathExtractedAudio));

                                BlazorDownloadApiItem mp3 = new BlazorDownloadApiItem()
                                {
                                    Id = camId,
                                    ProgressTotal = videos.Count(),
                                    ProgressCurrent = videos.Count(),
                                    DownloadCompleted = true,
                                    Base64Uri = base64uri,
                                    Message = "Download finished",
                                };

                                return mp3;
                            }

                            if (fileType == ".mp4")
                            {
                                var base64uri = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(tempFullPath));
                                BlazorDownloadApiItem mp4 = new BlazorDownloadApiItem()
                                {
                                    Id = camId,
                                    ProgressTotal = videos.Count(),
                                    ProgressCurrent = videos.Count(),
                                    DownloadCompleted = true,
                                    Base64Uri = base64uri,
                                    Message = "Download finished",
                                };

                                contextCam.VideoUrl = tempFilename;
                                contextCam.VideoUrlExtension = tempFileExtension;
                                await this.chatHubRepository.UpdateCam(contextCam);

                                return mp4;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }

        [HttpGet("{moduleId}/{connectionId}/{roomId}/{camId}")]
        [ActionName("DownloadVideo")]
        [AllowAnonymous]
        public async Task<string> DownloadVideo(string moduleId, string connectionId, string roomId, string camId)
        {
            try
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./wwwroot/Modules/Oqtane.ChatHubs", TemporaryFilesFolder = "/tmp" });
                ChatHubUser contextUser = await this.GetUser(connectionId);
                if (contextUser != null)
                {

                    var contextCam = await this.chatHubRepository.GetCamById(Convert.ToInt32(camId));
                    if (contextCam != null)
                    {

                        var camSequences = await this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == contextCam.Id).ToListAsync();
                        if (camSequences != null)
                        {

                            string tempFileExtension = ".mp4";
                            string tempFilename = Guid.NewGuid().ToString();
                            string tempWebRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                            string tempDirectoryPath = Path.Combine(tempWebRootPath, "Modules", "Oqtane.ChatHubs", "temp");
                            string tempFullPath = Path.Combine(tempDirectoryPath, tempFilename + tempFileExtension);

                            if (!Directory.Exists(tempDirectoryPath))
                            {
                                Directory.CreateDirectory(tempDirectoryPath);
                            }

                            List<string> videos = new List<string>();
                            foreach (var camSequence in camSequences.Select((item, index) => new { item = item, index = index }))
                            {
                                var tempIndex = camSequence.index;

                                string fileExtension = camSequence.item.FilenameExtension;
                                string filename = camSequence.item.Filename;
                                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                                string directoryPath = Path.Combine(webRootPath, "Modules", "Oqtane.ChatHubs", "videos", contextUser.Username, moduleId, roomId, camId);
                                string fullPath = Path.Combine(directoryPath, filename + fileExtension);

                                string outputPathFileExtension = ".mp4";
                                string outputPath = Path.Combine(directoryPath, filename + outputPathFileExtension);

                                if (Directory.Exists(directoryPath))
                                {
                                    if (System.IO.File.Exists(fullPath) &&
                                        !System.IO.File.Exists(outputPath))
                                    {
                                        await FFMpegArguments
                                            .FromFileInput(fullPath)
                                            .OutputToFile(outputPath, false, options => options
                                            .WithVideoCodec(VideoCodec.LibX264)
                                            .WithAudioCodec(AudioCodec.Aac)
                                            //.WithConstantRateFactor(21)
                                            //.WithVariableBitrate(4)
                                            //.WithVideoFilters(filterOptions => filterOptions
                                            //.Scale(VideoSize.Ld))
                                            .WithFastStart())
                                            .ProcessAsynchronously();
                                    }
                                }

                                if (Directory.Exists(directoryPath))
                                {
                                    if (System.IO.File.Exists(fullPath))
                                    {
                                        videos.Add(outputPath);
                                    }
                                }
                            }

                            FFMpeg.Join(
                                tempFullPath,
                                videos.ToArray()
                            );

                            var base64uri = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(tempFullPath));
                            return base64uri;
                        }
                    }
                }

                return null;
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }

    }
}

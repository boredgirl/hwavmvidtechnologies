﻿<div align="center">
	<img src="https://github.com/boredgirl/hwavmvidtechnologies/blob/main/Server/wwwroot/Modules/Oqtane.ChatHubs/wasmchatlogo.png?raw=true" class="img-fluid" width="240" title="wasmchat">
</div>

## asp .net core blazor signalR entity framework video chat hub template

-> short installation guide for hwavmvid project installation

-> install vs and sql server
-> search for control panel -> programs -> install features -> under iis enable websockets and leave webdav disabled

-> open vs and clone the hwavmvid project into folder on desktop
-> whenever you want copy package to oqtane project rebuild solution in debug mode or create package in release mode

-> open vs and clone the oqtane project into folder on desktop named oqtane.framework
-> in vs click view -> open git repository and checkout commit from end of year 2021 -> maybe this one f54d0754

-> in vs open the server project properties and enable ssl connection in debug section
-> then run the project with iisexpress and during installation use host as oqtane username

-> if you do not allow browser access to audio and video and microphone the cams wont show up


-----------------------------------------------------------------------------------------------------
deprecated installation instructions use commit earlier
migrating to oqtane.3.2.1
for now resolve dependencies in oqtane server project

<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="6.0.3" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="6.0.3" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Protocols.NewtonsoftJson" Version="6.0.3" />
<PackageReference Include="Microsoft.Composition" Version="1.0.31" />
-----------------------------------------------------------------------------------------------------

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate/?hosted_button_id=SMWJYALAKFEWC)

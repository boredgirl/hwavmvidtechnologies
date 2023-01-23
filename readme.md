<div align="center">
	<img src="https://github.com/boredgirl/hwavmvidtechnologies/blob/main/Server/wwwroot/Modules/Oqtane.ChatHubs/wasmchatlogo.png?raw=true" class="img-fluid" width="240" title="wasmchat">
</div>

## asp .net core blazor signalR entity framework video chat hub template

-> short installation guide for hwavmvid project installation

-> install vs and sql server and ssms and asp net core sdk and runtime version 6.0.3
-> search for control panel -> programs -> install features -> under iis enable websockets and leave webdav disabled

-> open vs and clone the hwavmvid project into folder on desktop
-> whenever you want copy package to oqtane project rebuild solution in debug mode or create package in release mode

-> open vs and clone the oqtane project into folder on desktop named oqtane.framework
-> in vs click view -> open git repository and checkout commit from end of year oqtane version 3.2.1

-> in vs open the server project properties and enable ssl connection in debug section
-> then run the project with iisexpress and during installation use host as oqtane username

-> if you do not allow browser access to audio and video and microphone the cams wont show up


-----------------------------------------------------------------------------------------------------
-> if not work resolve packages in oqtane client and server project manual

<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="6.0.3" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client.Core" Version="6.0.3" />

<PackageReference Include="System.Composition" Version="7.0.0" />
<PackageReference Include="System.Composition.AttributedModel" Version="7.0.0" />
<PackageReference Include="System.Composition.TypedParts" Version="7.0.0" />
<PackageReference Include="System.Composition.Hosting" Version="7.0.0" />
<PackageReference Include="System.Composition.Runtime" Version="7.0.0" />
<PackageReference Include="System.Composition.Convention" Version="7.0.0" />

<PackageReference Include="System.Drawing.Common" Version="5.0.2" />
<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
<PackageReference Include="FFMpegCore" Version="4.7.0" />
<PackageReference Include="Instances" Version="1.6.1" />
-----------------------------------------------------------------------------------------------------
demo website here <a href="https://mihcelle.hwavmvid.com/" target="_blank">https://mihcelle.hwavmvid.com/</a>
-----------------------------------------------------------------------------------------------------


[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate/?hosted_button_id=SMWJYALAKFEWC)

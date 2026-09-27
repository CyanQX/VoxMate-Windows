#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\installer-input"
#endif

[Setup]
AppId={{D5C2F992-27F8-4F0B-9E4D-86B2DB93C0AA}
AppName=VoxMate
AppVersion={#AppVersion}
AppVerName=VoxMate {#AppVersion}
AppPublisher=VoxMate
VersionInfoVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductName=VoxMate
VersionInfoCompany=VoxMate
VersionInfoDescription=VoxMate Setup
LicenseFile=..\LICENSE
DefaultDirName={localappdata}\Programs\VoxMate
DefaultGroupName=VoxMate
PrivilegesRequired=lowest
SetupArchitecture=x64
MinVersion=10.0
WizardStyle=modern
SetupIconFile=..\src\VoiceTranslator.App\Assets\voxmate.ico
UninstallDisplayIcon={app}\VoiceTranslator.App.exe
Compression=lzma2/max
SolidCompression=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=VoxMate-Setup-{#AppVersion}-win-x64
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
chinesesimp.DesktopIconTask=创建桌面快捷方式
english.DesktopIconTask=Create a desktop shortcut
chinesesimp.LaunchApp=运行 VoxMate
english.LaunchApp=Launch VoxMate

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIconTask}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\VoxMate"; Filename: "{app}\VoiceTranslator.App.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\VoxMate"; Filename: "{app}\VoiceTranslator.App.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\VoiceTranslator.App.exe"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

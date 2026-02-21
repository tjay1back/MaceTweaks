[Setup]
AppName=MaceTweaks
AppVersion=2.1.0
AppPublisher=PeakJayX
DefaultDirName={autopf}\MaceTweaks
DefaultGroupName=MaceTweaks
OutputDir=installer_output
OutputBaseFilename=MaceTweaks_Setup
Compression=lzma2
SolidCompression=yes
SetupIconFile=MaceTweaks.ico
UninstallDisplayIcon={app}\MaceTweaks.exe
PrivilegesRequired=lowest
WizardStyle=modern

[Files]
Source: "publish\MaceTweaks.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu shortcut
Name: "{group}\MaceTweaks"; Filename: "{app}\MaceTweaks.exe"
Name: "{group}\Uninstall MaceTweaks"; Filename: "{uninstallexe}"
; Desktop shortcut (optional) - user desktop, not public
Name: "{userdesktop}\MaceTweaks"; Filename: "{app}\MaceTweaks.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\MaceTweaks.exe"; Description: "Launch MaceTweaks"; Flags: nowait postinstall skipifsilent

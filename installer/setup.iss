#define AppVersion GetEnv("AppVersion")
#if AppVersion == ""
  #define AppVersion "0.1.0-test"
#endif

#define AssetsDir "assets"
#define WizardImagePath AddBackslash(SourcePath) + AssetsDir + "\\wizard.bmp"
#define WizardSmallImagePath AddBackslash(SourcePath) + AssetsDir + "\\wizard-small.bmp"
#define SetupIconPath AddBackslash(SourcePath) + AssetsDir + "\\setup.ico"

[Setup]
AppId={{E8A6E2E4-3C2A-49D2-BB48-7A02489A4472}
AppName=TuColmadoRD
AppVersion={#AppVersion}
AppPublisher=Synset Solutions
AppPublisherURL=https://tucolmadord.com
AppSupportURL=https://wa.me/18296932458
AppUpdatesURL=https://tucolmadord.com
AppCopyright=Copyright (c) 2026 Synset Solutions
DefaultDirName={autopf}\TuColmadoRD
DefaultGroupName=TuColmadoRD
OutputDir=../publish/installer
OutputBaseFilename=TuColmadoRD-Setup-v{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
MinVersion=10.0
LicenseFile=terms-and-conditions.txt

#ifexist "{#SetupIconPath}"
SetupIconFile={#SetupIconPath}
#endif

#ifexist "{#WizardImagePath}"
WizardImageFile={#WizardImagePath}
#endif

#ifexist "{#WizardSmallImagePath}"
WizardSmallImageFile={#WizardSmallImagePath}
#endif

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear ícono en el escritorio"; Flags: checkedonce
Name: "startupicon"; Description: "Iniciar TuColmadoRD con Windows"; Flags: unchecked

[Files]
Source: "../publish/desktop/*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Pre-instalar WebView2 si es necesario (el setup.exe debería estar en assets/)
; Source: "../assets/MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"
Name: "{commondesktop}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"; Tasks: desktopicon
Name: "{userstartup}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"; Tasks: startupicon

[Run]
; Descargar e instalar WebView2 en modo silencioso si aplica
; Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Instalando componentes necesarios (WebView2)..."; Check: WebView2NotInstalled

[Code]
function WebView2NotInstalled: Boolean;
var version: String;
begin
  Result := not RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', version);
end;

#define AppVersion GetEnv("AppVersion")
#if AppVersion == ""
  #define AppVersion "0.1.0-test"
#endif

[Setup]
AppName=TuColmadoRD
AppVersion={#AppVersion}
AppPublisher=Synset Solutions
AppPublisherURL=https://tucolmadord.com
DefaultDirName={autopf}\TuColmadoRD
DefaultGroupName=TuColmadoRD
OutputDir=../publish/installer
OutputBaseFilename=TuColmadoRD-Setup-v{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
MinVersion=10.0

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear ícono en el escritorio"; Flags: checked
Name: "startupicon"; Description: "Iniciar TuColmadoRD con Windows"; Flags: unchecked

[Files]
Source: "../publish/desktop/TuColmadoRD.Desktop.exe"; DestDir: "{app}"; Flags: ignoreversion
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

using System.Collections.Generic;
using System.Windows.Input;
using SmartHunter.Core.Config;

namespace SmartHunter.Game.Config
{
    public class MainConfig
    {
        public string VersionsFileName = "Versions.json";
        public string LocalizationFileName = "en-US.json";
        public string SkinFileName = "Default.xaml";
        public string MonsterDataFileName = "MonsterData.json";
        public string PlayerDataFileName = "PlayerData.json";
        public string MemoryFileName = "Memory.json";
        public string ServerUrl = "http://140.238.55.121/index.php";
        public string UserDataPath = @"C:\Program Files (x86)\Steam\userdata\";

        public bool IgnoreHttpsErrors = true;
        public bool ShutdownWhenProcessExits = false;
        public bool BackupWhenProcessExits = false;
        public bool AutomaticallyCheckAndDownloadUpdates = true; // TODO: Rimetti a true
        public bool StartMHWWhenSmartHunterStart = false;

        public OverlayConfig Overlay = new OverlayConfig();

        [PreserveCollectionIntegrity]
        public Dictionary<InputControl, Key> Keybinds = new Dictionary<InputControl, Key>()
        {
            { InputControl.ManipulateWidget, Key.LeftAlt },
            { InputControl.HideWidgets, Key.F1 },
            { InputControl.CopyTeamDamage, Key.F5 },
            { InputControl.CopyPlayer1Damage, Key.F6},
            { InputControl.CopyPlayer2Damage, Key.F7},
            { InputControl.CopyPlayer3Damage, Key.F9},
            { InputControl.CopyPlayer4Damage, Key.F10}
        };

        public DebugConfig Debug = new DebugConfig();
    }
}

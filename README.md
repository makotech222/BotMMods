# BotMMods

UnityModManager 0.21.2 supported; Add the following to the UnityModManagerConfig.xml:

</GameInfo>
<GameInfo Name="Banner of the Maid">
<Folder>Banner of the Maid</Folder>
<ModsDirectory>Mods</ModsDirectory>
<ModInfo>Info.json</ModInfo>
<GameExe>banner.exe</GameExe>
<EntryPoint>[UnityEngine.UI.dll]UnityEngine.EventSystems.EventSystem.cctor:After</EntryPoint>
<StartingPoint>[Assembly-CSharp.dll]Game.Client.UITitleMenu.Awake:Before</StartingPoint>
</GameInfo> 

Download and Install the CheatMod.zip file from the git repo. Use the Unity Mod Manager UI to change settings


CheatMod:
- Experience Multiplier
- Set Minimum stat growth on level up
- Inf Item Usage
- Inf Weapon Usage
- No classup token usage
- No Learn Skill token usage
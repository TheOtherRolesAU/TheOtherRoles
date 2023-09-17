![banner](./Images/TOR_logo.png)
<p align="center"><a href="https://github.com/MrFangkuai/TheOtherRolesAF/releases/">

免责声明：此模组不隶属于Among Us或Innersloth LLC，其中包含的内容没有得到Innersloth LLC的认可或其他赞助。此模组包含的部分材料是Innersloth LLC的资产.© Innersloth LLC.</p>
<p align="center">
对于其他的MOD开发人员：要使用我们的代码，请阅读并尊重<ahref=“#License”>GPL 3许可</a></p>

# The Other Roles AF

The **The Other Roles AF** 是一个Among Us模组[Among Us下载](https://store.steampowered.com/app/945360/Among_Us) 增加了许多新的职业，新的设置和新的自定义帽子到游戏。
更多职业即将出现！:)

| 内鬼 | 船员 | 独立 | 附加 | 模式 |
|----------|-------------|-----------------|----------------|----------------|
| [教父 (教)](#mafia) | [市长](#mayor) | [小丑](#jester) | [溅血者](#bloody) | [经典模式](#roles) |
| [小弟 (小)](#mafia) | [工程师](#engineer) | [纵火犯](#arsonist) | [反传送](#anti-teleport) | [赌怪模式](#guesser-modifier) |
| [清洁工 (清)](#mafia) | [警长](#sheriff) | [豺狼](#jackal) | [Tie Breaker](#tie-breaker) | [模组捉迷藏模式](#hide-n-seek) |
| [Morphling](#morphling) | [捕快](#deputy) | [跟班](#sidekick) | [诱饵](#bait) | [原版捉迷藏模式(https://www.innersloth.com/new-game-mode-hide-n-seek-is-here-emergency-meeting-35/) |
| [隐蔽者](#camouflager) | [执灯人](#lighter) | [秃鹫](#vulture) | [恋人](#lovers) |
| [吸血鬼](#vampire) | [医生](#detective) | [律师](#lawyer) | [太阳镜](#sunglasses) |
| [抹除者](#eraser) | [时间之主](#time-master) | [起诉人](#prosecutor) | [小孩](#mini) |
| [骗术师](#trickster) | [法医](#medic) | [追击者](#pursuer) | [会员](#vip) |
| [清理者](#cleaner) | [换票师](#swapper) | [窃贼](#thief) | [醉鬼](#invert) |
| [术士](#warlock) | [灵媒](#seer) |  | [变色龙](#chameleon) |
| [赏金猎人](#bounty-hunter) | [黑客](#hacker) |  | [交换师](#shifter)
| [女巫](#witch) | [追踪者](#tracker) |  |  |
| [忍者](#ninja) | [告密者](#snitch) |  |  |
| [爆破手](#bomber) | [卧底](#spy) |  |  |
| [邪恶赌怪](#guesser) | [传送门师](#portalmaker) |  |  |
|  | [保安](#security-guard) |  |  |
|  | [通灵师](#medium) |  |  |
|  | [陷阱师](#trapper) |  |  |
|  | [正义赌怪](#guesser) |  |  |

[职业分配](#role-assignment) 解释了职业是如何在玩家之间分配的。

# 发布
| Among Us - 版本| 模组版本 | Link |
|----------|-------------|-----------------|
| 2023.07.12s| v1.0.0| [Download](https://github.com/MrFangkuai/TheOtherRolesAF/release)

# 安装

## Windows 安装 Steam
1. 下载最新版 [release](https://github.com/MrFangkuai/TheOtherRolesAF/releases/latest)
2.找出你游戏的文件夹。您可以在您的Steam库中右键单击游戏，将出现一个菜单，单击属性、本地数据、浏览。
3.返回一个文件夹到公共，并制作一个副本，你的游戏文件夹，并粘贴在同一目录的某个地方。
4.现在解压缩并将文件从.zip中拖动或解压缩到您刚才复制的“.exe”级别的游戏文件夹中(就在文件夹中)。
5.通过从这个文件夹启动.exe来运行游戏(第一次启动可能需要一段时间)。

不工作吗?您可能想要安装依赖项 [vc_redist](https://aka.ms/vs/16/release/vc_redist.x86.exe)

## Epic安装
1. 下载模组 [release](https://github.com/MrFangkuai/TheOtherRolesAF/releases/latest)
2. 找到你的游戏文件夹。应该存储在"Epic Games/AmongUs"(保证你在电脑上安装了Epic)
 3.现在解压缩并将文件从.zip文件中拖拽或提取到原始的Epic Among Us游戏文件夹中。 
 4. 通过在Epic Games启动器中启动游戏来运行游戏(第一次启动可能需要一段时间)。

不工作？你可能需要安装依赖项[vc_redist](https://aka.ms/vs/16/release/vc_redist.x86.exe)

## Linux 安装
1. 通过Steam安装《Among Us》 2. 下载最新的[版本](https://github.com/MrFangkuai/TheOtherRolesAF/releases/latest)并将其解压缩到steam/steam/steamapps /common/Among Us 
3.通过winecfg (https://docs.bepinex.dev/articles/advanced/proton_wine.html)启用http.dll
 4. 通过Steam发行游戏

自定义服务器
自定义服务器不是必要的，官方服务器与mod一起工作很好，但如果你想设置和托管自己的服务器，这里有一个指南供你遵循

**设置服务器:**
1. 获取Impostor版本(https://github.com/Impostor/Impostor)
2. 按照官方Impostor-Documentation (https://github.com/Impostor/Impostor/wiki/Running-the-server)上的步骤操作(使用您刚刚下载的服务器版本)。
3.确保在配置中将以下值设置为false。json文件:
```    ...
     'AntiCheat': {
       'Enabled': false,
      'BanIpFromGame': false
    }
```
4. 确保在主机上转发正确的端口。
5. 运行服务器并设置客户端。
将服务器设置为Docker容器:
如果您希望将服务器作为docker容器运行，则需要使用映像
aeonlucid/impostor:nightly
(目前只有“nightly”标签启动支持2021.3.31或更高版本的服务器)
除了运行它，我们还需要设置环境变量来禁用AntiCheat功能。
IMPOSTOR_AntiCheatEnabled=false
IMPOSTOR_AntiCheatBanIpFromGame=false

使用实例对docker运行命令:
docker运行-p 22023:22023/udp——env IMPOSTOR_AntiCheatEnabled=false——env IMPOSTOR_AntiCheatBanIpFromGame=false aeonlucid/impostor:nightly

或者在后台运行
docker运行-d -p 22023:22023/udp——env IMPOSTOR_AntiCheatEnabled=false——env IMPOSTOR_AntiCheatBanIpFromGame=false aeonlucid/impostor:nightly

**如果您对自定义服务器有任何问题，请联系https://github.com/Impostor/Impostor或https://discord.gg/ThJUGAsz**


# 贡献和资源
[OxygenFilter](https://github.com/NuclearPowered/Reactor.OxygenFilter) - 对于v2.3.0到v2.6.1之间的所有版本，我们都使用OxygenFilter来自动解除混淆
[Reactor](https://github.com/NuclearPowered/Reactor) - v2.0.0之前的所有版本使用的框架
[BepInEx](https://github.com/BepInEx) - 用于挂钩到游戏
[Essentials](https://github.com/DorCoMaNdO/Reactor-Essentials) - **DorCoMaNdO**自定义游戏选项:在v1.6之前:我们使用默认的Essentials版本
- v1.6-v1.8:我们稍微改变了默认的Essentials版本。这些变化可以在我们的这个[分支](https://github.com/Eisbison/Reactor-Essentials/tree/feature/TheOtherRoles-Adaption)上找到。
- v2.0.0及以后:我们不再使用反应器了，我们正在使用我们自己的实现，灵感来自**DorCoMaNdO**
[豺狼和跟班](https://www.twitch.tv/dhalucard) - 豺狼和跟班的最初想法来自于Dhalucard
[恋人](https://github.com/Woodi-dev/Among-Us-Love-Couple-Mod) ——恋人的想法来自* * Woodi-dev * * 
[小丑](https://github.com/Maartii/Jester) - 最初想法来自 **Maartii**\
[ExtraRolesAmongUs](https://github.com/NotHunter101/ExtraRolesAmongUs) - 工程师和医疗角色的想法来自**NotHunter101**Also some code 还使用了它们实现中的一些代码片段
[警长](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod) - 最初想法来自**Woodi-dev**
[TooManyRolesMods](https://github.com/Hardel-DW/TooManyRolesMods) -侦探和时间之主这两个角色的创意来自于 **Hardel-DW**. 还使用了它们实现中的一些代码片段
[我们的小镇](https://github.com/slushiegoose/Town-Of-Us) -换票师，交换师，纵火犯和市长职业来自**Slushiegoose**\
[Ottomated](https://twitter.com/ottomated_) - 化形者 告密者 伪装者的想法来自**Ottomated**\
[Crowded-Mod](https://github.com/CrowdedMods/CrowdedMod) - 我们对10+玩家大厅的执行受到了来自**Crowded Mod 团队的启发
[Goose-Goose-Duck](https://store.steampowered.com/app/1568590/Goose_Goose_Duck) - 秃鹫职业来源于**Slushiegoose**\
[TheEpicRoles](https://github.com/LaicosVK/TheEpicRoles) -第一个被杀获得盾和选项菜单(完全+一些代码)的想法，由**LaicosVK** **DasMonschta** **Nova**\启发
[忍者](#ninja), [Thief](#thief), [律师](#lawyer) / [追击者](#pursuer), [捕快](#deputy), [传送门师](#portalmaker), [赌怪模式](#guesser-modifier) -创意:[K3ndo](https://github.com/K3ndoo);由[Gendelo](https://github.com/gendelo3)和[Mallöris](https://github.com/Mallaris)开发
右上角延迟汉化灵感来源于[TownOfUs-R](https://github.com/eDonnes124/Town-Of-Us-R)的[PingTrackerUpdate](https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/PingTrackerUpdate.cs)

# 设置
模组增加了一些新的设置在我们之间(除了职业设置):
- **船员数量:**可以在大厅内设置船员职业的数量。
- **填充船员角色(忽略最小/最大):**每个人都会得到一个职业，即使设置说会有普通的船员(需要足够的角色超过0%)。
- **中立的数量:**中立的职业数量可以在大厅内设置。
- **内鬼数量:**可以在大厅内设置内鬼职业的数量。
- **附加的数量:附加的数量可以在大厅内设置。
- **地图:**地图可以在大厅内改变。
- **会议的最大数量:**您可以设置会议的最大数量
- **允许跳过紧急会议:**如果设置为关闭，紧急会议中将没有跳过按钮。如果玩家不投票，他们会自己投票。
- **隐藏玩家姓名:**隐藏所有玩家的姓名，其中职业是未知的你。团队爱好者/内鬼/豺狼仍然可以看到队友的名字。内鬼也能看到Spy的名字，每个人都能看到mini的年龄。
- **允许一起扫描:**允许玩家在同一时间执行扫描。
- **最后一场比赛第一击杀**前一轮的第一个被杀的玩家将被屏蔽所有玩家可见，直到第一次会议。
- **在困扰或缩小之前完成任务**缩小功能以及困扰将为玩家隐藏，直到他们的所有任务完成
- **管理表显示尸体**
- **当灯关闭时，摄像头切换到夜视状态**当灯关闭时，摄像头上看不到任何颜色和皮肤。小孩可以被发现!
- **内鬼视觉忽略夜视摄像头**
- **在随机地图上播放**如果启用它允许您设置每个当前地图的百分比，除了ehT dks。
- **鬼魂可以看到职业
- **鬼能看到选票
-幽灵可以看到附加
- **幽灵看到任务和其他信息**其他信息:像谁被诅咒，手铐等，但也医疗盾，闪光，工程师修复等。
- **在会议期间，地图是可访问的，并将显示你的最后一个位置，当一个机构得到报告/会议被调用
- **当你是一个鬼和完成任务，你会得到一个缩小/概述功能
- **任务计数:**你现在可以选择更多的任务。
- **角色总结:**当游戏结束时，会有一个所有玩家的列表，他们的职业，他们的任务进度以及一个杀手杀死了多少玩家。
- **暗/亮:**显示会议中每个玩家的颜色类型。
- **在游戏中显示设置**您可以使用位于地图按钮下方的按钮或f1键来打开或关闭大厅设置的显示

### 每个地图的任务计数限制
您可以配置:
—普通任务最多4个
—最多23个短任务
-最多15个长任务

请注意，如果配置的选项超过映射的可用任务数，则任务将被限制为该任务数。＼
例如:如果你在飞艇上配置了4个普通任务，船员将只会收到2个普通任务，因为飞艇不会提供超过2个普通任务。

| 地图| 常见任务 | 短任务 | 长任务 |
|----------|:-------------:|:-------------:|:-------------:|
| Skeld / Dleks | 2 | 19 | 8
| Mira HQ | 2 | 13 | 11
| Polus | 4 | 14 | 15
| Airship | 2 | 23 | 15
-----------------------

# Roles

## Role Assignment

First you need to choose how many special roles of each kind (Impostor/Neutral/Crewmate) you want in the game.
The count you set will only be reached, if there are enough Crewmates/Impostors in the game and if enough roles are set to be in the game (i.e. they are set to > 0%). The roles are then being distributed as follows:
- First all roles that are set to 100% are being assigned to arbitrary players.
- After that each role that has 10%-90% selected adds 1-9 tickets to a ticket pool (there exists a ticket pool for Crewmates, Neutrals and Impostors). Then the roles will be selected randomly from the pools as long it's possible (until the selected number is reached, until there are no more Crewmates/Impostors or until there are no more tickets). If a role is selected from the pool, obviously all the tickets of that role are being removed.
- The Mafia, Lovers and Mini are being selected independently (without using the ticket system) according to the spawn chance you selected. After that the Crewmate, Neutral and Impostor roles are selected and assigned in a random order.

**Example:**\
Settings: 2 special Crewmate roles, Snitch: 100%, Hacker: 10%, Tracker: 30%\
Result: Snitch is assigned, then one role out of the pool [Hacker, Tracker, Tracker, Tracker] is being selected\
Note: Changing the settings to Hacker: 20%, Tracker: 60% would statistically result in the same outcome .


## Mafia
### **Team: Impostors**
The Mafia are a group of three Impostors.\
The Godfather works like a normal Impostor.\
The Mafioso is an Impostor who cannot kill until the Godfather is dead.\
The Janitor is an Impostor who cannot kill, but they can hide dead bodies instead.\
\
**NOTE:**
- There have to be 3 Impostors activated for the mafia to spawn.

### Game Options
| Name | Description |
|----------|:-------------:|
| Mafia Spawn Chance | -
| Janitor Cooldown | -
-----------------------

## Morphling
### **Team: Impostors**
The Morphling is an Impostor which can additionally scan the appearance of a player. After an arbitrary time they can take on that appearance for 10s.\
\
**NOTE:**
- They shrink to the size of the Mini when they copy its look.
- The Hacker sees the new color on the admin table.
- The color of the footprints changes accordingly (also the ones that were already on the ground).
- The other Impostor still sees that they are an Impostor (the name remains red).
- The shield indicator changes accordingly (the Morphling gains or loses the shield indicator).
- Tracker arrows keep working.

### Game Options
| Name | Description |
|----------|:-------------:|
| Morphling Spawn Chance | -
| Morphling Cooldown | -
| Morph Duration | Time the Morphling stays morphed
-----------------------

## Camouflager
### **Team: Impostors**
The Camouflager is an Impostor which can additionally activate a camouflage mode.\
The camouflage mode lasts for 10s and while it is active, all player names/pets/hats\
are hidden and all players have the same color.\
\
**NOTE:**
- The Mini will look like all the other players
- The color of the footprints turns gray (also the ones that were already on the ground).
- The Hacker sees gray icons on the admin table
- The shield is not visible anymore
- Tracker arrows keep working

### Game Options
| Name | Description |
|----------|:-------------:|
| Camouflager Spawn Chance | -
| Camouflager Cooldown | -
| Camo Duration | Time players stay camouflaged
-----------------------

## Vampire
### **Team: Impostors**
The Vampire is an Impostor, that can bite other player. Bitten players die after a configurable amount of time.\
If the Vampire spawn chance is greater 0 (even if there is no Vampire in the game), all players can place one garlic.\
If a victim is near a garlic, the "Bite Button" turns into the default "Kill Button" and the Vampire can only perform a normal kill.\
\
**NOTE:**
- If a bitten player is still alive when a meeting is being called, they die at the start of the meeting.
- The cooldown is the same as the default kill cooldown (+ the kill delay if the Vampire bites the target).
- If there is a Vampire in the game, there can't be a Warlock.
- If the Vampire bites a player and the Thief kills the Vampire, the bite will still be performed, but the new Vampire will be displayed in the kill animation.
- If the Vampire bites a player and gets killed before the bite is performed, the biten player will survive.

### Game Options
| Name | Description |
|----------|:-------------:|
| Vampire Spawn Chance | -
| Vampire Kill Delay | -
| Vampire Cooldown | Sets the kill/bite cooldown
| Vampire Can Kill Near Garlics | The Vampire can never bite when their victim is near a garlic. If this option is set to true, they can still perform a normal kill there.
-----------------------

## Eraser
### **Team: Impostors**
The Eraser is an Impostor that can erase the role of every player.\
The targeted players will lose their role after the meeting right before a player is exiled.\
After every erase, the cooldown increases by 10 seconds.\
The erase will be performed, even if the Eraser or their target die before the next meeting.\
By default the Eraser can erase everyone but the Spy and other Impostors.\
Depending on the options they can also erase them (Impostors will lose their special Impostor ability).\
\
**NOTE:**
- The Shifter shift will always be triggered before the Erase (hence either the new role of the Shifter will be erased or the Shifter saves the role of their target, depending on whom the Eraser erased).
- As the erase is being triggered before the ejection of a player, a Jester win would not happen, as the erase will be triggered before.
- Modifier will not be erased.

### Game Options
| Name | Description |
|----------|:-------------:|
| Eraser Spawn Chance | -
| Eraser Cooldown | The Eraser's cooldown will increase by 10 seconds after every erase.
| Eraser Can Erase Anyone | If set to false, they can't erase the Spy and other Impostors
-----------------------

## Trickster
### **Team: Impostors**
The Trickster is an Impostor that can place 3 jack-in-the-boxes that are invisible at first to other players.\
If the Trickster has placed all of their boxes they will be converted into a vent network usable only by the Trickster themself, but the boxes are revealed to the others.\
If the boxes are converted to a vent network, the Trickster gains a new ability "Lights out" to limit the visibility of Non-Impostors, that cannot be fixed by other players. Lights are automatically restored after a while.\
\
**NOTE:**
- Impostors will get a text indicator at the bottom of the screen to notify them if the lights are out due to the Trickster ability, as there is no sabotage arrows or task to sabotage text to otherwise notify them about it.

### Game Options
| Name | Description |
|----------|:-------------:|
| Trickster Spawn Chance | -
| Trickster Box Cooldown | Cooldown for placing jack-in-the-boxes
| Trickster Lights Out Cooldown | Cooldown for their "lights out" ability
| Trickster Lights Out Duration | Duration after which the light is automatically restored
-----------------------

## Cleaner
### **Team: Impostors**
The Cleaner is an Impostor who has the ability to clean up dead bodies.\
\
**NOTE:**
- The Kill and Clean cooldown are shared, preventing them from immediately cleaning their own kills.
- If there is a Cleaner in the game, there can't be a Vulture.

### Game Options
| Name | Description |
|----------|:-------------:|
| Cleaner Spawn Chance | -
| Cleaner Cooldown | Cooldown for cleaning dead bodies
-----------------------

## Warlock
### **Team: Impostors**
The Warlock is an Impostor, that can curse another player (the cursed player doesn't get notified).\
If the cursed person stands next to another player, the Warlock is able to kill that player (no matter how far away they are).\
Performing a kill with the help of a cursed player, will lift the curse and it will result in the Warlock being unable to move for a configurable amount of time.\
The Warlock can still perform normal kills, but the two buttons share the same cooldown.\
\
**NOTE:**
- The Warlock can always kill their Impostor mates (and even themself) using the "cursed kill"
- If there is a Warlock in the game, there can't be a Vampire
- Performing a normal kill, doesn't lift the curse

### Game Options
| Name | Description |
|----------|:-------------:|
| Warlock Spawn Chance | -
| Warlock Cooldown | Cooldown for using the Curse and curse Kill
| Warlock Root Time | Time the Warlock is rooted in place after killing using the curse
-----------------------

## Bounty Hunter
### **Team: Impostors**
The Bounty Hunter is an Impostor, that continuously get bounties (the targeted player doesn't get notified).\
The target of the Bounty Hunter swaps after every meeting and after a configurable amount of time.\
If the Bounty Hunter kills their target, their kill cooldown will be a lot less than usual.\
Killing a player that's not their current target results in an increased kill cooldown.\
Depending on the options, there'll be an arrow pointing towards the current target.\
\
**NOTE:**
- The target won't be an Impostor, a Spy or the Bounty Hunter's Lover.
- Killing the target resets the timer and a new target will be selected.

### Game Options
| Name | Description |
|----------|:-------------:|
| Bounty Hunter Spawn Chance | -
| Duration After Which Bounty Changes | -
| Cooldown After Killing Bounty | -
| Additional Cooldown After Killing Others | Time will be added to the normal impostor cooldown if the Bounty Hunter kills a not-bounty player
| Show Arrow Pointing Towards The Bounty | If set to true an arrow will appear (only visiable for the Bounty Hunter)
| Bounty Hunter Arrow Update Interval | Sets how often the position is being updated
-----------------------

## Witch
### **Team: Impostors**
The Witch is an Impostor who has the ability to cast a spell on other players.\
During the next meeting, the spellbound player will be highlighted and they'll die right after the meeting.\
There are multiple options listed down below with which you can configure to fit your taste.\
Similar to the Vampire, shields and blanks will be checked twice (at the end of casting the spell on the player and at the end of the meeting, when the spell will be activated).\
This can result in players being marked as spelled during the meeting, but not dying in the end (when they get a shield or the Witch gets blanked after they were spelled by the Witch).\
If the Witch dies before the meeting starts or if the Witch is being guessed during the meeting, the spellbound players will be highlighted but they'll survive in any case.\
Depending on the options you can choose whether voting the Witch out will save all the spellbound players or not.\
\
**NOTE:**
- The spellbound players will die before the voted player dies (which might trigger e.g. trigger an Impostor win condition, even if the Witch is the one being voted)

### Game Options
| Name | Description |
|----------|:-------------:|
| Witch Spawn Chance | -
| Witch Spell Casting Cooldown | -
| Witch Additional Cooldown | The spell casting cooldown will be increased by the amount you set here after each spell
| Witch Can Spell Everyone | If set to false, the witch can't spell the Spy and other Impostors
| Witch Spell Casting Duration | The time that you need to stay next to the target in order to cast a spell on it
| Trigger Both Cooldowns | If set to true, casting a spell will also trigger cooldown of the kill button and vice versa (but the two cooldowns may vary)
| Voting The Witch Saves All The Targets | If set to true, all the cursed targets will survive at the end of the meeting
-----------------------

## Ninja
### **Team: Impostors**
The Ninja is an Impostor who has the ability to kill another player all over the map.\
You can mark a player with your ability and by using the ability again, you jump to the position of the marked player and kill it.\
Depending on the options you know where your marked player is.\
If the Ninja uses its ability, it will leave a trace (leaves) for a configurable amount of time where it activated the ability and additionally where it killed the before marked player.\
When performing a ninja ability kill, the ninja can be invisible for some seconds (depends on options)\
\
**NOTE:**
- The Ninja has a 5 second cooldown after marking a player
- The trace has a darker (black) or lighter (white) color depending on the players color that will fade into green
- The mark on the marked player will reset after a meeting or after using the ability to kill the marked player. Performing a normal kill will **NOT** reset the mark
- If the Ninja tries to kill a shielded player (e.g. Medic shield, Shield last game first kill ), the kill will not be performed
- If the Ninja tries to kill the Time Master while the shield is active, the Ninja won't teleport to the players position, but the Time Master shield will still be activated
- If the marked target is on a different floor on Submerged, the arrow will always point to the elevator

### Game Options
| Name | Description |
|----------|:-------------:|
| Ninja Spawn Chance | -
| Ninja Mark Cooldown | -
| Ninja Knows Location Of Target | -
| Trace Duration | -
| Time Till Trace Color Has Faded | -
| Time The Ninja Is Invisible | -
-----------------------

## Bomber
### **Team: Impostors**
The Bomber is an Impostor who has the ability to be very explosive. They have the ability to plant bombs to spread grouping Crewmates and also kill them.\
The Bomber also has the ability to perform a normal kill like all Impostors.\
The plant time of the bomb can be different to the kill cooldown depending on the settings.\
Crewmates can defuse a bomb depending on the settings.

**NOTE:**
- The bomb won't kill a shielded player (Medic, First Kill Shield)
- The bomb won't kill a Mini until it's grown up
- The bomb can kill the bomber as well as their teammate(s)
- The hearing range can be higher/lower than the actual destruction range, depending on the settings, and has a visual indicator
- The visual indicator slowly fades into red until the bomb explodes and it does not show the explosion range (only hearing range)!
- The bomb can be defused by standing on it and snipping the fuse (button)

### Game Options
| Name | Description |
|----------|:-------------:|
| Bomber Spawn Chance | -
| Bomb Destruction Time | -
| Bomb Destruction Range | -
| Bomb Hear Range | -
| Bomb Defuse Duration | -
| Bomb Cooldown | -
| Bomb Is Active After |
-----------------------

## Guesser
### **Team: Crewmates or Impostors**
The Guesser can be a Crewmate or an Impostor (depending on the settings).\
The Guesser can shoot players during the meeting, by guessing its role. If the guess is wrong, the Guesser dies instead.\
You can select how many players can be shot per game and if multiple players can be shot during a single meeting.\
The guesses Impostor and Crewmate are only right, if the player is part of the corresponding team and has no special role.\
You can only shoot during the voting time.\
Depending on the options, the Guesser can't guess the shielded player and depending on the Medic options the Medic/shielded player might be notified (no one will die, independently of what the Guesser guessed).\
\
**NOTE:**
- If a player gets shot, you'll get back your votes
- Jester wins won't be triggered, if the Guesser shoots the Jester before the Jester gets voted out

### Game Options
| Name | Description |
|----------|:-------------:|
| Guesser Spawn Chance | -
| Chance That The Guesser Is An Impostor | -
| Guesser Number Of Shots Per Game | -
| Guesser Can Shoot Multiple Times Per Meeting |  -
| Guesses Visible In Ghost Chat | -
| Guesses Ignore The Medic Shield | -
| Evil Guesser Can Guess The Spy | -
| Both Guesser Spawn Rate | -
| Guesser Can't Guess Snitch When Tasks Completed | -

-----------------------

## Jester
### **Team: Neutral**
The Jester does not have any tasks. They win the game as a solo, if they get voted out during a meeting.

### Game Options
| Name | Description |
|----------|:-------------:|
| Jester Spawn Chance | -
| Jester Can Call Emergency Meeting | Option to disable the emergency button for the Jester
-----------------------

## Arsonist
### **Team: Neutral**
The Arsonist does not have any tasks, they have to win the game as a solo.\
The Arsonist can douse other players by pressing the douse button and remaining next to the player for a few seconds.\
If the player that the Arsonist douses walks out of range, the cooldown will reset to 0.\
After dousing everyone alive the Arsonist can ignite all the players which results in an Arsonist win.

### Game Options
| Name | Description |
|----------|:-------------:|
| Arsonist Spawn Chance | -
| Arsonist Countdown | -
| Arsonist Douse Duration | The time it takes to douse a player
-----------------------

## Jackal
### **Team: Jackal**
The Jackal is part of an extra team, that tries to eliminate all the other players.\
The Jackal has no tasks and can kill Impostors, Crewmates and Neutrals.\
The Jackal (if allowed by the options) can select another player to be their Sidekick.
Creating a Sidekick removes all tasks of the Sidekick and adds them to the team Jackal. The Sidekick loses their current role (except if they're a Lover, then they play in two teams).
The "Create Sidekick Action" may only be used once per Jackal or once per game (depending on the options).
The Jackal can also promote Impostors to be their Sidekick, but depending on the options the Impostor will either really turn into the Sidekick and leave the team Impostors or they will just look like the Sidekick to the Jackal and remain as they were.\
Also if a Spy or Impostor gets sidekicked, they still will appear red to the Impostors.

The team Jackal enables multiple new outcomes of the game, listing some examples here:
- The Impostors could be eliminated and then the crew plays against the team Jackal.
- The Crew could be eliminated, then the Team Jackal fight against the Impostors (The Crew can still make a task win in this scenario)

The priority of the win conditions is the following:
1. Crewmate Mini lose by vote
2. Jester wins by vote
3. Arsonist win
4. Team Impostor wins by sabotage
5. Team Crew wins by tasks (also possible if the whole Crew is dead)
6. Lovers among the last three players win
7. Team Jackal wins by outnumbering (When the team Jackal contains an equal or greater amount of players than the Crew and there are 0 Impostors left and team Jackal contains no Lover)
8. Team Impostor wins by outnumbering (When the team Impostors contains an equal or greater amount of players than the Crew and there are 0 players of the team Jackal left and team Impostors contains no Lover)
9. Team Crew wins by outnumbering (When there is no player of the team Jackal and the team Impostors left)

**NOTE:**
- The Jackal (and their Sidekick) may be killed by a Sheriff.
- A Jackal cannot target the Mini, while it's growing up. After that they can kill it or select it as its Sidekick.
- The Crew can still win, even if all of their members are dead, if they finish their tasks fast enough (That's why converting the last Crewmate with tasks left into a Sidekick results in a task win for the crew.)

If both Impostors and Jackals are in the game, the game continues even if all Crewmates are dead. Crewmates may still win in this case by completing their tasks. Jackal and Impostor have to kill each other.

### Game Options
| Name | Description
|----------|:-------------:|
| Jackal Spawn Chance | - |
| Jackal/Sidekick Kill Cooldown | Kill cooldown |
| Jackal Create Sidekick Cooldown | Cooldown before a Sidekick can be created |
| Jackal can use vents | Yes/No |
| Jackal can create a Sidekick | Yes/No |
| Jackals promoted from Sidekick can create a Sidekick | Yes/No (to prevent the Jackal team from growing) |
| Jackals can make an Impostor to their Sidekick | Yes/No (to prevent a Jackal from turning an Impostor into a Sidekick, if they use the ability on an Impostor they see the Impostor as Sidekick, but the Impostor isn't converted to Sidekick. If this option is set to "No" Jackal and Sidekick can kill each other.) |
| Jackal and Sidekick have Impostor vision | - |
-----------------------

## Sidekick
### **Team: Jackal**
Gets assigned to a player during the game by the "Create Sidekick Action" of the Jackal and joins the Jackal in their quest to eliminate all other players.\
Upon the death of the Jackal (depending on the options), they might get promoted to Jackal themself and potentially even assign a Sidekick of their own.\
\
**NOTE:**
- A player that converts into a Sidekick loses their previous role and tasks (if they had one).
- The Sidekick may be killed by a Sheriff.
- The Sidekick cannot target the Mini, while it's growing up.

### Game Options
| Name | Description
|----------|:-------------:|
| Jackal/Sidekick Kill Cooldown | Uses the same kill cooldown setting as the Jackal |
| Sidekick gets promoted to Jackal on Jackal death |  Yes/No |
| Sidekick can kill | Yes/No |
| Sidekick can use vents | Yes/No |
-----------------------

## Vulture
### **Team: Neutral**
The Vulture does not have any tasks, they have to win the game as a solo.\
The Vulture is a neutral role that must eat a specified number of corpses (depending on the options) in order to win.\
Depending on the options, when a player dies, the Vulture gets an arrow pointing to the corpse.\
If there is a Vulture in the game, there can't be a Cleaner.

**NOTE**
- If the corpse is on a different floor on Submerged, the arrow will always point to the elevator

### Game Options
| Name | Description |
|----------|:-------------:|
| Vulture Spawn Chance | -
| Vulture Countdown | -
| Number Of Corpses Needed To Be Eaten | Corpes needed to be eaten to win the game
| Vulture Can Use Vents | -
| Show Arrows Pointing Towards The Corpes | -
-----------------------

## Lawyer
### **Team: Neutral**
The Lawyer is a neutral role that has a client.\
The client might be an Impostor or Jackal which is no Lover.\
Depending on the options, the client can also be a Jester.\
The Lawyer needs their client to win in order to win the game.\
Their client doesn't know that it is their client.\
If their client gets voted out, the Lawyer dies with the client.\
If their client dies, the Lawyer changes their role and becomes the [Pursuer](#pursuer), which has a different goal to win the game.\
\
How the Lawyer wins:
- Lawyer dead/alive, client alive and client wins: The Lawyer wins together with the team of the client.
- If their client is Jester and the Jester gets voted out, the Lawyer wins together with the Jester.

**NOTE:**
- If the client disconnects, the Lawyer will also turn into the Pursuer.
- The Lawyer needs to figure out the role of their client depending on the options.
- The tasks only count, if the Lawyer gets promoted to Pursuer.
- If the Lawyer dies before their client, they will lose all their tasks and will get the overview immediately.

### Game Options
| Name | Description |
|----------|:-------------:|
| Lawyer Spawn Chance | -
| Chance That The Lawyer Is Prosecutor | -
| Lawyer/Prosecutor Vision | Pursuer has normal vision
| Lawyer/Prosecutor Knows Target Role | -
| Lawyer/Prosecutor Can Call Emergency Meeting | -
| Lawyer Target Can Be The Jester | -
-----------------------

## Prosecutor
### **Team: Neutral**
The Prosecutor is a neutral role that resembles the Lawyer. The Prosecutor has a client who is a Crewmate.\
The Prosecutor needs their client to be voted out in order to win the game.\
The Prosecutor's client doesn't know that they are their client.\
If the client gets sidekicked, the Prosecutor changes their role and becomes the client's [Lawyer](#lawyer) and has to protect the
client from now on.\
If the Prosecutor's client dies, the Prosecutor changes their role and becomes the [Pursuer](#pursuer), which has a different goal to win the game.

**NOTE:**
- The Prosecutor's role settings are shared with the Lawyer settings.
- If the client disconnects, the Prosecutor will also turn into the Pursuer.
- The Prosecutor needs to figure out the role of their client depending on the options.
- The tasks only count, if the Prosecutor gets promoted to Pursuer.
- If the Prosecutor dies before their client, they will lose all their tasks and will get the overview immediately.

## Pursuer
### **Team: Neutral**
The Pursuer is still a neutral role, but has a different goal to win the game; they have to be alive when the game ends and the Crew wins.\
In order to achieve this goal, the Pursuer has an ability called "Blank", where they can fill a killer's (this also includes the Sheriff) weapon with a blank. So, if the killer attempts to kill someone, the killer will miss their target, and their cooldowns will be triggered as usual.\
If the killer fires the "Blank", shields (e.g. Medic shield or Time Master shield) will not be triggered.\
The Pursuer has tasks (which can already be done while being a Lawyer/Prosecutor), that count towards the task win for the Crewmates. If the Pursuer dies, their tasks won't be counted anymore.

### Game Options
| Name | Description |
|----------|:-------------:|
| Pursuer Blank Cooldown | -
| Pursuer Number Of Blanks | -
-----------------------

## Thief
### **Team: Neutral**
The Thief has to kill another killer (Impostor, Jackal/Sidekick and if enabled Sheriff)
in order to have a win condition.\
If the Thief doesn't kill another killer they will lose the game.\
If the Thief kills one of the other killers, the Thief overtakes their role (e.g. Ninja) and joins their team (in this case
team Impostor). They then have the new Role's win condition (e.g. Impostor-Win).\
If the Thief tries to kill any non-killing role (Crewmate or Neutral), they die similar to a misfiring Sheriff.\
\
**NOTE**
- If the option "Thief Can Kill Sheriff" is On, the Thief has tasks which will ONLY begin to count, if they kill
the Sheriff. While the Thief hasn't fired, their tasks do not count towards the taskwin.
- If the option "Thief Can Kill Sheriff" is Off, the Thief will not have tasks. 
- If the Thief kills the witch, already witched players stay witched (except for the Thief).
- If the Thief can guess to steal the role, guessing the witch will either save all targets or none of the targets (depending on the setting for Witch: Voting The Witch Saves All The Targets)

### Game Options
| Name | Description |
|----------|:-------------:|
| Thief Spawn Chance | -
| Thief Countdown | -
| Thief Can Kill Sheriff | -
| Thief Has Impostor Vision | -
| Thief Can Use Vents | -
| Thief Can Guess To Steal Role | -
-----------------------

## Mayor
### **Team: Crewmates**
The Mayor leads the Crewmates by having a vote that counts twice.\
The Mayor can always use their meeting, even if the maximum number of meetings was reached.\
The Mayor has a portable Meeting Button, depending on the options.\
The Mayor can see the vote colors after completing a configurable amount of tasks, depending on the options.\
The Mayor has the option to vote with only one vote instead of two (via a button in the meeting screen), depending on the settings.

### Game Options
| Name | Description |
|----------|:-------------:|
| Mayor Spawn Chance | -
| Mayor Can See Vote Colors | -
| Completed Tasks Needed To See Vote Colors | -
| Mobile Emergency Button | -
| Mayor Can Choose Single Vote | Off, On (Before Voting), On (Until Meeting Ends)
-----------------------

## Engineer
### **Team: Crewmates**
The Engineer (if alive) can fix a certain amount of sabotages per game from anywhere on the map.\
The Engineer can use vents.\
If the Engineer is inside a vent, depending on the options the members of the team Jackal/Impostors will see a blue outline around all vents on the map (in order to warn them).\
Because of the vents the Engineer might not be able to start some tasks using the "Use" button, you can double-click on the tasks instead.

**NOTE:**
- The kill button of Impostors activates if they stand next to a vent where the Engineer is. They can also kill them there. No other action (e.g. Morphling sample, Shifter shift, ...) can affect players inside vents.

### Game Options
| Name | Description |
|----------|:-------------:|
| Engineer Spawn Chance | -
| Number Of Sabotage Fixes| -
| Impostors See Vents Highlighted | -
| Jackal and Sidekick See Vents Highlighted | -
-----------------------

## Sheriff
### **Team: Crewmates**
The Sheriff has the ability to kill Impostors or Neutral roles if enabled.\
If they try to kill a Crewmate, they die instead.\
\
**NOTE:**
- If the Sheriff shoots the person the Medic shielded, the Sheriff and the shielded person **both remain unharmed**.
- If the Sheriff shoots a Mini Impostor while growing up, nothing happens. If it's fully grown, the Mini Impostor dies.

### Game Options
| Name | Description |
|----------|:-------------:|
| Sheriff Spawn Chance | -
| Sheriff Cooldown | -
| Sheriff Can Kill Neutrals | -
| Sheriff Has A Deputy | Deputy can not be in game without Sheriff
-----------------------

## Deputy
### **Team: Crewmates**
The Deputy has the ability to handcuff player.\
Handcuffs will be hidden until the handcuffed player try to use a disabled button/hotkey.\
Handcuffs disable:
- Kill
- Abilities
- Vent
- Report\
\
**NOTE:**
- Duration starts after the handcuffs become visible.
- Deputy can not be in game without Sheriff.

### Game Options
| Name | Description |
|----------|:-------------:|
| Deputy Number Of Handcuffs | -
| Handcuff Cooldown| -
| Handcuff Duration | -
| Sheriff And Deputy Know Each Other | -
| Deputy Gets Promoted To Sheriff | "Off", "On (Immediately)" or "On (After Meeting)"
| Deputy Keeps Handcuffs When Promoted |-
-----------------------

## Lighter
### **Team: Crewmates**
The Lighter has a different vision than everyone else depending on the settings.\
Their vision looks like a flashlight cone which can be moved around (known from the Hide'n'Seek mode).

### Game Options
| Name | Description |
|----------|:-------------:|
| Lighter Spawn Chance | -
| Vision On Lights On | The vision the Lighter has when the lights are on
| Vision On Lights Off | The vision the Lighter has when the lights are down
| Flashlight Width | -
-----------------------

## Detective
### **Team: Crewmates**
The Detective can see footprints that other players leave behind.\
The Detective's other feature shows when they report a corpse: they receive clues about the killer's identity. The type of information they get is based on the time it took them to find the corpse.

**NOTE:**
- When people change their colors (because of a morph or camouflage), all the footprints also change their colors (also the ones that were already on the ground). If the effects are over, all footprints switch back to the original color.
- The Detective does not see footprints of players that sit in vents
- More information about the [colors](#colors)

### Game Options
| Name | Description |
|----------|:-------------:|
| Detective Spawn Chance | -
| Anonymous Footprints | If set to true, all footprints will have the same color. Otherwise they will have the color of the respective player.
| Footprint Interval | The interval between two footprints
| Footprint Duration | Sets how long the footprints remain visible.
| Time Where Detective Reports Will Have Name | The amount of time that the Detective will have to report the body since death to get the killer's name.  |
| Time Where Detective Reports Will Have Color Type| The amount of time that the Detective will have to report the body since death to get the killer's color type. |
-----------------------

## Time Master
### **Team: Crewmates**
The Time Master has a time shield which they can activate. The time shield remains active for a configurable amount of time.\
If a player tries to kill the Time Master while the time shield is active, the kill won't happen and the
time will rewind for a set amount of time.\
The kill cooldown of the killer won't be reset, so the Time Master
has to make sure that the game won't result in the same situation.\
The Time Master won't be affected by the rewind.

**NOTE:**
- Only the movement is affected by the rewind.
- A Vampire bite will trigger the rewind. If the Time Master misses shielding the bite, they can still shield the kill which happens a few seconds later.
- If the Time Master was bitten and has their shield active before when a meeting is called, they survive but the time won't be rewound.
- If the Time Master has a Medic shield, they won't rewind.
- The shield itself ends immediately when triggered. So the Time Master can be attacked again as soon as the rewind ends.

### Game Options
| Name | Description |
|----------|:-------------:|
| Time Master Spawn Chance | - |
| Time Master Cooldown | - |
| Rewind Duration | How much time to rewind |
| Time Master Shield Duration |
-----------------------

## Medic
### **Team: Crewmates**
The Medic can shield (highlighted by an outline around the player) one player per game, which makes the player unkillable.\
The shielded player can still be voted out and might also be an Impostor.\
If set in the options, the shielded player and/or the Medic will get a red flash on their screen if someone (Impostor, Sheriff, ...) tried to murder them.\
If the Medic dies, the shield disappears with them.\
The Sheriff will not die if they try to kill a shielded Crewmate and won't perform a kill if they try to kill a shielded Impostor.\
Depending on the options, guesses from the Guesser will be blocked by the shield and the shielded player/medic might be notified.\
The Medic's other feature shows when they report a corpse: they will see how long ago the player died.

**NOTE:**
- If the shielded player is a Lover and the other Lover dies, they nevertheless kill themselves.
- If the Shifter has a shield or their target has a Shield, the shielded player switches.
- Shields set after the next meeting, will be set before a possible shift is being performed.

### Game Options
| Name | Description | Options |
|----------|:-------------:|:-------------:|
| Medic Spawn Chance | - | -
| Show Shielded Player | Sets who sees if a player has a shield | "Everyone", "Shielded + Medic", "Medic"
| Shielded Player Sees Murder Attempt| Whether a shielded player sees if someone tries to kill them | True/false |
| Shield Will Be Activated | Sets when the shield will be active | "Instantly", "Instantly, Visible After Meeting", "After Meeting"
| Medic Sees Murder Attempt On Shielded Player | - | If anyone tries to harm the shielded player (Impostor, Sheriff, Guesser, ...), the Medic will see a red flash
-----------------------

## Swapper
### **Team: Crewmates**
During meetings the Swapper can exchange votes that two people get (i.e. all votes
that player A got will be given to player B and vice versa).\
Because of the Swapper's strength in meetings, they might not start emergency meetings and can't fix lights and comms.\
The Swapper now has initial swap charges and can recharge those charges after completing a configurable amount of tasks.\
\
**NOTE:**
- The remaining charges will be displayed in brackets next to the players role while not in a meeting
- In a meeting the charges will appear next to the Confirm Swap button

### Game Options
| Name | Description
|----------|:-------------:|
| Swapper Spawn Chance | -
| Swapper can call emergency meeting | Option to disable the emergency button for the Swapper
| Swapper can only swap others | Sets whether the Swapper can swap themself or not
| Initial Swap Charges | -
| Number Of Tasks Needed For Recharging | -
-----------------------

## Seer
### **Team: Crewmates**
The Seer has two abilities (one can activate one of them or both in the options).\
The Seer sees the souls of players that died a round earlier, the souls slowly fade away.\
The Seer gets a blue flash on their screen, if a player dies somewhere on the map.

### Game Options
| Name | Description |
|----------|:-------------:|
| Seer Spawn Chance | -
| Seer Mode | Options: Show death flash and souls, show death flash, show souls
| Seer Limit Soul Duration | Toggle if souls should turn invisible after a while
| Seer Soul Duration | Sets how long it will take the souls to turn invisible after a meeting
-----------------------

## Hacker
### **Team: Crewmates**
If the Hacker activates the "Hacker mode", the Hacker gets more information than others from the admin table and vitals for a set duration.\
Otherwise they see the same information as everyone else.\
**Admin table:** The Hacker can see the colors (or color types) of the players on the table.\
**Vitals**: The Hacker can see how long dead players have been dead for.\
The Hacker can access his mobile gadgets (vitals & admin table), with a maximum of charges (uses) and a configurable amount of tasks needed to recharge.\
While accessing those mobile gadgets, the Hacker is not able to move.

**NOTE:**
- If the Morphling morphs or the Camouflager camouflages, the colors on the admin table change accordingly
- More information about the [colors](#colors)

### Game Options
| Name | Description |
|----------|:-------------:|
| Hacker Spawn Chance | -
| Hacker Cooldown | -
| Hacker Duration | Sets how long the "Hacker mode" remains active
| Hacker Only Sees Color Type | Sets if the Hacker sees the player colors on the admin table or only white/gray (for Lighter and darker colors)
| Max Mobile Gadget Charges | -
| Number Of Tasks Needed For Recharging | Number of tasks to get a charge
| Can't Move During Cam Duration | -
-----------------------

## Tracker
### **Team: Crewmates**
The Tracker can select one player to track. Depending on the options the Tracker can track a different person after each meeting or the Tracker tracks the same person for the whole game.\
An arrow points to the last tracked position of the player.\
The arrow updates its position every few seconds (configurable).\
Depending on the options, the Tracker has another ability: They can track all corpses on the map for a set amount of time. They will keep tracking corpses, even if they were cleaned or eaten by the Vulture.

**NOTE**
- If the tracked player is on a different floor on Submerged, the arrow will always point to the elevator

### Game Options
| Name | Description
|----------|:-------------:|
| Tracker Spawn Chance | -
| Tracker Update Interval | Sets how often the position is being updated
| Tracker Reset Target After Meeting | -
| Tracker Can Track Corpses | -
| Corpses Tracking Cooldown | -
| Corpses Tracking Duration | -
-----------------------

## Snitch
### **Team: Crewmates**
When the Snitch finishes all their tasks, they will get information in chat of the last location of all killers when the meeting starts.\
When the Snitch only has a configurable amount of tasks left, it will be revealed that there is a Snitch in the game with a text on evil player's screens.

**NOTE:**
- If the Snitch dies, all killers will be informed that the Snitch is dead
- Last location can be a room or open field

### Game Options
| Name | Description
|----------|:-------------:|
| Snitch Spawn Chance | -
| Task Count Where The Snitch Will Be Revealed | -
| Information Mode  | Whether the snitch will get info on the map and/or in the chat 
| Targets | Snitch Will See All Evil Players or Killing Evil Players
-----------------------

## Spy
### **Team: Crewmates**
The Spy is a Crewmate, which has no special abilities.\
The Spy looks like an additional Impostor to the Impostors, they can't tell the difference.\
There are two possibilities (depending on the set options):
- The Impostors can't kill the Spy (because otherwise their kill button would reveal, who the Spy is)
- The Impostors can kill the Spy but they can also kill their Impostor partner (if they mistake another Impostor for the Spy)
You can set whether the Sheriff can kill the Spy or not (in order to keep the lie alive).

**NOTE:**
- If the Spy gets sidekicked, it still will appear red to the Impostors.

### Game Options
| Name | Description
|----------|:-------------:|
| Spy Spawn Chance |
| Spy Can Die To Sheriff |
| Impostors Can Kill Anyone If There Is A Spy | This allows the Impostors to kill both the Spy and their Impostor partners
| Spy Can Enter Vents | Allow the Spy to enter/exit vents (but not actually move to connected vents)
| Spy Has Impostor Vision | Give the Spy the same vision as the Impostors have
-----------------------

## Portalmaker
### **Team: Crewmates**
The Portalmaker is a Crewmate that can place two portals on the map.\
These two portals are connected to each other.\
Those portals will be visible after the next meeting and can be used by everyone.\
Additionally to that, the Portalmaker gets information about who used the portals and when in the chat during each meeting, depending on the options.\
The Portalmaker can teleport themself to their placed portals from anywhere if the setting is enabled.

**NOTE:**
- The extra button to use a portal will appear after the Portalmaker set their portals and a meeting/body report was called.
- While one player uses a portal, it is blocked for any other player until the player got teleported.
- All ghosts can still use the portals, but won't block any living player from using it and the Portalmaker won't get any information about it in chat.
- If a morphed person uses a portal it will show the morphed name/color depending on the options.
- If a camouflaged person uses a portal it will show "A comouflaged person used the portal."

### Game Options
| Name | Description
|----------|:-------------:|
| Portalmaker Spawn Chance | -
| Portalmaker Cooldown | -
| Use Portal Cooldown | -
| Portalmaker Log Only Shows Color Type | -
| Log Shows Time | -
| Can Port To Portal From Everywhere | -
-----------------------

## Security Guard
### **Team: Crewmates**
The Security Guard is a Crewmate that has a certain number of screws that they can use for either sealing vents or for placing new cameras.\
Placing a new camera and sealing vents takes a configurable amount of screws. The total number of screws that a Security Guard has can also be configured.\
The new camera will be visible after the next meeting and accessible by everyone.\
The vents will be sealed after the next meeting, players can't enter or exit sealed vents, but they can still "move to them" underground.

**NOTE:**
- Trickster boxes can't be sealed
- The Security Guard can't place cameras on MiraHQ
- The remaining number of screws can be seen above their special button.
- On Skeld the four cameras will be replaced every 3 seconds (with the next four cameras). You can also navigate manually using the arrow keys
- Security Guard can access mobile cameras after placing all screws
- While accessing the mobile cameras, the Security Guard is not able to move

### Game Options
| Name | Description
|----------|:-------------:|
| Security Guard Spawn Chance |
| Security Guard Cooldown |
| Security Guard Number Of Screws | The number of screws that a Security Guard can use in a game
| Number Of Screws Per Cam | The number of screws it takes to place a camera
| Number Of Screws Per Vent | The number of screws it takes to seal a vent
| Security Guard Duration | -
| Gadget Max Charges | -
| Number Of Tasks Needed For Recharging | -
| Can't Move During Cam Duration | -
-----------------------

## Medium
### **Team: Crewmates**
The medium is a crewmate who can ask the souls of dead players for information. Like the Seer, the medium will see the souls of the players who have died (after the next meeting) and can question them. They then gets random information about the soul or the killer in the chat. The souls only stay for one round, i.e. until the next meeting. Depending on the options, the souls can only be questioned once and then disappear.
\
**Questions:**
The souls will always prefer to answer with the role specific information first!

**Role specific:**
- Sheriff suicide: "Yikes, that Sheriff shot backfired."
- Thief suicide: "I tried to steal the gun from their pocket, but they were just happy to see me."
- Active Lover dies: "I wanted to get out of this toxic relationship anyways."
- Passiv Lover suicide: "The love of my life died, thus with a kiss I die."
- Lawyer client kills Lawyer: "My client killed me. Do I still get paid?"
- Teamkill Jackal/Sidekick: "First they sidekicked me, then they killed me... BUT WHY?"
- Teamkill Impostor: "I guess they confused me for the Spy, is there even one?"
- Submerged o2-Kill: "Do I really need that mask for breathing?"
- Warlock self kill: "MAYBE I cursed the person next to me and killed myself. Oops."
- Vulture/Cleaner eats/cleans body: "Is my dead body some kind of art now or... aaand it's gone."

**Else random:**
- "I'm not sure, but I guess a darker/lighter color killed me."
- "If I counted correctly, then I died x seconds before the meeting started."
- "If my role hasn't been saved, there's no (role) in the game anymore."
- "It seems like my killer was the (role)."

**Chance That The Answer Contains Additional Information:**
- When you asked, x killer(s) was/were still alive.
- When you asked, x player(s) who can use vents was/were still alive.
- When you asked, x player(s) who is/are neutral but cannot kill was/were still alive.

### Game Options
| Name | Description
|----------|:-------------:|
| Medium Spawn Chance | -
| Medium Cooldown | -
| Medium Duration | The time it takes to question a soul
| Medium Each Soul Can Only Be Questioned Once | If set to true, souls can only be questioned once and then disappear
| Chance That The Answer Contains The Remaining Amount Of Killing Roles | Chance includes Sheriff and Thief
-----------------------

## Trapper
### **Team: Crewmates**
The Trapper is a crewmate which can set up traps to trap player and gain information from them.\
The traps will stuck player for x-seconds (depends on the setting) and reveal information in chat
about their "Role", if they are a "Good/Evil Role" or their "Name".\
The trap is not visible until a configurable amount of player were trapped.\
When the trap gets visible, the Trapper will gain the information in chat (in a random order).\
If a trap is triggered (and the option is enabled), the map of the Trapper will open up and show which trap
was triggered.\
The traps have a maximum of charges (uses) and a configurable amount of tasks are needed to recharge.\
\
**NOTE:**
- The Trapper can't be trapped in their own trap(s).

### Game Options
| Name | Description
|----------|:-------------:|
| Trapper Spawn Chance | -
| Trapper Cooldown | -
| Max Traps Charges | -
| Number Of Tasks Needed For Recharging | -
| Trap Needed Trigger To Reveal | -
| Show Anonymous Map | -
| Trap Information Type | "Name", "Role", "Good/Evil Role"
| Trap Duration | -
-----------------------

# Modifier
A Modifier is an addition to your Impostor/Neutral/Crewmate role.
Some Modifiers can be ingame more than once (Quantity option).

## Bloody

If killed, the Bloody Modifier will leave a trail for x-seconds on their killer. The trail will have the color of the killed person.\
\
**NOTE**
- Impostor, Neutral or Crewmate roles can have this Modifier

### Game Options
| Name | Description |
|----------|:-------------:|
| Bloody Spawn Chance | -
| Bloody Quantity | -
| Trail duration | -
-----------------------

## Anti Teleport

The Anti Teleport Modifier prevents the player from getting teleported to the Meeting Table if a body gets reported or an Emergency Meeting is called.\
The player will start the round where the previous one ended (Emergency Meeting Call/Body Report).\
\
**NOTE**
- Impostor, Neutral or Crewmate roles can have this Modifier

### Game Options
| Name | Description |
|----------|:-------------:|
| Anti Teleport Spawn Chance | -
| Anti Teleport Quantity | -
-----------------------

## Tie Breaker

If the Voting ends in a tie, the Tie Breaker takes place and the player with the Tie Breaker Modifier gets an extra vote thats not visible to anyone.\
Everyone will know if the Tie Breaker was involved in the Meeting or not.\
\
**NOTE**
- Impostor, Neutral or Crewmate roles can have this Modifier
- There can only be on player with this Modifier

### Game Options
| Name | Description |
|----------|:-------------:|
| Tie Breaker Spawn Chance | -
-----------------------

## Bait

The Bait forces the killer to self report the body (you can configure a delay in the options).\
There can be more than one Bait.

**NOTE:**
- If the Sheriff has the Bait Modifier and dies while trying to kill a Crewmate, the Sheriff will *NOT* report themself.
- Impostor, Neutral or Crewmate roles can have this Modifier

### Game Options
| Name | Description
|----------|:-------------:|
| Bait Spawn Chance | -
| Bait Quantity | -
| Bait Report Delay Min | -
| Bait Report Delay Max | -
| Warn The Killer With A Flash | -
-----------------------

## Lovers

There are always two Lovers which are linked together.\
Their primary goal is it to stay alive together until the end of the game.\
If one Lover dies (and the option is activated), the other Lover suicides.\
You can specify the chance of one Lover being an Impostor.\
The Lovers never know the role of their partner, they only see who their partner is.\
The Lovers win, if they are both alive when the game ends. They can also win with their original team (e.g. a dead Impostor Lover can win with the Impostors, an Arsonist Lover can still achieve an Arsonist win).\
If one of the Lovers is a killer (i.e. Jackal/Sidekick/Impostor), they can achieve a "Lovers solo win" where only the Lovers win.\
If there is no killer among the Lovers (e.g. an Arsonist Lover + Crewmate Lover) and they are both alive when the game ends, they win together with the Crewmates.\
If there's an Impostor/Jackal + Crewmate Lover in the game, the tasks of a Crewmate Lover won't be counted (for a task win) as long as they're alive.\
If the Lover dies, their tasks will also be counted.\
You can enable an exclusive chat only for Lovers.

**NOTE:**
In a 2 Crewmates vs 2 Impostors (or 2 members of team Jackal) and the Lovers are not in the same team, the game is not automatically over since the Lovers can still achieve a solo win. E.g. if there are the following roles Impostor + ImpLover + Lover + Crewmate left, the game will not end and the next kill will decide if the Impostors or Lovers win.

### Game Options
| Name | Description |
|----------|:-------------:|
| Lovers Spawn Chance | -
| Chance That One Lover Is Impostor | -
| Both Lovers Die | Whether the second Lover suicides, if the first one dies
| Enable Lover Chat | -
-----------------------

**NOTE:**
- The Modifier **Lover** can't be guessed, you'll have to guess the primary role of one of the Lovers, to kill both of them.

## Sunglasses

The Sunglasses will lower the Crewmate's vision by a small percentage. The percentage is configurable in the options.\
The vision will also be affected when lights out.

**NOTE:**
- Sunglasses only affects Crewmates.
- If you have the Sunglasses Modifier and get sidekicked, you will lose the Modifier.

### Game Options
| Name | Description
|----------|:-------------:|
| Sunglasses Spawn Chance | -
| Sunglasses Quantity | -
| Vision with sunglasses | -
-----------------------

## Mini

The Mini's character is smaller and hence visible to everyone in the game.\
The Mini cannot be killed until it turns 18 years old, however it can be voted out.

**Impostor/Jackal Mini:**
- While growing up the kill cooldown is doubled. When it's fully grown up its kill cooldown is 2/3 of the default one.
- If it gets thrown out of the ship, everything is fine.

**Crewmate Mini:**
- The Crewmate Mini aims to play out the strength its invincibility in the early game.
- If it gets thrown out of the ship before it turns 18, everyone loses. So think twice before you vote out a Mini.

**Neutral Mini:**
- The cooldown is not effected, except for the Team Jackal/Sidekick.
- If it gets thrown out of the ship, everything is fine except for the Jester.
- If the Jester Mini gets voted out the game will end in a Jester win.

**NOTE:**
- If the Sheriff tries to kill the Mini before it's fully grown, nothing happens.
- The Sheriff can kill the Impostor/Neutral Mini, but only if it's fully grown up.
- If the Mini's primary role is guessed correctly, it dies like every other role and nothing further happens.

### Game Options
| Name | Description |
|----------|:-------------:|
| Mini Spawn Chance | -
| Mini  | Mini Growing Up Duration
| Mini Grows Up In Meeting | -
-----------------------

## VIP

An Impostor, Jackal or Crewmate can be affected by the VIP (Very Important Player) Modifier.\
The VIP will show everyone when he dies with a flash similar to the Seer Flash.\
If the option Show Team Color is On, then everyone will get a flash in the color of the team the player was part of.

Teams:
- Impostor = Red
- Neutral = Blue
- Crewmate = White

### Game Options
| Name | Description
|----------|:-------------:|
| VIP Spawn Chance | -
| VIP Quantity | -
| Show Team Color | -
-----------------------

## Invert

The Invert Modifier inverts your controls (no matter if keyboard or mouse).\
\
**NOTE**
- Impostor, Neutral or Crewmate roles can have this Modifier

### Game Options
| Name | Description
|----------|:-------------:|
| Invert Spawn Chance | -
| Invert Quantity | -
-----------------------

## Chameleon

The Chameleon becomes (partially or fully) invisible when standing still for x-seconds (depends on the settings).\
\
**NOTE**
- You can use abilities while being invisible, only moving will make you visible again
- Impostor, Neutral or Crewmate roles can have this Modifier

### Game Options
| Name | Description
|----------|:-------------:|
| Chameleon Spawn Chance | -
| Chameleon Quantity | -
| Time Until Fading Starts | -
| Fade Duration | - 
| Minimum Visibility | -
-----------------------

## Shifter

The Shifter is a Modifier that can shift with another player. If the other player is Crewmate as well, they will swap their roles.\
Swapping roles with an Impostor or Neutral fails and the Shifter commits suicide after the next meeting (there won't be a body).\
The Shift will always be performed at the end of the next meeting right before a player is exiled. The target needs to be chosen during the round.\
Even if the Shifter or the target dies before the meeting, the Shift will still be performed.\
\
**NOTE:**
- The Shifter shift will always be triggered before the Erase (hence either the new role of the Shifter will be erased or the Shifter gets the role of their target, depending on whom the Eraser erased)
- One time use abilities (e.g. shielding a player or Engineer sabotage fix) can only used by one player in the game (i.e. the Shifter
can only use them, if the previous player did not use them before)
- The Shifter button is located in the bottom left corner right next to the garlic button (if the Vampire is enabled)
- Only a Crewmate role can have this Modifier

### Game Options
| Name | Description
|----------|:-------------:|
| Shifter Spawn Chance | -
-----------------------

# Gamemodes

##赌怪模式
**Guesser-Gamemode**是Classic-Gamemode的扩展，为您提供了大量的新选项
赌怪现在就像一个额外的，可以适用于所有成员，如果你想。设置可分别设置每个阵营的赌怪数量(内鬼，独立，船员)
当玩这个游戏模式时，赌怪可以有另一个职业(例如Medic Guesser)。相同的
当然适用于内鬼和独立
如果启用，玩家还可以拥有一个附加(如Medic guess Mini)。

### Game Options
| Name | Description
|----------|:-------------:|
| Number of Crew Guessers | -
| Number of Neutral Guessers | -
| Number of Impostor Guessers | -
| Force Jackal Guesser | If set to "On", the first neutral role who will be Guesser is the Jackal. 
| Force Thief Guesser | If set to "On", the first (or second if Force Jackal Guesser) neutral role who will be Guesser is the Thief. 
| Guessers Can Have A Modifier | -
| Guesser Number Of Shots | -
| Guesser Can Shoot Multiple Times Per Meeting | -
| Guesses Ignore The Medic Shield | -
| Evil Guesser Can Guess The Spy | -
| Guesser Can't Guess Snitch When Tasks Completed | -
-----------------------

**NOTE**
- If a Crewmate Guesser gets sidekicked, they will remain a Guesser even if the host (maybe) has set up only 1 "Neutral role Guesser".

## Hide 'n Seek
The **Hide 'n Seek-Gamemode** is a standalone Gamemode where Hunter have to catch their prey ("Hunted" players).\
The Hunter and Hunted player who are still alive are displayed to everyone in the bottom left corner (similar to the Arsonist display).\
When the game starts, the Hunter's movement is disabled for x-seconds (depends on the settings).\
There is a time-limit for each round, if the timer runs out and at least one Hunted is still alive, the Crew wins. The Hunted players can also win, if Taskwin is enabled and the Crew completes all tasks.\
If the Hunters kill all players before one of these conditions is triggered, the Hunters win.\

#### Hunter Abilities:
- Enable arrows (arrows point to all Hunted players for x-seconds (depending on settings))
- Mobile Admin table (like Hacker)
- Lighter ability (gives the Hunter a biggier vision radius for x-seconds (depending on settings))

#### Hunted Ability:
- Timeshield (like Timemaster, depending on settings)

Each Hunter action or finished Crew task will lower the timer by a configurable amount of time.\
\
**NOTE:**
- We added a vent on Polus (Specimen), but only for this Gamemode. The vent is connected with Admin & Lab.
- The Report button lights up, but cannot be pressed.
- The tasks can be configured separately for this mode, without affecting your normal game settings.
- Only the Hunter will be rewinded if they try to kill a player with an active timeshield.

### Game Options
| General | Description | Hunter | Description | Hunted | Description |
|----------|:-------------:|----------|:-------------:|----------|:-------------:|
| Map | -                                | Hunter Light Cooldown | -                  | Hunted Shield Cooldown | -
| Number Of Hunters | -                  | Hunter Light Duration | -                  | Hunted Shield Duration | -
| Kill Cooldown | -                      | Hunter Light Vision | -                    | Hunted Rewind Time | -
| Hunter Vision | -                      | Hunter Light Punish in Sec | -             | Hunted Shield Number | -
| Hunted Vision | -                      | Hunter Admin Cooldown | -
| Common Tasks | -                       | Hunter Admin Duration | -
| Short Tasks | -                        | Hunter Admin Punish In Sec | -
| Long Tasks | -                         | Hunter Arrow Cooldown | -
| Timer In Min | -                       | Hunter Arrow Duration | -
| Task Win Is Possible | -               | Hunter Arrow Punish In Sec | -
| Finish Tasks Punish In Sec | -
| Enable Sabotages | -
| Time The Hunter Needs To Wait | -

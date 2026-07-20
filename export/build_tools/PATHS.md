# 可迁移的导出路径

三个 BAT 根据它们自己的位置反推工程根目录；无论仓库位于哪个盘符或
目录，产物始终输出到工程内的 `export\Chaos_heidemarie`。

PCK 导出还需要两项工程外依赖。若仓库仍处在 `ChaosMod` 目录下，脚本会
自动在其父目录下查找：

- `Godot_v4.5.1\Godot_v4.5.1-stable_win64.exe`
- `refer\Slay the Spire 2_pck_new6-10\pck`

新设备的目录结构不同，可在运行 BAT 前的 PowerShell 中临时设置：

    $env:CHAOS_GODOT_PATH = "D:\tools\Godot_v4.5.1-stable_win64.exe"
    $env:CHAOS_STS2_RESOURCE_ROOT = "D:\ChaosMod\refer\Slay the Spire 2_pck_new6-10\pck"

也可用以下命令一次性写入当前用户的环境变量；重新打开终端或双击 BAT
即可生效：

    setx CHAOS_GODOT_PATH "D:\tools\Godot_v4.5.1-stable_win64.exe"
    setx CHAOS_STS2_RESOURCE_ROOT "D:\ChaosMod\refer\Slay the Spire 2_pck_new6-10\pck"

`CHAOS_STS2_RESOURCE_ROOT` 必须指向**解包后的游戏资源树**，不是 Steam
游戏安装目录。临时打包目录固定在 `export\build_tools\.pck_stage`，且已被
Git 忽略。

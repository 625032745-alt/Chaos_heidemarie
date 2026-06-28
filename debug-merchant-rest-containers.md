# [OPEN] merchant-rest-containers

## Symptoms
- `h:\Trae_home\ChaosMod\git\log\godot.log` 显示无法进入商店。
- 需要同时检查商店与篝火处的角色容器是否按原版和 `Chaos_XC_regent02` 正确接入。

## Hypotheses
- H1: `Heidemarie` 的商店角色场景路径有效，但场景内部缺少原版商店容器所需节点，导致进入商店时报错。
- H2: 商店角色容器本身存在，但引用的 Spine `.tres/.atlas/.skel` 或子节点路径不符合运行时预期。
- H3: 篝火角色场景也存在同类容器结构问题，只是当前先在商店处暴露。
- H4: 无法进入商店并非角色容器缺失，而是商店流程中更早的资源/脚本异常打断了房间切换。
- H5: 对照 `Chaos_XC_regent02` 与原版 `merchant/rest_site` 场景结构后，可以定位出缺失节点、错误类型或错误参数。

## Evidence Plan
- 检索 `godot.log` 中与 merchant、shop、rest、campfire、Heidemarie、Exception、ERROR 相关的日志。
- 对照 `Chaos_XC_regent02` 和原版的商店/篝火角色场景与容器结构，只在确认差异后做最小修复。

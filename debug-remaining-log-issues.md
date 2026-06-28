# [OPEN] remaining-log-issues

## Symptoms
- 需要处理 `h:\Trae_home\ChaosMod\git\log\godot.log` 中剩余的日志问题。

## Hypotheses
- H1: 剩余关键问题主要来自 `ChaosHeidemarie` 的资源引用不完整，比如 Spine 资源 UID、atlas、skel 或场景路径异常。
- H2: 剩余问题里有一部分只是缓存类 `Asset not cached` / FMOD 缺失提示，不会实际阻断功能，不应误判成必须修复项。
- H3: 仍然有 `ChaosHeidemarie` 自己引发的运行时异常或错误级日志，需要优先修复。
- H4: 一部分日志来自其他 mod 或原版流程，不属于当前工程的处理范围，应明确剔除。
- H5: 当前剩余问题可以分成“阻断型错误”“可修资源警告”“暂可接受噪音”三类，并按优先级逐项处理。

## Evidence Plan
- 检索 `godot.log` 中的 `ERROR`、`WARNING`、`WARN`、`ChaosHeidemarie`、`heidemarie`、`invalid UID`、`cannot find sfx path`、`Asset not cached`。
- 只在拿到明确证据后，才对 `ChaosHeidemarie` 对应代码或资源做最小修复。

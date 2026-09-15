# Phase 01：项目骨架与技术基线

## 目标

建立可持续开发的 Unity 6.3 LTS URP 项目，使 Windows x64 构建以 1920×960 的 2:1 逻辑画面启动，并具备程序集、配置、测试和诊断基础。

## Codex 执行任务

1. 确认或创建 URP 项目；记录精确编辑器版本，不擅自升级补丁版本。
2. 按根目录规范建立 `Assets/_Game` 目录和四个程序集。
3. 安装并启用 Unity Input System；建立适用于 UI 的 Input Actions 与 `InputSystemUIInputModule`。
4. 创建 `SCN_Boot` 与 `SCN_Main`，建立可测试的启动流程。
5. 创建 `GameRulesConfig`、`RaceConfig`、`PresentationConfig`、`TouchConfig` 类型与默认实例。
6. 配置 URP、横屏、Windows x64、单实例和 1920×960 默认窗口/全屏策略。
7. 创建 2:1 根画布、Camera 与安全区容器；实现非 2:1 输出下的 letterbox/pillarbox，不拉伸内容。
8. 加入配置验证器、版本信息和结构化日志入口。
9. 配置 `.gitignore`，排除 Unity 生成目录，保留 `.meta` 文件。
10. 编写基础 EditMode 测试：配置默认值、非法值拒绝、画面比例计算。

## 产物

- 可打开且无编译错误的 Unity 项目。
- 基础场景、程序集、输入资产与四类核心配置。
- `BuildInfo`/日志上下文和配置校验器。
- EditMode 测试程序集与首批测试。

## 验收

- `GameRulesConfig` 默认初始金币为 50，下注额严格为 1/5/10，玩家范围为 1–4。
- `RaceConfig` 固定声明 5 条赛道和比赛硬超时。
- 1920×960 下内容铺满；1920×1080 下保持 2:1 且不变形。
- 输入模块可识别多个 Pointer ID，场景中不存在旧 Standalone Input Module。
- Windows x64 Development Build 能启动到主场景，并可用屏幕按钮退出。

## 退出条件

全部基础测试通过；项目可构建；配置无错误；没有游客流程键盘绑定。达到后方可执行 Phase 02。


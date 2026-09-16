# 《走地鸡王》Codex 开发阶段索引

本目录把项目拆成八个可独立实施、验证和回退的阶段。Codex 每次只执行一个阶段，并以该文档的“退出条件”为进入下一阶段的门禁。

## 阅读顺序

1. 仓库根目录 `AGENTS.md`
2. 本索引
3. 当前阶段文档
4. `QUALITY_GATES.md`

## 阶段路线

| 阶段 | 主题 | 可演示里程碑 |
|---|---|---|
| Phase 01 | 项目骨架与技术基线 | Windows 2:1 空壳可启动并通过基础检查 |
| Phase 02 | 状态机与玩家入场 | 1–4 人用触控按钮加入并进入热身 |
| Phase 03 | 下注与金币 | 每位玩家独立选鸡、选 1/5/10 并锁定 |
| Phase 04 | 受控随机比赛 | 锁定后生成可复现的五鸡比赛并唯一冲线 |
| Phase 05 | 角色表现与随机事件 | 五种行为完整播放，异常不会卡局 |
| Phase 06 | 结算、换鸡与连局 | 派彩、冠军留场、败者替换、金币继承 |
| Phase 07 | 完整 UI、音频与展项运维 | 主菜单到 GameOver 全触控闭环 |
| Phase 08 | 硬件、压力测试与发布 | 红外多点触控实机验收及 Windows Release |
| Phase 09 | 场景、美术、UI 与角色表现 | GPT 生成美术接入并完成可展示视觉版本 |
| Phase 10 | 沉浸式角色与动态结算 | 13 种程序化 3D 小鸡、下注热身、全屏与领奖台 |

## 阶段执行模板

每次交给 Codex 的任务应包含：

```text
执行 Docs/Development/PHASE_0X_*.md。
先检查 AGENTS.md 和当前工作区，不覆盖用户已有改动。
完成本阶段列出的产物、自动化测试和手工验收记录。
未达到退出条件时不要进入下一阶段。
最后汇报变更文件、测试结果、未验证项和风险。
```

## 全局依赖顺序

```text
Project Baseline
  -> Game Flow + Players
  -> Betting + Economy
  -> Deterministic Race Plan
  -> Chicken Presentation
  -> Settlement + Roster Loop
  -> Full UX + Operations
  -> Hardware Validation + Release
  -> Art Direction + Scene Polish
  -> Immersive 3D Presentation + Podium Celebration
```

## 决策记录

架构或规则发生实质变化时，在 `Docs/Development/Decisions/` 新增 ADR，至少写明背景、决定、替代方案、后果与日期。不得仅在代码注释中改变产品规则。

# Release 发布与回滚清单

## 构建前

- [x] Unity 固定为 6000.3.24f1，`ProjectVersion.txt` 无变化。
- [x] `manifest.json` 与 `packages-lock.json` 已归档。
- [x] URP、1920×960、Windows x64、窗口模式和后台运行配置已复核。
- [x] EditMode、PlayMode、10,000 种子和 500 局 soak 测试通过。
- [x] ScriptableObject 配置验证通过，运行时代码无键盘/非受控随机旁路。

## 发布包

- [x] `Builds/Windows/Release` 完整目录已生成。
- [x] Release 能冷启动并显示主菜单。
- [x] Release 保留有限日志与诊断/五点校准入口。
- [x] Development 候选版与 Release 分目录存放。
- [x] 记录 Release 关键文件 SHA-256 清单和总大小。
- [x] 生成 `GroundChickenKing-v0.1.0-rc1-Release.zip`。
- [x] 生成同版安装恢复包 `GroundChickenKing-v0.1.0-rc1-Rollback.zip`；当前不存在更早正式版本。

## 发布后

- [ ] 按 `HARDWARE_ACCEPTANCE.md` 在目标设备完成并签字。
- [ ] 保存现场配置照片、测试日志、性能记录和签字表。
- [ ] 冷启动分别走通 Bankrupt 与 MaxRounds GameOver。
- [ ] 回滚包实际解压并完成一次冷启动检查。

只有以上项目全部完成，才可把 RC 标记为正式首版。

## 本版本豁免

项目方于 2026-09-15 明确跳过上述发布后实体硬件步骤。这些项目保持未勾选，不视为通过；风险与后续恢复方式见 ADR-0002。本包继续标记为 `0.1.0-rc1`。

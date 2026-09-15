# 软件发布清单：0.1.0-rc1

构建日期：2026-09-15

Unity：6000.3.24f1

平台：Windows 10/11 x64，本地离线

逻辑画面：1920×960，2:1

## 构建产物

- Development 候选版：`Builds/Windows/Development/GroundChickenKing.exe`
- Release 目录：`Builds/Windows/Release`
- Release 归档：`Builds/Windows/Archives/GroundChickenKing-v0.1.0-rc1-Release.zip`
- 回滚归档：`Builds/Windows/Archives/GroundChickenKing-v0.1.0-rc1-Rollback.zip`

Release 完整构建大小：96,908,740 bytes。

Release 归档大小：36,624,628 bytes。

Release 归档 SHA-256：`D9AF0217441AEA59A811B43B8EE08FB91D3E31799F1D6BDAD65F8CCA2943746D`。

关键文件校验值保存在 Release 目录的 `SHA256SUMS.txt`。

## 回滚说明

当前没有更早的正式现场版本，因此 rc1 回滚归档与 rc1 Release 归档内容相同，用于恢复被修改或损坏的安装目录；它不是跨版本降级包。首个现场签字版本通过后，应保留该版本作为后续发布的真正上一版回滚包。

## 发布状态

软件自动化与本机冷启动已通过。2026-09-15 项目方明确豁免本版本的实体投影、红外 HID、10 点并发和 60 分钟实机验收；豁免不等于测试通过。包名继续保留 RC 标识，并按未验证硬件风险使用。

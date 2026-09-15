using GroundChickenKing.Betting;
using GroundChickenKing.Flow;
using GroundChickenKing.Players;
using GroundChickenKing.Race;

namespace GroundChickenKing.UI
{
    // Single runtime localization entry point. A later language pack can replace this class without changing domain code.
    public static class UiTextCatalog
    {
        public const string ConfirmExit = "确定退出游戏？";
        public const string ConfirmReturnHome = "确定返回主菜单？\n当前游戏进度将结束";
        public const string ConfirmRestart = "确定重新开始？\n金币和局数将重置";
        public const string NoRecentError = "无";
        public const string SeedUnavailable = "尚未生成";
        public static string Volume(float value) => $"音量 {UnityEngine.Mathf.RoundToInt(value * 100f)}%";
        public static string Intensity(float value) => $"视觉强度 {UnityEngine.Mathf.RoundToInt(value * 100f)}%";
        public static string MaximumRounds(int value) => $"最大局数 {value}";
        public static string JoinSummary(int count) => count == 0 ? "请至少加入 1 名玩家" : $"已加入 {count} 名玩家，可以开始热身";
        public static string Warmup(int count) => $"{count} 名玩家已准备 · 小鸡正在热身";
        public static string BettingStatus(int locked, int eligible) => $"已锁定 {locked} / {eligible} · 全员确认后进入倒计时";
        public static string Countdown(int seed, string token) => $"比赛种子 {seed}\n{token}";
        public static string Racing(float elapsed, string plan) => $"比赛进行中 · {elapsed:0.0}s\n剧本 {plan}";
        public static string Curry(string champion, string losers) => $"🏆 {champion} 保留参赛\n🍛 {losers} 变成了香喷喷的咖喱鸡排饭";
        public static string RoundChampion(int round, string champion) => $"第 {round} 局冠军：{champion}";
        public static string Settlement(PlayerSettlementDetail detail) => $"玩家 {(int)detail.Seat}：{(detail.DidWin ? $"命中 +{detail.ReturnAmount}" : $"未命中 -{detail.Stake}")} · 余额 {detail.BalanceAfter}";
        public static string GameOverMessage(GameOverReason reason, int rounds) => reason == GameOverReason.Bankrupt ? "所有玩家金币耗尽" : $"达到最大局数 {rounds}";
        public static string Ranking(int rank, PlayerSnapshot player) => $"第 {rank} 名 · 玩家 {(int)player.Definition.Seat} · {player.Coins} 金币";
        public static string BetSelection(BetSnapshot bet, string chickenName) => bet.IsLocked ? $"已锁定 · {chickenName} · {bet.Stake} 金币" : $"选择：{chickenName} · {(bet.HasStake ? bet.Stake + " 金币" : "未选金额")}";
        public static string ChickenName(string id) => id switch { "chicken-flash" => "闪电鸡", "chicken-chubby" => "胖墩", "chicken-tiny" => "豆丁", "chicken-bro" => "鸡哥", "chicken-slacker" => "摸鱼鸡", _ => "未选鸡" };
        public static string Diagnostics(string build, int width, int height, int touches, string seed, string error, string path) => $"版本：{build}\n分辨率：{width} × {height}\n触点：{touches} / 10\n输入：Windows HID / Input System\n最近种子：{seed}\n最近错误：{error}\n日志：{path}";
    }
}

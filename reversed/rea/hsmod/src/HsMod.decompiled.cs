using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assets;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Services;
using Game.PackOpening;
using HarmonyLib;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.UI;
using Hearthstone.Util;
using PegasusLettuce;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("HsMod")]
[assembly: AssemblyDescription("Hearthstone Modify Based on BepInEx")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Pik_4")]
[assembly: AssemblyProduct("HsMod")]
[assembly: AssemblyCopyright("Copyright © Pik_4")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("4353F22A-2F95-49D7-A5E7-3E829BD8BF4B")]
[assembly: AssemblyFileVersion("4.1.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyVersion("4.1.0.0")]
namespace HsMod;

public static class PluginConfig
{
	public static class CommandConfig
	{
		public static int webServerPort = -1;

		public static string hsMatchLogPath = "";

		public static int width = -1;

		public static int height = -1;
	}

	public static ConfigEntry<bool> isPluginEnable;

	public static ConfigEntry<bool> isFakeOpenEnable;

	public static ConfigEntry<Utils.ConfigTemplate> configTemplate;

	public static ConfigEntry<bool> isTimeGearEnable;

	public static ConfigEntry<bool> isShortcutsEnable;

	public static ConfigEntry<int> targetFrameRate;

	public static ConfigEntry<bool> isDynamicFpsEnable;

	public static ConfigEntry<int> timeGear;

	public static ConfigEntry<int> receiveEnemyEmoteLimit;

	public static ConfigEntry<bool> isIGMMessageShow;

	public static ConfigEntry<bool> isOnApplicationFocus;

	public static ConfigEntry<bool> isAutoExit;

	public static ConfigEntry<bool> isAlertPopupShow;

	public static ConfigEntry<Utils.AlertPopupResponse> responseAlertPopup;

	public static ConfigEntry<bool> isRewardToastShow;

	public static ConfigEntry<bool> isAutoOpenBoxesRewardEnable;

	public static ConfigEntry<bool> isFullnameShow;

	public static ConfigEntry<bool> isOpponentRankInGameShow;

	public static ConfigEntry<bool> isSkipHeroIntro;

	public static ConfigEntry<bool> isThinkEmotesEnable;

	public static ConfigEntry<bool> isExtendedBMEnable;

	public static ConfigEntry<bool> isQuickPackOpeningEnable;

	public static ConfigEntry<bool> isAutoPackOpeningEnable;

	public static ConfigEntry<bool> isShowCardLargeCount;

	public static ConfigEntry<bool> isAutoRefundCardDisenchantEnable;

	public static ConfigEntry<bool> isShowCollectionCardIdEnable;

	public static ConfigEntry<bool> isShowRetireForever;

	public static ConfigEntry<bool> isIdleKickEnable;

	public static ConfigEntry<bool> isQuickModeEnable;

	public static ConfigEntry<bool> isCardTrackerEnable;

	public static ConfigEntry<bool> isCardRevealedEnable;

	public static ConfigEntry<bool> isMoveEnemyCardsEnable;

	public static ConfigEntry<bool> isAutoReportEnable;

	public static ConfigEntry<bool> isFiresideGatheringEnable;

	public static ConfigEntry<double> firesideGatheringLatitude;

	public static ConfigEntry<double> firesideGatheringLongitude;

	public static ConfigEntry<double> firesideGatheringGpsAccuracy;

	public static ConfigEntry<bool> isAutoRecvMercenaryRewardEnable;

	public static ConfigEntry<bool> isMercenaryBattleZoom;

	public static ConfigEntry<Utils.CardState> mercenaryDiamondCardState;

	public static ConfigEntry<Utils.CardState> randomMercenarySkinEnable;

	public static ConfigEntry<bool> isShutUpBobEnable;

	public static ConfigEntry<bool> isBgsGoldenEnable;

	public static ConfigEntry<bool> isOpponentGoldenCardShow;

	public static ConfigEntry<bool> isSignatureCardStateEnable;

	public static ConfigEntry<Utils.CardState> goldenCardState;

	public static ConfigEntry<Utils.CardState> maxCardState;

	public static ConfigEntry<KeyboardShortcut> keyTimeGearUp;

	public static ConfigEntry<KeyboardShortcut> keyTimeGearDown;

	public static ConfigEntry<KeyboardShortcut> keyTimeGearDefault;

	public static ConfigEntry<KeyboardShortcut> keyTimeGearMax;

	public static ConfigEntry<KeyboardShortcut> keySimulateDisconnect;

	public static ConfigEntry<KeyboardShortcut> keyCopyBattleTag;

	public static ConfigEntry<KeyboardShortcut> keyCopySelectBattleTag;

	public static ConfigEntry<KeyboardShortcut> keyConcede;

	public static ConfigEntry<KeyboardShortcut> keyContinueMulligan;

	public static ConfigEntry<KeyboardShortcut> keySquelch;

	public static ConfigEntry<KeyboardShortcut> keySoundMute;

	public static ConfigEntry<KeyboardShortcut> keyShutUpBob;

	public static ConfigEntry<KeyboardShortcut> keyRefund;

	public static ConfigEntry<KeyboardShortcut> keyReadNewCards;

	public static ConfigEntry<KeyboardShortcut> keyShowFPS;

	public static ConfigEntry<KeyboardShortcut> keyEmoteGreetings;

	public static ConfigEntry<KeyboardShortcut> keyEmoteWellPlayed;

	public static ConfigEntry<KeyboardShortcut> keyEmoteThanks;

	public static ConfigEntry<KeyboardShortcut> keyEmoteWow;

	public static ConfigEntry<KeyboardShortcut> keyEmoteOops;

	public static ConfigEntry<KeyboardShortcut> keyEmoteThreaten;

	public static ConfigEntry<int> skinCoin;

	public static ConfigEntry<int> skinCardBack;

	public static ConfigEntry<int> skinBoard;

	public static ConfigEntry<int> skinBgsBoard;

	public static ConfigEntry<int> skinBgsFinisher;

	public static ConfigEntry<int> skinBob;

	public static ConfigEntry<int> skinHero;

	public static ConfigEntry<int> skinOpposingHero;

	public static ConfigEntry<bool> isSkinDefalutHeroEnable;

	public static ConfigEntry<bool> isShowFPSEnable;

	public static ConfigEntry<bool> isInternalModeEnable;

	public static ConfigEntry<int> webServerPort;

	public static ConfigEntry<string> webPageBackImg;

	public static ConfigEntry<bool> isWebshellEnable;

	public static ConfigEntry<string> hsMatchLogPath;

	public static ConfigEntry<string> hsLogPath;

	public static ConfigEntry<long> autoQuitTimer;

	public static ConfigEntry<Utils.DevicePreset> fakeDevicePreset;

	public static ConfigEntry<OSCategory> fakeDeviceOs;

	public static ConfigEntry<ScreenCategory> fakeDeviceScreen;

	public static ConfigEntry<string> fakeDeviceName;

	public static ConfigEntry<int> fakePackCount;

	public static ConfigEntry<BoosterDbId> fakeBoosterDbId;

	public static ConfigEntry<bool> isFakeRandomResult;

	public static ConfigEntry<bool> isFakeRandomRarity;

	public static ConfigEntry<bool> isFakeRandomPremium;

	public static ConfigEntry<bool> isFakeAtypicalRandomPremium;

	public static ConfigEntry<TAG_PREMIUM> fakeRandomPremium;

	public static ConfigEntry<Utils.CardRarity> fakeRandomRarity;

	public static ConfigEntry<int> fakeCardID1;

	public static ConfigEntry<TAG_PREMIUM> fakeCardPremium1;

	public static ConfigEntry<int> fakeCardID2;

	public static ConfigEntry<TAG_PREMIUM> fakeCardPremium2;

	public static ConfigEntry<int> fakeCardID3;

	public static ConfigEntry<TAG_PREMIUM> fakeCardPremium3;

	public static ConfigEntry<int> fakeCardID4;

	public static ConfigEntry<TAG_PREMIUM> fakeCardPremium4;

	public static ConfigEntry<int> fakeCardID5;

	public static ConfigEntry<TAG_PREMIUM> fakeCardPremium5;

	public static ConfigEntry<Utils.BuyAdventureTemplate> buyAdventure;

	public static ConfigEntry<bool> isKarazhanFixEnable;

	public static ShowFPS showFPS;

	public static Dictionary<int, int> HeroesMapping = new Dictionary<int, int>();

	public static Dictionary<string, string> HeroesPowerMapping = new Dictionary<string, string>();

	public static long timeKeeper = DateTime.Now.Ticks;

	public static List<Utils.CardMapping> CardsMapping = new List<Utils.CardMapping>();

	public static IGraphicsManager graphicsManager;

	public static ConfigValue configValue = new ConfigValue();

	public static void ConfigBind(ConfigFile config)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Expected O, but got Unknown
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_073c: Unknown result type (might be due to invalid IL or missing references)
		//IL_076b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_081e: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Unknown result type (might be due to invalid IL or missing references)
		//IL_087c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_091d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0969: Unknown result type (might be due to invalid IL or missing references)
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d1: Expected O, but got Unknown
		//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a91: Expected O, but got Unknown
		//IL_0aba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac4: Expected O, but got Unknown
		//IL_0c70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7a: Expected O, but got Unknown
		//IL_0c9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca9: Expected O, but got Unknown
		//IL_0cd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cdc: Expected O, but got Unknown
		//IL_0d01: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0b: Expected O, but got Unknown
		//IL_0d34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3e: Expected O, but got Unknown
		//IL_0d63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6d: Expected O, but got Unknown
		//IL_0d96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da0: Expected O, but got Unknown
		//IL_0dc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dcf: Expected O, but got Unknown
		//IL_0df8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e02: Expected O, but got Unknown
		//IL_0e27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e31: Expected O, but got Unknown
		config.Clear();
		isPluginEnable = config.Bind<bool>("全局", "HsMod状态", false, "是否启用插件（修改该选项后建议重启炉石）");
		configTemplate = config.Bind<Utils.ConfigTemplate>("全局", "设置模板", Utils.ConfigTemplate.DoNothing, "配置运行模板，当选项为DoNothing时，不修改任何配置。配置修改完成后自动替换回DoNothing");
		isShortcutsEnable = config.Bind<bool>("全局", "快捷键状态", false, "是否启用快捷键");
		isTimeGearEnable = config.Bind<bool>("全局", "变速齿轮状态", false, "是否启用变速齿轮");
		timeGear = config.Bind<int>("全局", "变速倍率", 0, new ConfigDescription("变速齿轮倍速，1和0倍率相同，负数表示变慢", (AcceptableValueBase)(object)new AcceptableValueRange<int>(-32, 32), Array.Empty<object>()));
		isShowFPSEnable = config.Bind<bool>("全局", "显示FPS", false, "是否显示FPS信息（快捷键：左Crtl+P）");
		targetFrameRate = config.Bind<int>("全局", "游戏帧率", -1, new ConfigDescription("游戏帧率设置，-1表示不做修改或恢复默认值（默认值可能为30）", (AcceptableValueBase)(object)new AcceptableValueRange<int>(-1, 2333), Array.Empty<object>()));
		isIGMMessageShow = config.Bind<bool>("优化", "游戏内消息", true, "（牌店无法打开时，可以尝试设置该选项为开启状态）是否显示游戏内消息（广告推销、削弱补丁、天梯结算信息等）");
		isAlertPopupShow = config.Bind<bool>("优化", "弹出消息", true, "是否显示弹窗");
		responseAlertPopup = config.Bind<Utils.AlertPopupResponse>("优化", "弹出响应", Utils.AlertPopupResponse.DONOTHING, "在屏蔽弹出消息时，如何回应弹窗");
		isOnApplicationFocus = config.Bind<bool>("优化", "应用焦点", true, "isOnApplicationFocus");
		isRewardToastShow = config.Bind<bool>("优化", "结算展示", true, "是否展示结算任务、成就奖励、升级提示等（可能导致领取奖励时无提示）");
		isAutoOpenBoxesRewardEnable = config.Bind<bool>("优化", "自动开盒", false, "是否自动打开竞技场（对决、佣兵等）结算宝箱");
		isAutoExit = config.Bind<bool>("优化", "报错退出", false, "遇到错误是否自动退出");
		isShowCardLargeCount = config.Bind<bool>("优化", "收藏卡牌数量", false, "是否显示收藏卡牌数量大于等于10时的数量（选中时暂有Bug）");
		isShowCollectionCardIdEnable = config.Bind<bool>("优化", "显示卡牌ID", false, "是否在右键选择卡牌（皮肤）时，显示并复制所选内容的CardID");
		isShowRetireForever = config.Bind<bool>("优化", "显示放弃", false, "允许在0-0时放弃套牌");
		isIdleKickEnable = config.Bind<bool>("优化", "允许掉线", true, "（尚未测试）是否允许长时间无操作掉线（启动游戏时无法加载配置）");
		isQuickPackOpeningEnable = config.Bind<bool>("开包", "开包加速", false, "开包加速，使用空格开包时直接展示结果");
		isAutoPackOpeningEnable = config.Bind<bool>("开包", "自动开包", false, "（慎用，有BUG！）开完全部卡包，不区分卡包品类（基于开包加速）");
		isAutoRefundCardDisenchantEnable = config.Bind<bool>("开包", "自动分解", false, "是否在开包时自动分解全额反尘的卡");
		isAutoReportEnable = config.Bind<bool>("好友", "自动举报", false, "对局结束后自动举报对手昵称违规、作弊和脚本、恶意投降");
		isMoveEnemyCardsEnable = config.Bind<bool>("好友", "观战展示卡牌", false, "（尚未测试）在Ob中展示(旋转)对手手中的牌");
		isFiresideGatheringEnable = config.Bind<bool>("好友", "炉边聚会虚拟定位", false, "是否启用炉边聚会虚拟定位");
		firesideGatheringLatitude = config.Bind<double>("好友", "炉边聚会纬度", 0.0, "炉边聚会纬度");
		firesideGatheringLongitude = config.Bind<double>("好友", "炉边聚会经度", 0.0, "炉边聚会经度");
		firesideGatheringGpsAccuracy = config.Bind<double>("好友", "炉边聚会Gps定位精度", 54.0, "炉边聚会定位精度");
		isQuickModeEnable = config.Bind<bool>("炉石", "快速战斗", false, "是否启用酒馆或佣兵AI快速战斗模式");
		isFullnameShow = config.Bind<bool>("炉石", "显示全名", false, "是否显示对手战网全名；如果启用该选项，还会允许添加当前对手(启动快捷键时，也允许添加对手)。");
		isOpponentRankInGameShow = config.Bind<bool>("炉石", "显示天梯等级", false, "是否在传说前显示对手天梯等级");
		isCardTrackerEnable = config.Bind<bool>("炉石", "卡牌追踪", false, "推测对手卡牌，并给出提示（例如：抉择等，有概率识别错误）");
		isCardRevealedEnable = config.Bind<bool>("炉石", "卡牌揭示", false, "以明牌方式展示已知的卡牌（有概率导致炉石自动断线重连）");
		isSkipHeroIntro = config.Bind<bool>("炉石", "跳过英雄介绍", false, "是否跳过英雄介绍(ShouldSkipMulligan)");
		isExtendedBMEnable = config.Bind<bool>("炉石", "表情无冷却", false, "是否允许无限制表情(延迟最低1.5s)");
		isThinkEmotesEnable = config.Bind<bool>("炉石", "思考表情", true, "是否允许显示思考表情");
		receiveEnemyEmoteLimit = config.Bind<int>("炉石", "表情数量", -1, new ConfigDescription("游戏内表情数量接收限制，超过自动屏蔽对手表情，0时开局屏蔽，-1不限制（有小bug）", (AcceptableValueBase)(object)new AcceptableValueRange<int>(-1, 100), Array.Empty<object>()));
		isOpponentGoldenCardShow = config.Bind<bool>("炉石", "对手卡牌特效", true, "是否显示对手卡牌特效(覆盖ALL配置)");
		isSignatureCardStateEnable = config.Bind<bool>("炉石", "异画特效", true, "是否在卡牌最高特效中显示异画（仅影响卡牌最高特效）");
		goldenCardState = config.Bind<Utils.CardState>("炉石", "金卡特效", Utils.CardState.Default, "强制金卡特效");
		maxCardState = config.Bind<Utils.CardState>("炉石", "卡牌最高特效", Utils.CardState.Default, "强制卡牌最高特效特效（目前优先级：钻石、异画、金卡、普通）");
		isAutoRecvMercenaryRewardEnable = config.Bind<bool>("佣兵", "自动领奖", false, "是否自动领取佣兵佣兵奖励（屏蔽宝箱）");
		isMercenaryBattleZoom = config.Bind<bool>("佣兵", "允许缩放", true, "（可能存在BUG）是否允许佣兵战斗时缩放画面");
		mercenaryDiamondCardState = config.Bind<Utils.CardState>("佣兵", "钻石皮肤替换", Utils.CardState.Default, "如果可以，是否替换成钻石皮肤（优先级低于炉石-卡牌最高特效）");
		randomMercenarySkinEnable = config.Bind<Utils.CardState>("佣兵", "随机皮肤", Utils.CardState.Default, "随机皮肤（不包含钻皮且炉石-钻石卡特效值不能为disabled）");
		isShutUpBobEnable = config.Bind<bool>("酒馆", "沉默鲍勃", false, "是否让鲍勃闭嘴");
		isBgsGoldenEnable = config.Bind<bool>("酒馆", "酒馆镀金", false, "（测试，需要在炉石卡牌特效开启金卡特效）是否镀金酒馆。该镀金不会镀金随从和任务线。");
		skinCoin = config.Bind<int>("皮肤", "硬币", -1, "幸运币的偏好ID，-1表示不做修改（游戏内模拟拔线可以实时更新）");
		skinCardBack = config.Bind<int>("皮肤", "卡背", -1, "卡背的偏好ID，-1表示不做修改（实时生效）");
		skinBoard = config.Bind<int>("皮肤", "对战面板", -1, "（测试功能，可能会在酒馆卡住），对战面板替换，-1表示不做修改）");
		skinBgsBoard = config.Bind<int>("皮肤", "酒馆战斗面板", -1, "酒馆战斗面板的偏好ID，-1表示不做修改");
		skinBgsFinisher = config.Bind<int>("皮肤", "酒馆击杀特效", -1, "酒馆击杀的偏好ID，-1表示不做修改");
		skinBob = config.Bind<int>("皮肤", "鲍勃", -1, "鲍勃的偏好ID，-1表示不做修改");
		isSkinDefalutHeroEnable = config.Bind<bool>("皮肤", "默认英雄", false, "如果可以，将对战英雄皮肤都替换为默认皮肤");
		skinHero = config.Bind<int>("皮肤", "英雄", -1, "（慎用，非挂机不建议使用，优先级低于默认英雄。一般情况下建议从文件加载英雄皮肤，修改完后F4更新；如果再对局中，则还需要模拟拔线）英雄的偏好ID，-1表示不做修改");
		skinOpposingHero = config.Bind<int>("皮肤", "对手英雄", -1, "（慎用，非挂机不建议使用，优先级低于默认英雄）对手英雄的偏好ID，-1表示不做修改");
		keyTimeGearUp = config.Bind<KeyboardShortcut>("快捷键", "齿轮倍率+1", new KeyboardShortcut((KeyCode)273, Array.Empty<KeyCode>()), "齿轮倍率增加1，默认方向上");
		keyTimeGearDown = config.Bind<KeyboardShortcut>("快捷键", "齿轮倍率-1", new KeyboardShortcut((KeyCode)274, Array.Empty<KeyCode>()), "齿轮倍率减少1，默认方向下");
		keyTimeGearDefault = config.Bind<KeyboardShortcut>("快捷键", "齿轮倍率归零", new KeyboardShortcut((KeyCode)276, Array.Empty<KeyCode>()), "恢复默认齿轮倍率，默认方向左");
		keyTimeGearMax = config.Bind<KeyboardShortcut>("快捷键", "齿轮倍率最大", new KeyboardShortcut((KeyCode)275, Array.Empty<KeyCode>()), "齿轮倍率小于4时变为4，大于4时变为8，默认方向右");
		keySimulateDisconnect = config.Bind<KeyboardShortcut>("快捷键", "模拟拔线", new KeyboardShortcut((KeyCode)100, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "模拟掉线重连，注意需要禁用报错退出并允许弹出消息，默认左Ctrl+D");
		keyCopyBattleTag = config.Bind<KeyboardShortcut>("快捷键", "复制对手战网标签", new KeyboardShortcut((KeyCode)99, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "复制游戏内对手战网ID，默认左Ctrl+C");
		keyCopySelectBattleTag = config.Bind<KeyboardShortcut>("快捷键", "复制所选对手战网标签", new KeyboardShortcut((KeyCode)323, Array.Empty<KeyCode>()), "复制酒馆内所选对手战网ID，默认鼠标左键");
		keyConcede = config.Bind<KeyboardShortcut>("快捷键", "投降", new KeyboardShortcut((KeyCode)32, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "投降，默认左Ctrl+空格");
		keyContinueMulligan = config.Bind<KeyboardShortcut>("快捷键", "结束回合", new KeyboardShortcut((KeyCode)32, Array.Empty<KeyCode>()), "结束回合或替换卡牌确认，默认空格");
		keySquelch = config.Bind<KeyboardShortcut>("快捷键", "沉默对手", new KeyboardShortcut((KeyCode)113, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "屏蔽你对手的表情，默认左Ctrl+Q");
		keySoundMute = config.Bind<KeyboardShortcut>("快捷键", "静音/恢复音量", new KeyboardShortcut((KeyCode)115, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "静音/恢复音量，默认左Ctrl+S");
		keyShutUpBob = config.Bind<KeyboardShortcut>("快捷键", "闭了，鲍勃", new KeyboardShortcut((KeyCode)98, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "禁用/恢复鲍勃语音，默认左Ctrl+B");
		keyRefund = config.Bind<KeyboardShortcut>("快捷键", "一键全额分解", new KeyboardShortcut((KeyCode)122, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "一键分解全额分解的卡牌（仅在开包界面与收藏界面有效），默认左Ctrl+Z");
		keyReadNewCards = config.Bind<KeyboardShortcut>("快捷键", "朕，已阅！", new KeyboardShortcut((KeyCode)114, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "消除所有新！标记（仅在开包界面与收藏界面有效；佣兵模式（有bug，重启后失效）下，执行后可能需要重新进入收藏），默认左Ctrl+R");
		keyShowFPS = config.Bind<KeyboardShortcut>("快捷键", "显示/隐藏帧率", new KeyboardShortcut((KeyCode)112, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), "展示或隐藏游戏帧率信息，默认左Ctrl+P");
		keyEmoteGreetings = config.Bind<KeyboardShortcut>("快捷键", "问候", new KeyboardShortcut((KeyCode)49, Array.Empty<KeyCode>()), "表情问候，默认数字键1");
		keyEmoteWellPlayed = config.Bind<KeyboardShortcut>("快捷键", "称赞", new KeyboardShortcut((KeyCode)50, Array.Empty<KeyCode>()), "表情称赞，默认数字键2");
		keyEmoteThanks = config.Bind<KeyboardShortcut>("快捷键", "感谢", new KeyboardShortcut((KeyCode)51, Array.Empty<KeyCode>()), "表情感谢，默认数字键3");
		keyEmoteWow = config.Bind<KeyboardShortcut>("快捷键", "惊叹", new KeyboardShortcut((KeyCode)52, Array.Empty<KeyCode>()), "表情惊叹，默认数字键4");
		keyEmoteOops = config.Bind<KeyboardShortcut>("快捷键", "失误", new KeyboardShortcut((KeyCode)53, Array.Empty<KeyCode>()), "表情失误，默认数字键5");
		keyEmoteThreaten = config.Bind<KeyboardShortcut>("快捷键", "威胁", new KeyboardShortcut((KeyCode)54, Array.Empty<KeyCode>()), "表情威胁，默认数字键6");
		hsLogPath = config.Bind<string>("开发", "炉石日志", "", new ConfigDescription("炉石进程日志文件位置（相对于Hearthstone）", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		hsMatchLogPath = config.Bind<string>("开发", "对局日志", "BepInEx/HsMatch.log", "炉石对局日志文件位置（相对于Hearthstone），参数最先选用命令行");
		autoQuitTimer = config.Bind<long>("开发", "定时退出", 0L, "当游戏运行x秒后（在对局结束时）自动退出，x<=0时该选项无效。");
		isFakeOpenEnable = config.Bind<bool>("开发", "模拟开包状态", false, "是否启用模拟开包（修改该选项后建议重启炉石，启用时可能会导致卡包信息统计异常）");
		buyAdventure = config.Bind<Utils.BuyAdventureTemplate>("开发", "冒险购买", Utils.BuyAdventureTemplate.DoNothing, "（不建议购买卡拉赞）选择一个冒险进行购买尝试（有概率封号，酌情考虑使用）");
		isKarazhanFixEnable = config.Bind<bool>("开发", "卡拉赞修复", false, "（请打完后请关闭，目前无法打序章）卡拉赞黑鸦翱翔修复，也可以用作冒险跳关。（有概率封号，酌情考虑使用）");
		webServerPort = config.Bind<int>("开发", "网站端口", 58744, new ConfigDescription("WebServer端口，参数最先选用命令行", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 65535), Array.Empty<object>()));
		webPageBackImg = config.Bind<string>("开发", "网页背景图", "", new ConfigDescription("网页背景图片", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		isWebshellEnable = config.Bind<bool>("开发", "Webshell", false, "Webshell开关");
		isInternalModeEnable = config.Bind<bool>("开发", "内部模式", false, "是否切换到内部模式（需要重启炉石）");
		fakeDevicePreset = config.Bind<Utils.DevicePreset>("模拟", "设备模拟模板", Utils.DevicePreset.Default, "（重启炉石后生效）模拟设备，用于领取卡包卡背");
		fakeDeviceOs = config.Bind<OSCategory>("模拟", "设备模拟系统", (OSCategory)1, "模拟设备操作系统，当设备模拟模板为Custom时有效。");
		fakeDeviceScreen = config.Bind<ScreenCategory>("模拟", "设备屏幕大小", (ScreenCategory)4, "模拟尺寸（屏幕类型），当设备模拟模板为Custom时有效。");
		fakeDeviceName = config.Bind<string>("模拟", "设备设备型号", "HsMod", "模拟设备型号，当设备模拟模板为Custom时有效。");
		fakePackCount = config.Bind<int>("模拟", "数量", 233, "模拟卡包数量");
		fakeBoosterDbId = config.Bind<BoosterDbId>("模拟", "类型", (BoosterDbId)23, "模拟卡包类型。(替换卡包图标)");
		isFakeRandomResult = config.Bind<bool>("模拟", "随机结果", false, "是否启用随机结果");
		isFakeRandomRarity = config.Bind<bool>("模拟", "随机稀有度", false, "是否随机稀有度（基于随机结果）");
		isFakeRandomPremium = config.Bind<bool>("模拟", "随机品质", false, "是否随机品质（基于随机结果）");
		isFakeAtypicalRandomPremium = config.Bind<bool>("模拟", "随机其他特效", false, "随机品质中包括钻石或异画等（基于随机品质）");
		fakeRandomRarity = config.Bind<Utils.CardRarity>("模拟", "稀有度类型", Utils.CardRarity.LEGENDARY, "指定随机稀有度（基于随机稀有度）");
		fakeRandomPremium = config.Bind<TAG_PREMIUM>("模拟", "品质类型", (TAG_PREMIUM)1, "指定品质（基于随机品质）");
		fakeCardID1 = config.Bind<int>("模拟", "卡牌1", 71984, new ConfigDescription("Card 1 DbID.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardPremium1 = config.Bind<TAG_PREMIUM>("模拟", "卡牌1品质", (TAG_PREMIUM)1, new ConfigDescription("Card 1 Premium.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardID2 = config.Bind<int>("模拟", "卡牌2", 71945, new ConfigDescription("Card 2 DbID.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardPremium2 = config.Bind<TAG_PREMIUM>("模拟", "卡牌2品质", (TAG_PREMIUM)1, new ConfigDescription("Card 2 Premium.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardID3 = config.Bind<int>("模拟", "卡牌3", 73446, new ConfigDescription("Card 3 DbID.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardPremium3 = config.Bind<TAG_PREMIUM>("模拟", "卡牌3品质", (TAG_PREMIUM)1, new ConfigDescription("Card 3 Premium.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardID4 = config.Bind<int>("模拟", "卡牌4", 71781, new ConfigDescription("Card 4 DbID.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardPremium4 = config.Bind<TAG_PREMIUM>("模拟", "卡牌4品质", (TAG_PREMIUM)1, new ConfigDescription("Card 4 Premium.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardID5 = config.Bind<int>("模拟", "卡牌5", 67040, new ConfigDescription("Card 5 DbID.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		fakeCardPremium5 = config.Bind<TAG_PREMIUM>("模拟", "卡牌5品质", (TAG_PREMIUM)1, new ConfigDescription("Card 5 Premium.", (AcceptableValueBase)null, new object[1] { "Advanced" }));
		InitCardsMapping();
		LoadSkinsConfigFromFile();
		ConfigValueDelegate();
		ConfigTemplateSettingChanged(configTemplate.Value);
		timeKeeper = DateTime.Now.Ticks;
		if (CommandConfig.hsMatchLogPath == string.Empty)
		{
			CommandConfig.hsMatchLogPath = hsMatchLogPath.Value;
		}
		if (CommandConfig.webServerPort == -1)
		{
			CommandConfig.webServerPort = webServerPort.Value;
		}
	}

	public static void ConfigValueDelegate()
	{
		configTemplate.SettingChanged += delegate
		{
			ConfigTemplateSettingChanged(configTemplate.Value);
		};
		skinCardBack.SettingChanged += delegate
		{
			if (GameState.Get() != null)
			{
				GameState obj = GameState.Get();
				Player val = ((obj != null) ? obj.GetFriendlySidePlayer() : null);
				if (val != null)
				{
					val.GetCardBackId();
				}
				int num = 0;
				GameState obj2 = GameState.Get();
				Player val2 = ((obj2 != null) ? obj2.GetOpposingSidePlayer() : null);
				if (val2 != null)
				{
					num = val2.GetCardBackId();
				}
				int value = skinCardBack.Value;
				CardBackManager.Get().SetGameCardBackIDs(value, num);
			}
		};
		buyAdventure.SettingChanged += delegate
		{
			if (buyAdventure.Value != Utils.BuyAdventureTemplate.DoNothing)
			{
				Utils.BuyAdventure(buyAdventure.Value);
				buyAdventure.Value = Utils.BuyAdventureTemplate.DoNothing;
			}
		};
	}

	public static void ConfigTemplateSettingChanged(Utils.ConfigTemplate cTemplate)
	{
		switch (cTemplate)
		{
		case Utils.ConfigTemplate.DoNothing:
			break;
		case Utils.ConfigTemplate.AwayFromKeyboard:
			isShortcutsEnable.Value = false;
			isIGMMessageShow.Value = false;
			isAlertPopupShow.Value = false;
			responseAlertPopup.Value = Utils.AlertPopupResponse.YES;
			isOnApplicationFocus.Value = false;
			isRewardToastShow.Value = false;
			isAutoOpenBoxesRewardEnable.Value = true;
			isAutoExit.Value = true;
			isIdleKickEnable.Value = true;
			isQuickPackOpeningEnable.Value = true;
			isAutoRefundCardDisenchantEnable.Value = true;
			isAutoRecvMercenaryRewardEnable.Value = true;
			isMercenaryBattleZoom.Value = false;
			isSkipHeroIntro.Value = true;
			isThinkEmotesEnable.Value = false;
			receiveEnemyEmoteLimit.Value = 0;
			isOpponentGoldenCardShow.Value = false;
			skinCoin.Value = 1746;
			isSkinDefalutHeroEnable.Value = true;
			mercenaryDiamondCardState.Value = Utils.CardState.Disabled;
			randomMercenarySkinEnable.Value = Utils.CardState.Disabled;
			goldenCardState.Value = Utils.CardState.Disabled;
			maxCardState.Value = Utils.CardState.Disabled;
			configTemplate.Value = Utils.ConfigTemplate.DoNothing;
			break;
		case Utils.ConfigTemplate.AntiAwayFromKeyboard:
			isShortcutsEnable.Value = true;
			isIGMMessageShow.Value = true;
			isAlertPopupShow.Value = true;
			responseAlertPopup.Value = Utils.AlertPopupResponse.DONOTHING;
			isOnApplicationFocus.Value = false;
			isRewardToastShow.Value = true;
			isAutoOpenBoxesRewardEnable.Value = false;
			isAutoExit.Value = false;
			isIdleKickEnable.Value = false;
			isQuickPackOpeningEnable.Value = true;
			isAutoRefundCardDisenchantEnable.Value = false;
			isAutoRecvMercenaryRewardEnable.Value = true;
			isMercenaryBattleZoom.Value = false;
			isSkipHeroIntro.Value = true;
			isThinkEmotesEnable.Value = false;
			receiveEnemyEmoteLimit.Value = 3;
			isOpponentGoldenCardShow.Value = true;
			skinCoin.Value = -1;
			isSkinDefalutHeroEnable.Value = false;
			goldenCardState.Value = Utils.CardState.Default;
			maxCardState.Value = Utils.CardState.Default;
			mercenaryDiamondCardState.Value = Utils.CardState.Default;
			randomMercenarySkinEnable.Value = Utils.CardState.Default;
			configTemplate.Value = Utils.ConfigTemplate.DoNothing;
			break;
		}
	}

	public static void InitCardsMapping()
	{
		CardsMapping.Clear();
		Utils.CardMapping item = new Utils.CardMapping
		{
			ThisSkinType = Utils.SkinType.COIN,
			RealDbID = -1,
			FakeDbID = skinCoin.Value,
			RealCardID = "",
			FakeCardID = ""
		};
		if (item.FakeDbID != -1)
		{
			CardsMapping.Add(item);
		}
	}

	public static void UpdateCardsMapping()
	{
		for (int i = 0; i < CardsMapping.Count; i++)
		{
			if (CardsMapping[i].FakeDbID != -1 && CardsMapping[i].FakeCardID == "")
			{
				Utils.CardMapping value = CardsMapping[i];
				value.FakeCardID = GameUtils.TranslateDbIdToCardId(value.FakeDbID, false);
				CardsMapping[i] = value;
			}
		}
	}

	public static void UpdateCardsMappingReal(string realCardID, Utils.SkinType skinType)
	{
		UpdateCardsMapping();
		for (int i = 0; i < CardsMapping.Count; i++)
		{
			if (CardsMapping[i].ThisSkinType == skinType)
			{
				Utils.CardMapping value = CardsMapping[i];
				value.RealCardID = realCardID;
				value.RealDbID = GameUtils.TranslateCardIdToDbId(realCardID, false);
				CardsMapping[i] = value;
				break;
			}
		}
	}

	public static void LoadSkinsConfigFromFile(string file = "BepInEx/config/HsSkins.cfg")
	{
		HeroesMapping.Clear();
		if (File.Exists(file))
		{
			foreach (string item in File.ReadLines(file))
			{
				if (!item.StartsWith("#"))
				{
					string[] array = item.Split(':', StringSplitOptions.None);
					if (array.Length == 2 && !HeroesMapping.ContainsKey(int.Parse(array[0])))
					{
						string[] array2 = array[1].Split(',', StringSplitOptions.None);
						HeroesMapping.Add(int.Parse(array[0].Trim()), int.Parse(array2[new Random().Next(array2.Length)].Trim()));
					}
				}
			}
			return;
		}
		string text = "# 皮肤映射表\n";
		text += "# 说明：主要用作英雄（酒馆、对战）类皮肤替换；按下F4会在BepInEx目录下生成当前全部皮肤信息；\n";
		text += "# 格式：原始皮肤:替换皮肤（:为半角字符）；下一行是一个样例(玛法里奥·怒风替换成大导师玛法里奥)，可以删除\n";
		text += "#      亦支持a:b,c,d这种多值映射，实现随机皮肤。\n";
		text += "274:57761\n\n";
		File.WriteAllText(file, text);
	}
}
public class ConfigValue
{
	public bool IsOpponentRankInGameShowValue
	{
		get
		{
			if (GameUtils.IsGameTypeRanked())
			{
				return PluginConfig.isOpponentRankInGameShow.Value;
			}
			return false;
		}
		set
		{
			PluginConfig.isOpponentRankInGameShow.Value = value;
		}
	}

	public bool IsSkipHeroIntroValue
	{
		get
		{
			return PluginConfig.isSkipHeroIntro.Value;
		}
		set
		{
			PluginConfig.isSkipHeroIntro.Value = value;
		}
	}

	public bool IsShutUpBobEnableValue
	{
		get
		{
			return PluginConfig.isShutUpBobEnable.Value;
		}
		set
		{
			PluginConfig.isShutUpBobEnable.Value = value;
		}
	}

	public bool IsQuickPackOpeningEnableValue
	{
		get
		{
			return PluginConfig.isQuickPackOpeningEnable.Value;
		}
		set
		{
			PluginConfig.isQuickPackOpeningEnable.Value = value;
		}
	}

	public bool IsShowCardLargeCountValue
	{
		get
		{
			return PluginConfig.isShowCardLargeCount.Value;
		}
		set
		{
			PluginConfig.isShowCardLargeCount.Value = value;
		}
	}

	public bool IsMoveEnemyCardsEnableValue
	{
		get
		{
			return PluginConfig.isMoveEnemyCardsEnable.Value;
		}
		set
		{
			PluginConfig.isMoveEnemyCardsEnable.Value = value;
		}
	}

	public bool IsFiresideGatheringEnableValue
	{
		get
		{
			return PluginConfig.isFiresideGatheringEnable.Value;
		}
		set
		{
			PluginConfig.isFiresideGatheringEnable.Value = value;
		}
	}

	public double FiresideGatheringLatitudeValue
	{
		get
		{
			return PluginConfig.firesideGatheringLatitude.Value;
		}
		set
		{
			PluginConfig.firesideGatheringLatitude.Value = value;
		}
	}

	public double FiresideGatheringLongitudeValue
	{
		get
		{
			return PluginConfig.firesideGatheringLongitude.Value;
		}
		set
		{
			PluginConfig.firesideGatheringLongitude.Value = value;
		}
	}

	public double FiresideGatheringGpsAccuracyValue
	{
		get
		{
			return PluginConfig.firesideGatheringGpsAccuracy.Value;
		}
		set
		{
			PluginConfig.firesideGatheringGpsAccuracy.Value = value;
		}
	}

	public bool IsShowFPSEnableValue
	{
		get
		{
			return PluginConfig.isShowFPSEnable.Value;
		}
		set
		{
			PluginConfig.isShowFPSEnable.Value = value;
		}
	}

	public bool IsInternalModeEnableValue
	{
		get
		{
			return PluginConfig.isInternalModeEnable.Value;
		}
		set
		{
			PluginConfig.isInternalModeEnable.Value = value;
		}
	}

	public bool IsAlertPopupShowValue
	{
		get
		{
			return PluginConfig.isAlertPopupShow.Value;
		}
		set
		{
			PluginConfig.isAlertPopupShow.Value = value;
		}
	}

	public Utils.ConfigTemplate ConfigTemplateValue
	{
		set
		{
			PluginConfig.configTemplate.Value = value;
		}
	}

	public bool IsQuickModeEnableValue
	{
		get
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Invalid comparison between Unknown and I4
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Invalid comparison between Unknown and I4
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Invalid comparison between Unknown and I4
			if (PluginConfig.isQuickModeEnable.Value)
			{
				if (!GameMgr.Get().IsBattlegrounds())
				{
					if (GameMgr.Get().IsMercenaries())
					{
						if (!GameMgr.Get().IsAI() && !GameMgr.Get().IsLettuceTutorial() && (int)GameMgr.Get().GetGameType() != 1 && (int)GameMgr.Get().GetGameType() != 33)
						{
							return (int)GameMgr.Get().GetGameType() == 31;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		set
		{
			PluginConfig.isQuickModeEnable.Value = value;
		}
	}

	public bool IsTimeGearEnableValue
	{
		get
		{
			return PluginConfig.isTimeGearEnable.Value;
		}
		set
		{
			PluginConfig.isTimeGearEnable.Value = value;
		}
	}

	public bool TimeGearEnable
	{
		get
		{
			return PluginConfig.isTimeGearEnable.Value;
		}
		set
		{
			PluginConfig.isTimeGearEnable.Value = value;
		}
	}

	public int TimeGearValue
	{
		get
		{
			return PluginConfig.timeGear.Value;
		}
		set
		{
			PluginConfig.timeGear.Value = value;
		}
	}

	public long RunningTime => (DateTime.Now.Ticks - PluginConfig.timeKeeper) / 10000000;

	public string HsMatchLogPathValue
	{
		get
		{
			return PluginConfig.CommandConfig.hsMatchLogPath;
		}
		set
		{
			PluginConfig.hsMatchLogPath.Value = value;
			PluginConfig.CommandConfig.hsMatchLogPath = value;
		}
	}

	public string CacheOpponentFullName
	{
		get
		{
			if (!string.IsNullOrEmpty(Utils.CacheLastOpponentFullName))
			{
				return Utils.CacheLastOpponentFullName;
			}
			BnetPresenceMgr obj = BnetPresenceMgr.Get();
			object value;
			if (obj == null)
			{
				value = null;
			}
			else
			{
				GameState obj2 = GameState.Get();
				object obj3;
				if (obj2 == null)
				{
					obj3 = null;
				}
				else
				{
					Player opposingSidePlayer = obj2.GetOpposingSidePlayer();
					obj3 = ((opposingSidePlayer != null) ? opposingSidePlayer.GetGameAccountId() : null);
				}
				BnetPlayer player = obj.GetPlayer((BnetGameAccountId)obj3);
				value = ((player != null) ? player.GetFullName() : null);
			}
			if (!string.IsNullOrEmpty((string)value))
			{
				BnetPresenceMgr obj4 = BnetPresenceMgr.Get();
				if (obj4 == null)
				{
					return null;
				}
				GameState obj5 = GameState.Get();
				object obj6;
				if (obj5 == null)
				{
					obj6 = null;
				}
				else
				{
					Player opposingSidePlayer2 = obj5.GetOpposingSidePlayer();
					obj6 = ((opposingSidePlayer2 != null) ? opposingSidePlayer2.GetGameAccountId() : null);
				}
				BnetPlayer player2 = obj4.GetPlayer((BnetGameAccountId)obj6);
				if (player2 == null)
				{
					return null;
				}
				return player2.GetFullName();
			}
			return "";
		}
	}

	public static ConfigValue Get()
	{
		return PluginConfig.configValue;
	}
}
public static class PatchManager
{
	public static List<Harmony> AllHarmony = new List<Harmony>();

	public static List<string> AllHarmonyName = new List<string>();

	public static void LoadPatch(Type loadType)
	{
		try
		{
			Harmony val = Harmony.CreateAndPatchAll(loadType, (string)null);
			int num = val.GetPatchedMethods().Count();
			Utils.MyLogger((LogLevel)4, $"{loadType.Name} => Patched {num} methods");
			AllHarmony.Add(val);
			AllHarmonyName.Add(loadType.Name);
		}
		catch (Exception ex)
		{
			Utils.MyLogger((LogLevel)2, $"{loadType.Name} => {ex.Message} \n{ex.InnerException}");
		}
	}

	public static bool UnPatch(string name)
	{
		for (int i = 0; i < AllHarmonyName.Count; i++)
		{
			if (AllHarmonyName[i] == name)
			{
				AllHarmony[i].UnpatchSelf();
				AllHarmonyName.Remove(AllHarmonyName[i]);
				AllHarmony.Remove(AllHarmony[i]);
				Utils.MyLogger((LogLevel)4, "Unpatched " + name + "!");
				return true;
			}
		}
		return false;
	}

	public static void PatchSettingDelegate()
	{
		PluginConfig.isPluginEnable.SettingChanged += delegate
		{
			if (PluginConfig.isPluginEnable.Value)
			{
				PatchAll();
			}
			else
			{
				UnPatchAll();
			}
		};
		PluginConfig.isTimeGearEnable.SettingChanged += delegate
		{
			TimeScaleMgr.Get().Update();
		};
		PluginConfig.timeGear.SettingChanged += delegate
		{
			TimeScaleMgr.Get().Update();
		};
		PluginConfig.isShowCardLargeCount.SettingChanged += delegate
		{
			if (PluginConfig.isShowCardLargeCount.Value)
			{
				LoadPatch(typeof(Patcher.PatchRealtimeCardNum));
			}
			else
			{
				UnPatch("PatchRealtimeCardNum");
			}
		};
		PluginConfig.isIdleKickEnable.SettingChanged += delegate
		{
			InactivePlayerKicker.Get().SetShouldCheckForInactivity(PluginConfig.isIdleKickEnable.Value);
		};
		PluginConfig.targetFrameRate.SettingChanged += delegate
		{
			PluginConfig.graphicsManager = ServiceManager.Get<IGraphicsManager>();
			if (PluginConfig.targetFrameRate.Value > 0 && Options.Get().GetInt((Option)33, 0) != PluginConfig.targetFrameRate.Value)
			{
				PluginConfig.graphicsManager.UpdateTargetFramerate(PluginConfig.targetFrameRate.Value, false);
			}
			else if (PluginConfig.targetFrameRate.Value <= 0)
			{
				PluginConfig.graphicsManager.UpdateTargetFramerate(30, true);
			}
		};
		PluginConfig.isAutoRecvMercenaryRewardEnable.SettingChanged += delegate
		{
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				LoadPatch(typeof(Patcher.PatchMercenariesReward));
			}
			else
			{
				UnPatch("PatchMercenariesReward");
			}
		};
		PluginConfig.isAutoOpenBoxesRewardEnable.SettingChanged += delegate
		{
			if (PluginConfig.isAutoOpenBoxesRewardEnable.Value)
			{
				LoadPatch(typeof(Patcher.PatchBoxesReward));
			}
			else
			{
				UnPatch("PatchBoxesReward");
			}
		};
		PluginConfig.isMoveEnemyCardsEnable.SettingChanged += delegate
		{
			if (PluginConfig.isMoveEnemyCardsEnable.Value)
			{
				LoadPatch(typeof(Patcher.PatchDeathOb));
			}
			else
			{
				UnPatch("PatchDeathOb");
			}
		};
		PluginConfig.isFakeOpenEnable.SettingChanged += delegate
		{
			if (PluginConfig.isFakeOpenEnable.Value)
			{
				LoadPatch(typeof(Patcher.PatchFakePackOpening));
			}
			else
			{
				UnPatch("PatchFakePackOpening");
			}
		};
		PluginConfig.isKarazhanFixEnable.SettingChanged += delegate
		{
			if (PluginConfig.isKarazhanFixEnable.Value)
			{
				LoadPatch(typeof(Patcher.PatchKarazhan));
			}
			else
			{
				UnPatch("PatchKarazhan");
			}
		};
	}

	public static void PatchAll()
	{
		LoadPatch(typeof(Patcher));
		LoadPatch(typeof(Patcher.PatchMisc));
		LoadPatch(typeof(Patcher.PatchFiresideGathering));
		LoadPatch(typeof(Patcher.PatchEmote));
		LoadPatch(typeof(Patcher.PatchIGMMessage));
		LoadPatch(typeof(Patcher.PatchMercenaries));
		LoadPatch(typeof(Patcher.PatchHearthstone));
		LoadPatch(typeof(Patcher.PatchLogArchive));
		LoadPatch(typeof(Patcher.PatchBattlegrounds));
		LoadPatch(typeof(Patcher.PatchFavorite));
		LoadPatch(typeof(Patcher.PatchFakeDevice));
		LoadPatch(typeof(Patcher.PatchDevOptioins));
		if (PluginConfig.isShowCardLargeCount.Value)
		{
			LoadPatch(typeof(Patcher.PatchRealtimeCardNum));
		}
		if (PluginConfig.isMoveEnemyCardsEnable.Value)
		{
			LoadPatch(typeof(Patcher.PatchDeathOb));
		}
		if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
		{
			LoadPatch(typeof(Patcher.PatchMercenariesReward));
		}
		if (PluginConfig.isAutoOpenBoxesRewardEnable.Value)
		{
			LoadPatch(typeof(Patcher.PatchBoxesReward));
		}
		if (PluginConfig.isFakeOpenEnable.Value)
		{
			LoadPatch(typeof(Patcher.PatchFakePackOpening));
		}
		if (PluginConfig.isKarazhanFixEnable.Value)
		{
			LoadPatch(typeof(Patcher.PatchKarazhan));
		}
		TimeScaleMgr.Get().Update();
	}

	public static void UnPatchAll()
	{
		for (int i = 0; i < AllHarmony.Count; i++)
		{
			AllHarmony[i].UnpatchSelf();
			Utils.MyLogger((LogLevel)4, "Unpatched " + AllHarmonyName[i] + "!");
		}
		AllHarmony.Clear();
		AllHarmonyName.Clear();
	}

	public static void RePatchAll()
	{
		UnPatchAll();
		PatchAll();
	}
}
public class Patcher
{
	public class PatchMisc
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(GraphicsResolution), "IsAspectRatioWithinLimit")]
		public static bool PatchAspectRatioWithinLimit(ref bool __result, int width, int height, bool isWindowedMode)
		{
			__result = true;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Screen), "SetResolution", new Type[]
		{
			typeof(int),
			typeof(int),
			typeof(bool)
		})]
		public static bool PatchSetResolution(ref int width, ref int height, ref bool fullscreen)
		{
			if (PluginConfig.CommandConfig.width > 0 && PluginConfig.CommandConfig.height > 0)
			{
				if (width == PluginConfig.CommandConfig.width && height == PluginConfig.CommandConfig.height && !fullscreen)
				{
					return true;
				}
				if (Options.Get().GetInt((Option)13) == width && Options.Get().GetInt((Option)14) == height)
				{
					return false;
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Options), "GetInt", new Type[] { typeof(Option) })]
		public static bool PatchOptionsGetInt(ref Option option, ref int __result)
		{
			if ((int)option == 33 && PluginConfig.targetFrameRate.Value > 0)
			{
				__result = PluginConfig.targetFrameRate.Value;
				return false;
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(DesktopLoginTokenFetcher), "GetTokenFromTokenFetcher")]
		public static IEnumerable<CodeInstruction> PatchGetTokenFromTokenFetcher(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			list.RemoveAt(0);
			list.InsertRange(0, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[4]
			{
				new CodeInstruction(OpCodes.Ldstr, (object)"Aurora.VerifyWebCredentials"),
				new CodeInstruction(OpCodes.Call, (object)new Func<string, VarKey>(Vars.Key).Method),
				new CodeInstruction(OpCodes.Ldnull, (object)null),
				new CodeInstruction(OpCodes.Callvirt, (object)typeof(VarKey).GetMethod("GetStr", BindingFlags.Instance | BindingFlags.Public))
			});
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ConfigFile), "Load", new Type[]
		{
			typeof(string),
			typeof(bool)
		})]
		public static void PatchPreConfigFileLoad(ref string path, ref bool ignoreUselessLines, ConfigFile __instance, out string __state)
		{
			__state = null;
			string pattern = "(CN|KR|TW|EU|US)\\-[a-f0-9]{32}\\-\\d+";
			Match match = Regex.Match(string.Join(" ", Environment.GetCommandLineArgs()), pattern);
			if (!match.Success)
			{
				return;
			}
			__state = match.Value;
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (!string.IsNullOrEmpty(text) && text.Contains("Aurora") && text.Contains("VerifyWebCredentials") && text.Contains("Env"))
				{
					return;
				}
			}
			string text2 = "";
			text2 = (Directory.Exists(PlatformFilePaths.ExternalDataPath + "/Cache/") ? (PlatformFilePaths.ExternalDataPath + "/Cache/") : (Directory.Exists(PlatformFilePaths.PersistentDataPath + "/Cache/") ? (PlatformFilePaths.PersistentDataPath + "/Cache/") : ((!Directory.Exists("./BepInEx/config/")) ? "./" : "./BepInEx/config/")));
			path = text2 + "HsClient.config";
			File.WriteAllText(path, "[Config]\r\nVersion = 3\r\n[Aurora]\r\nVerifyWebCredentials = \"token\"\r\nClientCheck = 0\r\nEnv.Override = 1\r\nEnv = cn.actual.battle.net\r\n");
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ConfigFile), "Load", new Type[]
		{
			typeof(string),
			typeof(bool)
		})]
		public static void PatchPostConfigFileLoad(ref string path, ref bool ignoreUselessLines, ConfigFile __instance, string __state)
		{
			if (!string.IsNullOrEmpty(__state))
			{
				__instance.Set("Aurora.VerifyWebCredentials", __state);
				__instance.Set("Aurora.Env", __state.Substring(0, 2).ToLower() + ".actual.battle.net");
			}
		}

		[HarmonyPrefix]
		[HarmonyPostfix]
		[HarmonyPatch(typeof(ExceptionReporterControl), "ExceptionReportInitialize")]
		public static void PatchExceptionReporterControl()
		{
			ExceptionReporter.Get().SendExceptions = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ExceptionReporter), "ReportCaughtException", new Type[] { typeof(Exception) })]
		public static bool PatchReportCaughtException(ref Exception exception)
		{
			Utils.MyLogger((LogLevel)4, "message:" + exception.Message + "\nInnerException:\n" + exception.InnerException?.ToString() + "\nStackTrace:\n" + exception.StackTrace);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ExceptionReporter), "ReportExceptions")]
		public static bool PatchExceptionReporterReportExceptions()
		{
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ExceptionReporter), "SetSettings")]
		public static bool PatchExceptionReporterSetSettings(ref ExceptionSettings settings, ref bool __result)
		{
			settings = null;
			__result = false;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ExceptionReporter), "get_SubmitURL")]
		public static bool PatchExceptionReporterSubmitURLGetter(ref string __result)
		{
			__result = "";
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(InactivePlayerKicker), "SetShouldCheckForInactivity")]
		public static void PatchSetShouldCheckForInactivity(ref bool check)
		{
			if (!PluginConfig.isIdleKickEnable.Value)
			{
				check = false;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(InactivePlayerKicker), "Update")]
		public static void PatchInactivePlayerKickerUpdate()
		{
			if (!PluginConfig.isIdleKickEnable.Value)
			{
				InactivePlayerKicker.Get().SetShouldCheckForInactivity(PluginConfig.isIdleKickEnable.Value);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DialogManager), "ShowReconnectHelperDialog")]
		[HarmonyPatch(typeof(ReconnectHelperDialog), "Show")]
		[HarmonyPatch(typeof(Network), "OnFatalBnetError")]
		public static bool PatchError()
		{
			if (PluginConfig.isAutoExit.Value)
			{
				Application.Quit();
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AlertPopup), "Show")]
		public static bool PatchAlertPopupShow(ref UIBButton ___m_okayButton, ref UIBButton ___m_confirmButton, ref UIBButton ___m_cancelButton, ref PopupInfo ___m_popupInfo)
		{
			if (PluginConfig.isAlertPopupShow.Value)
			{
				return true;
			}
			if (___m_popupInfo.m_text == GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_LOGIN"))
			{
				Utils.MyLogger((LogLevel)4, ___m_popupInfo.m_text);
				return true;
			}
			switch (PluginConfig.responseAlertPopup.Value)
			{
			case Utils.AlertPopupResponse.OK:
				((PegUIElement)___m_okayButton).TriggerPress();
				((PegUIElement)___m_okayButton).TriggerRelease();
				break;
			case Utils.AlertPopupResponse.CONFIRM:
				((PegUIElement)___m_confirmButton).TriggerPress();
				((PegUIElement)___m_confirmButton).TriggerRelease();
				break;
			case Utils.AlertPopupResponse.CANCEL:
				((PegUIElement)___m_cancelButton).TriggerPress();
				((PegUIElement)___m_cancelButton).TriggerRelease();
				break;
			case Utils.AlertPopupResponse.YES:
				if (((Component)___m_confirmButton).gameObject.activeSelf)
				{
					((PegUIElement)___m_confirmButton).TriggerPress();
					((PegUIElement)___m_confirmButton).TriggerRelease();
				}
				else
				{
					((PegUIElement)___m_okayButton).TriggerPress();
					((PegUIElement)___m_okayButton).TriggerRelease();
				}
				break;
			}
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AlertPopup), "UpdateInfo")]
		public static void PatchAlertPopupUpdateInfo(ref PopupInfo info, ref UIBButton ___m_okayButton, ref UIBButton ___m_confirmButton, ref UIBButton ___m_cancelButton, ref PopupInfo ___m_popupInfo)
		{
			if (!PluginConfig.isAlertPopupShow.Value && (info.m_text == GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT") || ___m_popupInfo.m_text == GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT")))
			{
				Utils.MyLogger((LogLevel)4, info.m_text);
				if (((Component)___m_confirmButton).gameObject.activeSelf)
				{
					((PegUIElement)___m_confirmButton).TriggerPress();
					((PegUIElement)___m_confirmButton).TriggerRelease();
				}
				else
				{
					((PegUIElement)___m_okayButton).TriggerPress();
					((PegUIElement)___m_okayButton).TriggerRelease();
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SplashScreen), "GetRatingsScreenRegion")]
		public static bool PatchGetRatingsScreenRegion()
		{
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuestPopups), "ShowNextQuestNotification")]
		[HarmonyPatch(typeof(EndGameScreen), "ShowMercenariesExperienceRewards")]
		public static bool PatchEndGameScreen()
		{
			return PluginConfig.isRewardToastShow.Value;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RewardTrack), "HandleRewardGranted")]
		public static bool PatchHandleRewardGranted(int rewardTrackId, int level, bool forPaidTrack, List<RewardItemOutput> rewardItemOutput)
		{
			if (!PluginConfig.isRewardToastShow.Value)
			{
				RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack((RewardTrackType)1);
				if (rewardTrack != null)
				{
					rewardTrack.AckReward(rewardTrackId, level, forPaidTrack);
				}
				RewardTrack rewardTrack2 = RewardTrackManager.Get().GetRewardTrack((RewardTrackType)2);
				if (rewardTrack2 != null)
				{
					rewardTrack2.AckReward(rewardTrackId, level, forPaidTrack);
				}
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RewardPresenter), "ShowNextReward")]
		public static bool PatchRewardPresenterShowNextReward(RewardPresenter __instance, ref Action onHiddenCallback)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			Utils.MyLogger((LogLevel)4, "ShowNextReward");
			if (!PluginConfig.isRewardToastShow.Value)
			{
				__instance.Clear();
				List<ProfileNotice> notices = NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices;
				for (int i = 0; i < notices.Count; i++)
				{
					ProfileNotice val = notices[i];
					if (val != null)
					{
						Utils.MyLogger((LogLevel)4, $"NetCacheProfileNotices {val.Origin} {val.Type} {val.NoticeID}");
						Network.Get().AckNotice(val.NoticeID);
					}
				}
				NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices.Clear();
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CollectionManager), "RegisterCollectionNetHandlers")]
		public static void PatchRegisterCollectionNetHandlers()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			try
			{
				Network.Get().RemoveNetHandler((object)(PacketID)258, new NetHandler(Utils.TryRefundCardDisenchantCallback));
			}
			catch
			{
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PackOpening), "AutomaticallyOpenPack")]
		public static IEnumerable<CodeInstruction> PatchAutomaticallyOpenPack(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Expected O, but got Unknown
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Ret);
			if (num > 0)
			{
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsQuickPackOpeningEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldfld, (object)typeof(PackOpening).GetField("m_director", BindingFlags.Instance | BindingFlags.NonPublic)));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)new Func<PackOpeningDirector, IEnumerator>(PackOpeningDirectorPatch.ForceRevealAllCards).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)typeof(MonoBehaviour).GetMethod("StartCoroutine", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(IEnumerator) }, null)));
				list.Insert(num, new CodeInstruction(OpCodes.Pop, (object)null));
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PackOpening), "SpaceToOpenPack")]
		public static bool PatchSpaceToOpenPack(ref bool __result)
		{
			if (PackOpeningDirectorPatch.m_WaitingForAllCardsRevealed)
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PackOpening), "HandleKeyboardInput")]
		public static bool PatchPackOpeningHandleKeyboardInput(ref bool __result)
		{
			if (PackOpeningDirectorPatch.m_WaitingForAllCardsRevealed && PluginConfig.isAutoPackOpeningEnable.Value && PluginConfig.isQuickPackOpeningEnable.Value)
			{
				__result = true;
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PackOpeningDirector), "OnSpellFinished")]
		public static void PatchPackOpeningDirectorOnSpellFinished()
		{
			PackOpeningDirectorPatch.m_WaitingForCards = false;
			if (PluginConfig.isAutoRefundCardDisenchantEnable.Value)
			{
				Utils.TryRefundCardDisenchant();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PackOpeningDirector), "Awake")]
		public static void PatchPackOpeningDirectorAwake()
		{
			PackOpeningDirectorPatch.m_WaitingForCards = true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ShowFPS), "Awake")]
		public static IEnumerable<CodeInstruction> PatchShowFPSAwake(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Brfalse_S || list[i].opcode == OpCodes.Brfalse)
				{
					num = i;
					break;
				}
			}
			num--;
			if (num > 0)
			{
				for (int j = 0; j < 5; j++)
				{
					list.RemoveAt(num);
				}
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShowFPS), "OnGUI")]
		public static bool PatchOnGUI(ref Camera[] ___m_cameras, ref string ___m_fpsText)
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Expected O, but got Unknown
			int num = 0;
			if (___m_cameras.Length < Camera.allCamerasCount)
			{
				___m_cameras = (Camera[])(object)new Camera[Camera.allCamerasCount];
			}
			int allCameras = Camera.GetAllCameras(___m_cameras);
			FullScreenEffects val = default(FullScreenEffects);
			for (int i = 0; i < allCameras; i++)
			{
				if (((Component)___m_cameras[i]).TryGetComponent<FullScreenEffects>(ref val) && val.IsActive)
				{
					num++;
				}
			}
			string text = ___m_fpsText;
			try
			{
				text = text.Split('.', StringSplitOptions.None)[0];
			}
			catch
			{
				text = "-1";
			}
			if (PluginConfig.isShowFPSEnable.Value)
			{
				GUI.Box(new Rect(0f, 0f, 0f, 0f), text, new GUIStyle(GUIStyle.op_Implicit("box"))
				{
					clipping = (TextClipping)0,
					stretchHeight = true,
					stretchWidth = true,
					wordWrap = true,
					fontSize = 54,
					alignment = (TextAnchor)4,
					contentOffset = new Vector2(64f, 48f)
				});
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CollectionCardVisual), "EnterCraftingMode")]
		public static bool PatchEnterCraftingMode(CollectionCardVisual __instance)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected I4, but got Unknown
			if (PluginConfig.isShowCollectionCardIdEnable.Value)
			{
				int num = -1;
				ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
				switch ((int)viewMode)
				{
				case 0:
				case 1:
				case 5:
				case 6:
				case 7:
					if (__instance.CardId != "")
					{
						num = GameUtils.TranslateCardIdToDbId(__instance.CardId, false);
					}
					break;
				case 2:
					num = ((Component)__instance.GetActor()).GetComponent<CollectionCardBack>().GetCardBackId();
					break;
				default:
					return true;
				}
				UIStatus.Get().AddInfo($"ID: {num}");
				ClipboardUtils.CopyToClipboard(num.ToString());
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(BaconCollectionDisplay), "ShowFinisherDetailsDisplay")]
		public static void PatchShowFinisherDetailsDisplay(ref BattlegroundsFinisherDataModel dataModel, ref BattlegroundsFinisherCollectionPageDataModel pageModel)
		{
			if (PluginConfig.isShowCollectionCardIdEnable.Value)
			{
				UIStatus.Get().AddInfo($"ID: {dataModel.FinisherDbiId}");
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(BaconCollectionDisplay), "ShowBoardDetailsDisplay")]
		public static void PatchShowBoardDetailsDisplay(ref BattlegroundsBoardSkinDataModel dataModel, ref BattlegroundsBoardSkinCollectionPageDataModel pageModel)
		{
			if (PluginConfig.isShowCollectionCardIdEnable.Value)
			{
				UIStatus.Get().AddInfo($"ID: {dataModel.BoardDbiId}");
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AdventureDungeonCrawlDisplay), "Update")]
		public static void PatchAdventureDungeonCrawlDisplayUpdate(ref GameObject ___m_retireButton)
		{
			if (PluginConfig.isShowRetireForever.Value)
			{
				GameObject obj = ___m_retireButton;
				if (obj != null)
				{
					obj.SetActive(true);
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HearthstoneApplication), "OnApplicationFocus")]
		public static bool PatchOnApplicationFocus(bool focus)
		{
			return PluginConfig.isOnApplicationFocus.Value;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(TimeScaleMgr), "Update")]
		public static bool PatchTimeScaleMgr(ref float ___m_timeScaleMultiplier, ref float ___m_gameTimeScale)
		{
			if (PluginConfig.isTimeGearEnable.Value)
			{
				float num = 1f;
				if (PluginConfig.timeGear.Value > 1)
				{
					num = PluginConfig.timeGear.Value;
				}
				else if (PluginConfig.timeGear.Value < -1)
				{
					num = -1f / (float)PluginConfig.timeGear.Value;
				}
				if (num >= 8f)
				{
					num = 8f;
				}
				else if (num <= -8f)
				{
					num = 0.125f;
				}
				Time.timeScale = ((num > ___m_timeScaleMultiplier) ? ((num + (___m_timeScaleMultiplier - 1f) * 0.5f) * ___m_gameTimeScale) : ((___m_timeScaleMultiplier + (num - 1f) * 0.5f) * ___m_gameTimeScale));
				return false;
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SocialToastMgr), "AddToast", new Type[]
		{
			typeof(UserAttentionBlocker),
			typeof(string),
			typeof(TOAST_TYPE),
			typeof(float),
			typeof(bool)
		})]
		public static IEnumerable<CodeInstruction> PatchSocialToastMgrAddToast(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ret);
			if (num > 0)
			{
				num++;
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_3, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)typeof(Time).GetProperty("timeScale", BindingFlags.Static | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Mul, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Starg_S, (object)(byte)3));
			}
			return list;
		}
	}

	public class PatchBoxesReward
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(RewardBoxesDisplay), "RewardPackageOnComplete")]
		public static void PatchRewardPackageOnComplete(RewardBoxData boxData)
		{
			if (PluginConfig.isAutoOpenBoxesRewardEnable.Value)
			{
				((PegUIElement)boxData.m_RewardPackage).TriggerPress();
				((PegUIElement)boxData.m_RewardPackage).TriggerRelease();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RewardBoxesDisplay), "OnDoneButtonShown")]
		public static void PatchOnDoneButtonShown(Spell spell, object userData)
		{
			if (PluginConfig.isAutoOpenBoxesRewardEnable.Value)
			{
				((PegUIElement)RewardBoxesDisplay.Get().m_DoneButton).TriggerPress();
				((PegUIElement)RewardBoxesDisplay.Get().m_DoneButton).TriggerRelease();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RewardBoxesDisplay), "OnBonusLootButtonShown")]
		public static void PatchOnBonusLootButtonShown(Spell spell, object userData)
		{
			if (PluginConfig.isAutoOpenBoxesRewardEnable.Value)
			{
				((PegUIElement)RewardBoxesDisplay.Get().m_BonusLootNextButton).TriggerPress();
				((PegUIElement)RewardBoxesDisplay.Get().m_BonusLootNextButton).TriggerRelease();
			}
		}
	}

	public class PatchDevOptioins
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(HearthstoneApplication), "GetMode")]
		public static IEnumerable<CodeInstruction> PatchHearthstoneApplicationGetMode(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Expected O, but got Unknown
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Expected O, but got Unknown
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Brtrue);
			if (num > 0)
			{
				num++;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				if (list[num].opcode == OpCodes.Ldc_I4_2)
				{
					list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
					list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsInternalModeEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
					list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
					list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
					list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsInternalModeEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
					Label label2 = generator.DefineLabel();
					list.Insert(num, new CodeInstruction(OpCodes.Brtrue_S, (object)label2));
					num += 3;
					list.Insert(num, new CodeInstruction(OpCodes.Ldc_I4_1, (object)null));
					list[num++].labels.Add(label2);
					list.Insert(num, new CodeInstruction(OpCodes.Ret, (object)null));
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		public static IEnumerable<CodeInstruction> PatchJob_InitializeMode(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Expected O, but got Unknown
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Expected O, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Expected O, but got Unknown
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Expected O, but got Unknown
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Brfalse);
			if (num > 0 && list[num].operand is Label label)
			{
				Label label2 = generator.DefineLabel();
				list[num].operand = label2;
				num += 3;
				if (list[num].opcode == OpCodes.Ldc_I4_2)
				{
					list.Insert(num, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
					list[num++].labels.Add(label2);
					list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsInternalModeEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
					list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
					list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
					list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsInternalModeEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
					Label label3 = generator.DefineLabel();
					list.Insert(num, new CodeInstruction(OpCodes.Brtrue_S, (object)label3));
					num += 2;
					Label label4 = generator.DefineLabel();
					list.Insert(num++, new CodeInstruction(OpCodes.Br_S, (object)label4));
					list.Insert(num, new CodeInstruction(OpCodes.Ldc_I4_1, (object)null));
					list[num++].labels.Add(label3);
					list[num].labels.Add(label4);
				}
			}
			return list;
		}
	}

	public class PatchRealtimeCardNum
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CollectionCardCount), "UpdateVisibility")]
		public static IEnumerable<CodeInstruction> PatchCollectionCardCountUpdateVisibility(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldc_I4_S && (sbyte)list[i].operand == 10)
				{
					num = i;
					break;
				}
			}
			num--;
			if (num > 0)
			{
				list[num] = new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method);
				list[num + 1] = new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsShowCardLargeCountValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod());
				list[num + 2] = new CodeInstruction(OpCodes.Brfalse_S, list[num + 2].operand);
			}
			return list;
		}
	}

	public class PatchDeathOb
	{
		private static readonly MethodInfo registerCreateGameListenerInfo = typeof(GameState).GetMethod("RegisterCreateGameListener", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
		{
			typeof(CreateGameCallback),
			typeof(object)
		}, null);

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(FriendMgr), "OnSceneLoaded")]
		public static IEnumerable<CodeInstruction> PatchOnSceneLoaded(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Expected O, but got Unknown
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Ldftn && (x.operand as MethodInfo).Name == "OnGameOver");
			if (num > 0)
			{
				list.Insert(num++, new CodeInstruction(OpCodes.Ldftn, (object)new Action<CreateGamePhase, object>(FriendMgrPatch.OnGameCreated).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Newobj, (object)typeof(CreateGameCallback).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
				{
					typeof(object),
					typeof(IntPtr)
				}, null)));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)registerCreateGameListenerInfo));
				list.Insert(num++, new CodeInstruction(OpCodes.Pop, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ZoneHand), "GetCardScale")]
		public static IEnumerable<CodeInstruction> PatchZoneHandGetCardScale(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Expected O, but got Unknown
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Expected O, but got Unknown
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Expected O, but got Unknown
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Expected O, but got Unknown
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Expected O, but got Unknown
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Expected O, but got Unknown
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Expected O, but got Unknown
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ldfld && (x.operand as FieldInfo).Name == "enemyHand");
			if (num > 0)
			{
				num++;
				object operand = list[num++].operand;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsMoveEnemyCardsEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldfld, (object)typeof(ZoneHand).GetField("m_controller", BindingFlags.Instance | BindingFlags.NonPublic)));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(Player).GetMethod("IsRevealed", BindingFlags.Instance | BindingFlags.Public)));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)0.41f));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)0.085f));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)0.41f));
				list.Insert(num++, new CodeInstruction(OpCodes.Newobj, (object)typeof(Vector3).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
				{
					typeof(float),
					typeof(float),
					typeof(float)
				}, null)));
				list.Insert(num++, new CodeInstruction(OpCodes.Ret, (object)null));
				list.Insert(num, new CodeInstruction(OpCodes.Br_S, operand));
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ZoneHand), "GetCardRotation", new Type[]
		{
			typeof(int),
			typeof(int)
		})]
		public static IEnumerable<CodeInstruction> PatchZoneHandGetCardRotation(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected O, but got Unknown
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Expected O, but got Unknown
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Expected O, but got Unknown
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "IsRevealed");
			if (num > 0)
			{
				num++;
				object operand = list[num++].operand;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsMoveEnemyCardsEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Conv_R4, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Mul, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_1, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Add, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Stloc_3, (object)null));
				list.Insert(num, new CodeInstruction(OpCodes.Br_S, operand));
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ZoneHand), "GetCardPosition", new Type[]
		{
			typeof(int),
			typeof(int)
		})]
		public static IEnumerable<CodeInstruction> PatchZoneHandGetCardPosition(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Expected O, but got Unknown
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Expected O, but got Unknown
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Expected O, but got Unknown
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Expected O, but got Unknown
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Expected O, but got Unknown
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Expected O, but got Unknown
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Expected O, but got Unknown
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Expected O, but got Unknown
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Expected O, but got Unknown
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Expected O, but got Unknown
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Expected O, but got Unknown
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Expected O, but got Unknown
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Expected O, but got Unknown
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Expected O, but got Unknown
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Expected O, but got Unknown
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Expected O, but got Unknown
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Expected O, but got Unknown
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Expected O, but got Unknown
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Expected O, but got Unknown
			//IL_032a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Expected O, but got Unknown
			//IL_0353: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Expected O, but got Unknown
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_038a: Expected O, but got Unknown
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Expected O, but got Unknown
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bb: Expected O, but got Unknown
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03da: Expected O, but got Unknown
			//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f0: Expected O, but got Unknown
			//IL_0401: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Expected O, but got Unknown
			//IL_0413: Unknown result type (might be due to invalid IL or missing references)
			//IL_041d: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "IsRevealed");
			if (num > 0)
			{
				num++;
				object operand = list[num++].operand;
				Label label = generator.DefineLabel();
				Label label2 = generator.DefineLabel();
				Label label3 = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsMoveEnemyCardsEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldflda, (object)typeof(ZoneHand).GetField("m_centerOfHand", BindingFlags.Instance | BindingFlags.NonPublic)));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldfld, (object)typeof(Vector3).GetField("z", BindingFlags.Instance | BindingFlags.Public)));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_2, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Div, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Sub, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<int, int>(Mathf.Abs).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Conv_R4, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)2f));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<float, float, float>(Mathf.Pow).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_4, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Mul, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Conv_R4, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Div, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_1, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Brtrue_S, (object)label2));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Insert(num++, new CodeInstruction(OpCodes.Br_S, (object)label3));
				list.Insert(num, new CodeInstruction(OpCodes.Ldc_R4, (object)1f));
				list[num++].labels.Add(label2);
				list.Insert(num, new CodeInstruction(OpCodes.Mul, (object)null));
				list[num++].labels.Add(label3);
				list.Insert(num++, new CodeInstruction(OpCodes.Sub, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldloc_S, (object)(byte)6));
				list.Insert(num++, new CodeInstruction(OpCodes.Sub, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_R4, (object)0.6f));
				list.Insert(num++, new CodeInstruction(OpCodes.Sub, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Stloc_S, (object)(byte)5));
				list.Insert(num, new CodeInstruction(OpCodes.Br_S, operand));
			}
			return list;
		}
	}

	public class PatchFiresideGathering
	{
		private static readonly MethodInfo ChangeStateInfo = typeof(FiresideGatheringLocationHelperDialog).GetMethod("ChangeState", BindingFlags.Instance | BindingFlags.NonPublic);

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FiresideGatheringManager), "RequestNearbyFSGs")]
		public static bool PatchRequestNearbyFSGs(ref bool ___m_isRequestNearbyFSGsPending)
		{
			if (PluginConfig.isFiresideGatheringEnable.Value)
			{
				___m_isRequestNearbyFSGsPending = true;
				Network.Get().RequestNearbyFSGs(PluginConfig.firesideGatheringLatitude.Value, PluginConfig.firesideGatheringLongitude.Value, PluginConfig.firesideGatheringGpsAccuracy.Value, (List<string>)null);
				return false;
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(FiresideGatheringManager), "OnFSGAllowed")]
		public static IEnumerable<CodeInstruction> PatchOnFSGAllowed(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "get_GPSOrWifiServicesAvailable");
			if (num > 0)
			{
				num++;
				object operand = list[num].operand;
				list.Insert(num++, new CodeInstruction(OpCodes.Brtrue_S, operand));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsFiresideGatheringEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(FiresideGatheringLocationHelperDialog), "Start")]
		public static IEnumerable<CodeInstruction> PatchFiresideGatheringLocationHelperDialogStart(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Expected O, but got Unknown
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Expected O, but got Unknown
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Ldfld && (x.operand as FieldInfo).Name == "m_isCheckInFailure");
			if (num > 0)
			{
				num--;
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsFiresideGatheringEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_3, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)ChangeStateInfo));
				list.Insert(num++, new CodeInstruction(OpCodes.Ret, (object)null));
			}
			return list;
		}
	}

	public class PatchEmote
	{
		private static int m_lastPlayerId;

		private static float m_lastEnemyEmoteTime;

		private static int m_chainedEnemyEmotes;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MatchingQueueTab), "Update")]
		[HarmonyPatch(typeof(ThinkEmoteManager), "Update")]
		public static IEnumerable<CodeInstruction> PatchThinkEmoteManagerOrMatchingQueueTabUpdate(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			MethodInfo deltaTimeInfo = typeof(Time).GetProperty("deltaTime", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
			MethodInfo getMethod = typeof(Time).GetProperty("unscaledDeltaTime", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Call && x.operand as MethodInfo == deltaTimeInfo);
			if (num > 0)
			{
				list[num].operand = getMethod;
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ThinkEmoteManager), "Update")]
		public static bool PatchThinkEmoteManagerUpdate()
		{
			return PluginConfig.isThinkEmotesEnable.Value;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EmoteHandler), "EmoteSpamBlocked")]
		public static bool PatchUpdateAllMutes(ref float ___m_timeSinceLastEmote, ref bool __result)
		{
			if (PluginConfig.isExtendedBMEnable.Value)
			{
				if (___m_timeSinceLastEmote < 1.5f)
				{
					__result = true;
					return false;
				}
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EnemyEmoteHandler), "IsSquelched")]
		public static bool PatchIsSquelched(ref int playerId, ref bool __result)
		{
			if (PluginConfig.receiveEnemyEmoteLimit.Value == 0)
			{
				__result = true;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RemoteActionHandler), "CanReceiveEnemyEmote")]
		public static bool PatchPreCanReceiveEnemyEmote(ref EmoteType emoteType, ref int playerId, ref bool __result)
		{
			if (PluginConfig.receiveEnemyEmoteLimit.Value == 0)
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RemoteActionHandler), "CanReceiveEnemyEmote")]
		public static void PatchPostCanReceiveEnemyEmote(ref EmoteType emoteType, ref int playerId, ref bool __result)
		{
			int value;
			if (!__result || (value = PluginConfig.receiveEnemyEmoteLimit.Value) <= 0)
			{
				return;
			}
			float time = Time.time;
			if (m_lastPlayerId == playerId)
			{
				if (m_lastEnemyEmoteTime != 0f && time - m_lastEnemyEmoteTime < 9f)
				{
					m_chainedEnemyEmotes++;
					if (m_chainedEnemyEmotes > value)
					{
						EnemyEmoteHandler.Get().SquelchPlayer(playerId);
					}
				}
				else
				{
					m_chainedEnemyEmotes = 1;
				}
			}
			else
			{
				m_chainedEnemyEmotes = 1;
				m_lastPlayerId = playerId;
			}
			m_lastEnemyEmoteTime = time;
		}
	}

	public class PatchBattlegrounds
	{
		[HarmonyTranspiler]
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		public static IEnumerable<CodeInstruction> PatchPlayBobLineWithoutText(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Stfld && (x.operand as FieldInfo).Name == "<>1__state");
			if (num > 0)
			{
				num++;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsShutUpBobEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ret, (object)null));
			}
			return list;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(TB_BaconShop), "GetBobActor")]
		public static void PatchGetBobActor(ref Actor __result)
		{
			if ((Object)(object)__result != (Object)null && ConfigValue.Get().IsShutUpBobEnableValue && GameMgr.Get().IsBattlegrounds())
			{
				__result.ActivateSpellBirthState((SpellType)122);
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		public static IEnumerable<CodeInstruction> PatchHandleGameOverWithTiming(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "UpdateLayout");
			if (num > 0)
			{
				num++;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsShutUpBobEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Ret, (object)null));
			}
			return list;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyReversePatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPatch(typeof(SpellController), "OnProcessTaskList")]
		public static void AttackSpellControllerBase(AttackSpellController instance)
		{
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AttackSpellController), "OnProcessTaskList")]
		public static bool PatchAttackSpellControllerOnProcessTaskList(AttackSpellController __instance)
		{
			if (ConfigValue.Get().IsQuickModeEnableValue)
			{
				AttackSpellControllerBase(__instance);
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyReversePatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPatch(typeof(SpellController), "OnProcessTaskList")]
		public static void DeathSpellControllerBase(DeathSpellController instance)
		{
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DeathSpellController), "OnProcessTaskList")]
		public static bool PatchDeathSpellControllerOnProcessTaskList(DeathSpellController __instance)
		{
			if (ConfigValue.Get().IsQuickModeEnableValue)
			{
				DeathSpellControllerBase(__instance);
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyReversePatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPatch(typeof(SpellController), "OnProcessTaskList")]
		public static void TriggerSpellControllerBase(TriggerSpellController instance)
		{
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(TriggerSpellController), "OnProcessTaskList")]
		public static bool PatchTriggerSpellControllerOnProcessTaskList(TriggerSpellController __instance)
		{
			if (ConfigValue.Get().IsQuickModeEnableValue)
			{
				TriggerSpellControllerBase(__instance);
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyReversePatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPatch(typeof(SpellController), "OnProcessTaskList")]
		public static void SubSpellControllerBase(SubSpellController instance)
		{
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SubSpellController), "OnProcessTaskList")]
		public static bool PatchSubSpellControllerOnProcessTaskList(SubSpellController __instance)
		{
			if (ConfigValue.Get().IsQuickModeEnableValue)
			{
				SubSpellControllerBase(__instance);
				return false;
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SideQuestSpellController), "OnProcessTaskList")]
		public static IEnumerable PatchSubSideQuestSpellControllerOnProcessTaskList(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "SetSecretTriggered");
			if (num > 0)
			{
				Label label = generator.DefineLabel();
				list[++num].labels.Add(label);
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsQuickModeEnableValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)typeof(SpellController).GetMethod("OnProcessTaskList", BindingFlags.Instance | BindingFlags.NonPublic)));
				list.Insert(num++, new CodeInstruction(OpCodes.Ret, (object)null));
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameState), "IsUsingFastActorTriggers")]
		public static bool PatchGameStateIsUsingFastActorTriggers(bool __result)
		{
			if (ConfigValue.Get().IsQuickModeEnableValue)
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	public class PatchHearthstone
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Entity), "GetPremiumType")]
		public static bool PatchGetPremiumType(Entity __instance, ref TAG_PREMIUM __result)
		{
			try
			{
				if (GameMgr.Get() != null && GameState.Get() != null && GameState.Get().IsGameCreatedOrCreating())
				{
					if (GameMgr.Get().IsBattlegrounds() && (!PluginConfig.isBgsGoldenEnable.Value || ((EntityBase)__instance).IsMinion() || ((EntityBase)__instance).IsQuest()))
					{
						return true;
					}
					Utils.CardState value = PluginConfig.goldenCardState.Value;
					Utils.CardState value2 = PluginConfig.maxCardState.Value;
					int num = GameUtils.TranslateCardIdToDbId(((EntityBase)__instance).GetCardId(), false);
					bool flag = false;
					bool flag2 = false;
					if (Utils.CheckInfo.IsMercenarySkin(((EntityBase)__instance).GetCardId(), out var skin))
					{
						flag2 = true;
						if (num == skin.Diamond)
						{
							flag = true;
						}
					}
					if (__instance.IsControlledByOpposingSidePlayer() && !PluginConfig.isOpponentGoldenCardShow.Value && !GameMgr.Get().IsBattlegrounds())
					{
						__result = (TAG_PREMIUM)0;
						if (flag2)
						{
							((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)0);
							((EntityBase)__instance).SetTag<bool>((GAME_TAG)1932, false);
						}
						return false;
					}
					if (((EntityBase)__instance).HasTag((GAME_TAG)1932) || ((EntityBase)__instance).HasTag((GAME_TAG)2589) || flag)
					{
						if ((((EntityBase)__instance).HasTag((GAME_TAG)1932) || flag) && (value2 == Utils.CardState.All || (value2 == Utils.CardState.OnlyMy && __instance.IsControlledByFriendlySidePlayer())))
						{
							if (flag)
							{
								((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)2);
								((EntityBase)__instance).SetTag<bool>((GAME_TAG)1932, true);
							}
							__result = (TAG_PREMIUM)2;
							return false;
						}
						if (((EntityBase)__instance).HasTag((GAME_TAG)2589) && PluginConfig.isSignatureCardStateEnable.Value && (value2 == Utils.CardState.All || (value2 == Utils.CardState.OnlyMy && __instance.IsControlledByFriendlySidePlayer())))
						{
							__result = (TAG_PREMIUM)3;
							((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)3);
							return false;
						}
						if (value2 == Utils.CardState.Disabled && value == Utils.CardState.Disabled)
						{
							__result = (TAG_PREMIUM)0;
							if (flag2)
							{
								((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)0);
								((EntityBase)__instance).SetTag<bool>((GAME_TAG)1932, false);
							}
							return false;
						}
					}
					if (value == Utils.CardState.All || (value == Utils.CardState.OnlyMy && __instance.IsControlledByFriendlySidePlayer()))
					{
						((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)1);
						__result = (TAG_PREMIUM)1;
						return false;
					}
					if (value == Utils.CardState.Disabled)
					{
						__result = (TAG_PREMIUM)0;
						if (flag2)
						{
							((EntityBase)__instance).SetTag<TAG_PREMIUM>((GAME_TAG)12, (TAG_PREMIUM)0);
							((EntityBase)__instance).SetTag<bool>((GAME_TAG)1932, false);
						}
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Utils.MyLogger((LogLevel)2, ex);
				Utils.MyLogger((LogLevel)2, ex.StackTrace);
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PlayerLeaderboardManager), "SetNextOpponent")]
		public static IEnumerable<CodeInstruction> PatchSetNextOpponent(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ret);
			if (num > 0)
			{
				num += 2;
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Action<int>(PlayerLeaderboardManagerPatch.UpdateCurrentOpponent).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
			}
			return list;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PlayerLeaderboardManager), "SetCurrentOpponent")]
		public static void PatchSetCurrentOpponent(ref int opponentPlayerId)
		{
			if (opponentPlayerId != -1)
			{
				PlayerLeaderboardManagerPatch.UpdateCurrentOpponent(opponentPlayerId);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BnetPlayer), "GetBestName")]
		public static bool PatchBnetPlayerGetBestName(BnetPlayer __instance, ref BnetAccount ___m_account, ref Map<BnetGameAccountId, BnetGameAccount> ___m_gameAccounts, ref BnetGameAccount ___m_hsGameAccount, ref string __result)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			if (__instance != BnetPresenceMgr.Get().GetMyPlayer())
			{
				if (___m_account != (BnetAccount)null)
				{
					string fullName = ___m_account.GetFullName();
					if (fullName != null)
					{
						__result = fullName;
						return false;
					}
					if (___m_account.GetBattleTag() != (BnetBattleTag)null)
					{
						__result = ___m_account.GetBattleTag().GetName();
						return false;
					}
				}
				Enumerator<BnetGameAccountId, BnetGameAccount> enumerator = ___m_gameAccounts.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<BnetGameAccountId, BnetGameAccount> current = enumerator.Current;
						if (current.Value.GetBattleTag() != (BnetBattleTag)null)
						{
							__result = (PluginConfig.isFullnameShow.Value ? ((object)current.Value.GetBattleTag()).ToString() : current.Value.GetBattleTag().GetName());
							Utils.CacheLastOpponentFullName = ((object)current.Value.GetBattleTag()).ToString();
							Utils.CacheLastOpponentAccountID = current.Value.GetOwnerId();
							return false;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
				__result = null;
				return false;
			}
			BnetBattleTag battleTag = ___m_hsGameAccount.GetBattleTag();
			__result = ((battleTag != null) ? battleTag.GetName() : null);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BnetRecentPlayerMgr), "IsCurrentOpponent")]
		public static bool PatchIsCurrentOpponent(BnetPlayer player, ref bool __result)
		{
			if (PluginConfig.isFullnameShow.Value || PluginConfig.isShortcutsEnable.Value)
			{
				StackFrame[] frames = new StackTrace().GetFrames();
				for (int i = 0; i < frames.Length; i++)
				{
					if (frames[i].GetMethod().Name == "ShouldEnableOption")
					{
						__result = false;
						return false;
					}
				}
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		public static IEnumerable<CodeInstruction> PatchUpdateMedalWhenReady(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "get_ShowOpponentRankInGame");
			if (num > 0)
			{
				num++;
				object operand = list[num].operand;
				list.Insert(num++, new CodeInstruction(OpCodes.Brtrue_S, operand));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsOpponentRankInGameShowValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(NameBanner), "Initialize")]
		public static IEnumerable<CodeInstruction> PatchNameBannerInitialize(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Call && (x.operand as MethodInfo).Name == "IsGameTypeRanked");
			if (num > 0)
			{
				num++;
				Label label = generator.DefineLabel();
				list[num].labels.Add(label);
				Label label2 = generator.DefineLabel();
				list.Insert(num++, new CodeInstruction(OpCodes.Brtrue_S, (object)label2));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsOpponentRankInGameShowValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
				list.Insert(num++, new CodeInstruction(OpCodes.Br_S, (object)label));
				list.Insert(num, new CodeInstruction(OpCodes.Ldc_I4_1, (object)null));
				list[num].labels.Add(label2);
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MulliganManager), "HandleGameStart")]
		public static IEnumerable<CodeInstruction> PatchHandleGameStart(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			int num = list.FindLastIndex((CodeInstruction x) => x.opcode == OpCodes.Callvirt && (x.operand as MethodInfo).Name == "ShouldSkipMulligan");
			if (num > 0)
			{
				num++;
				object operand = list[num].operand;
				list.Insert(num++, new CodeInstruction(OpCodes.Brtrue_S, operand));
				list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)new Func<ConfigValue>(ConfigValue.Get).Method));
				list.Insert(num++, new CodeInstruction(OpCodes.Callvirt, (object)typeof(ConfigValue).GetProperty("IsSkipHeroIntroValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()));
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SoundManager), "UpdateAllMutes")]
		public static bool PatchUpdateAllMutes(ref bool ___m_mute)
		{
			___m_mute = SoundManagerPatch.MuteKeyPressed | ___m_mute;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameEntity), "ShowEndGameScreen")]
		public static void PatchEndGameScreenShow(ref TAG_PLAYSTATE playState, ref Spell enemyBlowUpSpell, ref Spell friendlyBlowUpSpell)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected I4, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Invalid comparison between Unknown and I4
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (!GameMgr.Get().IsSpectator() && (BnetEntityId)(object)Utils.CacheLastOpponentAccountID != (BnetEntityId)null && !string.IsNullOrEmpty(Utils.CacheLastOpponentFullName))
				{
					PluginConfig.LoadSkinsConfigFromFile();
					Utils.CacheRawHeroCardId = null;
					if (PluginConfig.isAutoReportEnable.Value)
					{
						Utils.TryReportOpponent();
						string text = "未知";
						TAG_PLAYSTATE val = playState;
						switch (val - 2)
						{
						case 0:
						case 2:
							text = "胜利";
							break;
						case 1:
						case 3:
						case 6:
							text = "失败";
							break;
						case 4:
							text = "平局";
							break;
						}
						string text2 = (((int)GameMgr.Get().GetGameType() == 7) ? ((object)GameMgr.Get().GetFormatType()/*cast due to .constrained prefix*/).ToString() : ((object)GameMgr.Get().GetGameType()/*cast due to .constrained prefix*/).ToString());
						string text3 = "-";
						text = string.Join("<br />", DateTime.Now.ToString().Split(' ', StringSplitOptions.None)) + "," + text + "," + text3 + "," + text2 + "," + Utils.CacheLastOpponentFullName + ",";
						text += $"High:{((BnetEntityId)Utils.CacheLastOpponentAccountID).High}+Low:{((BnetEntityId)Utils.CacheLastOpponentAccountID).Low} => 已举报";
						File.AppendAllText(PluginConfig.CommandConfig.hsMatchLogPath, text + "\n");
						Utils.CacheLastOpponentAccountID = null;
					}
				}
			}
			catch (Exception message)
			{
				Utils.MyLogger((LogLevel)2, message);
				Utils.CacheLastOpponentAccountID = null;
			}
			if (PluginConfig.autoQuitTimer.Value > 0 && ConfigValue.Get().RunningTime >= PluginConfig.autoQuitTimer.Value)
			{
				Utils.MyLogger((LogLevel)4, "定时重启！即将退出游戏...");
				Application.Quit();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Player), "IsRevealed")]
		public static bool PatchPlayerIsRevealed(ref bool __result)
		{
			if (PluginConfig.isCardRevealedEnable.Value)
			{
				__result = true;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameState), "OnPowerHistory")]
		public static void PatchDebugPrintPower(GameState __instance, ref List<PowerHistory> powerList)
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Invalid comparison between Unknown and I4
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Invalid comparison between Unknown and I4
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Invalid comparison between Unknown and I4
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Invalid comparison between Unknown and I4
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Invalid comparison between Unknown and I4
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Invalid comparison between Unknown and I4
			try
			{
				if (!PluginConfig.isCardTrackerEnable.Value || GameMgr.Get().IsBattlegrounds() || GameMgr.Get().IsMercenaries() || powerList == null)
				{
					return;
				}
				List<string> list = new List<string>();
				int num = 0;
				foreach (PowerHistory power in powerList)
				{
					num++;
					if (power == null)
					{
						continue;
					}
					PowerHistory val = power;
					if ((int)val.Type == 2)
					{
						Entity entity = ((HistShowEntity)val).Entity;
						Entity val2 = ((__instance != null) ? __instance.GetEntity(entity.ID) : null);
						if (val2 != null && (int)val2.GetControllerSide() == 2 && (int)((EntityBase)val2).GetZone() == 6 && (int)((EntityBase)val2).GetCardType() != 6)
						{
							DefLoader obj = DefLoader.Get();
							EntityDef val3 = ((obj != null) ? obj.GetEntityDef(entity.CardID) : null);
							if (val3 != null && (int)((EntityBase)val3).GetCardType() != 6 && !((EntityBase)val3).IsQuestline())
							{
								string name = val3.GetName();
								if (name != null)
								{
									name = name + "\n" + val3.GetCardTextInHand();
									UIStatus.Get().AddInfo("注意: " + name, 15f);
								}
							}
						}
					}
					if ((int)val.Type != 1)
					{
						continue;
					}
					Entity entity2 = ((HistFullEntity)val).Entity;
					int num2 = 0;
					foreach (Tag tag in entity2.Tags)
					{
						num2++;
						if (tag.Name == 49 && tag.Value == 4)
						{
							EntityDef entityDef = DefLoader.Get().GetEntityDef(entity2.CardID);
							list.Add((entityDef != null) ? entityDef.GetName() : null);
						}
					}
				}
				string text = string.Join(" ", list);
				if (text != "")
				{
					UIStatus.Get().AddInfo("注意: " + text, 15f);
				}
			}
			catch (Exception message)
			{
				Utils.MyLogger((LogLevel)2, message);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MulliganManager), "SetMulliganBannerText", new Type[]
		{
			typeof(string),
			typeof(string)
		})]
		public static void PatchSetMulliganBannerText(MulliganManager __instance, ref string title, ref string subtitle, ref bool ___friendlyPlayerGoesFirst)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Invalid comparison between Unknown and I4
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Invalid comparison between Unknown and I4
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Invalid comparison between Unknown and I4
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			if (!PluginConfig.isCardTrackerEnable.Value || GameMgr.Get().IsBattlegrounds())
			{
				return;
			}
			int cardCount = ((Zone)((Entity)GameState.Get().GetOpposingPlayer()).GetController().GetDeckZone()).GetCardCount();
			int cardCount2 = ((Zone)((Entity)GameState.Get().GetOpposingPlayer()).GetController().GetHandZone()).GetCardCount();
			int num = cardCount + cardCount2;
			num = (___friendlyPlayerGoesFirst ? (num - 1) : num);
			int num2 = 0;
			int num3 = 0;
			Enumerator<int, Entity> enumerator = GameState.Get().GetEntityMap().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Entity value = enumerator.Current.Value;
					if (value != null && (int)value.GetControllerSide() == 2)
					{
						if ((int)((EntityBase)value).GetZone() == 6)
						{
							num2++;
						}
						if ((int)((EntityBase)value).GetZone() == 1)
						{
							num3++;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
			string arg = ((num2 == 1 && num3 == 4) ? GameStrings.GetClassName((TAG_CLASS)7) : GameStrings.GetClassName(((EntityBase)((Entity)GameState.Get().GetOpposingPlayer()).GetHero()).GetClass()));
			title = $"对手职业是{arg}，套牌{num}张";
		}
	}

	public class PatchLogArchive
	{
		[HarmonyTargetMethod]
		private static MethodInfo PublicLogArchiveLogPath()
		{
			return AccessTools.TypeByName("Log").GetMethod("get_LogsPath");
		}

		[HarmonyPostfix]
		public static void PatchLogPathGetter(string __result)
		{
			if (PluginConfig.hsLogPath.Value != __result)
			{
				PluginConfig.hsLogPath.Value = __result;
			}
		}
	}

	public class PatchFavorite
	{
		private static readonly MethodInfo getValidCardBackID = typeof(CardBackManager).GetMethod("GetValidCardBackID", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly MethodInfo loadCardBackPrefabIntoSlot = typeof(CardBackManager).GetMethod("LoadCardBackPrefabIntoSlot", BindingFlags.Instance | BindingFlags.NonPublic);

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Entity), "LoadCard")]
		public static void PatchLoadCard(Entity __instance, ref string cardId, ref LoadCardData data)
		{
			//IL_052c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0532: Invalid comparison between Unknown and I4
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Invalid comparison between Unknown and I4
			//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a7: Invalid comparison between Unknown and I4
			//IL_0647: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Invalid comparison between Unknown and I4
			//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Invalid comparison between Unknown and I4
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cb: Invalid comparison between Unknown and I4
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Invalid comparison between Unknown and I4
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Invalid comparison between Unknown and I4
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0585: Unknown result type (might be due to invalid IL or missing references)
			//IL_0588: Invalid comparison between Unknown and I4
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_0467: Invalid comparison between Unknown and I4
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Invalid comparison between Unknown and I4
			//IL_061f: Unknown result type (might be due to invalid IL or missing references)
			//IL_058a: Unknown result type (might be due to invalid IL or missing references)
			//IL_058d: Invalid comparison between Unknown and I4
			//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fa: Invalid comparison between Unknown and I4
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_049f: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_0429: Invalid comparison between Unknown and I4
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Invalid comparison between Unknown and I4
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			string text = cardId;
			if (cardId != null && Utils.CheckInfo.IsMercenarySkin(cardId, out var skin))
			{
				if (PluginConfig.goldenCardState.Value == Utils.CardState.Disabled && PluginConfig.maxCardState.Value == Utils.CardState.Disabled)
				{
					cardId = GameUtils.TranslateDbIdToCardId(skin.Default, false);
				}
				else if ((PluginConfig.maxCardState.Value == Utils.CardState.Disabled || PluginConfig.mercenaryDiamondCardState.Value == Utils.CardState.Disabled) && GameUtils.TranslateCardIdToDbId(cardId, false) == skin.Diamond)
				{
					cardId = GameUtils.TranslateDbIdToCardId(skin.Default, false);
				}
				else if (!PluginConfig.isOpponentGoldenCardShow.Value && (int)__instance.GetCard().GetControllerSide() == 2)
				{
					cardId = GameUtils.TranslateDbIdToCardId(skin.Default, false);
				}
				else if ((PluginConfig.maxCardState.Value == Utils.CardState.All || PluginConfig.mercenaryDiamondCardState.Value == Utils.CardState.All) && skin.hasDiamond)
				{
					cardId = GameUtils.TranslateDbIdToCardId(skin.Diamond, false);
				}
				else if ((PluginConfig.maxCardState.Value == Utils.CardState.OnlyMy || PluginConfig.mercenaryDiamondCardState.Value == Utils.CardState.OnlyMy) && skin.hasDiamond && (int)__instance.GetCard().GetControllerSide() == 1)
				{
					cardId = GameUtils.TranslateDbIdToCardId(skin.Diamond, false);
				}
				else if (PluginConfig.randomMercenarySkinEnable.Value == Utils.CardState.OnlyMy || PluginConfig.randomMercenarySkinEnable.Value == Utils.CardState.All)
				{
					List<int> list = new List<int>();
					list.AddRange(skin.Id);
					list.Remove(skin.Diamond);
					int num = list[Random.Range(0, list.Count)];
					if (PluginConfig.randomMercenarySkinEnable.Value == Utils.CardState.OnlyMy)
					{
						__instance.GetCard().GetControllerSide();
						_ = 1;
						cardId = GameUtils.TranslateDbIdToCardId(num, false);
					}
					else
					{
						cardId = GameUtils.TranslateDbIdToCardId(num, false);
					}
				}
			}
			else
			{
				if (cardId != null)
				{
					DefLoader obj = DefLoader.Get();
					if (obj != null)
					{
						EntityDef entityDef = obj.GetEntityDef(cardId);
						if (((entityDef != null) ? new TAG_CARDTYPE?(((EntityBase)entityDef).GetCardType()) : ((TAG_CARDTYPE?)null)) == (TAG_CARDTYPE?)10)
						{
							if (PluginConfig.isSkinDefalutHeroEnable.Value && !GameMgr.Get().IsBattlegrounds())
							{
								try
								{
									TAG_CLASS tagClass = ((EntityBase)DefLoader.Get().GetEntityDef(cardId)).GetClass();
									if (GameUtils.ORDERED_HERO_CLASSES.Contains(tagClass))
									{
										cardId = GameUtils.GetHeroPowerCardIdFromHero((from x in (from x in GameDbf.CardHero.GetRecords()
												orderby x.CardId
												select x).ToList()
											where ((EntityBase)DefLoader.Get().GetEntityDef(x.CardId, true)).GetClass() == tagClass
											select x).FirstOrDefault().CardId);
										goto IL_060f;
									}
								}
								catch (Exception message)
								{
									Utils.MyLogger((LogLevel)2, message);
								}
							}
							if (PluginConfig.skinHero.Value != -1 && (int)__instance.GetCard().GetControllerSide() == 1)
							{
								cardId = GameUtils.GetHeroPowerCardIdFromHero(PluginConfig.skinHero.Value);
							}
							else if (PluginConfig.skinOpposingHero.Value != -1 && (int)__instance.GetCard().GetControllerSide() == 2)
							{
								cardId = GameUtils.GetHeroPowerCardIdFromHero(PluginConfig.skinOpposingHero.Value);
							}
							else if ((int)__instance.GetCard().GetControllerSide() == 1 && PluginConfig.HeroesMapping.Count != 0)
							{
								Utils.UpdateHeroPowerMapping();
								PluginConfig.HeroesPowerMapping.TryGetValue(cardId, out var value);
								cardId = ((value != null && value != "" && value != string.Empty) ? value : cardId);
							}
							goto IL_060f;
						}
					}
				}
				if (Utils.CheckInfo.IsHero(cardId, out var heroType))
				{
					if (PluginConfig.skinBob.Value != -1 && (int)heroType == 4)
					{
						if (Utils.CheckInfo.IsHero(PluginConfig.skinBob.Value, out var _))
						{
							cardId = GameUtils.TranslateDbIdToCardId(PluginConfig.skinBob.Value, false);
						}
					}
					else if ((int)heroType == 3 && (int)__instance.GetCard().GetControllerSide() == 1)
					{
						int value2;
						HeroType heroType3;
						if (PluginConfig.skinHero.Value != -1)
						{
							cardId = GameUtils.TranslateDbIdToCardId(PluginConfig.skinHero.Value, false);
						}
						else if (PluginConfig.HeroesMapping.TryGetValue(GameUtils.TranslateCardIdToDbId(cardId, false), out value2) && Utils.CheckInfo.IsHero(value2, out heroType3) && (int)heroType3 == 3)
						{
							cardId = GameUtils.TranslateDbIdToCardId(value2, false);
						}
					}
					else if (cardId.Substring(0, 5) == "HERO_" && (int)((EntityBase)DefLoader.Get().GetEntityDef(cardId)).GetCardType() == 3)
					{
						if (PluginConfig.isSkinDefalutHeroEnable.Value && !GameMgr.Get().IsBattlegrounds())
						{
							try
							{
								TAG_CLASS tagClass2 = ((EntityBase)DefLoader.Get().GetEntityDef(cardId)).GetClass();
								if (GameUtils.ORDERED_HERO_CLASSES.Contains(tagClass2))
								{
									cardId = GameUtils.TranslateDbIdToCardId((from x in (from x in GameDbf.CardHero.GetRecords()
											orderby x.CardId
											select x).ToList()
										where ((EntityBase)DefLoader.Get().GetEntityDef(x.CardId, true)).GetClass() == tagClass2
										select x).FirstOrDefault().CardId, false);
									goto IL_060f;
								}
							}
							catch (Exception message2)
							{
								Utils.MyLogger((LogLevel)2, message2);
							}
						}
						if ((int)__instance.GetCard().GetControllerSide() == 1)
						{
							Utils.CacheRawHeroCardId = text;
							int value3;
							HeroType heroType4;
							if (PluginConfig.skinHero.Value != -1)
							{
								cardId = GameUtils.TranslateDbIdToCardId(PluginConfig.skinHero.Value, false);
							}
							else if (PluginConfig.HeroesMapping.TryGetValue(GameUtils.TranslateCardIdToDbId(cardId, false), out value3) && Utils.CheckInfo.IsHero(value3, out heroType4) && ((int)heroType4 != 3 || (int)heroType4 != 4))
							{
								cardId = GameUtils.TranslateDbIdToCardId(value3, false);
							}
						}
						else if ((int)__instance.GetCard().GetControllerSide() == 2 && PluginConfig.skinOpposingHero.Value != -1)
						{
							cardId = GameUtils.TranslateDbIdToCardId(PluginConfig.skinOpposingHero.Value, false);
						}
					}
				}
				else if (PluginConfig.skinCoin.Value != -1 && cardId != null && cardId.Length > 4 && Utils.CheckInfo.IsCoin(cardId) && (int)__instance.GetCard().GetControllerSide() == 1)
				{
					cardId = GameUtils.TranslateDbIdToCardId(PluginConfig.skinCoin.Value, false);
				}
			}
			goto IL_060f;
			IL_060f:
			try
			{
				if (__instance != null)
				{
					((EntityBase)__instance).SetCardId(cardId);
				}
				if (__instance != null)
				{
					__instance.SetRealTimePremium(__instance.GetPremiumType());
				}
			}
			catch (Exception message3)
			{
				Utils.MyLogger((LogLevel)2, message3);
				cardId = text;
				if (__instance != null)
				{
					((EntityBase)__instance).SetCardId(text);
				}
				if (__instance != null)
				{
					__instance.SetRealTimePremium(__instance.GetPremiumType());
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Card), "RefreshActor")]
		public static void RefreshActor(Card __instance)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (__instance != null)
				{
					Entity entity = __instance.GetEntity();
					if (((entity != null) ? new TAG_ZONE?(((EntityBase)entity).GetZone()) : ((TAG_ZONE?)null)) == (TAG_ZONE?)1)
					{
						goto IL_0075;
					}
				}
				if (__instance != null)
				{
					Entity entity2 = __instance.GetEntity();
					if (((entity2 != null) ? new TAG_ZONE?(((EntityBase)entity2).GetZone()) : ((TAG_ZONE?)null)) != (TAG_ZONE?)3)
					{
						return;
					}
					goto IL_0075;
				}
				return;
				IL_0075:
				if (__instance != null)
				{
					Actor actor = __instance.GetActor();
					if (actor != null)
					{
						actor.SetCard(__instance);
					}
				}
				if (__instance != null)
				{
					Actor actor2 = __instance.GetActor();
					if (actor2 != null)
					{
						actor2.SetCardDefFromEntity(__instance.GetEntity());
					}
				}
				if (__instance != null)
				{
					Actor actor3 = __instance.GetActor();
					if (actor3 != null)
					{
						actor3.SetEntity(__instance.GetEntity());
					}
				}
				if (__instance != null)
				{
					Actor actor4 = __instance.GetActor();
					if (actor4 != null)
					{
						actor4.UpdateAllComponents(true);
					}
				}
			}
			catch (Exception message)
			{
				Utils.MyLogger((LogLevel)2, message);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ActorNames), "GetNameWithPremiumType")]
		public static void PatchGetNameWithPremiumType(ActorNames __instance, ref string __result, ref ACTOR_ASSET actorName, ref TAG_PREMIUM premiumType, ref string cardId)
		{
			if (__result == null)
			{
				string value = null;
				Utils.MyLogger((LogLevel)4, $"Function return null\nGetNameWithPremiumType(ActorNames.ACTOR_ASSET {actorName}, TAG_PREMIUM {premiumType}, string {cardId});");
				if (string.IsNullOrEmpty(__result))
				{
					ActorNames.s_premiumActorAssets.TryGetValue(actorName, out value);
					string.IsNullOrEmpty(value);
				}
				__result = value;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(TB_BaconShop), "GetFavoriteBattlegroundsGuideSkinCardId")]
		public static bool PatchGetFavoriteBattlegroundsGuideSkinCardId(ref string __result)
		{
			if (PluginConfig.skinBob.Value == -1)
			{
				return true;
			}
			if (Utils.CheckInfo.IsHero(PluginConfig.skinBob.Value, out var _))
			{
				__result = GameUtils.TranslateDbIdToCardId(PluginConfig.skinBob.Value, false);
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameMgr), "ChangeBoardIfNecessary")]
		public static bool PatchChangeBoardIfNecessary(ref GameSetup ___m_gameSetup)
		{
			if (PluginConfig.skinBoard.Value != -1 && Utils.CheckInfo.IsBoard())
			{
				___m_gameSetup.Board = PluginConfig.skinBoard.Value;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Gameplay), "InitCardBacks")]
		public static bool PatchGameplayInitCardBacks()
		{
			if (PluginConfig.skinCardBack.Value != -1 && Utils.CheckInfo.IsCardBack())
			{
				GameState obj = GameState.Get();
				Player val = ((obj != null) ? obj.GetFriendlySidePlayer() : null);
				if (val != null)
				{
					val.GetCardBackId();
				}
				int num = 0;
				GameState obj2 = GameState.Get();
				Player val2 = ((obj2 != null) ? obj2.GetOpposingSidePlayer() : null);
				if (val2 != null)
				{
					num = val2.GetCardBackId();
				}
				int value = PluginConfig.skinCardBack.Value;
				if (GameMgr.Get().IsBattlegrounds())
				{
					num = value;
				}
				CardBackManager.Get().SetGameCardBackIDs(value, num);
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CardBackManager), "LoadCardBackIdIntoSlot")]
		public static bool PatchGameplayInitCardBacks(ref int cardBackId, ref CardBackSlot slot, ref Map<int, CardBackData> ___m_cardBackData, CardBackManager __instance)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			if (PluginConfig.skinCardBack.Value != -1 && Utils.CheckInfo.IsCardBack() && (int)SceneMgr.Get().GetMode() == 6 && (int)slot == 3)
			{
				int num = (int)getValidCardBackID.Invoke(__instance, new object[1] { PluginConfig.skinCardBack.Value });
				CardBackData val = default(CardBackData);
				if (___m_cardBackData.TryGetValue(num, ref val))
				{
					loadCardBackPrefabIntoSlot?.Invoke(__instance, new object[2]
					{
						AssetReference.op_Implicit(val.PrefabName),
						slot
					});
				}
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(BaconBoard), "OnBoardSkinChosen")]
		[HarmonyPatch(typeof(BaconBoard), "LoadInitialTavernBoard")]
		public static void PatchOnBoardSkinChosen(ref int chosenBoardSkinId)
		{
			if (PluginConfig.skinBgsBoard.Value != 0 && Utils.CheckInfo.IsBgsBoard())
			{
				chosenBoardSkinId = PluginConfig.skinBgsBoard.Value;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FinisherGameplaySettings), "GetFinisherGameplaySettings")]
		public static bool PatchGetFinisherGameplaySettings(ref Entity hero, ref FinisherGameplaySettings __result)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if (PluginConfig.skinBgsFinisher.Value != -1 && Utils.CheckInfo.IsBgsFinisher() && (int)hero.GetControllerSide() == 1)
			{
				int num = PluginConfig.skinBgsFinisher.Value;
				if (num <= 0)
				{
					Log.Spells.PrintError(hero.GetDebugName() + " has no tag BATTLEGROUNDS_FAVORITE_FINISHER. Using Default Finisher.", Array.Empty<object>());
					num = 1;
				}
				BattlegroundsFinisherDbfRecord record = GameDbf.BattlegroundsFinisher.GetRecord(num);
				if (record == null)
				{
					Log.Spells.PrintError($"No Finisher was found for Finisher ID {num}. Using default finisher.", Array.Empty<object>());
					record = GameDbf.BattlegroundsFinisher.GetRecord(1);
				}
				AssetReference val = AssetReference.CreateFromAssetString(record.GameplaySettings);
				AssetHandle<FinisherGameplaySettings> val2 = ((val != null) ? AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(val, (AssetLoadingOptions)0) : null);
				FinisherGameplaySettings val3 = (AssetHandle<FinisherGameplaySettings>.op_Implicit(val2) ? val2.Asset : null);
				if ((Object)(object)val3 == (Object)null)
				{
					Log.Spells.PrintError($"Finisher ID {num} is missing its finisher settings entirely in HE2. Using default finisher.", Array.Empty<object>());
					record = GameDbf.BattlegroundsFinisher.GetRecord(1);
					val = AssetReference.CreateFromAssetString(record.GameplaySettings);
					val2 = AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(val, (AssetLoadingOptions)0);
					val3 = val2.Asset;
				}
				__result = val3;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CoinManager), "GetFavoriteCoinId")]
		public static bool PatchGetFavoriteCoinId(ref int __result)
		{
			if (PluginConfig.skinCoin.Value == 0)
			{
				return true;
			}
			if (Utils.CheckInfo.IsCoin())
			{
				int num = 0;
				foreach (CoinDbfRecord record in GameDbf.Coin.GetRecords())
				{
					if (record != null && record.CardId == PluginConfig.skinCoin.Value)
					{
						num = ((DbfRecord)record).ID;
						break;
					}
				}
				__result = num;
				return false;
			}
			return true;
		}
	}

	public class PatchIGMMessage
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(CardListPopup), "Show")]
		public static void PatchCardListPopupShow(ref UIBButton ___m_okayButton)
		{
			if (!PluginConfig.isIGMMessageShow.Value)
			{
				((PegUIElement)___m_okayButton).TriggerPress();
				((PegUIElement)___m_okayButton).TriggerRelease();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ViewCountController), "GetViewCount")]
		public static bool PatchGetViewCount(ref int __result, string uid)
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			__result = 0;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MessagePopupDisplay), "GetMessageCount")]
		public static bool PatchGetMessageCount(ref int __result, PopupEvent eventID)
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			__result = 0;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ViewCountController), "Serialize")]
		[HarmonyPatch(typeof(ViewCountController), "Deserialize")]
		public static bool PatchViewCountController()
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MessagePopupDisplay), "DisplayIGMMessage")]
		public static void PatchDisplayIGMMessage(MessagePopupDisplay __instance)
		{
			if (!PluginConfig.isIGMMessageShow.Value)
			{
				Traverse.Create((object)__instance).Method("OnMessageClosed", Array.Empty<object>()).GetValue();
				Traverse.Create((object)__instance).Method("OnMessageClosed", Array.Empty<object>()).GetValue();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SeasonEndDialog), "ShowWhenReady")]
		public static bool PatchSeasonEndDialog(SeasonEndDialog __instance)
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			Traverse.Create((object)__instance).Method("Finish", Array.Empty<object>()).GetValue();
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LoanerDeckDisplay), "CanShowTimerExpiredState")]
		public static bool PatchCanShowTimerExpiredState(ref bool __result)
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			__result = false;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LoanerDeckDisplay), "ShouldLoanerDecksBeDisplayed")]
		public static bool PatchShouldLoanerDecksBeDisplayed(ref bool __result)
		{
			if (PluginConfig.isIGMMessageShow.Value)
			{
				return true;
			}
			__result = false;
			return false;
		}
	}

	public class PatchMercenariesReward
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(RewardPopups), "ShowMercenariesFullyUpgraded")]
		public static bool PatchShowMercenariesFullyUpgraded(ref Action doneCallback, ref bool __result, ref RewardPopups __instance, ref Action ___OnPopupClosed)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				List<ProfileNotice> notices = NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices;
				for (int i = 0; i < notices.Count; i++)
				{
					ProfileNotice val = notices[i];
					if (val != null)
					{
						Utils.MyLogger((LogLevel)4, $"NetCacheProfileNotices {val.Origin} {val.Type} {val.NoticeID}");
						Network.Get().AckNotice(val.NoticeID);
					}
				}
				NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices.Clear();
				Traverse obj = Traverse.Create((object)__instance).Field("GetNextMercenaryFullUpgradedToShow");
				ProfileNoticeMercenariesMercenaryFullyUpgraded val2 = (ProfileNoticeMercenariesMercenaryFullyUpgraded)((obj != null) ? obj.GetValue() : null);
				if (val2 != null)
				{
					Network.Get().AckNotice(((ProfileNotice)val2).NoticeID);
					Utils.MyLogger((LogLevel)4, $"ShowMercenariesFullyUpgraded {((ProfileNotice)val2).NoticeID}");
					doneCallback?.Invoke();
					___OnPopupClosed?.Invoke();
				}
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RewardPopups), "GetNextMercenaryFullUpgradedToShow")]
		public static void PatchGetNextMercenaryFullUpgradedToShow(ref ProfileNoticeMercenariesMercenaryFullyUpgraded __result)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			if (!PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				return;
			}
			if (__result != null)
			{
				Network.Get().AckNotice(((ProfileNotice)__result).NoticeID);
				Utils.MyLogger((LogLevel)4, $"GetNextMercenaryFullUpgradedToShow {((ProfileNotice)__result).NoticeID}");
			}
			__result = null;
			List<ProfileNotice> notices = NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices;
			for (int i = 0; i < notices.Count; i++)
			{
				ProfileNotice val = notices[i];
				if (val != null)
				{
					Utils.MyLogger((LogLevel)4, $"NetCacheProfileNotices {val.Origin} {val.Type} {val.NoticeID}");
					Network.Get().AckNotice(val.NoticeID);
				}
			}
			NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices.Clear();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RewardPopups), "ShowMercenariesZoneUnlockPopup")]
		public static bool PatchShowMercenariesZoneUnlockPopup(ref Action onPopupCompleteCallback, ref bool __result, ref RewardPopups __instance, ref Action ___OnPopupClosed)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				List<ProfileNotice> notices = NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices;
				for (int i = 0; i < notices.Count; i++)
				{
					ProfileNotice val = notices[i];
					if (val != null)
					{
						Utils.MyLogger((LogLevel)4, $"NetCacheProfileNotices {val.Origin} {val.Type} {val.NoticeID}");
						Network.Get().AckNotice(val.NoticeID);
					}
				}
				NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices.Clear();
				Traverse obj = Traverse.Create((object)__instance).Field("GetNextMercenariesZoneUnlockToShow");
				ProfileNoticeMercenariesZoneUnlock val2 = (ProfileNoticeMercenariesZoneUnlock)((obj != null) ? obj.GetValue() : null);
				if (val2 != null)
				{
					Network.Get().AckNotice(((ProfileNotice)val2).NoticeID);
					Utils.MyLogger((LogLevel)4, $"ShowMercenariesZoneUnlockPopup {((ProfileNotice)val2).NoticeID}");
					onPopupCompleteCallback?.Invoke();
					___OnPopupClosed?.Invoke();
				}
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RewardPopups), "GetNextMercenariesZoneUnlockToShow")]
		public static void PatchGetNextMercenariesZoneUnlockToShow(ref ProfileNoticeMercenariesZoneUnlock __result)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			if (!PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				return;
			}
			if (__result != null)
			{
				Network.Get().AckNotice(((ProfileNotice)__result).NoticeID);
				Utils.MyLogger((LogLevel)4, $"GetNextMercenariesZoneUnlockToShow {((ProfileNotice)__result).NoticeID}");
			}
			__result = null;
			List<ProfileNotice> notices = NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices;
			for (int i = 0; i < notices.Count; i++)
			{
				ProfileNotice val = notices[i];
				if (val != null)
				{
					Utils.MyLogger((LogLevel)4, $"NetCacheProfileNotices {val.Origin} {val.Type} {val.NoticeID}");
					Network.Get().AckNotice(val.NoticeID);
				}
			}
			NetCache.Get().GetNetObject<NetCacheProfileNotices>().Notices.Clear();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MercenariesSeasonRewardsDialog), "Show")]
		[HarmonyPatch(typeof(MercenariesSeasonRewardsDialog), "ShowWhenReady")]
		public static bool PatchPatchSeasonEndDialog(MercenariesSeasonRewardsDialog __instance)
		{
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				Traverse.Create((object)__instance).Field("AckAndHide").GetValue();
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DialogManager), "ProcessMercenariesSeasonRewardsDialog")]
		public static bool PatchProcessMercenariesSeasonRewardsDialog(ref DialogRequest request, ref MercenariesSeasonRewardsDialog dialog)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				Info val = (Info)request.m_info;
				Network.Get().AckNotice(val.m_noticeId);
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RewardPopups), "ShowMercenariesRewards")]
		public static bool PatchShowMercenariesRewards(ref bool autoOpenChest, ref ProfileNoticeMercenariesRewards rewardNotice, ref ProfileNoticeMercenariesRewards bonusRewardNotice, ref Action doneCallback, ref RewardPopups __instance, ref bool __result)
		{
			if (PluginConfig.isAutoRecvMercenaryRewardEnable.Value)
			{
				autoOpenChest = true;
				if (rewardNotice != null)
				{
					Network.Get().AckNotice(((ProfileNotice)rewardNotice).NoticeID);
					if (bonusRewardNotice != null)
					{
						Network.Get().AckNotice(((ProfileNotice)bonusRewardNotice).NoticeID);
					}
					doneCallback?.Invoke();
					Traverse.Create((object)__instance).Field("OnPopupClosed").GetValue();
					__result = false;
					return false;
				}
			}
			return true;
		}
	}

	public class PatchMercenaries
	{
		public static readonly float m_FieldOfViewDefault = BoardCameras.Get().m_FieldOfViewDefault;

		public static readonly float m_FieldOfViewZoomed = BoardCameras.Get().m_FieldOfViewZoomed;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LettuceMissionEntity), "SetFullScreenFXForCombat")]
		public static bool PatchSetFullScreenFXForCombat()
		{
			if (PluginConfig.isMercenaryBattleZoom.Value)
			{
				return true;
			}
			BoardCameras.Get().m_FieldOfViewDefault = m_FieldOfViewDefault;
			BoardCameras.Get().m_FieldOfViewZoomed = m_FieldOfViewDefault;
			return false;
		}
	}

	public class PatchFakePackOpening
	{
		private static readonly MethodInfo onDirectorFinished = typeof(PackOpening).GetMethod("OnDirectorFinished", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly MethodInfo onBoosterOpened = typeof(PackOpening).GetMethod("OnBoosterOpened", BindingFlags.Instance | BindingFlags.NonPublic);

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameUtils), "IsFakePackOpeningEnabled")]
		public static bool PatchIsFakePackOpeningEnabled(ref bool __result)
		{
			if (!PluginConfig.isFakeOpenEnable.Value)
			{
				return true;
			}
			__result = true;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameUtils), "GetFakePackCount")]
		public static bool PatchGetFakePackCount(ref int __result)
		{
			if (!PluginConfig.isFakeOpenEnable.Value)
			{
				return true;
			}
			__result = ((PluginConfig.fakePackCount.Value >= 0) ? PluginConfig.fakePackCount.Value : 0);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NetCache), "GetTestData")]
		public static bool PatchGetTestData(ref Type type, ref object __result)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected I4, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			if (!PluginConfig.isFakeOpenEnable.Value)
			{
				return true;
			}
			if (type == typeof(NetCacheBoosters) && GameUtils.IsFakePackOpeningEnabled())
			{
				NetCacheBoosters val = new NetCacheBoosters();
				int fakePackCount = GameUtils.GetFakePackCount();
				BoosterStack item = new BoosterStack
				{
					Id = (int)PluginConfig.fakeBoosterDbId.Value,
					Count = fakePackCount
				};
				val.BoosterStacks.Add(item);
				__result = val;
				return false;
			}
			__result = null;
			return false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyReversePatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPatch(typeof(MonoBehaviour), "StopCoroutine", new Type[] { typeof(Coroutine) })]
		public static void PackOpeningBaseStopCoroutine(PackOpening instance, Coroutine routine)
		{
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PackOpening), "OpenBooster")]
		public static bool PatchOpenBooster(ref UnopenedPack pack, ref PackOpening __instance, ref float ___m_packOpeningStartTime, ref int ___m_packOpeningId, ref GameObject ___m_InputBlocker, ref Coroutine ___m_autoOpenPackCoroutine, ref PackOpeningDirector ___m_director, ref int ___m_lastOpenedBoosterId, ref UIBScrollable ___m_UnopenedPackScroller)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected I4, but got Unknown
			if (!PluginConfig.isFakeOpenEnable.Value)
			{
				return true;
			}
			AchievementManager.Get().PauseToastNotifications();
			int num = (int)PluginConfig.fakeBoosterDbId.Value;
			if (!GameUtils.IsFakePackOpeningEnabled())
			{
				num = pack.GetBoosterId();
				___m_packOpeningStartTime = Time.realtimeSinceStartup;
				___m_packOpeningId = num;
				BoosterPackUtils.OpenBooster(num);
			}
			___m_InputBlocker.SetActive(true);
			if (___m_autoOpenPackCoroutine != null)
			{
				PackOpeningBaseStopCoroutine(__instance, ___m_autoOpenPackCoroutine);
				___m_autoOpenPackCoroutine = null;
			}
			object firstArgument = __instance;
			EventHandler eventHandler = Delegate.CreateDelegate(typeof(EventHandler), firstArgument, onDirectorFinished) as EventHandler;
			___m_director.OnFinishedEvent += eventHandler;
			___m_lastOpenedBoosterId = num;
			BnetBar.Get().HideCurrencyFrames(false);
			if (GameUtils.IsFakePackOpeningEnabled())
			{
				onBoosterOpened?.Invoke(__instance, null);
			}
			___m_UnopenedPackScroller.Pause(true);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PackOpening), "OnBoosterOpened")]
		public static bool PatchOnBoosterOpened(ref float ___m_packOpeningStartTime, ref PackOpeningDirector ___m_director, ref int ___m_lastOpenedBoosterId, ref int ___m_packOpeningId, ref bool ___m_autoOpenPending)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Expected O, but got Unknown
			if (!PluginConfig.isFakeOpenEnable.Value)
			{
				return true;
			}
			float num = Time.realtimeSinceStartup - ___m_packOpeningStartTime;
			___m_director.Play(___m_lastOpenedBoosterId, num, ___m_packOpeningId);
			___m_autoOpenPending = false;
			if (PluginConfig.isFakeRandomResult.Value)
			{
				Utils.GenerateRandomCard(PluginConfig.isFakeRandomRarity.Value, PluginConfig.isFakeRandomPremium.Value, (TAG_RARITY)5, (TAG_PREMIUM)1);
			}
			List<BoosterCard> list = new List<BoosterCard>();
			BoosterCard val = new BoosterCard();
			val.Def.Name = GameUtils.TranslateDbIdToCardId(PluginConfig.fakeCardID1.Value, false);
			val.Def.Premium = PluginConfig.fakeCardPremium1.Value;
			list.Add(val);
			BoosterCard val2 = new BoosterCard();
			val2.Def.Name = GameUtils.TranslateDbIdToCardId(PluginConfig.fakeCardID2.Value, false);
			val2.Def.Premium = PluginConfig.fakeCardPremium2.Value;
			list.Add(val2);
			BoosterCard val3 = new BoosterCard();
			val3.Def.Name = GameUtils.TranslateDbIdToCardId(PluginConfig.fakeCardID3.Value, false);
			val3.Def.Premium = PluginConfig.fakeCardPremium3.Value;
			list.Add(val3);
			BoosterCard val4 = new BoosterCard();
			val4.Def.Name = GameUtils.TranslateDbIdToCardId(PluginConfig.fakeCardID4.Value, false);
			val4.Def.Premium = PluginConfig.fakeCardPremium4.Value;
			list.Add(val4);
			BoosterCard val5 = new BoosterCard();
			val5.Def.Name = GameUtils.TranslateDbIdToCardId(PluginConfig.fakeCardID5.Value, false);
			val5.Def.Premium = PluginConfig.fakeCardPremium5.Value;
			list.Add(val5);
			List<BoosterCard> list2 = list;
			___m_director.OnBoosterOpened(list2);
			return false;
		}
	}

	public class PatchFakeDevice
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Network), "GetPlatformBuilder")]
		public static void PatchGetPlatformBuilder(Network __instance, ref Platform __result)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected I4, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected I4, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			OSCategory val;
			ScreenCategory val2;
			string text;
			switch (PluginConfig.fakeDevicePreset.Value)
			{
			default:
				return;
			case Utils.DevicePreset.Default:
				return;
			case Utils.DevicePreset.iPad:
				val = (OSCategory)3;
				val2 = (ScreenCategory)3;
				text = "iPad13,11";
				break;
			case Utils.DevicePreset.iPhone:
				val = (OSCategory)3;
				val2 = (ScreenCategory)1;
				text = "iPhone13,4";
				break;
			case Utils.DevicePreset.Phone:
				val = (OSCategory)4;
				val2 = (ScreenCategory)1;
				text = "SAMSUNG-SM-G930FD";
				break;
			case Utils.DevicePreset.Tablet:
				val = (OSCategory)4;
				val2 = (ScreenCategory)3;
				text = "SAMSUNG-SM-G920F";
				break;
			case Utils.DevicePreset.HuaweiPhone:
				val = (OSCategory)4;
				val2 = (ScreenCategory)1;
				text = "Huawei Nova 8";
				break;
			case Utils.DevicePreset.Custom:
				val = PluginConfig.fakeDeviceOs.Value;
				val2 = PluginConfig.fakeDeviceScreen.Value;
				text = PluginConfig.fakeDeviceName.Value;
				break;
			}
			__result.Os = (int)val;
			__result.Screen = (int)val2;
			__result.Name = text;
			__result.UniqueDeviceIdentifier = GetUniqueDeviceID(val, val2, text);
		}

		private static string GetMD5(string message)
		{
			byte[] array = MD5.Create().ComputeHash(Encoding.Default.GetBytes(message));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		private static string GetUniqueDeviceID(OSCategory os, ScreenCategory screen, string deviceName)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I4, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			switch (os - 1)
			{
			case 0:
				return SHA1.Calc(Encoding.Default.GetBytes($"HsModeD{SystemInfo.deviceUniqueIdentifier}{os}{screen}{deviceName}"));
			case 1:
			case 2:
				return new Guid(GetMD5($"HsModeD{SystemInfo.deviceUniqueIdentifier}{os}{screen}{deviceName}")).ToString().ToUpper();
			case 3:
				return GetMD5($"HsModeD_{SystemInfo.deviceUniqueIdentifier}{os}{screen}{deviceName}");
			default:
				return "n/a";
			}
		}
	}

	public class PatchKarazhan
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(AdventureWing), "Initialize")]
		public static IEnumerable<CodeInstruction> PatchAdventureWingInitialize(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			list.RemoveRange(201, 9);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(AdventureWing), "Update")]
		public static IEnumerable PatchAdventureWingUpdate(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			for (int i = 0; i <= 112; i++)
			{
				list[i] = new CodeInstruction(OpCodes.Nop, (object)null);
			}
			for (int j = 125; j <= 134; j++)
			{
				list[j] = new CodeInstruction(OpCodes.Nop, (object)null);
			}
			return list;
		}
	}
}
public static class TimeScaleMgrPatch
{
	private static readonly MethodInfo updateInfo = typeof(TimeScaleMgr).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void Update(this TimeScaleMgr __instance)
	{
		updateInfo?.Invoke(__instance, null);
	}
}
public static class CollectionManagerPatch
{
	private static readonly MethodInfo onCollectionChanged = typeof(CollectionManager).GetMethod("OnCollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void OnCollectionChanged(this CollectionManager __instance)
	{
		onCollectionChanged?.Invoke(__instance, null);
	}
}
public static class SoundManagerPatch
{
	public static bool MuteKeyPressed = false;

	private static readonly MethodInfo updateAppMuteInfo = typeof(SoundManager).GetMethod("UpdateAppMute", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void OnMuteKeyPressed()
	{
		MuteKeyPressed = !MuteKeyPressed;
		SoundManager val = SoundManager.Get();
		if (val != null)
		{
			updateAppMuteInfo?.Invoke(val, null);
		}
	}
}
public static class SharedPlayerInfoPatch
{
	private static readonly FieldInfo m_gameAccountIdInfo = typeof(SharedPlayerInfo).GetField("m_gameAccountId", BindingFlags.Instance | BindingFlags.NonPublic);

	public static BnetGameAccountId GetGameAccountId(this SharedPlayerInfo __instance)
	{
		object obj = m_gameAccountIdInfo?.GetValue(__instance);
		return (BnetGameAccountId)((obj is BnetGameAccountId) ? obj : null);
	}
}
public static class PlayerLeaderboardManagerPatch
{
	private static readonly FieldInfo m_currentlyMousedOverTileInfo = typeof(PlayerLeaderboardManager).GetField("m_currentlyMousedOverTile", BindingFlags.Instance | BindingFlags.NonPublic);

	private static BnetPlayer m_currentOpponent;

	public static void UpdateCurrentOpponent(int opponentPlayerId)
	{
		if (GameState.Get() == null || !GameState.Get().GetPlayerInfoMap().ContainsKey(opponentPlayerId))
		{
			m_currentOpponent = null;
			return;
		}
		BnetGameAccountId gameAccountId = GameState.Get().GetPlayerInfoMap()[opponentPlayerId].GetGameAccountId();
		if ((BnetEntityId)(object)gameAccountId == (BnetEntityId)null)
		{
			m_currentOpponent = null;
		}
		else
		{
			m_currentOpponent = BnetPresenceMgr.Get().GetPlayer(gameAccountId);
		}
	}

	public static BnetPlayer GetCurrentOpponent(this PlayerLeaderboardManager __instance)
	{
		return m_currentOpponent;
	}

	public static BnetPlayer GetSelectedOpponent(this PlayerLeaderboardManager __instance)
	{
		object obj = m_currentlyMousedOverTileInfo?.GetValue(__instance);
		PlayerLeaderboardCard val = (PlayerLeaderboardCard)((obj is PlayerLeaderboardCard) ? obj : null);
		if (val == null || GameState.Get() == null)
		{
			return null;
		}
		int tag = ((EntityBase)val.m_playerHeroEntity).GetTag((GAME_TAG)30);
		if (!GameState.Get().GetPlayerInfoMap().ContainsKey(tag))
		{
			return null;
		}
		BnetGameAccountId gameAccountId = GameState.Get().GetPlayerInfoMap()[tag].GetGameAccountId();
		if ((BnetEntityId)(object)gameAccountId == (BnetEntityId)null)
		{
			return null;
		}
		return BnetPresenceMgr.Get().GetPlayer(gameAccountId);
	}
}
public static class EmoteHandlerPatch
{
	private static readonly FieldInfo m_totalEmotesInfo = typeof(EmoteHandler).GetField("m_totalEmotes", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo m_availableEmotesInfo = typeof(EmoteHandler).GetField("m_availableEmotes", BindingFlags.Instance | BindingFlags.NonPublic);

	private static List<EmoteOption> m_FoundedEmotes;

	public static void HandleKeyboardInput(this EmoteHandler __instance, int EmoteIndex, bool useExtended = false)
	{
		if (EmoteHandler.Get().EmoteSpamBlocked())
		{
			return;
		}
		List<EmoteOption> list = m_availableEmotesInfo.GetValue(__instance) as List<EmoteOption>;
		if (useExtended)
		{
			if (!PluginConfig.isExtendedBMEnable.Value || EmoteIndex + 1 > m_FoundedEmotes.Count)
			{
				return;
			}
			m_totalEmotesInfo.SetValue(__instance, (int)m_totalEmotesInfo.GetValue(__instance) + 1);
			if (!((EntityBase)GameState.Get().GetGameEntity()).HasTag((GAME_TAG)477))
			{
				m_FoundedEmotes[EmoteIndex].DoClick();
				return;
			}
			List<EmoteOption> list2 = new List<EmoteOption>();
			foreach (EmoteOption item in list.Concat(__instance.m_HiddenEmotes))
			{
				if (item.CanPlayerUseEmoteType(GameState.Get().GetFriendlySidePlayer()))
				{
					list2.Add(item);
				}
			}
			foreach (EmoteOption foundedEmote in m_FoundedEmotes)
			{
				if ((Object)(object)foundedEmote != (Object)null)
				{
					list2.Add(foundedEmote);
				}
			}
			if (list2.Count > 0)
			{
				list2[Random.Range(0, list2.Count)].DoClick();
			}
		}
		else
		{
			if (EmoteIndex + 1 > list.Count)
			{
				return;
			}
			m_totalEmotesInfo.SetValue(__instance, (int)m_totalEmotesInfo.GetValue(__instance) + 1);
			if (!((EntityBase)GameState.Get().GetGameEntity()).HasTag((GAME_TAG)477))
			{
				list[EmoteIndex].DoClick();
				return;
			}
			List<EmoteOption> list3 = new List<EmoteOption>();
			foreach (EmoteOption item2 in list.Concat(__instance.m_HiddenEmotes))
			{
				if (item2.CanPlayerUseEmoteType(GameState.Get().GetFriendlySidePlayer()))
				{
					list3.Add(item2);
				}
			}
			if (list3.Count > 0)
			{
				list3[Random.Range(0, list3.Count)].DoClick();
			}
		}
	}

	public static void DetermineFoundedEmotes(this EmoteHandler __instance)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Expected O, but got Unknown
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Expected O, but got Unknown
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected O, but got Unknown
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Expected O, but got Unknown
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected O, but got Unknown
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Expected O, but got Unknown
		if (m_FoundedEmotes != null && m_FoundedEmotes.Count != 0)
		{
			return;
		}
		m_FoundedEmotes = new List<EmoteOption>(11);
		EmoteOption val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 34);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)34,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_GREETINGS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 30);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)30,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_GREETINGS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 32);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)32,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_GREETINGS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 49);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)49,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_GREETINGS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 52);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)52,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_GREETINGS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 35);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)35,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 36);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)36,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 37);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)37,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 39);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)39,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 40);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)40,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		val = __instance.m_EmoteOverrides.FirstOrDefault((EmoteOption x) => (int)x.m_EmoteType == 41);
		if ((Object)(object)val == (Object)null)
		{
			val = new EmoteOption
			{
				m_EmoteType = (EmoteType)41,
				m_StringTag = "GAMEPLAY_EMOTE_LABEL_FIREWORKS"
			};
		}
		m_FoundedEmotes.Add(val);
		foreach (EmoteOption foundedEmote in m_FoundedEmotes)
		{
			foundedEmote.UpdateEmoteType();
		}
	}
}
public static class EnemyEmoteHandlerPatch
{
	private static readonly MethodInfo doSquelchClickInfo = typeof(EnemyEmoteHandler).GetMethod("DoSquelchClick", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo m_squelchedInfo = typeof(EnemyEmoteHandler).GetField("m_squelched", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void DoSquelchClick(this EnemyEmoteHandler __instance)
	{
		doSquelchClickInfo?.Invoke(__instance, null);
	}

	public static void SquelchPlayer(this EnemyEmoteHandler __instance, int playerId)
	{
		if (m_squelchedInfo.GetValue(__instance) is Map<int, bool> val)
		{
			val[playerId] = true;
		}
	}
}
public static class PackOpeningDirectorPatch
{
	public static bool m_WaitingForCards;

	public static bool m_WaitingForAllCardsRevealed;

	private static readonly FieldInfo m_hiddenCardsInfo = typeof(PackOpeningDirector).GetField("m_hiddenCards", BindingFlags.Instance | BindingFlags.NonPublic);

	public static IEnumerator ForceRevealAllCards(this PackOpeningDirector __instance)
	{
		object value = m_hiddenCardsInfo.GetValue(__instance);
		HiddenCards m_hiddenCards = (HiddenCards)((value is HiddenCards) ? value : null);
		if (m_hiddenCards != null)
		{
			m_WaitingForAllCardsRevealed = true;
			while (m_WaitingForCards)
			{
				yield return (object)new WaitForSeconds(0.05f);
			}
			m_hiddenCards.ForceRevealAllCards();
			m_WaitingForAllCardsRevealed = false;
		}
	}
}
public static class FriendMgrPatch
{
	private static BnetPlayer m_currentOpponent;

	public static BnetPlayer GetCurrentOpponent(this FriendMgr __instance)
	{
		return m_currentOpponent;
	}

	private static void UpdateCurrentOpponent()
	{
		if (GameState.Get() == null)
		{
			m_currentOpponent = null;
			return;
		}
		Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
		if (opposingSidePlayer == null)
		{
			m_currentOpponent = null;
		}
		else
		{
			m_currentOpponent = BnetPresenceMgr.Get().GetPlayer(opposingSidePlayer.GetGameAccountId());
		}
	}

	public static void OnGameCreated(CreateGamePhase phase, object userData)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		GameState.Get().UnregisterCreateGameListener(new CreateGameCallback(OnGameCreated), (object)null);
		UpdateCurrentOpponent();
	}
}
public static class PackOpeningPatch
{
	private static readonly MethodInfo onReloginComplete = typeof(PackOpening).GetMethod("OnReloginComplete", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo updatePacks = typeof(PackOpening).GetMethod("UpdatePacks", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void OnReloginComplete(this PackOpening __instance)
	{
		onReloginComplete?.Invoke(__instance, null);
	}

	public static void UpdatePacks(this PackOpening __instance)
	{
		updatePacks?.Invoke(__instance, null);
	}
}
[BepInPlugin("HsMod", "HsMod", "4.1.0.0")]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		Utils.DeleteFolder(PlatformFilePaths.ExternalDataPath + "/Cache");
		Utils.DeleteFolder(PlatformFilePaths.PersistentDataPath + "/Cache");
		string text = "";
		if (UtilsArgu.Instance.Exists("hsunitid"))
		{
			text = UtilsArgu.Instance.Single("hsunitid");
		}
		if (text.Length <= 0)
		{
			PluginConfig.ConfigBind(((BaseUnityPlugin)this).Config);
		}
		else
		{
			PluginConfig.ConfigBind(new ConfigFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx/config", text, "HsMod.cfg"), false, new BepInPlugin("HsMod", "HsMod", "4.1.0.0")));
		}
		if (UtilsArgu.Instance.Exists("port") && int.TryParse(UtilsArgu.Instance.Single("port"), out var result) && result > 0 && result < 65535)
		{
			PluginConfig.CommandConfig.webServerPort = result;
		}
		if (UtilsArgu.Instance.Exists("width") && int.TryParse(UtilsArgu.Instance.Single("width"), out var result2) && result2 > 0 && result2 < 65535)
		{
			PluginConfig.CommandConfig.width = result2;
		}
		if (UtilsArgu.Instance.Exists("height") && int.TryParse(UtilsArgu.Instance.Single("height"), out var result3) && result3 > 0 && result3 < 65535)
		{
			PluginConfig.CommandConfig.height = result3;
		}
		if (UtilsArgu.Instance.Exists("matchPath"))
		{
			PluginConfig.CommandConfig.hsMatchLogPath = UtilsArgu.Instance.Single("matchPath");
		}
		if (UtilsArgu.Instance.Exists("afk") && int.TryParse(UtilsArgu.Instance.Single("afk"), out var result4))
		{
			PluginConfig.timeGear.Value = 0;
			switch (result4)
			{
			case 0:
				PluginConfig.isPluginEnable.Value = false;
				PluginConfig.isTimeGearEnable.Value = false;
				break;
			case 1:
				PluginConfig.isPluginEnable.Value = true;
				PluginConfig.isTimeGearEnable.Value = true;
				PluginConfig.configTemplate.Value = Utils.ConfigTemplate.AwayFromKeyboard;
				break;
			case 2:
				PluginConfig.isPluginEnable.Value = true;
				PluginConfig.configTemplate.Value = Utils.ConfigTemplate.AntiAwayFromKeyboard;
				PluginConfig.isShortcutsEnable.Value = false;
				break;
			case 3:
				PluginConfig.isPluginEnable.Value = true;
				PluginConfig.isTimeGearEnable.Value = false;
				break;
			case 4:
				PluginConfig.isPluginEnable.Value = true;
				PluginConfig.configTemplate.Value = Utils.ConfigTemplate.AntiAwayFromKeyboard;
				break;
			}
		}
		if (PluginConfig.isPluginEnable.Value)
		{
			((BaseUnityPlugin)this).Logger.LogInfo((object)"Plugin HsMod is loaded!");
			PatchManager.PatchSettingDelegate();
			PatchManager.PatchAll();
		}
		else
		{
			OnDestroy();
		}
	}

	private void Start()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((BaseUnityPlugin)this).Logger.LogInfo((object)"Plugin HsMod is started!");
		if (!PluginConfig.isPluginEnable.Value)
		{
			OnDestroy();
			return;
		}
		PluginConfig.showFPS = new GameObject("ShowFPSSceneObject", new Type[1] { typeof(HSDontDestroyOnLoad) }).AddComponent<ShowFPS>();
		((Behaviour)PluginConfig.showFPS).enabled = false;
		PluginConfig.showFPS.StartFrameCount();
		PluginConfig.showFPS.StopFrameCount();
		PluginConfig.showFPS.ClearFrameCount();
		PluginConfig.isShowFPSEnable.SettingChanged += delegate
		{
			((Behaviour)PluginConfig.showFPS).enabled = PluginConfig.isShowFPSEnable.Value;
		};
		if (PluginConfig.isShowFPSEnable.Value)
		{
			((Behaviour)PluginConfig.showFPS).enabled = true;
		}
		if (PluginConfig.targetFrameRate.Value > 0)
		{
			Options obj = Options.Get();
			if (((obj != null) ? new int?(obj.GetInt((Option)33)) : ((int?)null)) != PluginConfig.targetFrameRate.Value)
			{
				PluginConfig.graphicsManager = ServiceManager.Get<IGraphicsManager>();
				IGraphicsManager graphicsManager = PluginConfig.graphicsManager;
				if (graphicsManager != null)
				{
					graphicsManager.UpdateTargetFramerate(PluginConfig.targetFrameRate.Value, false);
				}
			}
		}
		if (PluginConfig.CommandConfig.width > 0 && PluginConfig.CommandConfig.height > 0)
		{
			Screen.SetResolution(PluginConfig.CommandConfig.width, PluginConfig.CommandConfig.height, false);
		}
		if (string.IsNullOrEmpty(PluginConfig.webPageBackImg.Value) || PluginConfig.webPageBackImg.Value.EndsWith("safeimg"))
		{
			Utils.TryGetSafeImg();
		}
		WebServer.Start();
	}

	private void Update()
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Invalid comparison between Unknown and I4
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Invalid comparison between Unknown and I4
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Invalid comparison between Unknown and I4
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyUp((KeyCode)285))
		{
			int num = 0;
			foreach (Harmony item in PatchManager.AllHarmony)
			{
				num += item.GetPatchedMethods().Count();
			}
			PluginConfig.LoadSkinsConfigFromFile();
			UIStatus.Get().AddInfo($"[{num}]插件状态：" + (PluginConfig.isPluginEnable.Value ? "运行" : "停止"));
			InactivePlayerKicker.Get().SetShouldCheckForInactivity(PluginConfig.isIdleKickEnable.Value);
			WebServer.Restart();
		}
		if (!PluginConfig.isPluginEnable.Value || !PluginConfig.isShortcutsEnable.Value || !Input.anyKey)
		{
			return;
		}
		KeyboardShortcut value = PluginConfig.keyTimeGearUp.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			if (PluginConfig.timeGear.Value != 8)
			{
				if (PluginConfig.timeGear.Value <= -2 || PluginConfig.timeGear.Value >= 2)
				{
					ConfigEntry<int> timeGear = PluginConfig.timeGear;
					timeGear.Value += 1;
				}
				else
				{
					PluginConfig.timeGear.Value = 2;
				}
			}
			return;
		}
		value = PluginConfig.keyTimeGearDown.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			if (PluginConfig.timeGear.Value != -8)
			{
				if (PluginConfig.timeGear.Value <= -2 || PluginConfig.timeGear.Value >= 2)
				{
					ConfigEntry<int> timeGear2 = PluginConfig.timeGear;
					timeGear2.Value -= 1;
				}
				else
				{
					PluginConfig.timeGear.Value = -2;
				}
			}
			return;
		}
		value = PluginConfig.keyTimeGearMax.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			PluginConfig.timeGear.Value = ((PluginConfig.timeGear.Value >= 4) ? 8 : 4);
			return;
		}
		value = PluginConfig.keyTimeGearDefault.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			PluginConfig.timeGear.Value = 0;
			return;
		}
		value = PluginConfig.keySimulateDisconnect.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			Network.Get().SimulateUncleanDisconnectFromGameServer();
			return;
		}
		value = PluginConfig.keyShowFPS.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			PluginConfig.isShowFPSEnable.Value = !PluginConfig.isShowFPSEnable.Value;
			return;
		}
		if (SoundManager.Get() != null)
		{
			value = PluginConfig.keySoundMute.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				SoundManagerPatch.OnMuteKeyPressed();
				return;
			}
		}
		value = PluginConfig.keyReadNewCards.Value;
		if (((KeyboardShortcut)(ref value)).IsDown())
		{
			Utils.TryReadNewCards();
		}
		value = PluginConfig.keyRefund.Value;
		if (((KeyboardShortcut)(ref value)).IsDown() && (int)SceneMgr.Get().GetMode() != 4 && ((int)SceneMgr.Get().GetMode() == 5 || (int)SceneMgr.Get().GetMode() == 6))
		{
			Utils.LeakInfo.MyCards();
			Utils.TryRefundCardDisenchant();
		}
		else
		{
			if (GameState.Get() == null || GameMgr.Get() == null)
			{
				return;
			}
			if (GameMgr.Get().IsBattlegrounds())
			{
				value = PluginConfig.keyShutUpBob.Value;
				if (((KeyboardShortcut)(ref value)).IsDown())
				{
					PluginConfig.isShutUpBobEnable.Value = !PluginConfig.isShutUpBobEnable.Value;
					return;
				}
			}
			if (!GameState.Get().IsGameCreated())
			{
				return;
			}
			if (!GameMgr.Get().IsSpectator())
			{
				value = PluginConfig.keyConcede.Value;
				if (((KeyboardShortcut)(ref value)).IsDown())
				{
					GameState.Get().Concede();
					return;
				}
				if (GameState.Get().IsMulliganManagerActive() && (Object)(object)MulliganManager.Get().GetMulliganButton() != (Object)null)
				{
					value = PluginConfig.keyContinueMulligan.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						MulliganManager.Get().AutomaticContinueMulligan();
						return;
					}
				}
			}
			if (GameMgr.Get().IsBattlegrounds())
			{
				value = PluginConfig.keyCopySelectBattleTag.Value;
				if (((KeyboardShortcut)(ref value)).IsDown() && (Object)(object)PlayerLeaderboardManager.Get() != (Object)null && PlayerLeaderboardManager.Get().IsMousedOver())
				{
					BnetPlayer selectedOpponent = PlayerLeaderboardManager.Get().GetSelectedOpponent();
					if (selectedOpponent != null)
					{
						BnetBattleTag battleTag = selectedOpponent.GetBattleTag();
						if (battleTag != (BnetBattleTag)null)
						{
							string text = battleTag.GetString();
							ClipboardUtils.CopyToClipboard(text);
							UIStatus.Get().AddInfo(text);
						}
					}
					return;
				}
			}
			value = PluginConfig.keyCopyBattleTag.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				BnetPlayer val = null;
				if (GameMgr.Get().IsBattlegrounds())
				{
					if ((Object)(object)PlayerLeaderboardManager.Get() != (Object)null)
					{
						val = PlayerLeaderboardManager.Get()?.GetCurrentOpponent();
					}
				}
				else if (FriendMgr.Get() != null)
				{
					val = FriendMgr.Get()?.GetCurrentOpponent();
				}
				try
				{
					BnetBattleTag val2 = ((val != null) ? val.GetBattleTag() : null);
					if (val2 != (BnetBattleTag)null)
					{
						ClipboardUtils.CopyToClipboard(val2.GetString());
						UIStatus.Get().AddInfo(val2.GetString());
					}
					else if (!GameMgr.Get().IsBattlegrounds())
					{
						val = BnetPresenceMgr.Get().GetPlayer(GameState.Get().GetOpposingSidePlayer().GetGameAccountId());
						string text2 = ((val == null) ? null : ((object)val.GetBattleTag())?.ToString());
						text2 = ((text2 == null) ? Utils.CacheLastOpponentFullName : (Utils.CacheLastOpponentFullName.StartsWith(text2) ? Utils.CacheLastOpponentFullName : text2));
						ClipboardUtils.CopyToClipboard(text2);
						UIStatus.Get().AddInfo(text2);
					}
					return;
				}
				catch (Exception message)
				{
					Utils.MyLogger((LogLevel)2, message);
					return;
				}
			}
			if (!GameState.Get().IsMainPhase())
			{
				return;
			}
			value = PluginConfig.keySquelch.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				EnemyEmoteHandler.Get().DoSquelchClick();
			}
			else
			{
				if (GameMgr.Get().IsSpectator())
				{
					return;
				}
				value = PluginConfig.keyContinueMulligan.Value;
				if (((KeyboardShortcut)(ref value)).IsDown())
				{
					InputManager.Get().DoEndTurnButton();
				}
				else
				{
					if ((Object)(object)EmoteHandler.Get() == (Object)null)
					{
						return;
					}
					value = PluginConfig.keyEmoteGreetings.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(0);
						return;
					}
					value = PluginConfig.keyEmoteWellPlayed.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(1);
						return;
					}
					value = PluginConfig.keyEmoteThanks.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(2);
						return;
					}
					value = PluginConfig.keyEmoteWow.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(3);
						return;
					}
					value = PluginConfig.keyEmoteOops.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(4);
						return;
					}
					value = PluginConfig.keyEmoteThreaten.Value;
					if (((KeyboardShortcut)(ref value)).IsDown())
					{
						EmoteHandler.Get().HandleKeyboardInput(5);
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
	}
}
internal class PluginInfo
{
	public const string PLUGIN_GUID = "HsMod";

	public const string PLUGIN_NAME = "HsMod";

	public const string PLUGIN_AUTHOR = "Pik_4";

	public const string PLUGIN_VERSION = "4.1.0.0";
}
public class Utils
{
	public enum CardState
	{
		[Description("默认（不做修改）")]
		Default,
		[Description("仅友方有效")]
		OnlyMy,
		[Description("全部生效")]
		All,
		[Description("禁用特效")]
		Disabled
	}

	public enum SkinType
	{
		[Description("卡背")]
		CARDBACK,
		[Description("卡牌")]
		CARD,
		[Description("硬币")]
		COIN,
		[Description("英雄皮肤")]
		HERO,
		[Description("酒馆鲍勃")]
		BOB,
		[Description("酒馆终结特效")]
		BATTLEGROUNDSFINISHER,
		[Description("酒馆战场")]
		BATTLEGROUNDSBOARD,
		[Description("酒馆英雄皮肤")]
		BATTLEGROUNDSHERO,
		[Description("英雄技能")]
		HEROPOWER
	}

	public enum AlertPopupResponse
	{
		OK,
		CONFIRM,
		CANCEL,
		YES,
		DONOTHING
	}

	public enum ConfigTemplate
	{
		[Description("默认")]
		DoNothing,
		[Description("挂机")]
		AwayFromKeyboard,
		[Description("反挂机")]
		AntiAwayFromKeyboard
	}

	public enum BuyAdventureTemplate
	{
		[Description("默认")]
		DoNothing,
		[Description("纳克萨玛斯的诅咒")]
		BuyNAX,
		[Description("黑石山的火焰")]
		BuyBRM,
		[Description("探险者协会")]
		BuyLOE,
		[Description("卡拉赞之夜")]
		BuyKara
	}

	public enum CardRarity
	{
		COMMON = 1,
		RARE = 3,
		EPIC = 4,
		LEGENDARY = 5
	}

	public enum DevicePreset
	{
		Default,
		iPad,
		iPhone,
		Phone,
		Tablet,
		HuaweiPhone,
		Custom
	}

	public class CardCount
	{
		public int legendary;

		public int epic;

		public int rare;

		public int common;

		public int total;

		public int gLegendary;

		public int gEpic;

		public int gRare;

		public int gCommon;

		public int gTotal;

		public int totalDust;
	}

	public struct CollectionCard
	{
		public string Name;

		public int Count;

		public TAG_RARITY Rarity;

		public TAG_PREMIUM Premium;
	}

	public struct MercenarySkin
	{
		public string Name;

		public List<int> Id;

		public bool hasDiamond;

		public int Diamond;

		public int Default;
	}

	public struct CardMapping
	{
		public int RealDbID { get; set; }

		public int FakeDbID { get; set; }

		public string RealCardID { get; set; }

		public string FakeCardID { get; set; }

		public SkinType ThisSkinType { get; set; }
	}

	public static class CacheInfo
	{
		public static void UpdateCoin()
		{
			CacheCoin.Clear();
			foreach (CoinDbfRecord record in GameDbf.Coin.GetRecords())
			{
				if (record != null)
				{
					CacheCoin.Add(record.CardId);
				}
			}
		}

		public static void UpdateCoinCard()
		{
			CacheCoinCard.Clear();
			foreach (CoinDbfRecord record in GameDbf.Coin.GetRecords())
			{
				if (record != null)
				{
					CacheCoinCard.Add(record.CardId);
				}
			}
		}

		public static void UpdateGameBoard()
		{
			CacheGameBoard.Clear();
			foreach (BoardDbfRecord record in GameDbf.Board.GetRecords())
			{
				if (record != null)
				{
					CacheGameBoard.Add(((DbfRecord)record).ID);
				}
			}
		}

		public static void UpdateBgsBoard()
		{
			CacheBgsBoard.Clear();
			foreach (BattlegroundsBoardSkinDbfRecord record in GameDbf.BattlegroundsBoardSkin.GetRecords())
			{
				if (record != null)
				{
					CacheBgsBoard.Add(((DbfRecord)record).ID);
				}
			}
		}

		public static void UpdateHeroes()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			CacheHeroes.Clear();
			foreach (CardHeroDbfRecord record in GameDbf.CardHero.GetRecords())
			{
				if (record != null)
				{
					CacheHeroes.Add(record.CardId, record.HeroType);
				}
			}
		}

		public static void UpdateCardBack()
		{
			CacheCardBack.Clear();
			foreach (CardBackDbfRecord record in GameDbf.CardBack.GetRecords())
			{
				if (record != null)
				{
					CacheCardBack.Add(((DbfRecord)record).ID);
				}
			}
		}

		public static void UpdateBgsFinisher()
		{
			CacheBgsFinisher.Clear();
			foreach (BattlegroundsFinisherDbfRecord record in GameDbf.BattlegroundsFinisher.GetRecords())
			{
				if (record != null)
				{
					CacheBgsFinisher.Add(((DbfRecord)record).ID);
				}
			}
		}

		public static void UpdateMercenarySkin()
		{
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Invalid comparison between Unknown and I4
			CacheMercenarySkin.Clear();
			foreach (LettuceMercenaryDbfRecord record in GameDbf.LettuceMercenary.GetRecords())
			{
				if (record == null || record.MercenaryArtVariations.Count <= 0)
				{
					continue;
				}
				MercenarySkin item = new MercenarySkin
				{
					Name = record.MercenaryArtVariations.First().CardRecord.Name.GetString(),
					Id = new List<int>(),
					hasDiamond = false
				};
				foreach (MercenaryArtVariationDbfRecord item2 in record.MercenaryArtVariations.OrderBy((MercenaryArtVariationDbfRecord x) => ((DbfRecord)x).ID).ToList())
				{
					if (item2 == null)
					{
						continue;
					}
					item.Id.Add(item2.CardId);
					if (item2.DefaultVariation)
					{
						item.Default = item2.CardId;
					}
					foreach (MercenaryArtVariationPremiumDbfRecord mercenaryArtVariationPremium in item2.MercenaryArtVariationPremiums)
					{
						if (mercenaryArtVariationPremium != null && (int)mercenaryArtVariationPremium.Premium == 2)
						{
							item.hasDiamond = true;
							item.Diamond = item2.CardId;
						}
					}
				}
				CacheMercenarySkin.Add(item);
			}
		}
	}

	public static class CheckInfo
	{
		public static bool IsCoin()
		{
			if (CacheCoin.Count == 0)
			{
				CacheInfo.UpdateCoin();
			}
			if (CacheCoin.Contains(PluginConfig.skinCoin.Value))
			{
				return true;
			}
			return false;
		}

		public static bool IsCoin(string cardId)
		{
			int item = GameUtils.TranslateCardIdToDbId(cardId, false);
			if (CacheCoinCard.Count == 0)
			{
				CacheInfo.UpdateCoinCard();
			}
			if (CacheCoinCard.Contains(item))
			{
				return true;
			}
			return false;
		}

		public static bool IsBoard()
		{
			if (CacheGameBoard.Count == 0)
			{
				CacheInfo.UpdateGameBoard();
			}
			if (CacheGameBoard.Contains(PluginConfig.skinBoard.Value))
			{
				return true;
			}
			return false;
		}

		public static bool IsBgsBoard()
		{
			if (CacheBgsBoard.Count == 0)
			{
				CacheInfo.UpdateBgsBoard();
			}
			if (CacheBgsBoard.Contains(PluginConfig.skinBgsBoard.Value))
			{
				return true;
			}
			return false;
		}

		public static bool IsHero(int DbID, out HeroType heroType)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected I4, but got Unknown
			if (CacheHeroes.Count == 0)
			{
				CacheInfo.UpdateHeroes();
			}
			if (CacheHeroes.ContainsKey(DbID))
			{
				heroType = (HeroType)(int)CacheHeroes[DbID];
				return true;
			}
			heroType = (HeroType)0;
			return false;
		}

		public static bool IsCardBack()
		{
			if (CacheCardBack.Count == 0)
			{
				CacheInfo.UpdateCardBack();
			}
			if (CacheCardBack.Contains(PluginConfig.skinCardBack.Value))
			{
				return true;
			}
			return false;
		}

		public static bool IsBgsFinisher()
		{
			if (CacheBgsFinisher.Count == 0)
			{
				CacheInfo.UpdateBgsFinisher();
			}
			if (CacheBgsFinisher.Contains(PluginConfig.skinBgsFinisher.Value))
			{
				return true;
			}
			return false;
		}

		public static bool IsHero(string cardID, out HeroType heroType)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected I4, but got Unknown
			if (CacheHeroes.Count == 0)
			{
				CacheInfo.UpdateHeroes();
			}
			int key = GameUtils.TranslateCardIdToDbId(cardID, false);
			if (CacheHeroes.ContainsKey(key))
			{
				heroType = (HeroType)(int)CacheHeroes[key];
				return true;
			}
			heroType = (HeroType)0;
			return false;
		}

		public static bool IsMercenarySkin(string cardID, out MercenarySkin skin)
		{
			if (CacheMercenarySkin.Count == 0)
			{
				CacheInfo.UpdateMercenarySkin();
			}
			int item = GameUtils.TranslateCardIdToDbId(cardID, false);
			foreach (MercenarySkin item2 in CacheMercenarySkin)
			{
				if (item2.Id.Contains(item))
				{
					skin = item2;
					return true;
				}
			}
			skin = default(MercenarySkin);
			return false;
		}
	}

	public static class LeakInfo
	{
		public static void Mercenaries(string savePath = "BepInEx/HsMercenaries.log")
		{
			File.WriteAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到关卡信息如下：\n");
			File.AppendAllText(savePath, "[ID]\t[Heroic?]\t[Bounty]\t[BossName]\n");
			foreach (LettuceBountyDbfRecord record in GameDbf.LettuceBounty.GetRecords())
			{
				if (record != null)
				{
					string text = ((DbfRecord)record).ID + (record.Heroic ? " Heroic " : " ") + record.BountySetRecord.Name.GetString() + " " + GameDbf.Card.GetRecord(record.FinalBossCardId).Name.GetString();
					File.AppendAllText(savePath, text + "\n");
				}
			}
		}

		public static void MyCards(string savePath = "BepInEx/HsRefundCards.log")
		{
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到全额分解卡牌情况如下：\n");
			File.AppendAllText(savePath, "[Name]\t[PremiumType]\t[Rarity]\t[CardId]\t[CardDbId]\t[OwnedCount]\n");
			foreach (CollectibleCard ownedCard in CollectionManager.Get().GetOwnedCards())
			{
				if (ownedCard != null && ownedCard.IsCraftable && ownedCard.IsRefundable && ownedCard.OwnedCount > 0)
				{
					string text = $"{ownedCard.Name}\t{ownedCard.PremiumType}\t{ownedCard.Rarity}\t{ownedCard.CardId}\t{ownedCard.CardDbId}\t{ownedCard.OwnedCount}\t";
					File.AppendAllText(savePath, text + "\n");
				}
			}
		}

		public static void Skins(string savePath = "BepInEx/HsSkins.log")
		{
			//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
			File.WriteAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到硬币皮肤如下：\n");
			File.AppendAllText(savePath, "[CARD_ID]\t[Name]\n");
			foreach (CoinDbfRecord record in GameDbf.Coin.GetRecords())
			{
				if (record != null)
				{
					string text = $"{record.CardId}\t{record.Name.GetString()}";
					File.AppendAllText(savePath, text + "\n");
				}
			}
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到卡背信息如下：\n");
			File.AppendAllText(savePath, "[ID]\t[Name]\n");
			foreach (CardBackDbfRecord record2 in GameDbf.CardBack.GetRecords())
			{
				if (record2 != null)
				{
					string text2 = $"{((DbfRecord)record2).ID}\t{record2.Name.GetString()}";
					File.AppendAllText(savePath, text2 + "\n");
				}
			}
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到游戏面板信息如下：\n");
			File.AppendAllText(savePath, "[ID]\t[NOTE_DESC]\n");
			foreach (BoardDbfRecord record3 in GameDbf.Board.GetRecords())
			{
				if (record3 != null)
				{
					string text3 = string.Format("{0}\t{1}", ((DbfRecord)record3).ID, ((DbfRecord)record3).GetVar("NOTE_DESC"));
					File.AppendAllText(savePath, text3 + "\n");
				}
			}
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到酒馆战斗面板如下：\n");
			File.AppendAllText(savePath, "[ID]\t[CollectionShortName]\t[CollectionName]\n");
			foreach (BattlegroundsBoardSkinDbfRecord record4 in GameDbf.BattlegroundsBoardSkin.GetRecords())
			{
				if (record4 != null)
				{
					string text4 = $"{((DbfRecord)record4).ID}\t{record4.CollectionShortName.GetString()}\t{record4.CollectionName.GetString()}";
					File.AppendAllText(savePath, text4 + "\n");
				}
			}
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到酒馆终结特效如下：\n");
			File.AppendAllText(savePath, "[ID]\t[CollectionShortName]\t[CollectionName]\n");
			foreach (BattlegroundsFinisherDbfRecord record5 in GameDbf.BattlegroundsFinisher.GetRecords())
			{
				if (record5 != null)
				{
					string text5 = $"{((DbfRecord)record5).ID}\t{record5.CollectionShortName.GetString()}\t{record5.CollectionName.GetString()}";
					File.AppendAllText(savePath, text5 + "\n");
				}
			}
			File.AppendAllText(savePath, DateTime.Now.ToLocalTime().ToString() + "\t获取到英雄皮肤（包括酒馆）如下：\n");
			File.AppendAllText(savePath, "[CARD_ID]\t[Name]\t[HeroType]\n");
			foreach (CardHeroDbfRecord record6 in GameDbf.CardHero.GetRecords())
			{
				if (record6 != null)
				{
					string arg = GameDbf.Card.GetRecord(record6.CardId).Name.GetString();
					string text6 = $"{record6.CardId}\t{arg}\t{record6.HeroType}\t";
					File.AppendAllText(savePath, text6 + "\n");
				}
			}
		}
	}

	public static List<int> CacheCoin = new List<int>();

	public static List<int> CacheCoinCard = new List<int>();

	public static List<int> CacheGameBoard = new List<int>();

	public static List<int> CacheBgsBoard = new List<int>();

	public static List<int> CacheCardBack = new List<int>();

	public static List<int> CacheBgsFinisher = new List<int>();

	public static Dictionary<int, HeroType> CacheHeroes = new Dictionary<int, HeroType>();

	public static string CacheLastOpponentFullName;

	public static string CacheRawHeroCardId;

	public static BnetAccountId CacheLastOpponentAccountID;

	public static List<MercenarySkin> CacheMercenarySkin = new List<MercenarySkin>();

	public static string CardsCount(TAG_RARITY Rarity, TAG_PREMIUM premium, int count, ref CardCount cardCount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		bool flag = (int)premium == 1;
		switch (Rarity - 1)
		{
		case 0:
			if (flag)
			{
				cardCount.gCommon += count;
				cardCount.gTotal += count;
				cardCount.total += count;
				cardCount.totalDust += count * 50;
				return $"<td>金色普通</td><td>{count}</td>";
			}
			cardCount.common += count;
			cardCount.total += count;
			cardCount.totalDust += count * 5;
			return $"<td>普通</td><td>{count}</td>";
		case 2:
			if (flag)
			{
				cardCount.gRare += count;
				cardCount.gTotal += count;
				cardCount.total += count;
				cardCount.totalDust += count * 100;
				return $"<td>金色稀有</td><td>{count}</td>";
			}
			cardCount.rare += count;
			cardCount.total += count;
			cardCount.totalDust += count * 20;
			return $"<td>稀有</td><td>{count}</td>";
		case 3:
			if (flag)
			{
				cardCount.gEpic += count;
				cardCount.gTotal += count;
				cardCount.total += count;
				cardCount.totalDust += count * 400;
				return $"<td>金色史诗</td><td>{count}</td>";
			}
			cardCount.epic += count;
			cardCount.total += count;
			cardCount.totalDust += count * 100;
			return $"<td>史诗</td><td>{count}</td>";
		case 4:
			if (flag)
			{
				cardCount.gLegendary += count;
				cardCount.gTotal += count;
				cardCount.total += count;
				cardCount.totalDust += count * 1600;
				return $"<td>金色传说</td><td>{count}</td>";
			}
			cardCount.legendary += count;
			cardCount.total += count;
			cardCount.totalDust += count * 400;
			return $"<td>传说</td><td>{count}</td>";
		default:
			return "<td>未知</td>";
		}
	}

	public static string RankIdxToString(int rank)
	{
		string text;
		switch ((rank - 1) / 10)
		{
		case 0:
			text = "青铜";
			break;
		case 1:
			text = "白银";
			break;
		case 2:
			text = "黄金";
			break;
		case 3:
			text = "铂金";
			break;
		case 4:
			text = "钻石";
			break;
		case 5:
			return "传说";
		default:
			text = "未知";
			break;
		}
		return text + (11 - (rank - (rank - 1) / 10 * 10));
	}

	public static void MyLogger(LogLevel level, object message)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		ManualLogSource val = Logger.CreateLogSource("HsMod.MyLogger");
		val.Log(level, message);
		Logger.Sources.Remove((ILogSource)(object)val);
	}

	public static void TryReportOpponent()
	{
		List<SubcomplaintType> list = new List<SubcomplaintType> { (SubcomplaintType)201 };
		BattleNet.Get().SubmitReport(CacheLastOpponentAccountID, (ComplaintType)2, list);
		list.Clear();
		list.Add((SubcomplaintType)301);
		list.Add((SubcomplaintType)302);
		BattleNet.Get().SubmitReport(CacheLastOpponentAccountID, (ComplaintType)3, list);
		list.Clear();
		list.Add((SubcomplaintType)303);
		BattleNet.Get().SubmitReport(CacheLastOpponentAccountID, (ComplaintType)3, list);
		list.Clear();
		MyLogger((LogLevel)4, CacheLastOpponentFullName + ((object)((BnetEntityId)CacheLastOpponentAccountID).EntityId).ToString() + "已举报");
	}

	public static void TryRefundCardDisenchantCallback()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		CardSaleResult cardSaleResult = Network.Get().GetCardSaleResult();
		if ((int)cardSaleResult.Action != 2)
		{
			MyLogger((LogLevel)4, $"分解失败：{cardSaleResult.Action}");
			UIStatus.Get().AddInfo("分解失败");
		}
		else
		{
			MyLogger((LogLevel)4, "分解成功");
			CollectionManager.Get().OnCollectionChanged();
		}
	}

	public static void TryReadNewCards()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		if ((int)SceneMgr.Get().GetMode() == 5 || (int)SceneMgr.Get().GetMode() == 6)
		{
			foreach (CollectibleCard ownedCard in CollectionManager.Get().GetOwnedCards())
			{
				if (ownedCard != null && ownedCard.IsNewCard)
				{
					CollectionManager.Get().MarkAllInstancesAsSeen(ownedCard.CardId, ownedCard.PremiumType);
				}
			}
		}
		SceneMgr obj = SceneMgr.Get();
		if ((obj == null || (int)obj.GetMode() != 24) && (int)SceneMgr.Get().GetMode() != 28)
		{
			return;
		}
		List<MercenaryAcknowledgeData> list = new List<MercenaryAcknowledgeData>();
		foreach (LettuceMercenary mercenary in CollectionManager.Get().FindMercenaries((string)null, (bool?)true, (bool?)null, (bool?)null, (bool?)null, true, (bool?)null).m_mercenaries)
		{
			foreach (LettuceAbility ability in mercenary.m_abilityList)
			{
				if (!ability.IsAcknowledged(mercenary))
				{
					MercenaryAcknowledgeData val = new MercenaryAcknowledgeData
					{
						Type = (AcknowledgeType)1,
						AssetId = ability.ID,
						Acknowledged = true,
						MercenaryId = mercenary.ID
					};
					list.Append(val);
					CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(val);
				}
			}
			foreach (LettuceAbility equipment in mercenary.m_equipmentList)
			{
				if (!equipment.IsAcknowledged(mercenary))
				{
					MercenaryAcknowledgeData val2 = new MercenaryAcknowledgeData
					{
						Type = (AcknowledgeType)5,
						AssetId = equipment.ID,
						Acknowledged = true,
						MercenaryId = mercenary.ID
					};
					list.Append(val2);
					CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(val2);
				}
			}
			foreach (ArtVariation artVariation in mercenary.m_artVariations)
			{
				if (!artVariation.m_acknowledged)
				{
					MercenaryAcknowledgeData val3 = new MercenaryAcknowledgeData
					{
						Type = (AcknowledgeType)4,
						AssetId = ((DbfRecord)artVariation.m_record).ID,
						Acknowledged = true,
						MercenaryId = mercenary.ID
					};
					list.Append(val3);
					CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(val3);
				}
			}
		}
		Network.Get().RegisterNetHandler((object)(PacketID)10174, new NetHandler(OnMercCollectionAcknowledgeResponse), (TimeoutHandler)null);
		Network.Get().AcknowledgeMercenaryCollection(list);
		list.Clear();
	}

	public static void OnMercCollectionAcknowledgeResponse()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Network.Get().RemoveNetHandler((object)(PacketID)10174, new NetHandler(OnMercCollectionAcknowledgeResponse));
		if (!Network.Get().AcknowledgeMercenaryCollectionResponse().Success)
		{
			MyLogger((LogLevel)2, "Error acknowledging collection");
		}
	}

	public static void TryRefundCardDisenchant()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		int num = 0;
		Network val = Network.Get();
		val.RegisterNetHandler((object)(PacketID)258, new NetHandler(TryRefundCardDisenchantCallback), (TimeoutHandler)null);
		int num2 = default(int);
		int num3 = default(int);
		int num4 = default(int);
		foreach (CollectibleCard ownedCard in CollectionManager.Get().GetOwnedCards())
		{
			if (ownedCard != null && ownedCard.IsCraftable && ownedCard.IsRefundable && ownedCard.OwnedCount > 0)
			{
				CraftingManager.Get().TryGetCardSellValue(ownedCard.CardId, ownedCard.PremiumType, ref num2);
				num += ownedCard.OwnedCount * num2;
				CraftingManager.Get().TryGetCardSellValue(ownedCard.CardId, (TAG_PREMIUM)0, ref num3);
				CraftingManager.Get().TryGetCardSellValue(ownedCard.CardId, (TAG_PREMIUM)1, ref num4);
				int numCopiesInCollection = CollectionManager.Get().GetNumCopiesInCollection(ownedCard.CardId, (TAG_PREMIUM)0);
				int numCopiesInCollection2 = CollectionManager.Get().GetNumCopiesInCollection(ownedCard.CardId, (TAG_PREMIUM)1);
				int numCopiesInCollection3 = CollectionManager.Get().GetNumCopiesInCollection(ownedCard.CardId, (TAG_PREMIUM)3);
				CraftingPendingTransaction val2 = new CraftingPendingTransaction
				{
					CardID = ownedCard.CardId,
					Premium = ownedCard.PremiumType,
					NormalDisenchantCount = numCopiesInCollection,
					GoldenDisenchantCount = numCopiesInCollection2,
					SignatureDisenchantCount = numCopiesInCollection3
				};
				num2 = -(num3 * numCopiesInCollection + num4 * numCopiesInCollection2);
				val.CraftingTransaction(val2, num2, numCopiesInCollection, numCopiesInCollection2, numCopiesInCollection3);
				val2 = null;
			}
		}
		MyLogger((LogLevel)4, "尝试分解粉尘：" + num);
		UIStatus.Get().AddInfo("尝试分解粉尘：" + num);
	}

	public static void TryGetSafeImg()
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create("http://qndxx.youth54.cn/SmartLA/dxxjfgl.w?method=getNewestVersionInfo");
			obj.Timeout = 2333;
			obj.ReadWriteTimeout = 2333;
			obj.Method = "GET";
			obj.ContentType = "text/html;charset=UTF-8";
			obj.ServerCertificateValidationCallback = (object _s, X509Certificate _x509s, X509Chain _x509c, SslPolicyErrors _ssl) => true;
			Stream responseStream = ((HttpWebResponse)obj.GetResponse()).GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
			string input = streamReader.ReadToEnd();
			streamReader.Close();
			responseStream.Close();
			Match match = Regex.Match(input, "https?://h5.cyol.com/special/daxuexi/.*?/");
			if (match.Success)
			{
				MyLogger((LogLevel)4, match?.ToString() + "images/end.jpg");
				HttpWebRequest obj2 = (HttpWebRequest)WebRequest.Create("http://qndxx.youth54.cn/SmartLA/dxxjfgl.w?method=getNewestVersionInfo");
				obj2.Timeout = 2333;
				obj2.ReadWriteTimeout = 2333;
				obj2.Method = "GET";
				obj2.ContentType = "text/html;charset=UTF-8";
				obj2.ServerCertificateValidationCallback = (object _s, X509Certificate _x509s, X509Chain _x509c, SslPolicyErrors _ssl) => true;
				int num = Convert.ToInt32(((HttpWebResponse)obj2.GetResponse()).StatusCode);
				responseStream.Close();
				if (num != 200)
				{
					throw new Exception();
				}
				PluginConfig.webPageBackImg.Value = match?.ToString() + "images/end.jpg?safeimg";
				return;
			}
			throw new Exception();
		}
		catch (Exception ex)
		{
			MyLogger((LogLevel)4, ex);
			MyLogger((LogLevel)4, ex.StackTrace);
			PluginConfig.webPageBackImg.Value = "";
		}
	}

	public static List<int> GetCardsDbId()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		List<int> list = new List<int>();
		foreach (int collectibleCardDbId in GameDbf.GetIndex().GetCollectibleCardDbIds())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(collectibleCardDbId, false);
			if (entityDef != null && (int)entityDef.GetRarity() != 2 && (int)entityDef.GetRarity() != 0)
			{
				if ((int)((EntityBase)entityDef).GetCardType() != 3)
				{
					list.Add(collectibleCardDbId);
				}
				else if (((EntityBase)entityDef).GetCost() != 0)
				{
					list.Add(collectibleCardDbId);
				}
			}
		}
		return list;
	}

	public static int GetRandomCardID(TAG_RARITY rarity)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		List<int> cardsDbId = GetCardsDbId();
		int num;
		do
		{
			num = cardsDbId[Random.Range(0, cardsDbId.Count)];
		}
		while (DefLoader.Get().GetEntityDef(num, false).GetRarity() != rarity);
		return num;
	}

	public static void GenerateRandomCard(bool rarityRandom = false, bool premiumRandom = false, TAG_RARITY rarity = (TAG_RARITY)5, TAG_PREMIUM premium = (TAG_PREMIUM)1)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (!rarityRandom)
		{
			rarity = (TAG_RARITY)PluginConfig.fakeRandomRarity.Value;
		}
		if (!premiumRandom)
		{
			premium = PluginConfig.fakeRandomPremium.Value;
		}
		if (((object)PluginConfig.fakeBoosterDbId.Value/*cast due to .constrained prefix*/).ToString().Substring(0, 7) == "GOLDEN_")
		{
			premiumRandom = false;
			premium = (TAG_PREMIUM)1;
		}
		List<int> cardsDbId = GetCardsDbId();
		for (int i = 1; i <= 5; i++)
		{
			if (premiumRandom)
			{
				premium = (PluginConfig.isFakeAtypicalRandomPremium.Value ? ((TAG_PREMIUM)Random.Range(0, Enum.GetValues(typeof(TAG_PREMIUM)).Length)) : ((TAG_PREMIUM)Random.Range(0, 2)));
			}
			switch (i)
			{
			case 1:
				PluginConfig.fakeCardID1.Value = (rarityRandom ? cardsDbId[Random.Range(0, cardsDbId.Count)] : GetRandomCardID(rarity));
				PluginConfig.fakeCardPremium1.Value = premium;
				break;
			case 2:
				PluginConfig.fakeCardID2.Value = (rarityRandom ? cardsDbId[Random.Range(0, cardsDbId.Count)] : GetRandomCardID(rarity));
				PluginConfig.fakeCardPremium2.Value = premium;
				break;
			case 3:
				PluginConfig.fakeCardID3.Value = (rarityRandom ? cardsDbId[Random.Range(0, cardsDbId.Count)] : GetRandomCardID(rarity));
				PluginConfig.fakeCardPremium3.Value = premium;
				break;
			case 4:
				PluginConfig.fakeCardID4.Value = (rarityRandom ? cardsDbId[Random.Range(0, cardsDbId.Count)] : GetRandomCardID(rarity));
				PluginConfig.fakeCardPremium4.Value = premium;
				break;
			case 5:
				PluginConfig.fakeCardID5.Value = (rarityRandom ? cardsDbId[Random.Range(0, cardsDbId.Count)] : GetRandomCardID(rarity));
				PluginConfig.fakeCardPremium5.Value = premium;
				break;
			}
		}
	}

	public static void BuyAdventure(BuyAdventureTemplate adventure)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Invalid comparison between Unknown and I4
		if (adventure == BuyAdventureTemplate.DoNothing)
		{
			return;
		}
		if ((int)SceneMgr.Get().GetMode() == 1 || (int)SceneMgr.Get().GetMode() == 2)
		{
			UIStatus.Get().AddInfo("未初始化！");
			return;
		}
		if ((int)SceneMgr.Get().GetMode() == 4)
		{
			UIStatus.Get().AddInfo("不能在游戏内购买！");
			return;
		}
		try
		{
			int num = -1;
			ProductType val = (ProductType)0;
			switch (adventure)
			{
			default:
				return;
			case BuyAdventureTemplate.BuyKara:
				val = (ProductType)8;
				num = 16;
				break;
			case BuyAdventureTemplate.BuyNAX:
				val = (ProductType)3;
				num = 1;
				break;
			case BuyAdventureTemplate.BuyBRM:
				val = (ProductType)4;
				num = 6;
				break;
			case BuyAdventureTemplate.BuyLOE:
				val = (ProductType)7;
				num = 11;
				break;
			}
			if (adventure == BuyAdventureTemplate.BuyKara)
			{
				string text = default(string);
				for (int i = 16; i <= 20; i++)
				{
					num = i;
					if ((int)StoreManager.GetStaticProductItemOwnershipStatus(val, num, ref text) != 3)
					{
						break;
					}
				}
				if (num == 20)
				{
					num = 15;
				}
			}
			string text2 = default(string);
			if ((int)StoreManager.GetStaticProductItemOwnershipStatus(val, num, ref text2) == 3)
			{
				MyLogger((LogLevel)4, $"{adventure}：冒险已拥有！");
				UIStatus.Get().AddInfo("所选冒险已拥有！");
			}
			else
			{
				StoreManager.Get().StartAdventureTransaction(val, num, (ExitCallback)null, (object)null, (ShopType)2, 1, false, (IDataModel)null, 0);
			}
		}
		catch (Exception message)
		{
			MyLogger((LogLevel)4, message);
		}
	}

	public static void UpdateHeroPowerMapping()
	{
		PluginConfig.HeroesPowerMapping.Clear();
		foreach (KeyValuePair<int, int> item in PluginConfig.HeroesMapping)
		{
			string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(item.Key);
			string heroPowerCardIdFromHero2 = GameUtils.GetHeroPowerCardIdFromHero(item.Value);
			if (heroPowerCardIdFromHero != string.Empty && heroPowerCardIdFromHero2 != string.Empty && !PluginConfig.HeroesPowerMapping.ContainsKey(heroPowerCardIdFromHero))
			{
				PluginConfig.HeroesPowerMapping.Add(heroPowerCardIdFromHero, heroPowerCardIdFromHero2);
			}
		}
	}

	public static bool IsMercenaryFullyUpgraded(LettuceMercenary merc)
	{
		if (merc == null || !merc.m_owned || !merc.IsMaxLevel())
		{
			return false;
		}
		foreach (LettuceAbility ability in merc.m_abilityList)
		{
			if (ability.GetNextUpgradeCost() > 0)
			{
				return false;
			}
		}
		foreach (LettuceAbility equipment in merc.m_equipmentList)
		{
			if (equipment.GetNextUpgradeCost() > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static long CalcMercenaryCoinNeed(LettuceMercenary merc)
	{
		long num = 0L;
		if (!merc.m_owned)
		{
			num = merc.GetCraftingCost() - merc.m_currencyAmount;
			if (num <= 0)
			{
				return 4096L;
			}
			return num;
		}
		foreach (LettuceAbility ability in merc.m_abilityList)
		{
			if (ability.GetNextUpgradeCost() > 0)
			{
				switch (ability.GetNextTier())
				{
				case 2:
					num += 475;
					break;
				case 3:
					num += 425;
					break;
				case 4:
					num += 300;
					break;
				case 5:
					num += 150;
					break;
				case 6:
					num = num;
					break;
				}
			}
		}
		foreach (LettuceAbility equipment in merc.m_equipmentList)
		{
			if (equipment.GetNextUpgradeCost() > 0)
			{
				switch (equipment.GetNextTier())
				{
				case 2:
					num += 425;
					break;
				case 3:
					num += 325;
					break;
				case 4:
					num += 175;
					break;
				case 5:
					num = num;
					break;
				}
			}
		}
		num -= merc.m_currencyAmount;
		if (num <= 0)
		{
			return 8192L;
		}
		return num;
	}

	public static void DeleteFolder(string dir)
	{
		if (!Directory.Exists(dir))
		{
			return;
		}
		string[] fileSystemEntries = Directory.GetFileSystemEntries(dir);
		foreach (string text in fileSystemEntries)
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			else
			{
				DeleteFolder(text);
			}
		}
		Directory.Delete(dir, recursive: true);
	}
}
public class UtilsArgu
{
	private readonly Dictionary<string, Collection<string>> dictionary;

	private string argueKey;

	private static UtilsArgu _instance;

	public int Count => dictionary.Count;

	public static UtilsArgu Instance => _instance ?? (_instance = new UtilsArgu(Environment.GetCommandLineArgs()));

	public Collection<string> this[string parameter]
	{
		get
		{
			if (!dictionary.ContainsKey(parameter))
			{
				return null;
			}
			return dictionary[parameter];
		}
	}

	private static string RemoveQuotes(string argueValue)
	{
		int num = argueValue.IndexOf('"');
		int num2 = argueValue.LastIndexOf('"');
		while (num != num2)
		{
			argueValue = argueValue.Remove(num, 1);
			argueValue = argueValue.Remove(num2 - 1, 1);
			num = argueValue.IndexOf('"');
			num2 = argueValue.LastIndexOf('"');
		}
		return argueValue;
	}

	public UtilsArgu(IEnumerable<string> arguments)
	{
		dictionary = new Dictionary<string, Collection<string>>();
		Regex regex = new Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		foreach (string argument in arguments)
		{
			string[] array = regex.Split(argument, 3);
			switch (array.Length)
			{
			case 1:
				SaveArgueValue(array[0]);
				break;
			case 2:
				SaveArgueKey();
				argueKey = array[1];
				break;
			case 3:
			{
				SaveArgueKey();
				string text = RemoveQuotes(array[2]);
				SaveAllArgueValue(array[1], text.Split(',', StringSplitOptions.None));
				break;
			}
			}
		}
		SaveArgueKey();
	}

	private void SaveAllArgueValue(string key, IEnumerable<string> valueList)
	{
		foreach (string value in valueList)
		{
			AddDict(key, value);
		}
	}

	private void SaveArgueKey()
	{
		if (argueKey != null)
		{
			DictAddValue(argueKey, "true");
			argueKey = null;
		}
	}

	private void SaveArgueValue(string argueValue)
	{
		if (argueKey != null)
		{
			argueValue = RemoveQuotes(argueValue);
			AddDict(argueKey, argueValue);
			argueKey = null;
		}
	}

	private void AddDict(string key, string value)
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, new Collection<string>());
		}
		dictionary[key].Add(value);
	}

	private void DictAddValue(string key, string value)
	{
		if (dictionary.ContainsKey(key))
		{
			throw new ArgumentException("Argument " + key + " has already been defined");
		}
		dictionary.Add(key, new Collection<string>());
		dictionary[key].Add(value);
	}

	public bool IsTrue(string argueKey)
	{
		CheckSingle(argueKey);
		return this[argueKey]?[0].Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
	}

	private void CheckSingle(string argueKey)
	{
		if (this[argueKey] != null && this[argueKey].Count > 1)
		{
			throw new ArgumentException(argueKey + " has been specified more than once, expecting single value");
		}
	}

	public string Single(string argueKey)
	{
		CheckSingle(argueKey);
		if (this[argueKey] != null && !IsTrue(argueKey))
		{
			return this[argueKey][0];
		}
		return null;
	}

	public bool Exists(string argueKey)
	{
		if (this[argueKey] != null)
		{
			return this[argueKey].Count > 0;
		}
		return false;
	}
}
public class WebPage
{
	public const string jQuery = "/*! jQuery v1.12.4 | (c) jQuery Foundation | jquery.org/license */\r\n!function(a,b){\"object\"==typeof module&&\"object\"==typeof module.exports?module.exports=a.document?b(a,!0):function(a){if(!a.document)throw new Error(\"jQuery requires a window with a document\");return b(a)}:b(a)}(\"undefined\"!=typeof window?window:this,function(a,b){var c=[],d=a.document,e=c.slice,f=c.concat,g=c.push,h=c.indexOf,i={},j=i.toString,k=i.hasOwnProperty,l={},m=\"1.12.4\",n=function(a,b){return new n.fn.init(a,b)},o=/^[\\s\\uFEFF\\xA0]+|[\\s\\uFEFF\\xA0]+$/g,p=/^-ms-/,q=/-([\\da-z])/gi,r=function(a,b){return b.toUpperCase()};n.fn=n.prototype={jquery:m,constructor:n,selector:\"\",length:0,toArray:function(){return e.call(this)},get:function(a){return null!=a?0>a?this[a+this.length]:this[a]:e.call(this)},pushStack:function(a){var b=n.merge(this.constructor(),a);return b.prevObject=this,b.context=this.context,b},each:function(a){return n.each(this,a)},map:function(a){return this.pushStack(n.map(this,function(b,c){return a.call(b,c,b)}))},slice:function(){return this.pushStack(e.apply(this,arguments))},first:function(){return this.eq(0)},last:function(){return this.eq(-1)},eq:function(a){var b=this.length,c=+a+(0>a?b:0);return this.pushStack(c>=0&&b>c?[this[c]]:[])},end:function(){return this.prevObject||this.constructor()},push:g,sort:c.sort,splice:c.splice},n.extend=n.fn.extend=function(){var a,b,c,d,e,f,g=arguments[0]||{},h=1,i=arguments.length,j=!1;for(\"boolean\"==typeof g&&(j=g,g=arguments[h]||{},h++),\"object\"==typeof g||n.isFunction(g)||(g={}),h===i&&(g=this,h--);i>h;h++)if(null!=(e=arguments[h]))for(d in e)a=g[d],c=e[d],g!==c&&(j&&c&&(n.isPlainObject(c)||(b=n.isArray(c)))?(b?(b=!1,f=a&&n.isArray(a)?a:[]):f=a&&n.isPlainObject(a)?a:{},g[d]=n.extend(j,f,c)):void 0!==c&&(g[d]=c));return g},n.extend({expando:\"jQuery\"+(m+Math.random()).replace(/\\D/g,\"\"),isReady:!0,error:function(a){throw new Error(a)},noop:function(){},isFunction:function(a){return\"function\"===n.type(a)},isArray:Array.isArray||function(a){return\"array\"===n.type(a)},isWindow:function(a){return null!=a&&a==a.window},isNumeric:function(a){var b=a&&a.toString();return!n.isArray(a)&&b-parseFloat(b)+1>=0},isEmptyObject:function(a){var b;for(b in a)return!1;return!0},isPlainObject:function(a){var b;if(!a||\"object\"!==n.type(a)||a.nodeType||n.isWindow(a))return!1;try{if(a.constructor&&!k.call(a,\"constructor\")&&!k.call(a.constructor.prototype,\"isPrototypeOf\"))return!1}catch(c){return!1}if(!l.ownFirst)for(b in a)return k.call(a,b);for(b in a);return void 0===b||k.call(a,b)},type:function(a){return null==a?a+\"\":\"object\"==typeof a||\"function\"==typeof a?i[j.call(a)]||\"object\":typeof a},globalEval:function(b){b&&n.trim(b)&&(a.execScript||function(b){a.eval.call(a,b)})(b)},camelCase:function(a){return a.replace(p,\"ms-\").replace(q,r)},nodeName:function(a,b){return a.nodeName&&a.nodeName.toLowerCase()===b.toLowerCase()},each:function(a,b){var c,d=0;if(s(a)){for(c=a.length;c>d;d++)if(b.call(a[d],d,a[d])===!1)break}else for(d in a)if(b.call(a[d],d,a[d])===!1)break;return a},trim:function(a){return null==a?\"\":(a+\"\").replace(o,\"\")},makeArray:function(a,b){var c=b||[];return null!=a&&(s(Object(a))?n.merge(c,\"string\"==typeof a?[a]:a):g.call(c,a)),c},inArray:function(a,b,c){var d;if(b){if(h)return h.call(b,a,c);for(d=b.length,c=c?0>c?Math.max(0,d+c):c:0;d>c;c++)if(c in b&&b[c]===a)return c}return-1},merge:function(a,b){var c=+b.length,d=0,e=a.length;while(c>d)a[e++]=b[d++];if(c!==c)while(void 0!==b[d])a[e++]=b[d++];return a.length=e,a},grep:function(a,b,c){for(var d,e=[],f=0,g=a.length,h=!c;g>f;f++)d=!b(a[f],f),d!==h&&e.push(a[f]);return e},map:function(a,b,c){var d,e,g=0,h=[];if(s(a))for(d=a.length;d>g;g++)e=b(a[g],g,c),null!=e&&h.push(e);else for(g in a)e=b(a[g],g,c),null!=e&&h.push(e);return f.apply([],h)},guid:1,proxy:function(a,b){var c,d,f;return\"string\"==typeof b&&(f=a[b],b=a,a=f),n.isFunction(a)?(c=e.call(arguments,2),d=function(){return a.apply(b||this,c.concat(e.call(arguments)))},d.guid=a.guid=a.guid||n.guid++,d):void 0},now:function(){return+new Date},support:l}),\"function\"==typeof Symbol&&(n.fn[Symbol.iterator]=c[Symbol.iterator]),n.each(\"Boolean Number String Function Array Date RegExp Object Error Symbol\".split(\" \"),function(a,b){i[\"[object \"+b+\"]\"]=b.toLowerCase()});function s(a){var b=!!a&&\"length\"in a&&a.length,c=n.type(a);return\"function\"===c||n.isWindow(a)?!1:\"array\"===c||0===b||\"number\"==typeof b&&b>0&&b-1 in a}var t=function(a){var b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u=\"sizzle\"+1*new Date,v=a.document,w=0,x=0,y=ga(),z=ga(),A=ga(),B=function(a,b){return a===b&&(l=!0),0},C=1<<31,D={}.hasOwnProperty,E=[],F=E.pop,G=E.push,H=E.push,I=E.slice,J=function(a,b){for(var c=0,d=a.length;d>c;c++)if(a[c]===b)return c;return-1},K=\"checked|selected|async|autofocus|autoplay|controls|defer|disabled|hidden|ismap|loop|multiple|open|readonly|required|scoped\",L=\"[\\\\x20\\\\t\\\\r\\\\n\\\\f]\",M=\"(?:\\\\\\\\.|[\\\\w-]|[^\\\\x00-\\\\xa0])+\",N=\"\\\\[\"+L+\"*(\"+M+\")(?:\"+L+\"*([*^$|!~]?=)\"+L+\"*(?:'((?:\\\\\\\\.|[^\\\\\\\\'])*)'|\\\"((?:\\\\\\\\.|[^\\\\\\\\\\\"])*)\\\"|(\"+M+\"))|)\"+L+\"*\\\\]\",O=\":(\"+M+\")(?:\\\\((('((?:\\\\\\\\.|[^\\\\\\\\'])*)'|\\\"((?:\\\\\\\\.|[^\\\\\\\\\\\"])*)\\\")|((?:\\\\\\\\.|[^\\\\\\\\()[\\\\]]|\"+N+\")*)|.*)\\\\)|)\",P=new RegExp(L+\"+\",\"g\"),Q=new RegExp(\"^\"+L+\"+|((?:^|[^\\\\\\\\])(?:\\\\\\\\.)*)\"+L+\"+$\",\"g\"),R=new RegExp(\"^\"+L+\"*,\"+L+\"*\"),S=new RegExp(\"^\"+L+\"*([>+~]|\"+L+\")\"+L+\"*\"),T=new RegExp(\"=\"+L+\"*([^\\\\]'\\\"]*?)\"+L+\"*\\\\]\",\"g\"),U=new RegExp(O),V=new RegExp(\"^\"+M+\"$\"),W={ID:new RegExp(\"^#(\"+M+\")\"),CLASS:new RegExp(\"^\\\\.(\"+M+\")\"),TAG:new RegExp(\"^(\"+M+\"|[*])\"),ATTR:new RegExp(\"^\"+N),PSEUDO:new RegExp(\"^\"+O),CHILD:new RegExp(\"^:(only|first|last|nth|nth-last)-(child|of-type)(?:\\\\(\"+L+\"*(even|odd|(([+-]|)(\\\\d*)n|)\"+L+\"*(?:([+-]|)\"+L+\"*(\\\\d+)|))\"+L+\"*\\\\)|)\",\"i\"),bool:new RegExp(\"^(?:\"+K+\")$\",\"i\"),needsContext:new RegExp(\"^\"+L+\"*[>+~]|:(even|odd|eq|gt|lt|nth|first|last)(?:\\\\(\"+L+\"*((?:-\\\\d)?\\\\d*)\"+L+\"*\\\\)|)(?=[^-]|$)\",\"i\")},X=/^(?:input|select|textarea|button)$/i,Y=/^h\\d$/i,Z=/^[^{]+\\{\\s*\\[native \\w/,$=/^(?:#([\\w-]+)|(\\w+)|\\.([\\w-]+))$/,_=/[+~]/,aa=/'|\\\\/g,ba=new RegExp(\"\\\\\\\\([\\\\da-f]{1,6}\"+L+\"?|(\"+L+\")|.)\",\"ig\"),ca=function(a,b,c){var d=\"0x\"+b-65536;return d!==d||c?b:0>d?String.fromCharCode(d+65536):String.fromCharCode(d>>10|55296,1023&d|56320)},da=function(){m()};try{H.apply(E=I.call(v.childNodes),v.childNodes),E[v.childNodes.length].nodeType}catch(ea){H={apply:E.length?function(a,b){G.apply(a,I.call(b))}:function(a,b){var c=a.length,d=0;while(a[c++]=b[d++]);a.length=c-1}}}function fa(a,b,d,e){var f,h,j,k,l,o,r,s,w=b&&b.ownerDocument,x=b?b.nodeType:9;if(d=d||[],\"string\"!=typeof a||!a||1!==x&&9!==x&&11!==x)return d;if(!e&&((b?b.ownerDocument||b:v)!==n&&m(b),b=b||n,p)){if(11!==x&&(o=$.exec(a)))if(f=o[1]){if(9===x){if(!(j=b.getElementById(f)))return d;if(j.id===f)return d.push(j),d}else if(w&&(j=w.getElementById(f))&&t(b,j)&&j.id===f)return d.push(j),d}else{if(o[2])return H.apply(d,b.getElementsByTagName(a)),d;if((f=o[3])&&c.getElementsByClassName&&b.getElementsByClassName)return H.apply(d,b.getElementsByClassName(f)),d}if(c.qsa&&!A[a+\" \"]&&(!q||!q.test(a))){if(1!==x)w=b,s=a;else if(\"object\"!==b.nodeName.toLowerCase()){(k=b.getAttribute(\"id\"))?k=k.replace(aa,\"\\\\$&\"):b.setAttribute(\"id\",k=u),r=g(a),h=r.length,l=V.test(k)?\"#\"+k:\"[id='\"+k+\"']\";while(h--)r[h]=l+\" \"+qa(r[h]);s=r.join(\",\"),w=_.test(a)&&oa(b.parentNode)||b}if(s)try{return H.apply(d,w.querySelectorAll(s)),d}catch(y){}finally{k===u&&b.removeAttribute(\"id\")}}}return i(a.replace(Q,\"$1\"),b,d,e)}function ga(){var a=[];function b(c,e){return a.push(c+\" \")>d.cacheLength&&delete b[a.shift()],b[c+\" \"]=e}return b}function ha(a){return a[u]=!0,a}function ia(a){var b=n.createElement(\"div\");try{return!!a(b)}catch(c){return!1}finally{b.parentNode&&b.parentNode.removeChild(b),b=null}}function ja(a,b){var c=a.split(\"|\"),e=c.length;while(e--)d.attrHandle[c[e]]=b}function ka(a,b){var c=b&&a,d=c&&1===a.nodeType&&1===b.nodeType&&(~b.sourceIndex||C)-(~a.sourceIndex||C);if(d)return d;if(c)while(c=c.nextSibling)if(c===b)return-1;return a?1:-1}function la(a){return function(b){var c=b.nodeName.toLowerCase();return\"input\"===c&&b.type===a}}function ma(a){return function(b){var c=b.nodeName.toLowerCase();return(\"input\"===c||\"button\"===c)&&b.type===a}}function na(a){return ha(function(b){return b=+b,ha(function(c,d){var e,f=a([],c.length,b),g=f.length;while(g--)c[e=f[g]]&&(c[e]=!(d[e]=c[e]))})})}function oa(a){return a&&\"undefined\"!=typeof a.getElementsByTagName&&a}c=fa.support={},f=fa.isXML=function(a){var b=a&&(a.ownerDocument||a).documentElement;return b?\"HTML\"!==b.nodeName:!1},m=fa.setDocument=function(a){var b,e,g=a?a.ownerDocument||a:v;return g!==n&&9===g.nodeType&&g.documentElement?(n=g,o=n.documentElement,p=!f(n),(e=n.defaultView)&&e.top!==e&&(e.addEventListener?e.addEventListener(\"unload\",da,!1):e.attachEvent&&e.attachEvent(\"onunload\",da)),c.attributes=ia(function(a){return a.className=\"i\",!a.getAttribute(\"className\")}),c.getElementsByTagName=ia(function(a){return a.appendChild(n.createComment(\"\")),!a.getElementsByTagName(\"*\").length}),c.getElementsByClassName=Z.test(n.getElementsByClassName),c.getById=ia(function(a){return o.appendChild(a).id=u,!n.getElementsByName||!n.getElementsByName(u).length}),c.getById?(d.find.ID=function(a,b){if(\"undefined\"!=typeof b.getElementById&&p){var c=b.getElementById(a);return c?[c]:[]}},d.filter.ID=function(a){var b=a.replace(ba,ca);return function(a){return a.getAttribute(\"id\")===b}}):(delete d.find.ID,d.filter.ID=function(a){var b=a.replace(ba,ca);return function(a){var c=\"undefined\"!=typeof a.getAttributeNode&&a.getAttributeNode(\"id\");return c&&c.value===b}}),d.find.TAG=c.getElementsByTagName?function(a,b){return\"undefined\"!=typeof b.getElementsByTagName?b.getElementsByTagName(a):c.qsa?b.querySelectorAll(a):void 0}:function(a,b){var c,d=[],e=0,f=b.getElementsByTagName(a);if(\"*\"===a){while(c=f[e++])1===c.nodeType&&d.push(c);return d}return f},d.find.CLASS=c.getElementsByClassName&&function(a,b){return\"undefined\"!=typeof b.getElementsByClassName&&p?b.getElementsByClassName(a):void 0},r=[],q=[],(c.qsa=Z.test(n.querySelectorAll))&&(ia(function(a){o.appendChild(a).innerHTML=\"<a id='\"+u+\"'></a><select id='\"+u+\"-\\r\\\\' msallowcapture=''><option selected=''></option></select>\",a.querySelectorAll(\"[msallowcapture^='']\").length&&q.push(\"[*^$]=\"+L+\"*(?:''|\\\"\\\")\"),a.querySelectorAll(\"[selected]\").length||q.push(\"\\\\[\"+L+\"*(?:value|\"+K+\")\"),a.querySelectorAll(\"[id~=\"+u+\"-]\").length||q.push(\"~=\"),a.querySelectorAll(\":checked\").length||q.push(\":checked\"),a.querySelectorAll(\"a#\"+u+\"+*\").length||q.push(\".#.+[+~]\")}),ia(function(a){var b=n.createElement(\"input\");b.setAttribute(\"type\",\"hidden\"),a.appendChild(b).setAttribute(\"name\",\"D\"),a.querySelectorAll(\"[name=d]\").length&&q.push(\"name\"+L+\"*[*^$|!~]?=\"),a.querySelectorAll(\":enabled\").length||q.push(\":enabled\",\":disabled\"),a.querySelectorAll(\"*,:x\"),q.push(\",.*:\")})),(c.matchesSelector=Z.test(s=o.matches||o.webkitMatchesSelector||o.mozMatchesSelector||o.oMatchesSelector||o.msMatchesSelector))&&ia(function(a){c.disconnectedMatch=s.call(a,\"div\"),s.call(a,\"[s!='']:x\"),r.push(\"!=\",O)}),q=q.length&&new RegExp(q.join(\"|\")),r=r.length&&new RegExp(r.join(\"|\")),b=Z.test(o.compareDocumentPosition),t=b||Z.test(o.contains)?function(a,b){var c=9===a.nodeType?a.documentElement:a,d=b&&b.parentNode;return a===d||!(!d||1!==d.nodeType||!(c.contains?c.contains(d):a.compareDocumentPosition&&16&a.compareDocumentPosition(d)))}:function(a,b){if(b)while(b=b.parentNode)if(b===a)return!0;return!1},B=b?function(a,b){if(a===b)return l=!0,0;var d=!a.compareDocumentPosition-!b.compareDocumentPosition;return d?d:(d=(a.ownerDocument||a)===(b.ownerDocument||b)?a.compareDocumentPosition(b):1,1&d||!c.sortDetached&&b.compareDocumentPosition(a)===d?a===n||a.ownerDocument===v&&t(v,a)?-1:b===n||b.ownerDocument===v&&t(v,b)?1:k?J(k,a)-J(k,b):0:4&d?-1:1)}:function(a,b){if(a===b)return l=!0,0;var c,d=0,e=a.parentNode,f=b.parentNode,g=[a],h=[b];if(!e||!f)return a===n?-1:b===n?1:e?-1:f?1:k?J(k,a)-J(k,b):0;if(e===f)return ka(a,b);c=a;while(c=c.parentNode)g.unshift(c);c=b;while(c=c.parentNode)h.unshift(c);while(g[d]===h[d])d++;return d?ka(g[d],h[d]):g[d]===v?-1:h[d]===v?1:0},n):n},fa.matches=function(a,b){return fa(a,null,null,b)},fa.matchesSelector=function(a,b){if((a.ownerDocument||a)!==n&&m(a),b=b.replace(T,\"='$1']\"),c.matchesSelector&&p&&!A[b+\" \"]&&(!r||!r.test(b))&&(!q||!q.test(b)))try{var d=s.call(a,b);if(d||c.disconnectedMatch||a.document&&11!==a.document.nodeType)return d}catch(e){}return fa(b,n,null,[a]).length>0},fa.contains=function(a,b){return(a.ownerDocument||a)!==n&&m(a),t(a,b)},fa.attr=function(a,b){(a.ownerDocument||a)!==n&&m(a);var e=d.attrHandle[b.toLowerCase()],f=e&&D.call(d.attrHandle,b.toLowerCase())?e(a,b,!p):void 0;return void 0!==f?f:c.attributes||!p?a.getAttribute(b):(f=a.getAttributeNode(b))&&f.specified?f.value:null},fa.error=function(a){throw new Error(\"Syntax error, unrecognized expression: \"+a)},fa.uniqueSort=function(a){var b,d=[],e=0,f=0;if(l=!c.detectDuplicates,k=!c.sortStable&&a.slice(0),a.sort(B),l){while(b=a[f++])b===a[f]&&(e=d.push(f));while(e--)a.splice(d[e],1)}return k=null,a},e=fa.getText=function(a){var b,c=\"\",d=0,f=a.nodeType;if(f){if(1===f||9===f||11===f){if(\"string\"==typeof a.textContent)return a.textContent;for(a=a.firstChild;a;a=a.nextSibling)c+=e(a)}else if(3===f||4===f)return a.nodeValue}else while(b=a[d++])c+=e(b);return c},d=fa.selectors={cacheLength:50,createPseudo:ha,match:W,attrHandle:{},find:{},relative:{\">\":{dir:\"parentNode\",first:!0},\" \":{dir:\"parentNode\"},\"+\":{dir:\"previousSibling\",first:!0},\"~\":{dir:\"previousSibling\"}},preFilter:{ATTR:function(a){return a[1]=a[1].replace(ba,ca),a[3]=(a[3]||a[4]||a[5]||\"\").replace(ba,ca),\"~=\"===a[2]&&(a[3]=\" \"+a[3]+\" \"),a.slice(0,4)},CHILD:function(a){return a[1]=a[1].toLowerCase(),\"nth\"===a[1].slice(0,3)?(a[3]||fa.error(a[0]),a[4]=+(a[4]?a[5]+(a[6]||1):2*(\"even\"===a[3]||\"odd\"===a[3])),a[5]=+(a[7]+a[8]||\"odd\"===a[3])):a[3]&&fa.error(a[0]),a},PSEUDO:function(a){var b,c=!a[6]&&a[2];return W.CHILD.test(a[0])?null:(a[3]?a[2]=a[4]||a[5]||\"\":c&&U.test(c)&&(b=g(c,!0))&&(b=c.indexOf(\")\",c.length-b)-c.length)&&(a[0]=a[0].slice(0,b),a[2]=c.slice(0,b)),a.slice(0,3))}},filter:{TAG:function(a){var b=a.replace(ba,ca).toLowerCase();return\"*\"===a?function(){return!0}:function(a){return a.nodeName&&a.nodeName.toLowerCase()===b}},CLASS:function(a){var b=y[a+\" \"];return b||(b=new RegExp(\"(^|\"+L+\")\"+a+\"(\"+L+\"|$)\"))&&y(a,function(a){return b.test(\"string\"==typeof a.className&&a.className||\"undefined\"!=typeof a.getAttribute&&a.getAttribute(\"class\")||\"\")})},ATTR:function(a,b,c){return function(d){var e=fa.attr(d,a);return null==e?\"!=\"===b:b?(e+=\"\",\"=\"===b?e===c:\"!=\"===b?e!==c:\"^=\"===b?c&&0===e.indexOf(c):\"*=\"===b?c&&e.indexOf(c)>-1:\"$=\"===b?c&&e.slice(-c.length)===c:\"~=\"===b?(\" \"+e.replace(P,\" \")+\" \").indexOf(c)>-1:\"|=\"===b?e===c||e.slice(0,c.length+1)===c+\"-\":!1):!0}},CHILD:function(a,b,c,d,e){var f=\"nth\"!==a.slice(0,3),g=\"last\"!==a.slice(-4),h=\"of-type\"===b;return 1===d&&0===e?function(a){return!!a.parentNode}:function(b,c,i){var j,k,l,m,n,o,p=f!==g?\"nextSibling\":\"previousSibling\",q=b.parentNode,r=h&&b.nodeName.toLowerCase(),s=!i&&!h,t=!1;if(q){if(f){while(p){m=b;while(m=m[p])if(h?m.nodeName.toLowerCase()===r:1===m.nodeType)return!1;o=p=\"only\"===a&&!o&&\"nextSibling\"}return!0}if(o=[g?q.firstChild:q.lastChild],g&&s){m=q,l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),j=k[a]||[],n=j[0]===w&&j[1],t=n&&j[2],m=n&&q.childNodes[n];while(m=++n&&m&&m[p]||(t=n=0)||o.pop())if(1===m.nodeType&&++t&&m===b){k[a]=[w,n,t];break}}else if(s&&(m=b,l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),j=k[a]||[],n=j[0]===w&&j[1],t=n),t===!1)while(m=++n&&m&&m[p]||(t=n=0)||o.pop())if((h?m.nodeName.toLowerCase()===r:1===m.nodeType)&&++t&&(s&&(l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),k[a]=[w,t]),m===b))break;return t-=e,t===d||t%d===0&&t/d>=0}}},PSEUDO:function(a,b){var c,e=d.pseudos[a]||d.setFilters[a.toLowerCase()]||fa.error(\"unsupported pseudo: \"+a);return e[u]?e(b):e.length>1?(c=[a,a,\"\",b],d.setFilters.hasOwnProperty(a.toLowerCase())?ha(function(a,c){var d,f=e(a,b),g=f.length;while(g--)d=J(a,f[g]),a[d]=!(c[d]=f[g])}):function(a){return e(a,0,c)}):e}},pseudos:{not:ha(function(a){var b=[],c=[],d=h(a.replace(Q,\"$1\"));return d[u]?ha(function(a,b,c,e){var f,g=d(a,null,e,[]),h=a.length;while(h--)(f=g[h])&&(a[h]=!(b[h]=f))}):function(a,e,f){return b[0]=a,d(b,null,f,c),b[0]=null,!c.pop()}}),has:ha(function(a){return function(b){return fa(a,b).length>0}}),contains:ha(function(a){return a=a.replace(ba,ca),function(b){return(b.textContent||b.innerText||e(b)).indexOf(a)>-1}}),lang:ha(function(a){return V.test(a||\"\")||fa.error(\"unsupported lang: \"+a),a=a.replace(ba,ca).toLowerCase(),function(b){var c;do if(c=p?b.lang:b.getAttribute(\"xml:lang\")||b.getAttribute(\"lang\"))return c=c.toLowerCase(),c===a||0===c.indexOf(a+\"-\");while((b=b.parentNode)&&1===b.nodeType);return!1}}),target:function(b){var c=a.location&&a.location.hash;return c&&c.slice(1)===b.id},root:function(a){return a===o},focus:function(a){return a===n.activeElement&&(!n.hasFocus||n.hasFocus())&&!!(a.type||a.href||~a.tabIndex)},enabled:function(a){return a.disabled===!1},disabled:function(a){return a.disabled===!0},checked:function(a){var b=a.nodeName.toLowerCase();return\"input\"===b&&!!a.checked||\"option\"===b&&!!a.selected},selected:function(a){return a.parentNode&&a.parentNode.selectedIndex,a.selected===!0},empty:function(a){for(a=a.firstChild;a;a=a.nextSibling)if(a.nodeType<6)return!1;return!0},parent:function(a){return!d.pseudos.empty(a)},header:function(a){return Y.test(a.nodeName)},input:function(a){return X.test(a.nodeName)},button:function(a){var b=a.nodeName.toLowerCase();return\"input\"===b&&\"button\"===a.type||\"button\"===b},text:function(a){var b;return\"input\"===a.nodeName.toLowerCase()&&\"text\"===a.type&&(null==(b=a.getAttribute(\"type\"))||\"text\"===b.toLowerCase())},first:na(function(){return[0]}),last:na(function(a,b){return[b-1]}),eq:na(function(a,b,c){return[0>c?c+b:c]}),even:na(function(a,b){for(var c=0;b>c;c+=2)a.push(c);return a}),odd:na(function(a,b){for(var c=1;b>c;c+=2)a.push(c);return a}),lt:na(function(a,b,c){for(var d=0>c?c+b:c;--d>=0;)a.push(d);return a}),gt:na(function(a,b,c){for(var d=0>c?c+b:c;++d<b;)a.push(d);return a})}},d.pseudos.nth=d.pseudos.eq;for(b in{radio:!0,checkbox:!0,file:!0,password:!0,image:!0})d.pseudos[b]=la(b);for(b in{submit:!0,reset:!0})d.pseudos[b]=ma(b);function pa(){}pa.prototype=d.filters=d.pseudos,d.setFilters=new pa,g=fa.tokenize=function(a,b){var c,e,f,g,h,i,j,k=z[a+\" \"];if(k)return b?0:k.slice(0);h=a,i=[],j=d.preFilter;while(h){c&&!(e=R.exec(h))||(e&&(h=h.slice(e[0].length)||h),i.push(f=[])),c=!1,(e=S.exec(h))&&(c=e.shift(),f.push({value:c,type:e[0].replace(Q,\" \")}),h=h.slice(c.length));for(g in d.filter)!(e=W[g].exec(h))||j[g]&&!(e=j[g](e))||(c=e.shift(),f.push({value:c,type:g,matches:e}),h=h.slice(c.length));if(!c)break}return b?h.length:h?fa.error(a):z(a,i).slice(0)};function qa(a){for(var b=0,c=a.length,d=\"\";c>b;b++)d+=a[b].value;return d}function ra(a,b,c){var d=b.dir,e=c&&\"parentNode\"===d,f=x++;return b.first?function(b,c,f){while(b=b[d])if(1===b.nodeType||e)return a(b,c,f)}:function(b,c,g){var h,i,j,k=[w,f];if(g){while(b=b[d])if((1===b.nodeType||e)&&a(b,c,g))return!0}else while(b=b[d])if(1===b.nodeType||e){if(j=b[u]||(b[u]={}),i=j[b.uniqueID]||(j[b.uniqueID]={}),(h=i[d])&&h[0]===w&&h[1]===f)return k[2]=h[2];if(i[d]=k,k[2]=a(b,c,g))return!0}}}function sa(a){return a.length>1?function(b,c,d){var e=a.length;while(e--)if(!a[e](b,c,d))return!1;return!0}:a[0]}function ta(a,b,c){for(var d=0,e=b.length;e>d;d++)fa(a,b[d],c);return c}function ua(a,b,c,d,e){for(var f,g=[],h=0,i=a.length,j=null!=b;i>h;h++)(f=a[h])&&(c&&!c(f,d,e)||(g.push(f),j&&b.push(h)));return g}function va(a,b,c,d,e,f){return d&&!d[u]&&(d=va(d)),e&&!e[u]&&(e=va(e,f)),ha(function(f,g,h,i){var j,k,l,m=[],n=[],o=g.length,p=f||ta(b||\"*\",h.nodeType?[h]:h,[]),q=!a||!f&&b?p:ua(p,m,a,h,i),r=c?e||(f?a:o||d)?[]:g:q;if(c&&c(q,r,h,i),d){j=ua(r,n),d(j,[],h,i),k=j.length;while(k--)(l=j[k])&&(r[n[k]]=!(q[n[k]]=l))}if(f){if(e||a){if(e){j=[],k=r.length;while(k--)(l=r[k])&&j.push(q[k]=l);e(null,r=[],j,i)}k=r.length;while(k--)(l=r[k])&&(j=e?J(f,l):m[k])>-1&&(f[j]=!(g[j]=l))}}else r=ua(r===g?r.splice(o,r.length):r),e?e(null,g,r,i):H.apply(g,r)})}function wa(a){for(var b,c,e,f=a.length,g=d.relative[a[0].type],h=g||d.relative[\" \"],i=g?1:0,k=ra(function(a){return a===b},h,!0),l=ra(function(a){return J(b,a)>-1},h,!0),m=[function(a,c,d){var e=!g&&(d||c!==j)||((b=c).nodeType?k(a,c,d):l(a,c,d));return b=null,e}];f>i;i++)if(c=d.relative[a[i].type])m=[ra(sa(m),c)];else{if(c=d.filter[a[i].type].apply(null,a[i].matches),c[u]){for(e=++i;f>e;e++)if(d.relative[a[e].type])break;return va(i>1&&sa(m),i>1&&qa(a.slice(0,i-1).concat({value:\" \"===a[i-2].type?\"*\":\"\"})).replace(Q,\"$1\"),c,e>i&&wa(a.slice(i,e)),f>e&&wa(a=a.slice(e)),f>e&&qa(a))}m.push(c)}return sa(m)}function xa(a,b){var c=b.length>0,e=a.length>0,f=function(f,g,h,i,k){var l,o,q,r=0,s=\"0\",t=f&&[],u=[],v=j,x=f||e&&d.find.TAG(\"*\",k),y=w+=null==v?1:Math.random()||.1,z=x.length;for(k&&(j=g===n||g||k);s!==z&&null!=(l=x[s]);s++){if(e&&l){o=0,g||l.ownerDocument===n||(m(l),h=!p);while(q=a[o++])if(q(l,g||n,h)){i.push(l);break}k&&(w=y)}c&&((l=!q&&l)&&r--,f&&t.push(l))}if(r+=s,c&&s!==r){o=0;while(q=b[o++])q(t,u,g,h);if(f){if(r>0)while(s--)t[s]||u[s]||(u[s]=F.call(i));u=ua(u)}H.apply(i,u),k&&!f&&u.length>0&&r+b.length>1&&fa.uniqueSort(i)}return k&&(w=y,j=v),t};return c?ha(f):f}return h=fa.compile=function(a,b){var c,d=[],e=[],f=A[a+\" \"];if(!f){b||(b=g(a)),c=b.length;while(c--)f=wa(b[c]),f[u]?d.push(f):e.push(f);f=A(a,xa(e,d)),f.selector=a}return f},i=fa.select=function(a,b,e,f){var i,j,k,l,m,n=\"function\"==typeof a&&a,o=!f&&g(a=n.selector||a);if(e=e||[],1===o.length){if(j=o[0]=o[0].slice(0),j.length>2&&\"ID\"===(k=j[0]).type&&c.getById&&9===b.nodeType&&p&&d.relative[j[1].type]){if(b=(d.find.ID(k.matches[0].replace(ba,ca),b)||[])[0],!b)return e;n&&(b=b.parentNode),a=a.slice(j.shift().value.length)}i=W.needsContext.test(a)?0:j.length;while(i--){if(k=j[i],d.relative[l=k.type])break;if((m=d.find[l])&&(f=m(k.matches[0].replace(ba,ca),_.test(j[0].type)&&oa(b.parentNode)||b))){if(j.splice(i,1),a=f.length&&qa(j),!a)return H.apply(e,f),e;break}}}return(n||h(a,o))(f,b,!p,e,!b||_.test(a)&&oa(b.parentNode)||b),e},c.sortStable=u.split(\"\").sort(B).join(\"\")===u,c.detectDuplicates=!!l,m(),c.sortDetached=ia(function(a){return 1&a.compareDocumentPosition(n.createElement(\"div\"))}),ia(function(a){return a.innerHTML=\"<a href='#'></a>\",\"#\"===a.firstChild.getAttribute(\"href\")})||ja(\"type|href|height|width\",function(a,b,c){return c?void 0:a.getAttribute(b,\"type\"===b.toLowerCase()?1:2)}),c.attributes&&ia(function(a){return a.innerHTML=\"<input/>\",a.firstChild.setAttribute(\"value\",\"\"),\"\"===a.firstChild.getAttribute(\"value\")})||ja(\"value\",function(a,b,c){return c||\"input\"!==a.nodeName.toLowerCase()?void 0:a.defaultValue}),ia(function(a){return null==a.getAttribute(\"disabled\")})||ja(K,function(a,b,c){var d;return c?void 0:a[b]===!0?b.toLowerCase():(d=a.getAttributeNode(b))&&d.specified?d.value:null}),fa}(a);n.find=t,n.expr=t.selectors,n.expr[\":\"]=n.expr.pseudos,n.uniqueSort=n.unique=t.uniqueSort,n.text=t.getText,n.isXMLDoc=t.isXML,n.contains=t.contains;var u=function(a,b,c){var d=[],e=void 0!==c;while((a=a[b])&&9!==a.nodeType)if(1===a.nodeType){if(e&&n(a).is(c))break;d.push(a)}return d},v=function(a,b){for(var c=[];a;a=a.nextSibling)1===a.nodeType&&a!==b&&c.push(a);return c},w=n.expr.match.needsContext,x=/^<([\\w-]+)\\s*\\/?>(?:<\\/\\1>|)$/,y=/^.[^:#\\[\\.,]*$/;function z(a,b,c){if(n.isFunction(b))return n.grep(a,function(a,d){return!!b.call(a,d,a)!==c});if(b.nodeType)return n.grep(a,function(a){return a===b!==c});if(\"string\"==typeof b){if(y.test(b))return n.filter(b,a,c);b=n.filter(b,a)}return n.grep(a,function(a){return n.inArray(a,b)>-1!==c})}n.filter=function(a,b,c){var d=b[0];return c&&(a=\":not(\"+a+\")\"),1===b.length&&1===d.nodeType?n.find.matchesSelector(d,a)?[d]:[]:n.find.matches(a,n.grep(b,function(a){return 1===a.nodeType}))},n.fn.extend({find:function(a){var b,c=[],d=this,e=d.length;if(\"string\"!=typeof a)return this.pushStack(n(a).filter(function(){for(b=0;e>b;b++)if(n.contains(d[b],this))return!0}));for(b=0;e>b;b++)n.find(a,d[b],c);return c=this.pushStack(e>1?n.unique(c):c),c.selector=this.selector?this.selector+\" \"+a:a,c},filter:function(a){return this.pushStack(z(this,a||[],!1))},not:function(a){return this.pushStack(z(this,a||[],!0))},is:function(a){return!!z(this,\"string\"==typeof a&&w.test(a)?n(a):a||[],!1).length}});var A,B=/^(?:\\s*(<[\\w\\W]+>)[^>]*|#([\\w-]*))$/,C=n.fn.init=function(a,b,c){var e,f;if(!a)return this;if(c=c||A,\"string\"==typeof a){if(e=\"<\"===a.charAt(0)&&\">\"===a.charAt(a.length-1)&&a.length>=3?[null,a,null]:B.exec(a),!e||!e[1]&&b)return!b||b.jquery?(b||c).find(a):this.constructor(b).find(a);if(e[1]){if(b=b instanceof n?b[0]:b,n.merge(this,n.parseHTML(e[1],b&&b.nodeType?b.ownerDocument||b:d,!0)),x.test(e[1])&&n.isPlainObject(b))for(e in b)n.isFunction(this[e])?this[e](b[e]):this.attr(e,b[e]);return this}if(f=d.getElementById(e[2]),f&&f.parentNode){if(f.id!==e[2])return A.find(a);this.length=1,this[0]=f}return this.context=d,this.selector=a,this}return a.nodeType?(this.context=this[0]=a,this.length=1,this):n.isFunction(a)?\"undefined\"!=typeof c.ready?c.ready(a):a(n):(void 0!==a.selector&&(this.selector=a.selector,this.context=a.context),n.makeArray(a,this))};C.prototype=n.fn,A=n(d);var D=/^(?:parents|prev(?:Until|All))/,E={children:!0,contents:!0,next:!0,prev:!0};n.fn.extend({has:function(a){var b,c=n(a,this),d=c.length;return this.filter(function(){for(b=0;d>b;b++)if(n.contains(this,c[b]))return!0})},closest:function(a,b){for(var c,d=0,e=this.length,f=[],g=w.test(a)||\"string\"!=typeof a?n(a,b||this.context):0;e>d;d++)for(c=this[d];c&&c!==b;c=c.parentNode)if(c.nodeType<11&&(g?g.index(c)>-1:1===c.nodeType&&n.find.matchesSelector(c,a))){f.push(c);break}return this.pushStack(f.length>1?n.uniqueSort(f):f)},index:function(a){return a?\"string\"==typeof a?n.inArray(this[0],n(a)):n.inArray(a.jquery?a[0]:a,this):this[0]&&this[0].parentNode?this.first().prevAll().length:-1},add:function(a,b){return this.pushStack(n.uniqueSort(n.merge(this.get(),n(a,b))))},addBack:function(a){return this.add(null==a?this.prevObject:this.prevObject.filter(a))}});function F(a,b){do a=a[b];while(a&&1!==a.nodeType);return a}n.each({parent:function(a){var b=a.parentNode;return b&&11!==b.nodeType?b:null},parents:function(a){return u(a,\"parentNode\")},parentsUntil:function(a,b,c){return u(a,\"parentNode\",c)},next:function(a){return F(a,\"nextSibling\")},prev:function(a){return F(a,\"previousSibling\")},nextAll:function(a){return u(a,\"nextSibling\")},prevAll:function(a){return u(a,\"previousSibling\")},nextUntil:function(a,b,c){return u(a,\"nextSibling\",c)},prevUntil:function(a,b,c){return u(a,\"previousSibling\",c)},siblings:function(a){return v((a.parentNode||{}).firstChild,a)},children:function(a){return v(a.firstChild)},contents:function(a){return n.nodeName(a,\"iframe\")?a.contentDocument||a.contentWindow.document:n.merge([],a.childNodes)}},function(a,b){n.fn[a]=function(c,d){var e=n.map(this,b,c);return\"Until\"!==a.slice(-5)&&(d=c),d&&\"string\"==typeof d&&(e=n.filter(d,e)),this.length>1&&(E[a]||(e=n.uniqueSort(e)),D.test(a)&&(e=e.reverse())),this.pushStack(e)}});var G=/\\S+/g;function H(a){var b={};return n.each(a.match(G)||[],function(a,c){b[c]=!0}),b}n.Callbacks=function(a){a=\"string\"==typeof a?H(a):n.extend({},a);var b,c,d,e,f=[],g=[],h=-1,i=function(){for(e=a.once,d=b=!0;g.length;h=-1){c=g.shift();while(++h<f.length)f[h].apply(c[0],c[1])===!1&&a.stopOnFalse&&(h=f.length,c=!1)}a.memory||(c=!1),b=!1,e&&(f=c?[]:\"\")},j={add:function(){return f&&(c&&!b&&(h=f.length-1,g.push(c)),function d(b){n.each(b,function(b,c){n.isFunction(c)?a.unique&&j.has(c)||f.push(c):c&&c.length&&\"string\"!==n.type(c)&&d(c)})}(arguments),c&&!b&&i()),this},remove:function(){return n.each(arguments,function(a,b){var c;while((c=n.inArray(b,f,c))>-1)f.splice(c,1),h>=c&&h--}),this},has:function(a){return a?n.inArray(a,f)>-1:f.length>0},empty:function(){return f&&(f=[]),this},disable:function(){return e=g=[],f=c=\"\",this},disabled:function(){return!f},lock:function(){return e=!0,c||j.disable(),this},locked:function(){return!!e},fireWith:function(a,c){return e||(c=c||[],c=[a,c.slice?c.slice():c],g.push(c),b||i()),this},fire:function(){return j.fireWith(this,arguments),this},fired:function(){return!!d}};return j},n.extend({Deferred:function(a){var b=[[\"resolve\",\"done\",n.Callbacks(\"once memory\"),\"resolved\"],[\"reject\",\"fail\",n.Callbacks(\"once memory\"),\"rejected\"],[\"notify\",\"progress\",n.Callbacks(\"memory\")]],c=\"pending\",d={state:function(){return c},always:function(){return e.done(arguments).fail(arguments),this},then:function(){var a=arguments;return n.Deferred(function(c){n.each(b,function(b,f){var g=n.isFunction(a[b])&&a[b];e[f[1]](function(){var a=g&&g.apply(this,arguments);a&&n.isFunction(a.promise)?a.promise().progress(c.notify).done(c.resolve).fail(c.reject):c[f[0]+\"With\"](this===d?c.promise():this,g?[a]:arguments)})}),a=null}).promise()},promise:function(a){return null!=a?n.extend(a,d):d}},e={};return d.pipe=d.then,n.each(b,function(a,f){var g=f[2],h=f[3];d[f[1]]=g.add,h&&g.add(function(){c=h},b[1^a][2].disable,b[2][2].lock),e[f[0]]=function(){return e[f[0]+\"With\"](this===e?d:this,arguments),this},e[f[0]+\"With\"]=g.fireWith}),d.promise(e),a&&a.call(e,e),e},when:function(a){var b=0,c=e.call(arguments),d=c.length,f=1!==d||a&&n.isFunction(a.promise)?d:0,g=1===f?a:n.Deferred(),h=function(a,b,c){return function(d){b[a]=this,c[a]=arguments.length>1?e.call(arguments):d,c===i?g.notifyWith(b,c):--f||g.resolveWith(b,c)}},i,j,k;if(d>1)for(i=new Array(d),j=new Array(d),k=new Array(d);d>b;b++)c[b]&&n.isFunction(c[b].promise)?c[b].promise().progress(h(b,j,i)).done(h(b,k,c)).fail(g.reject):--f;return f||g.resolveWith(k,c),g.promise()}});var I;n.fn.ready=function(a){return n.ready.promise().done(a),this},n.extend({isReady:!1,readyWait:1,holdReady:function(a){a?n.readyWait++:n.ready(!0)},ready:function(a){(a===!0?--n.readyWait:n.isReady)||(n.isReady=!0,a!==!0&&--n.readyWait>0||(I.resolveWith(d,[n]),n.fn.triggerHandler&&(n(d).triggerHandler(\"ready\"),n(d).off(\"ready\"))))}});function J(){d.addEventListener?(d.removeEventListener(\"DOMContentLoaded\",K),a.removeEventListener(\"load\",K)):(d.detachEvent(\"onreadystatechange\",K),a.detachEvent(\"onload\",K))}function K(){(d.addEventListener||\"load\"===a.event.type||\"complete\"===d.readyState)&&(J(),n.ready())}n.ready.promise=function(b){if(!I)if(I=n.Deferred(),\"complete\"===d.readyState||\"loading\"!==d.readyState&&!d.documentElement.doScroll)a.setTimeout(n.ready);else if(d.addEventListener)d.addEventListener(\"DOMContentLoaded\",K),a.addEventListener(\"load\",K);else{d.attachEvent(\"onreadystatechange\",K),a.attachEvent(\"onload\",K);var c=!1;try{c=null==a.frameElement&&d.documentElement}catch(e){}c&&c.doScroll&&!function f(){if(!n.isReady){try{c.doScroll(\"left\")}catch(b){return a.setTimeout(f,50)}J(),n.ready()}}()}return I.promise(b)},n.ready.promise();var L;for(L in n(l))break;l.ownFirst=\"0\"===L,l.inlineBlockNeedsLayout=!1,n(function(){var a,b,c,e;c=d.getElementsByTagName(\"body\")[0],c&&c.style&&(b=d.createElement(\"div\"),e=d.createElement(\"div\"),e.style.cssText=\"position:absolute;border:0;width:0;height:0;top:0;left:-9999px\",c.appendChild(e).appendChild(b),\"undefined\"!=typeof b.style.zoom&&(b.style.cssText=\"display:inline;margin:0;border:0;padding:1px;width:1px;zoom:1\",l.inlineBlockNeedsLayout=a=3===b.offsetWidth,a&&(c.style.zoom=1)),c.removeChild(e))}),function(){var a=d.createElement(\"div\");l.deleteExpando=!0;try{delete a.test}catch(b){l.deleteExpando=!1}a=null}();var M=function(a){var b=n.noData[(a.nodeName+\" \").toLowerCase()],c=+a.nodeType||1;return 1!==c&&9!==c?!1:!b||b!==!0&&a.getAttribute(\"classid\")===b},N=/^(?:\\{[\\w\\W]*\\}|\\[[\\w\\W]*\\])$/,O=/([A-Z])/g;function P(a,b,c){if(void 0===c&&1===a.nodeType){var d=\"data-\"+b.replace(O,\"-$1\").toLowerCase();if(c=a.getAttribute(d),\"string\"==typeof c){try{c=\"true\"===c?!0:\"false\"===c?!1:\"null\"===c?null:+c+\"\"===c?+c:N.test(c)?n.parseJSON(c):c}catch(e){}n.data(a,b,c)}else c=void 0;\r\n}return c}function Q(a){var b;for(b in a)if((\"data\"!==b||!n.isEmptyObject(a[b]))&&\"toJSON\"!==b)return!1;return!0}function R(a,b,d,e){if(M(a)){var f,g,h=n.expando,i=a.nodeType,j=i?n.cache:a,k=i?a[h]:a[h]&&h;if(k&&j[k]&&(e||j[k].data)||void 0!==d||\"string\"!=typeof b)return k||(k=i?a[h]=c.pop()||n.guid++:h),j[k]||(j[k]=i?{}:{toJSON:n.noop}),\"object\"!=typeof b&&\"function\"!=typeof b||(e?j[k]=n.extend(j[k],b):j[k].data=n.extend(j[k].data,b)),g=j[k],e||(g.data||(g.data={}),g=g.data),void 0!==d&&(g[n.camelCase(b)]=d),\"string\"==typeof b?(f=g[b],null==f&&(f=g[n.camelCase(b)])):f=g,f}}function S(a,b,c){if(M(a)){var d,e,f=a.nodeType,g=f?n.cache:a,h=f?a[n.expando]:n.expando;if(g[h]){if(b&&(d=c?g[h]:g[h].data)){n.isArray(b)?b=b.concat(n.map(b,n.camelCase)):b in d?b=[b]:(b=n.camelCase(b),b=b in d?[b]:b.split(\" \")),e=b.length;while(e--)delete d[b[e]];if(c?!Q(d):!n.isEmptyObject(d))return}(c||(delete g[h].data,Q(g[h])))&&(f?n.cleanData([a],!0):l.deleteExpando||g!=g.window?delete g[h]:g[h]=void 0)}}}n.extend({cache:{},noData:{\"applet \":!0,\"embed \":!0,\"object \":\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\"},hasData:function(a){return a=a.nodeType?n.cache[a[n.expando]]:a[n.expando],!!a&&!Q(a)},data:function(a,b,c){return R(a,b,c)},removeData:function(a,b){return S(a,b)},_data:function(a,b,c){return R(a,b,c,!0)},_removeData:function(a,b){return S(a,b,!0)}}),n.fn.extend({data:function(a,b){var c,d,e,f=this[0],g=f&&f.attributes;if(void 0===a){if(this.length&&(e=n.data(f),1===f.nodeType&&!n._data(f,\"parsedAttrs\"))){c=g.length;while(c--)g[c]&&(d=g[c].name,0===d.indexOf(\"data-\")&&(d=n.camelCase(d.slice(5)),P(f,d,e[d])));n._data(f,\"parsedAttrs\",!0)}return e}return\"object\"==typeof a?this.each(function(){n.data(this,a)}):arguments.length>1?this.each(function(){n.data(this,a,b)}):f?P(f,a,n.data(f,a)):void 0},removeData:function(a){return this.each(function(){n.removeData(this,a)})}}),n.extend({queue:function(a,b,c){var d;return a?(b=(b||\"fx\")+\"queue\",d=n._data(a,b),c&&(!d||n.isArray(c)?d=n._data(a,b,n.makeArray(c)):d.push(c)),d||[]):void 0},dequeue:function(a,b){b=b||\"fx\";var c=n.queue(a,b),d=c.length,e=c.shift(),f=n._queueHooks(a,b),g=function(){n.dequeue(a,b)};\"inprogress\"===e&&(e=c.shift(),d--),e&&(\"fx\"===b&&c.unshift(\"inprogress\"),delete f.stop,e.call(a,g,f)),!d&&f&&f.empty.fire()},_queueHooks:function(a,b){var c=b+\"queueHooks\";return n._data(a,c)||n._data(a,c,{empty:n.Callbacks(\"once memory\").add(function(){n._removeData(a,b+\"queue\"),n._removeData(a,c)})})}}),n.fn.extend({queue:function(a,b){var c=2;return\"string\"!=typeof a&&(b=a,a=\"fx\",c--),arguments.length<c?n.queue(this[0],a):void 0===b?this:this.each(function(){var c=n.queue(this,a,b);n._queueHooks(this,a),\"fx\"===a&&\"inprogress\"!==c[0]&&n.dequeue(this,a)})},dequeue:function(a){return this.each(function(){n.dequeue(this,a)})},clearQueue:function(a){return this.queue(a||\"fx\",[])},promise:function(a,b){var c,d=1,e=n.Deferred(),f=this,g=this.length,h=function(){--d||e.resolveWith(f,[f])};\"string\"!=typeof a&&(b=a,a=void 0),a=a||\"fx\";while(g--)c=n._data(f[g],a+\"queueHooks\"),c&&c.empty&&(d++,c.empty.add(h));return h(),e.promise(b)}}),function(){var a;l.shrinkWrapBlocks=function(){if(null!=a)return a;a=!1;var b,c,e;return c=d.getElementsByTagName(\"body\")[0],c&&c.style?(b=d.createElement(\"div\"),e=d.createElement(\"div\"),e.style.cssText=\"position:absolute;border:0;width:0;height:0;top:0;left:-9999px\",c.appendChild(e).appendChild(b),\"undefined\"!=typeof b.style.zoom&&(b.style.cssText=\"-webkit-box-sizing:content-box;-moz-box-sizing:content-box;box-sizing:content-box;display:block;margin:0;border:0;padding:1px;width:1px;zoom:1\",b.appendChild(d.createElement(\"div\")).style.width=\"5px\",a=3!==b.offsetWidth),c.removeChild(e),a):void 0}}();var T=/[+-]?(?:\\d*\\.|)\\d+(?:[eE][+-]?\\d+|)/.source,U=new RegExp(\"^(?:([+-])=|)(\"+T+\")([a-z%]*)$\",\"i\"),V=[\"Top\",\"Right\",\"Bottom\",\"Left\"],W=function(a,b){return a=b||a,\"none\"===n.css(a,\"display\")||!n.contains(a.ownerDocument,a)};function X(a,b,c,d){var e,f=1,g=20,h=d?function(){return d.cur()}:function(){return n.css(a,b,\"\")},i=h(),j=c&&c[3]||(n.cssNumber[b]?\"\":\"px\"),k=(n.cssNumber[b]||\"px\"!==j&&+i)&&U.exec(n.css(a,b));if(k&&k[3]!==j){j=j||k[3],c=c||[],k=+i||1;do f=f||\".5\",k/=f,n.style(a,b,k+j);while(f!==(f=h()/i)&&1!==f&&--g)}return c&&(k=+k||+i||0,e=c[1]?k+(c[1]+1)*c[2]:+c[2],d&&(d.unit=j,d.start=k,d.end=e)),e}var Y=function(a,b,c,d,e,f,g){var h=0,i=a.length,j=null==c;if(\"object\"===n.type(c)){e=!0;for(h in c)Y(a,b,h,c[h],!0,f,g)}else if(void 0!==d&&(e=!0,n.isFunction(d)||(g=!0),j&&(g?(b.call(a,d),b=null):(j=b,b=function(a,b,c){return j.call(n(a),c)})),b))for(;i>h;h++)b(a[h],c,g?d:d.call(a[h],h,b(a[h],c)));return e?a:j?b.call(a):i?b(a[0],c):f},Z=/^(?:checkbox|radio)$/i,$=/<([\\w:-]+)/,_=/^$|\\/(?:java|ecma)script/i,aa=/^\\s+/,ba=\"abbr|article|aside|audio|bdi|canvas|data|datalist|details|dialog|figcaption|figure|footer|header|hgroup|main|mark|meter|nav|output|picture|progress|section|summary|template|time|video\";function ca(a){var b=ba.split(\"|\"),c=a.createDocumentFragment();if(c.createElement)while(b.length)c.createElement(b.pop());return c}!function(){var a=d.createElement(\"div\"),b=d.createDocumentFragment(),c=d.createElement(\"input\");a.innerHTML=\"  <link/><table></table><a href='/a'>a</a><input type='checkbox'/>\",l.leadingWhitespace=3===a.firstChild.nodeType,l.tbody=!a.getElementsByTagName(\"tbody\").length,l.htmlSerialize=!!a.getElementsByTagName(\"link\").length,l.html5Clone=\"<:nav></:nav>\"!==d.createElement(\"nav\").cloneNode(!0).outerHTML,c.type=\"checkbox\",c.checked=!0,b.appendChild(c),l.appendChecked=c.checked,a.innerHTML=\"<textarea>x</textarea>\",l.noCloneChecked=!!a.cloneNode(!0).lastChild.defaultValue,b.appendChild(a),c=d.createElement(\"input\"),c.setAttribute(\"type\",\"radio\"),c.setAttribute(\"checked\",\"checked\"),c.setAttribute(\"name\",\"t\"),a.appendChild(c),l.checkClone=a.cloneNode(!0).cloneNode(!0).lastChild.checked,l.noCloneEvent=!!a.addEventListener,a[n.expando]=1,l.attributes=!a.getAttribute(n.expando)}();var da={option:[1,\"<select multiple='multiple'>\",\"</select>\"],legend:[1,\"<fieldset>\",\"</fieldset>\"],area:[1,\"<map>\",\"</map>\"],param:[1,\"<object>\",\"</object>\"],thead:[1,\"<table>\",\"</table>\"],tr:[2,\"<table><tbody>\",\"</tbody></table>\"],col:[2,\"<table><tbody></tbody><colgroup>\",\"</colgroup></table>\"],td:[3,\"<table><tbody><tr>\",\"</tr></tbody></table>\"],_default:l.htmlSerialize?[0,\"\",\"\"]:[1,\"X<div>\",\"</div>\"]};da.optgroup=da.option,da.tbody=da.tfoot=da.colgroup=da.caption=da.thead,da.th=da.td;function ea(a,b){var c,d,e=0,f=\"undefined\"!=typeof a.getElementsByTagName?a.getElementsByTagName(b||\"*\"):\"undefined\"!=typeof a.querySelectorAll?a.querySelectorAll(b||\"*\"):void 0;if(!f)for(f=[],c=a.childNodes||a;null!=(d=c[e]);e++)!b||n.nodeName(d,b)?f.push(d):n.merge(f,ea(d,b));return void 0===b||b&&n.nodeName(a,b)?n.merge([a],f):f}function fa(a,b){for(var c,d=0;null!=(c=a[d]);d++)n._data(c,\"globalEval\",!b||n._data(b[d],\"globalEval\"))}var ga=/<|&#?\\w+;/,ha=/<tbody/i;function ia(a){Z.test(a.type)&&(a.defaultChecked=a.checked)}function ja(a,b,c,d,e){for(var f,g,h,i,j,k,m,o=a.length,p=ca(b),q=[],r=0;o>r;r++)if(g=a[r],g||0===g)if(\"object\"===n.type(g))n.merge(q,g.nodeType?[g]:g);else if(ga.test(g)){i=i||p.appendChild(b.createElement(\"div\")),j=($.exec(g)||[\"\",\"\"])[1].toLowerCase(),m=da[j]||da._default,i.innerHTML=m[1]+n.htmlPrefilter(g)+m[2],f=m[0];while(f--)i=i.lastChild;if(!l.leadingWhitespace&&aa.test(g)&&q.push(b.createTextNode(aa.exec(g)[0])),!l.tbody){g=\"table\"!==j||ha.test(g)?\"<table>\"!==m[1]||ha.test(g)?0:i:i.firstChild,f=g&&g.childNodes.length;while(f--)n.nodeName(k=g.childNodes[f],\"tbody\")&&!k.childNodes.length&&g.removeChild(k)}n.merge(q,i.childNodes),i.textContent=\"\";while(i.firstChild)i.removeChild(i.firstChild);i=p.lastChild}else q.push(b.createTextNode(g));i&&p.removeChild(i),l.appendChecked||n.grep(ea(q,\"input\"),ia),r=0;while(g=q[r++])if(d&&n.inArray(g,d)>-1)e&&e.push(g);else if(h=n.contains(g.ownerDocument,g),i=ea(p.appendChild(g),\"script\"),h&&fa(i),c){f=0;while(g=i[f++])_.test(g.type||\"\")&&c.push(g)}return i=null,p}!function(){var b,c,e=d.createElement(\"div\");for(b in{submit:!0,change:!0,focusin:!0})c=\"on\"+b,(l[b]=c in a)||(e.setAttribute(c,\"t\"),l[b]=e.attributes[c].expando===!1);e=null}();var ka=/^(?:input|select|textarea)$/i,la=/^key/,ma=/^(?:mouse|pointer|contextmenu|drag|drop)|click/,na=/^(?:focusinfocus|focusoutblur)$/,oa=/^([^.]*)(?:\\.(.+)|)/;function pa(){return!0}function qa(){return!1}function ra(){try{return d.activeElement}catch(a){}}function sa(a,b,c,d,e,f){var g,h;if(\"object\"==typeof b){\"string\"!=typeof c&&(d=d||c,c=void 0);for(h in b)sa(a,h,c,d,b[h],f);return a}if(null==d&&null==e?(e=c,d=c=void 0):null==e&&(\"string\"==typeof c?(e=d,d=void 0):(e=d,d=c,c=void 0)),e===!1)e=qa;else if(!e)return a;return 1===f&&(g=e,e=function(a){return n().off(a),g.apply(this,arguments)},e.guid=g.guid||(g.guid=n.guid++)),a.each(function(){n.event.add(this,b,e,d,c)})}n.event={global:{},add:function(a,b,c,d,e){var f,g,h,i,j,k,l,m,o,p,q,r=n._data(a);if(r){c.handler&&(i=c,c=i.handler,e=i.selector),c.guid||(c.guid=n.guid++),(g=r.events)||(g=r.events={}),(k=r.handle)||(k=r.handle=function(a){return\"undefined\"==typeof n||a&&n.event.triggered===a.type?void 0:n.event.dispatch.apply(k.elem,arguments)},k.elem=a),b=(b||\"\").match(G)||[\"\"],h=b.length;while(h--)f=oa.exec(b[h])||[],o=q=f[1],p=(f[2]||\"\").split(\".\").sort(),o&&(j=n.event.special[o]||{},o=(e?j.delegateType:j.bindType)||o,j=n.event.special[o]||{},l=n.extend({type:o,origType:q,data:d,handler:c,guid:c.guid,selector:e,needsContext:e&&n.expr.match.needsContext.test(e),namespace:p.join(\".\")},i),(m=g[o])||(m=g[o]=[],m.delegateCount=0,j.setup&&j.setup.call(a,d,p,k)!==!1||(a.addEventListener?a.addEventListener(o,k,!1):a.attachEvent&&a.attachEvent(\"on\"+o,k))),j.add&&(j.add.call(a,l),l.handler.guid||(l.handler.guid=c.guid)),e?m.splice(m.delegateCount++,0,l):m.push(l),n.event.global[o]=!0);a=null}},remove:function(a,b,c,d,e){var f,g,h,i,j,k,l,m,o,p,q,r=n.hasData(a)&&n._data(a);if(r&&(k=r.events)){b=(b||\"\").match(G)||[\"\"],j=b.length;while(j--)if(h=oa.exec(b[j])||[],o=q=h[1],p=(h[2]||\"\").split(\".\").sort(),o){l=n.event.special[o]||{},o=(d?l.delegateType:l.bindType)||o,m=k[o]||[],h=h[2]&&new RegExp(\"(^|\\\\.)\"+p.join(\"\\\\.(?:.*\\\\.|)\")+\"(\\\\.|$)\"),i=f=m.length;while(f--)g=m[f],!e&&q!==g.origType||c&&c.guid!==g.guid||h&&!h.test(g.namespace)||d&&d!==g.selector&&(\"**\"!==d||!g.selector)||(m.splice(f,1),g.selector&&m.delegateCount--,l.remove&&l.remove.call(a,g));i&&!m.length&&(l.teardown&&l.teardown.call(a,p,r.handle)!==!1||n.removeEvent(a,o,r.handle),delete k[o])}else for(o in k)n.event.remove(a,o+b[j],c,d,!0);n.isEmptyObject(k)&&(delete r.handle,n._removeData(a,\"events\"))}},trigger:function(b,c,e,f){var g,h,i,j,l,m,o,p=[e||d],q=k.call(b,\"type\")?b.type:b,r=k.call(b,\"namespace\")?b.namespace.split(\".\"):[];if(i=m=e=e||d,3!==e.nodeType&&8!==e.nodeType&&!na.test(q+n.event.triggered)&&(q.indexOf(\".\")>-1&&(r=q.split(\".\"),q=r.shift(),r.sort()),h=q.indexOf(\":\")<0&&\"on\"+q,b=b[n.expando]?b:new n.Event(q,\"object\"==typeof b&&b),b.isTrigger=f?2:3,b.namespace=r.join(\".\"),b.rnamespace=b.namespace?new RegExp(\"(^|\\\\.)\"+r.join(\"\\\\.(?:.*\\\\.|)\")+\"(\\\\.|$)\"):null,b.result=void 0,b.target||(b.target=e),c=null==c?[b]:n.makeArray(c,[b]),l=n.event.special[q]||{},f||!l.trigger||l.trigger.apply(e,c)!==!1)){if(!f&&!l.noBubble&&!n.isWindow(e)){for(j=l.delegateType||q,na.test(j+q)||(i=i.parentNode);i;i=i.parentNode)p.push(i),m=i;m===(e.ownerDocument||d)&&p.push(m.defaultView||m.parentWindow||a)}o=0;while((i=p[o++])&&!b.isPropagationStopped())b.type=o>1?j:l.bindType||q,g=(n._data(i,\"events\")||{})[b.type]&&n._data(i,\"handle\"),g&&g.apply(i,c),g=h&&i[h],g&&g.apply&&M(i)&&(b.result=g.apply(i,c),b.result===!1&&b.preventDefault());if(b.type=q,!f&&!b.isDefaultPrevented()&&(!l._default||l._default.apply(p.pop(),c)===!1)&&M(e)&&h&&e[q]&&!n.isWindow(e)){m=e[h],m&&(e[h]=null),n.event.triggered=q;try{e[q]()}catch(s){}n.event.triggered=void 0,m&&(e[h]=m)}return b.result}},dispatch:function(a){a=n.event.fix(a);var b,c,d,f,g,h=[],i=e.call(arguments),j=(n._data(this,\"events\")||{})[a.type]||[],k=n.event.special[a.type]||{};if(i[0]=a,a.delegateTarget=this,!k.preDispatch||k.preDispatch.call(this,a)!==!1){h=n.event.handlers.call(this,a,j),b=0;while((f=h[b++])&&!a.isPropagationStopped()){a.currentTarget=f.elem,c=0;while((g=f.handlers[c++])&&!a.isImmediatePropagationStopped())a.rnamespace&&!a.rnamespace.test(g.namespace)||(a.handleObj=g,a.data=g.data,d=((n.event.special[g.origType]||{}).handle||g.handler).apply(f.elem,i),void 0!==d&&(a.result=d)===!1&&(a.preventDefault(),a.stopPropagation()))}return k.postDispatch&&k.postDispatch.call(this,a),a.result}},handlers:function(a,b){var c,d,e,f,g=[],h=b.delegateCount,i=a.target;if(h&&i.nodeType&&(\"click\"!==a.type||isNaN(a.button)||a.button<1))for(;i!=this;i=i.parentNode||this)if(1===i.nodeType&&(i.disabled!==!0||\"click\"!==a.type)){for(d=[],c=0;h>c;c++)f=b[c],e=f.selector+\" \",void 0===d[e]&&(d[e]=f.needsContext?n(e,this).index(i)>-1:n.find(e,this,null,[i]).length),d[e]&&d.push(f);d.length&&g.push({elem:i,handlers:d})}return h<b.length&&g.push({elem:this,handlers:b.slice(h)}),g},fix:function(a){if(a[n.expando])return a;var b,c,e,f=a.type,g=a,h=this.fixHooks[f];h||(this.fixHooks[f]=h=ma.test(f)?this.mouseHooks:la.test(f)?this.keyHooks:{}),e=h.props?this.props.concat(h.props):this.props,a=new n.Event(g),b=e.length;while(b--)c=e[b],a[c]=g[c];return a.target||(a.target=g.srcElement||d),3===a.target.nodeType&&(a.target=a.target.parentNode),a.metaKey=!!a.metaKey,h.filter?h.filter(a,g):a},props:\"altKey bubbles cancelable ctrlKey currentTarget detail eventPhase metaKey relatedTarget shiftKey target timeStamp view which\".split(\" \"),fixHooks:{},keyHooks:{props:\"char charCode key keyCode\".split(\" \"),filter:function(a,b){return null==a.which&&(a.which=null!=b.charCode?b.charCode:b.keyCode),a}},mouseHooks:{props:\"button buttons clientX clientY fromElement offsetX offsetY pageX pageY screenX screenY toElement\".split(\" \"),filter:function(a,b){var c,e,f,g=b.button,h=b.fromElement;return null==a.pageX&&null!=b.clientX&&(e=a.target.ownerDocument||d,f=e.documentElement,c=e.body,a.pageX=b.clientX+(f&&f.scrollLeft||c&&c.scrollLeft||0)-(f&&f.clientLeft||c&&c.clientLeft||0),a.pageY=b.clientY+(f&&f.scrollTop||c&&c.scrollTop||0)-(f&&f.clientTop||c&&c.clientTop||0)),!a.relatedTarget&&h&&(a.relatedTarget=h===a.target?b.toElement:h),a.which||void 0===g||(a.which=1&g?1:2&g?3:4&g?2:0),a}},special:{load:{noBubble:!0},focus:{trigger:function(){if(this!==ra()&&this.focus)try{return this.focus(),!1}catch(a){}},delegateType:\"focusin\"},blur:{trigger:function(){return this===ra()&&this.blur?(this.blur(),!1):void 0},delegateType:\"focusout\"},click:{trigger:function(){return n.nodeName(this,\"input\")&&\"checkbox\"===this.type&&this.click?(this.click(),!1):void 0},_default:function(a){return n.nodeName(a.target,\"a\")}},beforeunload:{postDispatch:function(a){void 0!==a.result&&a.originalEvent&&(a.originalEvent.returnValue=a.result)}}},simulate:function(a,b,c){var d=n.extend(new n.Event,c,{type:a,isSimulated:!0});n.event.trigger(d,null,b),d.isDefaultPrevented()&&c.preventDefault()}},n.removeEvent=d.removeEventListener?function(a,b,c){a.removeEventListener&&a.removeEventListener(b,c)}:function(a,b,c){var d=\"on\"+b;a.detachEvent&&(\"undefined\"==typeof a[d]&&(a[d]=null),a.detachEvent(d,c))},n.Event=function(a,b){return this instanceof n.Event?(a&&a.type?(this.originalEvent=a,this.type=a.type,this.isDefaultPrevented=a.defaultPrevented||void 0===a.defaultPrevented&&a.returnValue===!1?pa:qa):this.type=a,b&&n.extend(this,b),this.timeStamp=a&&a.timeStamp||n.now(),void(this[n.expando]=!0)):new n.Event(a,b)},n.Event.prototype={constructor:n.Event,isDefaultPrevented:qa,isPropagationStopped:qa,isImmediatePropagationStopped:qa,preventDefault:function(){var a=this.originalEvent;this.isDefaultPrevented=pa,a&&(a.preventDefault?a.preventDefault():a.returnValue=!1)},stopPropagation:function(){var a=this.originalEvent;this.isPropagationStopped=pa,a&&!this.isSimulated&&(a.stopPropagation&&a.stopPropagation(),a.cancelBubble=!0)},stopImmediatePropagation:function(){var a=this.originalEvent;this.isImmediatePropagationStopped=pa,a&&a.stopImmediatePropagation&&a.stopImmediatePropagation(),this.stopPropagation()}},n.each({mouseenter:\"mouseover\",mouseleave:\"mouseout\",pointerenter:\"pointerover\",pointerleave:\"pointerout\"},function(a,b){n.event.special[a]={delegateType:b,bindType:b,handle:function(a){var c,d=this,e=a.relatedTarget,f=a.handleObj;return e&&(e===d||n.contains(d,e))||(a.type=f.origType,c=f.handler.apply(this,arguments),a.type=b),c}}}),l.submit||(n.event.special.submit={setup:function(){return n.nodeName(this,\"form\")?!1:void n.event.add(this,\"click._submit keypress._submit\",function(a){var b=a.target,c=n.nodeName(b,\"input\")||n.nodeName(b,\"button\")?n.prop(b,\"form\"):void 0;c&&!n._data(c,\"submit\")&&(n.event.add(c,\"submit._submit\",function(a){a._submitBubble=!0}),n._data(c,\"submit\",!0))})},postDispatch:function(a){a._submitBubble&&(delete a._submitBubble,this.parentNode&&!a.isTrigger&&n.event.simulate(\"submit\",this.parentNode,a))},teardown:function(){return n.nodeName(this,\"form\")?!1:void n.event.remove(this,\"._submit\")}}),l.change||(n.event.special.change={setup:function(){return ka.test(this.nodeName)?(\"checkbox\"!==this.type&&\"radio\"!==this.type||(n.event.add(this,\"propertychange._change\",function(a){\"checked\"===a.originalEvent.propertyName&&(this._justChanged=!0)}),n.event.add(this,\"click._change\",function(a){this._justChanged&&!a.isTrigger&&(this._justChanged=!1),n.event.simulate(\"change\",this,a)})),!1):void n.event.add(this,\"beforeactivate._change\",function(a){var b=a.target;ka.test(b.nodeName)&&!n._data(b,\"change\")&&(n.event.add(b,\"change._change\",function(a){!this.parentNode||a.isSimulated||a.isTrigger||n.event.simulate(\"change\",this.parentNode,a)}),n._data(b,\"change\",!0))})},handle:function(a){var b=a.target;return this!==b||a.isSimulated||a.isTrigger||\"radio\"!==b.type&&\"checkbox\"!==b.type?a.handleObj.handler.apply(this,arguments):void 0},teardown:function(){return n.event.remove(this,\"._change\"),!ka.test(this.nodeName)}}),l.focusin||n.each({focus:\"focusin\",blur:\"focusout\"},function(a,b){var c=function(a){n.event.simulate(b,a.target,n.event.fix(a))};n.event.special[b]={setup:function(){var d=this.ownerDocument||this,e=n._data(d,b);e||d.addEventListener(a,c,!0),n._data(d,b,(e||0)+1)},teardown:function(){var d=this.ownerDocument||this,e=n._data(d,b)-1;e?n._data(d,b,e):(d.removeEventListener(a,c,!0),n._removeData(d,b))}}}),n.fn.extend({on:function(a,b,c,d){return sa(this,a,b,c,d)},one:function(a,b,c,d){return sa(this,a,b,c,d,1)},off:function(a,b,c){var d,e;if(a&&a.preventDefault&&a.handleObj)return d=a.handleObj,n(a.delegateTarget).off(d.namespace?d.origType+\".\"+d.namespace:d.origType,d.selector,d.handler),this;if(\"object\"==typeof a){for(e in a)this.off(e,b,a[e]);return this}return b!==!1&&\"function\"!=typeof b||(c=b,b=void 0),c===!1&&(c=qa),this.each(function(){n.event.remove(this,a,c,b)})},trigger:function(a,b){return this.each(function(){n.event.trigger(a,b,this)})},triggerHandler:function(a,b){var c=this[0];return c?n.event.trigger(a,b,c,!0):void 0}});var ta=/ jQuery\\d+=\"(?:null|\\d+)\"/g,ua=new RegExp(\"<(?:\"+ba+\")[\\\\s/>]\",\"i\"),va=/<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\\w:-]+)[^>]*)\\/>/gi,wa=/<script|<style|<link/i,xa=/checked\\s*(?:[^=]|=\\s*.checked.)/i,ya=/^true\\/(.*)/,za=/^\\s*<!(?:\\[CDATA\\[|--)|(?:\\]\\]|--)>\\s*$/g,Aa=ca(d),Ba=Aa.appendChild(d.createElement(\"div\"));function Ca(a,b){return n.nodeName(a,\"table\")&&n.nodeName(11!==b.nodeType?b:b.firstChild,\"tr\")?a.getElementsByTagName(\"tbody\")[0]||a.appendChild(a.ownerDocument.createElement(\"tbody\")):a}function Da(a){return a.type=(null!==n.find.attr(a,\"type\"))+\"/\"+a.type,a}function Ea(a){var b=ya.exec(a.type);return b?a.type=b[1]:a.removeAttribute(\"type\"),a}function Fa(a,b){if(1===b.nodeType&&n.hasData(a)){var c,d,e,f=n._data(a),g=n._data(b,f),h=f.events;if(h){delete g.handle,g.events={};for(c in h)for(d=0,e=h[c].length;e>d;d++)n.event.add(b,c,h[c][d])}g.data&&(g.data=n.extend({},g.data))}}function Ga(a,b){var c,d,e;if(1===b.nodeType){if(c=b.nodeName.toLowerCase(),!l.noCloneEvent&&b[n.expando]){e=n._data(b);for(d in e.events)n.removeEvent(b,d,e.handle);b.removeAttribute(n.expando)}\"script\"===c&&b.text!==a.text?(Da(b).text=a.text,Ea(b)):\"object\"===c?(b.parentNode&&(b.outerHTML=a.outerHTML),l.html5Clone&&a.innerHTML&&!n.trim(b.innerHTML)&&(b.innerHTML=a.innerHTML)):\"input\"===c&&Z.test(a.type)?(b.defaultChecked=b.checked=a.checked,b.value!==a.value&&(b.value=a.value)):\"option\"===c?b.defaultSelected=b.selected=a.defaultSelected:\"input\"!==c&&\"textarea\"!==c||(b.defaultValue=a.defaultValue)}}function Ha(a,b,c,d){b=f.apply([],b);var e,g,h,i,j,k,m=0,o=a.length,p=o-1,q=b[0],r=n.isFunction(q);if(r||o>1&&\"string\"==typeof q&&!l.checkClone&&xa.test(q))return a.each(function(e){var f=a.eq(e);r&&(b[0]=q.call(this,e,f.html())),Ha(f,b,c,d)});if(o&&(k=ja(b,a[0].ownerDocument,!1,a,d),e=k.firstChild,1===k.childNodes.length&&(k=e),e||d)){for(i=n.map(ea(k,\"script\"),Da),h=i.length;o>m;m++)g=k,m!==p&&(g=n.clone(g,!0,!0),h&&n.merge(i,ea(g,\"script\"))),c.call(a[m],g,m);if(h)for(j=i[i.length-1].ownerDocument,n.map(i,Ea),m=0;h>m;m++)g=i[m],_.test(g.type||\"\")&&!n._data(g,\"globalEval\")&&n.contains(j,g)&&(g.src?n._evalUrl&&n._evalUrl(g.src):n.globalEval((g.text||g.textContent||g.innerHTML||\"\").replace(za,\"\")));k=e=null}return a}function Ia(a,b,c){for(var d,e=b?n.filter(b,a):a,f=0;null!=(d=e[f]);f++)c||1!==d.nodeType||n.cleanData(ea(d)),d.parentNode&&(c&&n.contains(d.ownerDocument,d)&&fa(ea(d,\"script\")),d.parentNode.removeChild(d));return a}n.extend({htmlPrefilter:function(a){return a.replace(va,\"<$1></$2>\")},clone:function(a,b,c){var d,e,f,g,h,i=n.contains(a.ownerDocument,a);if(l.html5Clone||n.isXMLDoc(a)||!ua.test(\"<\"+a.nodeName+\">\")?f=a.cloneNode(!0):(Ba.innerHTML=a.outerHTML,Ba.removeChild(f=Ba.firstChild)),!(l.noCloneEvent&&l.noCloneChecked||1!==a.nodeType&&11!==a.nodeType||n.isXMLDoc(a)))for(d=ea(f),h=ea(a),g=0;null!=(e=h[g]);++g)d[g]&&Ga(e,d[g]);if(b)if(c)for(h=h||ea(a),d=d||ea(f),g=0;null!=(e=h[g]);g++)Fa(e,d[g]);else Fa(a,f);return d=ea(f,\"script\"),d.length>0&&fa(d,!i&&ea(a,\"script\")),d=h=e=null,f},cleanData:function(a,b){for(var d,e,f,g,h=0,i=n.expando,j=n.cache,k=l.attributes,m=n.event.special;null!=(d=a[h]);h++)if((b||M(d))&&(f=d[i],g=f&&j[f])){if(g.events)for(e in g.events)m[e]?n.event.remove(d,e):n.removeEvent(d,e,g.handle);j[f]&&(delete j[f],k||\"undefined\"==typeof d.removeAttribute?d[i]=void 0:d.removeAttribute(i),c.push(f))}}}),n.fn.extend({domManip:Ha,detach:function(a){return Ia(this,a,!0)},remove:function(a){return Ia(this,a)},text:function(a){return Y(this,function(a){return void 0===a?n.text(this):this.empty().append((this[0]&&this[0].ownerDocument||d).createTextNode(a))},null,a,arguments.length)},append:function(){return Ha(this,arguments,function(a){if(1===this.nodeType||11===this.nodeType||9===this.nodeType){var b=Ca(this,a);b.appendChild(a)}})},prepend:function(){return Ha(this,arguments,function(a){if(1===this.nodeType||11===this.nodeType||9===this.nodeType){var b=Ca(this,a);b.insertBefore(a,b.firstChild)}})},before:function(){return Ha(this,arguments,function(a){this.parentNode&&this.parentNode.insertBefore(a,this)})},after:function(){return Ha(this,arguments,function(a){this.parentNode&&this.parentNode.insertBefore(a,this.nextSibling)})},empty:function(){for(var a,b=0;null!=(a=this[b]);b++){1===a.nodeType&&n.cleanData(ea(a,!1));while(a.firstChild)a.removeChild(a.firstChild);a.options&&n.nodeName(a,\"select\")&&(a.options.length=0)}return this},clone:function(a,b){return a=null==a?!1:a,b=null==b?a:b,this.map(function(){return n.clone(this,a,b)})},html:function(a){return Y(this,function(a){var b=this[0]||{},c=0,d=this.length;if(void 0===a)return 1===b.nodeType?b.innerHTML.replace(ta,\"\"):void 0;if(\"string\"==typeof a&&!wa.test(a)&&(l.htmlSerialize||!ua.test(a))&&(l.leadingWhitespace||!aa.test(a))&&!da[($.exec(a)||[\"\",\"\"])[1].toLowerCase()]){a=n.htmlPrefilter(a);try{for(;d>c;c++)b=this[c]||{},1===b.nodeType&&(n.cleanData(ea(b,!1)),b.innerHTML=a);b=0}catch(e){}}b&&this.empty().append(a)},null,a,arguments.length)},replaceWith:function(){var a=[];return Ha(this,arguments,function(b){var c=this.parentNode;n.inArray(this,a)<0&&(n.cleanData(ea(this)),c&&c.replaceChild(b,this))},a)}}),n.each({appendTo:\"append\",prependTo:\"prepend\",insertBefore:\"before\",insertAfter:\"after\",replaceAll:\"replaceWith\"},function(a,b){n.fn[a]=function(a){for(var c,d=0,e=[],f=n(a),h=f.length-1;h>=d;d++)c=d===h?this:this.clone(!0),n(f[d])[b](c),g.apply(e,c.get());return this.pushStack(e)}});var Ja,Ka={HTML:\"block\",BODY:\"block\"};function La(a,b){var c=n(b.createElement(a)).appendTo(b.body),d=n.css(c[0],\"display\");return c.detach(),d}function Ma(a){var b=d,c=Ka[a];return c||(c=La(a,b),\"none\"!==c&&c||(Ja=(Ja||n(\"<iframe frameborder='0' width='0' height='0'/>\")).appendTo(b.documentElement),b=(Ja[0].contentWindow||Ja[0].contentDocument).document,b.write(),b.close(),c=La(a,b),Ja.detach()),Ka[a]=c),c}var Na=/^margin/,Oa=new RegExp(\"^(\"+T+\")(?!px)[a-z%]+$\",\"i\"),Pa=function(a,b,c,d){var e,f,g={};for(f in b)g[f]=a.style[f],a.style[f]=b[f];e=c.apply(a,d||[]);for(f in b)a.style[f]=g[f];return e},Qa=d.documentElement;!function(){var b,c,e,f,g,h,i=d.createElement(\"div\"),j=d.createElement(\"div\");if(j.style){j.style.cssText=\"float:left;opacity:.5\",l.opacity=\"0.5\"===j.style.opacity,l.cssFloat=!!j.style.cssFloat,j.style.backgroundClip=\"content-box\",j.cloneNode(!0).style.backgroundClip=\"\",l.clearCloneStyle=\"content-box\"===j.style.backgroundClip,i=d.createElement(\"div\"),i.style.cssText=\"border:0;width:8px;height:0;top:0;left:-9999px;padding:0;margin-top:1px;position:absolute\",j.innerHTML=\"\",i.appendChild(j),l.boxSizing=\"\"===j.style.boxSizing||\"\"===j.style.MozBoxSizing||\"\"===j.style.WebkitBoxSizing,n.extend(l,{reliableHiddenOffsets:function(){return null==b&&k(),f},boxSizingReliable:function(){return null==b&&k(),e},pixelMarginRight:function(){return null==b&&k(),c},pixelPosition:function(){return null==b&&k(),b},reliableMarginRight:function(){return null==b&&k(),g},reliableMarginLeft:function(){return null==b&&k(),h}});function k(){var k,l,m=d.documentElement;m.appendChild(i),j.style.cssText=\"-webkit-box-sizing:border-box;box-sizing:border-box;position:relative;display:block;margin:auto;border:1px;padding:1px;top:1%;width:50%\",b=e=h=!1,c=g=!0,a.getComputedStyle&&(l=a.getComputedStyle(j),b=\"1%\"!==(l||{}).top,h=\"2px\"===(l||{}).marginLeft,e=\"4px\"===(l||{width:\"4px\"}).width,j.style.marginRight=\"50%\",c=\"4px\"===(l||{marginRight:\"4px\"}).marginRight,k=j.appendChild(d.createElement(\"div\")),k.style.cssText=j.style.cssText=\"-webkit-box-sizing:content-box;-moz-box-sizing:content-box;box-sizing:content-box;display:block;margin:0;border:0;padding:0\",k.style.marginRight=k.style.width=\"0\",j.style.width=\"1px\",g=!parseFloat((a.getComputedStyle(k)||{}).marginRight),j.removeChild(k)),j.style.display=\"none\",f=0===j.getClientRects().length,f&&(j.style.display=\"\",j.innerHTML=\"<table><tr><td></td><td>t</td></tr></table>\",j.childNodes[0].style.borderCollapse=\"separate\",k=j.getElementsByTagName(\"td\"),k[0].style.cssText=\"margin:0;border:0;padding:0;display:none\",f=0===k[0].offsetHeight,f&&(k[0].style.display=\"\",k[1].style.display=\"none\",f=0===k[0].offsetHeight)),m.removeChild(i)}}}();var Ra,Sa,Ta=/^(top|right|bottom|left)$/;a.getComputedStyle?(Ra=function(b){var c=b.ownerDocument.defaultView;return c&&c.opener||(c=a),c.getComputedStyle(b)},Sa=function(a,b,c){var d,e,f,g,h=a.style;return c=c||Ra(a),g=c?c.getPropertyValue(b)||c[b]:void 0,\"\"!==g&&void 0!==g||n.contains(a.ownerDocument,a)||(g=n.style(a,b)),c&&!l.pixelMarginRight()&&Oa.test(g)&&Na.test(b)&&(d=h.width,e=h.minWidth,f=h.maxWidth,h.minWidth=h.maxWidth=h.width=g,g=c.width,h.width=d,h.minWidth=e,h.maxWidth=f),void 0===g?g:g+\"\"}):Qa.currentStyle&&(Ra=function(a){return a.currentStyle},Sa=function(a,b,c){var d,e,f,g,h=a.style;return c=c||Ra(a),g=c?c[b]:void 0,null==g&&h&&h[b]&&(g=h[b]),Oa.test(g)&&!Ta.test(b)&&(d=h.left,e=a.runtimeStyle,f=e&&e.left,f&&(e.left=a.currentStyle.left),h.left=\"fontSize\"===b?\"1em\":g,g=h.pixelLeft+\"px\",h.left=d,f&&(e.left=f)),void 0===g?g:g+\"\"||\"auto\"});function Ua(a,b){return{get:function(){return a()?void delete this.get:(this.get=b).apply(this,arguments)}}}var Va=/alpha\\([^)]*\\)/i,Wa=/opacity\\s*=\\s*([^)]*)/i,Xa=/^(none|table(?!-c[ea]).+)/,Ya=new RegExp(\"^(\"+T+\")(.*)$\",\"i\"),Za={position:\"absolute\",visibility:\"hidden\",display:\"block\"},$a={letterSpacing:\"0\",fontWeight:\"400\"},_a=[\"Webkit\",\"O\",\"Moz\",\"ms\"],ab=d.createElement(\"div\").style;function bb(a){if(a in ab)return a;var b=a.charAt(0).toUpperCase()+a.slice(1),c=_a.length;while(c--)if(a=_a[c]+b,a in ab)return a}function cb(a,b){for(var c,d,e,f=[],g=0,h=a.length;h>g;g++)d=a[g],d.style&&(f[g]=n._data(d,\"olddisplay\"),c=d.style.display,b?(f[g]||\"none\"!==c||(d.style.display=\"\"),\"\"===d.style.display&&W(d)&&(f[g]=n._data(d,\"olddisplay\",Ma(d.nodeName)))):(e=W(d),(c&&\"none\"!==c||!e)&&n._data(d,\"olddisplay\",e?c:n.css(d,\"display\"))));for(g=0;h>g;g++)d=a[g],d.style&&(b&&\"none\"!==d.style.display&&\"\"!==d.style.display||(d.style.display=b?f[g]||\"\":\"none\"));return a}function db(a,b,c){var d=Ya.exec(b);return d?Math.max(0,d[1]-(c||0))+(d[2]||\"px\"):b}function eb(a,b,c,d,e){for(var f=c===(d?\"border\":\"content\")?4:\"width\"===b?1:0,g=0;4>f;f+=2)\"margin\"===c&&(g+=n.css(a,c+V[f],!0,e)),d?(\"content\"===c&&(g-=n.css(a,\"padding\"+V[f],!0,e)),\"margin\"!==c&&(g-=n.css(a,\"border\"+V[f]+\"Width\",!0,e))):(g+=n.css(a,\"padding\"+V[f],!0,e),\"padding\"!==c&&(g+=n.css(a,\"border\"+V[f]+\"Width\",!0,e)));return g}function fb(a,b,c){var d=!0,e=\"width\"===b?a.offsetWidth:a.offsetHeight,f=Ra(a),g=l.boxSizing&&\"border-box\"===n.css(a,\"boxSizing\",!1,f);if(0>=e||null==e){if(e=Sa(a,b,f),(0>e||null==e)&&(e=a.style[b]),Oa.test(e))return e;d=g&&(l.boxSizingReliable()||e===a.style[b]),e=parseFloat(e)||0}return e+eb(a,b,c||(g?\"border\":\"content\"),d,f)+\"px\"}n.extend({cssHooks:{opacity:{get:function(a,b){if(b){var c=Sa(a,\"opacity\");return\"\"===c?\"1\":c}}}},cssNumber:{animationIterationCount:!0,columnCount:!0,fillOpacity:!0,flexGrow:!0,flexShrink:!0,fontWeight:!0,lineHeight:!0,opacity:!0,order:!0,orphans:!0,widows:!0,zIndex:!0,zoom:!0},cssProps:{\"float\":l.cssFloat?\"cssFloat\":\"styleFloat\"},style:function(a,b,c,d){if(a&&3!==a.nodeType&&8!==a.nodeType&&a.style){var e,f,g,h=n.camelCase(b),i=a.style;if(b=n.cssProps[h]||(n.cssProps[h]=bb(h)||h),g=n.cssHooks[b]||n.cssHooks[h],void 0===c)return g&&\"get\"in g&&void 0!==(e=g.get(a,!1,d))?e:i[b];if(f=typeof c,\"string\"===f&&(e=U.exec(c))&&e[1]&&(c=X(a,b,e),f=\"number\"),null!=c&&c===c&&(\"number\"===f&&(c+=e&&e[3]||(n.cssNumber[h]?\"\":\"px\")),l.clearCloneStyle||\"\"!==c||0!==b.indexOf(\"background\")||(i[b]=\"inherit\"),!(g&&\"set\"in g&&void 0===(c=g.set(a,c,d)))))try{i[b]=c}catch(j){}}},css:function(a,b,c,d){var e,f,g,h=n.camelCase(b);return b=n.cssProps[h]||(n.cssProps[h]=bb(h)||h),g=n.cssHooks[b]||n.cssHooks[h],g&&\"get\"in g&&(f=g.get(a,!0,c)),void 0===f&&(f=Sa(a,b,d)),\"normal\"===f&&b in $a&&(f=$a[b]),\"\"===c||c?(e=parseFloat(f),c===!0||isFinite(e)?e||0:f):f}}),n.each([\"height\",\"width\"],function(a,b){n.cssHooks[b]={get:function(a,c,d){return c?Xa.test(n.css(a,\"display\"))&&0===a.offsetWidth?Pa(a,Za,function(){return fb(a,b,d)}):fb(a,b,d):void 0},set:function(a,c,d){var e=d&&Ra(a);return db(a,c,d?eb(a,b,d,l.boxSizing&&\"border-box\"===n.css(a,\"boxSizing\",!1,e),e):0)}}}),l.opacity||(n.cssHooks.opacity={get:function(a,b){return Wa.test((b&&a.currentStyle?a.currentStyle.filter:a.style.filter)||\"\")?.01*parseFloat(RegExp.$1)+\"\":b?\"1\":\"\"},set:function(a,b){var c=a.style,d=a.currentStyle,e=n.isNumeric(b)?\"alpha(opacity=\"+100*b+\")\":\"\",f=d&&d.filter||c.filter||\"\";c.zoom=1,(b>=1||\"\"===b)&&\"\"===n.trim(f.replace(Va,\"\"))&&c.removeAttribute&&(c.removeAttribute(\"filter\"),\"\"===b||d&&!d.filter)||(c.filter=Va.test(f)?f.replace(Va,e):f+\" \"+e)}}),n.cssHooks.marginRight=Ua(l.reliableMarginRight,function(a,b){return b?Pa(a,{display:\"inline-block\"},Sa,[a,\"marginRight\"]):void 0}),n.cssHooks.marginLeft=Ua(l.reliableMarginLeft,function(a,b){return b?(parseFloat(Sa(a,\"marginLeft\"))||(n.contains(a.ownerDocument,a)?a.getBoundingClientRect().left-Pa(a,{\r\nmarginLeft:0},function(){return a.getBoundingClientRect().left}):0))+\"px\":void 0}),n.each({margin:\"\",padding:\"\",border:\"Width\"},function(a,b){n.cssHooks[a+b]={expand:function(c){for(var d=0,e={},f=\"string\"==typeof c?c.split(\" \"):[c];4>d;d++)e[a+V[d]+b]=f[d]||f[d-2]||f[0];return e}},Na.test(a)||(n.cssHooks[a+b].set=db)}),n.fn.extend({css:function(a,b){return Y(this,function(a,b,c){var d,e,f={},g=0;if(n.isArray(b)){for(d=Ra(a),e=b.length;e>g;g++)f[b[g]]=n.css(a,b[g],!1,d);return f}return void 0!==c?n.style(a,b,c):n.css(a,b)},a,b,arguments.length>1)},show:function(){return cb(this,!0)},hide:function(){return cb(this)},toggle:function(a){return\"boolean\"==typeof a?a?this.show():this.hide():this.each(function(){W(this)?n(this).show():n(this).hide()})}});function gb(a,b,c,d,e){return new gb.prototype.init(a,b,c,d,e)}n.Tween=gb,gb.prototype={constructor:gb,init:function(a,b,c,d,e,f){this.elem=a,this.prop=c,this.easing=e||n.easing._default,this.options=b,this.start=this.now=this.cur(),this.end=d,this.unit=f||(n.cssNumber[c]?\"\":\"px\")},cur:function(){var a=gb.propHooks[this.prop];return a&&a.get?a.get(this):gb.propHooks._default.get(this)},run:function(a){var b,c=gb.propHooks[this.prop];return this.options.duration?this.pos=b=n.easing[this.easing](a,this.options.duration*a,0,1,this.options.duration):this.pos=b=a,this.now=(this.end-this.start)*b+this.start,this.options.step&&this.options.step.call(this.elem,this.now,this),c&&c.set?c.set(this):gb.propHooks._default.set(this),this}},gb.prototype.init.prototype=gb.prototype,gb.propHooks={_default:{get:function(a){var b;return 1!==a.elem.nodeType||null!=a.elem[a.prop]&&null==a.elem.style[a.prop]?a.elem[a.prop]:(b=n.css(a.elem,a.prop,\"\"),b&&\"auto\"!==b?b:0)},set:function(a){n.fx.step[a.prop]?n.fx.step[a.prop](a):1!==a.elem.nodeType||null==a.elem.style[n.cssProps[a.prop]]&&!n.cssHooks[a.prop]?a.elem[a.prop]=a.now:n.style(a.elem,a.prop,a.now+a.unit)}}},gb.propHooks.scrollTop=gb.propHooks.scrollLeft={set:function(a){a.elem.nodeType&&a.elem.parentNode&&(a.elem[a.prop]=a.now)}},n.easing={linear:function(a){return a},swing:function(a){return.5-Math.cos(a*Math.PI)/2},_default:\"swing\"},n.fx=gb.prototype.init,n.fx.step={};var hb,ib,jb=/^(?:toggle|show|hide)$/,kb=/queueHooks$/;function lb(){return a.setTimeout(function(){hb=void 0}),hb=n.now()}function mb(a,b){var c,d={height:a},e=0;for(b=b?1:0;4>e;e+=2-b)c=V[e],d[\"margin\"+c]=d[\"padding\"+c]=a;return b&&(d.opacity=d.width=a),d}function nb(a,b,c){for(var d,e=(qb.tweeners[b]||[]).concat(qb.tweeners[\"*\"]),f=0,g=e.length;g>f;f++)if(d=e[f].call(c,b,a))return d}function ob(a,b,c){var d,e,f,g,h,i,j,k,m=this,o={},p=a.style,q=a.nodeType&&W(a),r=n._data(a,\"fxshow\");c.queue||(h=n._queueHooks(a,\"fx\"),null==h.unqueued&&(h.unqueued=0,i=h.empty.fire,h.empty.fire=function(){h.unqueued||i()}),h.unqueued++,m.always(function(){m.always(function(){h.unqueued--,n.queue(a,\"fx\").length||h.empty.fire()})})),1===a.nodeType&&(\"height\"in b||\"width\"in b)&&(c.overflow=[p.overflow,p.overflowX,p.overflowY],j=n.css(a,\"display\"),k=\"none\"===j?n._data(a,\"olddisplay\")||Ma(a.nodeName):j,\"inline\"===k&&\"none\"===n.css(a,\"float\")&&(l.inlineBlockNeedsLayout&&\"inline\"!==Ma(a.nodeName)?p.zoom=1:p.display=\"inline-block\")),c.overflow&&(p.overflow=\"hidden\",l.shrinkWrapBlocks()||m.always(function(){p.overflow=c.overflow[0],p.overflowX=c.overflow[1],p.overflowY=c.overflow[2]}));for(d in b)if(e=b[d],jb.exec(e)){if(delete b[d],f=f||\"toggle\"===e,e===(q?\"hide\":\"show\")){if(\"show\"!==e||!r||void 0===r[d])continue;q=!0}o[d]=r&&r[d]||n.style(a,d)}else j=void 0;if(n.isEmptyObject(o))\"inline\"===(\"none\"===j?Ma(a.nodeName):j)&&(p.display=j);else{r?\"hidden\"in r&&(q=r.hidden):r=n._data(a,\"fxshow\",{}),f&&(r.hidden=!q),q?n(a).show():m.done(function(){n(a).hide()}),m.done(function(){var b;n._removeData(a,\"fxshow\");for(b in o)n.style(a,b,o[b])});for(d in o)g=nb(q?r[d]:0,d,m),d in r||(r[d]=g.start,q&&(g.end=g.start,g.start=\"width\"===d||\"height\"===d?1:0))}}function pb(a,b){var c,d,e,f,g;for(c in a)if(d=n.camelCase(c),e=b[d],f=a[c],n.isArray(f)&&(e=f[1],f=a[c]=f[0]),c!==d&&(a[d]=f,delete a[c]),g=n.cssHooks[d],g&&\"expand\"in g){f=g.expand(f),delete a[d];for(c in f)c in a||(a[c]=f[c],b[c]=e)}else b[d]=e}function qb(a,b,c){var d,e,f=0,g=qb.prefilters.length,h=n.Deferred().always(function(){delete i.elem}),i=function(){if(e)return!1;for(var b=hb||lb(),c=Math.max(0,j.startTime+j.duration-b),d=c/j.duration||0,f=1-d,g=0,i=j.tweens.length;i>g;g++)j.tweens[g].run(f);return h.notifyWith(a,[j,f,c]),1>f&&i?c:(h.resolveWith(a,[j]),!1)},j=h.promise({elem:a,props:n.extend({},b),opts:n.extend(!0,{specialEasing:{},easing:n.easing._default},c),originalProperties:b,originalOptions:c,startTime:hb||lb(),duration:c.duration,tweens:[],createTween:function(b,c){var d=n.Tween(a,j.opts,b,c,j.opts.specialEasing[b]||j.opts.easing);return j.tweens.push(d),d},stop:function(b){var c=0,d=b?j.tweens.length:0;if(e)return this;for(e=!0;d>c;c++)j.tweens[c].run(1);return b?(h.notifyWith(a,[j,1,0]),h.resolveWith(a,[j,b])):h.rejectWith(a,[j,b]),this}}),k=j.props;for(pb(k,j.opts.specialEasing);g>f;f++)if(d=qb.prefilters[f].call(j,a,k,j.opts))return n.isFunction(d.stop)&&(n._queueHooks(j.elem,j.opts.queue).stop=n.proxy(d.stop,d)),d;return n.map(k,nb,j),n.isFunction(j.opts.start)&&j.opts.start.call(a,j),n.fx.timer(n.extend(i,{elem:a,anim:j,queue:j.opts.queue})),j.progress(j.opts.progress).done(j.opts.done,j.opts.complete).fail(j.opts.fail).always(j.opts.always)}n.Animation=n.extend(qb,{tweeners:{\"*\":[function(a,b){var c=this.createTween(a,b);return X(c.elem,a,U.exec(b),c),c}]},tweener:function(a,b){n.isFunction(a)?(b=a,a=[\"*\"]):a=a.match(G);for(var c,d=0,e=a.length;e>d;d++)c=a[d],qb.tweeners[c]=qb.tweeners[c]||[],qb.tweeners[c].unshift(b)},prefilters:[ob],prefilter:function(a,b){b?qb.prefilters.unshift(a):qb.prefilters.push(a)}}),n.speed=function(a,b,c){var d=a&&\"object\"==typeof a?n.extend({},a):{complete:c||!c&&b||n.isFunction(a)&&a,duration:a,easing:c&&b||b&&!n.isFunction(b)&&b};return d.duration=n.fx.off?0:\"number\"==typeof d.duration?d.duration:d.duration in n.fx.speeds?n.fx.speeds[d.duration]:n.fx.speeds._default,null!=d.queue&&d.queue!==!0||(d.queue=\"fx\"),d.old=d.complete,d.complete=function(){n.isFunction(d.old)&&d.old.call(this),d.queue&&n.dequeue(this,d.queue)},d},n.fn.extend({fadeTo:function(a,b,c,d){return this.filter(W).css(\"opacity\",0).show().end().animate({opacity:b},a,c,d)},animate:function(a,b,c,d){var e=n.isEmptyObject(a),f=n.speed(b,c,d),g=function(){var b=qb(this,n.extend({},a),f);(e||n._data(this,\"finish\"))&&b.stop(!0)};return g.finish=g,e||f.queue===!1?this.each(g):this.queue(f.queue,g)},stop:function(a,b,c){var d=function(a){var b=a.stop;delete a.stop,b(c)};return\"string\"!=typeof a&&(c=b,b=a,a=void 0),b&&a!==!1&&this.queue(a||\"fx\",[]),this.each(function(){var b=!0,e=null!=a&&a+\"queueHooks\",f=n.timers,g=n._data(this);if(e)g[e]&&g[e].stop&&d(g[e]);else for(e in g)g[e]&&g[e].stop&&kb.test(e)&&d(g[e]);for(e=f.length;e--;)f[e].elem!==this||null!=a&&f[e].queue!==a||(f[e].anim.stop(c),b=!1,f.splice(e,1));!b&&c||n.dequeue(this,a)})},finish:function(a){return a!==!1&&(a=a||\"fx\"),this.each(function(){var b,c=n._data(this),d=c[a+\"queue\"],e=c[a+\"queueHooks\"],f=n.timers,g=d?d.length:0;for(c.finish=!0,n.queue(this,a,[]),e&&e.stop&&e.stop.call(this,!0),b=f.length;b--;)f[b].elem===this&&f[b].queue===a&&(f[b].anim.stop(!0),f.splice(b,1));for(b=0;g>b;b++)d[b]&&d[b].finish&&d[b].finish.call(this);delete c.finish})}}),n.each([\"toggle\",\"show\",\"hide\"],function(a,b){var c=n.fn[b];n.fn[b]=function(a,d,e){return null==a||\"boolean\"==typeof a?c.apply(this,arguments):this.animate(mb(b,!0),a,d,e)}}),n.each({slideDown:mb(\"show\"),slideUp:mb(\"hide\"),slideToggle:mb(\"toggle\"),fadeIn:{opacity:\"show\"},fadeOut:{opacity:\"hide\"},fadeToggle:{opacity:\"toggle\"}},function(a,b){n.fn[a]=function(a,c,d){return this.animate(b,a,c,d)}}),n.timers=[],n.fx.tick=function(){var a,b=n.timers,c=0;for(hb=n.now();c<b.length;c++)a=b[c],a()||b[c]!==a||b.splice(c--,1);b.length||n.fx.stop(),hb=void 0},n.fx.timer=function(a){n.timers.push(a),a()?n.fx.start():n.timers.pop()},n.fx.interval=13,n.fx.start=function(){ib||(ib=a.setInterval(n.fx.tick,n.fx.interval))},n.fx.stop=function(){a.clearInterval(ib),ib=null},n.fx.speeds={slow:600,fast:200,_default:400},n.fn.delay=function(b,c){return b=n.fx?n.fx.speeds[b]||b:b,c=c||\"fx\",this.queue(c,function(c,d){var e=a.setTimeout(c,b);d.stop=function(){a.clearTimeout(e)}})},function(){var a,b=d.createElement(\"input\"),c=d.createElement(\"div\"),e=d.createElement(\"select\"),f=e.appendChild(d.createElement(\"option\"));c=d.createElement(\"div\"),c.setAttribute(\"className\",\"t\"),c.innerHTML=\"  <link/><table></table><a href='/a'>a</a><input type='checkbox'/>\",a=c.getElementsByTagName(\"a\")[0],b.setAttribute(\"type\",\"checkbox\"),c.appendChild(b),a=c.getElementsByTagName(\"a\")[0],a.style.cssText=\"top:1px\",l.getSetAttribute=\"t\"!==c.className,l.style=/top/.test(a.getAttribute(\"style\")),l.hrefNormalized=\"/a\"===a.getAttribute(\"href\"),l.checkOn=!!b.value,l.optSelected=f.selected,l.enctype=!!d.createElement(\"form\").enctype,e.disabled=!0,l.optDisabled=!f.disabled,b=d.createElement(\"input\"),b.setAttribute(\"value\",\"\"),l.input=\"\"===b.getAttribute(\"value\"),b.value=\"t\",b.setAttribute(\"type\",\"radio\"),l.radioValue=\"t\"===b.value}();var rb=/\\r/g,sb=/[\\x20\\t\\r\\n\\f]+/g;n.fn.extend({val:function(a){var b,c,d,e=this[0];{if(arguments.length)return d=n.isFunction(a),this.each(function(c){var e;1===this.nodeType&&(e=d?a.call(this,c,n(this).val()):a,null==e?e=\"\":\"number\"==typeof e?e+=\"\":n.isArray(e)&&(e=n.map(e,function(a){return null==a?\"\":a+\"\"})),b=n.valHooks[this.type]||n.valHooks[this.nodeName.toLowerCase()],b&&\"set\"in b&&void 0!==b.set(this,e,\"value\")||(this.value=e))});if(e)return b=n.valHooks[e.type]||n.valHooks[e.nodeName.toLowerCase()],b&&\"get\"in b&&void 0!==(c=b.get(e,\"value\"))?c:(c=e.value,\"string\"==typeof c?c.replace(rb,\"\"):null==c?\"\":c)}}}),n.extend({valHooks:{option:{get:function(a){var b=n.find.attr(a,\"value\");return null!=b?b:n.trim(n.text(a)).replace(sb,\" \")}},select:{get:function(a){for(var b,c,d=a.options,e=a.selectedIndex,f=\"select-one\"===a.type||0>e,g=f?null:[],h=f?e+1:d.length,i=0>e?h:f?e:0;h>i;i++)if(c=d[i],(c.selected||i===e)&&(l.optDisabled?!c.disabled:null===c.getAttribute(\"disabled\"))&&(!c.parentNode.disabled||!n.nodeName(c.parentNode,\"optgroup\"))){if(b=n(c).val(),f)return b;g.push(b)}return g},set:function(a,b){var c,d,e=a.options,f=n.makeArray(b),g=e.length;while(g--)if(d=e[g],n.inArray(n.valHooks.option.get(d),f)>-1)try{d.selected=c=!0}catch(h){d.scrollHeight}else d.selected=!1;return c||(a.selectedIndex=-1),e}}}}),n.each([\"radio\",\"checkbox\"],function(){n.valHooks[this]={set:function(a,b){return n.isArray(b)?a.checked=n.inArray(n(a).val(),b)>-1:void 0}},l.checkOn||(n.valHooks[this].get=function(a){return null===a.getAttribute(\"value\")?\"on\":a.value})});var tb,ub,vb=n.expr.attrHandle,wb=/^(?:checked|selected)$/i,xb=l.getSetAttribute,yb=l.input;n.fn.extend({attr:function(a,b){return Y(this,n.attr,a,b,arguments.length>1)},removeAttr:function(a){return this.each(function(){n.removeAttr(this,a)})}}),n.extend({attr:function(a,b,c){var d,e,f=a.nodeType;if(3!==f&&8!==f&&2!==f)return\"undefined\"==typeof a.getAttribute?n.prop(a,b,c):(1===f&&n.isXMLDoc(a)||(b=b.toLowerCase(),e=n.attrHooks[b]||(n.expr.match.bool.test(b)?ub:tb)),void 0!==c?null===c?void n.removeAttr(a,b):e&&\"set\"in e&&void 0!==(d=e.set(a,c,b))?d:(a.setAttribute(b,c+\"\"),c):e&&\"get\"in e&&null!==(d=e.get(a,b))?d:(d=n.find.attr(a,b),null==d?void 0:d))},attrHooks:{type:{set:function(a,b){if(!l.radioValue&&\"radio\"===b&&n.nodeName(a,\"input\")){var c=a.value;return a.setAttribute(\"type\",b),c&&(a.value=c),b}}}},removeAttr:function(a,b){var c,d,e=0,f=b&&b.match(G);if(f&&1===a.nodeType)while(c=f[e++])d=n.propFix[c]||c,n.expr.match.bool.test(c)?yb&&xb||!wb.test(c)?a[d]=!1:a[n.camelCase(\"default-\"+c)]=a[d]=!1:n.attr(a,c,\"\"),a.removeAttribute(xb?c:d)}}),ub={set:function(a,b,c){return b===!1?n.removeAttr(a,c):yb&&xb||!wb.test(c)?a.setAttribute(!xb&&n.propFix[c]||c,c):a[n.camelCase(\"default-\"+c)]=a[c]=!0,c}},n.each(n.expr.match.bool.source.match(/\\w+/g),function(a,b){var c=vb[b]||n.find.attr;yb&&xb||!wb.test(b)?vb[b]=function(a,b,d){var e,f;return d||(f=vb[b],vb[b]=e,e=null!=c(a,b,d)?b.toLowerCase():null,vb[b]=f),e}:vb[b]=function(a,b,c){return c?void 0:a[n.camelCase(\"default-\"+b)]?b.toLowerCase():null}}),yb&&xb||(n.attrHooks.value={set:function(a,b,c){return n.nodeName(a,\"input\")?void(a.defaultValue=b):tb&&tb.set(a,b,c)}}),xb||(tb={set:function(a,b,c){var d=a.getAttributeNode(c);return d||a.setAttributeNode(d=a.ownerDocument.createAttribute(c)),d.value=b+=\"\",\"value\"===c||b===a.getAttribute(c)?b:void 0}},vb.id=vb.name=vb.coords=function(a,b,c){var d;return c?void 0:(d=a.getAttributeNode(b))&&\"\"!==d.value?d.value:null},n.valHooks.button={get:function(a,b){var c=a.getAttributeNode(b);return c&&c.specified?c.value:void 0},set:tb.set},n.attrHooks.contenteditable={set:function(a,b,c){tb.set(a,\"\"===b?!1:b,c)}},n.each([\"width\",\"height\"],function(a,b){n.attrHooks[b]={set:function(a,c){return\"\"===c?(a.setAttribute(b,\"auto\"),c):void 0}}})),l.style||(n.attrHooks.style={get:function(a){return a.style.cssText||void 0},set:function(a,b){return a.style.cssText=b+\"\"}});var zb=/^(?:input|select|textarea|button|object)$/i,Ab=/^(?:a|area)$/i;n.fn.extend({prop:function(a,b){return Y(this,n.prop,a,b,arguments.length>1)},removeProp:function(a){return a=n.propFix[a]||a,this.each(function(){try{this[a]=void 0,delete this[a]}catch(b){}})}}),n.extend({prop:function(a,b,c){var d,e,f=a.nodeType;if(3!==f&&8!==f&&2!==f)return 1===f&&n.isXMLDoc(a)||(b=n.propFix[b]||b,e=n.propHooks[b]),void 0!==c?e&&\"set\"in e&&void 0!==(d=e.set(a,c,b))?d:a[b]=c:e&&\"get\"in e&&null!==(d=e.get(a,b))?d:a[b]},propHooks:{tabIndex:{get:function(a){var b=n.find.attr(a,\"tabindex\");return b?parseInt(b,10):zb.test(a.nodeName)||Ab.test(a.nodeName)&&a.href?0:-1}}},propFix:{\"for\":\"htmlFor\",\"class\":\"className\"}}),l.hrefNormalized||n.each([\"href\",\"src\"],function(a,b){n.propHooks[b]={get:function(a){return a.getAttribute(b,4)}}}),l.optSelected||(n.propHooks.selected={get:function(a){var b=a.parentNode;return b&&(b.selectedIndex,b.parentNode&&b.parentNode.selectedIndex),null},set:function(a){var b=a.parentNode;b&&(b.selectedIndex,b.parentNode&&b.parentNode.selectedIndex)}}),n.each([\"tabIndex\",\"readOnly\",\"maxLength\",\"cellSpacing\",\"cellPadding\",\"rowSpan\",\"colSpan\",\"useMap\",\"frameBorder\",\"contentEditable\"],function(){n.propFix[this.toLowerCase()]=this}),l.enctype||(n.propFix.enctype=\"encoding\");var Bb=/[\\t\\r\\n\\f]/g;function Cb(a){return n.attr(a,\"class\")||\"\"}n.fn.extend({addClass:function(a){var b,c,d,e,f,g,h,i=0;if(n.isFunction(a))return this.each(function(b){n(this).addClass(a.call(this,b,Cb(this)))});if(\"string\"==typeof a&&a){b=a.match(G)||[];while(c=this[i++])if(e=Cb(c),d=1===c.nodeType&&(\" \"+e+\" \").replace(Bb,\" \")){g=0;while(f=b[g++])d.indexOf(\" \"+f+\" \")<0&&(d+=f+\" \");h=n.trim(d),e!==h&&n.attr(c,\"class\",h)}}return this},removeClass:function(a){var b,c,d,e,f,g,h,i=0;if(n.isFunction(a))return this.each(function(b){n(this).removeClass(a.call(this,b,Cb(this)))});if(!arguments.length)return this.attr(\"class\",\"\");if(\"string\"==typeof a&&a){b=a.match(G)||[];while(c=this[i++])if(e=Cb(c),d=1===c.nodeType&&(\" \"+e+\" \").replace(Bb,\" \")){g=0;while(f=b[g++])while(d.indexOf(\" \"+f+\" \")>-1)d=d.replace(\" \"+f+\" \",\" \");h=n.trim(d),e!==h&&n.attr(c,\"class\",h)}}return this},toggleClass:function(a,b){var c=typeof a;return\"boolean\"==typeof b&&\"string\"===c?b?this.addClass(a):this.removeClass(a):n.isFunction(a)?this.each(function(c){n(this).toggleClass(a.call(this,c,Cb(this),b),b)}):this.each(function(){var b,d,e,f;if(\"string\"===c){d=0,e=n(this),f=a.match(G)||[];while(b=f[d++])e.hasClass(b)?e.removeClass(b):e.addClass(b)}else void 0!==a&&\"boolean\"!==c||(b=Cb(this),b&&n._data(this,\"__className__\",b),n.attr(this,\"class\",b||a===!1?\"\":n._data(this,\"__className__\")||\"\"))})},hasClass:function(a){var b,c,d=0;b=\" \"+a+\" \";while(c=this[d++])if(1===c.nodeType&&(\" \"+Cb(c)+\" \").replace(Bb,\" \").indexOf(b)>-1)return!0;return!1}}),n.each(\"blur focus focusin focusout load resize scroll unload click dblclick mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave change select submit keydown keypress keyup error contextmenu\".split(\" \"),function(a,b){n.fn[b]=function(a,c){return arguments.length>0?this.on(b,null,a,c):this.trigger(b)}}),n.fn.extend({hover:function(a,b){return this.mouseenter(a).mouseleave(b||a)}});var Db=a.location,Eb=n.now(),Fb=/\\?/,Gb=/(,)|(\\[|{)|(}|])|\"(?:[^\"\\\\\\r\\n]|\\\\[\"\\\\\\/bfnrt]|\\\\u[\\da-fA-F]{4})*\"\\s*:?|true|false|null|-?(?!0\\d)\\d+(?:\\.\\d+|)(?:[eE][+-]?\\d+|)/g;n.parseJSON=function(b){if(a.JSON&&a.JSON.parse)return a.JSON.parse(b+\"\");var c,d=null,e=n.trim(b+\"\");return e&&!n.trim(e.replace(Gb,function(a,b,e,f){return c&&b&&(d=0),0===d?a:(c=e||b,d+=!f-!e,\"\")}))?Function(\"return \"+e)():n.error(\"Invalid JSON: \"+b)},n.parseXML=function(b){var c,d;if(!b||\"string\"!=typeof b)return null;try{a.DOMParser?(d=new a.DOMParser,c=d.parseFromString(b,\"text/xml\")):(c=new a.ActiveXObject(\"Microsoft.XMLDOM\"),c.async=\"false\",c.loadXML(b))}catch(e){c=void 0}return c&&c.documentElement&&!c.getElementsByTagName(\"parsererror\").length||n.error(\"Invalid XML: \"+b),c};var Hb=/#.*$/,Ib=/([?&])_=[^&]*/,Jb=/^(.*?):[ \\t]*([^\\r\\n]*)\\r?$/gm,Kb=/^(?:about|app|app-storage|.+-extension|file|res|widget):$/,Lb=/^(?:GET|HEAD)$/,Mb=/^\\/\\//,Nb=/^([\\w.+-]+:)(?:\\/\\/(?:[^\\/?#]*@|)([^\\/?#:]*)(?::(\\d+)|)|)/,Ob={},Pb={},Qb=\"*/\".concat(\"*\"),Rb=Db.href,Sb=Nb.exec(Rb.toLowerCase())||[];function Tb(a){return function(b,c){\"string\"!=typeof b&&(c=b,b=\"*\");var d,e=0,f=b.toLowerCase().match(G)||[];if(n.isFunction(c))while(d=f[e++])\"+\"===d.charAt(0)?(d=d.slice(1)||\"*\",(a[d]=a[d]||[]).unshift(c)):(a[d]=a[d]||[]).push(c)}}function Ub(a,b,c,d){var e={},f=a===Pb;function g(h){var i;return e[h]=!0,n.each(a[h]||[],function(a,h){var j=h(b,c,d);return\"string\"!=typeof j||f||e[j]?f?!(i=j):void 0:(b.dataTypes.unshift(j),g(j),!1)}),i}return g(b.dataTypes[0])||!e[\"*\"]&&g(\"*\")}function Vb(a,b){var c,d,e=n.ajaxSettings.flatOptions||{};for(d in b)void 0!==b[d]&&((e[d]?a:c||(c={}))[d]=b[d]);return c&&n.extend(!0,a,c),a}function Wb(a,b,c){var d,e,f,g,h=a.contents,i=a.dataTypes;while(\"*\"===i[0])i.shift(),void 0===e&&(e=a.mimeType||b.getResponseHeader(\"Content-Type\"));if(e)for(g in h)if(h[g]&&h[g].test(e)){i.unshift(g);break}if(i[0]in c)f=i[0];else{for(g in c){if(!i[0]||a.converters[g+\" \"+i[0]]){f=g;break}d||(d=g)}f=f||d}return f?(f!==i[0]&&i.unshift(f),c[f]):void 0}function Xb(a,b,c,d){var e,f,g,h,i,j={},k=a.dataTypes.slice();if(k[1])for(g in a.converters)j[g.toLowerCase()]=a.converters[g];f=k.shift();while(f)if(a.responseFields[f]&&(c[a.responseFields[f]]=b),!i&&d&&a.dataFilter&&(b=a.dataFilter(b,a.dataType)),i=f,f=k.shift())if(\"*\"===f)f=i;else if(\"*\"!==i&&i!==f){if(g=j[i+\" \"+f]||j[\"* \"+f],!g)for(e in j)if(h=e.split(\" \"),h[1]===f&&(g=j[i+\" \"+h[0]]||j[\"* \"+h[0]])){g===!0?g=j[e]:j[e]!==!0&&(f=h[0],k.unshift(h[1]));break}if(g!==!0)if(g&&a[\"throws\"])b=g(b);else try{b=g(b)}catch(l){return{state:\"parsererror\",error:g?l:\"No conversion from \"+i+\" to \"+f}}}return{state:\"success\",data:b}}n.extend({active:0,lastModified:{},etag:{},ajaxSettings:{url:Rb,type:\"GET\",isLocal:Kb.test(Sb[1]),global:!0,processData:!0,async:!0,contentType:\"application/x-www-form-urlencoded; charset=UTF-8\",accepts:{\"*\":Qb,text:\"text/plain\",html:\"text/html\",xml:\"application/xml, text/xml\",json:\"application/json, text/javascript\"},contents:{xml:/\\bxml\\b/,html:/\\bhtml/,json:/\\bjson\\b/},responseFields:{xml:\"responseXML\",text:\"responseText\",json:\"responseJSON\"},converters:{\"* text\":String,\"text html\":!0,\"text json\":n.parseJSON,\"text xml\":n.parseXML},flatOptions:{url:!0,context:!0}},ajaxSetup:function(a,b){return b?Vb(Vb(a,n.ajaxSettings),b):Vb(n.ajaxSettings,a)},ajaxPrefilter:Tb(Ob),ajaxTransport:Tb(Pb),ajax:function(b,c){\"object\"==typeof b&&(c=b,b=void 0),c=c||{};var d,e,f,g,h,i,j,k,l=n.ajaxSetup({},c),m=l.context||l,o=l.context&&(m.nodeType||m.jquery)?n(m):n.event,p=n.Deferred(),q=n.Callbacks(\"once memory\"),r=l.statusCode||{},s={},t={},u=0,v=\"canceled\",w={readyState:0,getResponseHeader:function(a){var b;if(2===u){if(!k){k={};while(b=Jb.exec(g))k[b[1].toLowerCase()]=b[2]}b=k[a.toLowerCase()]}return null==b?null:b},getAllResponseHeaders:function(){return 2===u?g:null},setRequestHeader:function(a,b){var c=a.toLowerCase();return u||(a=t[c]=t[c]||a,s[a]=b),this},overrideMimeType:function(a){return u||(l.mimeType=a),this},statusCode:function(a){var b;if(a)if(2>u)for(b in a)r[b]=[r[b],a[b]];else w.always(a[w.status]);return this},abort:function(a){var b=a||v;return j&&j.abort(b),y(0,b),this}};if(p.promise(w).complete=q.add,w.success=w.done,w.error=w.fail,l.url=((b||l.url||Rb)+\"\").replace(Hb,\"\").replace(Mb,Sb[1]+\"//\"),l.type=c.method||c.type||l.method||l.type,l.dataTypes=n.trim(l.dataType||\"*\").toLowerCase().match(G)||[\"\"],null==l.crossDomain&&(d=Nb.exec(l.url.toLowerCase()),l.crossDomain=!(!d||d[1]===Sb[1]&&d[2]===Sb[2]&&(d[3]||(\"http:\"===d[1]?\"80\":\"443\"))===(Sb[3]||(\"http:\"===Sb[1]?\"80\":\"443\")))),l.data&&l.processData&&\"string\"!=typeof l.data&&(l.data=n.param(l.data,l.traditional)),Ub(Ob,l,c,w),2===u)return w;i=n.event&&l.global,i&&0===n.active++&&n.event.trigger(\"ajaxStart\"),l.type=l.type.toUpperCase(),l.hasContent=!Lb.test(l.type),f=l.url,l.hasContent||(l.data&&(f=l.url+=(Fb.test(f)?\"&\":\"?\")+l.data,delete l.data),l.cache===!1&&(l.url=Ib.test(f)?f.replace(Ib,\"$1_=\"+Eb++):f+(Fb.test(f)?\"&\":\"?\")+\"_=\"+Eb++)),l.ifModified&&(n.lastModified[f]&&w.setRequestHeader(\"If-Modified-Since\",n.lastModified[f]),n.etag[f]&&w.setRequestHeader(\"If-None-Match\",n.etag[f])),(l.data&&l.hasContent&&l.contentType!==!1||c.contentType)&&w.setRequestHeader(\"Content-Type\",l.contentType),w.setRequestHeader(\"Accept\",l.dataTypes[0]&&l.accepts[l.dataTypes[0]]?l.accepts[l.dataTypes[0]]+(\"*\"!==l.dataTypes[0]?\", \"+Qb+\"; q=0.01\":\"\"):l.accepts[\"*\"]);for(e in l.headers)w.setRequestHeader(e,l.headers[e]);if(l.beforeSend&&(l.beforeSend.call(m,w,l)===!1||2===u))return w.abort();v=\"abort\";for(e in{success:1,error:1,complete:1})w[e](l[e]);if(j=Ub(Pb,l,c,w)){if(w.readyState=1,i&&o.trigger(\"ajaxSend\",[w,l]),2===u)return w;l.async&&l.timeout>0&&(h=a.setTimeout(function(){w.abort(\"timeout\")},l.timeout));try{u=1,j.send(s,y)}catch(x){if(!(2>u))throw x;y(-1,x)}}else y(-1,\"No Transport\");function y(b,c,d,e){var k,s,t,v,x,y=c;2!==u&&(u=2,h&&a.clearTimeout(h),j=void 0,g=e||\"\",w.readyState=b>0?4:0,k=b>=200&&300>b||304===b,d&&(v=Wb(l,w,d)),v=Xb(l,v,w,k),k?(l.ifModified&&(x=w.getResponseHeader(\"Last-Modified\"),x&&(n.lastModified[f]=x),x=w.getResponseHeader(\"etag\"),x&&(n.etag[f]=x)),204===b||\"HEAD\"===l.type?y=\"nocontent\":304===b?y=\"notmodified\":(y=v.state,s=v.data,t=v.error,k=!t)):(t=y,!b&&y||(y=\"error\",0>b&&(b=0))),w.status=b,w.statusText=(c||y)+\"\",k?p.resolveWith(m,[s,y,w]):p.rejectWith(m,[w,y,t]),w.statusCode(r),r=void 0,i&&o.trigger(k?\"ajaxSuccess\":\"ajaxError\",[w,l,k?s:t]),q.fireWith(m,[w,y]),i&&(o.trigger(\"ajaxComplete\",[w,l]),--n.active||n.event.trigger(\"ajaxStop\")))}return w},getJSON:function(a,b,c){return n.get(a,b,c,\"json\")},getScript:function(a,b){return n.get(a,void 0,b,\"script\")}}),n.each([\"get\",\"post\"],function(a,b){n[b]=function(a,c,d,e){return n.isFunction(c)&&(e=e||d,d=c,c=void 0),n.ajax(n.extend({url:a,type:b,dataType:e,data:c,success:d},n.isPlainObject(a)&&a))}}),n._evalUrl=function(a){return n.ajax({url:a,type:\"GET\",dataType:\"script\",cache:!0,async:!1,global:!1,\"throws\":!0})},n.fn.extend({wrapAll:function(a){if(n.isFunction(a))return this.each(function(b){n(this).wrapAll(a.call(this,b))});if(this[0]){var b=n(a,this[0].ownerDocument).eq(0).clone(!0);this[0].parentNode&&b.insertBefore(this[0]),b.map(function(){var a=this;while(a.firstChild&&1===a.firstChild.nodeType)a=a.firstChild;return a}).append(this)}return this},wrapInner:function(a){return n.isFunction(a)?this.each(function(b){n(this).wrapInner(a.call(this,b))}):this.each(function(){var b=n(this),c=b.contents();c.length?c.wrapAll(a):b.append(a)})},wrap:function(a){var b=n.isFunction(a);return this.each(function(c){n(this).wrapAll(b?a.call(this,c):a)})},unwrap:function(){return this.parent().each(function(){n.nodeName(this,\"body\")||n(this).replaceWith(this.childNodes)}).end()}});function Yb(a){return a.style&&a.style.display||n.css(a,\"display\")}function Zb(a){if(!n.contains(a.ownerDocument||d,a))return!0;while(a&&1===a.nodeType){if(\"none\"===Yb(a)||\"hidden\"===a.type)return!0;a=a.parentNode}return!1}n.expr.filters.hidden=function(a){return l.reliableHiddenOffsets()?a.offsetWidth<=0&&a.offsetHeight<=0&&!a.getClientRects().length:Zb(a)},n.expr.filters.visible=function(a){return!n.expr.filters.hidden(a)};var $b=/%20/g,_b=/\\[\\]$/,ac=/\\r?\\n/g,bc=/^(?:submit|button|image|reset|file)$/i,cc=/^(?:input|select|textarea|keygen)/i;function dc(a,b,c,d){var e;if(n.isArray(b))n.each(b,function(b,e){c||_b.test(a)?d(a,e):dc(a+\"[\"+(\"object\"==typeof e&&null!=e?b:\"\")+\"]\",e,c,d)});else if(c||\"object\"!==n.type(b))d(a,b);else for(e in b)dc(a+\"[\"+e+\"]\",b[e],c,d)}n.param=function(a,b){var c,d=[],e=function(a,b){b=n.isFunction(b)?b():null==b?\"\":b,d[d.length]=encodeURIComponent(a)+\"=\"+encodeURIComponent(b)};if(void 0===b&&(b=n.ajaxSettings&&n.ajaxSettings.traditional),n.isArray(a)||a.jquery&&!n.isPlainObject(a))n.each(a,function(){e(this.name,this.value)});else for(c in a)dc(c,a[c],b,e);return d.join(\"&\").replace($b,\"+\")},n.fn.extend({serialize:function(){return n.param(this.serializeArray())},serializeArray:function(){return this.map(function(){var a=n.prop(this,\"elements\");return a?n.makeArray(a):this}).filter(function(){var a=this.type;return this.name&&!n(this).is(\":disabled\")&&cc.test(this.nodeName)&&!bc.test(a)&&(this.checked||!Z.test(a))}).map(function(a,b){var c=n(this).val();return null==c?null:n.isArray(c)?n.map(c,function(a){return{name:b.name,value:a.replace(ac,\"\\r\\n\")}}):{name:b.name,value:c.replace(ac,\"\\r\\n\")}}).get()}}),n.ajaxSettings.xhr=void 0!==a.ActiveXObject?function(){return this.isLocal?ic():d.documentMode>8?hc():/^(get|post|head|put|delete|options)$/i.test(this.type)&&hc()||ic()}:hc;var ec=0,fc={},gc=n.ajaxSettings.xhr();a.attachEvent&&a.attachEvent(\"onunload\",function(){for(var a in fc)fc[a](void 0,!0)}),l.cors=!!gc&&\"withCredentials\"in gc,gc=l.ajax=!!gc,gc&&n.ajaxTransport(function(b){if(!b.crossDomain||l.cors){var c;return{send:function(d,e){var f,g=b.xhr(),h=++ec;if(g.open(b.type,b.url,b.async,b.username,b.password),b.xhrFields)for(f in b.xhrFields)g[f]=b.xhrFields[f];b.mimeType&&g.overrideMimeType&&g.overrideMimeType(b.mimeType),b.crossDomain||d[\"X-Requested-With\"]||(d[\"X-Requested-With\"]=\"XMLHttpRequest\");for(f in d)void 0!==d[f]&&g.setRequestHeader(f,d[f]+\"\");g.send(b.hasContent&&b.data||null),c=function(a,d){var f,i,j;if(c&&(d||4===g.readyState))if(delete fc[h],c=void 0,g.onreadystatechange=n.noop,d)4!==g.readyState&&g.abort();else{j={},f=g.status,\"string\"==typeof g.responseText&&(j.text=g.responseText);try{i=g.statusText}catch(k){i=\"\"}f||!b.isLocal||b.crossDomain?1223===f&&(f=204):f=j.text?200:404}j&&e(f,i,j,g.getAllResponseHeaders())},b.async?4===g.readyState?a.setTimeout(c):g.onreadystatechange=fc[h]=c:c()},abort:function(){c&&c(void 0,!0)}}}});function hc(){try{return new a.XMLHttpRequest}catch(b){}}function ic(){try{return new a.ActiveXObject(\"Microsoft.XMLHTTP\")}catch(b){}}n.ajaxSetup({accepts:{script:\"text/javascript, application/javascript, application/ecmascript, application/x-ecmascript\"},contents:{script:/\\b(?:java|ecma)script\\b/},converters:{\"text script\":function(a){return n.globalEval(a),a}}}),n.ajaxPrefilter(\"script\",function(a){void 0===a.cache&&(a.cache=!1),a.crossDomain&&(a.type=\"GET\",a.global=!1)}),n.ajaxTransport(\"script\",function(a){if(a.crossDomain){var b,c=d.head||n(\"head\")[0]||d.documentElement;return{send:function(e,f){b=d.createElement(\"script\"),b.async=!0,a.scriptCharset&&(b.charset=a.scriptCharset),b.src=a.url,b.onload=b.onreadystatechange=function(a,c){(c||!b.readyState||/loaded|complete/.test(b.readyState))&&(b.onload=b.onreadystatechange=null,b.parentNode&&b.parentNode.removeChild(b),b=null,c||f(200,\"success\"))},c.insertBefore(b,c.firstChild)},abort:function(){b&&b.onload(void 0,!0)}}}});var jc=[],kc=/(=)\\?(?=&|$)|\\?\\?/;n.ajaxSetup({jsonp:\"callback\",jsonpCallback:function(){var a=jc.pop()||n.expando+\"_\"+Eb++;return this[a]=!0,a}}),n.ajaxPrefilter(\"json jsonp\",function(b,c,d){var e,f,g,h=b.jsonp!==!1&&(kc.test(b.url)?\"url\":\"string\"==typeof b.data&&0===(b.contentType||\"\").indexOf(\"application/x-www-form-urlencoded\")&&kc.test(b.data)&&\"data\");return h||\"jsonp\"===b.dataTypes[0]?(e=b.jsonpCallback=n.isFunction(b.jsonpCallback)?b.jsonpCallback():b.jsonpCallback,h?b[h]=b[h].replace(kc,\"$1\"+e):b.jsonp!==!1&&(b.url+=(Fb.test(b.url)?\"&\":\"?\")+b.jsonp+\"=\"+e),b.converters[\"script json\"]=function(){return g||n.error(e+\" was not called\"),g[0]},b.dataTypes[0]=\"json\",f=a[e],a[e]=function(){g=arguments},d.always(function(){void 0===f?n(a).removeProp(e):a[e]=f,b[e]&&(b.jsonpCallback=c.jsonpCallback,jc.push(e)),g&&n.isFunction(f)&&f(g[0]),g=f=void 0}),\"script\"):void 0}),n.parseHTML=function(a,b,c){if(!a||\"string\"!=typeof a)return null;\"boolean\"==typeof b&&(c=b,b=!1),b=b||d;var e=x.exec(a),f=!c&&[];return e?[b.createElement(e[1])]:(e=ja([a],b,f),f&&f.length&&n(f).remove(),n.merge([],e.childNodes))};var lc=n.fn.load;n.fn.load=function(a,b,c){if(\"string\"!=typeof a&&lc)return lc.apply(this,arguments);var d,e,f,g=this,h=a.indexOf(\" \");return h>-1&&(d=n.trim(a.slice(h,a.length)),a=a.slice(0,h)),n.isFunction(b)?(c=b,b=void 0):b&&\"object\"==typeof b&&(e=\"POST\"),g.length>0&&n.ajax({url:a,type:e||\"GET\",dataType:\"html\",data:b}).done(function(a){f=arguments,g.html(d?n(\"<div>\").append(n.parseHTML(a)).find(d):a)}).always(c&&function(a,b){g.each(function(){c.apply(this,f||[a.responseText,b,a])})}),this},n.each([\"ajaxStart\",\"ajaxStop\",\"ajaxComplete\",\"ajaxError\",\"ajaxSuccess\",\"ajaxSend\"],function(a,b){n.fn[b]=function(a){return this.on(b,a)}}),n.expr.filters.animated=function(a){return n.grep(n.timers,function(b){return a===b.elem}).length};function mc(a){return n.isWindow(a)?a:9===a.nodeType?a.defaultView||a.parentWindow:!1}n.offset={setOffset:function(a,b,c){var d,e,f,g,h,i,j,k=n.css(a,\"position\"),l=n(a),m={};\"static\"===k&&(a.style.position=\"relative\"),h=l.offset(),f=n.css(a,\"top\"),i=n.css(a,\"left\"),j=(\"absolute\"===k||\"fixed\"===k)&&n.inArray(\"auto\",[f,i])>-1,j?(d=l.position(),g=d.top,e=d.left):(g=parseFloat(f)||0,e=parseFloat(i)||0),n.isFunction(b)&&(b=b.call(a,c,n.extend({},h))),null!=b.top&&(m.top=b.top-h.top+g),null!=b.left&&(m.left=b.left-h.left+e),\"using\"in b?b.using.call(a,m):l.css(m)}},n.fn.extend({offset:function(a){if(arguments.length)return void 0===a?this:this.each(function(b){n.offset.setOffset(this,a,b)});var b,c,d={top:0,left:0},e=this[0],f=e&&e.ownerDocument;if(f)return b=f.documentElement,n.contains(b,e)?(\"undefined\"!=typeof e.getBoundingClientRect&&(d=e.getBoundingClientRect()),c=mc(f),{top:d.top+(c.pageYOffset||b.scrollTop)-(b.clientTop||0),left:d.left+(c.pageXOffset||b.scrollLeft)-(b.clientLeft||0)}):d},position:function(){if(this[0]){var a,b,c={top:0,left:0},d=this[0];return\"fixed\"===n.css(d,\"position\")?b=d.getBoundingClientRect():(a=this.offsetParent(),b=this.offset(),n.nodeName(a[0],\"html\")||(c=a.offset()),c.top+=n.css(a[0],\"borderTopWidth\",!0),c.left+=n.css(a[0],\"borderLeftWidth\",!0)),{top:b.top-c.top-n.css(d,\"marginTop\",!0),left:b.left-c.left-n.css(d,\"marginLeft\",!0)}}},offsetParent:function(){return this.map(function(){var a=this.offsetParent;while(a&&!n.nodeName(a,\"html\")&&\"static\"===n.css(a,\"position\"))a=a.offsetParent;return a||Qa})}}),n.each({scrollLeft:\"pageXOffset\",scrollTop:\"pageYOffset\"},function(a,b){var c=/Y/.test(b);n.fn[a]=function(d){return Y(this,function(a,d,e){var f=mc(a);return void 0===e?f?b in f?f[b]:f.document.documentElement[d]:a[d]:void(f?f.scrollTo(c?n(f).scrollLeft():e,c?e:n(f).scrollTop()):a[d]=e)},a,d,arguments.length,null)}}),n.each([\"top\",\"left\"],function(a,b){n.cssHooks[b]=Ua(l.pixelPosition,function(a,c){return c?(c=Sa(a,b),Oa.test(c)?n(a).position()[b]+\"px\":c):void 0})}),n.each({Height:\"height\",Width:\"width\"},function(a,b){n.each({\r\npadding:\"inner\"+a,content:b,\"\":\"outer\"+a},function(c,d){n.fn[d]=function(d,e){var f=arguments.length&&(c||\"boolean\"!=typeof d),g=c||(d===!0||e===!0?\"margin\":\"border\");return Y(this,function(b,c,d){var e;return n.isWindow(b)?b.document.documentElement[\"client\"+a]:9===b.nodeType?(e=b.documentElement,Math.max(b.body[\"scroll\"+a],e[\"scroll\"+a],b.body[\"offset\"+a],e[\"offset\"+a],e[\"client\"+a])):void 0===d?n.css(b,c,g):n.style(b,c,d,g)},b,f?d:void 0,f,null)}})}),n.fn.extend({bind:function(a,b,c){return this.on(a,null,b,c)},unbind:function(a,b){return this.off(a,null,b)},delegate:function(a,b,c,d){return this.on(b,a,c,d)},undelegate:function(a,b,c){return 1===arguments.length?this.off(a,\"**\"):this.off(b,a||\"**\",c)}}),n.fn.size=function(){return this.length},n.fn.andSelf=n.fn.addBack,\"function\"==typeof define&&define.amd&&define(\"jquery\",[],function(){return n});var nc=a.jQuery,oc=a.$;return n.noConflict=function(b){return a.$===n&&(a.$=oc),b&&a.jQuery===n&&(a.jQuery=nc),n},b||(a.jQuery=a.$=n),n});";

	public const string SafeImg = "UklGRiqGAABXRUJQVlA4IB6GAADQ2wOdASroA+gDPm00lkkkIqeqodKp+VANiWdK+rr+B4vf2nZtJhwoL24YFOez5SJ6bBCOGyZsXXRedMt5uQ31ef7xz8qR69NPrLPHsX4Dl6nf4gcDT65noAdNJ+3OUd/Pf2k/I34I/Nf6z/deNPpO+be6nxoYy/gvB7+b/n/+b/mP3M+IX1Jeff5x/Qf9//Jewj+R/0H/Qfmz77zUD+d6Efud9z/5X+a9Y7kH7I/wX+1/73uC/rt/0PLf8nz1X2Av5//dv/f/g/ZU/9P9X+bPvy/SP9f/9P9l8CP89/vHpn///3K/tx///+38K/7Q//8ZaD0KdAv/AoeplQv96hYHsqGPkRbZfBGZq9CnSeGPPtd9h4jE371cf3aK1ulQ6dCi+B6xwVD1MqGloNY4xl22gPk4P7kRXBc8gilsyY3zfsGi+B/CQEqHU67OIhcjQPKStMECPMJ+OjbKfJAMxgRkIdaEqGQIbamerZjfYcJK05EuYjDsBBmJ9dYdnj90lLk08E7EAOKS0rVoCeo+ro0qHUyoZB4qhLiPuhUjX/rj7i7g8NGXMZUEZaESEQOiTD52tebbgKO2k+S1ZRjfRfBAArHZrdriQWkISry6qV/4+UviJNKbeUOj/Wp6tASodTKoTgcJPAWyISyZv9ggDIsaIL6i/q9pZVn4GXVWbi8pVcc6VDp4UnHfp7DC3DUiyZzv4Su7nnxCjNGH/KiPvwi2JKBUbMTS1TckdvYlsHQDkvxfEumv5HvCwJicBtChUlLt4ILWoHQCeYqYs27Mpsop88yoyTzT/abA9IcqE0j06F+I/q9Z4dL1llCD53Jax65vsEnSls9K586JEjwgivatp8jtLqSoL6qTfWCuLbHo71a8Cfx5ma4ov39B7987n+6lKCxYfWwk0Mh0CjzpybIOdmR0WORcNYaOZGFFSOu2wahfUBLzzrmaDQM65jnECofYrK58TH//FGFdFPmZwZjJK/vfSiwUBFBWnrNrIPuVDrHBnqAq0+xkxngsY9jInww0iEAXd4Vk2nfc4Q8b+CiB0S816nubkJGR77h/y4bUplJ3+1YDJNoyBAaV3rVAVVU6u+Q6zLsNOVn6sPt8EHeTl4glD2G4Ksu0t4w5xzCoOlNsFod3WKs9ttWnsua1soS7vpfZMrNB+GoYVx/T4PCrdqWx0Vs/G2LtFx3xbWoDaX9AfXkBSCM9SqcnZILs4Hp+xnaTeAKDcDNMABCEuzFAKcrXvEU5MrYk0bEFPIO5NqgzofTqs5FtPQ3zWAkAB2fNPARsy2HgxUDe2Cqra3NgRc0qfqxp2eVmam8gnPJxRu5yZTfo6E81j3GDydD3pzFvYgREb+WpqSqXj43iq6fZ2LGypdruY4HPD6nZjj5jh1O2iFgctde6hvutJnyZc9XkRAl4uzPqJ1U7iyr9nR+54QHgrON3StkuDvfJpNOaQ4H+F+ckKo7o7hhqX6Bx4VMZfEv0K/6AWVDH0YvPCwKDyhTHkO8myaBed1F+5DQfc44M37qrFU5VwWlTdqbB/mmV/8TsH+aYZzSf2DN4U1RysNh7bVcKH1j4QkRLYqGPoocqGJV0S8DjGES+DMIxJP9zdrpxXFuNfIs4PfJOxWUIvJajjUoSz7tgL7NXgDUKmv+97KxP9btEDDH6ecbh+hc4+MF4upEbUmw3iAlQ4cGi88Dk/oBQyvBR7Qf4XVUwPuAIWoJ/1Y3+rEXktTd9qMk0buAgtWLuWOR5xJz2NfSpeKMd3gtAESKPXegmNVEj4AeA+4gJUO3EM8UpwyLpzGZbBBdAsrZVchjaritmAYDFDzw2XsJnebCRCGkQJ/VLaJSJNWEnoAIlETOSeB+9N4/rTpUGWPpONV1Vyyej6KmEPydqSoY70QrAldAePsygqcuT2SXKzHMl5upPOAfVb42X5CW0X1cq1lsCSV43z9sb5w7KjAMlmPWCznrhTY1/ekOxZpoYxrgfrTohz9EwM3FJvh0QS1feoI0eWpTnItoKPVJuo32Vmc7RZpvKJuPKn58MFKGV9QgbtjJGWtp32N84dlRgMkrIxXjpuaqtKeqMhqKMbcefzsW0uBQrjS4NgGn0N26AvhcmHte0fLyHaUn2wOgnTRwTTm4EKZEJmuoI97hFK8RK1VDqcXSJAIT8X9kXJNUZpyX2Jj/rFb3otFbz9nKwt4u/SWkadnZ0au3p9AJ0f6mXyn0QCe3j2F1E+Sf1y5Xj6/oBZiawjRqEOjnThMzpHGEZq8WJLJSabw9Rme4/BjRgR1HFxV+IJG/DpPz1fsRh5Qsh1RYZDZS9FkPgO7+9Wcen1tqDbjRQDGFvvAqW5n/86BI6soZj9g0VuI8lqmFh6fhETHwkSSHb8SZ0gSYnWkwMW82T+xSgnFXuHjIWQOuA82pp3HLG/WzxV4C8bf5/hiJTIIupHbXJciWnGXI+myyLVUpKhsfVss5ygCEeShGR+NZeJW2jL/ss3CZyAC3PF1n9OAnRmIj5RByZJ9cZbFtifLNUe+g/ZmlfvunWeNmwkr0bElA2U0x1Kuh7+71Vh7/qcprtKyA2jllR2UuOw9XTj0Phqu/P6C5jBzASTlbDPbXiRmzbtIZt6Fh8hsfYXx+TTA3XPoYwttmC5eJZY/pt/gdTcirnGPvx8oSWw8QgdNjnaCVF4KElrI/jEQZdMXLKvR4ARKHR3YkiK9iku/wJMmp3UQDnYCXONfad8d79g7iPsQESYkojLIYinqGPyqwgWhDcmIwTHXLzo1iXctDLU2IEE0YeA+4AcW8k2OG5MiSHCdAbY5c5VIoOgyRgxDukOZjYf4SumQDpKhXaAes5jrHJbLT6bstBsOr3qt/YAvyvrsE9VHq+El4uM2Pp6L9OH2L5ACEmYN3GAe1n/Q1KPGGYnArHdhz+0HgAkaXRAmVs9gkunbFAL27LBUqp9nufZt1PsmafasQswR8eWZFjhSnXeD6SAc/VDDKj6FpKsi0Jyk3O4mt2Ix4Cxs/gQ6F1Grw+w2bNOjVvehGJn24EWxJz39o2ZGMLYl+cdr3YlWFjGKEOLgk90hPJ3P8bGstUXnAMxKIU4h3LfTol317eUKi4U7BJRst8a+ptyi1p5uoc66hJvafJpI8wM2uCPBjE5XkBG1QyK0+xdyysShOTRqy5zAn54b8MJgrfP5848RErKzzYBnZlDmWkliS3X7AF8CJqeQ2f4BDGONbDl82R8tFgn5+KwF/C9R9Y6GjD2+pgDwOPRpequWcRAlHYyYVpa9z15tG+0A/EIwwznM71JSk8I9zgVb57nu1u8yccUnxh61A8+WHB3Ilig8BpMoTLQDIUuF4YJWndc3fZt5A+0fVJ6uoD8nHW6pSxqIVNcVZn9cck4FEpHQWW521Arbt6N6a9Cfxep0n3JKptCWak0tLXCuiakyGjPImCAaOknzjJyb9pqVKbG5N2qAAoBBd+fvjFh2fZrd1TnQHOQhBTsnnYs3j64dhP47jI4e6HaJyA6EiEPKBQL1Z9N6SBoIgfBFAJRUn21kA2kFP9S0wjmCtd/sKgF7dnutpuywVVSqMUbq6/pcgX9YIzVhgM/j7fC8WytVTjzeCWf1wUWkCBtIisRCBMjDF/StoAoijC/Z5w/wNv4YRLkH/VqfkjP69RYK2yCQ5H8IHeHb7hz9eHsAxJJNxzGn7wB6mYQLYllHpaWkCnDm0FZBmqQrYH1Mh58xI3ztROfkY0Nz273hMpqBrkfIxpEVvbxLcq0k3HBF0jdqKfRe9CYTPvOGgVwSkf1JAFIKeaw9AVcZLSHh98wtp/kM4gdDRWCpAHu4f24rgkkA2y17dAZA4/KlNCYr3hepL3t5XZa5ZYT5vwyx/Xc4QlD5dme/HhBqsrmwsB5DH0U+9YgebFElsmPzCMFsfn4nBu5Z5PzpV9e5vdmIrEOV/0fFK+/rLT2P+aKJH6jKWYJ0epli+zlpGL6ImKPcofp0pxuCFgb3QBo9bBBN9EZ/l/6hd2QFSSt+FjYb0u55oorBF5wnCXjzy40ou5oNfyCcOgmxL9+mAU4ddZ9iyfRMoSD2LmVvrUgVIIkbcE3pLDFG1Dfe8otzZkvBw0XwS8+Yb0c+On/e70Vsk0C4OJQImRzLVGjhiYM9TfWEC8T+5divC6tqw4MMorUEYuDSC5o+M2LmdCEM7ztofR4i9mJJEGa29aIFXdhn4kP8l+MMfju/blyF/SOgu34pQkW3i+ymVCbbTKizK39yiAhdBYJJZqyYmfrT44mthl3L9KJocip5J96R/T1ziDfwRntRv25VQYJRmi4qhvgPZV2dCoCOYiuFGF7sB7dI+k2XYlAUrzkaZTjiWycG5dzg9QjdIDCWlisAtxrT77k0vYBR2iSa+2xtyvO7U5+KGVEgSkiSCDXoRJoBiPa31GOCweHl60dW0S3YgZb1/I83uEd7/r8OGj0bjW5tz4CPhp9mvnCoWV3+QgiQQYhkEqoUk0XrPP+rRX5VZKqkgWfSsAU8K64CilfjDddTJAtHAIWI/PDng6Kyz6sSNVg5X+8DjYvU1oRobj0PP3AS+eSPF0uVueumgJNOdeaJuYRcZiW4PfJQ15LTEnA9OXkedPyWpzOQvtCKWLws1IPYLAhMm94SK+5Xf/IAmcd28+gXjUgMJahRpU8jgtjsy2677WZeU3HGpIgwVYi8FKNsqES6qs9zH3M3Uk9YvdpTYNDCyJXC0S5A/4th7TGuGrKf20JtGavH8KQKUAIOxLyatFvykokJBenrWzvkoakg2XYnrG2TE9FMdnmvqQpEIVT5a8CV5sJFK7Luz0Ug5zPnkv86fc17zLyLGGu2SM1VlnKdGboqQFobkq2Jktn9FArSY8HYPmeuISvHtfoz0ijCMnUMKUQAk+gfBBkb1CXtwZR+tIZ4oKhLu0B/Y9cipETmLC6QfjVBVI1ZQS6XcFyyjuMl53YJ/THIe9/IsVjpxTG2KUqRZhDkqEmHSRBSB0ibSUposbMnEIYGaiOY3txjm4P69tj52X2A/ibC4cq0fraLCes9AWgKUNayJqrUDUL9mLgLSmZ0Y3iNQSvgsDpso740WSO4xxIBs3SmPZIB5jSjlQ6ZnGmP7vAiCpSdgJ4taQl43k5/Wgb2dgDYwlqFFoK4VALgPr7ttGD6Dcpk2BufStNvojFEZbrT6D7pXFkpWP1Aj9OLIa5YYMMxmr+zIEVGBJUApnbr81EmbDsLtuZg88WKD/LN2t94W6xx98WzcwUOkkIPtOgr/5IAUibLnlRdYnkiEIJUzJ+4zLS3Y2vJE6+WcLoB82nWWOfjY30MFXYAi7WjpyikjIGha5f6Nnfes40PS95nFvyS9KdST0cpa9wfXqme5+4zxgRdf4Xv5EHNy1kkICW6qW8oPGyCFu4CIULYTwONEacGV8JKJz415KSS0NjdXcVfm7wZO3OuSB1C3pCQsjFVUN+6PiPbfQpKGCROSSjYdaN9XhMtcBvHnn3rEfN20WdM1rfWBKiZPepclb607tKODwZ/tf4qYv1X3sWMbkmkexryAlQ6o9kxoACoXIKC4GxWMgLUB6A7FGibv65nKIVNeIYFK7+girBpEzbV9ZEINRtHTYMaziH8N6Yp2S6zTLTajOxZuGMns7LFShtzHOhjOnSHDTc1GUufRyKGU+xT/Wj+YtWdm0wT+ehlNlB6eVc8r/XlLDZq+FtrQQgcmYluD3yT/tQ+QLX2C2Ls4tZAed9Mt197kjYJZ5/q2V/EecXdccP+H403jdBMMDcV80Oi65dxopLrssjk/YzChdJRdhJrq5RkGQl+exLFZbKDsUFmWxcKv8alSLvnmxByxJEU00AsKW4wdCRdhC+3WWgZ4VZ3oChvptOLNnQoUADTTgRny1uSkTRsXIm6YttqgMbjbXtlyaGVLNaQ1DRcmjnpeI+5qxQzZCzESGqSLa0zVNivA0bcG3vWjRIugql+VHtpVblQMRmBbaYckQgDlvecIbXbJ8NEbFl5Dznx2Vdb0e5RzS1zX2yN2Qt5+XPWkxTm5oPVBMfxB7DLjBiBm6cKJn0IuGYvHZrXmCy3IIYlyTvPee/pWsvDKteyGro465XZ/gXOBawSJXlfzcW5J/ycRiuC8zyrIPfG2S09o7pBXnVfFLZDV6SBfE208uaza10ywVBZsctu4C/DuhUcXjuUaFR26t093KZTuThZj86lnHYEjYs6jUiB0JZqgIFddPXveaI6xNc94b5NodsWS0C4g3G6hOZgEC+PrSQqY+/xcgdTAPKHiuKBBiqIgy/XYLl4oyDkQHvzxB/TyYVr55VUmaKHWCEJVy7Zc56t8R4Xcl7ZbwukzDneQ4spXYafhYVlZpZ6Y3/mIj64fmqMe/Ejw4FKvttslf9R47lH8W2+A4cbb2b/K4Oh3+/uH0t9scIP0UrGsOlQcUQdEkQStZ/9zcbZLDP5phq/FkaNBaD0FRwi8i962Gg25BCIfoTIML7131eaIrj2cSyXxEQ5ce34M2Y6s+USj5d/5yYPTHYj0apvq173Nn0eA3JzEIk0lPUcPzElo5f+U7ovIBHb+JoM71iu1KJl2Z95j6kKqg6TrKXd+2alC9pq8CjKvZ44p7mVK2PyAfMfQmIxS9OQP0UJ279Xbm+WkltebehDtvFHrZgk4jm0W87tlV48s6+eiR6cwvTa+qihz87EId40RllDmKEiSjzQwCT1B/oR0nSp41S02Dhu9MIHeDdaaZDA2Zde6TvNcbo+YhvLzmEB/LPeK/TobVJhfTRKHOKdJCuCCaXg8XJR1Ox4N5Cl7hff8TQlWSxIiiCQNimFukmr82eaxRoFse8cE7ngMAZtdxrLcMil3j3kewe5GemWHARi5u/ZdE/w5Nkb7QvRCaRKCQJe/uEWUVW5DUlRRBLJCRiH3/sxDAPtxrY8X5eLU72/JdskicA4Oi2QgeI0n3YqgNmr3vVensJNXS0JD0kLz17RGTg8CkqxGS33gqwpTTIY7mrYBrBXZWeNWcqCytiPyT4D8uLHNzFnmmIDR8KEbdSF3eQw0gSw6ZqRuU1fd/GUVSuT5eoWRX6mMnMdn/Dk3LICfe4wy8wMbnoXf3uapwkatp4410QWt3APSV16vKOrhZ7U8gZJMoiJE5W0w+m8cWcO/q0kZrIYPpS1Cm/CFqbgcUiTJCbknjxA4H9OlyUnWa4YLnOXYEKP9qvX2E+pgji3mOdVJZPUDoZtbUVl+cvpM3sIj3wccz4B2VlDAdJQdY45on/lh0+k7Jiaxl/n5Ar149NhsEsv1iBv0F5nf+HBxzpAdC5sR/PvjXuARf7xuA9UkNXJ+Bg8oqNWZi8F+t4CkTaaETHwCoVbSO3FXnWZgQBoJKKtXSfSudkGxqiMN24M3DHBIb10aHMRN7cdid6mhp2w2TpQUib/K10MP8ePhmib6KYP6uzFsNL3Dng6NGiLUNK5aqgoIDIJ4WNF0vGFstOg8HuDTZ0aI077xwg3QqmmmEolpWSPc+6p2A9nSwvwMDh6ZGBZ6lvBopSt+m5e70hctNRJcf9EmlGgJRDJABfD4ph4ENwiIp8c+V2Ij5T2VwTdVqcId1T2Vl3F6zFRSU/6TQrLJjqPpqxAgRSbDk/gcYxXg1wUA+nuBP5Sk3yyrlHwGyzLomkGQIEBEI2g4wPzx9eRvd1NKeL+FJtdAomL/41i98cO0U/td7WlPI52TCsD0lPx8wJtE+9Fd+Q//2QucpkL1kpKyIdDUysmzyyIP4R82AXVn2EXX97CakdoqjuDbZPs4GvUJ2OoNQEwtwrcQfaf9Vr+lDWM40Bef4+ZRXzEwkdFJG1iEvO35MDVqgbnABsE0saMlRH9ZZn7JCaN3L0ioqN8dQRgLqnkmWzOj0jU5Wcl/kZn2znzYDMpWqoje63VM8vnk2yGG6Knz+BDopU204OMQJaHdO7lUsW++1MSHfCQoRrlqdV5xd8Ar8hizKwuKDQDuSpsFr9f6yo0XFWNnd5+XuBC/a6AR2MXfoB00PLQS0YbM8JWF+Dz1yOoLWYJ5HGpLkv+fuMWLif5l7bMUuXC+4byRZJk66uROCKO9zbCjuMXqtpnd90R/bF1qyaCkzo5plJE/1lx/L8OBkTl8zakSNGG+dNJEj/LsEVzCefHpd+V+bYN9MxbhIpNIcugmpR+D7OGfvBH4VG9s+mp9zLp+qw/NBu6+sCz4Uy3KkX9vCeCoL4Y11gOL0W3FqimdCJRzEFOh8rJtIJvA6GQO6xmIEJxCvtmHWPq94iPozdAKQIq8+M/RoCnN8BAVldqWFHiAJLreIGr+/JWjFpMMnE8NmqBl3MokaB6VT8eWoWGM4aUfi1qStnfCVD+yI0dZARP2uHyjFuaHOzelFbpyK7ghIzl6JBB8qMitOaui+HJb2i7J0OXHVYmUt+3WBpRdcY+tbNGgruV5+JqT3CO9JyFEW9lvC9/Vm9gwUYF7TWl473L1upB03h3nZvhKyRCQE0zJlb8x8zIGw/IiAzkoNMwSey8oM/ygADIsPnqKRcFcrdyDUK26Aaz7WFdHR/6mPYG3QBs4LVO5WQCgc7hqeGlbd2uD8hJS2kvWzzPFWV5ICrZ3P8c2nVaP14yVChO0XHWdLZkWE1z8gJtceSImvQ9g0cx+5n5iJbjqHzq8dH1JNBR0MvWY4PUgFnxfzoJricgrWbuWOrMG7O+2rQxhOkEr2RT4QZXRhxzTJJUxe6HSBHOZ+5zV7bsNWAoMJGKo7PdOIeN/VTgUtdS3z4DgeB/UF7VM5hxbE918zyYXwjpIlAhvKFr/cNIIK/4fA2/Gqec2GAjsrrxPoWKBHOc8bWnQ9qSOMEG5uaRPyfGiHeEUHg4GeIFNV3urx6+W3CHOZoWVYUCeF/WlAtBJVuZ1Ld/xy61ZXzxRwV6xBWZ8C99zi103tJ2evvyjhilI2q2TNr3bF6ZoNJYThYvwDT9yCnfVrmjeYwvyYaezeY3mAu3Fl80Pv/ImvKpWB8VuBYYpQiERE4OPQkh35/5rHoz3NTCSyeUlAhdw8p0yjk4fUhYnGJRNml8b53XYm36+PU8vYY/f/urv1HVT8PzGTBtuiiHy8zCTqxiHEyNtEdk2SaWxn4KtqBDssbRmznfmdv2S2YYVzhRuk/oyxEP5Vu6PNHT4iIl2uWHlpi4KDUqb0PgjI3H21Xl4RORFtThNkGHo+qvvQc/sZvc90nuHsusTV6FP+qIaOPSWu4glqX+PyrwMDbhA3TzUj1jT5IZGBZOVrmuH7EVzNH7L2dIkiqyaJG4Y7d6ZiwuZK6mWLDcXy9dWfwJesq9ccpAeHAQc4dE3ZvFbhzFBMkZDqHQtovyECKFXHSPeKvEHFDiQHBIwwMJ4GLj9RJ2luQUVo8/ly2HrbOU88U7fwZdx0WFD7fdC3gWzgYkGvivvMteDp6Ccn4csrW5dARadiHViu9J9fjQMzGz8+7oONWon1So4dDiS0EYJXxNI9SGd3w2Jz747pM22eHGz3HIrq8YWrEPl1VD2095hp0lTYMXKrs08xkvrSxP7+qqUKf/SPtSpdvjOTJNJZBYjEmYZ2IvmmH7bQIVt5xG7+yMhFedmkxZMrrzT4wwf90bfdZs1SQ5edPp2oDaS0oJwSkDRoc8BHH2CIECQKxc+36xu/NGReS8SX1yIln/qIgH3bjxiiXtPb3z3UXZ6+3lZ2FdrkUyt+62jdd0JSTrVgDXqU8aeIbQ5hjRaUgsLiBaGa7TZfTLc6DTwcrkLFBCeV9IQYrH8AjBXuVltdA5+LBcR6AmugQZDcBg7SI5VibihrKkpYs4WaGHM1i0K+28wp8HaIJitcQ31drezbXBca1OPQG5IoynDnXprAxvTFxGQouovT4Qy2i0E2Stcl1Njl1wW2hTdyayT4CSHmNuay2xtT3KxRNO4rb6EPx81q+DMAMlBtC7tfUx1UVF55i6CDUS8P7voPK3aHejqM+TRULFvIYXDSadxvhoNKOy7uQI/WwKuSAYG+AcpecV+nNIZdeLu3tos2BuDAUrcMqUD425tagHSQWCFoEXLC4bYSwFQooO1eeeomgLPs8chWKfRfeav+bqWDY9D3Z7yrlGSWcZOeRPiUSx0H+fYCZsr3qUGA44dIYtyq7QgjKdFhx2cGTgvDWlqF5dBdBi5oNh4oVWpukk3eZPJ+PZHSlG2sx2Zp8hH7HOueAvD50hk45FJXddKpYvHBtZEjDwJp77Gh5BFmXfYfycwLLiCY2oZb+3voRaTCL0PHaNKq+FJ7ieY01FcyqbHFs9JPZo8Mzgnt/8iwNxFyxcbmskNcBdDSZL9PHF1yj5xArtLI/eaNY+SmjoQekHRvFkE2mayeC5oGQaVs6LSfhERQkaZTm+iuxVt4nZTUq4b9mUvqBYqr5iIgPEJEbkq6tpvC1jVs2G0MB+K0K8bPVM1r0u0ocVT9d8g/HDipY+xKk+r6vqKMkfq+ncx3qcSpxU36ozusmqLYGK7SG7bA3VhQaKtDh33tkjWlPebo6gz4p1G8YONa7v6dbajplSOWNgIo1wzwfB/l10afUP6ab4SOht4PA7G+wdoLnHD3tuEBONu8XCmznQ5eCQAA/vap/w2yuMB6otBRP4WY9WbgH189UsxsBzMBiT/WjYfe3PRabOasEEUByLES4AZR7EE51IUpeR08e6VjtOxjGY9vBT4GHFDXggpK0VZWXCGE8Rji47gAAAABDR7VuLYZtnICJFTrwBZQNrviazYltpUc5oDcf4ObniNkKeuySfGtgCoui5fMAAAIYQCMVGP0AAYii3NnyslZkIRryyHqTr6d+xtACQfjJw11tAAGutpGAABHDHFKQ9mGUTCmPgxNU0AAQEBhLbkhwAAV+rvi2WltkZUueCXkJADvljm44C/mUDKjQFEaaDEXAM4NAAAEcQAQafXWhqZUAfMEz0kkmX2zgpl9mdM0/0AMiJ6tkHAAAFJAC24hhnowle++RwehplxMJUfzzuvikAbK9OHxHXMcEHHuE13fmjrT7yDpYygW65cwjvwFMqsqVolyYcgTQxYAiEItOZetcqaR71nowIA4B3hNDmM6HHn8inng6Z8wknml9pceyEwg4oTW13u8BioQv7IFMaa/WKfgN9AHRFhZIEoYYAgm+qSKMda4YG2BpIHBRSNkBmUGZWJRmSW8qO6mKZRihQnkPov6ENmkjxcIIyGAzQ+gAQnttAIF/sriv5Ocm6Pejkg+boPTGZrxQId4UH6mEVJA9iMk0qcVPWZ26T7bR+2XXeBl5D94QL0LmFP88lqUg2DPP0ZbQ3KWWF9VUMYodP8u7tHB0SNY7XUz70wPdluzD3CXy9qArf0cUnuBgnN4ElrlzVYP770j/etZkPWubEluZQeAHe5io/Rx8HwBf+GV9pucQDnEnjvGkLirCBkApQvSym1Ff4ObZ9q60x6shMCoyWSJwztKnBHu8rTxGclCqQ8c4+g14YJOKxCgN7q3wtNwakEjXvxzSiTYhAewNDvmc49oL+M6pI6B1Lh10xaH5cStkYJbRyUxgPaqf+oS6AGbxy83lNaJEhYwWghpu39Yuo/uMQUkG4NvElvz4b8jnEHpUYbESkH1CB5SEeRRnZqF2knbGOBSPqM/BhnNaY+NEX5dMWGmzis0S5sEa8NCtOdnJzjR9txy7NCsNQiRJGD9FcRgT8ZWEDuxRsIsj/+0IFXUSze5JR0fcc0pGXv1aQEus7aiE9eo65oG4qjw3dXCCAAWcLQc1frDmW9DDOJ66eiy+FaXlFigEHadcWh2SdaAweVWSctniEO3uuYAAAAIHnE4eVkjHc2VmhfZ6lolSS1J3/exLOemqu39yeKOmAQLYwBeMVfTSNnHilnfOSmhyT8Xvg2/6Xkx13aRrVSJdJ6LA8Ed0U8VWiLiLleY7CXZdxELmmA4Gg9d6QY8d/TizKaepxHfTUeinYxFDlvsNx/NgfmgwSgMVeUQm0OWQ4OSyEMcKnqvn+bBgd4bm6SVA/HW9fh6M9Aom3Ji5j4p42o1hlH5YwlLl+Kma4IezvZMSMR98fv/CY2/ucu4X2lvv2vx6LqhOz4gJ/flZaRclBZg0yF+V34eT4U/cwuujWWl9e+F9L9cq0BUwlgRRZRyIjEthikwyjT9BVL7ufLhncfZuTR/uVRZIQrGTYVacbYNc9GhsB+tWKpafGz1sCBFzohX3khodfd3I7zmnkZ5ThFSDemlDW9uOYF5wVGF3tt15bCNJ9IHO2Wz6Ebm8vAQywgn097SY/HLwySCyPjUl9RMsxWuE8BO1D07Xbw5V15HSrJw0RTrTSamlOaKZvnwAV3njgB/HSNzPHCiMZc9nHWEFbNwBZlKQYwBG69fmixDtNxLTjmBICGT6S4ju2/vW7uwLUj1tIxWbuse9sptGJodSLvsBQRVhRUnaZ/VSvIvzbNWQNlyLTpaX2iWrvFGOVJ/WGuc312lddbo7YIgbyt1gEud4YODI5PgoFj7Baoy1t51YmiuggvPW9WwWI2hE8TthD2OM9Rab7/lH24tvtGmK7ZI0y5wn+VbS1tQuvjikKqN2EwCRDuT3EsANJKkqfSCAIWkayqI+jNcnJNetLKPdX9eT04kdlNDCcEuoetmjCw+6TqGWaPdKuhjm8hTofCOvmHO+P7SDPz4Q0jKqPuNg9Aw+COeG4K32BkbASNTEucXyBm7kvzXFLoiyMsfjhLQ+LkmbtNB1rRH8ClOJ09VUQkBwojPGpT/zC/8zFEbNzskEec8vaZbLzoI0AG8A1Q4o40BI58571bAbkmf/IRvzn8cYH6K42cqUkQxV79edZ322PdLwl2+11KFTwfGwZX0jC9gz2dieeIU4J0m6x/4+aBI9F+gDk35L5+w5y8rpuq4FgXDa+FE9sYyT6Cgu3Krpe0AazfRdmcXdQ3K7EVI9Y7/Zun7sXL3hfEq16iN/i09HNGn4jIl2mwPYu84YFY+pc/dGYxG+qO/BDOPhaDb7d90k7dLDLQei0NF0kv03/ZbtRJh4F0dyC6ZeS9d+GKgVwddfehjZPWO6llNl4/8pxKWlYqvPkZ3I8JoyOLMv7GAKV3A5chLo/8xboSdzRTERj7XJLLAXgAAAeSDBAAAukDusXSpgKRhxSg0EGJNyJjW1X6+e+7wpgCS8g/RsTT0HxcbiBYfF3Zq+i2/VDUqnpFjulQG6inFeve3OhpljsFSHg/RQL77nsmLy3mznvWMW3qFfgGGcmh/QKQNYL5e1NelyGkPKcgEbhSazWhIgIgYSjJMqhZOBasNBKXCqxsyhU7NAcq25RkIJefNqO/mAQvrxqVLC1jidk5mmaKxs8skZISfD5LjkA+x6z+Ucdsa728CostXuYfrCZdQIzjUAAAAAABCoC9JK6vFa4K67D7Sdc0bqVf5nId9gq6SeK7k013Xa7slDuVdiiBKDT1fbaYWN9gW4dg/PgaMu3em1S4wXihfSU64n9YCH/F4KEaYz2DA+O7Oh/JORGy+XcMwyuoqFnnkOfppvzjRlkDaiMz+UOMNWdTsC/FGa/lUu8HtgE14AAF2AAACKpIYMaaAnJcn9WQdZ3X+g+ACMiK//75gsJipNL/3/UIAJjnTwpfRuopUqgcrhNDFtr+XLf50a2pSCzKXGC33wzY7aZXKb5C4Zx+J1thI/Ajizn3AGWkOrZlBJlOjJy2ycjs9f22aHYmrJ/CEItRyzkPgN+7mYDJtdi154kpDeDQJXA/bAqMhImLAVSX4xXNIxYXcy0Ch5YlHRTXg6OOUp/lxXGgAQ/AAAAAAHulDqO9C1OI3hyZChcbj3X+Nyg9CQf27qzKylkW/udbIj2enGOgFSVTmJLDFw4bZ9guXycXXZpvI63rixo/409mX7znJXDgHGpacQI05wXFiFHvcv+YztUvppG6opWu/BwonsEVMwB4KFh78bqIqToQgBWHvzgbLs6Q1cXBTvm53PrGDn9UoU6SanJpgpYLvtZqNBmKCrSc7I8J3YlXoR8GPL09bQQKNlA4AAAAACc4+jjGYhEuU03xsGH0r6bCXnR7TnEw64VdcMyOUL8siGRXbkflS/tiH+AJD+IedZsAIqoAB8v58wYHU9dbTPs4FNqDuW1MKkAkXN5Uva4hZdnzj/g/n2wN2uXZ46HmAAAATgAABTD6EXAO5nYPG+ljnUtBzxqtzwBDz+rzQUThmFyezqN720w4HaKvWh7rrBoHodEae4dzGDQoSFas7cl12ve/nGGJRAuF2UWqbKm/Ol0JuW/d++U1sr//j+Zvm+gnXbsRi4gttBtZFXWjvohdYDL76uRerNwxqNFwTjECXiQH7HR+hv5m8GAFtjotgMH7nZIYHMNBaG+DGCU2zxAWOos293/iyXn6rPLpZ+n/5U6Zz/JxWwB/tMkDj5yKUY+LE8YccmJPT+gc48vuT80/gm4avb03/Ue36kPYWFH/3Anv+Omw8qRgwX91ItB/8AAkMrPqsUEOGFsaPqp3dJ5t+dKctJKZhxCSyBnA4XhX3FUeAABOMAAjLCGlDwvvYQE3uVvlj5GFX4BqklpoJ4a5aV08wlRsnIsAEOHg/+9wV/dDJY4Rq25H3LaQO9FUuZGLd3EZyYYpXSaHrLem5wJ3YSB2lGcaife/33g6TRRIt36rowN9pjZSYxnNs0bRuXjDmj01wrD4LZREjP3n8RFU2y3uVOw4bhtqfMvXOClcHkBthYNy9AHqxgcb98pOAd9Qmi3bYWu56Y+VsCyCW+wT04vCsEosB/JUisLSAINgYvfU9c4rXP6mPyMj54cP8MNcNYez1Wv2kAm03SQZjQsx66hQns8iDb288jffU+fupmcBi1EUTVsBm6erax7+G89PL8PjzUcNpi92f+EvgjZ8c/y+3kNoB1B7GX+vuEnRI19pJ/bNajBa0+rvL8dNkrnOhJJ6PCPnpFt0gf2LQeVCYSpsOvnn5kSfjrzxsqJ4OT2IsO/wAxK29+SPx0e4a66h+XPho/VIPE330wAIaemQ1kD1RUUgH8Daj8C5/mzLtdG1fTsycQY2Gho/VIJIX4GgEywLorBjwACAeDlbzxK/7azZonDV3wnYkI9veDmTW6+MnE6Pksp2xO6341XvfeOAxHDQV4ABeGdQkF59FoCcpvSj5LCW55742dZmwHCePBgFyXj6lDkyrZ+Xlev3FKO4wUqeG67fc8dKb3GHvxN65rd4pwG2tPpZ9y1ZVomrv1ID8GQGG1UhosFFwfD7a921EC/vUCLe9E7M2FSreZV/EtCH+176zKY3wjwY6BkRq3zWBXndV7ZDYVthCCeKTJgO8t+gMq+4HEMEkgcxS05WRw8j/ZT9oWoudZf+/9swPkiYLH7jSbn4OUj0OxAAVii5uA5xtnlwXudTuLF+jErj2g/5roHO5hWzKg/+5rOT53Kefr8hzuNBzqZLZsUmuAyOA+UQN0kw3tHWTTTuWoTohheovz5LDdPDZgy07A+cnCPEYB3fFqgBzoAfPJT+/wRX+lNnPOK6gP1Fr5C3g9HG2NP5l1jnd1SxuUw+lEc967O9G67u2MXD24YRKUh/2jy6YcaVEWXIIl5tSrAviyJxRYKpc/jJe1xvWdW293SC0+Kmm2vx8DQr4DeQFFBQAACRlVYRRPOYDjW6QgB0yfoX3ZFTwY9AQohM7NTah1J/Uhd9JszlgV0dAXO217yKiThb2H6HPq6/AMnGYnhX02/DCCtIE0ub8HZtL1OUDmICmeqVhgbYUlzhVFQKANTpLfEcRd41kCJxAAA3lMb0vj2XZNcrfIAx2sqOcmGoPM1O3L2/+R56GLJYPESJEiaxCe5cy89lJBmybAFO8V7b2OFJOBvaXSJGWuShZI6ZGU4oW6AsPiGaL2mGIgS2VVqrm2LLQo2TPGwIpBp475fOvj88mkWzLNwsse0XE4/ZtmP3pUGzb56ZWHUMnTkbxSVISVSJLvMS9ImaqeLtbAAmf4aFJBwY6MM0xT+FrCudMVIu+sfjtaA2q/AGz5KzGzBQkoakKMcFNnnlCLHWIVvrrDZJPOHDQfPa3N0TQzWVfONbeTvt3hUA0s1VNFe1LSdhEUvvqqWoNPLw5bZKiz/u2FOLBmUjeXtDz3ny2kcjXT+BgX8Fd2Ok/nRxNfizU+KMv+UNXEZYduHY+Xkmdia4rcSXExZksQ82u0jrEMB/k1ZDroEi7NZqAW6VhTUEYwsIYSXmKEf7ARiDAQCGgONteuN/6DOh7MWmLvYAIvvDfXCDWsabhZkEWGjuloRM/GeK0AabRB6m8/wEGCF+YndB556+D706GUH8YCYE+srDX9hWyEm0HlcruHWSv8oFIglCYyR0/KMSV7rC+dG2AuD9kzRal0RTClzG2vaaLIJurf8PRKbIBdSNyX7i4ElSOZ4eFkEgsQhegnmvv9yreJniUoXWBq3v+pS5sXB7fmnc4JRuQZNWBFOkiUy0MVCV1a93G9IwYQoMeA9YqBX4cALm5c6TMBx/gEItphVKFDSYmlRoYcSxAdzy96tW7RIqEQ3hpgC6+AnGjPcscOr9HCVoN4D+NCZ8RJIsr6rPJ9vsQG6TqOrm9MWd6+vu8P9kOJnIGx96+9CmfDnPgVPDJb6nbefiiaPAG060DVTfOJg9aEnPteDflupz6MmMmbpulnyoX6xhd8JdQHXh2E+50wqQct9pukE0QwSzYNE5xQV5qT1SwcR0hHIg3tvPD6XXOdUgGUaSIjblQscRP4+CfRWHwOdao+ASIl/Zx7WPTjs9ZMC94JauyTSe0Q1iHbgFxtWrckSBRH+jJpb08Ze+LrO98ZXusfJpTzGl8vLBW53pbPDL5PqsEM8+DO6kKXCcFQbM727I0/PgzhMRNrFO/Ro/Y5MFIAmn5KbCQXlaxZv45fXWevuNf1y/04s48uQOD74BVdpl1aCdQWOInN7mEUoA/d8C+Ukc5uQSJ9HI39n0WQiZKI0jRKFWe8loGkycM3/SzbzCy+qMDR2epQkOic1wly39Sb5Vup3pSvTs2raQXcyHkOL7ZvI75x0c5JAo7smrMXX0Yi23j7XyDx6o+GvcqWC4f9QElnzRtKgGZsov69mJTEaJ0AeNRI5XWlmcABMblUu4sEu508w7Ufym5hdO/bs3REO9ZcHD42s1YSCnBkhVAAADsloBznFtk3vOeMZINYzuPXkmoUP9QD9o67boEOf48ZKALrQxIRh/fV0nb0g5OY1dh8m28ILnPw19A6XW4i4mGtD7IWXJME5Iq6jLcVegqNEMofYzUZh4sAYC1DlQmVDsPvivJQNt6oUX4xOmEdCRaz7wyVUdd9dc5L0lhmzRR5yTqBYrjzGbO3xl47PGGg1Jo2JEA4UVyGqh2cJnZxpBhiopWyZh8cCtqlGnE44ueiWCJcBT3YFwZyDo0/bfRkoBu46ZJZWhNACWrlw6KPW48pINWfl+MJPr+a97UMG8v2F7CJb9lX132SJCdP3p9uwr7huP8hbj7JrkQlw1GKtVet3D8ZFDBk6DK7/Ig5YqOVjKv2LEVHJqvOMzX3xV/B+zAvA0o8LZn5+H+caGRjC5EZ+u7A/vz/lUeBS5934AKU/ycCuok3AVJr8s6EWbNer0z1/3KaTRgt6gA6tBOBK3pZUuaO4F8fXWGl15ciWWWY03ZUAqoUcSprEUqxKmTUctCJCmEjo0gwmD2JUUSrZ2hp+KDnp/tSzbgBpOGekc3J1FMfvN/QU+UD0gRvF/IgOKJMex2BxSv+CnLhzqxYtYJBEsaYcAAEtNFq0CEXTfvCf2LonzMi8Nrqjx6J9DqBxkuW7z0+6pr4uoDh8XA+uVgihp4+l/cBMwLMKY41IyoUqKhE5wtyec9p8cPeUfDLY2Or0rlCSDSOeXzjs52nKSXtJFi0Bzkgkn1OOz7eBDkkmQkcygBbaetZhRIQ3X4oDZXCE4HAlwgp2/rXRXIFFamY7Mtff7AYPQXlJwupxd+rVl/hiz+d0mkaeanuvKr5dpGCmwu836d/QoFCUF2eTkXvRpBJMwQxfZqQDF/N4hsQfuRJgaUUu/PRyoaG3Roy5i5iy4mi0RhOvSc1XSrzafB3I1um9u6OrkMdcRMeJ6MoDH2JtZ/AJzYNIUFDALWhTYzT4+tk/yZaxBopDlqcWS2Qh0eU6MOkb+BQ8OcUkkCGdsjyUv2+spxrUt6yAgXL7ogb69H+5IvQY5PDtEZFp+j5qFLgxJdS3qpLbJpJx4RugNudqyFB8PQ9aBDWd/wvGJiLApWe25/ci9ZOjL7KZ+EO7n5yUayqYEoDLSt4d9lxaAAACa6MxNVQxfWNzTiAFSFtaAEqxRmsP1N6DXPfVDKeL1FQAAA3gjmFirRFJT6w74WwKEVvt6+ix5ZmKpd3QR5CPaWBZS53LQ6wnClZeVjnJg+sD35/kfI6iA7/OhfwiNnBcSadiWsAAAxgAAAHO/iihMFKXivNk4rUCwhQ/RpY3V5b7gvJP2zBmjmetr7b5NK3V4rBxwFDHekMl1oV2oCo56eXkpwLxbyImo487pi8h/Ac6vOP88TgjfQjlxjYKAXiNC11+MjgbmGrKY7yTSovSG4Xfb/iobHLsrT1XRl2wc1x4QYA/DZc97pWbusYU6HsNglPagNq7uQDk7vW26LQFKarvAnaAIfghJgh3wTAnz91mHrR4ewY/5Vx9Ux4Q0e+/rbhWLNhYN1oqA6qM6VIioT9zBCuIfLk2ysZ0h+UHPtBam2NNqWcQFmET6v9kkkADkxQ3A0FFcTVchosui9AlPPoQQOW6NETQ1YNmvqVEnSu2P8hY6rN64/AslWNtd/pFMPl9Gp5KaaXLcimBdS4fe66bnoZ8gH/ByhuY0w7Z9I2nvwOVkYupY4EwzUq3SUw3EnjDYFmg5Iat+RzRdAJrFTIykEfjyIO4BEnki9shV++URMQVs46zl7s1CW1UbLB7UY1V9Qwr/QyHenm59k0bHBu8RE9zDdoNZ8jdvdhoRzXGlSwB42kUG12oEx2tgbsTOHGSSvoUtVkvlzZyI4gULu66+c/2HdtiZJm5/fEYp52sv2IJvCt/qRRl1heiHDx46dt9jrqYlLMSaFPUoZN3CemGpPA1lONcYM3wY6qQEv/3Jgs5Et4qiWJ3aJg5tlJZN9ooPAAAB798U3cAbFzNjDL0piBRpJBSPZCL/UQVFJN2YNDj94MKj14XIU4N2jEN+gMTC/wCXxzAAQnYbyDGb4royrd8tz5bzfYYHSM/axI146Si4qwAh8jVDDuTC7yOVpYXu+AZ8k7h9WVcGdPZE7j1a4pEEZWDdEuG4jg+i9Cl9mdXSh4zll9HVD6G1b9uCxu7fzSfeLwVzGJKDH7FLU+oErcLkJUhnPYYBC/heLaSroaxRtXU/SEyqeqL5SejGRFUSNLMwLk9hVBBnZgnaD8Muu/6ZT6Rb6V74Z8VViN5Y58MDNSoEVYl40jo4kz7nN+hsUMW7CcwQGZQR4eeemeowr0P23a6Bor7LpsaQ/kerKJK2J1Lq8yVr0OF7HrAvJv2l9KZJl9fto40S3KYSRMzxCsQ/+Mv57WzQlcUnshdCife8LMqI1ZP7vLpDTprZ8GuiHe3TGTIs+qpFNg0zlK1JYFJ9YRwDNi7FqpbEVFg/SBmlpwjo+jhX+pqvsXsxI1qi8TegfPiSY8iTBuxftd95SbNDdwsR0eMQe7YIH0CQ9+JSr5c5hIpdNTP8AN204T9Z+gwtR2++kUaG0Ex7jaauCmu4plFigNVhfRMMUKk+aKQAAAg7rJAIFJX+RL4xDPBRBJ5PInFo17tNZKNjiY+J3zBZqYqrtqJxOyPDT29VwXR+Rf4uhYFXKm/XZ5gzci4xjUgPQpbcsloZXoj0TOPD1ESPQNxlV/16fabKcHt5XVED2lcDZZ49Dyrq6ss6+tn5iaj31HEzA56XcZRO+FHJ987rCRHIk4H6jzgZdFfE1s29V9FXoHE5Xe/SXLndait6+GGD4/p90Yx3yZY/OO7wHeD5FEmiyfTES36nHp0+qMIgN0Izy+xIpYUq9mFGsRmtLRSv1kRwMoHoV4l0or28tSSHtGzvbFVbFFZYInd59Nz0vpm/BCa+sUStfyNh81c4zH7PWaV94nvlhGR3qlyZv7sX35dclzIF/dik2JCCNOhkkPzbfGa1E1Z8SbXT0ncjsf+rxxRE3I/kuQTKWqNnpqwPf219+SXszdH4nIEO/G74xEsmWJ8Ps2rfwSoEbIxoViQ6MUK/KPTxHqnS1z2v5zBNsH0xuuLVCQX0iYvpcfpi89rUQb087qF6Y9L/s1Xy42uF/TGFsG5WyK6VQAfQ7zenYyPvdA8ucBRriL8yhDn4za4yaqNiPpRjr967DaXR64EUIRLb0228xktq1J4OYRvo412yBO+TB5N1z+XoYC/zWqS7Oy/Ufop1yv6sMuXwi7fFsYiFBcKS4an6boSrtssOsOHUfgnLzcf4eohfYmHPTok1IVSEN3QRAXgghZcETp1mcpMC2Y2evPTCuEdaGB0PEznW4o0O29YC6q9VyVE5NQ6HqeKBhcuJfz00smqpUwozPOHwsi6yPBlN1a97a6BG/Taal3By6u78IPSw/6kI5WAalV+SczAu6f4XfmdgblIB3c+KZdQBaJlzW6haOxPP2qaHzVK4XLkhU3+NEOk4TOzrjyBP/MYnd6ahOK/Q6X3vfDutH5ViEUWSMuiwyR6FqKFpaAxRNIDWE5nviEwpcVlffo4t1erroqCINpi6o2IxFFhPp3Ja3sHo3vfh9Hsg/50/sZna8+Uyj/Rfxp17B5RPLyb3zJqiqej1egFQ1YrLp7iKqmSGUTF/W34ujtfdGdnRfHnQqXr+ba3k+swPhzYTCdzIaC8pHChop2G6EesQfI13IAz1c44x5xCp9h3iMy5z2e0WoXNfM+CO9BEjLVJz6pa9xggH1qaXWiURXB8P7WbFiSIvyQfdNUSbT1UEeZgFX1ZCru9GccJObBgz6oqP6HGfvR/1BaFloaBJXqQehvRnseecXucq2sakxEvIYEXURLyswuEErnYft/U/zLoLYPb4vs7sAPatr5dUT3d+v3ZFH1SeYTP9KalAWr1Vcc0tOKt1Zu6vRqvcURUJvuJFGt82n2Zf556nftupuupCcK37Jj/oIOKp4FLDL8BeHrydkKuoEgbJOzb8BI/aYFu/3avCvVnPkq5v1agLDwtBJsZNRyIYRXe6UOvar4CEthxH5dOJRRxo7h5kj+NsZAr89kWPfCEDeChZ9O8rotXCpnb2v7zUBvvyFz/VzqzB+0sp8xamNgNtZMjf54iXDawgthtkwmylkuFeQ/ONnFsR8d8+OoJJfkICCtdKSG1zZps63G08ywTsmzu2nVTh2u3zguEoDoArBkJ+Twf4w+8q3TDQWD5c5u4NELr4MwcvU28wcK/AZ9yhnJRqG4bw+gvzqU1562EH0AO89U2dpi6pILNG/8Tt8Ig5e695MkubW2WSSZWl+KCDzm89YP7Yy50QPyF/4GJkKTc4IjQFZ2pwdC6KwH/fIbZ1wgrPRSk9bihlCaA1/tGVkoopjmV1RmP3rYAb7NimR8yuMpQDcxDuak10SeyM8UwFHCHtN5NjEGhUwt6HgFciGveLeNWD1WYdN05GwIbw375BkDsVVfFYo2gP3RTAvejbAAAA8skoF7bQVL8aSxvjihm26z9jAh4Az5/25hcHxdwIABnZMfd4X1wPz92grl8nDaQ7RCNYNphE7WhAo1dxmIBzkqnCngDgIg6IAEMgOrKIbThihrMbTq8AhhtXm+vgQLRTKZqQ+ucPTS/5bsxjTGLkjWd/FFeoAOqr34PjW9ECqeBiPkYjZDD38JG20AMM5jfw1YwHDVAIz+88VHWbQr5EW0ChjVxqJB5FrWCJE5vXEx5S11+DvfiOs0f05eUaUaaNYExl9wB7cRJhf5iOuhn75e53u5WJcgxgsoVqbqrNL7rXoRpl/QJ33zutJ/rEcp/Kx+7gTbfzcCU3XMAEbZht9xoCq12KnSpcqf1oo1nc9DjSAKK6OHJbyL9CcAAAAKlT11to/gX+QJpIXi4jeEZaazkCCXhMrxGxxUqpuf/dkdRau9O2Fu/C+Vz8/gvLEv47PYKUT7tlB9DrD/IEv4KBV9vFX8l53nw908YNSa2MsRFa0IHhPaYD9eeZ74B27f/wV7qPhoYOekrD7+NSC06VjhZijYHKGok/H5lQWMAeQRqZPfI1umWns5BJ2cKUw4AX5fAyWxNn370/KDApsNcDxJr6Tych3RMz7jFuC2CqwLMM0pkMfEZIfTssgAHO29WHzKu+q3Wytng833UlE8rPBxIM5++ha5L/ug8tBsTXmGr34EcmDLA5Fbm09JskMIumfxxdp/SGx7eiVtv4sLLGXdoknqds+07SphXG/mXgGlwPtLsDk+vF1AqyRNARqGbibi3WJbUtYbu3sndbyeXnsyci3HluFmhxJqmf9pR1Df/MTBC/ANjH9LPmwnoSZv/TymF7UXR61k08zf0HThRkNzEZF6tZSBtZLH/RXwJ4Awm41DX+CTI3o+mhBsTEcY/nOF3HAQ5ptc/ogzws3+HNWGBFOvZM0Kzl4gRB0MoOTKDVdmiFAgAo/1Hvb90J+IgB35r8Oc+KkAPBvK93aFkudPGPN0sIxtco9U6FMAkpP6ybLV0UXzCtSBU1jY9pSye/2aG0SzZ9Iz2mD5GG9lzx4uQ8vLmuaa5Uo4Jf7/GWoqM+fS4+2FefqHkrkGp+eTBHrJealcvhoUuZGUp6vvbpsDlFRSe/lnW8f97nfiKP/iG/x3Deuc27U0NgTTHRWSIK8Mw83dnvj9zn6IZtEeSYm5hN7MX5oit5C5DyX/8kA8E1dThxdMlzWrRkpfJc3xgqRYBUBUSE6C80Y8cvUoAwVpXFgjkb50uz1y2A3g+38I/D0iUhnJwBpocGd8MY4iledHCjPKJjKXeBHCMZDC3aV6aJoaSz+5XWocmT0BphfWKnjaabbjg6VrUFixsPs72ncsQkQaghYulsD/2wvhlpUxSq4H/mdk4NPeoNz7wcJ+t/ZMnY/w0r4nlRQ/OLS+zPBWC7RNMEl9AopxBCChvcSW4MA486cqSGcrM6vKyl8+FdLK6K9YBGer9kT/99e+j6ZYzygQjvya8Nb7FAac8F3qqM2+2ufyn8XwPymu0eZocIPNdR56brSZ70kNoS0ga3PtyCo25pXzXOQgbsyAR1kYmlwRIoVv/Y/7vu8Guy2w9h06uBKW6mZ7s97rSUM38kkLde0im/FFryZEXW8emHIuqKjqaYHVUIpuom8OsJUe2jDeR3x1DtRbpLp/k7Eo1gEQmbl6z9tcEGoEQRFwmXnmoj2PZUlUaaywMPPcAHVgAP6AqJgm6oI5p7hWv/obneGNJLuAjCnCF+UGXn81CQUk6iHakjLTQ6sjVIrRSwG3n70FSkVH0kTuxWu3Y2sAiEjU4LGPSboQ3Bs7xaPMPrfmk5AJ1uYe+FhuxyRlmbUM/Q0ws88T/Mxsq5s+p6gWs3KnF4Futl3obm8/dvXWfHFlpssHCdtb8H93Rdw3pQGoRiRrCBNIrnYbUqpPIPNfaVpd79oxA2ojkPz6Br3Dn2jX8J6uKu9W3DGCyFGPO7ycGOSlULV0gqNSCtKhZWAQXiAa/GakYpf6zgM+IzqmvBW1/9BVRN5SBHHZ68XRiqbbbzofQUHND6d2+lO5xrN1Wnv9E2ronU81rv3p3WtXElOKsCZKfC8GumPg+BvDYv1NnJzxqEyWZAdOH9j2loA40SnwdBzNetbzb+OBJfelM4FUU2c/SPamBPkYYTakjwFOz/YLnau3gPd4NfBsKrn/nFmML+ukmrXCLz5SVT9z9Gi0OgWn7FFsaMxlfHuTSvW0VDvXQh/JKuSJFz5g5d/bP+CSh1NSFMrYSvnFhxUpgF5C8ZbDbK4stAoVF79aZNEf7x/tVf8rTkp6U0gB/bhXEtUtmq5vcEVTuVUm3FiFospWKHoSx0nol6aifCh8j+y3QP3C0oVQthPXRYMYo3cj5XlWZZBmlG2DEPoIzIUdSwt9Xbsgm6fNuJb4v07h/Kn9tdqQoMV5tGtpfv55fwhxrrbMlDPVgRTQnyfSefr1Cq1ij+Xor3DIuAA9DvBmCy2KpBePOWuxeH5zcDX2wcPn6YztCex466avjusmDuqXO0ruglcRib5rHlLs2F2AmOcutCAMbbR7u1pe7yAYblw2eyvh7yRZT+yPE9eMqt4Q7RWsCi+CyiyeOREolQvuGClJjHBDbNp/9VGgl9XzguIZTzaARfqCgP+lFXwdZIsj2hlxE0cXnrNALhCpYe8MhXNLfHiHV6vYv1Cxc4TWCicif3KsVQUqMLjyujZeiDEyGHZ6itvhjHWrM/v9yCOhxkagq7B6v4VfBZu6C7woETDK2RaFYj899YyqjiJFMYDf/Dw8Qr8J2vB28d44HQbaugPqb+G1zONJtCsONq9u69vb6A4rcpvJqrN8/GMZP/RHtFyS998X/BOpo/sFG1IjdGCHkKTYhDGlyka3HLU+ZP2cK5jkckrGlDeH6WyKShzHUBCdSTXC3h0XPphlyk3kLav3MyJqGbWAhG7iYXpp5ibXz9cTTqO96htoIjMRQAQCUms9567A3XEjIhFGKscYMFxYbHM0rsHncsYQFgdelt8Ec7ByBsONTniBWPSUHV1AxCVYLKAGDs7v37r9VOgy8oZL95bxLOLFAGXNNW0zLNNLGfGi6XYF3aOSM25gMkU96wAcKu4YptE/CE1baLhtSOam3ubyikq5e5fEfz16b35Zu52l9xATQ3wYhOpHoc2xiI+IqNfoQ6TnH2AFM6CaIBwDMFlb8gLMZUrQHrDkoWFZCW921lSXwsHOkqGvf+4CgOohB8anF+Et2Hz6JXxsyZnUKMPKomTNDYMcuKAUb6xc5Ff7NAqTsdkLsHYxJb9KvpSMa6G6NVM05aAAZg/qPKXZfFRkD801tpmMAl3M9OAGFyrW5kVguoq03ubsxYEsbeGkj4MxhTqDu3jhv3Yxz0BPV9lEcRL32mNSCDlz5WW9DFVF1brvjRFG2ZxSb9G/7Nr09OKgYw7+lm3Xk8W1huzWpfpZ3VdpC0NNfXv7l7s68txLMWaydKhlHqoMHhCClGMONCBBidyJRP2TxLFSgEbDuuMLMhMkT288tUT5kSfnEKWsOpfnWTo+ocM8PEV97IdvGBkGSlJDrEHhEa0kxy53cnOejo+ICaiVAKBBKqMHbDPOw6nigzrsdWX3kEaEOSIzuvHleMZGX3UjDz5UedYXFm4AiFBL3eykvrJ8yPmaM+PtWGtSOtvcO3sXeiOmUEZWEMIwZ7dy2gDtBL4p2RJp57l31+wRBOWK3CH29C2tBZWnlPR3pLgF3aqyp0AYNSeHEoLoGOoExOcr3wZ/e3QubhC81/YzMMw5QyPUak0lqdSHvYdp6vgE/I60aIlKgJxujCGF+ueDlpas/Ryw+0oFZ0Rx0Jb5hgD14aPKCI7PHY3Q07NVAd6Z0CgojLR6OpdLC0VsuctH9aV0suikMjs98SVlo2IE3kAsKZYbXRcvOKLOwaDbDzfq/L0LZ0YCL8anLJsRkCgO6fNxzyUQATOVMSXpU5yfcyKBhTQC5/3a5Cq5qq6OLkbeO3h7iiy1uHd4hEtz6vFnSL0vcNbpk8h03YsYHyEgrpNSUm8C89+QfV8ii8QAy165MA4qjY+oXMruhwkhfCowhiisaxyB79iH0Cr1ST+y9utYv1Mvej8OYPSBWsYkG8IvjQzQSLD13ZgkyhSDBFtsLHJEfoAePy7PMMAFIoUe9hT13QzUUwdwAU73848OD5cZtN3B9jpbEP4cYcTCa75LuN7PQxxjWu/8rH9WBxfZPRZIvnt6cKe0SrQDg8hcBJ0pGX1j0tsB7gAI1iBAL4jmCC+HqnQ8JJ2zU3CTtbpoj/7GEL4s5m0Q9Yy02/rDS0TmmPavRAnNqvV49xL5SEQjkcearc+sKF93pK+2DDsJucVK8S1+O4gFoPHP5nTb/py159U7wSG2Avl7eKWIr0J12SkUw1v4AGVcTvykStLYtGUvwXOEPg0fvwcCsyE696PUwZxRHOfJCNrwQ2RWhLGFKkGK+3NCiA0RFYEx6cpMVFBfUdw1DeLopClOwLIpD5wgejnqhOHjxVB0LWLOu6KUvbQMZNZX5nd7d6fpzrTKQT19YYjB3wc4wlo9wuoP3lRzU/ps4AAD/g8dJzzUGzWVGU0F5SQFHvbyXOFhaIR7K1tUNntRNsJGQuArMJCzyfQUyMHjwzJv+D5APC9mY/8Ap4I6Qgvt6AjsoTXm/fF5dmSkAbZhKLQs6oG5XZkJQwlwbkxATvycxo7Nc841Ub+Zu8+o2JKRGta5eztXkSltjk0+12FOA46JyUTLMYXDXagfZyboEHcOLje3hV0jxxJPS8KdF10cSpzpmTYYpKZiViYQvFQSO6eHE6SVrwk2zAhQ1SxgVQOQvs1ub+ozgVv7jts02kigiGJfut1rztYE00rcqHuYvBwmiX9NqQsw34azsMt1KSb4Tj0n127+6FpzhBT1KGMow3Rohm0zh2A7lf84A3Gsz7okM+D6LvycFOEM+ee0CxbAozGBBJfHuh8MAFROjbJNrZTt4XEZNmAusvqq3DeLrRtOgVHU6Ex3qvAkTJEN6yJzGaLcX7OMFtNkZ4g2GsXuH0GbG0ZZmwq+cUrYxsdDHxngnf2d1I7/zgf6x/q6k7e+yqmwcSpsC+qBLbWvTOJJNAtu/h6+FHUc7epG6RVRY9CmhXgar9WIY6yie6AaAPaDKg1qfQJjkGPuZ9TfLHWssZ/7EvFC7nRQWZvpKZjqDMe3CmNlugossM5seMwRqNEY2zVTLp8om2GLP1KGXLk1oL3xIqnK/J6rq5x+ZPk11iK4iAuRsqV/rBg4K+rmoVtY+f/sZH9URhM3/6mzEJUy9yB33P3xH7rfKzkkDmRAAxWEJhy0w3qBAVgQUV/JlCz+dS5eGn5IygPrLPK7/g35eGbsYdyjbsPEG6FFRy6K9nawzE+rrOhxspCr73zXLLpJLpFSx/AVolJm0jyx3IwR0GsM5fH3BCYJ3oJFsOgd1f+pX/hmIeSGsWyvfxByq/yUT3v8M63ng5MMpaOm+jXWD5anQgrKWYPmYS+hZSgfLtME//mbyJ3tAOhDhbO9HFHZCcrMAqs9TRoN2bnIcbz/cYmWu3fdbqvFMfFHUj+kUxqwPRqFyzh4Qj8ce9qe5bwtWszThYaFutaMgl7yyuPU6nPOntx4w4ifxH/h7lT+k3/TGJQEwRbX7/9uvb6EkvFG721r88HibIhIu726yBd4tEuef4TH6yTw1Xk2hDobeuG1St36FD6FGCNrmvL/8t41yeUUieQU7IDBd7ym6heSylJ4018PD8He46p8J8AbtmammFOoJjb+nL54uDYt+AJlzyCJmYmlCk2X8KrezOp6dvnEmOCYtnsdsuU6L85mQ58UVFcIuHqLbbGqk+WSJR7Uh/nQ4+0gW9XJFl8SYauvHSgcWSWZZp6tSk+kS7rV/7yOpG95/3aLV7ima74tkmV17NU/j333RPN5346YPbFPiMy6hX2I9wnwa2UWcjUstHR9Sh76lz0aBLiOqVR179kfgQqBNOrGTQmm3wZBINblKNsnvCStW6GY/MoGI+QoIheB8TfWM6LUSlsJE2CYfhUjX5AlClG6jFkmvnd6hwGQCXFxIL5EgsEimFyynpun4pl+honhVHBLgY4nddxRCN7tIBmYS3LBFf90Dshp3i45B7Sgp+DUR9xcbmFC+Yy0EYIzOETO6Sb1UMSw9N8eYU5z9RI4599bXxHYT28Z6zE5oEaHDXPDU0uf3/z7YdPQ+bUe+jXXaHUoNmCKVArZPOr25RMYE1RtVFYDaltLoR1FafAjXr5HEh7FCnp6KKqzLQeEkUtSJRPlWjayIrv9N61th7cpiV6oyc7qIoba9ZXJ1ODBp/79xpVCm4tDW+0XnWdH9OhI18dt2fbBrjo8/FIOuzLsV8l9iKbyi7W1SD/LlqIs3elNSqQyQqWew9y3GfiMWGolfaFBwGgmthJOCO5BFg416baZbKxfrkg5V95KFfhTSDsqUSb7ch00/cBNjbS3BYiqS1Ig88KtXHW6jpkIKsy/QuyTy8GLhtT0lthC9YysbfqA4mR+bX6SC+pLpnfT0dY3mQPHWjcjL/3LL6OAomdYjbo6bByoJn+YVxHzfu9BdBkUZgIwsmwGdtLPC2ZZIqfScORWQmUN1Ur1MTqvZ/nfij7xKHYGu7R/B6xV11ODOTfaOFURzepVQWIyV3TI4kF+kl/LSTPtT0CrzGshlp9m5nDZ8GoQJIqjUmbKftaazm+ckoJjuK6uOgh4RveQ7zONeNFjLQipfOtG3GQ0KxLqTAE0Jw2eaSUnlhP4BFOr+JiG9ekBB3Kabo6Z1S5bMrMWh+YYTJlFrFPpIKb4ZcfjY0sVFClrApH6qI/0H8SAmpQ+x9fpNN932zY7EJISwAVB407mzLl4kCEw2dDWG+CJUO7uYD4hqv7/qbI3WMTSKD6I42tOrP7NJTGP6lJfx6IU6ME1C8nITwt5cdH4KMoQpemQqeD0qyayt57DmyKJZc6PnOnai8y30Ibqa4j5lqrclrgah/iCllEs/+TyWj1hMHDmmIlLkWWxuCT37vm3xudaKhURo/+sdTYAGgL7kBVo7CJGqdMFaZ80tczAzlM4RFiY9wlX9Ken9gTnALp7xT5K9pycQFbJN7XHYvUiHhaio+xyLLGKABcG3UroYIeWE1UjgbYKQeEdDNIG5Ds5NrN9n4it68IvtxLQM8hKluy02lJ2ScscBptWiuWr9GLCsg3TwwIATQZj8ShOXG0ISHKzt4o0gs7gY+2LRJEqX7uI/wybtCzJ14V4aQDNmlCB77DuE3FRsgrlyACYaxJVJy1HL3fY1vWlsPy4yLBZhTLGAT3dPB7idCiqIKbPHsRcd/72nJByhunBNCKXiNvISVTxbDgtn3CRAAAeBRliTojMnJbIoHrqXDdVE6b0ZG7LAVEegmo84bS6Cgv6sq2QRn8V2SYN2t6k4wtuR3OvLQU9NfTIiEFjqGn4uEM/hkIigzSmhSNtm/n2PR8DExYTdFc99y8gNAsuyqRgDgAU8jUZtdc/B9yW3D2o41Xexu0JVOcEnqQ1Kz3ahfmW5DAgDu7oAAAAA2oEFyYSGBZ/o2b7nzLnym8zNB6aT93r1hWj18VN9Cvpy6IihxoHhuAcC57v3mVbh6+AxH8ubAP5FGXRaKCpQtqMwkWTrEO4hTVZxRjDSsHxPj+1VUpd/fGujSDURCv3duwvhV/Fyfuq3YSPPM7Y6VrPX5QFDJToajEQWJ2hU313Xa3kI28nbBzN3dt6T9g9M6fLN3lVTrtGQH1aYdLpVV3llAIWrEAABBwABBYXXMMLXJREsApK3DgP0wy+6I0JMY6PikFgdg5eK3c815GcXy/vvj1Yr8ZBcThYTbudn0WaGxPJcwx6qxKwdK1KeBUlaDFAvwnBaZRRMHOfMXWd/xy1WtRt2VeGrocTAB+6h+o1D1Y/5hUxOigbi3aCZ2TfnbzSRlD+NBZtipHWxjDrvbnVHVO8FX17xR01qsWHQDm9deXIo7xtRMYE2+txlIZNG0JGdyBj1M3tU3ATln+C5LXoFOBxZXeu9hsG1aBX9TBNRYZEL+0KUGbqUC3ozdf2V+rXRs6J2tARJDtLWzqjIJlWTJGcM01qzu9gIVcpnmDA7LzoZ2uW6rhCQ3m3J1e7m/V7nwQEZYHW3t0KMCTNWTsKlwTRj13MAAaM3F9ygu94BeglPUWbr+aK5DROOkO4o9MRLAT+4e4jwA0TcROSGFwM3Dr/lDfaPEr9/8S8gQkCvQQQkYMRbJMrHQHUL+DqmjMNeDzv2y0wCyhQyoptWtD0EAUwu/m1cTjqChbknuCMjXk4DQcGzz2vNT+buj1O4QQ472lRgI5FaRj6rNPnQVirL56gr3+t+VsvBcp9bTELLcbyJOkiV4bRSLPicssOrR1vu6WbE4EtgACJ6D5SVjh434RQVEeSXCjbe8Q1JfC21b1ClRe/FO40K0EI0xJLw+d7X0vWUzmE3G1VnUjgKfgC9vh9gFKnv9A0Y/9Kj0q6k4tQaESSgHfN90LyQ0s6NuzzA6AZGqhC325yf97gNqFxffCqHgz+ASeow79dsU+KM7vm90yQ6YegNIy4od2N4+sISfl266FR7C3qN5DA3bd1lcMmSPjM277MOCoSC80H9cWpIfwbjA/GEto/9lqoKYS+paHj47IcvvZMwq37ChFnmCaEewY9n3tjhfz8n+ygDB15HTE7Qx63n64A0pNQFwCx0y5rey0UfINGTlKBPjqxXgv3fV7TBIruvqQYOzxMvTJFL0kKuvRqDbJ885EspZDmcYtc3pGWJ9kvcbvFGb/S4B//GFSYTMTBvaxVYazqpKiPqu7kfu5ThHpn9zsCmgwyLl5mmbNLvubCP86jrEwB083yaqU8gN3GmGpEgt6o5RXrT+GzTn7bcY+pAnzR/oaOZwA/ZuLY+N80fc8iGCJt4zI8TEOdPzrUzavShoqgSUac+IDBN682oD1UnmdMWAdiDpKZCia84YHiuc2yrfF4ZYXjlRmwYux9bD9KsEPcrxf3zAdjU1SlPbB3GgAhzUm9/DrpS736ezfxu1Z6TR1Mm5qR3dk+GFwPVZ33TmIUmYdAPpG4ZCnkAadXEuQDkZS7bLV03sNWdX7Zz5/kSseiMqU4BKRWWkcFPKeYDldlU7gsye2QllJQZD6uQ4VhVUkZgvwt6fRyUpp329l1rDJq+iNlJGSPNPLAZ1mpFWfcFPtwLoWX330JwjvwedyUsikcL4h1rDT7p689rgs5f4sICvNrpdw4MY0CfP8MocXA636znzkW1P281EWDm/RY+/Z3hmuQVgXPy78poiYbdr/0J1YpBgsFrSovKZoyIit7eF6uT1Zf/6Gd8Ghb/j/czuOwn46xrgdiLaIAMkj/uOn1466K+JW0+HxVstdre4Ehgp47Zc0wuPDL5Wg7NagbpSMEbKbGfeZaHn+xA1239f9wNSlQhBICyQmiwUkCNxEQVYjkKVZ6RboVRmD8Lnxk5zPxtYH6tvWA2Sf1be68OWSXjAwoDmWRNXL+Mw4aMUbSnjt6/RPFvx4oWhMqu/zsNYHuo0wKC05zjCHS+jfFGob33ONVnqkinAXzNeFZeEpUnRu+iZhOEkRtIxqzPsTCC4anLZ/Y1NnKoBKYIWKAW5jxolaNVEi/nHoIhXM5KAWz5yu34rQgW05A+QTfG+ks/dZbUcxDkcrPN/qxESajKsI24K3cM6wWxzW+ANNaqi9wJ3sMgL2XcuV9e8sxj0RnFLZ16wxD97qW9dg+NJ8cpVSIY62H13kS95GxBGHtCpHHxbZSbUIv32zj8Nm7inkWBUfUsI9w/fiFhmj/0ISYCD4dqj53LIz5zUkRoyLp99VjATsX/UrBwcyHEGjfv5a42svt1Pw04cYzeq/iHOH6o5U/89sPKdCMchb1g6SeyIdjJ7LXlG4p3TOgy3YMfOcNHeRbaEwetWZcTJQMNpIocDyJH2syKGWRFtzX31JAUfgxDObsc42kc7KOwyqiWEuoz5T55ZJ3N3Le9iqBeMhNSeAW3deiFu4PDM76A9cDiWSQ2qCLkwSmtZYFs93DM5UJj2BdQsgQihSwqRV7dAW2/2SsNVaWDUnRwgz7wRDVEy2hCzjgCYqkvo1sewbVLEC1QjNEHxrTY7jwZsmqqhZxDeUlBRrqEn2HYSuTsBN4MRxsTquKZDX7MlyoliHnIwov4nLQC06IJR5ou1ZklarHzPTeOj/tQ4YwvjhecCSCnpygr7lYLO+tKG34WhlxDO61ejk3UHyT1iah/MgN0GtFsFK7ieas5iIErKWKlpPQZlDNQsmAD1vkScwbV4cmgxigY+AY2t3jk2Ap3MMC4RTV/JR6F3n3wznVavUZbI5hkYdhHCt+1mqsVumptTWeKXhmZZcefG9t+fV/tWS5PbaleQBeQwrdPZ71qQXHG+WHnx9+f8XJc0hZ6qkvJbVcvDSX6IGzEOdU9m7TCi6NhruT0rSMfMif5XAGhufZY7Bgle1ZWAl2zrCX0T6PiSvbRygI5Etf0iLGeTRSTfitP8nQgzGBWePABG0rr5AHuNHNDvB2q+LgfmvNvzuXz805OwkeUemc9p0BVd3PspRnMjwoJ/vkSGG3GIZY/9Nv64nV1/ODKfRbckc8DgN0kAtMGbf3a9FSQ0z0+Bn1kP7OMmDiGOcTwBmxSEAbQdJSb1RNqeug8d+eqy69beGG79dovotf/QZSdmOJ7EUMT4pOBnAqGZRPkOipXZHEfXEUr37g6pxDuC0L7ba+e7yFFUkZEyCnXBWJo/f+riu40aSdtBQOJ1RK7U5/MC/KrEenF4ionhOzrUJiwRFnqucrFH5PiXMvAdNl7sljlnDQkLREstATVzFSaR+qub8+dSNOTDTmV1+B8WUQohpoTu4eY8egU+44Wu5rTAuy0+YZ+PqBZ/AAmjjOmAW9jysyRME58IjGpQCqKzE/KYtbPQHk8ec39MD0TZljpiWr3UOFg6zJlaVNHWJ532NJSo60f/4/u/+EMCIoMO3C5O9NjV36+DSd+VTJWp+XPojAxsEdsJ2dgVe7q0g3adb0Q/oTGLRZYliPeW3Lz+fTDoFlEulDs+fJZpCBXLtNkUng2QtRxHXjzcnXJD4qO3A5uyeR4CKGKhjM226whrNNhV0EXlhsTBki7zdbyFdu4R/zfKP5zY8TVgDsXoq/1rrucnQ+mBIYHuWB0iQuyRUC6j0MN5kvl+63ldJmSZiMQidyrRBcCCx6mVXTG9mNlthhkfQpBSdEbphALHIJM2F6svxFeUj/+V1Z22q0PAIQ3/TUiLQFHzhftnOoeuBSRPLWmN7RDcMZNH/dUagBYntKI9VPzDF2RKz2noowcO+eST2AQj6bYFECAhte5Un4MG+Fx0IrAPc1xTkdJ9onJ/a/tVSu5PBmrY32YIUAWBLtyDoD5NHDllTscgNwh0CpQsv3k8ADlgidob4kKgE8ZkeqwOCXt5bDZVIsmUg2ZoSp+V7VyzGilsB5lPUVL8DkCSFBo8qRwvEKnw2l5OnrD11POrkLlyEARTK8wkFsPZD1YCBnC/t2YNB88VyyPpgJ8DI2qfO1rhg8hMqaslJYAXGfmz6VHnjUTglcbZ9PFQtKuFKtRUnkWSHImhRr0VQWcAEj/G5x5OkMCKHIrRmCGMinx5Jy4Yee4XEXnpUUQ/e9+n3HGMRwkH471btovrmI3feKdUt5Fv0lD7BHwVboytZ+8UhOLWsPDt+VargwXJsKrJBPwyVlzrs0eeu7zJAwgk4slZGhtgIzHYi9qCUDDPoexyfdceg+0C3lWcpeWOm40rCNBPawmDRY2kRp/Fcb2MCKjPNfQKHQHeJLCFIGCfn72LIlPZidLKYMDFtMhhM/AFo/OdIo28ITbRyYY2iQw8xBZ/vwSti9gxzFUM14RVJFovU0e176a5fPVIRV2Kqgr71HF91U0RI9853w7cCTObxzSkZwg3WC4DWqLB6B9eUaR+HrPLz2xgEeyLDWVlbxIemKh4I3poX3P/VD5xBkU2fzmqLwJ+maHnP1aFVVlVcpDbv8sROSdQqBrhv0oTj7HiueWsRJy07yjKP/aNr59Wp3Bv9J4WXrkIqJT9bloGTNX8a+SZZTsndRnCovay/tiQBLJoLVDx8uRLMIG1brSVA/FFhgF+f/WVAJml88oY+3rtdqLZGfQ+YarjY4/j11AIoH034WNaGqJ8zXgllvi+aaUGsSziyWUTQGdesiXHvwrg4FbCDixFsoSksV/NlSOtIOKNuQbbCqCU540CrCQY+Re2u3TtnL3LF0RILhfBGMqOJ60Z8yyArZi+lMdkUnFZLaYPH//7lQcb3/uWHm6ZVQdacaWu4TAzFZadKg3x2CZ7PK8Uj22xbM5KXhlXXq9rS2u0k9Tjp0LQA60K9AKEpKyXlgLMQEkaniNrOuYmSm1SXSyRsrhE2RkKy2F2tDoRJDYYInL2Ff/C7y1Fs2X1wvlbvo6FClXRnZcMtRNoj+NMrcxgP2OjDNRZWWecrInpFBagAm4YQdvRSCr2szJub7S+4tqATVBzBITzryxq8+Kg23hTpFazf22Xs7/gmO6EpnxVbV+tDt2jcQVo9OTjR+ECm+J6Q00DBOggMClrs80RgfU5Akj8tlsYF+MYS95T369Mlqr/DtIp5g3wXg+f1FwYXcznupDEMD9PqfUHDo465jfTVzvNKlWhiPz2CxdvOTLUiSxxPDwBpbi0ziD2p2ddkkUCsemlD4llBhY8Wuc9SUTOToQ1t5foGkrCe1xog+fHYZ6aBSB5/4I5e6rgXnYkllFP6YhOOTj81OOjBmaCb9yFV8fTsZMIf/4RpbNZfh2PKBH994aj6gkFqGz4xMCorWf4mDExxHc/qNMeEmuwYZ5cdaIBZeUP40lkRQuDpGPGrwn0XvMD8PKV32TlEMZyFZxudgiYJpAmwqMfoHxw5Zr0H+OoOnhk+FgEP87pYe3zSHB1E9dodViEVtqbaxYiiBJUEoZc+gSGFYIWEqtGhg7KzRMcX2bPSBTDTS93vApHz/Ini68ciGpkj4SjREnxALvRbznTts77FDftn+WapaoNcN/3O2Pw1Xyfi+AuveFkp3tYdrOVfxUj2nJBccuHk6nQnaJ6T81fAnnFimfQwGR7SAc1M2hNMjMzNhP4Nhs8TAl4oujIY0ERU1k3o5qSE2Zmxkt+ehZoJMbZMTrVrg3/Sxufw40khYZCwB/0nyi+GvEBtMY2B+q0FVjY9OI+adJ2iZAwQdBpGUAnJz8DTTRBLsyCmVtnAcIt5zrNmvyzOysR0CDYCWvSOFTh24re3PH9LnroO9+nWVfzqIZxNa42NSU20uut3oFRkfqUqNW0Rdn/mYrpdJGKaLc+G6oWGD2lx4PicWF66vEEMPZIlp/WophZXRGCNfChUaa7n3CYWiVpJWI7EH3k27R0zyBRipBF3ggGsFlmgYTzP6oxqX+1+kBBvzRYOUEdft92xE61HhntdE5IQAhSSkSz1MGqjTdNOacx3LrM6qIbGdsYB2Pv4Pp19waihWZczl8M6GTQKdT4/C0hni4KMN65RfabZIqHWLS3u33YSbXJGD/lWPU/LpXUaef1fuER6gJ9zV3hVyeOXG/o3MgmXhgcjtnzqsr9eO/ld7P8LoM39pxEVCa+6kxZTle66g2UYyBkNzKWGC5dZ3MQMMY62ACfKKknKuI8dLTlxX6E4SchaGO1Nl3iCadYKRj5ZeEYYnPEuGWSk6DUk24tpuK7IvkkpUQlv+TSLyFYfZWdfbEWW6WGTNhpXTHSuUbACP/0/H75X0K0lfvQwu1B4qxKEHaVZEBYldH4+FWoRA42SB17sPV/o22tKc49pvXdmgCaX7K9X4Gwqje9cnyBX14PCC48COjEZTCyDt1mKrv7AT2DEXAWNN4XMIdp9hKYGUsbLZ0AWCMpSMH+NVReWnuiPNKwLD/rr3+zaB6pDwlF4MihRq/nhnOUx8385LZrNFiM+j5XwpRvHD3gvzMlBHUkJ0yENd6nGvhBybHfqLXbZFV1R73QlbCjZ9yeqR/j3Eo4BnUGlsBxJA5k0bKLMPPF7/Uq92G7au2Fzo7BxioDrgpFlkqOd4x58dy4uQXxawp5pMW1IUtj0dLNLTi2jcbbAKRPbBTTSN55s5bEOsr6Rq7O0wlYaJjv8Rr0aY3pTey8suGmYTRCwraCsqG/MPnrK6e2ZNZjrGGNRk69RFxQ/PDZX+Bfudjid3Yr2PKGItTSTJ8WUPVYTYwgCcXnK1TbhMB28P5BzZoTszKKHUT0XL4hOx+9pVahN6udNakGMsFDwlVmW9e7gj5+yj5MnAYpRzY8Mzz+No2vT6Cw69nBgDSeFLdNzYnM3ZvkeNH8Kh7eSIPGfHoV4YdtTKkMxeAi6wgr6Gmdr6ivmvYquh1cCLOY3SyGZB2nP4RkI06mwg74JHiBiImn3+IKFdlNCnxfJAdQN7m1nUAwC9LI81KdIZROaqOx9b/FJrXpyqbKhj+KW5Kt9SQQdV4ZGKFT5aofz1uS6ktWav3Q2JFUrzTtdcQlVgHkBGURko+TsN/D+965t52LOrj69sWMAEGiFM7DGqbDqQH9crR1/aXNi9ERFlEWmAFZBL9Db/95Y71tTnxrpGX8lJgsWW5k+f1/kJ8DIxmk/T85seSfFY1bf2HtvcCwbOFCZEzfW5uH+cLDh+3N6Veo/Rxsz/h2pfWM6XNxqunI51UiJIaFBnXz8w2TaEpoyJoYg1b1r6KVgTb+Ah5YSoGRx2/YSI2Pu5nJot3axaokqyXJfnEy7QY4ThWJelaoPVrF1t4731YeR2PRxbP5/am+U4c9XEuXfkgkTYRPMpBaCTe3NLMWT/NWzMHW+IlUjjQFHVJy+X8XW3cU1zXSHIuHtjl16WkIIHf5fwNNK4tNYlFUu2cLsXy6vNJqtVTDKlIPNkLPf+DBF1HZCMCa6cLddvLqWyXKmCtuTxr3X3bFKrVPGjyhWx1QguoFWsvlFTbtpHjN30BHPAKUUytiiXcBMByXuM5fTfKcv8Q8tgIlLsth9OWZ667R7v5Nnw7zagFmE1gAhl9d4VVRSWCRk2T4bpBC05tK6ey69soEeXaFtWYa9jibFeakKNZqwyadcRWVJlAYS1beAs4sjGqOsc+jy2cgjGfP5+ENT8iC/ShnxJTBV7LeEQcHlH4q4v7neW0ZviIqoNZVw8j4MbLlGZ+0fONJn92bA2iej1IHLZSCh6ybjHMGHOqiwqG1lnddX8AlQkU+UJ7m3C7wHUkfui/xwdetNhawOzj2cKwOiYsQw1rC4qHes9SFDDX5FpldXrFKROTWXRjg6kV3bF4jxmc/QwjEetNBVXEVvOVbjpJvA1ziW1s8eGkOYgNF94ki4bcaFLWXf/LLj2Nm+qus3xA5FUOCf/gYEm/phx7TatxMrBTjPjo86pmrr46I+e0Ef+Z9SeGUXVyRDoZAJ8uwTBUz/uczitQObXEaUpHv3r24UEH0M0xy2uwp2rUEKBG5pQGsFuEpQDqxk3OH6+haA2/cx21eSnqD0lvc1tHUXlwo1iGg7pfO9xohNo7Y+OQo84BAvWP3+ipscFFjw75dmG2zdcV2/g+f8rCAKv9BJaO5/L9nWka12lMKvaaeol7MaTEBdayv5Oc6VxwhF1Jln5jqdYSE2JcPC0FXstjZaXOJ5pKtdw7RAbYfGl7gqjOfm83YC55MZ47NxIM50pf2cZr/uFYBqGeJe+hnF2pwrJZRdPCMElNEA5OXCaUCbBWg3j9wOWctfe/mvmNEGMa2HCQ738+35wj450zPCU7lcovpvYEE3zOYa6k/lOataP46Nm16r4Jc5IgI0pvS9T+cqKU2EegFDGya6zgYnhXSkhjfYam9+otMpOoanWj4/gRsgsuEhprCi6MDfj1d2KVBbVoLV6F5DDuViNIDgmaLrtrtK2F66E8c2nA6ikiyVochy8y8FpMhBxK/lXYIcu08utsmdFNCX1ViW3pSz/n4bMAHohgIbG+Yub0HTqBOm3lmangwqK52E0izbNQ/fE9UeNCaUgOY6+xLuU8AEhMNMJf9uUXqDAcVqm7FeGAuT0bGc9bkJLulhNM/ajeFJ1hbJNcuqSgJYrCxFk3+n5TdgdMcdn0mRX8Dq8F0EAAUoqh2MliEzvTzztiLuX2jOBAuWj6RFhTydQuRpyej6/QF8o+Ox/VTZ811F23VyCpfnGeD0lRes+olns/wv+j5lou5rtsmbo33w8XQwzQA0bYJbZh7OrwGryGdL/41Omd/4IdNV4VxO+npOlRtrWKy74fKzY4VbbmilMSkv0dpS2a/toRpHv7/pauTPu1gBwLga2rjN88n5jyRdWaDp463R2ucLEdVSnqxhv7dJQlQyXnDIzyzYpBcBKxGvgN688sVH6HlsRIxwbdYCJ4zUYwXmsintcpTIrRiGZ/n+jSWJtwdeFQNChJUrMVmQj+cjJn3E4NZt7c9rPBbUN5foYY7fyUZpZrw//NsP1746VThL2YJjXkfXVybKAGAX1A0yD/jVAtq7ps1OlZPSDgqOyJRT3Kn1pR0isRpkKiyRkAKXU+GJDZ4PdFbbc3N+qS3fAS4YEP13rSyyIODzaXzfrDj3xAx6GRfHVyFz1O13wmrTkoWSw/QFUhrfBimoNitWSfdbbFl4sxyx5jt0HHm3NSsbsBSD6sxIhF3NadA5YOXuGpx106TbR74QrttSGOZ1WLIKX9NnbXBuwdEpnDfgCI+cwg6duaTE2qMspt3nqrHnGzNHuHpgynyBZbox+fivy8zu/RlDZ8v7iuhT36X5N7ilMPN7hxozEbfJ68jOlEnEQBHEkqJ71k/ZG4Lt+17l4xtEcxZ4P+WPeGBT889z11xyZzl17bKGVkePumvnyIJ9ZHgo7PD7Rh+/bD68GQMrG//4+YZwBQng8hLnvho67g6jh9DZkw/zAeQwghRcO421Co+6kNkDlNPPSaGVPif80t4Hec1BBUUPdLsXXdeBN/qZf4NYeUo6gyYWotBpsUh5FHTI0jCh4cWhKPZgC7dcX5q9ESL66jzavScwcN8LvYoGUoQlMDdyVnCk1tTPFu3vWw2lQjS912W8jJ65kDThkl9cqQ8ue+CxdCLBmds/J3Qh2mc4kwpBsyWOiQeGXtlTsZDHwTazrWTgmATmH2P1sNh6tWJaYoB7Sh2zb4H62kP0fIdlfuJDpejZ+8KyKVoHotgOiayxkOzLvhz2ClqqjpzaNMyPGsWmUjLW6M5uaWALz5cDDLvGJStau9hFvmxld/bK5RMRxXNcNAXENgohzWQUHuHbelN4K8xah0r9Py+PrjWhvBYtolaNUrupLRixP3fIzC25JxdPsTGrkF/A10MHoiNNPSEHk/0bUkOK7UdFfpgV+vW4A5I7iCsXIp66QnlyTapkF3x7x7BhxiiZ5rHpnvi19xFBCS/6pvEEJdyN+BZwgHx7LimvSUGiIMqKRXbrBwoONRf97V6BUjDltaJcyUws3JxZAC4JUE5kjmO0maXybAwIpnP9oAZKGbnzj0Hpfko/k5YSgQUQXKkCiNN9B7rw5AXIeiDDyHsHTXekgfJKWYlwOqHRsZa0jxlJfv1x41xGhXhzx5je6qjWbCMKZ9fXCDeARQnP4gQsxLtq1qt6YvqSrirSmRrnZC47AP2FuXGEcmydz3cPM4mQHKqkh7i4iJkp6gF/+C9iNKvpm1y9z376ef2evZND6u5dHwV3hN1B96blS5MjMlNzPeU/burGPXbEPD4sOOTiOePImI220NDgJOqL9WxmTOjrqs9BAhRLWRiVGoNKzUPmxxcIWqQNpa7B77LIX5M4xGa+NYSp8g89pVPeMDFWVJ5QQ8LLpapQ7YzlTf4dXcMesdyxx5bgvdEidQUGrG7aKYS5ASPkNzk1T7ZzGVXeSecG4lODYgIAdRkzeqZMN1M4kGCuNqoiAX/JHLo0Ne2eq/cwHU0gFM8LqJcZtReGTzUYaI3czD4HdoLzrsakJFaiBLk7u3pB7WvLMMmsQ8lvGLBopkvKCs5V6NdMLqT6+mlhBcN0VoB73DKX4r6k4EsGoicR/ala0RzGIjd7BAx01xB/Frqe/UcZqFW7Aq0URkK9ha8O+KPdvcw9x51hpV8R+UJ1CsqeyyRXNRQYCURtYuP5x8jDC6DMyKPHcFLoKbyPN6PoLYK/WAeLrLrUHTixMuaiL2fmNBngcq+k/NOkzxLjVKVfN+bHHix6GtAFxQE1xoDEjKjTom3oEMfm6HNLeF2Yc1JSZlEy35O5IHet5hb3/HEUfoRQO9dvwaoT4ZSY2ibJKZYhxyEd9M5SthtmGbmNS0Wl3iLBNY4rGgo2Uty3rdJ7gLV0Eou8CwcRGbcYolr74aloX46ZMkfNLXsWk1+WnyIY1VQbE5VNPrfb33WTJnwkCIIcS1xLYBLEjtQawZZ4xU73JI4dgWjSRGnMbauCHVmRTdOPRsMIn6H+dwCq/UFbkfMQsc9FEGSHvy9p30eHKPWtXhDXiFtDal4tp1bKe+NF8OSRH2eZbXxKiUurbFJSg42/CDbX23ojMfZG3TFSJ4uosURHXCXyyQvQWIpj3f7ALEw/xZodLRQ3lPdjl2/G3YZw4kYKLzxCqfwTGXx7bmprvwrDBbDTOyQkro14c5reWmgwcnadtyIXR9d7D3tykf4oP38oIDea3h+S9M9MLZ1/AmuxNAtIWkDecXajeUbqhc/exAJpXE1NrE9VtcO961Zb9VRlP/fzRhHDlbRbsF9LIzFMvR4f0Ou4X5bWEXmfLI9zBlEdvIP1XxPgSADkxfOyMOGg0SPXOcn8R9Tju6LlxGHEQuvyoS7MgIW5YV+brRPKkwTnYQgDW70oU3nt2E4EzZyTKGmBhBmXZaDX6wYVR+WgsyDIgeeCqQdTpzM8xS6mukEvFaD5PYBQLG1EkHGfMaoFQ/LgyHugCucs2lVfJFvpjqr8dPehjPvYQPOBDN2YeZLMiWV0WC6fiodVnnidYHIERzK+ESisC3quW4BZIfeGvjngxYCmDxqxay80dZcvlfgVeCvvekwDr7DxKAj2wCDEWyZfW3CXvXCOHlLE0E0cgyAK6M33Qedbn6spm7WEqTi7C0Hx/m7g6pcsdcq7QK+/EJfJN45qPiDT2uxYOZ8v/YD8sVZSOVrQJD9PFoBhvoY4Pc8lGV8UpTtTJqYW/7KfaBG1gOXtWgCqTJJhbfYXRqko3/geFDcFTmSedKi9LKidPHa3SCj7sebxwp9SJzfjNI3jF+luUTDw1EDd15yzoGX1bVoe9+IUyvr8psr+kEvBv9Wt+FmhiCVwQzt79yJy+arHiLzCIeGs+oidJATf4Xqt6H0EBfdRm3InEPkYrgzu2qO/zbNfvDp1PfkTyH+lWKsDAt2MGXvvaagGxoLgMIkoKyGJp2V5kfH+ClpkES+goPIHnM4KOXkCC0hYrUcJiwLb3bhworby19EsFOmnlJQxvOqTG3Qf3CMKL8aT4O8S0I+vauDYSasSE377Cb0Z9Ba4qDPw3RepClXqf58Qiw2HCHk0eZPrInh+VzqEgmxP81F/wQsfSJG0WNZws5lODBexR+KJyv8QeDKguprDcwyRJLqUA6W/vSfgrgJyUB+FvwrGi4g+Na+eLU4Oh1tWf4e6snBDVis8lYOa09EkfHbn035B5o2y8/qBjK6/iXFsYLXgID+saDEGpdQoJjO3PMaRtsWApN7h1xDp3VYPGtkYVwtnkGXvELF9I0K1A4GN8xDemEtzS29Pgn+0WjkCy75JzpZruZ6xfhJJzvVAk9kXNXr7k2ix4WiTF/5nbN25Yb17yJuH3J6O6aFAK+uKVQeH4aFJJgc345JcRlHMJy18KXxByjGTGC2KHUdyh6vxgw2XuifeeIi9KLgRV4ynen2+jmacB6F/nJydR9E6wTLhRWDNmBbrOjCWkys3slKk1KmhJZ07wPHco9At0Xjm5gAzrr6hiALj4jf3auE3VxzUK1RLSZ2D6N8UBqyO7bZW4RjQbdSlXYNwJ5PsOeaiqH9kRvduSrLXslQY6Rus3OIWYK1alcb1PEOWSclZfXQ+OtrAw4uThy61zAUVlGNC87I5zYY8xvQbeT70A8Zwc3IjFOKEYDnQvVGzPbpqwsmZBG+NYfMxwWI3hDrWjrJedVCDCk4dWQ58KCqkkeKHZjTAZ46n+wkjPBjVkXwOPwznHtYAvTAGWAymv/cbrOIgNLWp3Ir7BKI1OpitKlz42zkJElN/iix1vj2AnXKEUSqisiTjq9xzIynMbdJJY3gRvqH/p2hTqJWXXFS+zeFc3BBWv6UaQKQO9tRaxNKX/tQZre2aC6TrQkog8xMZKjfK4Gv97tBbIexKM3D9q4RZ7cqdyDGn/nRjylq7dhvc6ytNpL+wS1wu9DbxAlVcAJdV7XoP3mq+nVmQ0KQoTAou3Fb1ACCmXyeNwcQFiT3ZEfuT20w9GIzmnnCijzhlNA/F76yjb9xLlsMzrWuwn2IlMusqjg8JvrJ7qJryZrQAyeOY3mWM4s6LpR0d9zivALFk4XNRle6Axhlkd+hzReuKe0IQoYjzF/MZQ/2Sl5AR2lG12XPEiTYpEOZp5pm99r2rKgB9wYhFum3GJXCEjcyN1+2Nk1LaTdOkzlJS+WmaVjkxzelj6GdYIeMXLIq8EDSK1jUV3rY+A5roLw5KHw5jSKU0LeGJOEhb0ygWJQ9GFubzVnyKSA9BsXtzAYz9PQQD3yxCgUY16q1vEMGcPpkCgTbBFNx39QJmWHKe2IzlyM+TFa03obhNRYglTtf6MW6jcd0wSgpEWT+XEaDiZRs3+68Elm5tpcq3qao1QIjwASTsNIXqhDCGb8AH4Hvy3+d8HTP+zXCXJ/62jCCTu9X3GdmhmAShSkipdD38EnFYD8BrSH3ETEdfpmdD5G6JI3cB1dRDTr9m/gSsyWopoLDd88s42ylVoHq1Gr3V/KQJh9OJ4FS3tm9OgLvsObrygvMk7jYZvxJ6sVx0DCC+XOTTGY6IEWaSRyEVRZ45wGhPxpxbkXOzTMHmXdkI3WUYfdsd+pUsjBinw+gO6uA6MlL2JppK3iWWuREZklUZq0+bhvStGnv0VM95S+Ug7GlwuY7+SzHmsLN1gkXSWWtQt0TSevzexINdupOGOMnS8pFHNre2QlF6SzBxZ3KmBkrmYtg8lduI7TbnX8Vl5665kVwPy7jpMrGfnGVvM6pgSwzb7is3WsLbZ3yOMQl7Ja/z07+PAkQLKxe16hPnlX3vZLOCBGWXb2lad+VYy0/9rBbUFMcRVAVUtIF2E6RjinYZz1VyFICSfGP2Erweaxwi7g/yJDzvx4p1czuYH2s2XBR9RXC+7Ap46nzpR8EhgZGnp5zR19VGP9l3RIKF0FtQs01u1Zxy3eqvpGhNEYWq9EvC5kmZQuzIttE41rt8Py6UnPZLq5pDBz/aYZXIPHNcBtesqPkmfjN7B/E2e1q21gQ0HuNhbG4iSuwdwOEMRwqYl+u+SFd7XVkjBQcZGFmQqwR1Cb6e6W/tHGeZXeqrfwsKY0FI6xF5PI8jrkM95IdH1Sox77lc7jAnNDERNJWFbGoaFIpWXd8YsTSeDGPLnq5ch3XNluGA+Y5dyHrTo1vgLh83UVFsBpXrr8acGlneOMybyeCRuqzc62JyhVT//m3lMyKOJ286bhLZk0VU7tpApx9PvGKix7a4KMs2xz30OB6Up1um8PYnDby6jM6kyLXbvC9skFLfX6OQp7hKitqL34BmlVmuUEYrol2OjGldTDsbe3/zjkWxGN86/0QsQAFZTGj5W4Y1I2CtCjv/tedg07I2X/cSY/4ghNX4TOVXVvTYCWXZDQsaUSodQhNtwvnB5B+cSBQzn/P0+oclN8tCpsNoiqPeZ0DKGgy4yo8OEWV3oLz2pw6z7Br8w/AMe7U7rqt4VQ+KrMCJxt5anPUo4OGngP3FRIF3C/raPneIdrHDYUwSFHMNFRVrNSDL4Ghj8SDG5EMqN9CJbuopczcrfYOVyea7twXPxRB9tLj/dZyxFEPE4oFmvBAWxsrKdzY3FIeOXHFU2H4PZg/rYm+zQ2xHBR36Q4u//79znP03V4BCE2CHNQ+tBX4gSivonF7komhXUVYMVgKYZqw3KGLaOowB4n/HQkLCp/G7/6ILtx/fBZjwzz6/pv/fKtNVaJF8BNRMAyn8mYbDfZndq2k9W7Tu6VXEEmMEFu8DxRHmfnLbkskMYrZbXU1ioHhEG3Ntb3BDmL7xZ8JkmmNDAZZQokSoZzFcu5jfR56uj+CDncPclcnCMf/WK/M4pFCVsF2zPA8iM4vZreVqWmMCWw+rcZ5iHeYkcmwdkSUEiyoGgDzt8BqlyfiiUNGGeoGsFTfFyeZ5sKbZG7+K2Nydx3lePJuEG6wa0SuBTKbdis6Txol91uX3p2jJq0lkU0NlhPRE8tJ66LcFwvxSbQOZLjODAURbg4GEYaZAa1o5VH8lM4j6Ia53E1ezRcNYJ0LKIAC7AeR7TAUH4kuREjyKirlBT84o87kgXx/Nxuud4lGTL7RdxuWV9mNS8DNoulo9Qeh4NmWZo6WLQlMwSs8uEepV7ByyNpwu6ZGH29/dV4MKs7GX5fGl0K5akVSMwjVDs3LhyOwm/UcePdrrDeDLjwNB7bOQ9qa3nDJo9tCgSSiw9p3DO4ZJUfjDwnPR95aZ5IPSu9SEntt8fUbSLUC2vJN9XK09KEOq2KycbELctdvLUIT4JEc2JysCCVFLCGbOW+/P4Hjs2r8IQhMMOBW3iBnnCnNQRKVhNaNB7St2nWqwboxvUnN0YXthfd+SFVsFNRVxO8BHIFyD8dOw/mJ+/rwykItoOCiQL+guWvpYqgXqEYOAnbgR9mPx9KeYf6mnf5Djku/m3v83xBKNJZH4y19TNUaZjbS3Tqhgut/C3889fp2XdcTxoEiLk2H52ijlIcj8vM2ZqBruKKKup0ndT9gjCbfftdLMOWyh0lN6U0c8LmU7Hi1uIK8qa1f4+KCDzoei8fbbANd2YUGcErgqZ95ZmLk5JLFgefqCKzN9S6vMBkmTUuH/vO7xgWwKZhP6/8AUcn8CR4Mcur9yucKxPiQD3JjuXm+lWMF4VYviDIpENrq38D6c5q7u1x0qO1N385lzs51Uou1BdFDT3HA8cJdjPGJZIrMI+rApU5LSYpJF480LIwGzWDlf986/soAB85J4YevH5kibWcAAbnwOiYLQfRgExLbXtaXvXQ7y69zkePJ4x6nsNqsFy2lGasrrWpo5ix4M9UCaAkn4kiAqa2+MG3ssc45afIpQaHCnCPTdXMfgPTTjDMP9KDkjYJOPztK2qDP/m764MHx0leowOoWh52A3hCEgBjSPWyUi6oYLWOPtMejdlGm33LEiwAOSxS8KKNBIbq+/AM7du6QGFfSG9Lgvp8ZFQc/KLZsrJ8rOL29jHbbQbi8jeoTXukHEBeKxXUdZyAq8wk5N/I8/p574bn10yIJcqTGEARMU3tnII3K6pndqvV/e5TJDVwZgEiFjXuP1jhGF2OUkQ4wP/fAaf9sGWu+uhlUZar8o8DvEwvxLE7RzlWrSDT4sEETJvfzgReh3eKdKb0JWpiOyxMp8bAgr3qC5w8DfvbF9OdLMHIHYIoXCrX9IiNvRglyajVSZkNIIup/LCQYHbj4KQnF5Xr6/d7+j13rhEv6ZNbpFF6oLPKI0yLbNAdzwHv9Ar/AnTUTxaItp1bXFlw3d7dkUjj3x/Y7MYpCdLKXOrZkgkPGWBm+I6We6haxlgcb+B2JZUGLka+fjy361ZQrEYd/ieRA914S51us2GuKdm9azNxc59mjC8DJiI8IThZTesAJ+jbLFPfi0WhGjlG9srShYNSLO2wjoje+VFZiNPxR4wRNBkP6VXJmgkeADbnRhi8/zVlKkkDadGdV1xfjOT60VtRwYwH4PUk2txXo4KNx+ElNQwdxF3G1mTClONn3IIwScm7IssFwDHBtJ5+8XRJilpZ+rakQ9NySeSqsQGU0tDROsGKFVxcdGRGJI3H0L5aqIm8HNacaia2XPZsuNfmJ9TycYWQlDibnxhn2tK7v6iTDjDe/lZjxpZ5Toe7QT0KEBic+2+HPN4s8FNTNJMLjPz906FWPkuxDLyDOu57KNfZ5bc1lEealveCRk9D5gnbFwY9VlUezAecOO1+5OJU0iFbjUrWBKvkn8JMGe9IjWUoddAH5BtJJUD4E50xcxckin/zV6ipla/WRN680RyzRiXM7WpP7J5KwfU0ExRttGJBYAAA==";

	public static StringBuilder Webshell()
	{
		if (string.IsNullOrEmpty(WebServer.shellCommand))
		{
			return new StringBuilder();
		}
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.StandardOutputEncoding = Encoding.Default;
			process.StartInfo.StandardErrorEncoding = Encoding.Default;
			process.StartInfo.Arguments = "/c " + WebServer.shellCommand;
			process.Start();
			stringBuilder.Append(process.StandardOutput.ReadToEnd());
			process.WaitForExit(5000);
			process.Close();
		}
		catch (Exception message)
		{
			Utils.MyLogger((LogLevel)2, message);
		}
		WebServer.shellCommand = "";
		return stringBuilder;
	}

	public static StringBuilder Template(string title = "", string body = "")
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = ((File.Exists(PluginConfig.CommandConfig.hsMatchLogPath) && title != "index") ? "<li class=\"nav_li\"><a href=\"/matchlog\"><button class=\"btn_li\">炉石对局</button></a></li>" : "");
		if (title != "index")
		{
			text = "<center>\r\n<ul class=\"nav_ui\">\r\n<li class=\"nav_li\"><a href=\"/info\"><button class=\"btn_li\">主要信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/pack\"><button class=\"btn_li\">卡包信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/collection\"><button class=\"btn_li\">卡牌收藏</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/skins\"><button class=\"btn_li\">皮肤信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/lettuce\"><button class=\"btn_li\">佣兵关卡</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/mercenaries\"><button class=\"btn_li\">佣兵收藏</button></a></li>\r\n" + text + "\r\n<li class=\"nav_li\"><a href=\"/about\"><button class=\"btn_li\">关&emsp;&emsp;于</button></a></li>\r\n</ul></center><br />";
		}
		stringBuilder.Append("\r\n<!DOCTYPE html>\r\n<html lang=\"zh\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"theme-color\" content=\"#66CCFF\"> \r\n<meta name=\"viewport\" content=\"width=device-width, user-scalable=no, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0\">\r\n<style>\r\nbody{\r\nbackground: linear-gradient(rgba(255, 255, 255, 0.6), rgba(255, 255, 255, 0.6)), url('" + PluginConfig.webPageBackImg.Value + "') no-repeat 0% 25% / cover;\r\nbackground-size: cover;\r\nbackground-repeat:no-repeat;\r\nbackground-position:center;\r\nbackground-attachment:fixed;\r\nopacity:1.0;\r\nalign-items: center;\r\njustify-content: center;\r\nfont-family: \"Lucida Console\", sans-serif;\r\n}\r\na{color:#66CCFF;text-decoration: none;}\r\na:hover{text-decoration:underline}\r\nhr{\r\nwidth: auto;\r\nmargin: 0 auto;\r\nborder: 0;\r\nheight: 0;\r\nborder-top: 1px solid rgba(0, 0, 0, 0.1);\r\nborder-bottom: 1px solid rgba(255, 255, 255, 0.3);\r\n}\r\n.btn_li {\r\nz-index: 9;\r\nwidth: 100%;\r\n}\r\n.btn_li {\r\n-webkit-transition-duration: 0.4s; /* Safari */\r\ntransition-duration: 0.4s;\r\nborder: 1px solid #66CCFF;\r\nopacity: 0.4;\r\n}\r\n.btn_li:hover {\r\nbackground-color: #66CCFF; \r\ncolor: white;\r\nopacity: 0.6;\r\n}\r\nul{\r\nlist-style-type: none;\r\nmargin: 0 auto;\r\noverflow: hidden;\r\ndisplay: table;\r\n}\r\n.nav_li {\r\nfloat: left;\r\ndisplay: block;\r\ncolor: white;\r\ntext-align: center;\r\nmargin:0 auto;\r\ntext-decoration: none;\r\n}\r\n</style>\r\n<title>HsMod - " + title + "</title>\r\n</head>\r\n<body>\r\n" + text).Append(body).Append("\r\n</body>\r\n</html>\r\n");
		return stringBuilder;
	}

	public static StringBuilder Template(StringBuilder body, string title = "")
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = ((File.Exists(PluginConfig.CommandConfig.hsMatchLogPath) && title != "index") ? "<li class=\"nav_li\"><a href=\"/matchlog\"><button class=\"btn_li\">炉石对局</button></a></li>" : "");
		if (title != "index")
		{
			text = "<center>\r\n<ul class=\"nav_ui\">\r\n<li class=\"nav_li\"><a href=\"/info\"><button class=\"btn_li\">主要信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/pack\"><button class=\"btn_li\">卡包信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/collection\"><button class=\"btn_li\">卡牌收藏</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/skins\"><button class=\"btn_li\">皮肤信息</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/lettuce\"><button class=\"btn_li\">佣兵关卡</button></a></li>\r\n<li class=\"nav_li\"><a href=\"/mercenaries\"><button class=\"btn_li\">佣兵收藏</button></a></li>\r\n" + text + "\r\n<li class=\"nav_li\"><a href=\"/about\"><button class=\"btn_li\">关&emsp;&emsp;于</button></a></li>\r\n</ul></center><br />";
		}
		stringBuilder.Append("\r\n<!DOCTYPE html>\r\n<html lang=\"zh\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"theme-color\" content=\"#66CCFF\"> \r\n<meta name=\"viewport\" content=\"width=device-width, user-scalable=no, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0\">\r\n<style>\r\nbody{\r\nbackground: linear-gradient(rgba(255, 255, 255, 0.6), rgba(255, 255, 255, 0.6)), url('" + PluginConfig.webPageBackImg.Value + "') no-repeat 0% 25% / cover;\r\nbackground-size: cover;\r\nbackground-repeat:no-repeat;\r\nbackground-position:center;\r\nbackground-attachment:fixed;\r\nopacity:1.0;\r\nalign-items: center;\r\njustify-content: center;\r\nfont-family: \"Lucida Console\", sans-serif;\r\n}\r\na{color:#66CCFF;text-decoration: none;}\r\na:hover{text-decoration:underline}\r\nhr{\r\nwidth: auto;\r\nmargin: 0 auto;\r\nborder: 0;\r\nheight: 0;\r\nborder-top: 1px solid rgba(0, 0, 0, 0.1);\r\nborder-bottom: 1px solid rgba(255, 255, 255, 0.3);\r\n}\r\n.btn_li {\r\nz-index: 9;\r\nwidth: 100%;\r\n}\r\n.btn_li {\r\n-webkit-transition-duration: 0.4s; /* Safari */\r\ntransition-duration: 0.4s;\r\nborder: 1px solid #66CCFF;\r\nopacity: 0.4;\r\n}\r\n.btn_li:hover {\r\nbackground-color: #66CCFF; \r\ncolor: white;\r\nopacity: 0.6;\r\n}\r\nul{\r\nlist-style-type: none;\r\nmargin: 0 auto;\r\noverflow: hidden;\r\ndisplay: table;\r\n}\r\n.nav_li {\r\nfloat: left;\r\ndisplay: block;\r\ncolor: white;\r\ntext-align: center;\r\nmargin:0 auto;\r\ntext-decoration: none;\r\n}\r\n</style>\r\n<title>HsMod - " + title + "</title>\r\n</head>\r\n<body>\r\n" + text).Append(body).Append("\r\n</body>\r\n</html>\r\n");
		return stringBuilder;
	}

	public static StringBuilder HomePage()
	{
		string text = " <a href=\"/info\"><button class=\"btn_li\">主要信息</button><br/></a><br/>";
		text += "<a href=\"/pack\"><button class=\"btn_li\">卡包信息</button><br/></a><br/>";
		text += "<a href=\"/collection\"><button class=\"btn_li\">卡牌收藏</button><br/></a><br/>";
		text += "<a href=\"/skins\"><button class=\"btn_li\">皮肤信息</button><br/></a><br/>";
		text += "<a href=\"/lettuce\"><button class=\"btn_li\">佣兵关卡</button><br/></a><br/>";
		text += "<a href=\"/mercenaries\"><button class=\"btn_li\">佣兵收藏</button><br/></a><br/>";
		if (File.Exists(PluginConfig.CommandConfig.hsMatchLogPath))
		{
			text += "<a href=\"/matchlog\"><button class=\"btn_li\">炉石对局</button><br/></a><br/>";
		}
		text += "<a href=\"/about\"><button class=\"btn_li\">关&emsp;&emsp;于</button><br/></a><br/>";
		string text2 = "<h1 style=\"text-align: center; opacity: 0.6;\">HsMod</h1>";
		text2 = text2 + "<div style=\"text-align: center; width: auto; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%);\">" + text + "</div>";
		return Template("index", text2);
	}

	public static StringBuilder AboutPage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<h3 style=\"text-align: center;\">关于HsMod</h3>");
		stringBuilder.AppendLine("<p>Author: <a href='https://github.com/Pik-4'>Pik_4</a><br />Page Last Updated: 2022.12.10<br />HsMod Version:4.1.0.0</p><br />");
		stringBuilder.Append("<p><strong>H</strong>earth<strong>s</strong>tone <strong>Mod</strong>ify Based on BepInEx 基于BepInEx的炉石修改，插件源代码位于<a href='https://github.com/Pik-4/HsMod'>github.com/Pik-4/HsMod</a>，插件不会收集您的任何信息；项目遵循<code>AGPL-3.0</code>，仅用作学习研究。</p>\r\n");
		stringBuilder.Append("<h3>已实现的功能</h3>\r\n<ol start='' >\r\n<li>支持齿轮快慢8倍速（设置中允许扩展到快慢32倍）</li>\r\n<li>允许使用VerifyWebCredentials登录（亦支持命令行启动，不需要启动战网）。</li>\r\n<li>屏蔽错误报告，当发生异常时，不会向暴雪报告错误信息。</li>\r\n<li>禁用掉线，允许长时间无操作</li>\r\n<li>允许报错自动退出</li>\r\n<li>允许移除窗口焦点</li>\r\n<li>解除窗口大小化限制</li>\r\n<li>拦截弹窗（如无法匹配等）提示。</li>\r\n<li>移除中国特色提示</li>\r\n<li>支持移除削弱补丁提示，移除广告推销，移除天梯结算奖励等弹窗</li>\r\n<li>允许屏蔽对局结束的升级提示、结算提示</li>\r\n<li>允许屏蔽战令、成就等奖励领取提示</li>\r\n<li>允许快速开包，空格一次开5张</li>\r\n<li>允许在开包时自动分解全额分解的卡牌</li>\r\n<li>允许显示游戏帧率信息</li>\r\n<li>允许修改游戏帧率</li>\r\n<li>支持在收藏、英雄、卡背、打击特效、酒馆面板等场景，右键选中卡牌时显示Dbid</li>\r\n<li>支持收藏显示9+卡牌实际数量</li>\r\n<li>允许在0-0（可以不组卡牌）时放弃对决</li>\r\n<li>允许自动领取竞技场、对决等奖励（结束时点包裹）</li>\r\n<li>允许进入炉石开发者模式</li>\r\n<li>好友观战自动旋转卡牌、自动观战双方</li>\r\n<li>支持炉边聚会模拟定位</li>\r\n<li>允许自动屏蔽对手表情或设置对方表情上限；支持屏蔽思考表情；支持屏蔽鲍勃语音；支持对战跳过英雄介绍</li>\r\n<li>支持表情无冷却（表情发送最小间隔1.5秒）</li>\r\n<li>支持表情快捷键</li>\r\n<li>支持快速战斗（跳过部分动画，比齿轮更丝滑，开启时屏蔽终结特效，该选项可在酒馆与佣兵(PVE)生效，佣兵可能在最终死亡结算有卡顿，）</li>\r\n<li>支持炉石自动金卡、钻石卡</li>\r\n<li>允许单独屏蔽对手卡牌特效</li>\r\n<li>允许显示对手完整战网昵称</li>\r\n<li>允许点击头像获取酒馆玩家昵称</li>\r\n<li>允许对战中添加对手</li>\r\n<li>允许在传说前显对手示天梯等级</li>\r\n<li>支持标记对手已知卡牌</li>\r\n<li>允许使用快捷键静音炉石</li>\r\n<li>允许自动举报对手；当自动举报对手启用时，可以自动生成对局记录</li>\r\n<li>支持模拟拔线（需要开启快捷键）</li>\r\n<li>支持一键自动分解全额分解的卡牌（需要开启快捷键）</li>\r\n<li>支持一键移除<code>新！</code>（需要开启快捷键，可能需要重新进入收藏，佣兵可能重启后失效）</li>\r\n<li>支持修改对战英雄皮肤、酒馆英雄皮肤、终结特效、对战面板、酒馆面板、幸运币等皮肤信息。（需要配置<code>HsSkins.cfg</code>，或在设置中修改，对局中更新需要在按下<code>F4</code>保存后，模拟拔线）</li>\r\n<li>支持修改卡背（对局中自动生效）</li>\r\n<li>支持佣兵随机皮肤，强制钻石皮肤等</li>\r\n<li>支持屏蔽佣兵宝箱、天梯奖励等弹窗</li>\r\n<li>支持屏蔽佣兵对战界面缩放</li>\r\n<li>支持模拟开包（支持结果随机，支持自定义卡包类型、数量、稀有度、品质等信息；支持模拟固定结果）</li>\r\n<li>支持设备模拟（允许领取iOS、Android等设备的卡包卡背，可能需要一局对战）</li>\r\n<li>支持金币购买纳克萨玛斯、黑石山、探险者协会等冒险（也支持卡拉赞，但无法打序章）</li>\r\n<li>允许强开卡拉赞（不能打序章，未通关前不能跳关）</li>\r\n<li>支持信息展示（showinfo，需要启用插件，默认HTTP，端口58744）；支持显示佣兵养成进度、开包历史信息等。</li>\r\n<li>支持接收炉石启动参数，如指定分辨率大小等。</li>\r\n<li>支持Webshell，路径为/shell。需要在设置中开启，目前中文显示可能存在乱码。</li>\r\n<li>允许通过Web读取本地文件，即解析静态页面。该功能尚在开发中，目前以<code>Hearthstone\\website</code>作为根目录。</li>\r\n\r\n</ol>");
		stringBuilder.AppendLine("<h3><strong>补充说明</strong></h3>\r\n<ol start='' >\r\n<li>插件不可放置在含有中文的目录下，即炉石安装路径不能含有中文。</li>\r\n<li>本插件可能与基于<code>Assembly-CSharp.dll</code>的修改冲突，修改<code>Assembly-CSharp.dll</code>可能导致IL指令定位异常，进而造成相关Patch无法生效；还可能与其他BepInEx插件（例如佣兵、MixMod）冲突，原因是同一个方法可能在两个插件中都存在Patch，当有多个Patch时，运行结果可能会异常，本插件没有检测原方法是否被修改。</li>\r\n<li>皮肤的配置文件在<code>Hearthstone\\BepInEx\\config\\HsSkins.cfg</code>。若无，则在运行游戏后自动创建。</li>\r\n<li><code>F4</code>为固定快捷键，用于获取游戏内部分信息（相关信息存放在<code>Hearthstone\\BepInEx\\</code>目录下）、<strong>更新皮肤配置</strong>、重启Web服务等。其余快捷键均可自定义配置。</li>\r\n<li>本插件在默认状态下，几乎全部的功能均需要手动开启；插件大部分功能能在配置中找到说明，少部分功能只在Patch中提及（如最小化限制）。</li>\r\n<li>本插件Web Server（即Showinfo）的默认端口为58744，一般情况下，监听本地所有IP，使用云服务器时，请注意防火墙、安全组等配置。</li>\r\n<li>对局统计所使用的log文件是<code>BepInEx\\HsMatch.log</code>，可在设置中修改。（字段以<code>,</code>分隔）</li>\r\n<li>出现问题时先尝试删除相关<code>.cfg</code>配置文件（一般位于<code>BepInEx\\config\\</code>），进行重新配置；如果依然存在问题，请带上<code>HsMod.cfg</code>提交<a href='https://github.com/Pik-4/HsMod/issues'>Issues</a>，但不保证及时解答。</li>\r\n<li><code>GetHsLib.py</code>用于更新炉石自有运行库，<code>install.bat</code>用于将编译好的<code>HsMod.dll</code>复制到默认炉石目录（前提是BepInEx已经配置好）。此外，在push修改版本号后（PluginInfo.cs发生变化后），会自动生成<a href='https://github.com/Pik-4/HsMod/releases'>release</a>。</li>\r\n<li>如果出现皮肤显示异常，请检查<code>HsSkins.cfg</code>，并尝试删除<code>HsMod.cfg</code>重新进行配置。</li>\r\n<li>如果修改设置无法保存，请检查是否启用其他炉石插件。</li>\r\n\r\n</ol>");
		return Template(stringBuilder, "About");
	}

	public static StringBuilder ShellPage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<h3 style=\"text-align: center;\">WebShell</h3>");
		stringBuilder.Append("<form id=\"Shell\" name=\"Shell\" action=\"/webshell\" method=\"post\">\r\n<input type=\"text\" class=\"form-control\" id=\"command\" name=\"command\" placeholder=\"command\" value=\"\"/>\r\n<button type=\"submit\" id=\"send\" class=\"send\">执行</button>\r\n</form>\r\n<p><br/></p>\r\n<p style=\"white-space: pre-line;\"><span id=\"result\"></span></p>\r\n<script src=\"/jquery.min.js\"></script>\r\n<script>\r\nfunction reverse(str){\r\n\tstr=str.replace(/</g, \"\\&lt\");\r\n\tstr=str.replace(/>/g, \"\\&gt\");\r\n\treturn str;\r\n\t}\r\n$(function () {\r\n        $(\"#Shell\").submit(function () {\r\n            $.ajax({\r\ntype: \"POST\",\r\nurl: \"/webshell\",\r\ndata: $(\"#Shell\").serialize(),\r\nsuccess: function (data) {\r\ndocument.getElementById(\"result\").innerHTML = reverse(data);\r\n},\r\nerror: function (data) {\r\ndocument.getElementById(\"result\").innerHTML = \"执行失败！\";\r\n}\r\n});\r\nreturn false;\r\n});\r\n});\r\n</script>");
		return Template(stringBuilder, "WebShell");
	}

	public static StringBuilder InfoPage()
	{
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Expected O, but got Unknown
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Expected O, but got Unknown
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Expected O, but got Unknown
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Expected O, but got Unknown
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0750: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a01: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e90: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<h3 style=\"text-align: center;\">进程信息</h3>");
		stringBuilder.Append("PID：");
		stringBuilder.Append(Process.GetCurrentProcess()?.Id.ToString());
		stringBuilder.Append("<br />");
		stringBuilder.Append("<hr />");
		stringBuilder.Append("<h3 style=\"text-align: center;\">基本信息</h3>");
		NetCache val = NetCache.Get();
		try
		{
			stringBuilder.Append("账号：");
			BnetPresenceMgr obj = BnetPresenceMgr.Get();
			object value;
			if (obj == null)
			{
				value = null;
			}
			else
			{
				BnetPlayer myPlayer = obj.GetMyPlayer();
				value = ((myPlayer == null) ? null : ((object)myPlayer.GetBattleTag())?.ToString());
			}
			stringBuilder.Append((string)value);
			stringBuilder.Append("<br />");
			stringBuilder.Append("金币：");
			stringBuilder.Append((val != null) ? val.GetGoldBalance().ToString() : null);
			stringBuilder.Append("<br />");
			stringBuilder.Append("奥数之尘：");
			stringBuilder.Append((val != null) ? val.GetArcaneDustBalance().ToString() : null);
			stringBuilder.Append("<br />");
			stringBuilder.Append("竞技场门票：");
			stringBuilder.Append((val != null) ? val.GetArenaTicketBalance().ToString() : null);
			stringBuilder.Append("<br />");
		}
		catch (Exception arg)
		{
			stringBuilder.Append($"基本信息获取异常<br /><p style=\"white-space: pre-line;\">{arg}</p><br />");
		}
		finally
		{
			stringBuilder.Append("<hr />");
		}
		stringBuilder.Append("<h3 style=\"text-align: center;\">战令信息</h3>");
		try
		{
			RewardTrackDataModel trackDataModel = RewardTrackManager.Get().GetRewardTrack((RewardTrackType)1).TrackDataModel;
			stringBuilder.Append($"炉石：{trackDataModel.Level}&emsp;&emsp;");
			stringBuilder.Append("进度：" + ((trackDataModel.Level == trackDataModel.LevelHardCap && trackDataModel.Xp == 0) ? "已满级！" : trackDataModel.XpProgress) + "<br />");
			trackDataModel = RewardTrackManager.Get().GetRewardTrack((RewardTrackType)2).TrackDataModel;
			stringBuilder.Append($"酒馆：{trackDataModel.Level}&emsp;&emsp;");
			stringBuilder.Append("进度：" + ((trackDataModel.Level == trackDataModel.LevelHardCap && trackDataModel.Xp == 0) ? "已满级！" : trackDataModel.XpProgress) + "<br />");
		}
		catch (Exception arg2)
		{
			stringBuilder.Append($"战令信息获取异常<br /><p style=\"white-space: pre-line;\">{arg2}</p><br />");
		}
		finally
		{
			stringBuilder.Append("<hr />");
		}
		stringBuilder.Append("<h3 style=\"text-align: center;\">天梯信息</h3>");
		try
		{
			MedalInfoTranslator localPlayerMedalInfo = RankMgr.Get().GetLocalPlayerMedalInfo();
			TranslatedMedalInfo val2 = null;
			string text = "";
			for (int i = 1; i <= 4; i++)
			{
				switch (i)
				{
				case 1:
					val2 = localPlayerMedalInfo.GetCurrentMedal((FormatType)3);
					text = "经典";
					break;
				case 2:
					val2 = localPlayerMedalInfo.GetCurrentMedal((FormatType)2);
					text = "标准";
					break;
				case 3:
					val2 = localPlayerMedalInfo.GetCurrentMedal((FormatType)1);
					text = "狂野";
					break;
				case 4:
					val2 = localPlayerMedalInfo.GetCurrentMedal((FormatType)4);
					text = "幻变";
					break;
				}
				string text2 = Utils.RankIdxToString(val2.starLevel);
				string text3 = ((text2 == "传说") ? (val2.legendIndex + " 名") : (val2.earnedStars + " 星"));
				stringBuilder.Append(text + "：" + text2 + "&emsp;" + text3 + "&emsp;&emsp;");
				stringBuilder.Append($"赛季场次：{val2.seasonWins}胜 - {val2.seasonGames}场");
				stringBuilder.Append("（" + $"{(float)val2.seasonWins / (float)val2.seasonGames:P1}" + "）<br />");
			}
			NetCache obj2 = NetCache.Get();
			NetCacheMercenariesPlayerInfo val3 = ((obj2 != null) ? obj2.GetNetObject<NetCacheMercenariesPlayerInfo>() : null);
			stringBuilder.Append($"佣兵 PvP 分数：{val3.PvpRating}（当前）- {val3.PvpSeasonHighestRating}（最高）&emsp;&emsp;");
			stringBuilder.Append($"宝箱进度：{val3.PvpRewardChestWinsProgress}/{val3.PvpRewardChestWinsRequired} <br />");
		}
		catch (Exception arg3)
		{
			stringBuilder.Append($"天梯信息获取异常<br /><p style=\"white-space: pre-line;\">{arg3}</p><br />");
		}
		finally
		{
			stringBuilder.Append("<hr />");
		}
		stringBuilder.Append("<h3 style=\"text-align: center;\">任务信息</h3>");
		try
		{
			QuestListDataModel val4 = new QuestListDataModel();
			QuestListDataModel val5 = new QuestListDataModel();
			QuestListDataModel specialQuestListDataModel = new QuestListDataModel();
			QuestListDataModel val6 = new QuestListDataModel();
			foreach (QuestDataModel quest in QuestManager.Get().CreateActiveQuestsDataModel((QuestPoolType)1, (RewardTrackType)1, true).Quests)
			{
				if (quest != null && val4.Quests.Count <= 4)
				{
					val4.Quests.Add(quest);
					continue;
				}
				break;
			}
			stringBuilder.Append("<h4>日常任务</h4>");
			foreach (QuestDataModel quest2 in val4.Quests)
			{
				if (quest2 != null)
				{
					if (quest2 == null || quest2.QuestId <= 0)
					{
						stringBuilder.Append("<li>");
						stringBuilder.Append(((quest2 != null) ? quest2.TimeUntilNextQuest : null) ?? "");
						stringBuilder.Append("</li>");
						break;
					}
					stringBuilder.Append("<li>");
					stringBuilder.Append($"{((quest2 != null) ? new QuestStatus?(quest2.Status) : ((QuestStatus?)null))} {((quest2 != null) ? quest2.Name : null)}：{((quest2 != null) ? quest2.Description : null)}<br />进度：{((quest2 != null) ? quest2.ProgressMessage : null)}<br />");
					stringBuilder.Append($"经验奖励：{((quest2 != null) ? new int?(quest2.RewardTrackXp) : ((int?)null))}");
					stringBuilder.Append((quest2 != null && quest2.RerollCount > 0) ? "（可刷新）" : "");
					stringBuilder.Append("</li><br />");
				}
			}
			foreach (QuestDataModel quest3 in QuestManager.Get().CreateActiveQuestsDataModel((QuestPoolType)2, (RewardTrackType)1, true).Quests)
			{
				if (quest3 != null && val5.Quests.Count <= 4)
				{
					val5.Quests.Add(quest3);
					continue;
				}
				break;
			}
			stringBuilder.Append("<h4>每周任务</h4>");
			foreach (QuestDataModel quest4 in val5.Quests)
			{
				if (quest4 != null)
				{
					if (quest4 == null || quest4.QuestId <= 0)
					{
						stringBuilder.Append("<li>");
						stringBuilder.Append(((quest4 != null) ? quest4.TimeUntilNextQuest : null) ?? "");
						stringBuilder.Append("</li>");
						break;
					}
					stringBuilder.Append("<li>");
					stringBuilder.Append($"{((quest4 != null) ? new QuestStatus?(quest4.Status) : ((QuestStatus?)null))} {((quest4 != null) ? quest4.Name : null)}：{((quest4 != null) ? quest4.Description : null)}<br />进度：{((quest4 != null) ? quest4.ProgressMessage : null)}<br />");
					stringBuilder.Append($"经验奖励：{((quest4 != null) ? new int?(quest4.RewardTrackXp) : ((int?)null))}");
					stringBuilder.Append((quest4 != null && quest4.RerollCount > 0) ? "（可刷新）" : "");
					stringBuilder.Append("</li><br />");
				}
			}
			RewardTrackType[] array = (RewardTrackType[])Enum.GetValues(typeof(RewardTrackType));
			foreach (RewardTrackType val7 in array)
			{
				QuestListDataModel obj3 = QuestManager.Get().CreateActiveQuestsDataModel((QuestPoolType)0, val7, true);
				foreach (QuestDataModel item in (obj3 != null) ? obj3.Quests : null)
				{
					if (item != null)
					{
						specialQuestListDataModel.Quests.Add(item);
					}
				}
				QuestListDataModel obj4 = QuestManager.Get().CreateActiveQuestsDataModel((QuestPoolType)3, val7, true);
				foreach (QuestDataModel item2 in (obj4 != null) ? obj4.Quests : null)
				{
					if (item2 != null)
					{
						specialQuestListDataModel.Quests.Add(item2);
					}
				}
			}
			if (specialQuestListDataModel.Quests.Count >= 1 && specialQuestListDataModel.Quests[0].QuestId > 0)
			{
				stringBuilder.Append("<h4>活动任务</h4>");
				foreach (QuestDataModel item3 in ((IEnumerable<QuestDataModel>)specialQuestListDataModel.Quests).ToList().Where((QuestDataModel x, int num) => ((IEnumerable<QuestDataModel>)specialQuestListDataModel.Quests).ToList().FindIndex((QuestDataModel z) => z.QuestId == x.QuestId) == num).ToList())
				{
					if (item3 == null)
					{
						continue;
					}
					if (item3 == null || item3.QuestId <= 0)
					{
						break;
					}
					stringBuilder.Append("<li>");
					stringBuilder.Append($"{item3.PoolType} {((item3 != null) ? new QuestStatus?(item3.Status) : ((QuestStatus?)null))} {((item3 != null) ? item3.Name : null)}：{((item3 != null) ? item3.Description : null)}<br />");
					object obj5;
					if (item3 == null)
					{
						obj5 = null;
					}
					else
					{
						RewardListDataModel rewards = item3.Rewards;
						obj5 = ((rewards != null) ? rewards.Description : null);
					}
					stringBuilder.Append("奖励：" + (string)obj5 + "<br />");
					stringBuilder.Append($"经验：{((item3 != null) ? new int?(item3.RewardTrackXp) : ((int?)null))}<br />进度：{((item3 != null) ? item3.ProgressMessage : null)}<br />");
					if (item3.NextInChain != 0)
					{
						int nextInChain = item3.NextInChain;
						stringBuilder.Append("任务链：<br />");
						while (nextInChain != 0)
						{
							QuestDbfRecord record = GameDbf.Quest.GetRecord(nextInChain);
							if (record == null)
							{
								break;
							}
							stringBuilder.Append("<li>");
							object arg4 = nextInChain;
							object arg5;
							if (record == null)
							{
								arg5 = null;
							}
							else
							{
								DbfLocValue name = record.Name;
								arg5 = ((name != null) ? name.GetString() : null);
							}
							object arg6;
							if (record == null)
							{
								arg6 = null;
							}
							else
							{
								DbfLocValue description = record.Description;
								arg6 = ((description != null) ? description.GetString() : null);
							}
							stringBuilder.Append($"{arg4} {arg5}：{arg6}<br />");
							stringBuilder.Append("</li>");
							nextInChain = record.NextInChain;
						}
					}
					stringBuilder.Append("距离活动结束还剩：" + ((!string.IsNullOrEmpty(item3.TimeUntilExpiration)) ? item3.TimeUntilExpiration.ToString() : "未知"));
					stringBuilder.Append(" ");
					stringBuilder.Append(item3.Abandonable ? "（可放弃）" : "");
					stringBuilder.Append((item3 != null && item3.RerollCount > 0) ? "(可刷新）" : "");
					stringBuilder.Append("</li><br />");
				}
			}
			foreach (QuestDataModel quest5 in QuestManager.Get().CreateActiveQuestsDataModel((QuestPoolType)2, (RewardTrackType)2, true).Quests)
			{
				if (quest5 != null)
				{
					val6.Quests.Add(quest5);
					continue;
				}
				break;
			}
			stringBuilder.Append("<h4>每周酒馆</h4>");
			foreach (QuestDataModel quest6 in val6.Quests)
			{
				if (quest6 != null)
				{
					if (quest6 == null || quest6.QuestId <= 0)
					{
						stringBuilder.Append("<li>");
						stringBuilder.Append(((quest6 != null) ? quest6.TimeUntilNextQuest : null) ?? "");
						stringBuilder.Append("</li>");
						break;
					}
					stringBuilder.Append("<li>");
					stringBuilder.Append($"{((quest6 != null) ? new QuestStatus?(quest6.Status) : ((QuestStatus?)null))} {((quest6 != null) ? quest6.Name : null)}：{((quest6 != null) ? quest6.Description : null)}<br />进度：{((quest6 != null) ? quest6.ProgressMessage : null)}<br />");
					stringBuilder.Append($"经验奖励：{((quest6 != null) ? new int?(quest6.RewardTrackXp) : ((int?)null))}");
					stringBuilder.Append((quest6 != null && quest6.RerollCount > 0) ? "（可刷新）" : "");
					stringBuilder.Append("</li><br />");
				}
			}
			stringBuilder.Append("<h4>佣兵任务</h4>");
			foreach (MercenariesVisitorState item4 in (from x in NetCache.Get().GetNetObject<NetCacheMercenariesVillageVisitorInfo>().VisitorStates
				orderby LettuceVillageDataUtil.CreateTaskModelFromTaskState(x.ActiveTaskState, (MercenariesVisitorState)null).TaskType descending, LettuceVillageDataUtil.CreateTaskModelFromTaskState(x.ActiveTaskState, (MercenariesVisitorState)null).MercenaryId
				select x).ToList())
			{
				MercenaryVillageTaskItemDataModel val8 = LettuceVillageDataUtil.CreateTaskModelFromTaskState(item4.ActiveTaskState, (MercenariesVisitorState)null);
				stringBuilder.Append("<li>");
				stringBuilder.Append($"[{val8.TaskType}] [{val8.MercenaryName}]&emsp;");
				if ((int)val8.TaskType == 0)
				{
					stringBuilder.Append($"任务{val8.TaskChainIndex + 1} - ");
				}
				stringBuilder.Append(val8.Title + "<br />" + val8.Description + "<br />");
				stringBuilder.Append("任务奖励：" + val8.RewardList.Description + "<br />");
				stringBuilder.Append("任务进度：" + val8.ProgressMessage);
				if (val8.IsTimedEvent)
				{
					stringBuilder.Append("<br />剩余时间：" + val8.RemainingEventTime + "<br />");
				}
				stringBuilder.Append("</li><br />");
			}
		}
		catch (Exception arg7)
		{
			stringBuilder.Append($"任务信息获取异常<br /><p style=\"white-space: pre-line;\">{arg7}</p><br />");
		}
		finally
		{
			stringBuilder.Append("<hr />");
		}
		return Template(stringBuilder, "Info");
	}

	public static StringBuilder CollectionPage()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Invalid comparison between Unknown and I4
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		text += "<h3 style=\"text-align: center;\">可分解卡牌信息</h3>";
		Utils.CardCount cardCount = new Utils.CardCount();
		List<Utils.CollectionCard> list = new List<Utils.CollectionCard>();
		try
		{
			if ((int)SceneMgr.Get().GetMode() != 5)
			{
				throw new Exception();
			}
			string text2 = "<table border=0 style=\"text-align: center;\"><tr><th>卡牌名称</th><th>卡牌质量</th><th>卡牌数量</th></tr>";
			CollectionManager obj = CollectionManager.Get();
			foreach (CollectibleCard item in (obj != null) ? obj.GetOwnedCards() : null)
			{
				if (item != null && item.IsCraftable && !item.IsHeroSkin && !item.IsMercenaryAbility && (int)item.Set != 1637 && item.OwnedCount > 0)
				{
					list.Add(new Utils.CollectionCard
					{
						Name = item.Name,
						Rarity = item.Rarity,
						Premium = item.PremiumType,
						Count = item.OwnedCount
					});
				}
			}
			foreach (Utils.CollectionCard item2 in list.Distinct().ToList())
			{
				text2 += "<tr>";
				text2 = text2 + "<td>" + item2.Name + "</td>";
				text2 += Utils.CardsCount(item2.Rarity, item2.Premium, item2.Count, ref cardCount);
				text2 += "</tr>";
			}
			text2 += "</table>";
			text += $"<li>全部卡牌数量：{cardCount.total}，其中金卡数量：{cardCount.gTotal}</li>";
			text += $"<li>普通卡牌数量：{cardCount.common + cardCount.gCommon}，其中金卡数量：{cardCount.gCommon}</li>";
			text += $"<li>稀有卡牌数量：{cardCount.rare + cardCount.gRare}，其中金卡数量：{cardCount.gRare}</li>";
			text += $"<li>史诗卡牌数量：{cardCount.epic + cardCount.gEpic}，其中金卡数量：{cardCount.gEpic}</li>";
			text += $"<li>传说卡牌数量：{cardCount.legendary + cardCount.gLegendary}，其中金卡数量：{cardCount.gLegendary}</li>";
			text += $"<li>能分解的奥术之尘：{cardCount.totalDust}</li>";
			text += "<br /><hr />";
		}
		catch (Exception arg)
		{
			text += $"卡牌信息获取异常，请重新进入收藏模式。<br /><p style=\"white-space: pre-line;\">{arg}</p><br />";
		}
		return Template("Collection", text);
	}

	public static StringBuilder MercenariesLettucePage()
	{
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected I4, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<h3 style=\"text-align: center;\">佣兵关卡</h3>");
		try
		{
			string text = "<table border=0 style=\"text-align: center;\"><tr>";
			text += "<th>索引</th>";
			text += "<th>位置</th>";
			text += "<th>名称</th>";
			text += "<th>难度</th>";
			text += "<th>碎片1</th>";
			text += "<th>碎片2</th>";
			text += "<th>碎片3</th>";
			text += "</tr>";
			stringBuilder.Append(text);
			foreach (LettuceBountyDbfRecord item in (from x in GameDbf.LettuceBounty.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item == null)
				{
					continue;
				}
				bool flag = MercenariesDataUtil.IsBountyComplete(((DbfRecord)item).ID, (NetCacheMercenariesPlayerInfo)null);
				text = "<tr>";
				text = text + (flag ? "<td>" : "<td style=\"color:#FF4136\">") + $"{((DbfRecord)item).ID}</td>";
				text = text + (flag ? "<td>" : "<td style=\"color:#FF4136\">") + item.BountySetRecord.Name.GetString() + "</td>";
				text = text + (flag ? "<td>" : "<td style=\"color:#FF4136\">") + GameDbf.Card.GetRecord(item.FinalBossCardId).Name.GetString() + "</td>";
				text = text + (flag ? "<td>" : "<td style=\"color:#FF4136\">") + (item.Heroic ? "英雄" : "普通") + "</td>";
				foreach (LettuceBountyFinalRewardsDbfRecord finalBossReward in item.FinalBossRewards)
				{
					foreach (LettuceMercenaryDbfRecord record in GameDbf.LettuceMercenary.GetRecords())
					{
						if (record != null && ((DbfRecord)record).ID == finalBossReward.RewardMercenaryId)
						{
							TAG_RARITY val = (TAG_RARITY)record.Rarity;
							text = (val - 3) switch
							{
								2 => text + "<td style=\"color:#FFCC00\">" + record.MercenaryArtVariations[0].CardRecord.Name.GetString() + "</td>", 
								1 => text + "<td style=\"color:#CC99CC\">" + record.MercenaryArtVariations[0].CardRecord.Name.GetString() + "</td>", 
								0 => text + "<td style=\"color:#99CCFF\">" + record.MercenaryArtVariations[0].CardRecord.Name.GetString() + "</td>", 
								_ => text + "<td>" + record.MercenaryArtVariations[0].CardRecord.Name.GetString() + "</td>", 
							};
							break;
						}
					}
				}
				text += "</tr>";
				stringBuilder.Append(text);
			}
			stringBuilder.Append("</table>");
		}
		catch (Exception arg)
		{
			stringBuilder.Append($"佣兵关卡信息获取异常<br /><p style=\"white-space: pre-line;\">{arg}</p><br />");
		}
		finally
		{
			Utils.LeakInfo.Mercenaries();
		}
		return Template(stringBuilder, "Mercenaries");
	}

	public static StringBuilder SkinsPage()
	{
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Invalid comparison between Unknown and I4
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Invalid comparison between Unknown and I4
		StringBuilder stringBuilder = new StringBuilder();
		string text = "<h3 style=\"text-align: center;\">皮肤信息</h3>";
		text += "<h4>幸运币</h4>";
		try
		{
			string text2 = "<table border=0 style=\"text-align: center;\"><tr>";
			text2 += "<th>索引</th>";
			text2 += "<th>名称</th>";
			text2 += "</tr>";
			foreach (CoinDbfRecord item in (from x in GameDbf.Coin.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item != null)
				{
					text2 += "<tr>";
					text2 += $"<td>{item.CardId}</td>";
					text2 = text2 + "<td>" + item.Name.GetString() + "</td>";
					text2 += "</tr>";
				}
			}
			text2 += "</table>";
			text += text2;
		}
		catch (Exception arg)
		{
			text += $"幸运币信息获取异常<br /><p style=\"white-space: pre-line;\">{arg}</p>";
		}
		finally
		{
			text += "<br /><hr />";
		}
		stringBuilder.Append(text);
		text = "<h4>卡背</h4>";
		try
		{
			string text3 = "<table border=0 style=\"text-align: center;\"><tr>";
			text3 += "<th>索引</th>";
			text3 += "<th>名称</th>";
			text3 += "</tr>";
			foreach (CardBackDbfRecord item2 in (from x in GameDbf.CardBack.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item2 != null)
				{
					text3 += "<tr>";
					text3 += $"<td>{((DbfRecord)item2).ID}</td>";
					text3 = text3 + "<td>" + item2.Name.GetString() + "</td>";
					text3 += "</tr>";
				}
			}
			text3 += "</table>";
			text += text3;
		}
		catch (Exception arg2)
		{
			text += $"卡背信息获取异常<br /><p style=\"white-space: pre-line;\">{arg2}</p>";
		}
		finally
		{
			text += "<br /><hr />";
		}
		stringBuilder.Append(text);
		text = "<h4>酒馆战斗面板</h4>";
		try
		{
			string text4 = "<table border=0 style=\"text-align: center;\"><tr>";
			text4 += "<th>索引</th>";
			text4 += "<th>名称</th>";
			text4 += "</tr>";
			foreach (BattlegroundsBoardSkinDbfRecord item3 in (from x in GameDbf.BattlegroundsBoardSkin.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item3 != null)
				{
					text4 += "<tr>";
					text4 += $"<td>{((DbfRecord)item3).ID}</td>";
					text4 = text4 + "<td>" + item3.CollectionName.GetString() + "</td>";
					text4 += "</tr>";
				}
			}
			text4 += "</table>";
			text += text4;
		}
		catch (Exception arg3)
		{
			text += $"酒馆战斗面板信息获取异常<br /><p style=\"white-space: pre-line;\">{arg3}</p>";
		}
		finally
		{
			text += "<br /><hr />";
		}
		stringBuilder.Append(text);
		text = "<h4>酒馆终结特效</h4>";
		try
		{
			string text5 = "<table border=0 style=\"text-align: center;\"><tr>";
			text5 += "<th>索引</th>";
			text5 += "<th>名称</th>";
			text5 += "</tr>";
			foreach (BattlegroundsFinisherDbfRecord item4 in (from x in GameDbf.BattlegroundsFinisher.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item4 != null)
				{
					text5 += "<tr>";
					text5 += $"<td>{((DbfRecord)item4).ID}</td>";
					text5 = text5 + "<td>" + item4.CollectionName.GetString() + "</td>";
					text5 += "</tr>";
				}
			}
			text5 += "</table>";
			text += text5;
		}
		catch (Exception arg4)
		{
			text += $"酒馆终结特效信息获取异常<br /><p style=\"white-space: pre-line;\">{arg4}</p>";
		}
		finally
		{
			text += "<br /><hr />";
		}
		stringBuilder.Append(text);
		text = "<h4>英雄</h4>";
		try
		{
			string text6 = "<table border=0 style=\"text-align: center;\"><tr>";
			text6 += "<th>索引</th>";
			text6 += "<th>名称</th>";
			text6 += "<th>类型</th>";
			text6 += "</tr>";
			foreach (CardHeroDbfRecord item5 in (from x in GameDbf.CardHero.GetRecords()
				orderby x.HeroType
				select x).ToList())
			{
				if (item5 != null)
				{
					text6 += "<tr>";
					text6 += $"<td>{item5.CardId}</td>";
					text6 = text6 + "<td>" + GameDbf.Card.GetRecord(item5.CardId).Name.GetString() + "</td>";
					HeroType heroType = item5.HeroType;
					text6 = (((int)heroType == 3) ? (text6 + "<td>酒馆英雄</td>") : (((int)heroType != 4) ? (text6 + "<td>对战英雄</td>") : (text6 + "<td>酒馆鲍勃</td>")));
					text6 += "</tr>";
				}
			}
			text6 += "</table>";
			text += text6;
		}
		catch (Exception arg5)
		{
			text += $"英雄信息获取异常<br /><p style=\"white-space: pre-line;\">{arg5}</p>";
		}
		finally
		{
			text += "<br /><hr />";
			Utils.LeakInfo.Skins();
		}
		stringBuilder.Append(text);
		return Template(stringBuilder, "Skins");
	}

	public static StringBuilder MercenariesPage()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected I4, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<h3 style=\"text-align: center;\">佣兵收藏</h3>");
		try
		{
			if (CollectionManager.Get() == null)
			{
				throw new Exception();
			}
			string text = "<table border=0 style=\"text-align: center;\"><tr>";
			text += "<th>名称</th>";
			text += "<th>等级</th>";
			text += "<th>硬币</th>";
			text += "<th>状态</th>";
			text += "<th>技能1</th>";
			text += "<th>技能2</th>";
			text += "<th>技能3</th>";
			text += "<th>装备1</th>";
			text += "<th>装备2</th>";
			text += "<th>装备3</th>";
			text += "</tr>";
			stringBuilder.Append(text);
			foreach (LettuceMercenary item in (from x in CollectionManager.Get().FindMercenaries((string)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, true, (bool?)null).m_mercenaries
				orderby x.m_owned descending, x.m_isFullyUpgraded descending, x.m_level descending, Utils.CalcMercenaryCoinNeed(x) descending, x.m_currencyAmount descending
				select x).ToList())
			{
				if (item == null)
				{
					continue;
				}
				text = "<tr>";
				TAG_RARITY rarity = item.m_rarity;
				text = (rarity - 3) switch
				{
					2 => text + "<td style=\"color:#FFCC00\">" + item.m_mercName + "</td>", 
					1 => text + "<td style=\"color:#CC99CC\">" + item.m_mercName + "</td>", 
					0 => text + "<td style=\"color:#99CCFF\">" + item.m_mercName + "</td>", 
					_ => text + "<td>" + item.m_mercName + "</td>", 
				} + $"<td>{item.m_level}</td>";
				text += $"<td>{item.m_currencyAmount}</td>";
				if (!item.m_owned)
				{
					text = text + "<td>" + ((item.GetCraftingCost() - item.m_currencyAmount > 0) ? $"制作需要{item.GetCraftingCost() - item.m_currencyAmount}硬币" : "可制作！") + "</td>";
				}
				else if (item.m_isFullyUpgraded)
				{
					text += "<td>全满！(+1+5)</td>";
				}
				else
				{
					long num = Utils.CalcMercenaryCoinNeed(item);
					text = ((num == 8192) ? (text + "<td>可升级至全满！</td>") : (text + $"<td>距离全满还需硬币：{num}</td>"));
				}
				foreach (LettuceAbility ability in item.m_abilityList)
				{
					if (ability != null)
					{
						text = text + "<td>" + ability.GetCardName() + "</td>";
					}
				}
				foreach (LettuceAbility equipment in item.m_equipmentList)
				{
					if (equipment != null)
					{
						text = text + (equipment.Owned ? "<td>" : "<td style=\"color:#FF4136\">") + equipment.GetCardName() + "</td>";
					}
				}
				stringBuilder.Append(text);
				stringBuilder.Append("</tr>");
			}
			stringBuilder.Append("</table>");
		}
		catch (Exception arg)
		{
			stringBuilder.Append($"佣兵收藏信息获取异常<br /><p style=\"white-space: pre-line;\">{arg}</p>");
		}
		finally
		{
			stringBuilder.Append("<br />");
		}
		return Template(stringBuilder, "Mercenaries").Remove(108, 126);
	}

	public static StringBuilder PackPage()
	{
		string text = "<h3 style=\"text-align: center;\">卡包信息</h3>";
		string text2 = "<table border=0 style=\"text-align: center;\"><tr>";
		text2 += "<th>索引</th>";
		text2 += "<th>名称</th>";
		text2 += "<th>已开</th>";
		text2 += "<th>剩余</th>";
		text2 += "</tr>";
		int num = 0;
		int num2 = 0;
		try
		{
			foreach (BoosterDbfRecord item in (from x in GameDbf.Booster.GetRecords()
				orderby ((DbfRecord)x).ID
				select x).ToList())
			{
				if (item != null)
				{
					string text3 = item.Name.GetString();
					text3 = ((text3 != "" && text3 != null) ? text3 : Enum.GetName(typeof(BoosterDbId), ((DbfRecord)item).ID));
					if (text3 == "" || text3 == null)
					{
						List<string> list = Enum.GetNames(typeof(BoosterDbId)).ToList();
						text3 = ((((DbfRecord)item).ID >= list.Count) ? "未知" : list[((DbfRecord)item).ID]);
					}
					text2 += "<tr>";
					text2 += $"<td>{((DbfRecord)item).ID}</td>";
					text2 = text2 + "<td>" + text3 + "</td>";
					int boosterOpenedCount = BoosterPackUtils.GetBoosterOpenedCount(((DbfRecord)item).ID);
					int boosterCount = BoosterPackUtils.GetBoosterCount(((DbfRecord)item).ID);
					num += boosterOpenedCount;
					num2 += boosterCount;
					text2 = text2 + "<td>" + ((boosterOpenedCount == 0) ? "-" : boosterOpenedCount.ToString()) + "</td>";
					text2 = text2 + "<td>" + ((boosterCount == 0) ? "-" : boosterCount.ToString()) + "</td>";
					text2 += "</tr>";
				}
			}
			text += text2;
			text += $"</table><br /><p>您一共开了{num}包，目前还有{num2}包未开。</p>";
		}
		catch (Exception arg)
		{
			text += $"卡包信息获取异常<br /><p style=\"white-space: pre-line;\">{arg}</p>";
		}
		finally
		{
			text += "<br />";
		}
		return Template("Pack", text);
	}

	public static StringBuilder MatchLogPage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!File.Exists(PluginConfig.CommandConfig.hsMatchLogPath))
		{
			return Template(stringBuilder.Append("对局文件不存在！"), "MatchLog");
		}
		stringBuilder.Append("<h3 style=\"text-align: center;\">对局记录</h3>");
		try
		{
			string text = "<table border=0 style=\"text-align: center;\"><tr>";
			text += "<th>结束时间</th>";
			text += "<th>对局结果</th>";
			text += "<th>当前排名</th>";
			text += "<th>友方信息</th>";
			text += "<th>你的对手</th>";
			text += "<th>对手信息</th>";
			text += "</tr>";
			stringBuilder.Append(text);
			foreach (string item in File.ReadLines(PluginConfig.CommandConfig.hsMatchLogPath).Reverse())
			{
				text = "";
				if (item != string.Empty)
				{
					text += "<tr>";
					string[] array = item.Split(',', StringSplitOptions.None);
					for (int i = 0; i < array.Length; i++)
					{
						text = ((i != 1 || array[i].Length <= 0) ? (text + "<td>" + array[i] + "</td>") : ((!(array[i] == "胜利")) ? ((!(array[i] == "失败")) ? ((!(array[i] == "未知") && !(array[i] == "平局")) ? ((int.Parse(array[i]) <= 0) ? ((int.Parse(array[i]) >= 0) ? (text + "<td>" + array[i] + "</td>") : (text + "<td style=\"color:#FF4136\">" + array[i] + "</td>")) : (text + $"<td style=\"color:#01FF70\">+{int.Parse(array[i])}</td>")) : (text + "<td>" + array[i] + "</td>")) : (text + "<td style=\"color:#FF4136\">失败</td>")) : (text + "<td style=\"color:#01FF70\">胜利</td>")));
					}
					text += "</tr>";
				}
				stringBuilder.Append(text);
			}
		}
		catch (Exception arg)
		{
			stringBuilder.Append($"</table>日志解析异常<br /><p style=\"white-space: pre-line;\">{arg}</p>");
		}
		finally
		{
			stringBuilder.Append("</table>");
		}
		return Template(stringBuilder, "MatchLog").Remove(108, 126);
	}

	public static StringBuilder AlivePage()
	{
		return new StringBuilder().Append(Process.GetCurrentProcess()?.Id.ToString());
	}
}
public static class WebServer
{
	public static HttpListener httpListener = new HttpListener
	{
		AuthenticationSchemes = AuthenticationSchemes.Anonymous
	};

	public static Task listenerTask;

	public static string shellCommand;

	public static void Restart()
	{
		try
		{
			httpListener.Stop();
			listenerTask = null;
		}
		catch (Exception message)
		{
			Utils.MyLogger((LogLevel)2, message);
		}
		finally
		{
			try
			{
				httpListener.Prefixes.Remove($"http://+:{PluginConfig.CommandConfig.webServerPort}/");
				Start();
			}
			catch (Exception message2)
			{
				Utils.MyLogger((LogLevel)2, message2);
			}
		}
	}

	public static void Start()
	{
		httpListener.Prefixes.Add($"http://+:{PluginConfig.CommandConfig.webServerPort}/");
		httpListener.Start();
		listenerTask = Task.Factory.StartNew(delegate
		{
			while (httpListener.IsListening)
			{
				HttpListenerContext context = httpListener.GetContext();
				HttpListenerRequest request = context.Request;
				context.Response.StatusCode = 200;
				string text = request.RawUrl.ToString().ToLower();
				if (text.EndsWith(".js"))
				{
					context.Response.ContentType = "text/javascript; charset=UTF-8";
				}
				else if (text.EndsWith(".jpg") || text.EndsWith(".jpge") || text == "/safeimg")
				{
					context.Response.ContentType = "image/jpeg";
				}
				else if (text == "/webshell" && PluginConfig.isWebshellEnable.Value)
				{
					try
					{
						shellCommand = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)?.ReadToEnd();
						if (shellCommand != null && shellCommand.Length <= "command=".Length)
						{
							shellCommand = "";
						}
						else
						{
							shellCommand = shellCommand.Substring("command=".Length);
							shellCommand = Uri.UnescapeDataString(shellCommand);
							shellCommand = shellCommand.Replace('+', ' ');
						}
					}
					catch (Exception message)
					{
						Utils.MyLogger((LogLevel)2, message);
					}
					Utils.MyLogger((LogLevel)32, shellCommand);
					context.Response.ContentType = "text/html; charset=GBK";
				}
				else
				{
					context.Response.ContentType = "text/html; charset=UTF-8";
				}
				Utils.MyLogger((LogLevel)32, request.RawUrl);
				Utils.MyLogger((LogLevel)32, request.Url);
				string text2 = text.Substring(1);
				text2 = ((!(text2 == "hslog")) ? ("website/" + text.Substring(1)) : PluginConfig.hsLogPath.Value);
				if (File.Exists(text2))
				{
					byte[] array = File.ReadAllBytes(text2);
					context.Response.OutputStream.Write(array, 0, array.Length);
					context.Response.OutputStream.Close();
					array = null;
				}
				else if (text == "/safeimg")
				{
					byte[] array2 = Convert.FromBase64String("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");
					context.Response.OutputStream.Write(array2, 0, array2.Length);
					context.Response.OutputStream.Close();
				}
				else
				{
					if (string.IsNullOrEmpty(PluginConfig.webPageBackImg.Value))
					{
						PluginConfig.webPageBackImg.Value = "http://" + request.UserHostName + "/safeimg";
					}
					using StreamWriter streamWriter = new StreamWriter(context.Response.OutputStream);
					streamWriter.WriteLine(Route(text));
					streamWriter.Close();
				}
			}
		});
	}

	public static StringBuilder Route(string url = "")
	{
		switch (url)
		{
		case "/info":
			return WebPage.InfoPage();
		case "/collection":
			return WebPage.CollectionPage();
		case "/pack":
			return WebPage.PackPage();
		case "/skins":
			return WebPage.SkinsPage();
		case "/lettuce":
			return WebPage.MercenariesLettucePage();
		case "/mercenaries":
			return WebPage.MercenariesPage();
		case "/matchlog":
			return WebPage.MatchLogPage();
		case "/alive":
			return WebPage.AlivePage();
		case "":
		case "/":
		case "/home":
			return WebPage.HomePage();
		case "/about":
			return WebPage.AboutPage();
		case "/shell":
			return WebPage.ShellPage();
		case "/webshell":
			return WebPage.Webshell();
		case "/jquery.min.js":
			return new StringBuilder("/*! jQuery v1.12.4 | (c) jQuery Foundation | jquery.org/license */\r\n!function(a,b){\"object\"==typeof module&&\"object\"==typeof module.exports?module.exports=a.document?b(a,!0):function(a){if(!a.document)throw new Error(\"jQuery requires a window with a document\");return b(a)}:b(a)}(\"undefined\"!=typeof window?window:this,function(a,b){var c=[],d=a.document,e=c.slice,f=c.concat,g=c.push,h=c.indexOf,i={},j=i.toString,k=i.hasOwnProperty,l={},m=\"1.12.4\",n=function(a,b){return new n.fn.init(a,b)},o=/^[\\s\\uFEFF\\xA0]+|[\\s\\uFEFF\\xA0]+$/g,p=/^-ms-/,q=/-([\\da-z])/gi,r=function(a,b){return b.toUpperCase()};n.fn=n.prototype={jquery:m,constructor:n,selector:\"\",length:0,toArray:function(){return e.call(this)},get:function(a){return null!=a?0>a?this[a+this.length]:this[a]:e.call(this)},pushStack:function(a){var b=n.merge(this.constructor(),a);return b.prevObject=this,b.context=this.context,b},each:function(a){return n.each(this,a)},map:function(a){return this.pushStack(n.map(this,function(b,c){return a.call(b,c,b)}))},slice:function(){return this.pushStack(e.apply(this,arguments))},first:function(){return this.eq(0)},last:function(){return this.eq(-1)},eq:function(a){var b=this.length,c=+a+(0>a?b:0);return this.pushStack(c>=0&&b>c?[this[c]]:[])},end:function(){return this.prevObject||this.constructor()},push:g,sort:c.sort,splice:c.splice},n.extend=n.fn.extend=function(){var a,b,c,d,e,f,g=arguments[0]||{},h=1,i=arguments.length,j=!1;for(\"boolean\"==typeof g&&(j=g,g=arguments[h]||{},h++),\"object\"==typeof g||n.isFunction(g)||(g={}),h===i&&(g=this,h--);i>h;h++)if(null!=(e=arguments[h]))for(d in e)a=g[d],c=e[d],g!==c&&(j&&c&&(n.isPlainObject(c)||(b=n.isArray(c)))?(b?(b=!1,f=a&&n.isArray(a)?a:[]):f=a&&n.isPlainObject(a)?a:{},g[d]=n.extend(j,f,c)):void 0!==c&&(g[d]=c));return g},n.extend({expando:\"jQuery\"+(m+Math.random()).replace(/\\D/g,\"\"),isReady:!0,error:function(a){throw new Error(a)},noop:function(){},isFunction:function(a){return\"function\"===n.type(a)},isArray:Array.isArray||function(a){return\"array\"===n.type(a)},isWindow:function(a){return null!=a&&a==a.window},isNumeric:function(a){var b=a&&a.toString();return!n.isArray(a)&&b-parseFloat(b)+1>=0},isEmptyObject:function(a){var b;for(b in a)return!1;return!0},isPlainObject:function(a){var b;if(!a||\"object\"!==n.type(a)||a.nodeType||n.isWindow(a))return!1;try{if(a.constructor&&!k.call(a,\"constructor\")&&!k.call(a.constructor.prototype,\"isPrototypeOf\"))return!1}catch(c){return!1}if(!l.ownFirst)for(b in a)return k.call(a,b);for(b in a);return void 0===b||k.call(a,b)},type:function(a){return null==a?a+\"\":\"object\"==typeof a||\"function\"==typeof a?i[j.call(a)]||\"object\":typeof a},globalEval:function(b){b&&n.trim(b)&&(a.execScript||function(b){a.eval.call(a,b)})(b)},camelCase:function(a){return a.replace(p,\"ms-\").replace(q,r)},nodeName:function(a,b){return a.nodeName&&a.nodeName.toLowerCase()===b.toLowerCase()},each:function(a,b){var c,d=0;if(s(a)){for(c=a.length;c>d;d++)if(b.call(a[d],d,a[d])===!1)break}else for(d in a)if(b.call(a[d],d,a[d])===!1)break;return a},trim:function(a){return null==a?\"\":(a+\"\").replace(o,\"\")},makeArray:function(a,b){var c=b||[];return null!=a&&(s(Object(a))?n.merge(c,\"string\"==typeof a?[a]:a):g.call(c,a)),c},inArray:function(a,b,c){var d;if(b){if(h)return h.call(b,a,c);for(d=b.length,c=c?0>c?Math.max(0,d+c):c:0;d>c;c++)if(c in b&&b[c]===a)return c}return-1},merge:function(a,b){var c=+b.length,d=0,e=a.length;while(c>d)a[e++]=b[d++];if(c!==c)while(void 0!==b[d])a[e++]=b[d++];return a.length=e,a},grep:function(a,b,c){for(var d,e=[],f=0,g=a.length,h=!c;g>f;f++)d=!b(a[f],f),d!==h&&e.push(a[f]);return e},map:function(a,b,c){var d,e,g=0,h=[];if(s(a))for(d=a.length;d>g;g++)e=b(a[g],g,c),null!=e&&h.push(e);else for(g in a)e=b(a[g],g,c),null!=e&&h.push(e);return f.apply([],h)},guid:1,proxy:function(a,b){var c,d,f;return\"string\"==typeof b&&(f=a[b],b=a,a=f),n.isFunction(a)?(c=e.call(arguments,2),d=function(){return a.apply(b||this,c.concat(e.call(arguments)))},d.guid=a.guid=a.guid||n.guid++,d):void 0},now:function(){return+new Date},support:l}),\"function\"==typeof Symbol&&(n.fn[Symbol.iterator]=c[Symbol.iterator]),n.each(\"Boolean Number String Function Array Date RegExp Object Error Symbol\".split(\" \"),function(a,b){i[\"[object \"+b+\"]\"]=b.toLowerCase()});function s(a){var b=!!a&&\"length\"in a&&a.length,c=n.type(a);return\"function\"===c||n.isWindow(a)?!1:\"array\"===c||0===b||\"number\"==typeof b&&b>0&&b-1 in a}var t=function(a){var b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u=\"sizzle\"+1*new Date,v=a.document,w=0,x=0,y=ga(),z=ga(),A=ga(),B=function(a,b){return a===b&&(l=!0),0},C=1<<31,D={}.hasOwnProperty,E=[],F=E.pop,G=E.push,H=E.push,I=E.slice,J=function(a,b){for(var c=0,d=a.length;d>c;c++)if(a[c]===b)return c;return-1},K=\"checked|selected|async|autofocus|autoplay|controls|defer|disabled|hidden|ismap|loop|multiple|open|readonly|required|scoped\",L=\"[\\\\x20\\\\t\\\\r\\\\n\\\\f]\",M=\"(?:\\\\\\\\.|[\\\\w-]|[^\\\\x00-\\\\xa0])+\",N=\"\\\\[\"+L+\"*(\"+M+\")(?:\"+L+\"*([*^$|!~]?=)\"+L+\"*(?:'((?:\\\\\\\\.|[^\\\\\\\\'])*)'|\\\"((?:\\\\\\\\.|[^\\\\\\\\\\\"])*)\\\"|(\"+M+\"))|)\"+L+\"*\\\\]\",O=\":(\"+M+\")(?:\\\\((('((?:\\\\\\\\.|[^\\\\\\\\'])*)'|\\\"((?:\\\\\\\\.|[^\\\\\\\\\\\"])*)\\\")|((?:\\\\\\\\.|[^\\\\\\\\()[\\\\]]|\"+N+\")*)|.*)\\\\)|)\",P=new RegExp(L+\"+\",\"g\"),Q=new RegExp(\"^\"+L+\"+|((?:^|[^\\\\\\\\])(?:\\\\\\\\.)*)\"+L+\"+$\",\"g\"),R=new RegExp(\"^\"+L+\"*,\"+L+\"*\"),S=new RegExp(\"^\"+L+\"*([>+~]|\"+L+\")\"+L+\"*\"),T=new RegExp(\"=\"+L+\"*([^\\\\]'\\\"]*?)\"+L+\"*\\\\]\",\"g\"),U=new RegExp(O),V=new RegExp(\"^\"+M+\"$\"),W={ID:new RegExp(\"^#(\"+M+\")\"),CLASS:new RegExp(\"^\\\\.(\"+M+\")\"),TAG:new RegExp(\"^(\"+M+\"|[*])\"),ATTR:new RegExp(\"^\"+N),PSEUDO:new RegExp(\"^\"+O),CHILD:new RegExp(\"^:(only|first|last|nth|nth-last)-(child|of-type)(?:\\\\(\"+L+\"*(even|odd|(([+-]|)(\\\\d*)n|)\"+L+\"*(?:([+-]|)\"+L+\"*(\\\\d+)|))\"+L+\"*\\\\)|)\",\"i\"),bool:new RegExp(\"^(?:\"+K+\")$\",\"i\"),needsContext:new RegExp(\"^\"+L+\"*[>+~]|:(even|odd|eq|gt|lt|nth|first|last)(?:\\\\(\"+L+\"*((?:-\\\\d)?\\\\d*)\"+L+\"*\\\\)|)(?=[^-]|$)\",\"i\")},X=/^(?:input|select|textarea|button)$/i,Y=/^h\\d$/i,Z=/^[^{]+\\{\\s*\\[native \\w/,$=/^(?:#([\\w-]+)|(\\w+)|\\.([\\w-]+))$/,_=/[+~]/,aa=/'|\\\\/g,ba=new RegExp(\"\\\\\\\\([\\\\da-f]{1,6}\"+L+\"?|(\"+L+\")|.)\",\"ig\"),ca=function(a,b,c){var d=\"0x\"+b-65536;return d!==d||c?b:0>d?String.fromCharCode(d+65536):String.fromCharCode(d>>10|55296,1023&d|56320)},da=function(){m()};try{H.apply(E=I.call(v.childNodes),v.childNodes),E[v.childNodes.length].nodeType}catch(ea){H={apply:E.length?function(a,b){G.apply(a,I.call(b))}:function(a,b){var c=a.length,d=0;while(a[c++]=b[d++]);a.length=c-1}}}function fa(a,b,d,e){var f,h,j,k,l,o,r,s,w=b&&b.ownerDocument,x=b?b.nodeType:9;if(d=d||[],\"string\"!=typeof a||!a||1!==x&&9!==x&&11!==x)return d;if(!e&&((b?b.ownerDocument||b:v)!==n&&m(b),b=b||n,p)){if(11!==x&&(o=$.exec(a)))if(f=o[1]){if(9===x){if(!(j=b.getElementById(f)))return d;if(j.id===f)return d.push(j),d}else if(w&&(j=w.getElementById(f))&&t(b,j)&&j.id===f)return d.push(j),d}else{if(o[2])return H.apply(d,b.getElementsByTagName(a)),d;if((f=o[3])&&c.getElementsByClassName&&b.getElementsByClassName)return H.apply(d,b.getElementsByClassName(f)),d}if(c.qsa&&!A[a+\" \"]&&(!q||!q.test(a))){if(1!==x)w=b,s=a;else if(\"object\"!==b.nodeName.toLowerCase()){(k=b.getAttribute(\"id\"))?k=k.replace(aa,\"\\\\$&\"):b.setAttribute(\"id\",k=u),r=g(a),h=r.length,l=V.test(k)?\"#\"+k:\"[id='\"+k+\"']\";while(h--)r[h]=l+\" \"+qa(r[h]);s=r.join(\",\"),w=_.test(a)&&oa(b.parentNode)||b}if(s)try{return H.apply(d,w.querySelectorAll(s)),d}catch(y){}finally{k===u&&b.removeAttribute(\"id\")}}}return i(a.replace(Q,\"$1\"),b,d,e)}function ga(){var a=[];function b(c,e){return a.push(c+\" \")>d.cacheLength&&delete b[a.shift()],b[c+\" \"]=e}return b}function ha(a){return a[u]=!0,a}function ia(a){var b=n.createElement(\"div\");try{return!!a(b)}catch(c){return!1}finally{b.parentNode&&b.parentNode.removeChild(b),b=null}}function ja(a,b){var c=a.split(\"|\"),e=c.length;while(e--)d.attrHandle[c[e]]=b}function ka(a,b){var c=b&&a,d=c&&1===a.nodeType&&1===b.nodeType&&(~b.sourceIndex||C)-(~a.sourceIndex||C);if(d)return d;if(c)while(c=c.nextSibling)if(c===b)return-1;return a?1:-1}function la(a){return function(b){var c=b.nodeName.toLowerCase();return\"input\"===c&&b.type===a}}function ma(a){return function(b){var c=b.nodeName.toLowerCase();return(\"input\"===c||\"button\"===c)&&b.type===a}}function na(a){return ha(function(b){return b=+b,ha(function(c,d){var e,f=a([],c.length,b),g=f.length;while(g--)c[e=f[g]]&&(c[e]=!(d[e]=c[e]))})})}function oa(a){return a&&\"undefined\"!=typeof a.getElementsByTagName&&a}c=fa.support={},f=fa.isXML=function(a){var b=a&&(a.ownerDocument||a).documentElement;return b?\"HTML\"!==b.nodeName:!1},m=fa.setDocument=function(a){var b,e,g=a?a.ownerDocument||a:v;return g!==n&&9===g.nodeType&&g.documentElement?(n=g,o=n.documentElement,p=!f(n),(e=n.defaultView)&&e.top!==e&&(e.addEventListener?e.addEventListener(\"unload\",da,!1):e.attachEvent&&e.attachEvent(\"onunload\",da)),c.attributes=ia(function(a){return a.className=\"i\",!a.getAttribute(\"className\")}),c.getElementsByTagName=ia(function(a){return a.appendChild(n.createComment(\"\")),!a.getElementsByTagName(\"*\").length}),c.getElementsByClassName=Z.test(n.getElementsByClassName),c.getById=ia(function(a){return o.appendChild(a).id=u,!n.getElementsByName||!n.getElementsByName(u).length}),c.getById?(d.find.ID=function(a,b){if(\"undefined\"!=typeof b.getElementById&&p){var c=b.getElementById(a);return c?[c]:[]}},d.filter.ID=function(a){var b=a.replace(ba,ca);return function(a){return a.getAttribute(\"id\")===b}}):(delete d.find.ID,d.filter.ID=function(a){var b=a.replace(ba,ca);return function(a){var c=\"undefined\"!=typeof a.getAttributeNode&&a.getAttributeNode(\"id\");return c&&c.value===b}}),d.find.TAG=c.getElementsByTagName?function(a,b){return\"undefined\"!=typeof b.getElementsByTagName?b.getElementsByTagName(a):c.qsa?b.querySelectorAll(a):void 0}:function(a,b){var c,d=[],e=0,f=b.getElementsByTagName(a);if(\"*\"===a){while(c=f[e++])1===c.nodeType&&d.push(c);return d}return f},d.find.CLASS=c.getElementsByClassName&&function(a,b){return\"undefined\"!=typeof b.getElementsByClassName&&p?b.getElementsByClassName(a):void 0},r=[],q=[],(c.qsa=Z.test(n.querySelectorAll))&&(ia(function(a){o.appendChild(a).innerHTML=\"<a id='\"+u+\"'></a><select id='\"+u+\"-\\r\\\\' msallowcapture=''><option selected=''></option></select>\",a.querySelectorAll(\"[msallowcapture^='']\").length&&q.push(\"[*^$]=\"+L+\"*(?:''|\\\"\\\")\"),a.querySelectorAll(\"[selected]\").length||q.push(\"\\\\[\"+L+\"*(?:value|\"+K+\")\"),a.querySelectorAll(\"[id~=\"+u+\"-]\").length||q.push(\"~=\"),a.querySelectorAll(\":checked\").length||q.push(\":checked\"),a.querySelectorAll(\"a#\"+u+\"+*\").length||q.push(\".#.+[+~]\")}),ia(function(a){var b=n.createElement(\"input\");b.setAttribute(\"type\",\"hidden\"),a.appendChild(b).setAttribute(\"name\",\"D\"),a.querySelectorAll(\"[name=d]\").length&&q.push(\"name\"+L+\"*[*^$|!~]?=\"),a.querySelectorAll(\":enabled\").length||q.push(\":enabled\",\":disabled\"),a.querySelectorAll(\"*,:x\"),q.push(\",.*:\")})),(c.matchesSelector=Z.test(s=o.matches||o.webkitMatchesSelector||o.mozMatchesSelector||o.oMatchesSelector||o.msMatchesSelector))&&ia(function(a){c.disconnectedMatch=s.call(a,\"div\"),s.call(a,\"[s!='']:x\"),r.push(\"!=\",O)}),q=q.length&&new RegExp(q.join(\"|\")),r=r.length&&new RegExp(r.join(\"|\")),b=Z.test(o.compareDocumentPosition),t=b||Z.test(o.contains)?function(a,b){var c=9===a.nodeType?a.documentElement:a,d=b&&b.parentNode;return a===d||!(!d||1!==d.nodeType||!(c.contains?c.contains(d):a.compareDocumentPosition&&16&a.compareDocumentPosition(d)))}:function(a,b){if(b)while(b=b.parentNode)if(b===a)return!0;return!1},B=b?function(a,b){if(a===b)return l=!0,0;var d=!a.compareDocumentPosition-!b.compareDocumentPosition;return d?d:(d=(a.ownerDocument||a)===(b.ownerDocument||b)?a.compareDocumentPosition(b):1,1&d||!c.sortDetached&&b.compareDocumentPosition(a)===d?a===n||a.ownerDocument===v&&t(v,a)?-1:b===n||b.ownerDocument===v&&t(v,b)?1:k?J(k,a)-J(k,b):0:4&d?-1:1)}:function(a,b){if(a===b)return l=!0,0;var c,d=0,e=a.parentNode,f=b.parentNode,g=[a],h=[b];if(!e||!f)return a===n?-1:b===n?1:e?-1:f?1:k?J(k,a)-J(k,b):0;if(e===f)return ka(a,b);c=a;while(c=c.parentNode)g.unshift(c);c=b;while(c=c.parentNode)h.unshift(c);while(g[d]===h[d])d++;return d?ka(g[d],h[d]):g[d]===v?-1:h[d]===v?1:0},n):n},fa.matches=function(a,b){return fa(a,null,null,b)},fa.matchesSelector=function(a,b){if((a.ownerDocument||a)!==n&&m(a),b=b.replace(T,\"='$1']\"),c.matchesSelector&&p&&!A[b+\" \"]&&(!r||!r.test(b))&&(!q||!q.test(b)))try{var d=s.call(a,b);if(d||c.disconnectedMatch||a.document&&11!==a.document.nodeType)return d}catch(e){}return fa(b,n,null,[a]).length>0},fa.contains=function(a,b){return(a.ownerDocument||a)!==n&&m(a),t(a,b)},fa.attr=function(a,b){(a.ownerDocument||a)!==n&&m(a);var e=d.attrHandle[b.toLowerCase()],f=e&&D.call(d.attrHandle,b.toLowerCase())?e(a,b,!p):void 0;return void 0!==f?f:c.attributes||!p?a.getAttribute(b):(f=a.getAttributeNode(b))&&f.specified?f.value:null},fa.error=function(a){throw new Error(\"Syntax error, unrecognized expression: \"+a)},fa.uniqueSort=function(a){var b,d=[],e=0,f=0;if(l=!c.detectDuplicates,k=!c.sortStable&&a.slice(0),a.sort(B),l){while(b=a[f++])b===a[f]&&(e=d.push(f));while(e--)a.splice(d[e],1)}return k=null,a},e=fa.getText=function(a){var b,c=\"\",d=0,f=a.nodeType;if(f){if(1===f||9===f||11===f){if(\"string\"==typeof a.textContent)return a.textContent;for(a=a.firstChild;a;a=a.nextSibling)c+=e(a)}else if(3===f||4===f)return a.nodeValue}else while(b=a[d++])c+=e(b);return c},d=fa.selectors={cacheLength:50,createPseudo:ha,match:W,attrHandle:{},find:{},relative:{\">\":{dir:\"parentNode\",first:!0},\" \":{dir:\"parentNode\"},\"+\":{dir:\"previousSibling\",first:!0},\"~\":{dir:\"previousSibling\"}},preFilter:{ATTR:function(a){return a[1]=a[1].replace(ba,ca),a[3]=(a[3]||a[4]||a[5]||\"\").replace(ba,ca),\"~=\"===a[2]&&(a[3]=\" \"+a[3]+\" \"),a.slice(0,4)},CHILD:function(a){return a[1]=a[1].toLowerCase(),\"nth\"===a[1].slice(0,3)?(a[3]||fa.error(a[0]),a[4]=+(a[4]?a[5]+(a[6]||1):2*(\"even\"===a[3]||\"odd\"===a[3])),a[5]=+(a[7]+a[8]||\"odd\"===a[3])):a[3]&&fa.error(a[0]),a},PSEUDO:function(a){var b,c=!a[6]&&a[2];return W.CHILD.test(a[0])?null:(a[3]?a[2]=a[4]||a[5]||\"\":c&&U.test(c)&&(b=g(c,!0))&&(b=c.indexOf(\")\",c.length-b)-c.length)&&(a[0]=a[0].slice(0,b),a[2]=c.slice(0,b)),a.slice(0,3))}},filter:{TAG:function(a){var b=a.replace(ba,ca).toLowerCase();return\"*\"===a?function(){return!0}:function(a){return a.nodeName&&a.nodeName.toLowerCase()===b}},CLASS:function(a){var b=y[a+\" \"];return b||(b=new RegExp(\"(^|\"+L+\")\"+a+\"(\"+L+\"|$)\"))&&y(a,function(a){return b.test(\"string\"==typeof a.className&&a.className||\"undefined\"!=typeof a.getAttribute&&a.getAttribute(\"class\")||\"\")})},ATTR:function(a,b,c){return function(d){var e=fa.attr(d,a);return null==e?\"!=\"===b:b?(e+=\"\",\"=\"===b?e===c:\"!=\"===b?e!==c:\"^=\"===b?c&&0===e.indexOf(c):\"*=\"===b?c&&e.indexOf(c)>-1:\"$=\"===b?c&&e.slice(-c.length)===c:\"~=\"===b?(\" \"+e.replace(P,\" \")+\" \").indexOf(c)>-1:\"|=\"===b?e===c||e.slice(0,c.length+1)===c+\"-\":!1):!0}},CHILD:function(a,b,c,d,e){var f=\"nth\"!==a.slice(0,3),g=\"last\"!==a.slice(-4),h=\"of-type\"===b;return 1===d&&0===e?function(a){return!!a.parentNode}:function(b,c,i){var j,k,l,m,n,o,p=f!==g?\"nextSibling\":\"previousSibling\",q=b.parentNode,r=h&&b.nodeName.toLowerCase(),s=!i&&!h,t=!1;if(q){if(f){while(p){m=b;while(m=m[p])if(h?m.nodeName.toLowerCase()===r:1===m.nodeType)return!1;o=p=\"only\"===a&&!o&&\"nextSibling\"}return!0}if(o=[g?q.firstChild:q.lastChild],g&&s){m=q,l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),j=k[a]||[],n=j[0]===w&&j[1],t=n&&j[2],m=n&&q.childNodes[n];while(m=++n&&m&&m[p]||(t=n=0)||o.pop())if(1===m.nodeType&&++t&&m===b){k[a]=[w,n,t];break}}else if(s&&(m=b,l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),j=k[a]||[],n=j[0]===w&&j[1],t=n),t===!1)while(m=++n&&m&&m[p]||(t=n=0)||o.pop())if((h?m.nodeName.toLowerCase()===r:1===m.nodeType)&&++t&&(s&&(l=m[u]||(m[u]={}),k=l[m.uniqueID]||(l[m.uniqueID]={}),k[a]=[w,t]),m===b))break;return t-=e,t===d||t%d===0&&t/d>=0}}},PSEUDO:function(a,b){var c,e=d.pseudos[a]||d.setFilters[a.toLowerCase()]||fa.error(\"unsupported pseudo: \"+a);return e[u]?e(b):e.length>1?(c=[a,a,\"\",b],d.setFilters.hasOwnProperty(a.toLowerCase())?ha(function(a,c){var d,f=e(a,b),g=f.length;while(g--)d=J(a,f[g]),a[d]=!(c[d]=f[g])}):function(a){return e(a,0,c)}):e}},pseudos:{not:ha(function(a){var b=[],c=[],d=h(a.replace(Q,\"$1\"));return d[u]?ha(function(a,b,c,e){var f,g=d(a,null,e,[]),h=a.length;while(h--)(f=g[h])&&(a[h]=!(b[h]=f))}):function(a,e,f){return b[0]=a,d(b,null,f,c),b[0]=null,!c.pop()}}),has:ha(function(a){return function(b){return fa(a,b).length>0}}),contains:ha(function(a){return a=a.replace(ba,ca),function(b){return(b.textContent||b.innerText||e(b)).indexOf(a)>-1}}),lang:ha(function(a){return V.test(a||\"\")||fa.error(\"unsupported lang: \"+a),a=a.replace(ba,ca).toLowerCase(),function(b){var c;do if(c=p?b.lang:b.getAttribute(\"xml:lang\")||b.getAttribute(\"lang\"))return c=c.toLowerCase(),c===a||0===c.indexOf(a+\"-\");while((b=b.parentNode)&&1===b.nodeType);return!1}}),target:function(b){var c=a.location&&a.location.hash;return c&&c.slice(1)===b.id},root:function(a){return a===o},focus:function(a){return a===n.activeElement&&(!n.hasFocus||n.hasFocus())&&!!(a.type||a.href||~a.tabIndex)},enabled:function(a){return a.disabled===!1},disabled:function(a){return a.disabled===!0},checked:function(a){var b=a.nodeName.toLowerCase();return\"input\"===b&&!!a.checked||\"option\"===b&&!!a.selected},selected:function(a){return a.parentNode&&a.parentNode.selectedIndex,a.selected===!0},empty:function(a){for(a=a.firstChild;a;a=a.nextSibling)if(a.nodeType<6)return!1;return!0},parent:function(a){return!d.pseudos.empty(a)},header:function(a){return Y.test(a.nodeName)},input:function(a){return X.test(a.nodeName)},button:function(a){var b=a.nodeName.toLowerCase();return\"input\"===b&&\"button\"===a.type||\"button\"===b},text:function(a){var b;return\"input\"===a.nodeName.toLowerCase()&&\"text\"===a.type&&(null==(b=a.getAttribute(\"type\"))||\"text\"===b.toLowerCase())},first:na(function(){return[0]}),last:na(function(a,b){return[b-1]}),eq:na(function(a,b,c){return[0>c?c+b:c]}),even:na(function(a,b){for(var c=0;b>c;c+=2)a.push(c);return a}),odd:na(function(a,b){for(var c=1;b>c;c+=2)a.push(c);return a}),lt:na(function(a,b,c){for(var d=0>c?c+b:c;--d>=0;)a.push(d);return a}),gt:na(function(a,b,c){for(var d=0>c?c+b:c;++d<b;)a.push(d);return a})}},d.pseudos.nth=d.pseudos.eq;for(b in{radio:!0,checkbox:!0,file:!0,password:!0,image:!0})d.pseudos[b]=la(b);for(b in{submit:!0,reset:!0})d.pseudos[b]=ma(b);function pa(){}pa.prototype=d.filters=d.pseudos,d.setFilters=new pa,g=fa.tokenize=function(a,b){var c,e,f,g,h,i,j,k=z[a+\" \"];if(k)return b?0:k.slice(0);h=a,i=[],j=d.preFilter;while(h){c&&!(e=R.exec(h))||(e&&(h=h.slice(e[0].length)||h),i.push(f=[])),c=!1,(e=S.exec(h))&&(c=e.shift(),f.push({value:c,type:e[0].replace(Q,\" \")}),h=h.slice(c.length));for(g in d.filter)!(e=W[g].exec(h))||j[g]&&!(e=j[g](e))||(c=e.shift(),f.push({value:c,type:g,matches:e}),h=h.slice(c.length));if(!c)break}return b?h.length:h?fa.error(a):z(a,i).slice(0)};function qa(a){for(var b=0,c=a.length,d=\"\";c>b;b++)d+=a[b].value;return d}function ra(a,b,c){var d=b.dir,e=c&&\"parentNode\"===d,f=x++;return b.first?function(b,c,f){while(b=b[d])if(1===b.nodeType||e)return a(b,c,f)}:function(b,c,g){var h,i,j,k=[w,f];if(g){while(b=b[d])if((1===b.nodeType||e)&&a(b,c,g))return!0}else while(b=b[d])if(1===b.nodeType||e){if(j=b[u]||(b[u]={}),i=j[b.uniqueID]||(j[b.uniqueID]={}),(h=i[d])&&h[0]===w&&h[1]===f)return k[2]=h[2];if(i[d]=k,k[2]=a(b,c,g))return!0}}}function sa(a){return a.length>1?function(b,c,d){var e=a.length;while(e--)if(!a[e](b,c,d))return!1;return!0}:a[0]}function ta(a,b,c){for(var d=0,e=b.length;e>d;d++)fa(a,b[d],c);return c}function ua(a,b,c,d,e){for(var f,g=[],h=0,i=a.length,j=null!=b;i>h;h++)(f=a[h])&&(c&&!c(f,d,e)||(g.push(f),j&&b.push(h)));return g}function va(a,b,c,d,e,f){return d&&!d[u]&&(d=va(d)),e&&!e[u]&&(e=va(e,f)),ha(function(f,g,h,i){var j,k,l,m=[],n=[],o=g.length,p=f||ta(b||\"*\",h.nodeType?[h]:h,[]),q=!a||!f&&b?p:ua(p,m,a,h,i),r=c?e||(f?a:o||d)?[]:g:q;if(c&&c(q,r,h,i),d){j=ua(r,n),d(j,[],h,i),k=j.length;while(k--)(l=j[k])&&(r[n[k]]=!(q[n[k]]=l))}if(f){if(e||a){if(e){j=[],k=r.length;while(k--)(l=r[k])&&j.push(q[k]=l);e(null,r=[],j,i)}k=r.length;while(k--)(l=r[k])&&(j=e?J(f,l):m[k])>-1&&(f[j]=!(g[j]=l))}}else r=ua(r===g?r.splice(o,r.length):r),e?e(null,g,r,i):H.apply(g,r)})}function wa(a){for(var b,c,e,f=a.length,g=d.relative[a[0].type],h=g||d.relative[\" \"],i=g?1:0,k=ra(function(a){return a===b},h,!0),l=ra(function(a){return J(b,a)>-1},h,!0),m=[function(a,c,d){var e=!g&&(d||c!==j)||((b=c).nodeType?k(a,c,d):l(a,c,d));return b=null,e}];f>i;i++)if(c=d.relative[a[i].type])m=[ra(sa(m),c)];else{if(c=d.filter[a[i].type].apply(null,a[i].matches),c[u]){for(e=++i;f>e;e++)if(d.relative[a[e].type])break;return va(i>1&&sa(m),i>1&&qa(a.slice(0,i-1).concat({value:\" \"===a[i-2].type?\"*\":\"\"})).replace(Q,\"$1\"),c,e>i&&wa(a.slice(i,e)),f>e&&wa(a=a.slice(e)),f>e&&qa(a))}m.push(c)}return sa(m)}function xa(a,b){var c=b.length>0,e=a.length>0,f=function(f,g,h,i,k){var l,o,q,r=0,s=\"0\",t=f&&[],u=[],v=j,x=f||e&&d.find.TAG(\"*\",k),y=w+=null==v?1:Math.random()||.1,z=x.length;for(k&&(j=g===n||g||k);s!==z&&null!=(l=x[s]);s++){if(e&&l){o=0,g||l.ownerDocument===n||(m(l),h=!p);while(q=a[o++])if(q(l,g||n,h)){i.push(l);break}k&&(w=y)}c&&((l=!q&&l)&&r--,f&&t.push(l))}if(r+=s,c&&s!==r){o=0;while(q=b[o++])q(t,u,g,h);if(f){if(r>0)while(s--)t[s]||u[s]||(u[s]=F.call(i));u=ua(u)}H.apply(i,u),k&&!f&&u.length>0&&r+b.length>1&&fa.uniqueSort(i)}return k&&(w=y,j=v),t};return c?ha(f):f}return h=fa.compile=function(a,b){var c,d=[],e=[],f=A[a+\" \"];if(!f){b||(b=g(a)),c=b.length;while(c--)f=wa(b[c]),f[u]?d.push(f):e.push(f);f=A(a,xa(e,d)),f.selector=a}return f},i=fa.select=function(a,b,e,f){var i,j,k,l,m,n=\"function\"==typeof a&&a,o=!f&&g(a=n.selector||a);if(e=e||[],1===o.length){if(j=o[0]=o[0].slice(0),j.length>2&&\"ID\"===(k=j[0]).type&&c.getById&&9===b.nodeType&&p&&d.relative[j[1].type]){if(b=(d.find.ID(k.matches[0].replace(ba,ca),b)||[])[0],!b)return e;n&&(b=b.parentNode),a=a.slice(j.shift().value.length)}i=W.needsContext.test(a)?0:j.length;while(i--){if(k=j[i],d.relative[l=k.type])break;if((m=d.find[l])&&(f=m(k.matches[0].replace(ba,ca),_.test(j[0].type)&&oa(b.parentNode)||b))){if(j.splice(i,1),a=f.length&&qa(j),!a)return H.apply(e,f),e;break}}}return(n||h(a,o))(f,b,!p,e,!b||_.test(a)&&oa(b.parentNode)||b),e},c.sortStable=u.split(\"\").sort(B).join(\"\")===u,c.detectDuplicates=!!l,m(),c.sortDetached=ia(function(a){return 1&a.compareDocumentPosition(n.createElement(\"div\"))}),ia(function(a){return a.innerHTML=\"<a href='#'></a>\",\"#\"===a.firstChild.getAttribute(\"href\")})||ja(\"type|href|height|width\",function(a,b,c){return c?void 0:a.getAttribute(b,\"type\"===b.toLowerCase()?1:2)}),c.attributes&&ia(function(a){return a.innerHTML=\"<input/>\",a.firstChild.setAttribute(\"value\",\"\"),\"\"===a.firstChild.getAttribute(\"value\")})||ja(\"value\",function(a,b,c){return c||\"input\"!==a.nodeName.toLowerCase()?void 0:a.defaultValue}),ia(function(a){return null==a.getAttribute(\"disabled\")})||ja(K,function(a,b,c){var d;return c?void 0:a[b]===!0?b.toLowerCase():(d=a.getAttributeNode(b))&&d.specified?d.value:null}),fa}(a);n.find=t,n.expr=t.selectors,n.expr[\":\"]=n.expr.pseudos,n.uniqueSort=n.unique=t.uniqueSort,n.text=t.getText,n.isXMLDoc=t.isXML,n.contains=t.contains;var u=function(a,b,c){var d=[],e=void 0!==c;while((a=a[b])&&9!==a.nodeType)if(1===a.nodeType){if(e&&n(a).is(c))break;d.push(a)}return d},v=function(a,b){for(var c=[];a;a=a.nextSibling)1===a.nodeType&&a!==b&&c.push(a);return c},w=n.expr.match.needsContext,x=/^<([\\w-]+)\\s*\\/?>(?:<\\/\\1>|)$/,y=/^.[^:#\\[\\.,]*$/;function z(a,b,c){if(n.isFunction(b))return n.grep(a,function(a,d){return!!b.call(a,d,a)!==c});if(b.nodeType)return n.grep(a,function(a){return a===b!==c});if(\"string\"==typeof b){if(y.test(b))return n.filter(b,a,c);b=n.filter(b,a)}return n.grep(a,function(a){return n.inArray(a,b)>-1!==c})}n.filter=function(a,b,c){var d=b[0];return c&&(a=\":not(\"+a+\")\"),1===b.length&&1===d.nodeType?n.find.matchesSelector(d,a)?[d]:[]:n.find.matches(a,n.grep(b,function(a){return 1===a.nodeType}))},n.fn.extend({find:function(a){var b,c=[],d=this,e=d.length;if(\"string\"!=typeof a)return this.pushStack(n(a).filter(function(){for(b=0;e>b;b++)if(n.contains(d[b],this))return!0}));for(b=0;e>b;b++)n.find(a,d[b],c);return c=this.pushStack(e>1?n.unique(c):c),c.selector=this.selector?this.selector+\" \"+a:a,c},filter:function(a){return this.pushStack(z(this,a||[],!1))},not:function(a){return this.pushStack(z(this,a||[],!0))},is:function(a){return!!z(this,\"string\"==typeof a&&w.test(a)?n(a):a||[],!1).length}});var A,B=/^(?:\\s*(<[\\w\\W]+>)[^>]*|#([\\w-]*))$/,C=n.fn.init=function(a,b,c){var e,f;if(!a)return this;if(c=c||A,\"string\"==typeof a){if(e=\"<\"===a.charAt(0)&&\">\"===a.charAt(a.length-1)&&a.length>=3?[null,a,null]:B.exec(a),!e||!e[1]&&b)return!b||b.jquery?(b||c).find(a):this.constructor(b).find(a);if(e[1]){if(b=b instanceof n?b[0]:b,n.merge(this,n.parseHTML(e[1],b&&b.nodeType?b.ownerDocument||b:d,!0)),x.test(e[1])&&n.isPlainObject(b))for(e in b)n.isFunction(this[e])?this[e](b[e]):this.attr(e,b[e]);return this}if(f=d.getElementById(e[2]),f&&f.parentNode){if(f.id!==e[2])return A.find(a);this.length=1,this[0]=f}return this.context=d,this.selector=a,this}return a.nodeType?(this.context=this[0]=a,this.length=1,this):n.isFunction(a)?\"undefined\"!=typeof c.ready?c.ready(a):a(n):(void 0!==a.selector&&(this.selector=a.selector,this.context=a.context),n.makeArray(a,this))};C.prototype=n.fn,A=n(d);var D=/^(?:parents|prev(?:Until|All))/,E={children:!0,contents:!0,next:!0,prev:!0};n.fn.extend({has:function(a){var b,c=n(a,this),d=c.length;return this.filter(function(){for(b=0;d>b;b++)if(n.contains(this,c[b]))return!0})},closest:function(a,b){for(var c,d=0,e=this.length,f=[],g=w.test(a)||\"string\"!=typeof a?n(a,b||this.context):0;e>d;d++)for(c=this[d];c&&c!==b;c=c.parentNode)if(c.nodeType<11&&(g?g.index(c)>-1:1===c.nodeType&&n.find.matchesSelector(c,a))){f.push(c);break}return this.pushStack(f.length>1?n.uniqueSort(f):f)},index:function(a){return a?\"string\"==typeof a?n.inArray(this[0],n(a)):n.inArray(a.jquery?a[0]:a,this):this[0]&&this[0].parentNode?this.first().prevAll().length:-1},add:function(a,b){return this.pushStack(n.uniqueSort(n.merge(this.get(),n(a,b))))},addBack:function(a){return this.add(null==a?this.prevObject:this.prevObject.filter(a))}});function F(a,b){do a=a[b];while(a&&1!==a.nodeType);return a}n.each({parent:function(a){var b=a.parentNode;return b&&11!==b.nodeType?b:null},parents:function(a){return u(a,\"parentNode\")},parentsUntil:function(a,b,c){return u(a,\"parentNode\",c)},next:function(a){return F(a,\"nextSibling\")},prev:function(a){return F(a,\"previousSibling\")},nextAll:function(a){return u(a,\"nextSibling\")},prevAll:function(a){return u(a,\"previousSibling\")},nextUntil:function(a,b,c){return u(a,\"nextSibling\",c)},prevUntil:function(a,b,c){return u(a,\"previousSibling\",c)},siblings:function(a){return v((a.parentNode||{}).firstChild,a)},children:function(a){return v(a.firstChild)},contents:function(a){return n.nodeName(a,\"iframe\")?a.contentDocument||a.contentWindow.document:n.merge([],a.childNodes)}},function(a,b){n.fn[a]=function(c,d){var e=n.map(this,b,c);return\"Until\"!==a.slice(-5)&&(d=c),d&&\"string\"==typeof d&&(e=n.filter(d,e)),this.length>1&&(E[a]||(e=n.uniqueSort(e)),D.test(a)&&(e=e.reverse())),this.pushStack(e)}});var G=/\\S+/g;function H(a){var b={};return n.each(a.match(G)||[],function(a,c){b[c]=!0}),b}n.Callbacks=function(a){a=\"string\"==typeof a?H(a):n.extend({},a);var b,c,d,e,f=[],g=[],h=-1,i=function(){for(e=a.once,d=b=!0;g.length;h=-1){c=g.shift();while(++h<f.length)f[h].apply(c[0],c[1])===!1&&a.stopOnFalse&&(h=f.length,c=!1)}a.memory||(c=!1),b=!1,e&&(f=c?[]:\"\")},j={add:function(){return f&&(c&&!b&&(h=f.length-1,g.push(c)),function d(b){n.each(b,function(b,c){n.isFunction(c)?a.unique&&j.has(c)||f.push(c):c&&c.length&&\"string\"!==n.type(c)&&d(c)})}(arguments),c&&!b&&i()),this},remove:function(){return n.each(arguments,function(a,b){var c;while((c=n.inArray(b,f,c))>-1)f.splice(c,1),h>=c&&h--}),this},has:function(a){return a?n.inArray(a,f)>-1:f.length>0},empty:function(){return f&&(f=[]),this},disable:function(){return e=g=[],f=c=\"\",this},disabled:function(){return!f},lock:function(){return e=!0,c||j.disable(),this},locked:function(){return!!e},fireWith:function(a,c){return e||(c=c||[],c=[a,c.slice?c.slice():c],g.push(c),b||i()),this},fire:function(){return j.fireWith(this,arguments),this},fired:function(){return!!d}};return j},n.extend({Deferred:function(a){var b=[[\"resolve\",\"done\",n.Callbacks(\"once memory\"),\"resolved\"],[\"reject\",\"fail\",n.Callbacks(\"once memory\"),\"rejected\"],[\"notify\",\"progress\",n.Callbacks(\"memory\")]],c=\"pending\",d={state:function(){return c},always:function(){return e.done(arguments).fail(arguments),this},then:function(){var a=arguments;return n.Deferred(function(c){n.each(b,function(b,f){var g=n.isFunction(a[b])&&a[b];e[f[1]](function(){var a=g&&g.apply(this,arguments);a&&n.isFunction(a.promise)?a.promise().progress(c.notify).done(c.resolve).fail(c.reject):c[f[0]+\"With\"](this===d?c.promise():this,g?[a]:arguments)})}),a=null}).promise()},promise:function(a){return null!=a?n.extend(a,d):d}},e={};return d.pipe=d.then,n.each(b,function(a,f){var g=f[2],h=f[3];d[f[1]]=g.add,h&&g.add(function(){c=h},b[1^a][2].disable,b[2][2].lock),e[f[0]]=function(){return e[f[0]+\"With\"](this===e?d:this,arguments),this},e[f[0]+\"With\"]=g.fireWith}),d.promise(e),a&&a.call(e,e),e},when:function(a){var b=0,c=e.call(arguments),d=c.length,f=1!==d||a&&n.isFunction(a.promise)?d:0,g=1===f?a:n.Deferred(),h=function(a,b,c){return function(d){b[a]=this,c[a]=arguments.length>1?e.call(arguments):d,c===i?g.notifyWith(b,c):--f||g.resolveWith(b,c)}},i,j,k;if(d>1)for(i=new Array(d),j=new Array(d),k=new Array(d);d>b;b++)c[b]&&n.isFunction(c[b].promise)?c[b].promise().progress(h(b,j,i)).done(h(b,k,c)).fail(g.reject):--f;return f||g.resolveWith(k,c),g.promise()}});var I;n.fn.ready=function(a){return n.ready.promise().done(a),this},n.extend({isReady:!1,readyWait:1,holdReady:function(a){a?n.readyWait++:n.ready(!0)},ready:function(a){(a===!0?--n.readyWait:n.isReady)||(n.isReady=!0,a!==!0&&--n.readyWait>0||(I.resolveWith(d,[n]),n.fn.triggerHandler&&(n(d).triggerHandler(\"ready\"),n(d).off(\"ready\"))))}});function J(){d.addEventListener?(d.removeEventListener(\"DOMContentLoaded\",K),a.removeEventListener(\"load\",K)):(d.detachEvent(\"onreadystatechange\",K),a.detachEvent(\"onload\",K))}function K(){(d.addEventListener||\"load\"===a.event.type||\"complete\"===d.readyState)&&(J(),n.ready())}n.ready.promise=function(b){if(!I)if(I=n.Deferred(),\"complete\"===d.readyState||\"loading\"!==d.readyState&&!d.documentElement.doScroll)a.setTimeout(n.ready);else if(d.addEventListener)d.addEventListener(\"DOMContentLoaded\",K),a.addEventListener(\"load\",K);else{d.attachEvent(\"onreadystatechange\",K),a.attachEvent(\"onload\",K);var c=!1;try{c=null==a.frameElement&&d.documentElement}catch(e){}c&&c.doScroll&&!function f(){if(!n.isReady){try{c.doScroll(\"left\")}catch(b){return a.setTimeout(f,50)}J(),n.ready()}}()}return I.promise(b)},n.ready.promise();var L;for(L in n(l))break;l.ownFirst=\"0\"===L,l.inlineBlockNeedsLayout=!1,n(function(){var a,b,c,e;c=d.getElementsByTagName(\"body\")[0],c&&c.style&&(b=d.createElement(\"div\"),e=d.createElement(\"div\"),e.style.cssText=\"position:absolute;border:0;width:0;height:0;top:0;left:-9999px\",c.appendChild(e).appendChild(b),\"undefined\"!=typeof b.style.zoom&&(b.style.cssText=\"display:inline;margin:0;border:0;padding:1px;width:1px;zoom:1\",l.inlineBlockNeedsLayout=a=3===b.offsetWidth,a&&(c.style.zoom=1)),c.removeChild(e))}),function(){var a=d.createElement(\"div\");l.deleteExpando=!0;try{delete a.test}catch(b){l.deleteExpando=!1}a=null}();var M=function(a){var b=n.noData[(a.nodeName+\" \").toLowerCase()],c=+a.nodeType||1;return 1!==c&&9!==c?!1:!b||b!==!0&&a.getAttribute(\"classid\")===b},N=/^(?:\\{[\\w\\W]*\\}|\\[[\\w\\W]*\\])$/,O=/([A-Z])/g;function P(a,b,c){if(void 0===c&&1===a.nodeType){var d=\"data-\"+b.replace(O,\"-$1\").toLowerCase();if(c=a.getAttribute(d),\"string\"==typeof c){try{c=\"true\"===c?!0:\"false\"===c?!1:\"null\"===c?null:+c+\"\"===c?+c:N.test(c)?n.parseJSON(c):c}catch(e){}n.data(a,b,c)}else c=void 0;\r\n}return c}function Q(a){var b;for(b in a)if((\"data\"!==b||!n.isEmptyObject(a[b]))&&\"toJSON\"!==b)return!1;return!0}function R(a,b,d,e){if(M(a)){var f,g,h=n.expando,i=a.nodeType,j=i?n.cache:a,k=i?a[h]:a[h]&&h;if(k&&j[k]&&(e||j[k].data)||void 0!==d||\"string\"!=typeof b)return k||(k=i?a[h]=c.pop()||n.guid++:h),j[k]||(j[k]=i?{}:{toJSON:n.noop}),\"object\"!=typeof b&&\"function\"!=typeof b||(e?j[k]=n.extend(j[k],b):j[k].data=n.extend(j[k].data,b)),g=j[k],e||(g.data||(g.data={}),g=g.data),void 0!==d&&(g[n.camelCase(b)]=d),\"string\"==typeof b?(f=g[b],null==f&&(f=g[n.camelCase(b)])):f=g,f}}function S(a,b,c){if(M(a)){var d,e,f=a.nodeType,g=f?n.cache:a,h=f?a[n.expando]:n.expando;if(g[h]){if(b&&(d=c?g[h]:g[h].data)){n.isArray(b)?b=b.concat(n.map(b,n.camelCase)):b in d?b=[b]:(b=n.camelCase(b),b=b in d?[b]:b.split(\" \")),e=b.length;while(e--)delete d[b[e]];if(c?!Q(d):!n.isEmptyObject(d))return}(c||(delete g[h].data,Q(g[h])))&&(f?n.cleanData([a],!0):l.deleteExpando||g!=g.window?delete g[h]:g[h]=void 0)}}}n.extend({cache:{},noData:{\"applet \":!0,\"embed \":!0,\"object \":\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\"},hasData:function(a){return a=a.nodeType?n.cache[a[n.expando]]:a[n.expando],!!a&&!Q(a)},data:function(a,b,c){return R(a,b,c)},removeData:function(a,b){return S(a,b)},_data:function(a,b,c){return R(a,b,c,!0)},_removeData:function(a,b){return S(a,b,!0)}}),n.fn.extend({data:function(a,b){var c,d,e,f=this[0],g=f&&f.attributes;if(void 0===a){if(this.length&&(e=n.data(f),1===f.nodeType&&!n._data(f,\"parsedAttrs\"))){c=g.length;while(c--)g[c]&&(d=g[c].name,0===d.indexOf(\"data-\")&&(d=n.camelCase(d.slice(5)),P(f,d,e[d])));n._data(f,\"parsedAttrs\",!0)}return e}return\"object\"==typeof a?this.each(function(){n.data(this,a)}):arguments.length>1?this.each(function(){n.data(this,a,b)}):f?P(f,a,n.data(f,a)):void 0},removeData:function(a){return this.each(function(){n.removeData(this,a)})}}),n.extend({queue:function(a,b,c){var d;return a?(b=(b||\"fx\")+\"queue\",d=n._data(a,b),c&&(!d||n.isArray(c)?d=n._data(a,b,n.makeArray(c)):d.push(c)),d||[]):void 0},dequeue:function(a,b){b=b||\"fx\";var c=n.queue(a,b),d=c.length,e=c.shift(),f=n._queueHooks(a,b),g=function(){n.dequeue(a,b)};\"inprogress\"===e&&(e=c.shift(),d--),e&&(\"fx\"===b&&c.unshift(\"inprogress\"),delete f.stop,e.call(a,g,f)),!d&&f&&f.empty.fire()},_queueHooks:function(a,b){var c=b+\"queueHooks\";return n._data(a,c)||n._data(a,c,{empty:n.Callbacks(\"once memory\").add(function(){n._removeData(a,b+\"queue\"),n._removeData(a,c)})})}}),n.fn.extend({queue:function(a,b){var c=2;return\"string\"!=typeof a&&(b=a,a=\"fx\",c--),arguments.length<c?n.queue(this[0],a):void 0===b?this:this.each(function(){var c=n.queue(this,a,b);n._queueHooks(this,a),\"fx\"===a&&\"inprogress\"!==c[0]&&n.dequeue(this,a)})},dequeue:function(a){return this.each(function(){n.dequeue(this,a)})},clearQueue:function(a){return this.queue(a||\"fx\",[])},promise:function(a,b){var c,d=1,e=n.Deferred(),f=this,g=this.length,h=function(){--d||e.resolveWith(f,[f])};\"string\"!=typeof a&&(b=a,a=void 0),a=a||\"fx\";while(g--)c=n._data(f[g],a+\"queueHooks\"),c&&c.empty&&(d++,c.empty.add(h));return h(),e.promise(b)}}),function(){var a;l.shrinkWrapBlocks=function(){if(null!=a)return a;a=!1;var b,c,e;return c=d.getElementsByTagName(\"body\")[0],c&&c.style?(b=d.createElement(\"div\"),e=d.createElement(\"div\"),e.style.cssText=\"position:absolute;border:0;width:0;height:0;top:0;left:-9999px\",c.appendChild(e).appendChild(b),\"undefined\"!=typeof b.style.zoom&&(b.style.cssText=\"-webkit-box-sizing:content-box;-moz-box-sizing:content-box;box-sizing:content-box;display:block;margin:0;border:0;padding:1px;width:1px;zoom:1\",b.appendChild(d.createElement(\"div\")).style.width=\"5px\",a=3!==b.offsetWidth),c.removeChild(e),a):void 0}}();var T=/[+-]?(?:\\d*\\.|)\\d+(?:[eE][+-]?\\d+|)/.source,U=new RegExp(\"^(?:([+-])=|)(\"+T+\")([a-z%]*)$\",\"i\"),V=[\"Top\",\"Right\",\"Bottom\",\"Left\"],W=function(a,b){return a=b||a,\"none\"===n.css(a,\"display\")||!n.contains(a.ownerDocument,a)};function X(a,b,c,d){var e,f=1,g=20,h=d?function(){return d.cur()}:function(){return n.css(a,b,\"\")},i=h(),j=c&&c[3]||(n.cssNumber[b]?\"\":\"px\"),k=(n.cssNumber[b]||\"px\"!==j&&+i)&&U.exec(n.css(a,b));if(k&&k[3]!==j){j=j||k[3],c=c||[],k=+i||1;do f=f||\".5\",k/=f,n.style(a,b,k+j);while(f!==(f=h()/i)&&1!==f&&--g)}return c&&(k=+k||+i||0,e=c[1]?k+(c[1]+1)*c[2]:+c[2],d&&(d.unit=j,d.start=k,d.end=e)),e}var Y=function(a,b,c,d,e,f,g){var h=0,i=a.length,j=null==c;if(\"object\"===n.type(c)){e=!0;for(h in c)Y(a,b,h,c[h],!0,f,g)}else if(void 0!==d&&(e=!0,n.isFunction(d)||(g=!0),j&&(g?(b.call(a,d),b=null):(j=b,b=function(a,b,c){return j.call(n(a),c)})),b))for(;i>h;h++)b(a[h],c,g?d:d.call(a[h],h,b(a[h],c)));return e?a:j?b.call(a):i?b(a[0],c):f},Z=/^(?:checkbox|radio)$/i,$=/<([\\w:-]+)/,_=/^$|\\/(?:java|ecma)script/i,aa=/^\\s+/,ba=\"abbr|article|aside|audio|bdi|canvas|data|datalist|details|dialog|figcaption|figure|footer|header|hgroup|main|mark|meter|nav|output|picture|progress|section|summary|template|time|video\";function ca(a){var b=ba.split(\"|\"),c=a.createDocumentFragment();if(c.createElement)while(b.length)c.createElement(b.pop());return c}!function(){var a=d.createElement(\"div\"),b=d.createDocumentFragment(),c=d.createElement(\"input\");a.innerHTML=\"  <link/><table></table><a href='/a'>a</a><input type='checkbox'/>\",l.leadingWhitespace=3===a.firstChild.nodeType,l.tbody=!a.getElementsByTagName(\"tbody\").length,l.htmlSerialize=!!a.getElementsByTagName(\"link\").length,l.html5Clone=\"<:nav></:nav>\"!==d.createElement(\"nav\").cloneNode(!0).outerHTML,c.type=\"checkbox\",c.checked=!0,b.appendChild(c),l.appendChecked=c.checked,a.innerHTML=\"<textarea>x</textarea>\",l.noCloneChecked=!!a.cloneNode(!0).lastChild.defaultValue,b.appendChild(a),c=d.createElement(\"input\"),c.setAttribute(\"type\",\"radio\"),c.setAttribute(\"checked\",\"checked\"),c.setAttribute(\"name\",\"t\"),a.appendChild(c),l.checkClone=a.cloneNode(!0).cloneNode(!0).lastChild.checked,l.noCloneEvent=!!a.addEventListener,a[n.expando]=1,l.attributes=!a.getAttribute(n.expando)}();var da={option:[1,\"<select multiple='multiple'>\",\"</select>\"],legend:[1,\"<fieldset>\",\"</fieldset>\"],area:[1,\"<map>\",\"</map>\"],param:[1,\"<object>\",\"</object>\"],thead:[1,\"<table>\",\"</table>\"],tr:[2,\"<table><tbody>\",\"</tbody></table>\"],col:[2,\"<table><tbody></tbody><colgroup>\",\"</colgroup></table>\"],td:[3,\"<table><tbody><tr>\",\"</tr></tbody></table>\"],_default:l.htmlSerialize?[0,\"\",\"\"]:[1,\"X<div>\",\"</div>\"]};da.optgroup=da.option,da.tbody=da.tfoot=da.colgroup=da.caption=da.thead,da.th=da.td;function ea(a,b){var c,d,e=0,f=\"undefined\"!=typeof a.getElementsByTagName?a.getElementsByTagName(b||\"*\"):\"undefined\"!=typeof a.querySelectorAll?a.querySelectorAll(b||\"*\"):void 0;if(!f)for(f=[],c=a.childNodes||a;null!=(d=c[e]);e++)!b||n.nodeName(d,b)?f.push(d):n.merge(f,ea(d,b));return void 0===b||b&&n.nodeName(a,b)?n.merge([a],f):f}function fa(a,b){for(var c,d=0;null!=(c=a[d]);d++)n._data(c,\"globalEval\",!b||n._data(b[d],\"globalEval\"))}var ga=/<|&#?\\w+;/,ha=/<tbody/i;function ia(a){Z.test(a.type)&&(a.defaultChecked=a.checked)}function ja(a,b,c,d,e){for(var f,g,h,i,j,k,m,o=a.length,p=ca(b),q=[],r=0;o>r;r++)if(g=a[r],g||0===g)if(\"object\"===n.type(g))n.merge(q,g.nodeType?[g]:g);else if(ga.test(g)){i=i||p.appendChild(b.createElement(\"div\")),j=($.exec(g)||[\"\",\"\"])[1].toLowerCase(),m=da[j]||da._default,i.innerHTML=m[1]+n.htmlPrefilter(g)+m[2],f=m[0];while(f--)i=i.lastChild;if(!l.leadingWhitespace&&aa.test(g)&&q.push(b.createTextNode(aa.exec(g)[0])),!l.tbody){g=\"table\"!==j||ha.test(g)?\"<table>\"!==m[1]||ha.test(g)?0:i:i.firstChild,f=g&&g.childNodes.length;while(f--)n.nodeName(k=g.childNodes[f],\"tbody\")&&!k.childNodes.length&&g.removeChild(k)}n.merge(q,i.childNodes),i.textContent=\"\";while(i.firstChild)i.removeChild(i.firstChild);i=p.lastChild}else q.push(b.createTextNode(g));i&&p.removeChild(i),l.appendChecked||n.grep(ea(q,\"input\"),ia),r=0;while(g=q[r++])if(d&&n.inArray(g,d)>-1)e&&e.push(g);else if(h=n.contains(g.ownerDocument,g),i=ea(p.appendChild(g),\"script\"),h&&fa(i),c){f=0;while(g=i[f++])_.test(g.type||\"\")&&c.push(g)}return i=null,p}!function(){var b,c,e=d.createElement(\"div\");for(b in{submit:!0,change:!0,focusin:!0})c=\"on\"+b,(l[b]=c in a)||(e.setAttribute(c,\"t\"),l[b]=e.attributes[c].expando===!1);e=null}();var ka=/^(?:input|select|textarea)$/i,la=/^key/,ma=/^(?:mouse|pointer|contextmenu|drag|drop)|click/,na=/^(?:focusinfocus|focusoutblur)$/,oa=/^([^.]*)(?:\\.(.+)|)/;function pa(){return!0}function qa(){return!1}function ra(){try{return d.activeElement}catch(a){}}function sa(a,b,c,d,e,f){var g,h;if(\"object\"==typeof b){\"string\"!=typeof c&&(d=d||c,c=void 0);for(h in b)sa(a,h,c,d,b[h],f);return a}if(null==d&&null==e?(e=c,d=c=void 0):null==e&&(\"string\"==typeof c?(e=d,d=void 0):(e=d,d=c,c=void 0)),e===!1)e=qa;else if(!e)return a;return 1===f&&(g=e,e=function(a){return n().off(a),g.apply(this,arguments)},e.guid=g.guid||(g.guid=n.guid++)),a.each(function(){n.event.add(this,b,e,d,c)})}n.event={global:{},add:function(a,b,c,d,e){var f,g,h,i,j,k,l,m,o,p,q,r=n._data(a);if(r){c.handler&&(i=c,c=i.handler,e=i.selector),c.guid||(c.guid=n.guid++),(g=r.events)||(g=r.events={}),(k=r.handle)||(k=r.handle=function(a){return\"undefined\"==typeof n||a&&n.event.triggered===a.type?void 0:n.event.dispatch.apply(k.elem,arguments)},k.elem=a),b=(b||\"\").match(G)||[\"\"],h=b.length;while(h--)f=oa.exec(b[h])||[],o=q=f[1],p=(f[2]||\"\").split(\".\").sort(),o&&(j=n.event.special[o]||{},o=(e?j.delegateType:j.bindType)||o,j=n.event.special[o]||{},l=n.extend({type:o,origType:q,data:d,handler:c,guid:c.guid,selector:e,needsContext:e&&n.expr.match.needsContext.test(e),namespace:p.join(\".\")},i),(m=g[o])||(m=g[o]=[],m.delegateCount=0,j.setup&&j.setup.call(a,d,p,k)!==!1||(a.addEventListener?a.addEventListener(o,k,!1):a.attachEvent&&a.attachEvent(\"on\"+o,k))),j.add&&(j.add.call(a,l),l.handler.guid||(l.handler.guid=c.guid)),e?m.splice(m.delegateCount++,0,l):m.push(l),n.event.global[o]=!0);a=null}},remove:function(a,b,c,d,e){var f,g,h,i,j,k,l,m,o,p,q,r=n.hasData(a)&&n._data(a);if(r&&(k=r.events)){b=(b||\"\").match(G)||[\"\"],j=b.length;while(j--)if(h=oa.exec(b[j])||[],o=q=h[1],p=(h[2]||\"\").split(\".\").sort(),o){l=n.event.special[o]||{},o=(d?l.delegateType:l.bindType)||o,m=k[o]||[],h=h[2]&&new RegExp(\"(^|\\\\.)\"+p.join(\"\\\\.(?:.*\\\\.|)\")+\"(\\\\.|$)\"),i=f=m.length;while(f--)g=m[f],!e&&q!==g.origType||c&&c.guid!==g.guid||h&&!h.test(g.namespace)||d&&d!==g.selector&&(\"**\"!==d||!g.selector)||(m.splice(f,1),g.selector&&m.delegateCount--,l.remove&&l.remove.call(a,g));i&&!m.length&&(l.teardown&&l.teardown.call(a,p,r.handle)!==!1||n.removeEvent(a,o,r.handle),delete k[o])}else for(o in k)n.event.remove(a,o+b[j],c,d,!0);n.isEmptyObject(k)&&(delete r.handle,n._removeData(a,\"events\"))}},trigger:function(b,c,e,f){var g,h,i,j,l,m,o,p=[e||d],q=k.call(b,\"type\")?b.type:b,r=k.call(b,\"namespace\")?b.namespace.split(\".\"):[];if(i=m=e=e||d,3!==e.nodeType&&8!==e.nodeType&&!na.test(q+n.event.triggered)&&(q.indexOf(\".\")>-1&&(r=q.split(\".\"),q=r.shift(),r.sort()),h=q.indexOf(\":\")<0&&\"on\"+q,b=b[n.expando]?b:new n.Event(q,\"object\"==typeof b&&b),b.isTrigger=f?2:3,b.namespace=r.join(\".\"),b.rnamespace=b.namespace?new RegExp(\"(^|\\\\.)\"+r.join(\"\\\\.(?:.*\\\\.|)\")+\"(\\\\.|$)\"):null,b.result=void 0,b.target||(b.target=e),c=null==c?[b]:n.makeArray(c,[b]),l=n.event.special[q]||{},f||!l.trigger||l.trigger.apply(e,c)!==!1)){if(!f&&!l.noBubble&&!n.isWindow(e)){for(j=l.delegateType||q,na.test(j+q)||(i=i.parentNode);i;i=i.parentNode)p.push(i),m=i;m===(e.ownerDocument||d)&&p.push(m.defaultView||m.parentWindow||a)}o=0;while((i=p[o++])&&!b.isPropagationStopped())b.type=o>1?j:l.bindType||q,g=(n._data(i,\"events\")||{})[b.type]&&n._data(i,\"handle\"),g&&g.apply(i,c),g=h&&i[h],g&&g.apply&&M(i)&&(b.result=g.apply(i,c),b.result===!1&&b.preventDefault());if(b.type=q,!f&&!b.isDefaultPrevented()&&(!l._default||l._default.apply(p.pop(),c)===!1)&&M(e)&&h&&e[q]&&!n.isWindow(e)){m=e[h],m&&(e[h]=null),n.event.triggered=q;try{e[q]()}catch(s){}n.event.triggered=void 0,m&&(e[h]=m)}return b.result}},dispatch:function(a){a=n.event.fix(a);var b,c,d,f,g,h=[],i=e.call(arguments),j=(n._data(this,\"events\")||{})[a.type]||[],k=n.event.special[a.type]||{};if(i[0]=a,a.delegateTarget=this,!k.preDispatch||k.preDispatch.call(this,a)!==!1){h=n.event.handlers.call(this,a,j),b=0;while((f=h[b++])&&!a.isPropagationStopped()){a.currentTarget=f.elem,c=0;while((g=f.handlers[c++])&&!a.isImmediatePropagationStopped())a.rnamespace&&!a.rnamespace.test(g.namespace)||(a.handleObj=g,a.data=g.data,d=((n.event.special[g.origType]||{}).handle||g.handler).apply(f.elem,i),void 0!==d&&(a.result=d)===!1&&(a.preventDefault(),a.stopPropagation()))}return k.postDispatch&&k.postDispatch.call(this,a),a.result}},handlers:function(a,b){var c,d,e,f,g=[],h=b.delegateCount,i=a.target;if(h&&i.nodeType&&(\"click\"!==a.type||isNaN(a.button)||a.button<1))for(;i!=this;i=i.parentNode||this)if(1===i.nodeType&&(i.disabled!==!0||\"click\"!==a.type)){for(d=[],c=0;h>c;c++)f=b[c],e=f.selector+\" \",void 0===d[e]&&(d[e]=f.needsContext?n(e,this).index(i)>-1:n.find(e,this,null,[i]).length),d[e]&&d.push(f);d.length&&g.push({elem:i,handlers:d})}return h<b.length&&g.push({elem:this,handlers:b.slice(h)}),g},fix:function(a){if(a[n.expando])return a;var b,c,e,f=a.type,g=a,h=this.fixHooks[f];h||(this.fixHooks[f]=h=ma.test(f)?this.mouseHooks:la.test(f)?this.keyHooks:{}),e=h.props?this.props.concat(h.props):this.props,a=new n.Event(g),b=e.length;while(b--)c=e[b],a[c]=g[c];return a.target||(a.target=g.srcElement||d),3===a.target.nodeType&&(a.target=a.target.parentNode),a.metaKey=!!a.metaKey,h.filter?h.filter(a,g):a},props:\"altKey bubbles cancelable ctrlKey currentTarget detail eventPhase metaKey relatedTarget shiftKey target timeStamp view which\".split(\" \"),fixHooks:{},keyHooks:{props:\"char charCode key keyCode\".split(\" \"),filter:function(a,b){return null==a.which&&(a.which=null!=b.charCode?b.charCode:b.keyCode),a}},mouseHooks:{props:\"button buttons clientX clientY fromElement offsetX offsetY pageX pageY screenX screenY toElement\".split(\" \"),filter:function(a,b){var c,e,f,g=b.button,h=b.fromElement;return null==a.pageX&&null!=b.clientX&&(e=a.target.ownerDocument||d,f=e.documentElement,c=e.body,a.pageX=b.clientX+(f&&f.scrollLeft||c&&c.scrollLeft||0)-(f&&f.clientLeft||c&&c.clientLeft||0),a.pageY=b.clientY+(f&&f.scrollTop||c&&c.scrollTop||0)-(f&&f.clientTop||c&&c.clientTop||0)),!a.relatedTarget&&h&&(a.relatedTarget=h===a.target?b.toElement:h),a.which||void 0===g||(a.which=1&g?1:2&g?3:4&g?2:0),a}},special:{load:{noBubble:!0},focus:{trigger:function(){if(this!==ra()&&this.focus)try{return this.focus(),!1}catch(a){}},delegateType:\"focusin\"},blur:{trigger:function(){return this===ra()&&this.blur?(this.blur(),!1):void 0},delegateType:\"focusout\"},click:{trigger:function(){return n.nodeName(this,\"input\")&&\"checkbox\"===this.type&&this.click?(this.click(),!1):void 0},_default:function(a){return n.nodeName(a.target,\"a\")}},beforeunload:{postDispatch:function(a){void 0!==a.result&&a.originalEvent&&(a.originalEvent.returnValue=a.result)}}},simulate:function(a,b,c){var d=n.extend(new n.Event,c,{type:a,isSimulated:!0});n.event.trigger(d,null,b),d.isDefaultPrevented()&&c.preventDefault()}},n.removeEvent=d.removeEventListener?function(a,b,c){a.removeEventListener&&a.removeEventListener(b,c)}:function(a,b,c){var d=\"on\"+b;a.detachEvent&&(\"undefined\"==typeof a[d]&&(a[d]=null),a.detachEvent(d,c))},n.Event=function(a,b){return this instanceof n.Event?(a&&a.type?(this.originalEvent=a,this.type=a.type,this.isDefaultPrevented=a.defaultPrevented||void 0===a.defaultPrevented&&a.returnValue===!1?pa:qa):this.type=a,b&&n.extend(this,b),this.timeStamp=a&&a.timeStamp||n.now(),void(this[n.expando]=!0)):new n.Event(a,b)},n.Event.prototype={constructor:n.Event,isDefaultPrevented:qa,isPropagationStopped:qa,isImmediatePropagationStopped:qa,preventDefault:function(){var a=this.originalEvent;this.isDefaultPrevented=pa,a&&(a.preventDefault?a.preventDefault():a.returnValue=!1)},stopPropagation:function(){var a=this.originalEvent;this.isPropagationStopped=pa,a&&!this.isSimulated&&(a.stopPropagation&&a.stopPropagation(),a.cancelBubble=!0)},stopImmediatePropagation:function(){var a=this.originalEvent;this.isImmediatePropagationStopped=pa,a&&a.stopImmediatePropagation&&a.stopImmediatePropagation(),this.stopPropagation()}},n.each({mouseenter:\"mouseover\",mouseleave:\"mouseout\",pointerenter:\"pointerover\",pointerleave:\"pointerout\"},function(a,b){n.event.special[a]={delegateType:b,bindType:b,handle:function(a){var c,d=this,e=a.relatedTarget,f=a.handleObj;return e&&(e===d||n.contains(d,e))||(a.type=f.origType,c=f.handler.apply(this,arguments),a.type=b),c}}}),l.submit||(n.event.special.submit={setup:function(){return n.nodeName(this,\"form\")?!1:void n.event.add(this,\"click._submit keypress._submit\",function(a){var b=a.target,c=n.nodeName(b,\"input\")||n.nodeName(b,\"button\")?n.prop(b,\"form\"):void 0;c&&!n._data(c,\"submit\")&&(n.event.add(c,\"submit._submit\",function(a){a._submitBubble=!0}),n._data(c,\"submit\",!0))})},postDispatch:function(a){a._submitBubble&&(delete a._submitBubble,this.parentNode&&!a.isTrigger&&n.event.simulate(\"submit\",this.parentNode,a))},teardown:function(){return n.nodeName(this,\"form\")?!1:void n.event.remove(this,\"._submit\")}}),l.change||(n.event.special.change={setup:function(){return ka.test(this.nodeName)?(\"checkbox\"!==this.type&&\"radio\"!==this.type||(n.event.add(this,\"propertychange._change\",function(a){\"checked\"===a.originalEvent.propertyName&&(this._justChanged=!0)}),n.event.add(this,\"click._change\",function(a){this._justChanged&&!a.isTrigger&&(this._justChanged=!1),n.event.simulate(\"change\",this,a)})),!1):void n.event.add(this,\"beforeactivate._change\",function(a){var b=a.target;ka.test(b.nodeName)&&!n._data(b,\"change\")&&(n.event.add(b,\"change._change\",function(a){!this.parentNode||a.isSimulated||a.isTrigger||n.event.simulate(\"change\",this.parentNode,a)}),n._data(b,\"change\",!0))})},handle:function(a){var b=a.target;return this!==b||a.isSimulated||a.isTrigger||\"radio\"!==b.type&&\"checkbox\"!==b.type?a.handleObj.handler.apply(this,arguments):void 0},teardown:function(){return n.event.remove(this,\"._change\"),!ka.test(this.nodeName)}}),l.focusin||n.each({focus:\"focusin\",blur:\"focusout\"},function(a,b){var c=function(a){n.event.simulate(b,a.target,n.event.fix(a))};n.event.special[b]={setup:function(){var d=this.ownerDocument||this,e=n._data(d,b);e||d.addEventListener(a,c,!0),n._data(d,b,(e||0)+1)},teardown:function(){var d=this.ownerDocument||this,e=n._data(d,b)-1;e?n._data(d,b,e):(d.removeEventListener(a,c,!0),n._removeData(d,b))}}}),n.fn.extend({on:function(a,b,c,d){return sa(this,a,b,c,d)},one:function(a,b,c,d){return sa(this,a,b,c,d,1)},off:function(a,b,c){var d,e;if(a&&a.preventDefault&&a.handleObj)return d=a.handleObj,n(a.delegateTarget).off(d.namespace?d.origType+\".\"+d.namespace:d.origType,d.selector,d.handler),this;if(\"object\"==typeof a){for(e in a)this.off(e,b,a[e]);return this}return b!==!1&&\"function\"!=typeof b||(c=b,b=void 0),c===!1&&(c=qa),this.each(function(){n.event.remove(this,a,c,b)})},trigger:function(a,b){return this.each(function(){n.event.trigger(a,b,this)})},triggerHandler:function(a,b){var c=this[0];return c?n.event.trigger(a,b,c,!0):void 0}});var ta=/ jQuery\\d+=\"(?:null|\\d+)\"/g,ua=new RegExp(\"<(?:\"+ba+\")[\\\\s/>]\",\"i\"),va=/<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\\w:-]+)[^>]*)\\/>/gi,wa=/<script|<style|<link/i,xa=/checked\\s*(?:[^=]|=\\s*.checked.)/i,ya=/^true\\/(.*)/,za=/^\\s*<!(?:\\[CDATA\\[|--)|(?:\\]\\]|--)>\\s*$/g,Aa=ca(d),Ba=Aa.appendChild(d.createElement(\"div\"));function Ca(a,b){return n.nodeName(a,\"table\")&&n.nodeName(11!==b.nodeType?b:b.firstChild,\"tr\")?a.getElementsByTagName(\"tbody\")[0]||a.appendChild(a.ownerDocument.createElement(\"tbody\")):a}function Da(a){return a.type=(null!==n.find.attr(a,\"type\"))+\"/\"+a.type,a}function Ea(a){var b=ya.exec(a.type);return b?a.type=b[1]:a.removeAttribute(\"type\"),a}function Fa(a,b){if(1===b.nodeType&&n.hasData(a)){var c,d,e,f=n._data(a),g=n._data(b,f),h=f.events;if(h){delete g.handle,g.events={};for(c in h)for(d=0,e=h[c].length;e>d;d++)n.event.add(b,c,h[c][d])}g.data&&(g.data=n.extend({},g.data))}}function Ga(a,b){var c,d,e;if(1===b.nodeType){if(c=b.nodeName.toLowerCase(),!l.noCloneEvent&&b[n.expando]){e=n._data(b);for(d in e.events)n.removeEvent(b,d,e.handle);b.removeAttribute(n.expando)}\"script\"===c&&b.text!==a.text?(Da(b).text=a.text,Ea(b)):\"object\"===c?(b.parentNode&&(b.outerHTML=a.outerHTML),l.html5Clone&&a.innerHTML&&!n.trim(b.innerHTML)&&(b.innerHTML=a.innerHTML)):\"input\"===c&&Z.test(a.type)?(b.defaultChecked=b.checked=a.checked,b.value!==a.value&&(b.value=a.value)):\"option\"===c?b.defaultSelected=b.selected=a.defaultSelected:\"input\"!==c&&\"textarea\"!==c||(b.defaultValue=a.defaultValue)}}function Ha(a,b,c,d){b=f.apply([],b);var e,g,h,i,j,k,m=0,o=a.length,p=o-1,q=b[0],r=n.isFunction(q);if(r||o>1&&\"string\"==typeof q&&!l.checkClone&&xa.test(q))return a.each(function(e){var f=a.eq(e);r&&(b[0]=q.call(this,e,f.html())),Ha(f,b,c,d)});if(o&&(k=ja(b,a[0].ownerDocument,!1,a,d),e=k.firstChild,1===k.childNodes.length&&(k=e),e||d)){for(i=n.map(ea(k,\"script\"),Da),h=i.length;o>m;m++)g=k,m!==p&&(g=n.clone(g,!0,!0),h&&n.merge(i,ea(g,\"script\"))),c.call(a[m],g,m);if(h)for(j=i[i.length-1].ownerDocument,n.map(i,Ea),m=0;h>m;m++)g=i[m],_.test(g.type||\"\")&&!n._data(g,\"globalEval\")&&n.contains(j,g)&&(g.src?n._evalUrl&&n._evalUrl(g.src):n.globalEval((g.text||g.textContent||g.innerHTML||\"\").replace(za,\"\")));k=e=null}return a}function Ia(a,b,c){for(var d,e=b?n.filter(b,a):a,f=0;null!=(d=e[f]);f++)c||1!==d.nodeType||n.cleanData(ea(d)),d.parentNode&&(c&&n.contains(d.ownerDocument,d)&&fa(ea(d,\"script\")),d.parentNode.removeChild(d));return a}n.extend({htmlPrefilter:function(a){return a.replace(va,\"<$1></$2>\")},clone:function(a,b,c){var d,e,f,g,h,i=n.contains(a.ownerDocument,a);if(l.html5Clone||n.isXMLDoc(a)||!ua.test(\"<\"+a.nodeName+\">\")?f=a.cloneNode(!0):(Ba.innerHTML=a.outerHTML,Ba.removeChild(f=Ba.firstChild)),!(l.noCloneEvent&&l.noCloneChecked||1!==a.nodeType&&11!==a.nodeType||n.isXMLDoc(a)))for(d=ea(f),h=ea(a),g=0;null!=(e=h[g]);++g)d[g]&&Ga(e,d[g]);if(b)if(c)for(h=h||ea(a),d=d||ea(f),g=0;null!=(e=h[g]);g++)Fa(e,d[g]);else Fa(a,f);return d=ea(f,\"script\"),d.length>0&&fa(d,!i&&ea(a,\"script\")),d=h=e=null,f},cleanData:function(a,b){for(var d,e,f,g,h=0,i=n.expando,j=n.cache,k=l.attributes,m=n.event.special;null!=(d=a[h]);h++)if((b||M(d))&&(f=d[i],g=f&&j[f])){if(g.events)for(e in g.events)m[e]?n.event.remove(d,e):n.removeEvent(d,e,g.handle);j[f]&&(delete j[f],k||\"undefined\"==typeof d.removeAttribute?d[i]=void 0:d.removeAttribute(i),c.push(f))}}}),n.fn.extend({domManip:Ha,detach:function(a){return Ia(this,a,!0)},remove:function(a){return Ia(this,a)},text:function(a){return Y(this,function(a){return void 0===a?n.text(this):this.empty().append((this[0]&&this[0].ownerDocument||d).createTextNode(a))},null,a,arguments.length)},append:function(){return Ha(this,arguments,function(a){if(1===this.nodeType||11===this.nodeType||9===this.nodeType){var b=Ca(this,a);b.appendChild(a)}})},prepend:function(){return Ha(this,arguments,function(a){if(1===this.nodeType||11===this.nodeType||9===this.nodeType){var b=Ca(this,a);b.insertBefore(a,b.firstChild)}})},before:function(){return Ha(this,arguments,function(a){this.parentNode&&this.parentNode.insertBefore(a,this)})},after:function(){return Ha(this,arguments,function(a){this.parentNode&&this.parentNode.insertBefore(a,this.nextSibling)})},empty:function(){for(var a,b=0;null!=(a=this[b]);b++){1===a.nodeType&&n.cleanData(ea(a,!1));while(a.firstChild)a.removeChild(a.firstChild);a.options&&n.nodeName(a,\"select\")&&(a.options.length=0)}return this},clone:function(a,b){return a=null==a?!1:a,b=null==b?a:b,this.map(function(){return n.clone(this,a,b)})},html:function(a){return Y(this,function(a){var b=this[0]||{},c=0,d=this.length;if(void 0===a)return 1===b.nodeType?b.innerHTML.replace(ta,\"\"):void 0;if(\"string\"==typeof a&&!wa.test(a)&&(l.htmlSerialize||!ua.test(a))&&(l.leadingWhitespace||!aa.test(a))&&!da[($.exec(a)||[\"\",\"\"])[1].toLowerCase()]){a=n.htmlPrefilter(a);try{for(;d>c;c++)b=this[c]||{},1===b.nodeType&&(n.cleanData(ea(b,!1)),b.innerHTML=a);b=0}catch(e){}}b&&this.empty().append(a)},null,a,arguments.length)},replaceWith:function(){var a=[];return Ha(this,arguments,function(b){var c=this.parentNode;n.inArray(this,a)<0&&(n.cleanData(ea(this)),c&&c.replaceChild(b,this))},a)}}),n.each({appendTo:\"append\",prependTo:\"prepend\",insertBefore:\"before\",insertAfter:\"after\",replaceAll:\"replaceWith\"},function(a,b){n.fn[a]=function(a){for(var c,d=0,e=[],f=n(a),h=f.length-1;h>=d;d++)c=d===h?this:this.clone(!0),n(f[d])[b](c),g.apply(e,c.get());return this.pushStack(e)}});var Ja,Ka={HTML:\"block\",BODY:\"block\"};function La(a,b){var c=n(b.createElement(a)).appendTo(b.body),d=n.css(c[0],\"display\");return c.detach(),d}function Ma(a){var b=d,c=Ka[a];return c||(c=La(a,b),\"none\"!==c&&c||(Ja=(Ja||n(\"<iframe frameborder='0' width='0' height='0'/>\")).appendTo(b.documentElement),b=(Ja[0].contentWindow||Ja[0].contentDocument).document,b.write(),b.close(),c=La(a,b),Ja.detach()),Ka[a]=c),c}var Na=/^margin/,Oa=new RegExp(\"^(\"+T+\")(?!px)[a-z%]+$\",\"i\"),Pa=function(a,b,c,d){var e,f,g={};for(f in b)g[f]=a.style[f],a.style[f]=b[f];e=c.apply(a,d||[]);for(f in b)a.style[f]=g[f];return e},Qa=d.documentElement;!function(){var b,c,e,f,g,h,i=d.createElement(\"div\"),j=d.createElement(\"div\");if(j.style){j.style.cssText=\"float:left;opacity:.5\",l.opacity=\"0.5\"===j.style.opacity,l.cssFloat=!!j.style.cssFloat,j.style.backgroundClip=\"content-box\",j.cloneNode(!0).style.backgroundClip=\"\",l.clearCloneStyle=\"content-box\"===j.style.backgroundClip,i=d.createElement(\"div\"),i.style.cssText=\"border:0;width:8px;height:0;top:0;left:-9999px;padding:0;margin-top:1px;position:absolute\",j.innerHTML=\"\",i.appendChild(j),l.boxSizing=\"\"===j.style.boxSizing||\"\"===j.style.MozBoxSizing||\"\"===j.style.WebkitBoxSizing,n.extend(l,{reliableHiddenOffsets:function(){return null==b&&k(),f},boxSizingReliable:function(){return null==b&&k(),e},pixelMarginRight:function(){return null==b&&k(),c},pixelPosition:function(){return null==b&&k(),b},reliableMarginRight:function(){return null==b&&k(),g},reliableMarginLeft:function(){return null==b&&k(),h}});function k(){var k,l,m=d.documentElement;m.appendChild(i),j.style.cssText=\"-webkit-box-sizing:border-box;box-sizing:border-box;position:relative;display:block;margin:auto;border:1px;padding:1px;top:1%;width:50%\",b=e=h=!1,c=g=!0,a.getComputedStyle&&(l=a.getComputedStyle(j),b=\"1%\"!==(l||{}).top,h=\"2px\"===(l||{}).marginLeft,e=\"4px\"===(l||{width:\"4px\"}).width,j.style.marginRight=\"50%\",c=\"4px\"===(l||{marginRight:\"4px\"}).marginRight,k=j.appendChild(d.createElement(\"div\")),k.style.cssText=j.style.cssText=\"-webkit-box-sizing:content-box;-moz-box-sizing:content-box;box-sizing:content-box;display:block;margin:0;border:0;padding:0\",k.style.marginRight=k.style.width=\"0\",j.style.width=\"1px\",g=!parseFloat((a.getComputedStyle(k)||{}).marginRight),j.removeChild(k)),j.style.display=\"none\",f=0===j.getClientRects().length,f&&(j.style.display=\"\",j.innerHTML=\"<table><tr><td></td><td>t</td></tr></table>\",j.childNodes[0].style.borderCollapse=\"separate\",k=j.getElementsByTagName(\"td\"),k[0].style.cssText=\"margin:0;border:0;padding:0;display:none\",f=0===k[0].offsetHeight,f&&(k[0].style.display=\"\",k[1].style.display=\"none\",f=0===k[0].offsetHeight)),m.removeChild(i)}}}();var Ra,Sa,Ta=/^(top|right|bottom|left)$/;a.getComputedStyle?(Ra=function(b){var c=b.ownerDocument.defaultView;return c&&c.opener||(c=a),c.getComputedStyle(b)},Sa=function(a,b,c){var d,e,f,g,h=a.style;return c=c||Ra(a),g=c?c.getPropertyValue(b)||c[b]:void 0,\"\"!==g&&void 0!==g||n.contains(a.ownerDocument,a)||(g=n.style(a,b)),c&&!l.pixelMarginRight()&&Oa.test(g)&&Na.test(b)&&(d=h.width,e=h.minWidth,f=h.maxWidth,h.minWidth=h.maxWidth=h.width=g,g=c.width,h.width=d,h.minWidth=e,h.maxWidth=f),void 0===g?g:g+\"\"}):Qa.currentStyle&&(Ra=function(a){return a.currentStyle},Sa=function(a,b,c){var d,e,f,g,h=a.style;return c=c||Ra(a),g=c?c[b]:void 0,null==g&&h&&h[b]&&(g=h[b]),Oa.test(g)&&!Ta.test(b)&&(d=h.left,e=a.runtimeStyle,f=e&&e.left,f&&(e.left=a.currentStyle.left),h.left=\"fontSize\"===b?\"1em\":g,g=h.pixelLeft+\"px\",h.left=d,f&&(e.left=f)),void 0===g?g:g+\"\"||\"auto\"});function Ua(a,b){return{get:function(){return a()?void delete this.get:(this.get=b).apply(this,arguments)}}}var Va=/alpha\\([^)]*\\)/i,Wa=/opacity\\s*=\\s*([^)]*)/i,Xa=/^(none|table(?!-c[ea]).+)/,Ya=new RegExp(\"^(\"+T+\")(.*)$\",\"i\"),Za={position:\"absolute\",visibility:\"hidden\",display:\"block\"},$a={letterSpacing:\"0\",fontWeight:\"400\"},_a=[\"Webkit\",\"O\",\"Moz\",\"ms\"],ab=d.createElement(\"div\").style;function bb(a){if(a in ab)return a;var b=a.charAt(0).toUpperCase()+a.slice(1),c=_a.length;while(c--)if(a=_a[c]+b,a in ab)return a}function cb(a,b){for(var c,d,e,f=[],g=0,h=a.length;h>g;g++)d=a[g],d.style&&(f[g]=n._data(d,\"olddisplay\"),c=d.style.display,b?(f[g]||\"none\"!==c||(d.style.display=\"\"),\"\"===d.style.display&&W(d)&&(f[g]=n._data(d,\"olddisplay\",Ma(d.nodeName)))):(e=W(d),(c&&\"none\"!==c||!e)&&n._data(d,\"olddisplay\",e?c:n.css(d,\"display\"))));for(g=0;h>g;g++)d=a[g],d.style&&(b&&\"none\"!==d.style.display&&\"\"!==d.style.display||(d.style.display=b?f[g]||\"\":\"none\"));return a}function db(a,b,c){var d=Ya.exec(b);return d?Math.max(0,d[1]-(c||0))+(d[2]||\"px\"):b}function eb(a,b,c,d,e){for(var f=c===(d?\"border\":\"content\")?4:\"width\"===b?1:0,g=0;4>f;f+=2)\"margin\"===c&&(g+=n.css(a,c+V[f],!0,e)),d?(\"content\"===c&&(g-=n.css(a,\"padding\"+V[f],!0,e)),\"margin\"!==c&&(g-=n.css(a,\"border\"+V[f]+\"Width\",!0,e))):(g+=n.css(a,\"padding\"+V[f],!0,e),\"padding\"!==c&&(g+=n.css(a,\"border\"+V[f]+\"Width\",!0,e)));return g}function fb(a,b,c){var d=!0,e=\"width\"===b?a.offsetWidth:a.offsetHeight,f=Ra(a),g=l.boxSizing&&\"border-box\"===n.css(a,\"boxSizing\",!1,f);if(0>=e||null==e){if(e=Sa(a,b,f),(0>e||null==e)&&(e=a.style[b]),Oa.test(e))return e;d=g&&(l.boxSizingReliable()||e===a.style[b]),e=parseFloat(e)||0}return e+eb(a,b,c||(g?\"border\":\"content\"),d,f)+\"px\"}n.extend({cssHooks:{opacity:{get:function(a,b){if(b){var c=Sa(a,\"opacity\");return\"\"===c?\"1\":c}}}},cssNumber:{animationIterationCount:!0,columnCount:!0,fillOpacity:!0,flexGrow:!0,flexShrink:!0,fontWeight:!0,lineHeight:!0,opacity:!0,order:!0,orphans:!0,widows:!0,zIndex:!0,zoom:!0},cssProps:{\"float\":l.cssFloat?\"cssFloat\":\"styleFloat\"},style:function(a,b,c,d){if(a&&3!==a.nodeType&&8!==a.nodeType&&a.style){var e,f,g,h=n.camelCase(b),i=a.style;if(b=n.cssProps[h]||(n.cssProps[h]=bb(h)||h),g=n.cssHooks[b]||n.cssHooks[h],void 0===c)return g&&\"get\"in g&&void 0!==(e=g.get(a,!1,d))?e:i[b];if(f=typeof c,\"string\"===f&&(e=U.exec(c))&&e[1]&&(c=X(a,b,e),f=\"number\"),null!=c&&c===c&&(\"number\"===f&&(c+=e&&e[3]||(n.cssNumber[h]?\"\":\"px\")),l.clearCloneStyle||\"\"!==c||0!==b.indexOf(\"background\")||(i[b]=\"inherit\"),!(g&&\"set\"in g&&void 0===(c=g.set(a,c,d)))))try{i[b]=c}catch(j){}}},css:function(a,b,c,d){var e,f,g,h=n.camelCase(b);return b=n.cssProps[h]||(n.cssProps[h]=bb(h)||h),g=n.cssHooks[b]||n.cssHooks[h],g&&\"get\"in g&&(f=g.get(a,!0,c)),void 0===f&&(f=Sa(a,b,d)),\"normal\"===f&&b in $a&&(f=$a[b]),\"\"===c||c?(e=parseFloat(f),c===!0||isFinite(e)?e||0:f):f}}),n.each([\"height\",\"width\"],function(a,b){n.cssHooks[b]={get:function(a,c,d){return c?Xa.test(n.css(a,\"display\"))&&0===a.offsetWidth?Pa(a,Za,function(){return fb(a,b,d)}):fb(a,b,d):void 0},set:function(a,c,d){var e=d&&Ra(a);return db(a,c,d?eb(a,b,d,l.boxSizing&&\"border-box\"===n.css(a,\"boxSizing\",!1,e),e):0)}}}),l.opacity||(n.cssHooks.opacity={get:function(a,b){return Wa.test((b&&a.currentStyle?a.currentStyle.filter:a.style.filter)||\"\")?.01*parseFloat(RegExp.$1)+\"\":b?\"1\":\"\"},set:function(a,b){var c=a.style,d=a.currentStyle,e=n.isNumeric(b)?\"alpha(opacity=\"+100*b+\")\":\"\",f=d&&d.filter||c.filter||\"\";c.zoom=1,(b>=1||\"\"===b)&&\"\"===n.trim(f.replace(Va,\"\"))&&c.removeAttribute&&(c.removeAttribute(\"filter\"),\"\"===b||d&&!d.filter)||(c.filter=Va.test(f)?f.replace(Va,e):f+\" \"+e)}}),n.cssHooks.marginRight=Ua(l.reliableMarginRight,function(a,b){return b?Pa(a,{display:\"inline-block\"},Sa,[a,\"marginRight\"]):void 0}),n.cssHooks.marginLeft=Ua(l.reliableMarginLeft,function(a,b){return b?(parseFloat(Sa(a,\"marginLeft\"))||(n.contains(a.ownerDocument,a)?a.getBoundingClientRect().left-Pa(a,{\r\nmarginLeft:0},function(){return a.getBoundingClientRect().left}):0))+\"px\":void 0}),n.each({margin:\"\",padding:\"\",border:\"Width\"},function(a,b){n.cssHooks[a+b]={expand:function(c){for(var d=0,e={},f=\"string\"==typeof c?c.split(\" \"):[c];4>d;d++)e[a+V[d]+b]=f[d]||f[d-2]||f[0];return e}},Na.test(a)||(n.cssHooks[a+b].set=db)}),n.fn.extend({css:function(a,b){return Y(this,function(a,b,c){var d,e,f={},g=0;if(n.isArray(b)){for(d=Ra(a),e=b.length;e>g;g++)f[b[g]]=n.css(a,b[g],!1,d);return f}return void 0!==c?n.style(a,b,c):n.css(a,b)},a,b,arguments.length>1)},show:function(){return cb(this,!0)},hide:function(){return cb(this)},toggle:function(a){return\"boolean\"==typeof a?a?this.show():this.hide():this.each(function(){W(this)?n(this).show():n(this).hide()})}});function gb(a,b,c,d,e){return new gb.prototype.init(a,b,c,d,e)}n.Tween=gb,gb.prototype={constructor:gb,init:function(a,b,c,d,e,f){this.elem=a,this.prop=c,this.easing=e||n.easing._default,this.options=b,this.start=this.now=this.cur(),this.end=d,this.unit=f||(n.cssNumber[c]?\"\":\"px\")},cur:function(){var a=gb.propHooks[this.prop];return a&&a.get?a.get(this):gb.propHooks._default.get(this)},run:function(a){var b,c=gb.propHooks[this.prop];return this.options.duration?this.pos=b=n.easing[this.easing](a,this.options.duration*a,0,1,this.options.duration):this.pos=b=a,this.now=(this.end-this.start)*b+this.start,this.options.step&&this.options.step.call(this.elem,this.now,this),c&&c.set?c.set(this):gb.propHooks._default.set(this),this}},gb.prototype.init.prototype=gb.prototype,gb.propHooks={_default:{get:function(a){var b;return 1!==a.elem.nodeType||null!=a.elem[a.prop]&&null==a.elem.style[a.prop]?a.elem[a.prop]:(b=n.css(a.elem,a.prop,\"\"),b&&\"auto\"!==b?b:0)},set:function(a){n.fx.step[a.prop]?n.fx.step[a.prop](a):1!==a.elem.nodeType||null==a.elem.style[n.cssProps[a.prop]]&&!n.cssHooks[a.prop]?a.elem[a.prop]=a.now:n.style(a.elem,a.prop,a.now+a.unit)}}},gb.propHooks.scrollTop=gb.propHooks.scrollLeft={set:function(a){a.elem.nodeType&&a.elem.parentNode&&(a.elem[a.prop]=a.now)}},n.easing={linear:function(a){return a},swing:function(a){return.5-Math.cos(a*Math.PI)/2},_default:\"swing\"},n.fx=gb.prototype.init,n.fx.step={};var hb,ib,jb=/^(?:toggle|show|hide)$/,kb=/queueHooks$/;function lb(){return a.setTimeout(function(){hb=void 0}),hb=n.now()}function mb(a,b){var c,d={height:a},e=0;for(b=b?1:0;4>e;e+=2-b)c=V[e],d[\"margin\"+c]=d[\"padding\"+c]=a;return b&&(d.opacity=d.width=a),d}function nb(a,b,c){for(var d,e=(qb.tweeners[b]||[]).concat(qb.tweeners[\"*\"]),f=0,g=e.length;g>f;f++)if(d=e[f].call(c,b,a))return d}function ob(a,b,c){var d,e,f,g,h,i,j,k,m=this,o={},p=a.style,q=a.nodeType&&W(a),r=n._data(a,\"fxshow\");c.queue||(h=n._queueHooks(a,\"fx\"),null==h.unqueued&&(h.unqueued=0,i=h.empty.fire,h.empty.fire=function(){h.unqueued||i()}),h.unqueued++,m.always(function(){m.always(function(){h.unqueued--,n.queue(a,\"fx\").length||h.empty.fire()})})),1===a.nodeType&&(\"height\"in b||\"width\"in b)&&(c.overflow=[p.overflow,p.overflowX,p.overflowY],j=n.css(a,\"display\"),k=\"none\"===j?n._data(a,\"olddisplay\")||Ma(a.nodeName):j,\"inline\"===k&&\"none\"===n.css(a,\"float\")&&(l.inlineBlockNeedsLayout&&\"inline\"!==Ma(a.nodeName)?p.zoom=1:p.display=\"inline-block\")),c.overflow&&(p.overflow=\"hidden\",l.shrinkWrapBlocks()||m.always(function(){p.overflow=c.overflow[0],p.overflowX=c.overflow[1],p.overflowY=c.overflow[2]}));for(d in b)if(e=b[d],jb.exec(e)){if(delete b[d],f=f||\"toggle\"===e,e===(q?\"hide\":\"show\")){if(\"show\"!==e||!r||void 0===r[d])continue;q=!0}o[d]=r&&r[d]||n.style(a,d)}else j=void 0;if(n.isEmptyObject(o))\"inline\"===(\"none\"===j?Ma(a.nodeName):j)&&(p.display=j);else{r?\"hidden\"in r&&(q=r.hidden):r=n._data(a,\"fxshow\",{}),f&&(r.hidden=!q),q?n(a).show():m.done(function(){n(a).hide()}),m.done(function(){var b;n._removeData(a,\"fxshow\");for(b in o)n.style(a,b,o[b])});for(d in o)g=nb(q?r[d]:0,d,m),d in r||(r[d]=g.start,q&&(g.end=g.start,g.start=\"width\"===d||\"height\"===d?1:0))}}function pb(a,b){var c,d,e,f,g;for(c in a)if(d=n.camelCase(c),e=b[d],f=a[c],n.isArray(f)&&(e=f[1],f=a[c]=f[0]),c!==d&&(a[d]=f,delete a[c]),g=n.cssHooks[d],g&&\"expand\"in g){f=g.expand(f),delete a[d];for(c in f)c in a||(a[c]=f[c],b[c]=e)}else b[d]=e}function qb(a,b,c){var d,e,f=0,g=qb.prefilters.length,h=n.Deferred().always(function(){delete i.elem}),i=function(){if(e)return!1;for(var b=hb||lb(),c=Math.max(0,j.startTime+j.duration-b),d=c/j.duration||0,f=1-d,g=0,i=j.tweens.length;i>g;g++)j.tweens[g].run(f);return h.notifyWith(a,[j,f,c]),1>f&&i?c:(h.resolveWith(a,[j]),!1)},j=h.promise({elem:a,props:n.extend({},b),opts:n.extend(!0,{specialEasing:{},easing:n.easing._default},c),originalProperties:b,originalOptions:c,startTime:hb||lb(),duration:c.duration,tweens:[],createTween:function(b,c){var d=n.Tween(a,j.opts,b,c,j.opts.specialEasing[b]||j.opts.easing);return j.tweens.push(d),d},stop:function(b){var c=0,d=b?j.tweens.length:0;if(e)return this;for(e=!0;d>c;c++)j.tweens[c].run(1);return b?(h.notifyWith(a,[j,1,0]),h.resolveWith(a,[j,b])):h.rejectWith(a,[j,b]),this}}),k=j.props;for(pb(k,j.opts.specialEasing);g>f;f++)if(d=qb.prefilters[f].call(j,a,k,j.opts))return n.isFunction(d.stop)&&(n._queueHooks(j.elem,j.opts.queue).stop=n.proxy(d.stop,d)),d;return n.map(k,nb,j),n.isFunction(j.opts.start)&&j.opts.start.call(a,j),n.fx.timer(n.extend(i,{elem:a,anim:j,queue:j.opts.queue})),j.progress(j.opts.progress).done(j.opts.done,j.opts.complete).fail(j.opts.fail).always(j.opts.always)}n.Animation=n.extend(qb,{tweeners:{\"*\":[function(a,b){var c=this.createTween(a,b);return X(c.elem,a,U.exec(b),c),c}]},tweener:function(a,b){n.isFunction(a)?(b=a,a=[\"*\"]):a=a.match(G);for(var c,d=0,e=a.length;e>d;d++)c=a[d],qb.tweeners[c]=qb.tweeners[c]||[],qb.tweeners[c].unshift(b)},prefilters:[ob],prefilter:function(a,b){b?qb.prefilters.unshift(a):qb.prefilters.push(a)}}),n.speed=function(a,b,c){var d=a&&\"object\"==typeof a?n.extend({},a):{complete:c||!c&&b||n.isFunction(a)&&a,duration:a,easing:c&&b||b&&!n.isFunction(b)&&b};return d.duration=n.fx.off?0:\"number\"==typeof d.duration?d.duration:d.duration in n.fx.speeds?n.fx.speeds[d.duration]:n.fx.speeds._default,null!=d.queue&&d.queue!==!0||(d.queue=\"fx\"),d.old=d.complete,d.complete=function(){n.isFunction(d.old)&&d.old.call(this),d.queue&&n.dequeue(this,d.queue)},d},n.fn.extend({fadeTo:function(a,b,c,d){return this.filter(W).css(\"opacity\",0).show().end().animate({opacity:b},a,c,d)},animate:function(a,b,c,d){var e=n.isEmptyObject(a),f=n.speed(b,c,d),g=function(){var b=qb(this,n.extend({},a),f);(e||n._data(this,\"finish\"))&&b.stop(!0)};return g.finish=g,e||f.queue===!1?this.each(g):this.queue(f.queue,g)},stop:function(a,b,c){var d=function(a){var b=a.stop;delete a.stop,b(c)};return\"string\"!=typeof a&&(c=b,b=a,a=void 0),b&&a!==!1&&this.queue(a||\"fx\",[]),this.each(function(){var b=!0,e=null!=a&&a+\"queueHooks\",f=n.timers,g=n._data(this);if(e)g[e]&&g[e].stop&&d(g[e]);else for(e in g)g[e]&&g[e].stop&&kb.test(e)&&d(g[e]);for(e=f.length;e--;)f[e].elem!==this||null!=a&&f[e].queue!==a||(f[e].anim.stop(c),b=!1,f.splice(e,1));!b&&c||n.dequeue(this,a)})},finish:function(a){return a!==!1&&(a=a||\"fx\"),this.each(function(){var b,c=n._data(this),d=c[a+\"queue\"],e=c[a+\"queueHooks\"],f=n.timers,g=d?d.length:0;for(c.finish=!0,n.queue(this,a,[]),e&&e.stop&&e.stop.call(this,!0),b=f.length;b--;)f[b].elem===this&&f[b].queue===a&&(f[b].anim.stop(!0),f.splice(b,1));for(b=0;g>b;b++)d[b]&&d[b].finish&&d[b].finish.call(this);delete c.finish})}}),n.each([\"toggle\",\"show\",\"hide\"],function(a,b){var c=n.fn[b];n.fn[b]=function(a,d,e){return null==a||\"boolean\"==typeof a?c.apply(this,arguments):this.animate(mb(b,!0),a,d,e)}}),n.each({slideDown:mb(\"show\"),slideUp:mb(\"hide\"),slideToggle:mb(\"toggle\"),fadeIn:{opacity:\"show\"},fadeOut:{opacity:\"hide\"},fadeToggle:{opacity:\"toggle\"}},function(a,b){n.fn[a]=function(a,c,d){return this.animate(b,a,c,d)}}),n.timers=[],n.fx.tick=function(){var a,b=n.timers,c=0;for(hb=n.now();c<b.length;c++)a=b[c],a()||b[c]!==a||b.splice(c--,1);b.length||n.fx.stop(),hb=void 0},n.fx.timer=function(a){n.timers.push(a),a()?n.fx.start():n.timers.pop()},n.fx.interval=13,n.fx.start=function(){ib||(ib=a.setInterval(n.fx.tick,n.fx.interval))},n.fx.stop=function(){a.clearInterval(ib),ib=null},n.fx.speeds={slow:600,fast:200,_default:400},n.fn.delay=function(b,c){return b=n.fx?n.fx.speeds[b]||b:b,c=c||\"fx\",this.queue(c,function(c,d){var e=a.setTimeout(c,b);d.stop=function(){a.clearTimeout(e)}})},function(){var a,b=d.createElement(\"input\"),c=d.createElement(\"div\"),e=d.createElement(\"select\"),f=e.appendChild(d.createElement(\"option\"));c=d.createElement(\"div\"),c.setAttribute(\"className\",\"t\"),c.innerHTML=\"  <link/><table></table><a href='/a'>a</a><input type='checkbox'/>\",a=c.getElementsByTagName(\"a\")[0],b.setAttribute(\"type\",\"checkbox\"),c.appendChild(b),a=c.getElementsByTagName(\"a\")[0],a.style.cssText=\"top:1px\",l.getSetAttribute=\"t\"!==c.className,l.style=/top/.test(a.getAttribute(\"style\")),l.hrefNormalized=\"/a\"===a.getAttribute(\"href\"),l.checkOn=!!b.value,l.optSelected=f.selected,l.enctype=!!d.createElement(\"form\").enctype,e.disabled=!0,l.optDisabled=!f.disabled,b=d.createElement(\"input\"),b.setAttribute(\"value\",\"\"),l.input=\"\"===b.getAttribute(\"value\"),b.value=\"t\",b.setAttribute(\"type\",\"radio\"),l.radioValue=\"t\"===b.value}();var rb=/\\r/g,sb=/[\\x20\\t\\r\\n\\f]+/g;n.fn.extend({val:function(a){var b,c,d,e=this[0];{if(arguments.length)return d=n.isFunction(a),this.each(function(c){var e;1===this.nodeType&&(e=d?a.call(this,c,n(this).val()):a,null==e?e=\"\":\"number\"==typeof e?e+=\"\":n.isArray(e)&&(e=n.map(e,function(a){return null==a?\"\":a+\"\"})),b=n.valHooks[this.type]||n.valHooks[this.nodeName.toLowerCase()],b&&\"set\"in b&&void 0!==b.set(this,e,\"value\")||(this.value=e))});if(e)return b=n.valHooks[e.type]||n.valHooks[e.nodeName.toLowerCase()],b&&\"get\"in b&&void 0!==(c=b.get(e,\"value\"))?c:(c=e.value,\"string\"==typeof c?c.replace(rb,\"\"):null==c?\"\":c)}}}),n.extend({valHooks:{option:{get:function(a){var b=n.find.attr(a,\"value\");return null!=b?b:n.trim(n.text(a)).replace(sb,\" \")}},select:{get:function(a){for(var b,c,d=a.options,e=a.selectedIndex,f=\"select-one\"===a.type||0>e,g=f?null:[],h=f?e+1:d.length,i=0>e?h:f?e:0;h>i;i++)if(c=d[i],(c.selected||i===e)&&(l.optDisabled?!c.disabled:null===c.getAttribute(\"disabled\"))&&(!c.parentNode.disabled||!n.nodeName(c.parentNode,\"optgroup\"))){if(b=n(c).val(),f)return b;g.push(b)}return g},set:function(a,b){var c,d,e=a.options,f=n.makeArray(b),g=e.length;while(g--)if(d=e[g],n.inArray(n.valHooks.option.get(d),f)>-1)try{d.selected=c=!0}catch(h){d.scrollHeight}else d.selected=!1;return c||(a.selectedIndex=-1),e}}}}),n.each([\"radio\",\"checkbox\"],function(){n.valHooks[this]={set:function(a,b){return n.isArray(b)?a.checked=n.inArray(n(a).val(),b)>-1:void 0}},l.checkOn||(n.valHooks[this].get=function(a){return null===a.getAttribute(\"value\")?\"on\":a.value})});var tb,ub,vb=n.expr.attrHandle,wb=/^(?:checked|selected)$/i,xb=l.getSetAttribute,yb=l.input;n.fn.extend({attr:function(a,b){return Y(this,n.attr,a,b,arguments.length>1)},removeAttr:function(a){return this.each(function(){n.removeAttr(this,a)})}}),n.extend({attr:function(a,b,c){var d,e,f=a.nodeType;if(3!==f&&8!==f&&2!==f)return\"undefined\"==typeof a.getAttribute?n.prop(a,b,c):(1===f&&n.isXMLDoc(a)||(b=b.toLowerCase(),e=n.attrHooks[b]||(n.expr.match.bool.test(b)?ub:tb)),void 0!==c?null===c?void n.removeAttr(a,b):e&&\"set\"in e&&void 0!==(d=e.set(a,c,b))?d:(a.setAttribute(b,c+\"\"),c):e&&\"get\"in e&&null!==(d=e.get(a,b))?d:(d=n.find.attr(a,b),null==d?void 0:d))},attrHooks:{type:{set:function(a,b){if(!l.radioValue&&\"radio\"===b&&n.nodeName(a,\"input\")){var c=a.value;return a.setAttribute(\"type\",b),c&&(a.value=c),b}}}},removeAttr:function(a,b){var c,d,e=0,f=b&&b.match(G);if(f&&1===a.nodeType)while(c=f[e++])d=n.propFix[c]||c,n.expr.match.bool.test(c)?yb&&xb||!wb.test(c)?a[d]=!1:a[n.camelCase(\"default-\"+c)]=a[d]=!1:n.attr(a,c,\"\"),a.removeAttribute(xb?c:d)}}),ub={set:function(a,b,c){return b===!1?n.removeAttr(a,c):yb&&xb||!wb.test(c)?a.setAttribute(!xb&&n.propFix[c]||c,c):a[n.camelCase(\"default-\"+c)]=a[c]=!0,c}},n.each(n.expr.match.bool.source.match(/\\w+/g),function(a,b){var c=vb[b]||n.find.attr;yb&&xb||!wb.test(b)?vb[b]=function(a,b,d){var e,f;return d||(f=vb[b],vb[b]=e,e=null!=c(a,b,d)?b.toLowerCase():null,vb[b]=f),e}:vb[b]=function(a,b,c){return c?void 0:a[n.camelCase(\"default-\"+b)]?b.toLowerCase():null}}),yb&&xb||(n.attrHooks.value={set:function(a,b,c){return n.nodeName(a,\"input\")?void(a.defaultValue=b):tb&&tb.set(a,b,c)}}),xb||(tb={set:function(a,b,c){var d=a.getAttributeNode(c);return d||a.setAttributeNode(d=a.ownerDocument.createAttribute(c)),d.value=b+=\"\",\"value\"===c||b===a.getAttribute(c)?b:void 0}},vb.id=vb.name=vb.coords=function(a,b,c){var d;return c?void 0:(d=a.getAttributeNode(b))&&\"\"!==d.value?d.value:null},n.valHooks.button={get:function(a,b){var c=a.getAttributeNode(b);return c&&c.specified?c.value:void 0},set:tb.set},n.attrHooks.contenteditable={set:function(a,b,c){tb.set(a,\"\"===b?!1:b,c)}},n.each([\"width\",\"height\"],function(a,b){n.attrHooks[b]={set:function(a,c){return\"\"===c?(a.setAttribute(b,\"auto\"),c):void 0}}})),l.style||(n.attrHooks.style={get:function(a){return a.style.cssText||void 0},set:function(a,b){return a.style.cssText=b+\"\"}});var zb=/^(?:input|select|textarea|button|object)$/i,Ab=/^(?:a|area)$/i;n.fn.extend({prop:function(a,b){return Y(this,n.prop,a,b,arguments.length>1)},removeProp:function(a){return a=n.propFix[a]||a,this.each(function(){try{this[a]=void 0,delete this[a]}catch(b){}})}}),n.extend({prop:function(a,b,c){var d,e,f=a.nodeType;if(3!==f&&8!==f&&2!==f)return 1===f&&n.isXMLDoc(a)||(b=n.propFix[b]||b,e=n.propHooks[b]),void 0!==c?e&&\"set\"in e&&void 0!==(d=e.set(a,c,b))?d:a[b]=c:e&&\"get\"in e&&null!==(d=e.get(a,b))?d:a[b]},propHooks:{tabIndex:{get:function(a){var b=n.find.attr(a,\"tabindex\");return b?parseInt(b,10):zb.test(a.nodeName)||Ab.test(a.nodeName)&&a.href?0:-1}}},propFix:{\"for\":\"htmlFor\",\"class\":\"className\"}}),l.hrefNormalized||n.each([\"href\",\"src\"],function(a,b){n.propHooks[b]={get:function(a){return a.getAttribute(b,4)}}}),l.optSelected||(n.propHooks.selected={get:function(a){var b=a.parentNode;return b&&(b.selectedIndex,b.parentNode&&b.parentNode.selectedIndex),null},set:function(a){var b=a.parentNode;b&&(b.selectedIndex,b.parentNode&&b.parentNode.selectedIndex)}}),n.each([\"tabIndex\",\"readOnly\",\"maxLength\",\"cellSpacing\",\"cellPadding\",\"rowSpan\",\"colSpan\",\"useMap\",\"frameBorder\",\"contentEditable\"],function(){n.propFix[this.toLowerCase()]=this}),l.enctype||(n.propFix.enctype=\"encoding\");var Bb=/[\\t\\r\\n\\f]/g;function Cb(a){return n.attr(a,\"class\")||\"\"}n.fn.extend({addClass:function(a){var b,c,d,e,f,g,h,i=0;if(n.isFunction(a))return this.each(function(b){n(this).addClass(a.call(this,b,Cb(this)))});if(\"string\"==typeof a&&a){b=a.match(G)||[];while(c=this[i++])if(e=Cb(c),d=1===c.nodeType&&(\" \"+e+\" \").replace(Bb,\" \")){g=0;while(f=b[g++])d.indexOf(\" \"+f+\" \")<0&&(d+=f+\" \");h=n.trim(d),e!==h&&n.attr(c,\"class\",h)}}return this},removeClass:function(a){var b,c,d,e,f,g,h,i=0;if(n.isFunction(a))return this.each(function(b){n(this).removeClass(a.call(this,b,Cb(this)))});if(!arguments.length)return this.attr(\"class\",\"\");if(\"string\"==typeof a&&a){b=a.match(G)||[];while(c=this[i++])if(e=Cb(c),d=1===c.nodeType&&(\" \"+e+\" \").replace(Bb,\" \")){g=0;while(f=b[g++])while(d.indexOf(\" \"+f+\" \")>-1)d=d.replace(\" \"+f+\" \",\" \");h=n.trim(d),e!==h&&n.attr(c,\"class\",h)}}return this},toggleClass:function(a,b){var c=typeof a;return\"boolean\"==typeof b&&\"string\"===c?b?this.addClass(a):this.removeClass(a):n.isFunction(a)?this.each(function(c){n(this).toggleClass(a.call(this,c,Cb(this),b),b)}):this.each(function(){var b,d,e,f;if(\"string\"===c){d=0,e=n(this),f=a.match(G)||[];while(b=f[d++])e.hasClass(b)?e.removeClass(b):e.addClass(b)}else void 0!==a&&\"boolean\"!==c||(b=Cb(this),b&&n._data(this,\"__className__\",b),n.attr(this,\"class\",b||a===!1?\"\":n._data(this,\"__className__\")||\"\"))})},hasClass:function(a){var b,c,d=0;b=\" \"+a+\" \";while(c=this[d++])if(1===c.nodeType&&(\" \"+Cb(c)+\" \").replace(Bb,\" \").indexOf(b)>-1)return!0;return!1}}),n.each(\"blur focus focusin focusout load resize scroll unload click dblclick mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave change select submit keydown keypress keyup error contextmenu\".split(\" \"),function(a,b){n.fn[b]=function(a,c){return arguments.length>0?this.on(b,null,a,c):this.trigger(b)}}),n.fn.extend({hover:function(a,b){return this.mouseenter(a).mouseleave(b||a)}});var Db=a.location,Eb=n.now(),Fb=/\\?/,Gb=/(,)|(\\[|{)|(}|])|\"(?:[^\"\\\\\\r\\n]|\\\\[\"\\\\\\/bfnrt]|\\\\u[\\da-fA-F]{4})*\"\\s*:?|true|false|null|-?(?!0\\d)\\d+(?:\\.\\d+|)(?:[eE][+-]?\\d+|)/g;n.parseJSON=function(b){if(a.JSON&&a.JSON.parse)return a.JSON.parse(b+\"\");var c,d=null,e=n.trim(b+\"\");return e&&!n.trim(e.replace(Gb,function(a,b,e,f){return c&&b&&(d=0),0===d?a:(c=e||b,d+=!f-!e,\"\")}))?Function(\"return \"+e)():n.error(\"Invalid JSON: \"+b)},n.parseXML=function(b){var c,d;if(!b||\"string\"!=typeof b)return null;try{a.DOMParser?(d=new a.DOMParser,c=d.parseFromString(b,\"text/xml\")):(c=new a.ActiveXObject(\"Microsoft.XMLDOM\"),c.async=\"false\",c.loadXML(b))}catch(e){c=void 0}return c&&c.documentElement&&!c.getElementsByTagName(\"parsererror\").length||n.error(\"Invalid XML: \"+b),c};var Hb=/#.*$/,Ib=/([?&])_=[^&]*/,Jb=/^(.*?):[ \\t]*([^\\r\\n]*)\\r?$/gm,Kb=/^(?:about|app|app-storage|.+-extension|file|res|widget):$/,Lb=/^(?:GET|HEAD)$/,Mb=/^\\/\\//,Nb=/^([\\w.+-]+:)(?:\\/\\/(?:[^\\/?#]*@|)([^\\/?#:]*)(?::(\\d+)|)|)/,Ob={},Pb={},Qb=\"*/\".concat(\"*\"),Rb=Db.href,Sb=Nb.exec(Rb.toLowerCase())||[];function Tb(a){return function(b,c){\"string\"!=typeof b&&(c=b,b=\"*\");var d,e=0,f=b.toLowerCase().match(G)||[];if(n.isFunction(c))while(d=f[e++])\"+\"===d.charAt(0)?(d=d.slice(1)||\"*\",(a[d]=a[d]||[]).unshift(c)):(a[d]=a[d]||[]).push(c)}}function Ub(a,b,c,d){var e={},f=a===Pb;function g(h){var i;return e[h]=!0,n.each(a[h]||[],function(a,h){var j=h(b,c,d);return\"string\"!=typeof j||f||e[j]?f?!(i=j):void 0:(b.dataTypes.unshift(j),g(j),!1)}),i}return g(b.dataTypes[0])||!e[\"*\"]&&g(\"*\")}function Vb(a,b){var c,d,e=n.ajaxSettings.flatOptions||{};for(d in b)void 0!==b[d]&&((e[d]?a:c||(c={}))[d]=b[d]);return c&&n.extend(!0,a,c),a}function Wb(a,b,c){var d,e,f,g,h=a.contents,i=a.dataTypes;while(\"*\"===i[0])i.shift(),void 0===e&&(e=a.mimeType||b.getResponseHeader(\"Content-Type\"));if(e)for(g in h)if(h[g]&&h[g].test(e)){i.unshift(g);break}if(i[0]in c)f=i[0];else{for(g in c){if(!i[0]||a.converters[g+\" \"+i[0]]){f=g;break}d||(d=g)}f=f||d}return f?(f!==i[0]&&i.unshift(f),c[f]):void 0}function Xb(a,b,c,d){var e,f,g,h,i,j={},k=a.dataTypes.slice();if(k[1])for(g in a.converters)j[g.toLowerCase()]=a.converters[g];f=k.shift();while(f)if(a.responseFields[f]&&(c[a.responseFields[f]]=b),!i&&d&&a.dataFilter&&(b=a.dataFilter(b,a.dataType)),i=f,f=k.shift())if(\"*\"===f)f=i;else if(\"*\"!==i&&i!==f){if(g=j[i+\" \"+f]||j[\"* \"+f],!g)for(e in j)if(h=e.split(\" \"),h[1]===f&&(g=j[i+\" \"+h[0]]||j[\"* \"+h[0]])){g===!0?g=j[e]:j[e]!==!0&&(f=h[0],k.unshift(h[1]));break}if(g!==!0)if(g&&a[\"throws\"])b=g(b);else try{b=g(b)}catch(l){return{state:\"parsererror\",error:g?l:\"No conversion from \"+i+\" to \"+f}}}return{state:\"success\",data:b}}n.extend({active:0,lastModified:{},etag:{},ajaxSettings:{url:Rb,type:\"GET\",isLocal:Kb.test(Sb[1]),global:!0,processData:!0,async:!0,contentType:\"application/x-www-form-urlencoded; charset=UTF-8\",accepts:{\"*\":Qb,text:\"text/plain\",html:\"text/html\",xml:\"application/xml, text/xml\",json:\"application/json, text/javascript\"},contents:{xml:/\\bxml\\b/,html:/\\bhtml/,json:/\\bjson\\b/},responseFields:{xml:\"responseXML\",text:\"responseText\",json:\"responseJSON\"},converters:{\"* text\":String,\"text html\":!0,\"text json\":n.parseJSON,\"text xml\":n.parseXML},flatOptions:{url:!0,context:!0}},ajaxSetup:function(a,b){return b?Vb(Vb(a,n.ajaxSettings),b):Vb(n.ajaxSettings,a)},ajaxPrefilter:Tb(Ob),ajaxTransport:Tb(Pb),ajax:function(b,c){\"object\"==typeof b&&(c=b,b=void 0),c=c||{};var d,e,f,g,h,i,j,k,l=n.ajaxSetup({},c),m=l.context||l,o=l.context&&(m.nodeType||m.jquery)?n(m):n.event,p=n.Deferred(),q=n.Callbacks(\"once memory\"),r=l.statusCode||{},s={},t={},u=0,v=\"canceled\",w={readyState:0,getResponseHeader:function(a){var b;if(2===u){if(!k){k={};while(b=Jb.exec(g))k[b[1].toLowerCase()]=b[2]}b=k[a.toLowerCase()]}return null==b?null:b},getAllResponseHeaders:function(){return 2===u?g:null},setRequestHeader:function(a,b){var c=a.toLowerCase();return u||(a=t[c]=t[c]||a,s[a]=b),this},overrideMimeType:function(a){return u||(l.mimeType=a),this},statusCode:function(a){var b;if(a)if(2>u)for(b in a)r[b]=[r[b],a[b]];else w.always(a[w.status]);return this},abort:function(a){var b=a||v;return j&&j.abort(b),y(0,b),this}};if(p.promise(w).complete=q.add,w.success=w.done,w.error=w.fail,l.url=((b||l.url||Rb)+\"\").replace(Hb,\"\").replace(Mb,Sb[1]+\"//\"),l.type=c.method||c.type||l.method||l.type,l.dataTypes=n.trim(l.dataType||\"*\").toLowerCase().match(G)||[\"\"],null==l.crossDomain&&(d=Nb.exec(l.url.toLowerCase()),l.crossDomain=!(!d||d[1]===Sb[1]&&d[2]===Sb[2]&&(d[3]||(\"http:\"===d[1]?\"80\":\"443\"))===(Sb[3]||(\"http:\"===Sb[1]?\"80\":\"443\")))),l.data&&l.processData&&\"string\"!=typeof l.data&&(l.data=n.param(l.data,l.traditional)),Ub(Ob,l,c,w),2===u)return w;i=n.event&&l.global,i&&0===n.active++&&n.event.trigger(\"ajaxStart\"),l.type=l.type.toUpperCase(),l.hasContent=!Lb.test(l.type),f=l.url,l.hasContent||(l.data&&(f=l.url+=(Fb.test(f)?\"&\":\"?\")+l.data,delete l.data),l.cache===!1&&(l.url=Ib.test(f)?f.replace(Ib,\"$1_=\"+Eb++):f+(Fb.test(f)?\"&\":\"?\")+\"_=\"+Eb++)),l.ifModified&&(n.lastModified[f]&&w.setRequestHeader(\"If-Modified-Since\",n.lastModified[f]),n.etag[f]&&w.setRequestHeader(\"If-None-Match\",n.etag[f])),(l.data&&l.hasContent&&l.contentType!==!1||c.contentType)&&w.setRequestHeader(\"Content-Type\",l.contentType),w.setRequestHeader(\"Accept\",l.dataTypes[0]&&l.accepts[l.dataTypes[0]]?l.accepts[l.dataTypes[0]]+(\"*\"!==l.dataTypes[0]?\", \"+Qb+\"; q=0.01\":\"\"):l.accepts[\"*\"]);for(e in l.headers)w.setRequestHeader(e,l.headers[e]);if(l.beforeSend&&(l.beforeSend.call(m,w,l)===!1||2===u))return w.abort();v=\"abort\";for(e in{success:1,error:1,complete:1})w[e](l[e]);if(j=Ub(Pb,l,c,w)){if(w.readyState=1,i&&o.trigger(\"ajaxSend\",[w,l]),2===u)return w;l.async&&l.timeout>0&&(h=a.setTimeout(function(){w.abort(\"timeout\")},l.timeout));try{u=1,j.send(s,y)}catch(x){if(!(2>u))throw x;y(-1,x)}}else y(-1,\"No Transport\");function y(b,c,d,e){var k,s,t,v,x,y=c;2!==u&&(u=2,h&&a.clearTimeout(h),j=void 0,g=e||\"\",w.readyState=b>0?4:0,k=b>=200&&300>b||304===b,d&&(v=Wb(l,w,d)),v=Xb(l,v,w,k),k?(l.ifModified&&(x=w.getResponseHeader(\"Last-Modified\"),x&&(n.lastModified[f]=x),x=w.getResponseHeader(\"etag\"),x&&(n.etag[f]=x)),204===b||\"HEAD\"===l.type?y=\"nocontent\":304===b?y=\"notmodified\":(y=v.state,s=v.data,t=v.error,k=!t)):(t=y,!b&&y||(y=\"error\",0>b&&(b=0))),w.status=b,w.statusText=(c||y)+\"\",k?p.resolveWith(m,[s,y,w]):p.rejectWith(m,[w,y,t]),w.statusCode(r),r=void 0,i&&o.trigger(k?\"ajaxSuccess\":\"ajaxError\",[w,l,k?s:t]),q.fireWith(m,[w,y]),i&&(o.trigger(\"ajaxComplete\",[w,l]),--n.active||n.event.trigger(\"ajaxStop\")))}return w},getJSON:function(a,b,c){return n.get(a,b,c,\"json\")},getScript:function(a,b){return n.get(a,void 0,b,\"script\")}}),n.each([\"get\",\"post\"],function(a,b){n[b]=function(a,c,d,e){return n.isFunction(c)&&(e=e||d,d=c,c=void 0),n.ajax(n.extend({url:a,type:b,dataType:e,data:c,success:d},n.isPlainObject(a)&&a))}}),n._evalUrl=function(a){return n.ajax({url:a,type:\"GET\",dataType:\"script\",cache:!0,async:!1,global:!1,\"throws\":!0})},n.fn.extend({wrapAll:function(a){if(n.isFunction(a))return this.each(function(b){n(this).wrapAll(a.call(this,b))});if(this[0]){var b=n(a,this[0].ownerDocument).eq(0).clone(!0);this[0].parentNode&&b.insertBefore(this[0]),b.map(function(){var a=this;while(a.firstChild&&1===a.firstChild.nodeType)a=a.firstChild;return a}).append(this)}return this},wrapInner:function(a){return n.isFunction(a)?this.each(function(b){n(this).wrapInner(a.call(this,b))}):this.each(function(){var b=n(this),c=b.contents();c.length?c.wrapAll(a):b.append(a)})},wrap:function(a){var b=n.isFunction(a);return this.each(function(c){n(this).wrapAll(b?a.call(this,c):a)})},unwrap:function(){return this.parent().each(function(){n.nodeName(this,\"body\")||n(this).replaceWith(this.childNodes)}).end()}});function Yb(a){return a.style&&a.style.display||n.css(a,\"display\")}function Zb(a){if(!n.contains(a.ownerDocument||d,a))return!0;while(a&&1===a.nodeType){if(\"none\"===Yb(a)||\"hidden\"===a.type)return!0;a=a.parentNode}return!1}n.expr.filters.hidden=function(a){return l.reliableHiddenOffsets()?a.offsetWidth<=0&&a.offsetHeight<=0&&!a.getClientRects().length:Zb(a)},n.expr.filters.visible=function(a){return!n.expr.filters.hidden(a)};var $b=/%20/g,_b=/\\[\\]$/,ac=/\\r?\\n/g,bc=/^(?:submit|button|image|reset|file)$/i,cc=/^(?:input|select|textarea|keygen)/i;function dc(a,b,c,d){var e;if(n.isArray(b))n.each(b,function(b,e){c||_b.test(a)?d(a,e):dc(a+\"[\"+(\"object\"==typeof e&&null!=e?b:\"\")+\"]\",e,c,d)});else if(c||\"object\"!==n.type(b))d(a,b);else for(e in b)dc(a+\"[\"+e+\"]\",b[e],c,d)}n.param=function(a,b){var c,d=[],e=function(a,b){b=n.isFunction(b)?b():null==b?\"\":b,d[d.length]=encodeURIComponent(a)+\"=\"+encodeURIComponent(b)};if(void 0===b&&(b=n.ajaxSettings&&n.ajaxSettings.traditional),n.isArray(a)||a.jquery&&!n.isPlainObject(a))n.each(a,function(){e(this.name,this.value)});else for(c in a)dc(c,a[c],b,e);return d.join(\"&\").replace($b,\"+\")},n.fn.extend({serialize:function(){return n.param(this.serializeArray())},serializeArray:function(){return this.map(function(){var a=n.prop(this,\"elements\");return a?n.makeArray(a):this}).filter(function(){var a=this.type;return this.name&&!n(this).is(\":disabled\")&&cc.test(this.nodeName)&&!bc.test(a)&&(this.checked||!Z.test(a))}).map(function(a,b){var c=n(this).val();return null==c?null:n.isArray(c)?n.map(c,function(a){return{name:b.name,value:a.replace(ac,\"\\r\\n\")}}):{name:b.name,value:c.replace(ac,\"\\r\\n\")}}).get()}}),n.ajaxSettings.xhr=void 0!==a.ActiveXObject?function(){return this.isLocal?ic():d.documentMode>8?hc():/^(get|post|head|put|delete|options)$/i.test(this.type)&&hc()||ic()}:hc;var ec=0,fc={},gc=n.ajaxSettings.xhr();a.attachEvent&&a.attachEvent(\"onunload\",function(){for(var a in fc)fc[a](void 0,!0)}),l.cors=!!gc&&\"withCredentials\"in gc,gc=l.ajax=!!gc,gc&&n.ajaxTransport(function(b){if(!b.crossDomain||l.cors){var c;return{send:function(d,e){var f,g=b.xhr(),h=++ec;if(g.open(b.type,b.url,b.async,b.username,b.password),b.xhrFields)for(f in b.xhrFields)g[f]=b.xhrFields[f];b.mimeType&&g.overrideMimeType&&g.overrideMimeType(b.mimeType),b.crossDomain||d[\"X-Requested-With\"]||(d[\"X-Requested-With\"]=\"XMLHttpRequest\");for(f in d)void 0!==d[f]&&g.setRequestHeader(f,d[f]+\"\");g.send(b.hasContent&&b.data||null),c=function(a,d){var f,i,j;if(c&&(d||4===g.readyState))if(delete fc[h],c=void 0,g.onreadystatechange=n.noop,d)4!==g.readyState&&g.abort();else{j={},f=g.status,\"string\"==typeof g.responseText&&(j.text=g.responseText);try{i=g.statusText}catch(k){i=\"\"}f||!b.isLocal||b.crossDomain?1223===f&&(f=204):f=j.text?200:404}j&&e(f,i,j,g.getAllResponseHeaders())},b.async?4===g.readyState?a.setTimeout(c):g.onreadystatechange=fc[h]=c:c()},abort:function(){c&&c(void 0,!0)}}}});function hc(){try{return new a.XMLHttpRequest}catch(b){}}function ic(){try{return new a.ActiveXObject(\"Microsoft.XMLHTTP\")}catch(b){}}n.ajaxSetup({accepts:{script:\"text/javascript, application/javascript, application/ecmascript, application/x-ecmascript\"},contents:{script:/\\b(?:java|ecma)script\\b/},converters:{\"text script\":function(a){return n.globalEval(a),a}}}),n.ajaxPrefilter(\"script\",function(a){void 0===a.cache&&(a.cache=!1),a.crossDomain&&(a.type=\"GET\",a.global=!1)}),n.ajaxTransport(\"script\",function(a){if(a.crossDomain){var b,c=d.head||n(\"head\")[0]||d.documentElement;return{send:function(e,f){b=d.createElement(\"script\"),b.async=!0,a.scriptCharset&&(b.charset=a.scriptCharset),b.src=a.url,b.onload=b.onreadystatechange=function(a,c){(c||!b.readyState||/loaded|complete/.test(b.readyState))&&(b.onload=b.onreadystatechange=null,b.parentNode&&b.parentNode.removeChild(b),b=null,c||f(200,\"success\"))},c.insertBefore(b,c.firstChild)},abort:function(){b&&b.onload(void 0,!0)}}}});var jc=[],kc=/(=)\\?(?=&|$)|\\?\\?/;n.ajaxSetup({jsonp:\"callback\",jsonpCallback:function(){var a=jc.pop()||n.expando+\"_\"+Eb++;return this[a]=!0,a}}),n.ajaxPrefilter(\"json jsonp\",function(b,c,d){var e,f,g,h=b.jsonp!==!1&&(kc.test(b.url)?\"url\":\"string\"==typeof b.data&&0===(b.contentType||\"\").indexOf(\"application/x-www-form-urlencoded\")&&kc.test(b.data)&&\"data\");return h||\"jsonp\"===b.dataTypes[0]?(e=b.jsonpCallback=n.isFunction(b.jsonpCallback)?b.jsonpCallback():b.jsonpCallback,h?b[h]=b[h].replace(kc,\"$1\"+e):b.jsonp!==!1&&(b.url+=(Fb.test(b.url)?\"&\":\"?\")+b.jsonp+\"=\"+e),b.converters[\"script json\"]=function(){return g||n.error(e+\" was not called\"),g[0]},b.dataTypes[0]=\"json\",f=a[e],a[e]=function(){g=arguments},d.always(function(){void 0===f?n(a).removeProp(e):a[e]=f,b[e]&&(b.jsonpCallback=c.jsonpCallback,jc.push(e)),g&&n.isFunction(f)&&f(g[0]),g=f=void 0}),\"script\"):void 0}),n.parseHTML=function(a,b,c){if(!a||\"string\"!=typeof a)return null;\"boolean\"==typeof b&&(c=b,b=!1),b=b||d;var e=x.exec(a),f=!c&&[];return e?[b.createElement(e[1])]:(e=ja([a],b,f),f&&f.length&&n(f).remove(),n.merge([],e.childNodes))};var lc=n.fn.load;n.fn.load=function(a,b,c){if(\"string\"!=typeof a&&lc)return lc.apply(this,arguments);var d,e,f,g=this,h=a.indexOf(\" \");return h>-1&&(d=n.trim(a.slice(h,a.length)),a=a.slice(0,h)),n.isFunction(b)?(c=b,b=void 0):b&&\"object\"==typeof b&&(e=\"POST\"),g.length>0&&n.ajax({url:a,type:e||\"GET\",dataType:\"html\",data:b}).done(function(a){f=arguments,g.html(d?n(\"<div>\").append(n.parseHTML(a)).find(d):a)}).always(c&&function(a,b){g.each(function(){c.apply(this,f||[a.responseText,b,a])})}),this},n.each([\"ajaxStart\",\"ajaxStop\",\"ajaxComplete\",\"ajaxError\",\"ajaxSuccess\",\"ajaxSend\"],function(a,b){n.fn[b]=function(a){return this.on(b,a)}}),n.expr.filters.animated=function(a){return n.grep(n.timers,function(b){return a===b.elem}).length};function mc(a){return n.isWindow(a)?a:9===a.nodeType?a.defaultView||a.parentWindow:!1}n.offset={setOffset:function(a,b,c){var d,e,f,g,h,i,j,k=n.css(a,\"position\"),l=n(a),m={};\"static\"===k&&(a.style.position=\"relative\"),h=l.offset(),f=n.css(a,\"top\"),i=n.css(a,\"left\"),j=(\"absolute\"===k||\"fixed\"===k)&&n.inArray(\"auto\",[f,i])>-1,j?(d=l.position(),g=d.top,e=d.left):(g=parseFloat(f)||0,e=parseFloat(i)||0),n.isFunction(b)&&(b=b.call(a,c,n.extend({},h))),null!=b.top&&(m.top=b.top-h.top+g),null!=b.left&&(m.left=b.left-h.left+e),\"using\"in b?b.using.call(a,m):l.css(m)}},n.fn.extend({offset:function(a){if(arguments.length)return void 0===a?this:this.each(function(b){n.offset.setOffset(this,a,b)});var b,c,d={top:0,left:0},e=this[0],f=e&&e.ownerDocument;if(f)return b=f.documentElement,n.contains(b,e)?(\"undefined\"!=typeof e.getBoundingClientRect&&(d=e.getBoundingClientRect()),c=mc(f),{top:d.top+(c.pageYOffset||b.scrollTop)-(b.clientTop||0),left:d.left+(c.pageXOffset||b.scrollLeft)-(b.clientLeft||0)}):d},position:function(){if(this[0]){var a,b,c={top:0,left:0},d=this[0];return\"fixed\"===n.css(d,\"position\")?b=d.getBoundingClientRect():(a=this.offsetParent(),b=this.offset(),n.nodeName(a[0],\"html\")||(c=a.offset()),c.top+=n.css(a[0],\"borderTopWidth\",!0),c.left+=n.css(a[0],\"borderLeftWidth\",!0)),{top:b.top-c.top-n.css(d,\"marginTop\",!0),left:b.left-c.left-n.css(d,\"marginLeft\",!0)}}},offsetParent:function(){return this.map(function(){var a=this.offsetParent;while(a&&!n.nodeName(a,\"html\")&&\"static\"===n.css(a,\"position\"))a=a.offsetParent;return a||Qa})}}),n.each({scrollLeft:\"pageXOffset\",scrollTop:\"pageYOffset\"},function(a,b){var c=/Y/.test(b);n.fn[a]=function(d){return Y(this,function(a,d,e){var f=mc(a);return void 0===e?f?b in f?f[b]:f.document.documentElement[d]:a[d]:void(f?f.scrollTo(c?n(f).scrollLeft():e,c?e:n(f).scrollTop()):a[d]=e)},a,d,arguments.length,null)}}),n.each([\"top\",\"left\"],function(a,b){n.cssHooks[b]=Ua(l.pixelPosition,function(a,c){return c?(c=Sa(a,b),Oa.test(c)?n(a).position()[b]+\"px\":c):void 0})}),n.each({Height:\"height\",Width:\"width\"},function(a,b){n.each({\r\npadding:\"inner\"+a,content:b,\"\":\"outer\"+a},function(c,d){n.fn[d]=function(d,e){var f=arguments.length&&(c||\"boolean\"!=typeof d),g=c||(d===!0||e===!0?\"margin\":\"border\");return Y(this,function(b,c,d){var e;return n.isWindow(b)?b.document.documentElement[\"client\"+a]:9===b.nodeType?(e=b.documentElement,Math.max(b.body[\"scroll\"+a],e[\"scroll\"+a],b.body[\"offset\"+a],e[\"offset\"+a],e[\"client\"+a])):void 0===d?n.css(b,c,g):n.style(b,c,d,g)},b,f?d:void 0,f,null)}})}),n.fn.extend({bind:function(a,b,c){return this.on(a,null,b,c)},unbind:function(a,b){return this.off(a,null,b)},delegate:function(a,b,c,d){return this.on(b,a,c,d)},undelegate:function(a,b,c){return 1===arguments.length?this.off(a,\"**\"):this.off(b,a||\"**\",c)}}),n.fn.size=function(){return this.length},n.fn.andSelf=n.fn.addBack,\"function\"==typeof define&&define.amd&&define(\"jquery\",[],function(){return n});var nc=a.jQuery,oc=a.$;return n.noConflict=function(b){return a.$===n&&(a.$=oc),b&&a.jQuery===n&&(a.jQuery=nc),n},b||(a.jQuery=a.$=n),n});");
		default:
			return new StringBuilder();
		}
	}
}

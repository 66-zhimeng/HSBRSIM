using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

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

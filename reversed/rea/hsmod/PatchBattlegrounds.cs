using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Assets;
using BepInEx.Logging;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using HarmonyLib;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.Util;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace HsMod;

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

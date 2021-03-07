using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace TruePause
{

	[BepInPlugin("org.ltmadness.valheim.truepause", "TruePause", "0.0.1")]
    public class TruePause : BaseUnityPlugin
    {
		public void Awake() => Harmony.CreateAndPatchAll(typeof(TruePause), null);


		[HarmonyPatch(typeof(Menu), "Update")]
		[HarmonyPrefix]
		public static bool Update(ref Menu __instance)
		{
			if (Game.instance.IsShuttingDown())
			{
				__instance.m_root.gameObject.SetActive(false);
				return false;
			}
			if (__instance.m_root.gameObject.activeSelf)
			{
				AccessTools.Field(typeof(Menu), "m_hiddenFrames").SetValue(__instance, 0);
				if ((Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")) && !(GameObject)AccessTools.Field(typeof(Menu), "m_settingsInstance").GetValue(__instance) && !Feedback.IsVisible())
				{
					if (__instance.m_quitDialog.gameObject.activeSelf)
					{
						__instance.OnQuitNo();
						return false;
					}
					if (__instance.m_logoutDialog.gameObject.activeSelf)
					{
						__instance.OnLogoutNo();
						return false;
					}
					Time.timeScale = 1f;
					__instance.m_root.gameObject.SetActive(false);
					return false;
				}
			}
			else
			{
				int m_hiddenFrames = (int) AccessTools.Field(typeof(Menu), "m_hiddenFrames").GetValue(__instance);
				m_hiddenFrames++;
				AccessTools.Field(typeof(Menu), "m_hiddenFrames").SetValue(__instance, m_hiddenFrames);
				bool flag = !InventoryGui.IsVisible() && !Minimap.IsOpen() && !global::Console.IsVisible() && !TextInput.IsVisible() && !ZNet.instance.InPasswordDialog() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible();
				if ((Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")) && flag)
				{
					if (ZNet.instance.IsServer())
					{
						Time.timeScale = 0f;
						__instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
						__instance.m_root.Find("Menu").gameObject.SetActive(false);
					}
					Gogan.LogEvent("Screen", "Enter", "Menu", 0L);
					__instance.m_root.gameObject.SetActive(true);
					__instance.m_menuDialog.gameObject.SetActive(true);
					__instance.m_logoutDialog.gameObject.SetActive(false);
					__instance.m_quitDialog.gameObject.SetActive(false);
				}
			}
			return false;
		}

		[HarmonyPatch(typeof(Menu), "OnLogoutYes")]
		[HarmonyPrefix]
		public static void OnLogoutYes()
		{
			Time.timeScale = 1f;
		}

		[HarmonyPatch(typeof(Menu), "OnQuitYes")]
		[HarmonyPrefix]
		public static void OnQuitYes()
		{
			Time.timeScale = 1f;
		}

		[HarmonyPatch(typeof(Menu), "OnClose")]
		[HarmonyPrefix]
		public static void OnClose(ref Menu __instance)
		{
			Time.timeScale = 1f;
		}

		[HarmonyPatch(typeof(Menu), "OnQuit")]
		[HarmonyPrefix]
		public static void OnQuit(ref Menu __instance)
		{
			__instance.m_root.Find("OLD_menu").gameObject.SetActive(false);
		}

		[HarmonyPatch(typeof(Menu), "OnLogout")]
		[HarmonyPrefix]
		public static void OnLogout(ref Menu __instance)
		{
			__instance.m_root.Find("OLD_menu").gameObject.SetActive(false);
		}

		[HarmonyPatch(typeof(Menu), "OnLogoutNo")]
		[HarmonyPrefix]
		public static void OnLogoutNo(ref Menu __instance)
		{
			__instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
		}

		[HarmonyPatch(typeof(Menu), "OnQuitNo")]
		[HarmonyPrefix]
		public static void OnQuitNo(ref Menu __instance)
		{
			__instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
		}
	}
}

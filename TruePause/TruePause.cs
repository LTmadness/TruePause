using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TruePause
{

	[BepInPlugin("org.ltmadness.valheim.truepause", "TruePause", "0.0.1")]
    public class TruePause : BaseUnityPlugin
    {
		private static List<string> windows = new List<string>() { "MenuRoot", "Audio", "Controls", "Misc", "Graphics", "dialog" };
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
					Time.timeScale = 1f;
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
		public static bool OnLogoutYes()
		{
			Time.timeScale = 1f;
			Gogan.LogEvent("Game", "LogOut", "", 0L);
			Game.instance.Logout();
			return false;
		}

		[HarmonyPatch(typeof(Menu), "OnQuitYes")]
		[HarmonyPrefix]
		public static bool OnQuitYes()
		{
			Time.timeScale = 1f;
			Gogan.LogEvent("Game", "Quit", "", 0L);
			Application.Quit();
			return false;
		}
	}
}

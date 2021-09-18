using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TruePause
{

    [BepInPlugin("org.ltmadness.valheim.truepause", "TruePause", "0.0.5")]
    public class TruePause : BaseUnityPlugin
    {
        private static bool musicStopped = false;
        private static ConfigEntry<bool> focusPause;
        public void Awake()
        {
            focusPause = Config.Bind<bool>("Settings", "Pause when game is not focused", true, "If window you focused ojn is not the game the game will pause");
            Harmony.CreateAndPatchAll(typeof(TruePause), null);
        }

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
                    StartTime();
                    __instance.m_root.gameObject.SetActive(false);
                    return false;
                }
            }
            else
            {
                int m_hiddenFrames = (int)AccessTools.Field(typeof(Menu), "m_hiddenFrames").GetValue(__instance);
                m_hiddenFrames++;
                AccessTools.Field(typeof(Menu), "m_hiddenFrames").SetValue(__instance, m_hiddenFrames);
                bool flag = !InventoryGui.IsVisible() && !Minimap.IsOpen() && !global::Console.IsVisible() && !TextInput.IsVisible() && !ZNet.instance.InPasswordDialog() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible();
                if (((Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")) || (!Application.isFocused && focusPause.Value && ZNet.instance.IsServer())) && flag)
                {
                    __instance.m_root.gameObject.SetActive(true);
                    __instance.m_menuDialog.gameObject.SetActive(true);
                    if (ZNet.instance.IsServer())
                    {
                        StopTime();
                        __instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
                        __instance.m_root.Find("Menu").gameObject.SetActive(false);
                    }
                    Gogan.LogEvent("Screen", "Enter", "Menu", 0L);
                    __instance.m_logoutDialog.gameObject.SetActive(false);
                    __instance.m_quitDialog.gameObject.SetActive(false);
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Menu), "OnLogoutYes")]
        [HarmonyPostfix]
        public static void OnLogoutYes()
        {
            StartTime();
        }

        [HarmonyPatch(typeof(Menu), "OnQuitYes")]
        [HarmonyPostfix]
        public static void OnQuitYes()
        {
            StartTime();
        }

        [HarmonyPatch(typeof(Menu), "OnClose")]
        [HarmonyPostfix]
        public static void OnClose(ref Menu __instance)
        {
            StartTime();
        }

        [HarmonyPatch(typeof(Menu), "OnQuit")]
        [HarmonyPostfix]
        public static void OnQuit(ref Menu __instance)
        {
            if (ZNet.instance.IsServer())
            {
                __instance.m_root.Find("OLD_menu").gameObject.SetActive(false);
                __instance.m_root.Find("Menu").gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(Menu), "OnLogout")]
        [HarmonyPostfix]
        public static void OnLogout(ref Menu __instance)
        {
            if (ZNet.instance.IsServer())
            {
                __instance.m_root.Find("OLD_menu").gameObject.SetActive(false);
                __instance.m_root.Find("Menu").gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(Menu), "OnLogoutNo")]
        [HarmonyPostfix]
        public static void OnLogoutNo(ref Menu __instance)
        {
            if (ZNet.instance.IsServer())
            {
                __instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
                __instance.m_root.Find("Menu").gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(Menu), "OnQuitNo")]
        [HarmonyPostfix]
        public static void OnQuitNo(ref Menu __instance)
        {
            if (ZNet.instance.IsServer())
            {
                __instance.m_root.Find("OLD_menu").gameObject.SetActive(true);
                __instance.m_root.Find("Menu").gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(MusicMan), "UpdateMusic")]
        [HarmonyPrefix]
        public static bool UpdateMusic(/*float dt*/)
        {
            return !musicStopped;
        }

            public static void StopTime()
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            musicStopped = true;
        }

        public static void StartTime()
        {
            if (Time.timeScale.Equals(0f))
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
                musicStopped = false;
            }
        }
    }
}

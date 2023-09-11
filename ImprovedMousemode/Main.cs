using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using DV.UI;
using DV.Utils;
using UnityEngine;


namespace ImprovedMousemode;

#if DEBUG
[EnableReloading]
#endif
public static class Main
{
	public static Harmony? harmony = null;
	public static Settings Settings = new();

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			Settings = Settings.Load<Settings>(modEntry);
			harmony?.PatchAll(Assembly.GetExecutingAssembly());

			// Other plugin startup logic
			modEntry.OnToggle = OnToggle;
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}

		return true;
	}

	private static bool OnToggle(UnityModManager.ModEntry modEntry, bool enabled)
	{
		if (enabled)
		{
			harmony?.PatchAll(Assembly.GetExecutingAssembly());
		}
		else
		{
			harmony?.UnpatchAll(modEntry.Info.Id);
		}
		return true;
	}

	private static void OnGUI(UnityModManager.ModEntry modEntry)
	{
		Settings.Draw(modEntry);
	}

	private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
	{
		Settings.Save(modEntry);
	}

	[HarmonyPatch(typeof(CanvasProviderDV), nameof(CanvasProviderDV.ShouldTryToggle))]
	class AddKeyDownToMouseModeTrigger
	{
		public static bool Postfix(bool result, CanvasController.ElementType type)
		{
			bool inMousemode = SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.IsOn(CanvasController.ElementType.MouseMode);
			if (Settings.holdForMousemode && type == CanvasController.ElementType.MouseMode && !inMousemode)
			{
				return KeyBindings.mouseLookKeys.IsUp() || KeyBindings.mouseLookKeys.IsDown();
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(ACanvasController<CanvasController.ElementType>))]
	class CenterMouseOnEnteringMousemode
	{
		[HarmonyPatch(nameof(ACanvasController<CanvasController.ElementType>.TrySetState), new Type[] { typeof(CanvasController.ElementType), typeof(bool) })]
		public static void Prefix(CanvasController.ElementType type, bool on)
		{
			if (Settings.mouseCenteringEnabled && type == CanvasController.ElementType.MouseMode && on)
			{
				Win32MouseHandler.GetCursorPos(out Vector2Int centerMousePosition);
				AccessTools.FieldRefAccess<CursorManager, Vector2Int>(CursorManager.Instance, "mousePosition") = centerMousePosition;
			}
		}
	}
}

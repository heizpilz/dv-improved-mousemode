using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using DV.UI;

namespace ImprovedMousemode;

public static class Main
{

	public static UnityModManager.ModEntry? mod;

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		Harmony? harmony = null;

		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Other plugin startup logic
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}

		return true;
	}
	
	[HarmonyPatch(typeof(CanvasProviderDV), nameof(CanvasProviderDV.ShouldTryToggle))]
	class AddKeyDownToMouseModeTrigger
	{
		static bool Postfix(bool result, CanvasController.ElementType type)
		{
			if (type == CanvasController.ElementType.MouseMode)
			{
				return KeyBindings.mouseLookKeys.IsUp() || KeyBindings.mouseLookKeys.IsDown();
			} else {
				return result;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityModManagerNet;
using DV.UI;
using DV.Utils;


namespace ImprovedMousemode;

[EnableReloading]
public static class Main
{

	public static UnityModManager.ModEntry? mod;
	public static Harmony? harmony = null;
	public static Settings Settings = new();

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			Settings = Settings.Load<Settings>(modEntry);
			PatchWithSettings();

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
			PatchWithSettings();
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

	public static void PatchWithSettings()
	{
		harmony?.Patch(AddKeyDownToMouseModeTrigger.original, postfix: new HarmonyMethod(AddKeyDownToMouseModeTrigger.postfix));
		if (Settings.mouseCenteringEnabled)
		{
			harmony?.Patch(CenterMouseOnEnteringMousemode.original, transpiler: new HarmonyMethod(CenterMouseOnEnteringMousemode.transpiler));
		}
		else
		{
			harmony?.Unpatch(CenterMouseOnEnteringMousemode.original, CenterMouseOnEnteringMousemode.transpiler);
		}
	}

	[HarmonyPatch(typeof(CanvasProviderDV), nameof(CanvasProviderDV.ShouldTryToggle))]
	class AddKeyDownToMouseModeTrigger
	{
		internal static readonly MethodInfo original = typeof(CanvasProviderDV).GetMethod(nameof(CanvasProviderDV.ShouldTryToggle));
		internal static readonly MethodInfo postfix = typeof(AddKeyDownToMouseModeTrigger).GetMethod(nameof(Postfix));
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

	[HarmonyPatch(typeof(CursorManager), "OnValueChanged")]
	class CenterMouseOnEnteringMousemode
	{
		internal static readonly MethodInfo original = typeof(CursorManager).GetMethod("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
		internal static readonly MethodInfo transpiler = typeof(CenterMouseOnEnteringMousemode).GetMethod(nameof(Transpiler));
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == typeof(DV.UI.Win32MouseHandler).GetMethod("SetCursorPos"))
				{
					// replace call to restore mouseposition with instruction that keeps the stack intact
					yield return new CodeInstruction(OpCodes.Pop);
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}
}

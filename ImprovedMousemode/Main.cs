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
	private static Harmony? harmony = null;

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Other plugin startup logic
			modEntry.OnToggle = OnToggle;
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

	[HarmonyPatch(typeof(CanvasProviderDV), nameof(CanvasProviderDV.ShouldTryToggle))]
	class AddKeyDownToMouseModeTrigger
	{
		static bool Postfix(bool result, CanvasController.ElementType type)
		{
			bool inMousemode = SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.IsOn(CanvasController.ElementType.MouseMode);
			if (type == CanvasController.ElementType.MouseMode && !inMousemode)
			{
				return KeyBindings.mouseLookKeys.IsUp() || KeyBindings.mouseLookKeys.IsDown();
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(CursorManager), "OnValueChanged")]
	class CenterMouseOnEnteringMousemode
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

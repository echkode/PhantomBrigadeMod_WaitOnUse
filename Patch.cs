using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Input.Components;

namespace EchKode.PBMods.WaitOnUse
{
	[HarmonyPatch]
	public static partial class Patch
	{
		[HarmonyPatch(typeof(CIViewCombatAction), "OnActionSelect")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Civca_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			// Fake a PaintingType.Immediate enum value so that an action can bypass timing placement and be put on
			// the combat timeline immediately.
			//
			// Trigger when paintingType == PaintingType.Timing, durationType = DurationType.Variable and duration == 1
			// This is a unique combo that none of the stock actions has.

			var cm = new CodeMatcher(instructions, generator);
			var removeSelectedActionMethodInfo = AccessTools.DeclaredMethod(typeof(InputContext), nameof(InputContext.RemoveSelectedAction));
			var durationTypeFieldInfo = AccessTools.DeclaredField(typeof(DataBlockActionCore), nameof(DataBlockActionCore.durationType));
			var durationFieldInfo = AccessTools.DeclaredField(typeof(DataBlockActionCore), nameof(DataBlockActionCore.duration));
			var removeSelectedActionMatch = new CodeMatch(OpCodes.Callvirt, removeSelectedActionMethodInfo);
			var loadTiming = new CodeInstruction(OpCodes.Ldc_I4_6);
			var loadZero = new CodeInstruction(OpCodes.Ldc_I4_0);
			var loadDurationType = new CodeInstruction(OpCodes.Ldfld, durationTypeFieldInfo);
			var loadDuration = new CodeInstruction(OpCodes.Ldfld, durationFieldInfo);
			var loadOne = new CodeInstruction(OpCodes.Ldc_R4, 1f);
			var callPlaceAction = CodeInstruction.Call(typeof(Patch), nameof(PlaceAction));
			var ret = new CodeInstruction(OpCodes.Ret);

			cm.Start()
				.MatchEndForward(removeSelectedActionMatch)
				.Advance(1);
			var loadAction = cm.Instruction.Clone();
			cm.Advance(1);
			var loadDataCore = cm.Instruction.Clone();
			cm.Advance(3);
			var loadPaintingType = cm.Instruction.Clone();
			cm.CreateLabel(out var switchStartLabel);
			var skipToSwitch = new CodeInstruction(OpCodes.Bne_Un, switchStartLabel);

			cm.InsertAndAdvance(loadPaintingType)
				.InsertAndAdvance(loadTiming)
				.InsertAndAdvance(skipToSwitch)
				.InsertAndAdvance(loadAction)
				.InsertAndAdvance(loadDataCore)
				.InsertAndAdvance(loadDurationType)
				.InsertAndAdvance(loadZero)
				.InsertAndAdvance(skipToSwitch)
				.InsertAndAdvance(loadAction)
				.InsertAndAdvance(loadDataCore)
				.InsertAndAdvance(loadDuration)
				.InsertAndAdvance(loadOne)
				.InsertAndAdvance(skipToSwitch)
				.InsertAndAdvance(loadAction)
				.InsertAndAdvance(callPlaceAction)
				.InsertAndAdvance(ret);

			return cm.InstructionEnumeration();
		}

		public static void PlaceAction(DataContainerAction actionData)
		{
			var input = Contexts.sharedInstance.input;
			var combat = Contexts.sharedInstance.combat;
			var unitCombat = IDUtility.GetCombatEntity(combat.unitSelected.id);
			var startTime = ActionUtility.GetLastActionTime(unitCombat, TrackType.Primary) + 0.001f;
			combat.ReplacePredictionTimeTarget(startTime);
			input.ReplaceCombatUIMode(CombatUIModes.Time_Placement);
			input.ReplaceSelectedAction(actionData.key);
			AudioUtility.CreateAudioEvent("ui_unit_select_action");
			CombatUIUtility.AttemptToFinishTimePlacement();
		}
	}
}

// Copyright (c) 2024 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System;

using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;

namespace EchKode.PBMods.WaitOnUse.Functions
{
	[Serializable]
	public sealed class CombatActionWaitOnUseValidate : ICombatActionValidationFunction
	{
		public bool IsValid(CombatEntity unitCombat) => Helper.FindEquipmentUseAction(unitCombat) != null;
	}

	[Serializable]
	public sealed class CombatActionWaitOnUse : ICombatActionExecutionFunction
	{
		public void Run(CombatEntity unitCombat, ActionEntity action)
		{
			// The wait action has already been created with a start time and the duration from the config file.
			// We just need to change its duration.

			var equipmentUseAction = Helper.FindEquipmentUseAction(unitCombat, waitAction: action);
			if (equipmentUseAction == null)
			{
				return;
			}

			var useEndTime = equipmentUseAction.startTime.f + equipmentUseAction.duration.f;
			var duration = useEndTime - action.startTime.f;
			if (duration < Helper.MinimumActionDuration)
			{
				return;
			}
			action.ReplaceDuration(duration);
		}
	}

	static class Helper
	{
		// Actions under a certain duration cause UI problems.
		internal const float MinimumActionDuration = 0.3f;

		internal static ActionEntity FindEquipmentUseAction(CombatEntity unitCombat, ActionEntity waitAction = null)
		{
			ActionEntity equipmentUseAction = null;
			var lastMovementTrackActionTime = waitAction == null
				? ActionUtility.GetLastActionTime(unitCombat, TrackType.Primary)
				: waitAction.startTime.f;
			var actions = Contexts.sharedInstance.action.GetEntitiesWithActionOwner(unitCombat.id.id);
			foreach (var action in actions)
			{
				if (action.isDisposed)
				{
					continue;
				}
				if (!action.hasStartTime || !action.hasDuration)
				{
					continue;
				}

				var actionEndTime = action.startTime.f + action.duration.f;
				if (actionEndTime < lastMovementTrackActionTime + MinimumActionDuration)
				{
					continue;
				}
				if (!action.hasActiveEquipmentPart)
				{
					continue;
				}

				if (!action.hasDataLinkActionCore)
				{
					continue;
				}

				var dataCore = action.dataLinkActionCore;
				if (dataCore.data == null || dataCore.data.trackType != TrackType.Secondary)
				{
					continue;
				}

				var part = IDUtility.GetEquipmentEntity(action.activeEquipmentPart.equipmentID);
				if (part == null || !part.hasPrimaryActivationSubsystem)
				{
					// Weapons have primary activation subsystems.
					continue;
				}

				if (part.IsPartTaggedAs(EquipmentTags.defensive))
				{
					// Exclude shields since they usually don't need the unit to stand around to be effective.
					continue;
				}

				if (equipmentUseAction != null && action.startTime.f > equipmentUseAction.startTime.f)
				{
					continue;
				}

				equipmentUseAction = action;
			}

			return equipmentUseAction;
		}
	}
}

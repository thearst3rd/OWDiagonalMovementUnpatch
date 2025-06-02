using HarmonyLib;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OWDiagonalMovementUnpatch
{
	/*
	 * Outer Wilds version 1.1.15 introduced a bug which allowed moving diagonally (i.e. holding W + A) to be faster
	 * than moving directly forward. It allowed a new speedrun strat and was super epic and awesome and good. In patch
	 * 1.1.16, Mobius FIXED this bug!!! It's ridiculous!!!!!!!!! In particular, CompositeInputCommands.UpdateFromAction
	 * was adjusted to normalize the input vector.
	 *
	 * This patch reimplements the function CompositeInputCommands.UpdateFromAction as it was in 1.1.15, with a few
	 * lines commented out to allow the input to not be normalized.
	 */
	[HarmonyPatch(typeof(CompositeInputCommands), "UpdateFromAction")]
	internal class UpdateFromActionPatch
	{
		static bool Prefix(CompositeInputCommands __instance)
		{
			__instance.PrimaryAction.Update();
			__instance.SecondaryAction.Update();
			bool num = __instance.PrimaryAction.HasActiveMouseInput || __instance.SecondaryAction.HasActiveMouseInput;
			bool flag = OWInput.UsingGamepad() && (InputUtil.IsJoystickAxis(__instance.PrimaryAction.AxisID) || InputUtil.IsJoystickAxis(__instance.SecondaryAction.AxisID));
			StickControl stickControl;
			if (__instance.TryGetSharedStickControl(out stickControl))
			{
				Vector2 value = stickControl.ReadUnprocessedValue();
				if (__instance.PrimaryAction.AxisID == AxisIdentifier.CTRLR_LSTICKX || __instance.PrimaryAction.AxisID == AxisIdentifier.CTRLR_RSTICKX)
				{
					value = new Vector2(value.y, value.x);
				}
				__instance.AxisValue = OWInputProcessorUtil.ApplyOWDoubleAxisDeadzones(value, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
			}
			else
			{
				float x = __instance._secondaryAction.Value;
				float y = __instance._primaryAction.Value;
				AxisControl axisControl;
				if (__instance.TryGetSharedAxisControl(__instance._secondaryAction, out axisControl))
				{
					x = axisControl.ReadUnprocessedValue();
				}
				AxisControl axisControl2;
				if (__instance.TryGetSharedAxisControl(__instance._primaryAction, out axisControl2))
				{
					y = axisControl2.ReadUnprocessedValue();
				}
				__instance.AxisValue = new Vector2(x, y);
				if (flag)
				{
					__instance.AxisValue = OWInputProcessorUtil.ApplyOWDoubleAxisDeadzones(__instance.AxisValue, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
				}
				// UNPATCH BEGINS HERE!!!
				// Comment out these lines so the input doesn't get normalized
				//else if (!InputUtil.IsMouseMoveAxis(__instance.PrimaryAction.AxisID) && !InputUtil.IsMouseMoveAxis(__instance.SecondaryAction.AxisID))
				//{
				//	__instance.AxisValue = __instance.AxisValue.normalized;
				//}
				// UNPATCH ENDS HERE!!!
			}
			__instance.InitializeAxisID();
			if (num)
			{
				__instance.IsActiveThisFrame = __instance.PrimaryAction.Phase == InputActionPhase.Performed || __instance.SecondaryAction.Phase == InputActionPhase.Performed;
				return false; // Don't run original method
			}
			float num2 = ((__instance.SecondaryAction != null) ? float.Epsilon : __instance.PressedThreshold);
			__instance.IsActiveThisFrame = __instance.AxisValue.magnitude > num2;
			return false; // Don't run original method
		}
	}
}

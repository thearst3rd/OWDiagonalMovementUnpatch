using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace OWDiagonalMovementUnpatch;

public class OWDiagonalMovementUnpatch : ModBehaviour
{
	public static OWDiagonalMovementUnpatch Instance;

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		// Starting here, you'll have access to OWML's mod helper.
		ModHelper.Console.WriteLine($"Outer Wilds Diagonal Movement Unpatch is loaded!", MessageType.Success);

		new Harmony("thearst3rd.OWDiagonalMovementUnpatch").PatchAll(Assembly.GetExecutingAssembly());
	}
}

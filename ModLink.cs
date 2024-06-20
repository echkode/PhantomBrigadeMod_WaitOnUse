// Copyright (c) 2024 EchKode
// SPDX-License-Identifier: BSD-3-Clause

namespace EchKode.PBMods.WaitOnUse
{
	public partial class ModLink : PhantomBrigade.Mods.ModLink
	{
		internal static int ModIndex;
		internal static string ModID;
		internal static string ModPath;

		public override void OnLoadStart()
		{
			ModIndex = modIndexPreload;
			ModID = metadata.id;
			ModPath = metadata.path;

			// Uncomment to get a file on the desktop showing the IL of the patched methods.
			// Output from FileLog.Log() will trigger the generation of that file regardless if this is set so
			// FileLog.Log() should be put in a guard.
			//EnableHarmonyFileLog();
		}
	}
}

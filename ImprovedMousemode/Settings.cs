using DV;
using UnityModManagerNet;

namespace ImprovedMousemode;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Center mouse on entering mousemode", DrawType.Toggle)]
    public bool mouseCenteringEnabled = true;

    public readonly bool vanillaSupportsHolding = BuildInfo.BUILD_VERSION_MAJOR > 97;
    [Draw("Enable holding mousemode button for mousemode", DrawType.Toggle, InvisibleOn = $"{nameof(vanillaSupportsHolding)}|true")]
    public bool holdForMousemode = false;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
        
    }
}
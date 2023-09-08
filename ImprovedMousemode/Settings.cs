using UnityModManagerNet;

namespace ImprovedMousemode;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Center mouse on entering mousemode", DrawType.Toggle)]
    public bool mouseCenteringEnabled = true;

    [Draw("Enable holding mousemode button for mousemode", DrawType.Toggle)]
    public bool holdForMousemode = true;
    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
        
    }
}
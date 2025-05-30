using DV;
using UnityModManagerNet;

namespace ImprovedMousemode;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Center mouse on entering mousemode", DrawType.Toggle)]
    public bool mouseCenteringEnabled = true;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
        
    }
}
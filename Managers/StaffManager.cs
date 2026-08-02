using GhostInTheMachine.Controllers;
using OWML.Utils;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class StaffManager : ManagerBase<StaffManager>
{
    static ItemType itemType;
    static string itemName;

    public static ItemType ItemType => itemType;
    public static string ItemName => itemName;

    GameObject prefab;

    protected override void Awake()
    {
        base.Awake();

        if (itemType == ItemType.Invalid)
        {
            itemType = EnumUtils.Create<ItemType>(nameof(NomaiStaffItem));
        }
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = GhostInTheMachine.NewHorizons.GetTranslationForUI(nameof(NomaiStaffItem));
        }

        prefab = GhostInTheMachine.CloneVanillaProp("BrittleHollow_Body/Sector_BH/Sector_OldSettlement/Fragment OldSettlement 0/Core_OldSettlement0/Props_Core_OldSettlement0/Prefab_NOM_Staff");
        prefab.AddComponent<NomaiStaffItem>();
        prefab.SetActive(false);
    }

    public void GivePlayerStaff()
    {
        var staff = Instantiate(prefab).GetComponent<NomaiStaffItem>();
        staff.gameObject.SetActive(true);
        Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(staff);
    }
}

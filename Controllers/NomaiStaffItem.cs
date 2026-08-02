using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers
{
    public class NomaiStaffItem : OWItem
    {
        string translatedName;

        public override void Awake()
        {
            base.Awake();

            _type = StaffManager.ItemType;
            translatedName = StaffManager.ItemName;
            _interactable = true;
            _interactRange = 2f;
            _localDropOffset = new Vector3(0f, -0.1f, 0f);
            _localDropNormal = Vector3.up;
        }

        public override string GetDisplayName() => translatedName;

        public override bool CheckIsDroppable() => true;

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCorePickUp);
            transform.localPosition = new Vector3(0.25f, -1.25f, 0.25f);
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            _localDropNormal = Vector3.Lerp(Vector3.up, Random.onUnitSphere, 0.1f).normalized;
            base.DropItem(position, normal, parent, sector, customDropTarget);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreDrop);
        }
    }
}

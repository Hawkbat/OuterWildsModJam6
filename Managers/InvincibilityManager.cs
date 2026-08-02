
namespace GhostInTheMachine.Managers;

public class InvincibilityManager : ManagerBase<InvincibilityManager>
{
    DeathManager deathManager;
    PlayerResources playerResources;
    ShipDamageController shipDamageController;
    int count = 0;

    protected override void Awake()
    {
        base.Awake();

        deathManager = FindObjectOfType<DeathManager>();
        playerResources = FindObjectOfType<PlayerResources>();
        shipDamageController = FindObjectOfType<ShipDamageController>();
    }

    public void PushInvincibility()
    {
        count++;
        if (count >= 1)
        {
            SetInvincible(true);
        }
    }

    public void PopInvincibility()
    {
        count--;
        if (count <= 0)
        {
            SetInvincible(false);
        }
    }

    void SetInvincible(bool invincible)
    {
        deathManager._invincible = invincible;
        //playerResources._invincible = invincible;
        shipDamageController._invincible = invincible;
    }
}

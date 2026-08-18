namespace GhostInTheMachine;

public interface IAchievements
{
    void EarnAchievement(string uniqueID);

    bool HasAchievement(string uniqueID);
}

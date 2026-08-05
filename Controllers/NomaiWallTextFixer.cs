using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiWallTextFixer : MonoBehaviour
{
    NomaiWallText textWall;

    protected void Awake()
    {
        textWall = GetComponent<NomaiWallText>();
        textWall.HideTextOnStart();
    }

    protected void Start()
    {
        textWall.HideImmediate();
        Destroy(this);
    }
}

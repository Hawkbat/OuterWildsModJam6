using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiWallTextFixer : MonoBehaviour
{
    protected void Awake()
    {
        var textWall = GetComponent<NomaiWallText>();
        textWall.HideImmediate();
        Destroy(this);
    }
}

using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class TimeLoopMaskPlatformController : MonoBehaviour
{
    TimeLoopCoreController coreController;
    MaskMonolithController[] monoliths;

    protected void Awake()
    {
        coreController = FindObjectOfType<TimeLoopCoreController>();
        monoliths = GetComponentsInChildren<MaskMonolithController>(true);
        // Disable vanilla data stream objects and prevent them from being re-enabled by the core controller
        foreach (var obj in coreController._dataStreamObjects)
        {
            obj.SetActive(false);
        }
        coreController._dataStreamObjects = [];

        // Disable vanilla monolith objects
        transform.parent.Find("Props_NOM_Monolith_group").gameObject.SetActive(false);
        transform.parent.Find("MaskMonoliths").gameObject.SetActive(false);
    }

    protected void Update()
    {
        foreach (var monolith in monoliths)
        {
            monolith.SetUploading(coreController._dataTransmitting);
        }
    }
}

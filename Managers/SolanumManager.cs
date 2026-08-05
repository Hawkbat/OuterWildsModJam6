using GhostInTheMachine.Controllers;
using OWML.Utils;
using System.IO;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class SolanumManager : ManagerBase<SolanumManager>
{
    static NomaiWord word;

    NomaiConversationManager conversationManager;
    GameObject quantumMoon;

    public static NomaiWord Word => word;

    protected override void Awake()
    {
        base.Awake();

        quantumMoon = GameObject.Find("QuantumMoon_Body");
        // TODO: This is slow (crawls all of the Quantum Moon hierarchy) but the states are inactive so regular Find won't work. May need to depend on NH DLL for SearchUtils
        conversationManager = quantumMoon.GetComponentInChildren<NomaiConversationManager>(true);

        if (word == NomaiWord.Identify)
        {
            word = EnumUtils.Create<NomaiWord>(nameof(NomaiMaskItem));
        }

        AddConversationPair(word, NomaiWord.Identify, "planets/QuantumMoon/Solanum_Identify.xml");
    }

    void AddConversationPair(NomaiWord wordA, NomaiWord wordB, string xmlPath)
    {
        var xmlText = File.ReadAllText(Path.Combine(GhostInTheMachine.Instance.ModHelper.Manifest.ModFolderPath, xmlPath));
        var wallJson = $@"{{
            ""parentPath"": ""Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot/NomaiConversation/ResponseStone/ArcSocket"",
            ""isRelativeToParent"": true,
			""position"": {{""x"": 0.5, ""y"": -0.4168, ""z"": -0.9157}}, 
			""rotation"" : {{""x"": 354.7477, ""y"": 327.7819, ""z"": 271.4651}},
            ""xmlFile"": ""{xmlPath}"",
			""seed"": {54352 + (int)wordA + (int)wordB}
		}}";
        var wallObj = GhostInTheMachine.NewHorizons.CreateNomaiText(xmlText, wallJson, quantumMoon);
        var wallText = wallObj.GetComponentInChildren<NomaiWallText>(true);
        wallText.gameObject.AddComponent<NomaiWallTextFixer>();
        ArrayHelpers.Append(ref conversationManager._questions, new NomaiConversationManager.StonePair()
        {
            wordA = wordA,
            wordB = wordB,
            response = wallText
        });
    }
}

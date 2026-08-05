using GhostInTheMachine.Controllers;
using OWML.Utils;
using System.IO;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class SolanumManager : ManagerBase<SolanumManager>
{
    static NomaiWord customWord;

    NomaiConversationManager conversationManager;
    GameObject quantumMoon;

    public static NomaiWord CustomWord
    {
        get
        {
            // Default value (0)
            if (customWord == NomaiWord.Identify)
            {
                customWord = EnumUtils.Create<NomaiWord>(nameof(NomaiMaskItem));
            }
            return customWord;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        quantumMoon = GameObject.Find("QuantumMoon_Body");
        // TODO: This is slow (crawls all of the Quantum Moon hierarchy) but the states are inactive so regular Find won't work. May need to depend on NH DLL for SearchUtils
        conversationManager = quantumMoon.GetComponentInChildren<NomaiConversationManager>(true);

        conversationManager._dialogueComplete = true;
        conversationManager._characterDialogueTree.GetInteractVolume().DisableInteraction();

        AddConversationPair(CustomWord, NomaiWord.Identify, "planets/QuantumMoon/Solanum_Identify.xml");
        AddConversationPair(CustomWord, NomaiWord.Explain, "planets/QuantumMoon/Solanum_Explain.xml");
        AddConversationPair(CustomWord, NomaiWord.Eye, "planets/QuantumMoon/Solanum_Eye.xml");
        AddConversationPair(CustomWord, NomaiWord.QuantumMoon, "planets/QuantumMoon/Solanum_QuantumMoon.xml");
        AddConversationPair(CustomWord, NomaiWord.You, "planets/QuantumMoon/Solanum_You.xml");
        AddConversationPair(CustomWord, NomaiWord.Me, "planets/QuantumMoon/Solanum_Me.xml");
        AddConversationPair(CustomWord, NomaiWord.TheNomai, "planets/QuantumMoon/Solanum_TheNomai.xml");
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

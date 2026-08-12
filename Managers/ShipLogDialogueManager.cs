using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class ShipLogDialogueManager : ManagerBase<ShipLogDialogueManager>
{
    static Sprite ghostDefaultSprite;
    static Sprite playerDefaultSprite;

    ShipLogDetectiveMode detectiveMode;

    protected override void Awake()
    {
        base.Awake();
        detectiveMode = FindObjectOfType<ShipLogDetectiveMode>();
        if (ghostDefaultSprite == null)
        {
            ghostDefaultSprite = GetEntrySprite("GITM_GHOST");
        }
        if (playerDefaultSprite == null)
        {
            playerDefaultSprite = GetEntrySprite("GITM_PLAYER");
        }
    }

    protected void Update()
    {
        var time = Time.unscaledTime;
        var scale = 1f / detectiveMode._scaleRoot.localScale.x;
        var offset = -detectiveMode._panRoot.localPosition / scale;
        Shader.SetGlobalFloat("_DataGhostUIUnscaledTime", time);
        Shader.SetGlobalFloat("_DataGhostUIEffectScale", scale);
        Shader.SetGlobalVector("_DataGhostUIEffectOffset", (Vector4)offset);
    }

    public void OnMarkCardAsRead(ShipLogEntryCard card)
    {
        var entry = card.GetEntry();
        // Ghost entries unlock their own explore facts and any follow-up rumors when marked as read
        if (entry.GetID().StartsWith("GITM_GHOST_") || entry.GetID().StartsWith("GITM_CHOICE_"))
        {
            var followupFacts = detectiveMode._manager._factDict.Values.Where(f => !f.IsRevealed() && ((f.IsRumor() && f.GetSourceID() == entry.GetID()) || (!f.IsRumor() && f.GetEntryID() == entry.GetID()))).ToList();
            foreach (var fact in followupFacts)
            {
                detectiveMode._manager.RevealFact(fact.GetID());
            }
            RestartRevealQueue(followupFacts);
            if (detectiveMode._descriptionField.IsVisible())
            {
                detectiveMode._descriptionField.SetEntry(entry);
            }
        }
    }

    public void OnInitCard(ShipLogEntryCard card)
    {
        var entry = card.GetEntry();
        if (entry.GetID().StartsWith("GITM_GHOST_") || entry.GetID().StartsWith("GITM_FIND_"))
        {
            if (entry.GetSprite() == null || entry.GetSprite().name == "DEFAULT_PHOTO")
            {
                entry.SetSprite(ghostDefaultSprite);
            }
            card._background.material = card._nameBackground.material = card._border.material = CustomAssetsManager.Instance.GhostUIMaterial;
            card._name.color = new Color(0.75f, 0.75f, 1f);
            card._photo.sprite = ghostDefaultSprite;
        }
        else if (entry.GetID().StartsWith("GITM_PLAYER_") || entry.GetID().StartsWith("GITM_CHOICE_"))
        {
            if (entry.GetSprite() == null || entry.GetSprite().name == "DEFAULT_PHOTO")
            {
                entry.SetSprite(playerDefaultSprite);
                card._photo.sprite = playerDefaultSprite;
            }
        }
    }

    void RestartRevealQueue(List<ShipLogFact> revealQueue)
    {
        if (revealQueue.Count == 0) return;
        foreach (ShipLogEntryLink link in detectiveMode._linkList)
        {
            link.UpdatePosition();
            link.UpdateVisibility();
            ShipLogEntryLink relatedLink = detectiveMode.GetLink(link.GetTargetEntryCard().GetEntry().GetID(), link.GetSourceEntryCard().GetEntry().GetID());
            if (relatedLink != null)
            {
                relatedLink.UpdateVisibility();
                if (relatedLink.IsVisible() && link.IsVisible())
                {
                    if (relatedLink.GetRevealOrder() < link.GetRevealOrder())
                    {
                        link.Hide();
                    }
                    else
                    {
                        relatedLink.Hide();
                    }
                }
            }
        }
        detectiveMode._factRevealQueue.Clear();
        detectiveMode._factRevealQueue = revealQueue;
        detectiveMode._updateRevealAnim = true;
        detectiveMode._updateFrameAll = false;
        detectiveMode._targetCard = null;
        detectiveMode._animWaitSeconds = 0.5f;
        detectiveMode._panDuration = 0.7f;
        detectiveMode._queueIndex = 0;
        detectiveMode._startScale = detectiveMode._scaleRoot.localScale;
        detectiveMode.PrepareRevealAnimations();
    }

    static Sprite GetEntrySprite(string spriteName)
    {
        var tex = GhostInTheMachine.Instance.ModHelper.Assets.GetTexture($"planets/Ghost/sprites/{spriteName}.png");
        var rect = new Rect(0, 0, tex.width, tex.height);
        var pivot = new Vector2(tex.width / 2, tex.height / 2);
        return Sprite.Create(tex, rect, pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
    }
}

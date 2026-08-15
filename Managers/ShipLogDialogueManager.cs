using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class ShipLogDialogueManager : ManagerBase<ShipLogDialogueManager>
{
    const string GHOST_PREFIX = "GITM_GHOST_";
    const string CHOICE_PREFIX = "GITM_CHOICE_";
    const string VISION_PREFIX = "GITM_VISION_";
    const string FIND_PREFIX = "GITM_FIND_";

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

    // Lami's entries and the player's replies are the conversation itself, so reading one is what carries it forward
    static bool IsConversationEntry(string entryID) => entryID.StartsWith(GHOST_PREFIX) || entryID.StartsWith(CHOICE_PREFIX);

    // Vision reviews read like Lami talking, but there's nothing for her to say until the vision has been seen
    static bool IsReadableEntry(string entryID) => IsConversationEntry(entryID) || entryID.StartsWith(VISION_PREFIX);

    // A rumor is only the reader's to unlock if nothing else already owns it: visions are earned out in the
    // world, and the act gates own the leads that shouldn't surface until the act they belong to
    static bool CanRevealOnRead(ShipLogFact fact) =>
        !fact.GetEntryID().StartsWith(VISION_PREFIX) &&
        !Constants.ShipLogFacts.GATED_RUMORS.Contains(fact.GetID());

    public void OnMarkCardAsRead(ShipLogEntryCard card)
    {
        var entry = card.GetEntry();
        // Ghost entries unlock their own explore facts and any follow-up rumors when marked as read.
        // Rumors pointing at a vision are left alone, since those are earned by finding the vision out in
        // the world; otherwise asking Lami about the statues would hand over a checklist of every one there is.
        if (IsReadableEntry(entry.GetID()))
        {
            StartCoroutine(RevealFollowupsCoroutine(card, entry));
        }
    }

    IEnumerator RevealFollowupsCoroutine(ShipLogEntryCard card, ShipLogEntry entry)
    {
        var entryID = entry.GetID();
        var alreadyRevealed = new HashSet<string>(detectiveMode._manager._factDict.Values.Where(f => f.IsRevealed()).Select(f => f.GetID()));

        var followupFacts = detectiveMode._manager._factDict.Values.Where(f => !f.IsRevealed() && ((f.IsRumor() && f.GetSourceID() == entryID && CanRevealOnRead(f)) || (!f.IsRumor() && f.GetEntryID() == entryID && !entryID.EndsWith("_CURIOSITY")))).ToList();
        foreach (var fact in followupFacts)
        {
            detectiveMode._manager.RevealFact(fact.GetID());
        }

        // Wait a frame before animating. New Horizons runs its conditional checks in LateUpdate after the
        // ShipLogUpdated event, so anything an act gate unlocks in response to what we just revealed only
        // exists after this point; without the wait it wouldn't show up until the log was closed and reopened
        yield return null;

        var revealedFacts = detectiveMode._manager._factDict.Values.Where(f => f.IsRevealed() && !alreadyRevealed.Contains(f.GetID())).ToList();
        foreach (var fact in revealedFacts)
        {
            RefreshCardName(fact.GetEntryID());
        }

        // Revealing an explore fact moves this entry from rumored to explored, and the vanilla MarkAsRead that got us here only ever marked the rumor facts, so the card would be left showing as unread
        entry.MarkAsRead();
        card.UpdateUnreadIconVisibility();

        RestartRevealQueue(revealedFacts);
        if (detectiveMode._descriptionField.IsVisible())
        {
            detectiveMode._descriptionField.SetEntry(entry);
        }
    }

    public void OnInitCard(ShipLogEntryCard card)
    {
        var entry = card.GetEntry();
        if (entry.GetID().StartsWith(GHOST_PREFIX) || entry.GetID().StartsWith(FIND_PREFIX) || entry.GetID().StartsWith(VISION_PREFIX))
        {
            if (entry.GetSprite() == null || entry.GetSprite().name == "DEFAULT_PHOTO")
            {
                entry.SetSprite(ghostDefaultSprite);
            }
            card._background.material = card._nameBackground.material = card._border.material = CustomAssetsManager.Instance.GhostUIMaterial;
            card._name.color = new Color(0.75f, 0.75f, 1f);
            card._photo.sprite = ghostDefaultSprite;
        }
        else if (entry.GetID().StartsWith("GITM_PLAYER_") || entry.GetID().StartsWith(CHOICE_PREFIX))
        {
            if (entry.GetSprite() == null || entry.GetSprite().name == "DEFAULT_PHOTO")
            {
                entry.SetSprite(playerDefaultSprite);
                card._photo.sprite = playerDefaultSprite;
            }
        }
    }

    void RefreshCardName(string entryID)
    {
        if (!detectiveMode._cardDict.TryGetValue(entryID, out var card)) return;
        card._name.text = card.GetEntry().GetName(true);
        card._name.SetAllDirty();
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

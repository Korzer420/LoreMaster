using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.UIDefs;
using LoreMaster.Manager;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LoreMaster.ItemChangerData.UIDefs;

/// <summary>
/// Shows the text in a dream dialogue textbox. This is a modified version of the Dialogue Center + LoreUIDef from ItemChanger:
/// <para />https://github.com/homothetyhk/HollowKnight.ItemChanger/blob/8bae5fd7a7e3791ec0a224bf8f334844fc945ca0/ItemChanger/Internal/DialogueCenter.cs
/// </summary>
internal class DreamLoreUIDef : MsgUIDef
{
    #region Members

    private bool _convoEnded;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the key that should be searched for, for being displayed.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the sheet the key that should be searched for.
    /// </summary>
    public string Sheet { get; set; }

    #endregion

    #region Event handler

    private void DialogueBox_HideText(On.DialogueBox.orig_HideText orig, DialogueBox self)
    {
        _convoEnded = true;
        orig(self);
    }

    #endregion

    #region Methods

    public override void SendMessage(MessageType type, Action callback)
    {
        if ((type & MessageType.Lore) == MessageType.Lore)
            LoreMaster.Instance.Handler.StartCoroutine(DisplayDreamText(callback));
        else
            base.SendMessage(type, callback);
    }

    /// <summary>
    /// Displays the dream dialogue.
    /// </summary>
    private IEnumerator DisplayDreamText(Action callback)
    {
        PlayMakerFSM.BroadcastEvent("DREAM DIALOGUE START");
        PlayMakerFSM dreamTextBox = FsmVariables.GlobalVariables.FindFsmGameObject("DialogueManager").Value.LocateMyFSM("Box Open Dream");
        GameObject dialogueText = FsmVariables.GlobalVariables.FindFsmGameObject("DialogueText").Value;
        dreamTextBox.Fsm.Event("BOX UP DREAM");
        yield return new WaitForSeconds(0.3f);

        dialogueText.LocateMyFSM("Dialogue Page Control").FsmVariables.GetFsmGameObject("Requester").Value = null;
        dialogueText.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Top;

        _convoEnded = false;
        On.DialogueBox.HideText += DialogueBox_HideText;
        StartConversation(dialogueText.GetComponent<DialogueBox>());
        yield return new WaitUntil(() => _convoEnded);
        
        On.DialogueBox.HideText -= DialogueBox_HideText;
        dreamTextBox.Fsm.Event("BOX DOWN DREAM");
        yield return new WaitForSeconds(0.3f);
        PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
        callback?.Invoke();

        dialogueText.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopLeft;
    }

    private void StartConversation(DialogueBox box)
    {
        box.currentPage = 1;
        TextMeshPro textMesh = box.GetComponent<TextMeshPro>();
        textMesh.text = Language.Language.Get(Key, Sheet);
        textMesh.ForceMeshUpdate();
        box.ShowPage(1);
    }

    public override UIDef Clone()
    {
        return new DreamLoreUIDef()
        {
            Key = Key,
            Sheet = Sheet,
            name = name.Clone(),
            shopDesc = shopDesc.Clone(),
            sprite = sprite.Clone(),
        };
    }

    #endregion
}

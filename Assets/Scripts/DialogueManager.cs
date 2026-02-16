using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public NPC currentTalkingNPC;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // สำหรับปุ่ม Skip
    public void OnSkipClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.SkipToLastLine();
    }

    // สำหรับปุ่ม Auto
    public void OnAutoClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.ToggleAutoPlay();
    }

    // สำหรับปุ่ม Continue (ปุ่มลูกศรที่ขึ้นตอนพิมพ์จบ)
    public void OnContinueClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.NextLine();
    }
}

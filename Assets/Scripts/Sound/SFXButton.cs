using UnityEngine;
using UnityEngine.UI;

public class SFXButton : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            // เมื่อคลิก จะไปเรียกเสียงจาก AudioManager ตัวกลางเสมอ
            btn.onClick.AddListener(() => {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayClickSound();
            });
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // เพิ่มตัวนี้เพื่อเปลี่ยนฉาก
using System.Collections; // เพิ่มตัวนี้เพื่อใช้ Coroutine (หน่วงเวลา)

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI moveText; 
    public TextMeshProUGUI timeText; 

    [Header("End Game Settings")]
    public GameObject splashPanel;    // ลาก LevelSplashPanel มาใส่ใน Inspector
    public TextMeshProUGUI infoText;  // ลาก Text ที่อยู่ใน Panel มาใส่
    private bool isEnding = false;    // กันไม่ให้ทำงานซ้ำซ้อน

    void Update()
    {
        // 1. โชว์จำนวนก้าว
        moveText.text = "MOVES: " + BloxorzController.moveCount;
        
        // 2. โชว์เวลา
        float t = BloxorzController.timer;
        if (t < 0) t = 0;

        string minutes = ((int)t / 60).ToString("00");
        string seconds = (t % 60).ToString("00");
        timeText.text = "TIME: " + minutes + ":" + seconds;

        timeText.color = (t <= 10f) ? Color.red : Color.white;

        // --- เพิ่มส่วนเช็คเวลาหมดตรงนี้ครับ ---
        if (t <= 0 && !isEnding)
        {
            isEnding = true;
            StartCoroutine(GameOverSequence());
        }
    }

    IEnumerator GameOverSequence()
    {
        // 1. เปิดหน้าจอคั่นด่าน
        if (splashPanel != null) splashPanel.SetActive(true);

        // 2. เปลี่ยนข้อความเป็นคำว่าแพ้
        if (infoText != null) infoText.text = "YOU LOSE! TIME UP";

        // 3. รอ 3 วินาทีให้คนอ่านก่อน
        yield return new WaitForSeconds(3f);

        // 4. เด้งกลับหน้าแรก (ต้องเช็คใน Build Settings ว่าหน้าแรกคือเลข 0)
        SceneManager.LoadScene(0);
    }
}
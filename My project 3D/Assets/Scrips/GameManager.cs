using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{

    // การเชื่อมต่อกับ UI 

    [Header("UI Panels")]
    public GameObject splashPanel;    

    [Header("Separate Text Objects")]
    public TextMeshProUGUI stageText;  
    public TextMeshProUGUI loseText;   
    public TextMeshProUGUI winText;     

    public float waitTime = 3f;       
    private static bool isRestartingFromFall = false; // ตัวแปรเช็คว่าเพิ่งตกเหวมาเพื่อข้าม Intro ด่าน


    //เริ่มต้นด่าน (Start)
    void Start()
    {
        HideAllText(); // เคลียร์ข้อความเก่าทิ้งก่อน

        // ถ้าไม่ได้เป็นการเกิดใหม่จากการตกเหว (เช่น เพิ่งเริ่มเกม หรือเพิ่งเปลี่ยนด่าน)
        if (!isRestartingFromFall)
        {
            StartCoroutine(ShowStartLevel()); // ให้โชว์ชื่อด่านก่อนเริ่ม
        }
        else
        {
            // ถ้าเป็นการตกเหวแล้วเกิดใหม่ ให้เริ่มเล่นได้เลย ไม่ต้องโชว์ชื่อด่านซ้ำ
            if (splashPanel != null) splashPanel.SetActive(false);
            isRestartingFromFall = false; 
        }
    }

    // ฟังก์ชันย่อยสำหรับซ่อนข้อความทั้งหมดบนหน้าจอ
    void HideAllText()
    {
        if (stageText != null) stageText.gameObject.SetActive(false);
        if (loseText != null) loseText.gameObject.SetActive(false);
        if (winText != null) winText.gameObject.SetActive(false);
    }

    // ส่วนที่ 3: ลำดับเหตุการณ์ (Sequences)

    // โชว์ชื่อด่านตอนเริ่ม (เช่น STAGE 1) แล้วหายไปเพื่อให้เริ่มเล่น
    IEnumerator ShowStartLevel()
    {
        if (splashPanel != null) splashPanel.SetActive(true);
        HideAllText();

        if (stageText != null) 
        {
            stageText.gameObject.SetActive(true);
            stageText.text = "S T A G E   " + SceneManager.GetActiveScene().buildIndex;
        }
        
        yield return new WaitForSeconds(waitTime); // รอ 3 วินาที
        if (splashPanel != null) splashPanel.SetActive(false); // ปิดแผ่นบังหน้าจอ
        HideAllText();
    }

    // เมื่อเวลาหมด (เรียกจาก BloxorzController)
    public void TriggerTimeOut()
    {
        isRestartingFromFall = false;
        StartCoroutine(EndSequence("lose")); // เข้าสู่ช่วงจบด่านแบบแพ้
    }

    // เมื่อชนะด่าน (เรียกจาก BloxorzController)
    public void WinLevel()
    {
        isRestartingFromFall = false;
        StartCoroutine(NextLevelSequence()); // เข้าสู่ช่วงไปด่านถัดไป
    }

    // เมื่อตกเหว (เรียกจาก BloxorzController)
    public void RestartDueToFall()
    {
        isRestartingFromFall = true; // ตั้งค่าไว้ว่าเพิ่งตกเหวมา
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // โหลดด่านเดิมใหม่ทันที
    }

    // ลำดับการไปด่านถัดไป
    IEnumerator NextLevelSequence()
    {
        if (splashPanel != null) splashPanel.SetActive(true);
        HideAllText();

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // เช็คว่ามีด่านถัดไปใน Build Settings ไหม
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (stageText != null) 
            {
                stageText.gameObject.SetActive(true);
                stageText.text = "S T A G E   " + nextSceneIndex;
            }
            yield return new WaitForSeconds(waitTime);
            SceneManager.LoadScene(nextSceneIndex); // เปลี่ยนด่าน
        }
        else
        {
            // ถ้าไม่มีด่านถัดไปแล้ว แปลว่าจบเกม(ชนะทั้งหมด)
            yield return StartCoroutine(EndSequence("win"));
        }
    }

    // ลำดับตอนจบเกม(ทั้งแพ้และชนะ)
    IEnumerator EndSequence(string type)
    {
        if (splashPanel != null) splashPanel.SetActive(true);
        HideAllText();

        // เลือกข้อความให้ตรงกับสถานะ (แพ้หรือชนะ)
        if (type == "lose" && loseText != null) loseText.gameObject.SetActive(true);
        if (type == "win" && winText != null) winText.gameObject.SetActive(true);

        yield return new WaitForSeconds(waitTime);

        // รีเซ็ตค่าสถิติก่อนกลับไปหน้าแรกของเกม
        BloxorzController.timer = 0;      
        BloxorzController.moveCount = 0;  

        SceneManager.LoadScene(0); // กลับหน้า Main Menu
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีเพื่อใช้เปลี่ยนด่าน

public class MainMenu : MonoBehaviour
{
    // ฟังก์ชันสำหรับกดปุ่ม Start
    public void PlayGame()
    {
        // คำสั่งให้เปลี่ยนไปด่านถัดไป (ด่านที่ 1 ใน Build Settings)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ฟังก์ชันสำหรับกดปุ่ม Quit
    public void QuitGame()
    {
        Debug.Log("Game Quit!"); // โชว์ข้อความในหน้า Console ว่ากดออกแล้ว
        Application.Quit();     // คำสั่งปิดโปรแกรมเกม
    }
}
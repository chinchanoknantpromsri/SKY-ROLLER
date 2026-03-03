using UnityEngine;

public class Goal : MonoBehaviour
{
    // ฟังก์ชันนี้ทำงานเมื่อตัวบล็อก(Player)เคลื่อนที่เข้ามาสัมผัสพื้นที่ของหลุม
    void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบTagของวัตถุที่เข้ามาชนว่าเป็นPlayer
        if (other.CompareTag("Player"))
        {
            // แสดงข้อความยืนยันในระบบว่าผู้เล่นเข้าสู่จุดหมายสำเร็จ
            // (ใช้ทำงานร่วมกับสคริปต์หลักเพื่อยืนยันพิกัดการชนะ)
            Debug.Log("Reached Finish Line!");
        }
    }
}
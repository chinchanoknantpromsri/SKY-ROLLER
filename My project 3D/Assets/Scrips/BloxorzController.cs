using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BloxorzController : MonoBehaviour
{

    //การตั้งค่าพื้นฐาน
    [Header("Movement Settings")]
    public float rollSpeed = 600f;  // ความเร็วในการหมุนบล็อก
    public float gravity = 45f;     // แรงโน้มถ่วงเวลาบล็อกตก

    [Header("Game Stats")]
    public float timeLimit = 60f;           // เวลาจำกัดในแต่ละด่าน
    public static int moveCount = 0;        // ตัวแปร Static เก็บจำนวนก้าว (แชร์ข้อมูลข้าม Scene ได้)
    public static float timer;              // ตัวแปร Static เก็บเวลาที่เหลือ
    
    private static int lastSceneIndex = -1; // จำ Index ของด่านล่าสุดเพื่อเช็คการ Reset เวลา
    private bool isGameOver = false;        // สถานะแพ้/ชนะ
    private bool isMoving = false;          // เช็คว่าบล็อกกำลังหมุนอยู่ไหม (ป้องกันกดปุ่มซ้อน)
    private bool isStarting = true;         // ช่วงเริ่มด่าน (ใช้ทำ Intro บล็อกหล่นลงมา)
    private Vector3 lastMoveDir;            // เก็บทางล่าสุดที่เดิน เพื่อใช้คำนวณตอนตก

    [Header("Audio Settings")]
    public AudioSource playerAudio; // ตัวปล่อยเสียง
    public AudioClip moveSFX;       // เสียงตอนกลิ้ง
    public AudioClip fallSFX;       // เสียงตอนตก
    public AudioClip winSFX;        // เสียงตอนลงหลุม


    // ส่วนที่ 2: ฟังก์ชันเริ่มเกม (Start)
    void Start()
    {
        // ระบบจัดการเวลา: ถ้าเข้าด่านใหม่หรือเริ่มใหม่ ให้รีเซ็ตเวลา
        if (lastSceneIndex != SceneManager.GetActiveScene().buildIndex || timer <= 0)
        {
            timer = timeLimit; 
            lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        }

        // ทำให้บล็อกหายไปก่อนเพื่อทำ Intro บล็อกขยายขนาดขึ้นมา
        transform.localScale = Vector3.zero;
        StartCoroutine(LevelStartIntro());
    }


    // ส่วนที่ 3: ฟังก์ชันอัปเดต
    void Update()
    {
        if (isStarting || isGameOver) return; // ถ้าเริ่มด่านหรือจบเกมแล้ว ห้ามควบคุม

        timer -= Time.deltaTime; // นับถอยหลังเวลาตามวินาทีจริง

        // ถ้าเวลาหมด
        if (timer <= 0)
        {
            timer = 0;
            isGameOver = true;
            lastSceneIndex = -1; 

            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.TriggerTimeOut(); // เรียกฟังก์ชันเวลาหมดใน GameManager
            else SceneManager.LoadScene(0);
        }

        if (isMoving) return; // ถ้ากำลังกลิ้งอยู่ ห้ามกดเดินเพิ่ม

        // ตรวจจับปุ่มลูกศร 4 ทิศทาง เพื่อเริ่มการกลิ้ง (Move)
        if (Input.GetKeyDown(KeyCode.UpArrow)) StartCoroutine(Move(Vector3.forward));
        else if (Input.GetKeyDown(KeyCode.DownArrow)) StartCoroutine(Move(Vector3.back));
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) StartCoroutine(Move(Vector3.left));
        else if (Input.GetKeyDown(KeyCode.RightArrow)) StartCoroutine(Move(Vector3.right));
    }


    // ส่วนที่ 4: ฟังก์ชันการกลิ้งบล็อก (Move Algorithm)
    IEnumerator Move(Vector3 dir)
    {
        isMoving = true;
        moveCount++; 
        lastMoveDir = dir;

        // เล่นเสียงกลิ้ง
        if (playerAudio != null && moveSFX != null) {
            playerAudio.PlayOneShot(moveSFX);
        }

        // คำนวณหาจุดหมุน และ ระยะตามลักษณะของบล็อก (ตั้งหรือนอน)
        bool wasStanding = transform.position.y > 0.8f;
        float distToEdge = wasStanding ? 0.5f : (Mathf.Abs(Vector3.Dot(dir, transform.up)) > 0.1f ? 1.0f : 0.5f);
        Vector3 anchor = transform.position + (dir * distToEdge);
        anchor.y = 0f; 

        // คำนวณแกนการหมุน (Axis)
        Vector3 axis = Vector3.Cross(Vector3.up, dir);
        float totalAngle = 0;

        // ลูปค่อยๆหมุนบล็อกให้ครบ 90 องศา เพื่อให้เห็นภาพการพลิกที่สวย
        while (totalAngle < 90)
        {
            float angle = rollSpeed * Time.deltaTime;
            if (totalAngle + angle > 90) angle = 90 - totalAngle;
            transform.RotateAround(anchor, axis, angle);
            totalAngle += angle;
            yield return null;
        }

        // เมื่อกลิ้งเสร็จ เช็คว่าตกสนามไหม
        if (CheckIfFalling()) yield break;
        
        // จัดระเบียบพิกัดบล็อกให้ลงล็อคตาราง
        SnapToGrid();
        isMoving = false;
    }

    // ส่วนที่ 5: ฟังก์ชันเมื่อลงหลุม 

    IEnumerator WinSequence(Vector3 holePos)
    {
        isGameOver = true; 
        isMoving = true;

        if (playerAudio != null && winSFX != null) {
            playerAudio.PlayOneShot(winSFX);
        }

        // ล็อคตัวให้อยู่กลางหลุม และทำอนิเมชั่นตัวหดหล่นลงหลุม
        Vector3 targetPos = new Vector3(holePos.x, transform.position.y, holePos.z);
        transform.position = targetPos;
        
        float elapsed = 0;
        while (elapsed < 0.4f)
        {
            float t = elapsed / 0.4f;
            transform.position += Vector3.down * 5f * Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        FindObjectOfType<GameManager>().WinLevel(); // เรียกฟังก์ชันชนะใน GameManager
    }

    void RestartLevel() 
    { 
        FindObjectOfType<GameManager>().RestartDueToFall();
    }

    // อนิเมชั่นตอนเริ่มเกม: บล็อกหล่นจากฟ้าลงมาที่พื้น
    IEnumerator LevelStartIntro() {
        isStarting = true;
        Vector3 landPos = transform.position;
        Vector3 startPos = landPos + Vector3.up * 10f; 
        float elapsed = 0;
        while (elapsed < 0.6f) {
            float t = elapsed / 0.6f;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            transform.position = Vector3.Lerp(startPos, landPos, t * t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = landPos;
        isStarting = false; 
    }

    // ส่วนที่ 6: ฟังก์ชันตรวจสอบการตกสนาม 
    bool CheckIfFalling() {
        bool isStanding = Mathf.Abs(transform.up.y) > 0.8f;
        Vector3 pos = transform.position;

        // กรณีที่ 1: บล็อกตั้งอยู่
        if (isStanding) {
            Vector3 checkPos = pos + Vector3.down * 0.9f;
            // สร้างวงกลมจำลองเช็คฟิสิกส์ว่ามีพื้น (Tile) หรือหลุม (Goal) อยู่ข้างล่างไหม
            Collider[] hitColliders = Physics.OverlapSphere(checkPos, 0.3f);
            foreach (var col in hitColliders) {
                if (col.CompareTag("Tile") || col.CompareTag("Goal")) {
                    // ถ้าเจอหลุมในท่าตั้ง -> ชนะด่าน
                    if (col.CompareTag("Goal")) { StartCoroutine(WinSequence(col.transform.position)); return true; }
                    return false; // เจอพื้นปกติ ไม่ตก
                }
            }
            // ถ้าไม่เจออะไรเลย -> เริ่มการเอียงตกสนาม
            StartCoroutine(FallTilt(pos + (lastMoveDir * 0.5f), lastMoveDir)); return true;
        } 
        // กรณีที่ 2: บล็อกนอนอยู่
        else {
            Vector3 offset = (Mathf.Abs(transform.up.x) > 0.1f) ? Vector3.right * 0.5f : Vector3.forward * 0.5f;
            // เช็คพื้นทั้ง 2 จุด (หัว-ท้ายของบล็อก)
            bool g1 = CheckGroundAtPoint(pos + offset); 
            bool g2 = CheckGroundAtPoint(pos - offset);

            if (!g1 && !g2) { // ถ้าไม่มีพื้นทั้งสองข้าง -> ตกตรงๆ
                StartCoroutine(FallTilt(pos + (lastMoveDir * 0.5f), lastMoveDir)); return true; 
            }
            else if (!g1 || !g2) { // ถ้ามีพื้นข้างเดียว -> เอียงร่วง
                Vector3 safeSide = g1 ? (pos + offset) : (pos - offset);
                StartCoroutine(FallTilt(safeSide + (lastMoveDir * 0.5f), !g1 ? offset : -offset)); return true;
            }
            return false;
        }
    }

    // ฟังก์ชันย่อย: ใช้ยิงวงกลมจำลองเพื่อเช็คว่าจุดนั้นมีพื้นไหม
    bool CheckGroundAtPoint(Vector3 point) {
        Collider[] cols = Physics.OverlapSphere(point + Vector3.down * 0.4f, 0.2f);
        foreach (var c in cols) { if (c.CompareTag("Tile") || c.CompareTag("Goal")) return true; }
        return false;
    }

    // ฟังก์ชันจัดตำแหน่งบล็อก: ปรับตำแหน่ง/องศา ให้ลงล็อคตารางเสมอ (ป้องกันเลขทศนิยมเบี้ยว)
    void SnapToGrid() {
        bool isStandingNow = Mathf.Abs(transform.up.y) > 0.8f;
        float x = Mathf.Round(transform.position.x * 2) / 2f;
        float z = Mathf.Round(transform.position.z * 2) / 2f;
        if (isStandingNow) { x = Mathf.Round(x); z = Mathf.Round(z); }
        transform.position = new Vector3(x, isStandingNow ? 1.0f : 0.5f, z);
        transform.rotation = Quaternion.Euler(Mathf.Round(transform.eulerAngles.x/90)*90, Mathf.Round(transform.eulerAngles.y/90)*90, Mathf.Round(transform.eulerAngles.z/90)*90);
    }

    // ส่วนที่ 7: ฟังก์ชันตอนตกเหว (Falling Animation)
    IEnumerator FallTilt(Vector3 pivot, Vector3 fallDir) {
        isMoving = true; float rotateVelocity = 0f; float currentAngle = 0f;
        Vector3 axis = Vector3.Cross(Vector3.up, fallDir);

        if (playerAudio != null && fallSFX != null) {
            playerAudio.PlayOneShot(fallSFX);
        }

        // หมุนบล็อกให้เอียงร่วงขอบสนาม 90 องศา
        while (currentAngle < 90f) {
            rotateVelocity += gravity * 12f * Time.deltaTime; 
            float step = rotateVelocity * Time.deltaTime;
            if (currentAngle + step > 90f) step = 90f - currentAngle;
            transform.RotateAround(new Vector3(pivot.x, 0, pivot.z), axis, step);
            currentAngle += step;
            yield return null;
        }
        // เมื่อเอียงจนสุดแล้ว ให้เริ่มตกลงไปในอากาศจริงๆ
        StartCoroutine(FreeFall(axis, rotateVelocity));
    }

    // ฟังก์ชันร่วงหล่นอิสระ: กลิ้งหมุนตกลงเหวไปเรื่อยๆ จนพ้นระยะที่กำหนด
    IEnumerator FreeFall(Vector3 rotAxis, float initialRotVel) {
        float vVel = 0f;
        while (transform.position.y > -20f) {
            vVel += gravity * 3f * Time.deltaTime;
            transform.position += Vector3.down * vVel * Time.deltaTime;
            transform.Rotate(rotAxis, initialRotVel * Time.deltaTime);
            yield return null;
        }
        RestartLevel(); // เริ่มด่านใหม่
    }
}
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraResizer : MonoBehaviour
{
    [Header("Target Resolution")]
    [Tooltip("Oyunun tasarlandığı referans genişlik.")]
    public float targetWidth = 1080f;
    [Tooltip("Oyunun tasarlandığı referans yükseklik.")]
    public float targetHeight = 1920f;

    private Camera cam;
    private float initialOrthographicSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
        initialOrthographicSize = cam.orthographicSize;
        AdjustCamera();
    }

    void AdjustCamera()
    {
        // Hedef ekran oranı (1080 / 1920 = 0.5625)
        float targetAspect = targetWidth / targetHeight;

        // Mevcut cihazın ekran oranı
        float currentAspect = (float)Screen.width / Screen.height;

        // Eğer mevcut cihazın ekranı, hedefimizden daha ince/uzun ise (örn: 1080x2400, 20:9 formatı)
        if (currentAspect < targetAspect)
        {
            // Ortografik kameranın boyutunu (yüksekliğini) büyüterek yanların (sağ-sol duvarların)
            // ekrana sığmasını ve dışarıda kalmamasını (kırpılmamasını) sağlıyoruz.
            float differenceInSize = targetAspect / currentAspect;
            cam.orthographicSize = initialOrthographicSize * differenceInSize;
        }
        else
        {
            // Eğer cihaz daha geniş bir ekrana sahipse (örn: iPad)
            // Kamera varsayılan dikey boyutunu korur, oyun sahasının altı veya üstü kesilmez, sadece yanlarda biraz daha fazla boşluk görünür.
            cam.orthographicSize = initialOrthographicSize;
        }
    }

#if UNITY_EDITOR
    // Unity Editör'de "Game" ekranının boyutunu değiştirirsek kameranın canlı olarak tepki vermesi için:
    void Update()
    {
        if (!Application.isPlaying)
        {
            AdjustCamera();
        }
    }
#endif
}

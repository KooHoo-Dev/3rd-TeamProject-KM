using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleVSIntroFixedScale : MonoBehaviour
{
    [Header("1. 순서대로 등장시킬 단어들 (GET, READY, BATTLE 등)")]
    public GameObject[] wordObjects;

    [Header("2. 단어 등장 연출 세팅")]
    [Tooltip("단어가 나타날 때의 연출 속도 (초)")]
    public float scaleDuration = 0.15f; 
    [Tooltip("원본 크기 대비 튀어나오는 배율 (1.1 = 원본의 110% 크기로 튕김)")]
    public float punchScaleAmount = 1.15f; 

    [Header("3. 카메라 & 이펙트 세팅")]
    public Camera mainCamera;
    public float shakeDuration = 0.4f;
    public float shakeMagnitude = 0.3f;
    public ParticleSystem finalVFX;
    public AudioSource audioSource;
    public AudioClip hitSFX;
    public AudioClip finalExplosionSFX;

    private Vector3 originalCamPos;
    // 글자들의 원래 Scale을 저장할 리스트
    private List<Vector3> originalWordScales = new List<Vector3>();

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) originalCamPos = mainCamera.transform.localPosition;

        // 각 단어의 원래 Scale 저장 및 초기화
        for (int i = 0; i < wordObjects.Length; i++)
        {
            if (wordObjects[i] != null)
            {
                // 인스펙터에 설정된 원본 크기 저장
                originalWordScales.Add(wordObjects[i].transform.localScale);
                
                // 연출 전에는 활성화하고 크기만 0으로 세팅
                wordObjects[i].SetActive(true);
                wordObjects[i].transform.localScale = Vector3.zero;
            }
            else
            {
                originalWordScales.Add(Vector3.one);
            }
        }

        StartCoroutine(PlayEnhancedSequence());
    }

    private IEnumerator PlayEnhancedSequence()
    {
        // 1단계: 단어를 하나씩 원본 크기 기준으로 Scale 연출
        for (int i = 0; i < wordObjects.Length; i++)
        {
            GameObject word = wordObjects[i];
            Vector3 targetOriginalScale = originalWordScales[i]; // 저장해둔 원본 크기

            if (word != null)
            {
                // 원본 크기를 전달하여 Punch 연출 실행
                StartCoroutine(CoPunchScale(word.transform, targetOriginalScale));

                if (audioSource != null && hitSFX != null)
                    audioSource.PlayOneShot(hitSFX);

                StartCoroutine(CoCameraShake(0.1f, 0.1f));

                yield return new WaitForSeconds(0.35f);
            }
        }

        // 2단계: 피날레
        if (finalVFX != null) finalVFX.Play();
        if (audioSource != null && finalExplosionSFX != null)
            audioSource.PlayOneShot(finalExplosionSFX);

        yield return StartCoroutine(CoCameraShake(shakeDuration, shakeMagnitude));
    }

    // --- 원본 Scale 값을 받아 적용하도록 수정된 연출 로직 ---
    private IEnumerator CoPunchScale(Transform targetTransform, Vector3 originalScale)
    {
        float elapsed = 0.0f;
        Vector3 maxScale = originalScale * punchScaleAmount; // 원본 크기 * 배율

        // 1. 0에서 원본보다 살짝 큰 크기(maxScale)로 확대
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / scaleDuration;
            targetTransform.localScale = Vector3.Lerp(Vector3.zero, maxScale, percent);
            yield return null;
        }

        // 2. maxScale에서 원래 지정해둔 크기(originalScale)로 정착
        elapsed = 0.0f;
        float returnDuration = scaleDuration * 0.5f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / returnDuration;
            targetTransform.localScale = Vector3.Lerp(maxScale, originalScale, percent);
            yield return null;
        }

        // 원래 크기로 완전 고정
        targetTransform.localScale = originalScale;
    }

    private IEnumerator CoCameraShake(float duration, float magnitude)
    {
        if (mainCamera == null) yield break;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = originalCamPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
    }
}
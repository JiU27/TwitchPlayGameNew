using UnityEngine;
using System.Collections;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmount = 0.7f;
    [SerializeField] private float decreaseFactor = 1.0f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private float rotationDuration = 0.2f;
    [SerializeField] private GameObject bloodParticlePrefab;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;
        originalPos = mainCamera.transform.localPosition;
        originalRot = mainCamera.transform.localRotation;
    }

    public void PlayPlayerDamageEffects(Vector3 position)
    {
        StartCoroutine(Shake());
        SpawnBloodParticles(position);
    }

    public void PlayEnemyDamageEffects(Vector3 position, bool playerFacingRight)
    {
        StartCoroutine(RotateScreen(playerFacingRight));
        SpawnBloodParticles(position);
    }

    private IEnumerator Shake()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            mainCamera.transform.localPosition = originalPos + randomOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    private IEnumerator RotateScreen(bool facingRight)
    {
        float elapsed = 0.0f;
        float startRotation = mainCamera.transform.localEulerAngles.z;
        float endRotation = facingRight ? rotationAmount : -rotationAmount;

        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;
            float currentRotation = Mathf.Lerp(startRotation, endRotation, t);
            mainCamera.transform.localRotation = Quaternion.Euler(0, 0, currentRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0.0f;
        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;
            float currentRotation = Mathf.Lerp(endRotation, 0, t);
            mainCamera.transform.localRotation = Quaternion.Euler(0, 0, currentRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localRotation = originalRot;
    }

    private void SpawnBloodParticles(Vector3 position)
    {
        if (bloodParticlePrefab != null)
        {
            GameObject particleObject = Instantiate(bloodParticlePrefab, position, Quaternion.identity);
            ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleObject, duration);
            }
            else
            {
                Destroy(particleObject, 2f); // 默认2秒后销毁，如果没有ParticleSystem组件
            }
        }
    }
}
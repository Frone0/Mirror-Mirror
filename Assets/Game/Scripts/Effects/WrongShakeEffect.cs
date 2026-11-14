using UnityEngine;
using System.Collections;
public class WrongShakeEffect : MonoBehaviour
{
    public Transform ShakeObject;
    public Ui_Effects_Control UIControler;

    public float ShakeTime = 0.25f;
    public float ShakeStrength = 0.08f;

    Vector3 startPosition;
    Coroutine shakeRoutine;

    void Start()
    {
        startPosition = ShakeObject.localPosition;
    }

   void OnEnable()  {UIControler.GameManager.GetDamageEvent += Shake;}
   void OnDisable() {UIControler.GameManager.GetDamageEvent -= Shake;}

    void Shake()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeEffect());
    }

    IEnumerator ShakeEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < ShakeTime)
        {
            elapsedTime += Time.deltaTime;
            float strength = ShakeStrength * (1f - elapsedTime / ShakeTime); // gets weaker at the end

            ShakeObject.localPosition = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        ShakeObject.localPosition = startPosition;
        shakeRoutine = null;
    }
}

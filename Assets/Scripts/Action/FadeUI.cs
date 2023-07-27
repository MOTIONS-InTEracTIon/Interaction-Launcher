using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class FadeUI : MonoBehaviour
{
    public bool fadeOnStart = true;
    public bool fadeIn = true;
    public float fadeDuration = 2.0f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        if (fadeOnStart)
        {
            if(fadeIn)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                StartCoroutine(FadeOut());
            }

        }
    }

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(FadeRoutine(0, 1));
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(FadeRoutine(1, 0));
    }

    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        if(canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (alphaOut == 1)
        {
            canvasGroup.blocksRaycasts = true;
        }

        float timer = 0;

        while (timer <= fadeDuration)
        {
            float newAlpha = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            canvasGroup.alpha = newAlpha;

            timer += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = alphaOut;

        if (alphaOut == 0)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }
}


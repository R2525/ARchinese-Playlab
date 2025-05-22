using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiFaceController : MonoBehaviour
{
    public List<SkinnedMeshRenderer> Renderers; 

    public int MouthIndex;
    public int RightEyeTopIndex;
    public int RightEyeBottomIndex;
    public int LeftEyeTopIndex;
    public int LeftEyeBottomIndex;

    float MouthMaxWeight = 30f;
    public float MouthMaxWeightMin = 30f;
    public float MouthMaxWeightMax = 30f;
    public float EyeTopMaxWeight = 70f;
    public float EyeBottomMaxWeight = 40f;

    float blinkInterval = 20f;
    float blinkTimer = 0f;
    float mouthDt = 0f;
    float talkDt = 0f;
    float blinkDt = 0f;
    public float BlinkIntervalMin = 15f;
    public float BlinkIntervalMax = 45f;
    public float TalkLength = 3f;
    public float MouthOpenLength = 0.25f;
    public float BlinkLength = 0.25f;

    bool eyeClosing = true;
    bool mouthClosing = true;
    bool isTalking = false;
    bool isBlinking = false;

    void Start()
    {
        if (Renderers == null || Renderers.Count == 0)
        {
            Debug.LogError("No SkinnedMeshRenderer assigned to Renderers list.");
            return;
        }

        // Initialize random values
        changeBlinkInterval();
        changeMouthMaxOpenWeight();
    }

    void Update()
    {
        blinkTimer += Time.deltaTime;

        // Blink at the next interval
        if (blinkTimer > blinkInterval)
        {
            ToggleBlinking();
            blinkTimer = 0f;
        }

        talkUpdate(); // Mouth controls
        blinkUpdate(); // Eye controls

        // Test talk button
        
    }

    public void ToggleTalk()
    {
        isTalking = !isTalking;
        mouthClosing = true;

        // Reset mouth position for all models
        foreach (var renderer in Renderers)
        {
            if (renderer != null)
                renderer.SetBlendShapeWeight(MouthIndex, 0f);
        }
    }

    public void ToggleBlinking()
    {
        if (!isBlinking)
        {
            changeBlinkInterval();
        }

        // Reset eye position for all models
        foreach (var renderer in Renderers)
        {
            if (renderer != null)
            {
                renderer.SetBlendShapeWeight(RightEyeTopIndex, 0f);
                renderer.SetBlendShapeWeight(RightEyeBottomIndex, 0f);
                renderer.SetBlendShapeWeight(LeftEyeTopIndex, 0f);
                renderer.SetBlendShapeWeight(LeftEyeBottomIndex, 0f);
            }
        }

        isBlinking = !isBlinking;
    }

    void talkUpdate()
    {
        if (isTalking)
        {
            talkDt += Time.deltaTime;
            mouthDt += Time.deltaTime;

            foreach (var renderer in Renderers)
            {
                if (renderer != null)
                {
                    renderer.SetBlendShapeWeight(MouthIndex, mouthClosing ? Mathf.Lerp(0f, MouthMaxWeight, mouthDt / MouthOpenLength) :
                                                                            Mathf.Lerp(MouthMaxWeight, 0f, mouthDt / MouthOpenLength));
                }
            }

            if (mouthDt > MouthOpenLength)
            {
                mouthDt = 0f;
                mouthClosing = !mouthClosing;

                if (mouthClosing)
                {
                    changeMouthMaxOpenWeight();
                }
            }

            if (talkDt > TalkLength)
            {
                ToggleTalk();
                talkDt = 0f;
                mouthDt = 0f;
            }
        }
    }

    void blinkUpdate()
    {
        if (isBlinking)
        {
            blinkDt += Time.deltaTime;

            foreach (var renderer in Renderers)
            {
                if (renderer != null)
                {
                    renderer.SetBlendShapeWeight(RightEyeTopIndex, eyeClosing ? Mathf.Lerp(0f, EyeTopMaxWeight, blinkDt / BlinkLength) :
                                                                                Mathf.Lerp(EyeTopMaxWeight, 0f, blinkDt / BlinkLength));
                    renderer.SetBlendShapeWeight(RightEyeBottomIndex, eyeClosing ? Mathf.Lerp(0f, EyeBottomMaxWeight, blinkDt / BlinkLength) :
                                                                                   Mathf.Lerp(EyeBottomMaxWeight, 0f, blinkDt / BlinkLength));
                    renderer.SetBlendShapeWeight(LeftEyeTopIndex, eyeClosing ? Mathf.Lerp(0f, EyeTopMaxWeight, blinkDt / BlinkLength) :
                                                                               Mathf.Lerp(EyeTopMaxWeight, 0f, blinkDt / BlinkLength));
                    renderer.SetBlendShapeWeight(LeftEyeBottomIndex, eyeClosing ? Mathf.Lerp(0f, EyeBottomMaxWeight, blinkDt / BlinkLength) :
                                                                                 Mathf.Lerp(EyeBottomMaxWeight, 0f, blinkDt / BlinkLength));
                }
            }

            if (blinkDt > BlinkLength)
            {
                if (eyeClosing)
                {
                    eyeClosing = false;
                    blinkDt = 0f;
                }
                else
                {
                    blinkDt = 0f;
                    ToggleBlinking();
                    eyeClosing = true;
                }
            }
        }
    }

    void changeBlinkInterval()
    {
        blinkInterval = Random.Range(BlinkIntervalMin, BlinkIntervalMax);
    }

    void changeMouthMaxOpenWeight()
    {
        MouthMaxWeight = Random.Range(MouthMaxWeightMin, MouthMaxWeightMax);
    }
}

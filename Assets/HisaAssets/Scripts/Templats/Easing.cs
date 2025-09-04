using System;
using Unity.VisualScripting;
using UnityEngine;

public class Easing
{
    //public static float Liner(float min, float max, float t)
    //{
    //    return (1.0f - t) * min + max * t;
    //}

    //public static Vector2 Liner(Vector2 min, Vector2 max, float t)
    //{
    //    return (1.0f - t) * min + max * t;
    //}
    //public static Vector3 Liner(Vector3 min, Vector3 max, float t)
    //{
    //    return (1.0f - t) * min + max * t;
    //}

    private static T Lerp<T>(float t, float totaltime, T min, T max, System.Func<float, float> easingFunc)
    {
        if (t <= 0.0f) return min;
        if (t >= totaltime) return max;

        t /= totaltime;
        float easeValue = easingFunc(t);

        if (typeof(T) == typeof(float))
            return (T)(object)((float)(object)min + ((float)(object)max - (float)(object)min) * easeValue);
        if (typeof(T) == typeof(Vector2))
            return (T)(object)Vector2.LerpUnclamped((Vector2)(object)min, (Vector2)(object)max, easeValue);
        if (typeof(T) == typeof(Vector3))
            return (T)(object)Vector3.LerpUnclamped((Vector3)(object)min, (Vector3)(object)max, easeValue);

        throw new System.InvalidOperationException("Unsupported type for easing function.");
    }

    private static float LinerFunc(float t) => t;
    public static T Liner<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, LinerFunc);


    public static T InSine<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InSineFunc);
    public static T OutSine<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutSineFunc);
    public static T InOutSine<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutSineFunc);


    private static float InSineFunc(float t) => 1.0f - Mathf.Cos(t * Mathf.PI * 0.5f);
    private static float OutSineFunc(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
    private static float InOutSineFunc(float t) => 0.5f * (1.0f - Mathf.Cos(t * Mathf.PI));




    public static T InQuad<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InQuadFunc);
    public static T OutQuad<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutQuadFunc);
    public static T InOutQuad<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutQuadFunc);

    private static float InQuadFunc(float t) => t * t;
    private static float OutQuadFunc(float t) => -t * (t - 2.0f);
    private static float InOutQuadFunc(float t)
    {
        t *= 2.0f;
        return (t < 1.0f) ? 0.5f * t * t : -0.5f * ((t - 1.0f) * ((t - 1.0f) - 2.0f) - 1.0f);
    }

    public static T InCubic<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InCubicFunc);
    public static T OutCubic<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutCubicFunc);
    public static T InOutCubic<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutCubicFunc);

    private static float InCubicFunc(float t) => t * t * t;
    private static float OutCubicFunc(float t)
    {
        t -= 1.0f;
        return t * t * t + 1.0f;
    }
    private static float InOutCubicFunc(float t)
    {
        t *= 2.0f;
        if (t < 1.0f) return 0.5f * t * t * t;
        t -= 2.0f;
        return 0.5f * (t * t * t + 2.0f);
    }

    public static T InQuart<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InQuartFunc);
    public static T OutQuart<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutQuartFunc);
    public static T InOutQuart<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutQuartFunc);

    private static float InQuartFunc(float t) => t * t * t * t;
    private static float OutQuartFunc(float t)
    {
        t -= 1.0f;
        return 1.0f - t * t * t * t;
    }
    private static float InOutQuartFunc(float t)
    {
        t *= 2.0f;
        if (t < 1.0f) return 0.5f * t * t * t * t;
        t -= 2.0f;
        return -0.5f * (t * t * t * t - 2.0f);
    }

    public static T InQuint<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InQuintFunc);
    public static T OutQuint<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutQuintFunc);
    public static T InOutQuint<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutQuintFunc);

    private static float InQuintFunc(float t) => t * t * t * t * t;
    private static float OutQuintFunc(float t)
    {
        t -= 1.0f;
        return 1.0f + t * t * t * t * t;
    }
    private static float InOutQuintFunc(float t)
    {
        t *= 2.0f;
        if (t < 1.0f) return 0.5f * t * t * t * t * t;
        t -= 2.0f;
        return 0.5f * (t * t * t * t * t + 2.0f);
    }

    public static T InExpo<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InExpoFunc);
    public static T OutExpo<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutExpoFunc);
    public static T InOutExpo<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutExpoFunc);

    private static float InExpoFunc(float t) => Mathf.Pow(2.0f, 10.0f * (t - 1.0f));
    private static float OutExpoFunc(float t) => -Mathf.Pow(2.0f, -10.0f * t) + 1.0f;
    private static float InOutExpoFunc(float t)
    {
        t *= 2.0f;
        if (t < 1.0f)
            return 0.5f * Mathf.Pow(2.0f, 10.0f * (t - 1.0f));
        else
        {
            t -= 1.0f;
            return 0.5f * (-Mathf.Pow(2.0f, -10.0f * t) + 2.0f);
        }
    }

    public static T InCirc<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InCircFunc);
    public static T OutCirc<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutCircFunc);
    public static T InOutCirc<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutCircFunc);

    private static float InCircFunc(float t) => 1.0f - Mathf.Sqrt(1.0f - t * t);
    private static float OutCircFunc(float t) => Mathf.Sqrt(1.0f - t * t);
    private static float InOutCircFunc(float t)
    {
        t *= 2.0f;
        if (t < 1.0f)
            return 0.5f * (1.0f - Mathf.Sqrt(1.0f - t * t));
        else
        {
            t -= 2.0f;
            return 0.5f * (Mathf.Sqrt(1.0f - t * t) + 1.0f);
        }
    }
    public static T InBack<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InBackFunc);
    public static T OutBack<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, OutBackFunc);
    public static T InOutBack<T>(float t, float totaltime, T min, T max) => Lerp(t, totaltime, min, max, InOutBackFunc);

    private static float InBackFunc(float t)
    {
        float s = 1.70158f;
        return t * t * ((s + 1.0f) * t - s);
    }

    private static float OutBackFunc(float t)
    {
        float s = 1.70158f;
        t -= 1.0f;
        return 1.0f + t * t * ((s + 1.0f) * t + s);
    }

    private static float InOutBackFunc(float t)
    {
        float s = 1.70158f;
        t *= 2.0f;
        if (t < 1.0f)
        {
            s *= 1.525f;
            return 0.5f * (t * t * ((s + 1.0f) * t - s));
        }
        else
        {
            t -= 2.0f;
            s *= 1.525f;
            return 0.5f * (t * t * ((s + 1.0f) * t + s) + 2.0f);
        }
    }




    public static float easeInBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
    }

    public static float easeOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }



    /// <summary>
	/// 弾性のある動き。振幅と周期のみ
	/// </summary>
	/// <param name="t">今の時間</param>
	/// <param name="totaltime">トータルの時間</param>
	/// <param name="amplitude">振幅。デフォルトは1.0f</param>
	/// <param name="period">周期デフォルトは0.3f</param>
	/// <returns></returns>
    public static float InElasticAmplitude(float t, float totaltime, float amplitude, float period)
    {
        if (t <= 0.0f) return 0.0f;
        if (t >= totaltime) return 0.0f;

        float s = period / (2.0f * Mathf.PI) * Mathf.Asin(1.0f);
        t /= totaltime;

        return -amplitude * Mathf.Pow(2.0f, 10.0f * (t - 1.0f)) * Mathf.Sin((t - 1.0f - s) * (2.0f * Mathf.PI) / period);
    }

    public static float OutElasticAmplitude(float t, float totaltime, float amplitude, float period)
    {
        if (t <= 0.0f) return 0.0f;
        if (t >= totaltime) return 0.0f;

        float s = period / (2.0f * Mathf.PI) * Mathf.Asin(1.0f);
        t /= totaltime;

        return amplitude * Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / period);
    }

    public static float InOutElasticAmplitude(float t, float totaltime, float amplitude, float period)
    {
        if (t <= 0.0f) return 0.0f;
        if (t >= totaltime) return 0.0f;

        //float s = period / (2.0f * Mathf.PI) * Mathf.Asin(1.0f);
        t /= totaltime;
        float backPoint = 0.5f;


        if (t < backPoint)
        {
            Debug.Log("no");
            return OutElasticAmplitude(t, totaltime, amplitude, period);
        }
        else
        {
            Debug.Log("back");
            return InElasticAmplitude(t - backPoint, totaltime - backPoint, amplitude, period);
        }
        //if (t < 1.0f)
        //{
        //    return -0.5f * amplitude * Mathf.Pow(2.0f, 10.0f * (t - 1.0f)) * Mathf.Sin((t - 1.0f - s) * (2.0f * Mathf.PI) / period);
        //}
        //else
        //{
        //    return amplitude * Mathf.Pow(2.0f, -10.0f * (t - 1.0f)) * Mathf.Sin((t - 1.0f - s) * (2.0f * Mathf.PI) / period) * 0.5f + 1.0f;
        //}
    }

    public static Vector3 EaseAmplitudeScale(Vector3 initScale, float easeT, float easeTime, float ampritude, float period)
    {
        Vector3 newPos;
        newPos = initScale;
        newPos.x = initScale.x + -Easing.OutElasticAmplitude(easeT, easeTime, ampritude, period);
        newPos.y = initScale.y + Easing.OutElasticAmplitude(easeT, easeTime, ampritude, period);
        newPos.z = initScale.z - Easing.OutElasticAmplitude(easeT, easeTime, ampritude, period);
        return newPos;
    }
    public static Vector3 EaseAmplitudeScaleY(Vector3 initScale, float easeT, float easeTime, float ampritude, float period)
    {
        Vector3 newPos;
        newPos = initScale;
        newPos.y = initScale.y + Easing.OutElasticAmplitude(easeT, easeTime, ampritude, period);
        return newPos;
    }
    //0~1の範囲のbackRatioで戻るイージング
    //backRatioが0.5ならtotalTimeの半分、0.1なら最初の方に戻ってくる
    //注意点、戻ってくる時はminとmaxが逆になるので下のような動きをする

    //-------EaseIn---------/-------EaseIn---------
    //
    //                     */*******
    //                    * /       *****
    //                  **  /            ***
    //               ***    /               **
    //          *****       /                 *
    //    ******            /                  *

    //-------EaseIn---------/-------EaseOut---------
    //
    //                     */*
    //                    * / *
    //                  **  /  **
    //               ***    /    ***
    //          *****       /       *****
    //    ******            /            ******




    //こっちはまだ計算しないのでジェネリクス (T)を使えるっぽい
    public static T ZeroEeasing<T>(float t, float totaltime, T min, T max, float backRatio,
           Func<float, float, T, T, T> func1, Func<float, float, T, T, T> func2// Func<float, float,float,float,float>func1 の< >の中は引数...戻り値、ということ
           )
    {
        if (t <= 0.0f) return min;
        if (t >= totaltime) return min;
        float backPoint = totaltime * backRatio;//戻る時間(秒)を求める

        if (t < backPoint)
        {
            return func1(t, backPoint, min, max);
        }
        else
        {
            return func2(t - backPoint, totaltime - backPoint, max, min);
        }
    }
}
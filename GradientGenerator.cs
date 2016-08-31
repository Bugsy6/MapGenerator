using UnityEngine;
using System.Collections;

public class GradientGenerator {

    public static float[,] GenerateTopGradient(int size)
    {
        float[,] gradient = new float[size , size];
        float t = 0.001f;
        float tIncrement = 0.005f;

        for (int y = 0; y < size; y++)
        {
            float vertColor = Mathf.Lerp(1.0f, 0.0f, t);
            for (int x = 0; x < size; x++)
            {
                gradient[x, y] = Evaluate(vertColor);
            }
            t += tIncrement;
        }
        
        return gradient;
    }

    public static float[,] GenerateBottomGradient(int size)
    {
        float[,] gradient = new float[size, size];
        float t = 0.001f;
        float tIncrement = 0.005f;

        for (int y = 0; y < size; y++)
        {
            float vertColor = Mathf.Lerp(0.0f, 1.0f, t);
            for (int x = 0; x < size; x++)
            {
                gradient[x, y] = Evaluate(vertColor);
            }
            t += tIncrement;
        }
        return gradient;
    }

    public static float[,] GenerateRightGradient(int size)
    {
        float[,] gradient = new float[size, size];
        float t = 0.001f;
        float tIncrement = 0.005f;

        for (int x = 0; x < size; x++)
        {
            float vertColor = Mathf.Lerp(1.0f, 0.0f, t);
            for (int y = 0; y < size; y++)
            {
                gradient[x, y] = Evaluate(vertColor);
            }
            t += tIncrement;

        }
        return gradient;
    }

    public static float[,] GenerateLeftGradient(int size)
    {
        float[,] gradient = new float[size, size];
        float t = 0.001f;
        float tIncrement = 0.005f;

        for (int x = 0; x < size; x++)
        {
            float vertColor = Mathf.Lerp(0.0f, 1.0f, t);
            for (int y = 0; y < size; y++)
            {
                gradient[x, y] = Evaluate(vertColor);
            }
            t += tIncrement;

        }
        return gradient;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}

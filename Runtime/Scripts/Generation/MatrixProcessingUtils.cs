using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatrixProcessingUtils
{
    public static float[,] InterpolateBilinear(float[,] sourceArray, int scaleFactor) 
    {
        int srcRows = sourceArray.GetLength(0);
        int srcCols = sourceArray.GetLength(1);
        float[,] interpolatedArray = new float[srcRows*scaleFactor, srcCols*scaleFactor];
        
        for (int row = 0; row < srcRows - 1; row++) 
        {
            for (int col = 0; col < srcCols - 1; col++) 
            {
                float x1 = sourceArray[row, col];
                float x2 = sourceArray[row, col + 1];
                float x3 = sourceArray[row + 1, col];
                float x4 = sourceArray[row + 1, col + 1];
                
                for (int p = 0; p < scaleFactor; p++) 
                {
                    for (int q = 0; q < scaleFactor; q++) 
                    {
                        float y1 = x1 * (scaleFactor - p) * (scaleFactor - q);
                        float y2 = x2 * (scaleFactor - p) * q;
                        float y3 = x3 * p * (scaleFactor - q);
                        float y4 = x4 * p * q;
                        
                        interpolatedArray[row * scaleFactor + p, col * scaleFactor + q] = (y1 + y2 + y3 + y4) / (scaleFactor * scaleFactor);
                    }
                }
            }
        }
        
        return interpolatedArray;
    }

    public static float[,] SmoothBorders(float[,] inputArray) {
        float[,] outputArray = new float[inputArray.GetLength(0), inputArray.GetLength(1)];
        
        for (int i = 0; i < inputArray.GetLength(0); i++) {
            for (int j = 0; j < inputArray.GetLength(1); j++) {
                if (inputArray[i, j] == 0 && (i > 0 && inputArray[i - 1, j] == 1) && (i < inputArray.GetLength(0) - 1 && inputArray[i + 1, j] == 1)) {
                    // Smooth horizontal border
                    float t = 0.5f;
                    float y0 = inputArray[i - 1, j];
                    float y1 = inputArray[i + 1, j];
                    float a = (y0 + y1 - 2 * inputArray[i, j]) / 2;
                    float b = (y1 - y0) / 2;
                    float c = inputArray[i, j];
                    outputArray[i, j] = a * t * t + b * t + c;
                } else if (inputArray[i, j] == 0 && (j > 0 && inputArray[i, j - 1] == 1) && (j < inputArray.GetLength(1) - 1 && inputArray[i, j + 1] == 1)) {
                    // Smooth vertical border
                    float t = 0.5f;
                    float y0 = inputArray[i, j - 1];
                    float y1 = inputArray[i, j + 1];
                    float a = (y0 + y1 - 2 * inputArray[i, j]) / 2;
                    float b = (y1 - y0) / 2;
                    float c = inputArray[i, j];
                    outputArray[i, j] = a * t * t + b * t + c;
                } else {
                    outputArray[i, j] = inputArray[i, j];
                }
            }
        }
        
        return outputArray;
    }

    public static float[,] BlurLinear(float[,] inputArray, float fadeWidth) {
        float[,] outputArray = new float[inputArray.GetLength(0), inputArray.GetLength(1)];
        for (int i = 0; i < inputArray.GetLength(0); i++) {
            for (int j = 0; j < inputArray.GetLength(1); j++) {
                if (inputArray[i,j] == 0) {
                    float sum = 0;
                    int count = 0;
                    for (int k = i - (int)fadeWidth; k <= i + (int)fadeWidth; k++) {
                        for (int l = j - (int)fadeWidth; l <= j + (int)fadeWidth; l++) {
                            if (k >= 0 && k < inputArray.GetLength(0) && l >= 0 && l < inputArray.GetLength(1)) {
                                sum += inputArray[k,l];
                                count++;
                            }
                        }
                    }
                    float average = sum / count;
                    outputArray[i,j] = Mathf.Lerp(inputArray[i,j], average, fadeWidth);
                } else {
                    outputArray[i,j] = inputArray[i,j];
                }
            }
        }
        return outputArray;
    }

    public static float[,] BlurGauss(float[,] inputArray, float fadeWidth) {
        int kernelSize = (int)(fadeWidth * 2 + 1);
        float[,] kernel = new float[kernelSize, kernelSize];
        float sigma = fadeWidth / 3f;
        float sum = 0;
        for (int i = 0; i < kernelSize; i++) {
            for (int j = 0; j < kernelSize; j++) {
                float x = i - fadeWidth;
                float y = j - fadeWidth;
                kernel[i,j] = Mathf.Exp(-(x*x+y*y)/(2*sigma*sigma));
                sum += kernel[i,j];
            }
        }
        for (int i = 0; i < kernelSize; i++) {
            for (int j = 0; j < kernelSize; j++) {
                kernel[i,j] /= sum;
            }
        }
        float[,] outputArray = new float[inputArray.GetLength(0), inputArray.GetLength(1)];
        for (int i = 0; i < inputArray.GetLength(0); i++) {
            for (int j = 0; j < inputArray.GetLength(1); j++) {
                if (inputArray[i,j] == 0) {
                    float sum1 = 0;
                    for (int k = i - (int)fadeWidth; k <= i + (int)fadeWidth; k++) {
                        for (int l = j - (int)fadeWidth; l <= j + (int)fadeWidth; l++) {
                            if (k >= 0 && k < inputArray.GetLength(0) && l >= 0 && l < inputArray.GetLength(1)) {
                                sum1 += inputArray[k,l] * kernel[k-i+(int)fadeWidth,l-j+(int)fadeWidth];
                            }
                        }
                    }
                    outputArray[i,j] = sum1;
                } else {
                    outputArray[i,j] = inputArray[i,j];
                }
            }
        }
        return outputArray;
    }


}

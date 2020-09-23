using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

using System.Collections;
using UnityEngine;

public class ColorDetection : MonoBehaviour
{
    public GameObject originQuad;
    public GameObject hsvQuad;
    public GameObject colorRangeQuad;
    public GameObject rQuad;
    public GameObject gQuad;
    public GameObject bQuad;

    public Texture2D originTexture;

    private Mat originMat;
    private Mat hsvMat;
    private Mat colorRangeMat;
    private Mat rMat, rResult;
    private Mat gMat, gResult;
    private Mat bMat, bResult;

    private Scalar rMin, rMax;
    private Scalar gMin, gMax;
    private Scalar bMin, bMax;

    private void Start()
    {
        InitMatrix();
        InitScalar();

        Imgproc.cvtColor(originMat, hsvMat, Imgproc.COLOR_RGB2HSV);

        Core.inRange(hsvMat, rMin, rMax, colorRangeMat);

        Core.inRange(hsvMat, rMin, rMax, rMat);
        Core.inRange(hsvMat, gMin, gMax, gMat);
        Core.inRange(hsvMat, bMin, bMax, bMat);

        Imgproc.cvtColor(rMat, rMat, Imgproc.COLOR_GRAY2RGB);
        Imgproc.cvtColor(gMat, gMat, Imgproc.COLOR_GRAY2RGB);
        Imgproc.cvtColor(bMat, bMat, Imgproc.COLOR_GRAY2RGB);

        Core.bitwise_and(originMat, rMat, rResult);
        Core.bitwise_and(originMat, gMat, gResult);
        Core.bitwise_and(originMat, bMat, bResult);

        DrawMatrix();
    }

    private void InitMatrix()
    {
        originMat = new Mat(originTexture.height, originTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(originTexture, originMat);
        DrawMat(originQuad, originMat);

        hsvMat = new Mat();
        colorRangeMat = new Mat();
        rMat = new Mat();
        gMat = new Mat();
        bMat = new Mat();

        rResult = new Mat(originTexture.height, originTexture.width, CvType.CV_8UC3);
        gResult = new Mat(originTexture.height, originTexture.width, CvType.CV_8UC3);
        bResult = new Mat(originTexture.height, originTexture.width, CvType.CV_8UC3);
    }

    private void InitScalar()
    {
        rMin = new Scalar(0, 200, 0);
        rMax = new Scalar(19, 255, 255);

        gMin = new Scalar(40, 80, 80);
        gMax = new Scalar(70, 255, 255);

        bMin = new Scalar(92, 0, 0);
        bMax = new Scalar(124, 256, 256);
    }

    private void DrawMatrix()
    {
        DrawMat(originQuad, originMat);
        DrawMat(hsvQuad, hsvMat);
        DrawMat(colorRangeQuad, colorRangeMat);
        //DrawMat(rQuad, rMat);
        //DrawMat(gQuad, gMat);
        //DrawMat(bQuad, bMat);
        DrawMat(rQuad, rResult);
        DrawMat(gQuad, gResult);
        DrawMat(bQuad, bResult);
    }

    private void DrawMat(GameObject _quad, Mat _mat)
    {
        Texture2D outputTexture = new Texture2D(_mat.width(), _mat.height(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(_mat, outputTexture);
        _quad.GetComponent<Renderer>().material.mainTexture = outputTexture;
    }
}

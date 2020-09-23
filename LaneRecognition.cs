using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using UnityEngine;

public class LaneRecognition : MonoBehaviour
{
    // 원본 이미지
    public Texture2D roadTexture;

    public GameObject quad_1;
    public GameObject quad_2;
    public GameObject quad_3;
    public GameObject quad_4;
    public GameObject quad_5;
    public GameObject quad_6;

    #region OpenCV
    // 원본 Mat
    private Mat originMat;

    // 쓰고 읽을 Matrix
    private Mat inputMat;
    private Mat outputMat;

    // 이미지 가공 결과물이 저장될 Mat
    private Mat gaussianMat;
    private Mat grayMat;
    private Mat contourMat;
    private Mat regionMat;
    private Mat houghMat;
    private Mat lineMat;
    #endregion

    private void Start()
    {
        // Imgproc으로 이미지를 수정할 때마다 이미지가 반전된다
        // Core.flip(inputMat, outputMat, 0)으로 원래대로 회전 할 수 있음

        originMat = new Mat(roadTexture.height, roadTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(roadTexture, originMat);
        DrawMat(quad_1, originMat);

        Debug.Log("LOAD " + roadTexture.width.ToString() + "x" + roadTexture.height.ToString() + " :: roadTexture");
        outputMat = originMat.clone();
        inputMat = originMat.clone();

        // 원본 - > 흑백
        grayMat = new Mat();
        Imgproc.cvtColor(inputMat, grayMat, Imgproc.COLOR_BGR2GRAY);
        DrawMat(quad_2, grayMat);

        // 흑백 - > 가우스 필터 흑백
        gaussianMat = new Mat();
        Imgproc.GaussianBlur(grayMat, gaussianMat, gaussianMat.size(), 2, 2);
        DrawMat(quad_3, gaussianMat);

        // 가우스 필터 흑백 - > 테두리
        contourMat = new Mat();
        Imgproc.Canny(gaussianMat, contourMat, 50, 200);
        DrawMat(quad_4, contourMat);

        // 테두리 - > 관심영역 씌운 테두리
        regionMat = WriteRegionOfInterest(contourMat);
        DrawMat(quad_5, regionMat);

        // 관심영역 씌운 테두리 - > hough 알고리즘으로 추출한 선 좌표 Matrix
        houghMat = new Mat();
        Imgproc.HoughLinesP(regionMat, houghMat, 2, Mathf.PI / 180, 90, 120, 150);
        Debug.Log(houghMat.dump());

        // 선 좌표 Matrix - > 선만 그려진 Mat
        lineMat = Mat.zeros(outputMat.rows(), outputMat.cols(), outputMat.type());
        for (int x = 0; x < houghMat.rows(); x++)
        {
            Point pt1 = new Point(houghMat.get(x, 0)[0], houghMat.get(x, 0)[1]);
            Point pt2 = new Point(houghMat.get(x, 0)[2], houghMat.get(x, 0)[3]);

            Debug.Log(pt1.ToString() + "/" + pt2.ToString());
            Imgproc.line(lineMat, pt1, pt2, new Scalar(255, 0, 0), 4, Imgproc.LINE_AA, 0);
        }

        // 선만 그려진 Mat와 원본을 합침
        Core.addWeighted(lineMat, 0.8, inputMat, 1, 0, outputMat);
        DrawMat(quad_6, outputMat);
    }

    // Quad의 Texture를 바꿈
    private void DrawMat(GameObject _quad, Mat _mat)
    {
        Texture2D outputTexture = new Texture2D(_mat.width(), _mat.height(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(_mat, outputTexture);
        _quad.GetComponent<Renderer>().material.mainTexture = outputTexture;
    }

    // 관심 영역을 생성하여 _mat에 덧씌워줌
    private Mat WriteRegionOfInterest(Mat _mat)
    {
        // Matrix를 0으로 초기화 (까만 Mat)
        Mat maskMat = Mat.zeros(_mat.rows(), _mat.cols(), _mat.type());

        // 관심영역 좌표
        Point[] points = new Point[4];
        points[0] = new Point(100, _mat.height());
        points[1] = new Point(450, 320);
        points[2] = new Point(550, 320);
        points[3] = new Point(_mat.width() - 20, _mat.height());

        List<MatOfPoint> pointList = new List<MatOfPoint>();
        pointList.Add(new MatOfPoint(points));

        // 까만 Mat에 하얀색으로 관심영역을 그려줌
        Imgproc.fillPoly(maskMat, pointList, new Scalar(255, 255, 255));
        // and연산으로 하얀 영역과 _mat가 겹치는 부분만 남김
        Core.bitwise_and(_mat, maskMat, maskMat);
        return maskMat;
    }

}

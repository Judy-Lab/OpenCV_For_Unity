using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.Features2dModule;

using System.Linq;
using OpenCVForUnityExample;

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using UnityEditorInternal;
using OpenCVForUnity.Calib3dModule;

[RequireComponent(typeof(WebCamTextureToMatHelper))]
public class Homography : MonoBehaviour
{
    private WebCamTextureToMatHelper webCamTextureToMatHelper;
    private Texture2D webCamTexture;

    public GameObject dstQuad;
    public Texture2D originMakerTexture;

    private Mat inputMat;
    private Mat outputMat;
    private Mat grayMat;

    private ORB detector;
    private ORB extractor;

    private Texture2D makerTexture;
    private Mat makerMat;
    private Mat makerGrayMat;
    private MatOfKeyPoint makerKeyPoints;
    private Mat makerDescriptors;
    private Texture2D texture;

    private DescriptorMatcher matcher;
    private bool first = false;

    private void Start()
    {
        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
        webCamTextureToMatHelper.Initialize();

        grayMat = new Mat();
        makerGrayMat = new Mat(originMakerTexture.height, originMakerTexture.width, CvType.CV_8UC1);

        makerTexture = new Texture2D(originMakerTexture.width, originMakerTexture.height);
        Graphics.CopyTexture(originMakerTexture, makerTexture);

        detector = ORB.create();
        extractor = ORB.create();

        // Get Key Points of Maker
        makerMat = new Mat(originMakerTexture.height, originMakerTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(makerTexture, makerMat, false);
        makerKeyPoints = new MatOfKeyPoint();
        makerDescriptors = new Mat();

        Imgproc.cvtColor(makerMat, makerGrayMat, Imgproc.COLOR_BGR2GRAY);

        detector.detect(makerGrayMat, makerKeyPoints);
        extractor.compute(makerGrayMat, makerKeyPoints, makerDescriptors);

        matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
    }

    private void Update()
    {
        inputMat = webCamTextureToMatHelper.GetMat();

        MatOfKeyPoint camKeyPoints = new MatOfKeyPoint();
        Mat camDescriptors = new Mat();

        Imgproc.cvtColor(inputMat, grayMat, Imgproc.COLOR_BGR2GRAY);

        detector.detect(grayMat, camKeyPoints);
        extractor.compute(grayMat, camKeyPoints, camDescriptors);

        if (camKeyPoints.toList().Count < 1)
            return;

        List<MatOfDMatch> matches = new List<MatOfDMatch>();
        matcher.knnMatch(makerDescriptors, camDescriptors, matches, 2);

        //-- Filter matches using the Lowe's ratio test
        float ratioThresh = 0.75f;
        List<DMatch> listOfGoodMatches = new List<DMatch>();
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].rows() > 1)
            {
                DMatch[] dMatches = matches[i].toArray();
                if (dMatches[0].distance < ratioThresh * dMatches[1].distance)
                {
                    listOfGoodMatches.Add(dMatches[0]);
                }
            }
        }
        MatOfDMatch goodMatches = new MatOfDMatch();
        goodMatches.fromList(listOfGoodMatches);

        //-- Draw matches
        Mat resultImg = new Mat();
        Features2d.drawMatches(makerMat, makerKeyPoints, grayMat, camKeyPoints, goodMatches, resultImg);

        //listOfGoodMatches = goodMatches.toList();

        ////-- Localize the object
        //List<Point> obj = new List<Point>();
        //List<Point> scene = new List<Point>();
        //List<KeyPoint> listOfKeypointsObject = makerKeyPoints.toList();
        //List<KeyPoint> listOfKeypointsScene = camKeyPoints.toList();
        //for (int i = 0; i < listOfGoodMatches.Count(); i++)
        //{
        //    //-- Get the keypoints from the good matches
        //    obj.Add(listOfKeypointsObject[listOfGoodMatches[i].queryIdx].pt);
        //    scene.Add(listOfKeypointsScene[listOfGoodMatches[i].trainIdx].pt);
        //}
        //MatOfPoint2f objMat = new MatOfPoint2f();
        //MatOfPoint2f sceneMat = new MatOfPoint2f();
        //objMat.fromList(obj);
        //sceneMat.fromList(scene);
        //double ransacReprojThreshold = 3.0;
        //Mat H = Calib3d.findHomography(objMat, sceneMat, Calib3d.RANSAC, ransacReprojThreshold);

        ////-- Get the corners from the image_1 ( the object to be "detected" )
        //Mat objCorners = new Mat(4, 1, CvType.CV_32FC2); 
        //Mat sceneCorners = new Mat();
        //float[] objCornersData = new float[(int)(objCorners.total() * objCorners.channels())];
        //objCorners.get(0, 0, objCornersData);
        //objCornersData[0] = 0;
        //objCornersData[1] = 0;
        //objCornersData[2] = makerMat.cols();
        //objCornersData[3] = 0;
        //objCornersData[4] = makerMat.cols();
        //objCornersData[5] = makerMat.rows();
        //objCornersData[6] = 0;
        //objCornersData[7] = makerMat.rows();
        //objCorners.put(0, 0, objCornersData);

        //Core.perspectiveTransform(objCorners, sceneCorners, H);
        //byte[] sceneCornersData = new byte[(int)(sceneCorners.total() * sceneCorners.channels())];
        //sceneCorners.get(0, 0, sceneCornersData);

        ////-- Draw lines between the corners (the mapped object in the scene - image_2 )
        //Imgproc.line(resultImg, new Point(sceneCornersData[0] + makerMat.cols(), sceneCornersData[1]),
        //        new Point(sceneCornersData[2] + makerMat.cols(), sceneCornersData[3]), new Scalar(0, 255, 0), 4);
        //Imgproc.line(resultImg, new Point(sceneCornersData[2] + makerMat.cols(), sceneCornersData[3]),
        //        new Point(sceneCornersData[4] + makerMat.cols(), sceneCornersData[5]), new Scalar(0, 255, 0), 4);
        //Imgproc.line(resultImg, new Point(sceneCornersData[4] + makerMat.cols(), sceneCornersData[5]),
        //        new Point(sceneCornersData[6] + makerMat.cols(), sceneCornersData[7]), new Scalar(0, 255, 0), 4);
        //Imgproc.line(resultImg, new Point(sceneCornersData[6] + makerMat.cols(), sceneCornersData[7]),
        //        new Point(sceneCornersData[0] + makerMat.cols(), sceneCornersData[1]), new Scalar(0, 255, 0), 4);

        if (!first)
        {
            texture = new Texture2D(resultImg.cols(), resultImg.rows(), TextureFormat.RGBA32, false);
            dstQuad.GetComponent<Renderer>().material.mainTexture = texture;
            first = true;
        }

        Utils.matToTexture2D(resultImg, texture);
    }

    #region WebCam
    /// <summary>
    /// Raises the webcam texture to mat helper initialized event.
    /// </summary>
    public void OnWebCamTextureToMatHelperInitialized()
    {
        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

        webCamTexture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        Utils.fastMatToTexture2D(webCamTextureMat, webCamTexture);

        dstQuad.GetComponent<Renderer>().material.mainTexture = webCamTexture;
        //dstQuad.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
        Debug.Log(webCamTextureMat.cols().ToString() + " / " + webCamTextureMat.rows().ToString());

        float width = webCamTextureMat.width();
        float height = webCamTextureMat.height();

        #region CAMERA RESIZE CODE (DISABLE)
        //float widthScale = (float)Screen.width / width;
        //float heightScale = (float)Screen.height / height;
        //if (widthScale < heightScale)
        //{
        //    Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        //}
        //else
        //{
        //    Camera.main.orthographicSize = height / 2;
        //}
        #endregion

        float widthScale = dstQuad.transform.localScale.x / width;
        float heightScale = dstQuad.transform.localScale.y / height;

        //dstQuad.transform.localScale = new Vector3(width * widthScale, height * heightScale, 1);
    }

    /// <summary>
    /// Raises the webcam texture to mat helper disposed event.
    /// </summary>
    public void OnWebCamTextureToMatHelperDisposed()
    {
        if (inputMat != null)
            inputMat.Dispose();
        if (outputMat != null)
            outputMat.Dispose();

        if (webCamTexture != null)
        {
            Texture2D.Destroy(webCamTexture);
            webCamTexture = null;
        }
    }

    /// <summary>
    /// Raises the webcam texture to mat helper error occurred event.
    /// </summary>
    /// <param name="errorCode">Error code.</param>
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }
    #endregion
}

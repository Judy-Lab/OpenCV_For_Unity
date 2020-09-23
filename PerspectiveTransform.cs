using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

public class PerspectiveTransform : MonoBehaviour
{
    public GameObject oriQuad;
    public GameObject perQuad;

    // Start is called before the first frame update
    void Start()
    {
        Texture2D sourceTexture = Resources.Load("maker") as Texture2D;
        Mat inputMat = new Mat(sourceTexture.height, sourceTexture.width, CvType.CV_8UC4);

        Utils.texture2DToMat(sourceTexture, inputMat);
        UnityEngine.Debug.Log("inputMat.ToString() " + inputMat.ToString());

        Mat src_mat = new Mat(4, 1, CvType.CV_32FC2);
        Mat dst_mat = new Mat(4, 1, CvType.CV_32FC2);

        Mat outputMat = inputMat.clone();

        src_mat.put(0, 0, 0.0, 0.0, sourceTexture.width, 0.0, 0.0, sourceTexture.height, sourceTexture.width, sourceTexture.height);
        dst_mat.put(0, 0, 0.0, 0.0, sourceTexture.width, 100.0, 0.0, sourceTexture.height, sourceTexture.width, sourceTexture.height);

        Mat perspectiveTransform = Imgproc.getPerspectiveTransform(src_mat, dst_mat);
        Imgproc.warpPerspective(inputMat, outputMat, perspectiveTransform, new Size(sourceTexture.width, sourceTexture.height));

        Texture2D outputTexture = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);
        Texture2D inputTexture = new Texture2D(inputMat.cols(), inputMat.rows(), TextureFormat.RGBA32, false);

        #region CIRCLE POINT
        Imgproc.circle(inputMat, new Point(0, 0), 4, new Scalar(255, 0, 0), 8);
        Imgproc.circle(inputMat, new Point(sourceTexture.width, 0), 4, new Scalar(255, 0, 0), 8);
        Imgproc.circle(inputMat, new Point(0, sourceTexture.height), 4, new Scalar(255, 0, 0), 8);
        Imgproc.circle(inputMat, new Point(sourceTexture.width, sourceTexture.height), 4, new Scalar(255, 0, 0), 8);

        Imgproc.circle(outputMat, new Point(0, 0), 4, new Scalar(0, 0, 255), 8);
        Imgproc.circle(outputMat, new Point(sourceTexture.width, 100), 4, new Scalar(0, 0, 255), 8);
        Imgproc.circle(outputMat, new Point(0, sourceTexture.height), 4, new Scalar(0, 0, 255), 8);
        Imgproc.circle(outputMat, new Point(sourceTexture.width, sourceTexture.height), 4, new Scalar(0, 0, 255), 8);
        #endregion

        Utils.matToTexture2D(outputMat, outputTexture);
        Utils.matToTexture2D(inputMat, inputTexture);
        perQuad.GetComponent<Renderer>().material.mainTexture = outputTexture;
        oriQuad.GetComponent<Renderer>().material.mainTexture = inputTexture;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MvCamCtrl.NET;
using System.Threading;
using HalconDotNet;

namespace MachineVisionInspection
{
    public class MVSCamera
    {
        MyCamera.MV_CC_DEVICE_INFO stDevInfo = new MyCamera.MV_CC_DEVICE_INFO();
        MyCamera.MV_GIGE_DEVICE_INFO stGigEDev = new MyCamera.MV_GIGE_DEVICE_INFO();
        MyCamera device = new MyCamera();
        byte[] m_pDataForRed = new byte[20 * 1024 * 1024];
        byte[] m_pDataForGreen = new byte[20 * 1024 * 1024];
        byte[] m_pDataForBlue = new byte[20 * 1024 * 1024];
        uint g_nPayloadSize = 0;
        HWindow hWindow1;
        public HWindow HWindow1 { get => hWindow1; set => hWindow1 = value; }
        public MVSCamera()
        {
            stDevInfo.nTLayerType = MyCamera.MV_GIGE_DEVICE;
            hWindow1 = new HWindow();

        }

        public void Open(string cameraIp,string ethernetIp)
        {
            var parts = cameraIp.Split('.');
            try
            {
                int nIp1 = Convert.ToInt32(parts[0]);
                int nIp2 = Convert.ToInt32(parts[1]);
                int nIp3 = Convert.ToInt32(parts[2]);
                int nIp4 = Convert.ToInt32(parts[3]);
                stGigEDev.nCurrentIp = (uint)((nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4);

                parts = ethernetIp.Split('.');
                nIp1 = Convert.ToInt32(parts[0]);
                nIp2 = Convert.ToInt32(parts[1]);
                nIp3 = Convert.ToInt32(parts[2]);
                nIp4 = Convert.ToInt32(parts[3]);
                stGigEDev.nNetExport = (uint)((nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4);
            }
            catch
            {

            }

            IntPtr stGigeInfoPtr = Marshal.AllocHGlobal(216);
            Marshal.StructureToPtr(stGigEDev, stGigeInfoPtr, false);
            stDevInfo.SpecialInfo.stGigEInfo = new Byte[540];
            Marshal.Copy(stGigeInfoPtr, stDevInfo.SpecialInfo.stGigEInfo, 0, 540);

            // ch:创建设备 | en: Create device
            var nRet = device.MV_CC_CreateDevice_NET(ref stDevInfo);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Create Device Fail");
                return;
            }

            // ch:打开设备 | en:Open device
            nRet = device.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Open Device Fail");
                return;
            }

            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            //if (stDevInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            //{
            //    int nPacketSize = device.MV_CC_GetOptimalPacketSize_NET();
            //    if (nPacketSize > 0)
            //    {
            //        nRet = device.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
            //        if (nRet != MyCamera.MV_OK)
            //        {

            //        }
            //    }
            //    else
            //    {

            //    }
            //}
            // ch:获取包大小 || en: Get Payload Size
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            nRet = device.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get PayloadSize Fail");
                return;
            }
            g_nPayloadSize = stParam.nCurValue;

            // ch:设置触发模式为off || en:set trigger mode as off
            device.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);
            device.MV_CC_SetEnumValue_NET("TriggerMode", 0);

            nRet = device.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Start Grabbing Fail");
                return;
            }

        }
        /// <summary>
        /// 开始显示
        /// </summary>
        public void StartDisplay()
        {
            m_bGrabbing = true;
            Thread hReceiveImageThreadHandle = new Thread(ReceiveImageWorkThread);
            hReceiveImageThreadHandle.Start(device);
        }
        /// <summary>
        /// 停止显示
        /// </summary>
        public void StopDisplay()
        {
            //int nRet = -1;
            //// ch:停止抓图 || en:Stop grab image
            //nRet = device.MV_CC_StopGrabbing_NET();
            //if (nRet != MyCamera.MV_OK)
            //{
            //    MessageBox.Show("Stop Grabbing Fail");
            //}
            m_bGrabbing = false;
        }


        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="ImageCallbackFunc"></param>
        public void SetImageCallBack(MyCamera.cbOutputExdelegate ImageCallbackFunc)
        {
            MyCamera.cbOutputExdelegate ImageCallback;
            // ch:注册回调函数 | en:Register image callback
            ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
            var nRet = device.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
            if (MyCamera.MV_OK != nRet)
            {

            }
            // ch:开启抓图 || en: start grab image
            nRet = device.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {

            }
        }

        /// <summary>
        /// 停止抓取、关闭设备、销毁设备
        /// </summary>
        public void Close()
        {
            // ch:停止抓图 | en:Stop grabbing
            var nRet = device.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {

            }

            // ch:关闭设备 | en:Close device
            nRet = device.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {

            }

            // ch:销毁设备 | en:Destroy device
            nRet = device.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {

            }
        }

        //////////////////////////////////////////////////////////////////////
        bool m_bGrabbing;
        public void ReceiveImageWorkThread(object obj)
        {
            int nRet = MyCamera.MV_OK;
            MyCamera device = obj as MyCamera;
            MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            IntPtr pData = Marshal.AllocHGlobal((int)g_nPayloadSize * 3);
            if (pData == IntPtr.Zero)
            {
                return;
            }
            IntPtr pImageBuffer = Marshal.AllocHGlobal((int)g_nPayloadSize * 3);
            if (pImageBuffer == IntPtr.Zero)
            {
                return;
            }

            uint nDataSize = g_nPayloadSize * 3;
            HObject Hobj = new HObject();
            IntPtr RedPtr = IntPtr.Zero;
            IntPtr GreenPtr = IntPtr.Zero;
            IntPtr BluePtr = IntPtr.Zero;
            IntPtr pTemp = IntPtr.Zero;

            while (m_bGrabbing)
            {
                nRet = device.MV_CC_GetOneFrameTimeout_NET(pData, nDataSize, ref pFrameInfo, 1000);
                if (MyCamera.MV_OK == nRet)
                {
                    if (IsColorPixelFormat(pFrameInfo.enPixelType))
                    {//彩色图像
                        if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                        {
                            pTemp = pData;
                        }
                        else
                        {
                            nRet = ConvertToRGB(pData, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType, pImageBuffer);
                            if (MyCamera.MV_OK != nRet)
                            {
                                return;
                            }
                            pTemp = pImageBuffer;
                        }

                        unsafe
                        {
                            byte* pBufForSaveImage = (byte*)pTemp;

                            UInt32 nSupWidth = (pFrameInfo.nWidth + (UInt32)3) & 0xfffffffc;

                            for (int nRow = 0; nRow < pFrameInfo.nHeight; nRow++)
                            {
                                for (int col = 0; col < pFrameInfo.nWidth; col++)
                                {
                                    m_pDataForRed[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col)];
                                    m_pDataForGreen[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 1)];
                                    m_pDataForBlue[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 2)];
                                }
                            }
                        }

                        RedPtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForRed, 0);
                        GreenPtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForGreen, 0);
                        BluePtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForBlue, 0);

                        try
                        {
                            HOperatorSet.GenImage3Extern(out Hobj, (HTuple)"byte", pFrameInfo.nWidth, pFrameInfo.nHeight,
                                                (new HTuple(RedPtr)), (new HTuple(GreenPtr)), (new HTuple(BluePtr)), IntPtr.Zero);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    else if (IsMonoPixelFormat(pFrameInfo.enPixelType))
                    {//黑白图像
                        if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                        {
                            pTemp = pData;
                        }
                        else
                        {
                            nRet = ConvertToMono8(pData, pImageBuffer, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType);
                            if (MyCamera.MV_OK != nRet)
                            {
                                return;
                            }
                            pTemp = pImageBuffer;
                        }
                        try
                        {
                            HOperatorSet.GenImage1Extern(out Hobj, "byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pTemp, IntPtr.Zero);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            return;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    HalconDisplay(hWindow1, Hobj, pFrameInfo.nHeight, pFrameInfo.nWidth);
                }
                else
                {
                    continue;
                }
            }
            //Hobj 也同时被清除
            if (pData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pData);
            }
            if (pImageBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pImageBuffer);
            }
            return;
        }

       
        public void HalconDisplay(HTuple hWindow, HObject Hobj, HTuple hHeight, HTuple hWidth)
        {
            // ch: 显示 || display
            try
            {
                HOperatorSet.SetPart(hWindow, 0, 0, hHeight - 1, hWidth - 1);// ch: 使图像显示适应窗口大小 || en: Make the image adapt the window size
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            if (hWindow == null)
            {
                return;
            }
            try
            {
                HOperatorSet.DispObj(Hobj, hWindow);// ch 显示 || en: display
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            return;
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        public void DisplayWindowsInitial(PictureBox pictureBox)
        {
            // ch: 定义显示的起点和宽高 || en: Definition the width and height of the display window
            HTuple hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight;

            // ch: 设置显示窗口的起点和宽高 || en: Set the width and height of the display window
            hWindowRow = 0;
            hWindowColumn = 0;
            hWindowWidth = pictureBox.Width;//窗口的宽度
            hWindowHeight = pictureBox.Height;//窗口的高度

            try
            {
                HTuple hWindowID = (HTuple)pictureBox.Handle;//获得C#图像控件的句柄转为halcon用句柄
                hWindow1.OpenWindow(hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight, hWindowID, "visible", "");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

        }

        public HObject GetHalconImage()
        {
            HObject hObject = null;
            MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            IntPtr pData = Marshal.AllocHGlobal((int)g_nPayloadSize * 3);
            if (pData == IntPtr.Zero)
            {
                return null;
            }
            IntPtr pImageBuffer = Marshal.AllocHGlobal((int)g_nPayloadSize * 3);
            if (pImageBuffer == IntPtr.Zero)
            {
                return null ;
            }
            uint nDataSize = g_nPayloadSize * 3;
            IntPtr RedPtr = IntPtr.Zero;
            IntPtr GreenPtr = IntPtr.Zero;
            IntPtr BluePtr = IntPtr.Zero;
            IntPtr pTemp = IntPtr.Zero;

            int nRet = -1;
            nRet = device.MV_CC_GetOneFrameTimeout_NET(pData, nDataSize, ref pFrameInfo, 1000);

            //图像转换
            if (IsColorPixelFormat(pFrameInfo.enPixelType))
            {//彩色图像
                if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    pTemp = pData;
                }
                else
                {
                    nRet = ConvertToRGB(pData, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType, pImageBuffer);
                    if (MyCamera.MV_OK != nRet)
                    {
                        return null;
                    }
                    pTemp = pImageBuffer;
                }

                unsafe
                {
                    byte* pBufForSaveImage = (byte*)pTemp;

                    UInt32 nSupWidth = (pFrameInfo.nWidth + (UInt32)3) & 0xfffffffc;

                    for (int nRow = 0; nRow < pFrameInfo.nHeight; nRow++)
                    {
                        for (int col = 0; col < pFrameInfo.nWidth; col++)
                        {
                            m_pDataForRed[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col)];
                            m_pDataForGreen[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 1)];
                            m_pDataForBlue[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 2)];
                        }
                    }
                }

                RedPtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForRed, 0);
                GreenPtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForGreen, 0);
                BluePtr = Marshal.UnsafeAddrOfPinnedArrayElement(m_pDataForBlue, 0);

                try
                {
                    HOperatorSet.GenImageInterleaved(out hObject, pTemp, "rgb", pFrameInfo.nWidth, pFrameInfo.nHeight,
                        -1, "byte", pFrameInfo.nWidth, pFrameInfo.nHeight, 0, 0, -1, 0);
                    //HOperatorSet.GenImage3Extern(out hObject, (HTuple)"byte", pFrameInfo.nWidth, pFrameInfo.nHeight,
                    //                    (new HTuple(RedPtr)), (new HTuple(GreenPtr)), (new HTuple(BluePtr)), IntPtr.Zero);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else if(IsMonoPixelFormat(pFrameInfo.enPixelType))
            {//黑白图像
                if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    pTemp = pData;
                }
                else
                {
                    nRet = ConvertToMono8(pData, pImageBuffer, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType);
                    if (MyCamera.MV_OK != nRet)
                    {
                        return null;
                    }
                    pTemp = pImageBuffer;
                }
                try
                {
                    HOperatorSet.GenImage1(out hObject, "byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pTemp);
                    //HOperatorSet.GenImage1Extern(out hObject, "byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pTemp, IntPtr.Zero);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null; 
                }
            }
            else { }


            //释放内存
            if (pData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pData);
            }
            if (pImageBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pImageBuffer);
            }

            return hObject;
        }



        #region Auxiliary

        /// <summary>
        /// 灰度格式
        /// </summary>
        /// <param name="enType"></param>
        /// <returns></returns>
        private bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 彩色格式
        /// </summary>
        /// <param name="enType"></param>
        /// <returns></returns>
        private bool IsColorPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGBA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGRA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        public Int32 ConvertToMono8(IntPtr pInData, IntPtr pOutData, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType)
        {
            if (IntPtr.Zero == pInData || IntPtr.Zero == pOutData)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;
            MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

            stPixelConvertParam.pSrcData = pInData;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }

            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);

            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pOutData;//转换后的数据
            stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * 3);

            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MyCamera.MV_OK != nRet)
            {
                return -1;
            }

            return nRet;
        }

        public Int32 ConvertToRGB(IntPtr pSrc, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType, IntPtr pDst)
        {
            if (IntPtr.Zero == pSrc || IntPtr.Zero == pDst)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;
            MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

            stPixelConvertParam.pSrcData = pSrc;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }

            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);

            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pDst;//转换后的数据
            stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            stPixelConvertParam.nDstBufferSize = (uint)nWidth * nHeight * 3;

            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MyCamera.MV_OK != nRet)
            {
                return -1;
            }

            return MyCamera.MV_OK;
        }


        #endregion
    }

}

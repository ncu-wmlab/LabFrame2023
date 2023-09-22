using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace LABFrame2022
{
    public class ServerTester : MonoBehaviour
    {

        [Header("UI")]
        public Button btn_startGenerate;
        public Button btn_stopGenerate;
        public Button btn_sendRequest;
        public Button btn_initData;
        public Button btn_generateData;
        public Button btn_finishData;
        public Text txt_dataInfo;
        public Text txt_uploadInfo;
        

        private bool isGenerate = true;
        private System.Diagnostics.Stopwatch sw;

        void Start()
        {
            btn_startGenerate.onClick.AddListener(StartGenerate);
            btn_stopGenerate.onClick.AddListener(StopGenerate);
            btn_sendRequest.onClick.AddListener(UploadData);
            btn_initData.onClick.AddListener(InitData);
            btn_generateData.onClick.AddListener(GenerateData);
            btn_finishData.onClick.AddListener(FinishData);

            LabDataManager.Instance.SendDataAction += LabDataManager_Info;
        }



        #region Batch Save Data Test
        // 開始生成測試檔案
        private void StartGenerate()
        {
            LabDataManager.Instance.LabDataInit("123456789");
            txt_dataInfo.text = "檔案儲存在：" + LabTools.DataPath;
            isGenerate = true;

            StartCoroutine(DataGenerate());
            btn_startGenerate.interactable = false;
        }
        IEnumerator DataGenerate()
        {
            Debug.Log("Starting Data Generator ... ");
            if (txt_uploadInfo != null)
            {
                txt_uploadInfo.text = "Status：Starting Data Generator ... ";
            }
            float data = 0.5f;
            do
            {
                GenerateData();
                data++;
                yield return null;
            } while (isGenerate);

            Debug.Log("Data Generator end");
            if (txt_uploadInfo != null)
            {
                txt_uploadInfo.text = "Status：Data Generator end";
            }

        }
        // 停止生成
        private void StopGenerate()
        {
            Debug.Log("Stopping Data Generator ... ");
            if (txt_uploadInfo != null)
            {
                txt_uploadInfo.text = "Status：Stopping Data Generator ... ";
            }
            isGenerate = false;
            LABApplication.Instance.AppRestart();

            btn_startGenerate.interactable = true;
        }
        

        #endregion

        #region Single Save Data Test
        // 初始化 json
        private void InitData()
        {
            LabDataManager.Instance.LabDataInit("");

            txt_dataInfo.text = "檔案儲存在：" + LabTools.DataPath;

        }
        // 生成單筆資料
        private void GenerateData()
        {
            LabDataManager.Instance.WriteData(NewMyData());
        }
        // 儲存並重置
        private void FinishData()
        {
            LabDataManager.Instance.SendDataAction -= LabDataManager_Info;
            LABApplication.Instance.AppRestart();
            if (txt_uploadInfo != null)
            {
                txt_uploadInfo.text = "Status：LabFrame Restarted.";
            }
        }

        #endregion

        // 上傳資料
        private void UploadData()
        {
            bool isUpload = LabDataManager.Instance.UploadData();

            if (txt_uploadInfo != null && isUpload)
            {
                txt_uploadInfo.text = "Status：Data Uploaded";
            }
        }

        private void LabDataManager_Info(SendInfo sendInfo)
        {
            if (txt_uploadInfo != null)
            {
                txt_uploadInfo.text = sendInfo.info;
            }
        }

        private MyData NewMyData(float d = 1f)
        {
            return new MyData(
                (int)d,
                d.ToString(),
                d,
                new Vector3(d, d, d),
                new Matrix4x4(
                    new Vector4(d, d, d, d),
                    new Vector4(d, d, d, d),
                    new Vector4(d, d, d, d),
                    new Vector4(d, d, d, d)),
                new Quaternion( d, d, d, d)
            );
        }
    }

    [Serializable]
    class MyData : LabDataBase
    {
        public int id;
        public string Data1;
        public float Data2;
        public Vector3 Data3;
        public Matrix4x4 Data4;
        public Quaternion Data5;

        public MyData(int _id, string _data1, float _data2, Vector3 _data3, Matrix4x4 _data4, Quaternion _data5)
        {
            id = _id;
            Data1 = _data1;
            Data2 = _data2;
            Data3 = _data3;
            Data4 = _data4;
            Data5 = _data5;
        }
    }

}
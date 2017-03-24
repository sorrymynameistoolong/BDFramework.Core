﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BDFramwork.ScreenView;
using System.IO;
using ILRuntimeAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
public enum HotFixDataPath
{
   none = 0,
   PersistentPath,
   StreammingAsset
}
public class ClientMain : MonoBehaviour
{
    public HotFixDataPath m_enumHotFixDataPath = HotFixDataPath.StreammingAsset;
    private string m_strHotFixDataPath;
    //全局的调度中心
    private ScreenViewCenter m_layerCenter;
    //主要层级的layer
    static public ScreenViewLayer g_mainLayer;
    //热更的沙盒
    ILRuntimeAppDomain appdomain;


    
    void Awake()
    {
        switch (m_enumHotFixDataPath)
        {
            case HotFixDataPath.PersistentPath:
                m_strHotFixDataPath = Application.persistentDataPath;
                break;
            case HotFixDataPath.StreammingAsset:
                m_strHotFixDataPath = Application.streamingAssetsPath;
                break;
            default:
                break;
        }
        //加载热更模块
        StartCoroutine(LoadHotFixAssembly());
        //加载配置
        StartCoroutine(LoadHotFixConfig());
        //TODO要修改
        List<string> list = new List<string>()
        {
            "",
        };
        //注册各个模块
        m_layerCenter = new ScreenViewCenter();
        m_layerCenter.AddLayer();
        g_mainLayer = m_layerCenter.GetLayer(0);

        for (int i = 0; i < list.Count - 1; i++)
        {
            g_mainLayer.RegScreen(new ScreenView_HotFix_Wrapper(appdomain, list[i]));
        }

    }

 
    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_layerCenter != null)
        {
            m_layerCenter.Update(Time.deltaTime);
        }
    }
    /// <summary>
    /// 加载热更模块
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        appdomain = new ILRuntimeAppDomain();

#if UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.dll");
#else
        WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.dll");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] dll = www.bytes;
        www.Dispose();

        //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
#if UNITY_ANDROID
        www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.pdb");
#else
        www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.pdb");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] pdb = www.bytes;
        using (System.IO.MemoryStream fs = new MemoryStream(dll))
        {
            using (System.IO.MemoryStream p = new MemoryStream(pdb))
            {
                appdomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
            }
        }
    }

    /// <summary>
    /// 加载热更配置
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadHotFixConfig()
    {
        yield break;
    }
}

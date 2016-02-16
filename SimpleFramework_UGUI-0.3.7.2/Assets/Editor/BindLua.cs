﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

using Object = UnityEngine.Object;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.Rendering;
using Junfine.Debuger;
using UnityEngine.UI;
using SimpleFramework;
using SimpleFramework.Manager;

public static class LuaBinding
{
    //自动生成wrap功能
    //static LuaBinding()
    //{        
    //    string dir = Application.dataPath + "/Source/LuaWrap/";

    //    if (!Directory.Exists(dir))
    //    {
    //        Directory.CreateDirectory(dir);
    //    }
        
    //    string[] files = Directory.GetFiles(dir);

    //    if (files.Length <= 0)
    //    {            
    //        GenLuaDelegates();
    //        Binding();
    //        GenLuaBinder();
    //    }
    //}


    public class BindType
    {
        public string name;                 //类名称
        public Type type;
        public bool IsStatic;
        public string baseName = null;      //继承的类名字
        public string wrapName = "";        //产生的wrap文件名字
        public string libName = "";         //注册到lua的名字

        string GetTypeStr(Type t)
        {
            string str = t.ToString();

            if (t.IsGenericType)
            {
                str = GetGenericName(t);
            }
            else if (str.Contains("+"))
            {
                str = str.Replace('+', '.');
            }

            return str;
        }

        private static string[] GetGenericName(Type[] types)
        {
            string[] results = new string[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsGenericType)
                {
                    results[i] = GetGenericName(types[i]);
                }
                else
                {
                    results[i] = ToLuaExport.GetTypeStr(types[i]);
                }

            }

            return results;
        }

        private static string GetGenericName(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return type.Name;
            }

            Type[] gArgs = type.GetGenericArguments();
            string typeName = type.Name;
            string pureTypeName = typeName.Substring(0, typeName.IndexOf('`'));

            return pureTypeName + "<" + string.Join(",", GetGenericName(gArgs)) + ">";
        }

        public BindType(Type t)
        {
            type = t;

            name = ToLuaExport.GetTypeStr(t);

            if (t.IsGenericType)
            {
                libName = ToLuaExport.GetGenericLibName(t);
                wrapName = ToLuaExport.GetGenericLibName(t);
            }
            else
            {
                libName = t.FullName.Replace("+", ".");
                wrapName = name.Replace('.', '_');

                if (name == "object")
                {
                    wrapName = "System_Object";
                }
            }

            if (t.BaseType != null)
            {
                baseName = ToLuaExport.GetTypeStr(t.BaseType);

                if (baseName == "ValueType")
                {
                    baseName = null;
                }
            }

            if (t.GetConstructor(Type.EmptyTypes) == null && t.IsAbstract && t.IsSealed)
            {
                baseName = null;
                IsStatic = true;
            }
        }

        public BindType SetBaseName(string str)
        {
            baseName = str;
            return this;
        }

        public BindType SetWrapName(string str)
        {
            wrapName = str;
            return this;
        }

        public BindType SetLibName(string str)
        {
            libName = str;
            return this;
        }
    }

    static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    static BindType[] binds = new BindType[]
    {
        _GT(typeof(object)),
        _GT(typeof(System.String)),
        _GT(typeof(System.Enum)),   
        _GT(typeof(IEnumerator)),
        _GT(typeof(System.Delegate)),        
        _GT(typeof(Type)).SetBaseName("System.Object"),                                                     
        _GT(typeof(UnityEngine.Object)),
        
        //测试模板
        ////_GT(typeof(Dictionary<int,string>)).SetWrapName("DictInt2Str").SetLibName("DictInt2Str"),
        
        //custom    
        _GT(typeof(Debugger)),
		_GT(typeof(Util)),
		_GT(typeof(AppConst)),
		_GT(typeof(ByteBuffer)),
        _GT(typeof(NetworkManager)),
        _GT(typeof(ResourceManager)),
        _GT(typeof(PanelManager)),
        _GT(typeof(TimerManager)),
        _GT(typeof(LuaHelper)),
        _GT(typeof(LuaBehaviour)), 
        _GT(typeof(RectTransform)),
        _GT(typeof(DelegateFactory)),
        
        //unity                        
        _GT(typeof(Component)),
        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),        
        _GT(typeof(GameObject)),
        _GT(typeof(Transform)),
        _GT(typeof(Space)),

        _GT(typeof(Camera)),   
        _GT(typeof(CameraClearFlags)),           
        _GT(typeof(Material)),
        _GT(typeof(Renderer)),        
        _GT(typeof(MeshRenderer)),
        _GT(typeof(SkinnedMeshRenderer)),
        _GT(typeof(Light)),
        _GT(typeof(LightType)),     
        _GT(typeof(ParticleEmitter)),
        _GT(typeof(ParticleRenderer)),
        _GT(typeof(ParticleAnimator)),        
                
        _GT(typeof(Physics)),
        _GT(typeof(Collider)),
        _GT(typeof(BoxCollider)),
        _GT(typeof(MeshCollider)),
        _GT(typeof(SphereCollider)),
        
        _GT(typeof(CharacterController)),

        _GT(typeof(Animation)),             
        _GT(typeof(AnimationClip)).SetBaseName("UnityEngine.Object"),
        _GT(typeof(TrackedReference)),
        _GT(typeof(AnimationState)),  
        _GT(typeof(QueueMode)),  
        _GT(typeof(PlayMode)),                  
        
        _GT(typeof(AudioClip)),
        _GT(typeof(AudioSource)),                
        
        _GT(typeof(Application)),
        _GT(typeof(Input)),    
        _GT(typeof(TouchPhase)),            
        _GT(typeof(KeyCode)),             
        _GT(typeof(Screen)),
        _GT(typeof(Time)),
        _GT(typeof(RenderSettings)),
        _GT(typeof(SleepTimeout)),        

        _GT(typeof(AsyncOperation)).SetBaseName("System.Object"),
        _GT(typeof(AssetBundle)),   
        _GT(typeof(BlendWeights)),   
        _GT(typeof(QualitySettings)),          
        _GT(typeof(AnimationBlendMode)),    
        _GT(typeof(Texture)),
        _GT(typeof(RenderTexture)),
        _GT(typeof(ParticleSystem)),
        _GT(typeof(Text)),
        

        //ngui
        /*_GT(typeof(UICamera)),
        _GT(typeof(Localization)),
        _GT(typeof(NGUITools)),

        _GT(typeof(UIRect)),
        _GT(typeof(UIWidget)),        
        _GT(typeof(UIWidgetContainer)),     
        _GT(typeof(UILabel)),        
        _GT(typeof(UIToggle)),
        _GT(typeof(UIBasicSprite)),        
        _GT(typeof(UITexture)),
        _GT(typeof(UISprite)),           
        _GT(typeof(UIProgressBar)),
        _GT(typeof(UISlider)),
        _GT(typeof(UIGrid)),
        _GT(typeof(UIInput)),
        _GT(typeof(UIScrollView)),
        
        _GT(typeof(UITweener)),
        _GT(typeof(TweenWidth)),
        _GT(typeof(TweenRotation)),
        _GT(typeof(TweenPosition)),
        _GT(typeof(TweenScale)),
        _GT(typeof(UICenterOnChild)),    
        _GT(typeof(UIAtlas)),*/ 
   
        //_GT(typeof(LeanTween)),
        //_GT(typeof(LTDescr)),
        
    };


    [MenuItem("Lua/Gen Lua Wrap Files", false, 11)]
    public static void Binding()
    {
        if (!Application.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }

        BindType[] list = binds;

        for (int i = 0; i < list.Length; i++)
        {
            ToLuaExport.Clear();
            ToLuaExport.className = list[i].name;
            ToLuaExport.type = list[i].type;
            ToLuaExport.isStaticClass = list[i].IsStatic;
            ToLuaExport.baseClassName = list[i].baseName;
            ToLuaExport.wrapClassName = list[i].wrapName;
            ToLuaExport.libClassName = list[i].libName;
            ToLuaExport.Generate(null);
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < list.Length; i++)
        {
            sb.AppendFormat("\t\t{0}Wrap.Register();\r\n", list[i].wrapName);
        }

        EditorApplication.isPlaying = false;
        //StringBuilder sb1 = new StringBuilder();

        //for (int i = 0; i < binds.Length; i++)
        //{
        //    sb1.AppendFormat("\t\t{0}Wrap.Register(L);\r\n", binds[i].wrapName);
        //}

        GenLuaBinder();
        GenLuaDelegates();
        Debug.Log("Generate lua binding files over");
        AssetDatabase.Refresh();        
    }

    //[MenuItem("Lua/Gen LuaBinder File", false, 12)]
    static void GenLuaBinder()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("public static class LuaBinder");
        sb.AppendLine("{");
        sb.AppendLine("\tstatic List<string> cachedlist = new List<string>();");
        sb.AppendLine("\tpublic static void Bind(IntPtr L, string type = null)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tif (type == null || cachedlist.Contains(type)) return;");
        sb.AppendLine("\t\tcachedlist.Add(type); type += \"Wrap\";");
        sb.AppendLine("\t\tswitch (type) {");
        string[] files = Directory.GetFiles("Assets/Source/LuaWrap/", "*.cs", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Length; i++)
        {
            string wrapName = Path.GetFileName(files[i]);
            int pos = wrapName.LastIndexOf(".");
            wrapName = wrapName.Substring(0, pos);
            sb.AppendFormat("\t\t\tcase \"{0}\": {0}.Register(L); break;\r\n", wrapName);
        }
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string file = Application.dataPath + "/Source/Base/LuaBinder.cs";

        using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }
    }

    [MenuItem("Lua/Clear LuaBinder File + Wrap Files", false, 13)]
    public static void ClearLuaBinder()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public static class LuaBinder");
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static void Bind(IntPtr L, string type = null)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string file = Application.dataPath + "/Source/Base/LuaBinder.cs";

        using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }
        ClearFiles(Application.dataPath + "/Source/LuaWrap/");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 清除缓存文件
    /// </summary>
    /// <param name="path"></param>
    static void ClearFiles(string path) {
        string[] names = Directory.GetFiles(path);
        foreach (var filename in names) {
            File.Delete(filename); //删除缓存
        }
    }

    static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }

    [MenuItem("Lua/Gen Lua Delegates", false, 12)]
    static void GenLuaDelegates()
    {
        ToLuaExport.Clear();

        DelegateType[] list = new DelegateType[]
        {
            _DT(typeof(Action<GameObject>)),
            //_DT(typeof(Action<GameObject, int, string>)),
            //_DT(typeof(Action<int, int, int, List<int>>)),
            //_DT(typeof(UIEventListener.VoidDelegate)).SetName("VoidDelegate"),            
        };

        ToLuaExport.GenDelegates(list);

        Debug.Log("Create lua delegate over");
    }

    /// <summary>
    /// 编码LUA文件用UTF-8
    /// </summary>
    [MenuItem("Lua/Encode LuaFile with UTF-8", false, 50)]
    public static void EncodeLuaFile() {
        string path = Application.dataPath + "/Lua";
        string[] files = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);
        foreach (string f in files) {
            string file = f.Replace('\\', '/');

            string content = File.ReadAllText(file);
            using (var sw = new StreamWriter(file, false, new UTF8Encoding(false))) {
                sw.Write(content);
            }
            Debug.Log("Encode file::>>" + file + " OK!");
        }
    }

    [MenuItem("Lua/Gen u3d Wrap Files(慎用)", false, 51)]
    public static void U3dBinding()
    {
        List<string> dropList = new List<string>
        {      
            //特殊修改
            "UnityEngine.Object",

            //一般情况不需要的类, 编辑器相关的
            "HideInInspector",
            "ExecuteInEditMode",
            "AddComponentMenu",
            "ContextMenu",
            "RequireComponent",
            "DisallowMultipleComponent",
            "SerializeField",
            "AssemblyIsEditorAssembly",
            "Attribute",  //一些列文件，都是编辑器相关的     
            "FFTWindow",
  
            "Types",
            "UnitySurrogateSelector",            
            "TypeInferenceRules",            
            "ThreadPriority",
            "Debug",        //自定义debugger取代
            "GenericStack",

            //异常，lua无法catch
            "PlayerPrefsException",
            "UnassignedReferenceException",            
            "UnityException",
            "MissingComponentException",
            "MissingReferenceException",

            //RPC网络
            "RPC",
            "Network",
            "MasterServer",
            "BitStream",
            "HostData",
            "ConnectionTesterStatus",

            //unity 自带编辑器GUI
            "GUI",
            "EventType",
            "EventModifiers",
            //"Event",
            "FontStyle",
            "TextAlignment",
            "TextEditor",
            "TextEditorDblClickSnapping",
            "TextGenerator",
            "TextClipping",
            "TextGenerationSettings",
            "TextAnchor",
            "TextAsset",
            "TextWrapMode",
            "Gizmos",
            "ImagePosition",
            "FocusType",
            

            //地形相关
            "Terrain",                            
            "Tree",
            "SplatPrototype",
            "DetailPrototype",
            "DetailRenderMode",

            //其他
            "MeshSubsetCombineUtility",
            "AOT",
            "Random",
            "Mathf",
            "Social",
            "Enumerator",       
            "SendMouseEvents",               
            "Cursor",
            "Flash",
            "ActionScript",
            
    
            //非通用的类
            "ADBannerView",
            "ADInterstitialAd",            
            "Android",
            "jvalue",
            "iPhone",
            "iOS",
            "CalendarIdentifier",
            "CalendarUnit",
            "CalendarUnit",
            "FullScreenMovieControlMode",
            "FullScreenMovieScalingMode",
            "Handheld",
            "LocalNotification",
            "Motion",   //空类
            "NotificationServices",
            "RemoteNotificationType",      
            "RemoteNotification",
            "SamsungTV",
            "TextureCompressionQuality",
            "TouchScreenKeyboardType",
            "TouchScreenKeyboard",
            "MovieTexture",

            //我不需要的
            //2d 类
            "AccelerationEventWrap", //加速
            "AnimatorUtility",
            "AudioChorusFilter",		
		    "AudioDistortionFilter",
		    "AudioEchoFilter",
		    "AudioHighPassFilter",		    
		    "AudioLowPassFilter",
		    "AudioReverbFilter",
		    "AudioReverbPreset",
		    "AudioReverbZone",
		    "AudioRolloffMode",
		    "AudioSettings",		    
		    "AudioSpeakerMode",
		    "AudioType",
		    "AudioVelocityUpdateMode",
            
            "Ping",
            "Profiler",
            "StaticBatchingUtility",
            "Font",
            "Gyroscope",                        //不需要重力感应
            "ISerializationCallbackReceiver",   //u3d 继承的序列化接口，lua不需要
            "ImageEffectOpaque",                //后处理
            "ImageEffectTransformsToLDR",
            "PrimitiveType",                // 暂时不需要 GameObject.CreatePrimitive           
            "Skybox",                       //不会u3d自带的Skybox
            "SparseTexture",                // mega texture 不需要
            "Plane",
            "PlayerPrefs",

            //不用ugui
            "SpriteAlignment",
		    "SpriteMeshType",
		    "SpritePackingMode",
		    "SpritePackingRotation",
		    "SpriteRenderer",
		    "Sprite",
            "UIVertex",
            "CanvasGroup",
            "CanvasRenderer",
            "ICanvasRaycastFilter",
            "Canvas",
            "RectTransform",
            "DrivenRectTransformTracker",
            "DrivenTransformProperties",
            "RectTransformAxis",
		    "RectTransformEdge",
		    "RectTransformUtility",
		    "RectTransform",
            "UICharInfo",
		    "UILineInfo",

            //不需要轮子碰撞体
            "WheelCollider",
		    "WheelFrictionCurve",
		    "WheelHit",

            //手机不适用雾
            "FogMode",

            "UnityEventBase",
		    "UnityEventCallState",
		    "UnityEvent",

            "LightProbeGroup",
            "LightProbes",

            "NPOTSupport", //只是SystemInfo 的一个枚举值

            //没用到substance纹理
            "ProceduralCacheSize",
		    "ProceduralLoadingBehavior",
		    "ProceduralMaterial",
		    "ProceduralOutputType",
		    "ProceduralProcessorUsage",
		    "ProceduralPropertyDescription",
		    "ProceduralPropertyType",
		    "ProceduralTexture",

            //物理关节系统
		    "JointDriveMode",
		    "JointDrive",
		    "JointLimits",		
		    "JointMotor",
		    "JointProjectionMode",
		    "JointSpring",
            "SoftJointLimit",
            "SpringJoint",
            "HingeJoint",
            "FixedJoint",
            "ConfigurableJoint",
            "CharacterJoint",            
		    "Joint",

            "LODGroup",
		    "LOD",

            "DataUtility",          //给sprite使用的
            "CrashReport",
            "CombineInstance",
        };

        List<BindType> list = new List<BindType>();
        Assembly assembly = Assembly.Load("UnityEngine");
        Type[] types = assembly.GetExportedTypes();

        for (int i = 0; i < types.Length; i++)
        {
            //不导出： 模版类，event委托, c#协同相关, obsolete 类
            if (!types[i].IsGenericType && types[i].BaseType != typeof(System.MulticastDelegate) &&
                !typeof(YieldInstruction).IsAssignableFrom(types[i]) && !ToLuaExport.IsObsolete(types[i]))
            {
                list.Add(_GT(types[i]));
            }
            else
            {
                Debug.Log("drop generic type " + types[i].ToString());
            }
        }

        for (int i = 0; i < dropList.Count; i++)
        {
            list.RemoveAll((p) => { return p.type.ToString().Contains(dropList[i]); });
        }

        //for (int i = 0; i < list.Count; i++)
        //{
        //    if (!typeof(UnityEngine.Object).IsAssignableFrom(list[i].type) && !list[i].type.IsEnum && !typeof(UnityEngine.TrackedReference).IsAssignableFrom(list[i].type)
        //        && !list[i].type.IsValueType && !list[i].type.IsSealed)            
        //    {
        //        Debug.Log(list[i].type.Name);
        //    }
        //}

        for (int i = 0; i < list.Count; i++)
        {
            try
            {
                ToLuaExport.Clear();
                ToLuaExport.className = list[i].name;
                ToLuaExport.type = list[i].type;
                ToLuaExport.isStaticClass = list[i].IsStatic;
                ToLuaExport.baseClassName = list[i].baseName;
                ToLuaExport.wrapClassName = list[i].wrapName;
                ToLuaExport.libClassName = list[i].libName;
                ToLuaExport.Generate(null);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Generate wrap file error: " + e.ToString());
            }
        }

        GenLuaBinder();
        Debug.Log("Generate lua binding files over， Generate " + list.Count + " files");
        AssetDatabase.Refresh();
    }
}

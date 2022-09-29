# Unity_Utility
For Utility, Helper classes in Unity.

I Recommand you to Use [UPM](https://github.com/mob-sakai/UpmGitExtension.git) Package to Use This Plugin
Compatiable with Unity version 2021 LTS

Dependencies : [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/index.html), [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html), [Bezier Curve](http://www.bansheegz.com/BGCurve/UserGuide/).

----------------------
## NameSpaces
*dkstlzu.Utility* - Main

*dkstlzu.Utility* - Main

*dkstlzu.Utility* - Main

*dkstlzu.Utility* - Main

## Runtime
*AnimationCurveHelper.cs* - retrun ReverseCurve of UnityEngine.AnimationCurve

*BezierCurve.cs* - Wrapper Class implementing generic method of [BansheeGz](http://www.bansheegz.com/BGCurve/UserGuide/)

*DevConsole.cs* - Developer Command Console for Unity using [TMPRO](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html) and [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/index.html)

*DontDestroyManager.cs* - Component adding TargetComponent as DontDestroyOnLoad

*ESCManager.cs* - Manage ESCInput action using [Unity Legacy Input System](https://docs.unity3d.com/Manual/Input.html) or [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/index.html)

*EnumSettableMonoBehaviour.cs* - MonoBehaviour which take enum type as string in unity inspector (Use with **[EnumSettableMonoBehaviourEditor]**(https://github.com/dkstlzu/Unity_Utility/blob/main/Editor/EnumSettableEditor.cs) when inherit)

***EventTrigger.cs* - Easy component using unity OnTriggerXXX events which selectable collider type at inspector**

*EventTriggerChildCollider.cs* - Extend EventTrigger to child GameObjects' colliders

*FadeEffect.cs* - FadeEffect HelperClass mainly on FadeEffect while loading scene

*LogWriter.cs* - Make .txt file and write logs using System.IO

*ObjectPool.cs* - ObjectPool in Unity which provide **Initiator feature**

*PhysicsHelper.cs* - BoxCast : Draw Debug Case on Scene View in Unity (Ref from StackOverFlow)
                   - RadialCast : Cast Circle

*Printer.cs* - Printer Helper Class in Unity provide Print Mode Division (Provide Editor ShortCut at [PrinterEditor](https://github.com/dkstlzu/Unity_Utility/blob/main/Editor/PrinterEditor.cs))

*PriorityQueue.cs* - Simple PriorityQueue which old c# does not have (Used on [ESCManager](https://github.com/dkstlzu/Unity_Utility/blob/main/Runtime/ESCManager.cs))

*ResourceLoader.cs* - Resource auto loader along with Scenes (developing)

*ResourcesExtension.cs* - Extensions of Resource.Load method

*SceneLoadCallbackSetter.cs* - UnityEngine.SceneManagement.SceneManager.\[sceneLoaded, sceneUnloaded\] Manager Class

*Singleton.cs* - Unity Singleton

*SpriteRandomSelector.cs* - Select Random Sprite OnEnable

*Tester.cs* - Inheritable Tester class offering OnGUI -> GUI.Button

*UndoableAction.cs* - Extension of System.Action which have undoable action

*UniqueComponent.cs* - Keep unity component unique with [DontDestroyManager](https://github.com/dkstlzu/Unity_Utility/blob/main/Runtime/DontDestroyManager.cs)
                        because unity component does not made by constructor but AddComponent, So [Sigleton](https://github.com/dkstlzu/Unity_Utility/blob/main/Runtime/Singleton.cs) can not limit constructor as private
                        **!using \[DefaultExecutionOrder(-100)\] so when modifing ScriptExecutionOrder in Editor be careful**
        
*UnityConsole.cs* - Helper Class controling Unity Console Message **(Wrong Position it should be in Editor folder-to be fixed)**

*trigger.cs* - use internal bool value when ref by other script 




## Editor

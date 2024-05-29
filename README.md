# Unity_Utility
For Utility, Helper classes can be used in Unity commonly.

Compatiable with Unity version 2021 LTS or later

Dependencies : [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/index.html), [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html), [Bezier Curve](http://www.bansheegz.com/BGCurve/UserGuide/).

## NameSpaces
*dkstlzu.Utility* : Main

*dkstlzu.Utility.UI* : UI Relative Components

## Runtime
AnimationCurveExtensions.cs : return ReverseCurve of UnityEngine.AnimationCurve

DebugExtensions.cs : Drawing gizmo utility for some primitive shapes

DevConsole.cs : Developer Command Console for Unity using [TMPRO](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html) and [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/index.html)

*DontDestroyManager.cs* : Component adding TargetComponent as DontDestroyOnLoad

*EnumSettableMonoBehaviour.cs* : MonoBehaviour which take enum type as string in unity inspector (Use with **[EnumSettableMonoBehaviourEditor](/Editor/EnumSettableEditor.cs)** when inherit)

**:point_right:*EventTrigger.cs* : Component using unity OnTriggerXXX events with selectable collider type on inspector:point_left:**

*EventTriggerChildCollider.cs* : Extend EventTrigger to child GameObjects' colliders

*FadeEffect.cs* : FadeEffect HelperClass mainly on FadeEffect while loading scene

*LogWriter.cs* : Make .txt file and write logs using System.IO

**:point_right:*Physics2DHelper.cs*:point_left:** : RayCast, BoxCast, CircleCast, RadialCast - Draw Debug Case on Scene View in Unity

*Printer.cs* : Printer Helper Class in Unity provide Print Mode Division (Provide Editor ShortCut at [PrinterEditor](/Editor/PrinterEditor.cs))

ListBasedPriorityQueue.cs : Simple PriorityQueue which old c# does not have (Used on [ESCManager](/Runtime/ESCManager.cs))
>[After .Net 6 Priority Queue is supported](https://learn.microsoft.com/ko-kr/dotnet/api/system.collections.generic.priorityqueue-2?view=net-6.0&viewFallbackFrom=netcore-3.1)

**:point_right:*SceneLoadCallback.cs*:point_left:** : UnityEngine.SceneManagement.SceneManager.\[sceneLoaded, sceneUnloaded\] Manager Class

*Singleton.cs* : Unity Singleton

*Tester.cs* : Inheritable Tester class offering OnGUI -> GUI.Button

*Trigger.cs* : use internal bool value when ref by other script

*UniqueComponent.cs* : Keep unity component unique with [DontDestroyManager](/Runtime/DontDestroyManager.cs)
                        since unity component does not made by constructor but AddComponent, therefore [Sigleton](/Runtime/Singleton.cs) can not limit constructor as private  
> using \[DefaultExecutionOrder(-100)\] so when modifing ScriptExecutionOrder in Editor be careful

**:point_right:*UpdateManager.cs*:point_left:** : c# object based update system for performance improvement

### - Asynchronus Folder

*Asynchronus/AsyncAwait.cs* : Invoking Actions after delayed time by async await (Invoking Func is developing)

*Asynchronus/CoroutineHelper.cs* : Invoking Actions after delayed time by Unity Coroutine (Invoking Func is not supported)

> Unity coroutine is called by Unity Life Cycle Update but async await is called by Computer Instruction Clock which means Delayed method by CoroutineHelper is paused when Editor is paused but Delayed method by async await not

*Asynchronus/CoroutineSwapper.cs* : Helper Class for swappable coroutine list and Play()

*Asynchronus/TaskManager.cs* : Wrapper Class of Unity coroutine which is Pausable, Unpausable, Startable, Stoppable and has Finished(bool) callback (Ref from StackOverFlow and modified)

### - DragUI Folder

*DragUI/CaseForDragAndDropUI.cs* : [DragAndDropableUI](/Runtime/DragUI/DragAndDropableUI.cs) which has Same ID with this can fit in this component's position

*DragUI/DragAndDroppableUI.cs* : This can fit in to [CaseForDragAndDropUI](/Runtime/DragUI/DragAndDropableUI.cs) which has Same ID with this component

*DragUI/DroppableLayoutGroup.cs* : This is case of [DragAndDropableUI](/Runtime/DragUI/DragAndDropableUI.cs) which set parent transform to target LayoutGroup

### - Enum Folder

*Enum/EnumHelper.cs* : Helper Class get Enum infos with EnumType:Type

*Enum/IntValueAttribute.cs* : Custom *int* Attribute of enum value and methods

*Enum/StringValueAttribute.cs* : Custom *string* Attribute of enum value and methods

>[After .Net 10 Generic Attribute is supported so these will be deprecated](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/generic-attributes)

### - EventSystem Folder

>No Project use this system, was used on project which is doomed already
*EventSystem/IEvent.cs* : Base Interface of Event used on EventSystem

*EventSystem/Event.cs* : Base Class of Event used on EventSystem

*EventSystem/EventDispatcher.cs* : Basic implementation of [IEventDispatcher](/Runtime/EventSystem/IEventDispatcher.cs) used on EventSystem

*EventSystem/EventSystem.cs* : Unity Component managing this EventSystem (Internal Dictionary should be replaced as [SerializableDictionary](/Runtime/Serializables/SerializableDictionary.cs))

### - ObjectPool Folder

*ObjectPool/GameObjectPoolMonoBehaviour.cs* : GameObjectPool Setter Component

*ObjectPool/ObjectPool.cs* : ObjectPool in Unity which instantiate objects disabled on init

*ObjectPool/ObjectPoolMonoBehaviour.cs* : Show comprehensive information of ObjectPools

*ObjectPool/PooedObject.cs* : Indicator for attached GameObject is Object-Pooled object

### - Serializables Folder

*Serializables/SerializableColor.cs* : SerializableColor class for Serialization which compatible with UnityEngine.Color

*Serializables/SerializableVector3.cs* : SerializableVector3 class for Serialization which compatible with UnityEngine.\[Vector3, Vector2\]

### - Sound Folder

*Sound/SoundArgs.cs* : ScriptableObject containing Argument used on [SoundManager](/Runtime/Sound/SoundManager.cs)

*Sound/WorldSoundManager.cs* : Unity Component Managing to play Sound in Unity using UnityEngine.AudioSource using [ObjectPool](/Runtime/ObjectPool/ObjectPool.cs)

### - ThirdParties Folder
>Depedencies

*BezierCurve.cs* : Wrapper Class implementing generic method of [BansheeGz](http://www.bansheegz.com/BGCurve/UserGuide/)

*ThirdParties/BansheeGz* : [BansheeGz library](http://www.bansheegz.com/BGCurve/UserGuide/) on Dependencies

### - UI Folder

*UI/ESCManager.cs* : Unity Component managing ESCAction specifically for UIs

*UI/ObjectOpenClose.cs* : Unity Component using [UIHelper](/Runtime/UI/UIHelper.cs) => Has four CallBack

``` c#
public event Action OnOpen;
public event Action OnClose;
public event Action AfterOpen;
public event Action AfterClose;
```

*UI/RatioLayoutGroup.cs* : Unity Editor Tool Component used on arranging Child Objects **(Must not be used on runtime now)**

*UI/UIHelper.cs* : HelperClass implementing open, close effect of Unity GameObject using AnimationCurve, [TaskManagerTask](/Runtime/Asynchronus/TaskManager.cs)

## Editor

\[CustomEditor of [DontDestroyManager](/Runtime/DontDestroyManager.cs)\]  
*DontDestroyManagerEditor.cs* : Expose 'Use UniqueComponent' button to connect with [UniqueComponent](/Runtime/UniqueComponent.cs)

*EditPrefab.cs* : Open new scene for edit prefab and save it over to original prefab object
                  - which can handle the problem of editor script effect not applied to prefab because of serialization system of unity (ref from StackOverFlow and modified)

\[CustomEditor of [EnumSettalbeMonoBehaviour](/Runtime/EnumSettalbeMonoBehaviour.cs)\]  
:point_right:***EnumSettableEditor.cs* : Implementing dynamic enum type determination**:point_left:

\[CustomEditor of [EventTrigger](/Runtime/EventTrigger.cs)\]  
:point_right:***EventTriggerEditor.cs* : Offer selection of collider type**:point_left:

ObjectOpenCloseEditor.cs : Offer trigger-able buttons in editor

*Physics2DHelper.cs* : Turn on and off [Physics2DHelper](/Runtime/Physics2DHelper.cs) gizmos

*PrinterEditor.cs* : Switch [Printer](/Runtime/Printer.cs) modes and provide shortcut

*SceneCallbackHandlerPropertyDrawer.cs* : Offer comfortable asset referencable feature without typing name in string of [SceneCallback](/Runtime/SceneCallback.cs)

\[CustomEditor of [UniqueComponent](/Runtime/UniqueComponent.cs)\]  
*UniqueComponentEditor.cs* : Expose HashID only when Unique Method is HashID

### - BansheeGz Folder
> ThirdParty Editor (Do not touch without error)

### - Utility Folder
*Utility/EditorInspectorUtility.cs* : Helper Class providing GroupPropertyField used on Editor.OnInspectorGUI()

*Utility/GizmoHelper.cs* : Helper Class for DrawString on scene view in UnityEditor

*Utility/SerializedPropertyDebug.cs* : Helper Class for UnityEditor.SerializedProperty and UnityEditor.SerializedObject

*Utility/UnityConsole.cs* : Helper Class controlling Unity Console Message
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace dkstlzu.Utility
{
    public class FrameAnimatorEditorWindow : EditorWindow
    {
        enum EditBoxType
        {
            Body,
            Hit,
            Attack
        }
        
        class FrameData
        {
            public int SpriteIndex;
            public Sprite Sprite;
            public List<Rect> BodyBox;
            public List<Rect> HitBox;
            public List<Rect> AttackBox;

            public FrameData(int spriteIndex, Sprite sprite) : this(spriteIndex, sprite, Array.Empty<Rect>(), Array.Empty<Rect>(), Array.Empty<Rect>())
            {
                
            }

            public FrameData(FrameData data) : this(data.SpriteIndex, data.Sprite, data.BodyBox, data.HitBox, data.AttackBox)
            {
                
            }

            public FrameData(int spriteIndex, Sprite sprite, IEnumerable<Rect> bodyBox, IEnumerable<Rect> hitBox, IEnumerable<Rect> attackBox)
            {
                SpriteIndex = spriteIndex;
                Sprite = sprite;
                BodyBox = new List<Rect>(bodyBox);
                HitBox = new List<Rect>(hitBox);
                AttackBox = new List<Rect>(attackBox);
            }

            public override string ToString() => $"SpriteIndex : {SpriteIndex}, Sprite : {Sprite}, BodyBoxCount : {BodyBox.Count}, HitBoxCount : {HitBox.Count}, AttackBoxCount : {AttackBox.Count}";
        }
        
        public VisualTreeAsset Asset;
        public FrameAnimatorEditorWindowConfigScriptable Setting;
        public Sprite NoSpriteIndicator => Setting.NoSpriteIndicator;
        
        private List<RectElement> _bodyBoxElementList = new List<RectElement>();
        private List<RectElement> _hitBoxElementList = new List<RectElement>();
        private List<RectElement> _attackBoxElementList = new List<RectElement>();
        private int _bodyBoxPoolSize => Setting.BodyBoxPoolSize;
        private int _hitBoxPoolSize => Setting.HitBoxPoolSize;
        private int _attackBoxPoolSize => Setting.AttackBoxPoolSize;
        private EditBoxType _currentEditBox;
        private VisualElement _bodyBoxEditingIndicator;
        private VisualElement _hitBoxEditingIndicator;
        private VisualElement _attackBoxEditingIndicator;
        private VisualElement _autoSaveIndicator;
        
        private Label _noSpriteIndicator;
        private Label _currentFrameNumberLabel;
        private Label _currentSpriteNumberLabel;

        private bool _bodyBoxOn = true;
        private bool _hitBoxOn = true;
        private bool _attackBoxOn = true;
        private bool _colorSelectorOn = false;
        private EditorCoroutine _autoSaveCoroutine;

        public Color BodyBoxColor => Setting.BodyBoxColor;
        public Color HitBoxColor => Setting.HitBoxColor;
        public Color AttackBoxColor => Setting.AttackBoxColor;

        private FrameAnimationInfoScriptable _targetAsset;
        private int _currentSpriteIndex;
        private int m_currentFrame;
        private int _currentFrame
        {
            get => m_currentFrame;
            set
            {
                m_currentFrame = value;
                EditorPrefs.SetInt(_TARGET_FRAME_PREF_KEY, m_currentFrame);
            }
        }
        public int AnimationFPS => Setting.AnimationFPS;
        
        private Image spriteImage;
        private VisualElement _renderingVisualElement;
        private float _renderingVEStretchRatio;
        private VisualElement _pivotVisualElement;
        private ObjectField _spriteField;
        private Button _playButton;
        private Button _pauseButton;
        private Button _removeButton;
        private Button _stopButton;

        private bool _isPlaying = false;
        private EditorCoroutine _animationCoroutine;

        private static bool _isOpened => HasOpenInstances<FrameAnimatorEditorWindow>();
        private static bool _isFocused => focusedWindow is FrameAnimatorEditorWindow;

        private List<FrameData> _frameDatas = new List<FrameData>();
        private FrameData _currentFrameData => FrameDataIsValid ? _frameDatas[_currentFrame] : null;
        private FrameData _previousFrameData => FrameDataIsValid && _currentFrame > 0 ? _frameDatas[_currentFrame-1] : null;
        private FrameData _nextFrameData => FrameDataIsValid && _currentFrame < _maxFrameNumber ? _frameDatas[_currentFrame+1] : null;

        private const int _INVALID_INDEX_NUM = -1;
        private int _maxFrameNumber => FrameDataIsValid ? _frameDatas.Count-1 : _INVALID_INDEX_NUM;
        private int _maxSpriteIndex => FrameDataIsValid ? _frameDatas[^1].SpriteIndex : _INVALID_INDEX_NUM;
        private bool FrameDataIsValid => _frameDatas.Count > 0 && _frameDatas[0].SpriteIndex != _INVALID_INDEX_NUM;
        private bool CurrentFrameIsValid => _currentFrame >= 0 && _currentFrame <= _maxFrameNumber;

        private const string _FRAME_DEV_TOOL_TAG = "FrameDevTool";
        private const string _EDIT_BOX_PREF_KEY = "FrameDevToolEditingBoxType";
        private const string _DEFAULT_CONFIG_PATH_PREF_KEY = "FrameDevToolDefaultConfigPath";
        private const string _TARGET_ASSET_PREF_KEY = "FrameDevToolTarget";
        private const string _TARGET_FRAME_PREF_KEY = "FrameDevToolTargetFrame";
        
        [MenuItem("Tools/FrameAnimation/Edit Window")]
        static void ShowEditor()
        {
            var window = GetWindow<FrameAnimatorEditorWindow>();
            window.titleContent = new GUIContent("FrameAnimator Editor Window");
            window.minSize = new Vector2(100, 100);
        }
        
        private void CreateGUI() 
        {
            Asset.CloneTree(rootVisualElement);

            if (Setting == null)
            {
                Setting = GetDefaultConfig();
            }

            SetShortCutManager();

            _autoSaveIndicator = rootVisualElement.Q("AutoSaveIndicator");
            spriteImage = new Image();
            spriteImage.RegisterCallback<ChangeEvent<Sprite>>(OnSpriteImageChanged);
            spriteImage.StretchToParentSize();
            _pivotVisualElement = rootVisualElement.Q("PivotElement");
            spriteImage.Add(_pivotVisualElement);
            _renderingVisualElement = rootVisualElement.Q("Rendering");
            _renderingVisualElement.Add(spriteImage);
            
            _noSpriteIndicator = rootVisualElement.Q<Label>("NoSpriteIndicator");
            rootVisualElement.Q<ObjectField>("AssetReferenceField").objectType = typeof(FrameAnimationInfoScriptable);
            _currentFrameNumberLabel = rootVisualElement.Q<Label>("CurrentFrameNumber");
            _currentSpriteNumberLabel = rootVisualElement.Q<Label>("CurrentSpriteNumber");
            _spriteField = rootVisualElement.Q<ObjectField>("SpriteField");
            
            _playButton = rootVisualElement.Q<Button>("PlayButton");
            _pauseButton = rootVisualElement.Q<Button>("PauseButton");
            _removeButton = rootVisualElement.Q<Button>("RemoveButton");
            _stopButton = rootVisualElement.Q<Button>("StopButton");
            
            CreateCollisionBoxPool();
            RegisterCallbacks();

            string targetPath = EditorPrefs.GetString(_TARGET_ASSET_PREF_KEY);
            var previousTarget = AssetDatabase.LoadAssetAtPath<FrameAnimationInfoScriptable>(targetPath);
            if (previousTarget)
            {
                rootVisualElement.Q<ObjectField>("AssetReferenceField").value = previousTarget;
            }
        } 

        FrameAnimatorEditorWindowConfigScriptable GetDefaultConfig()
        {
            FrameAnimatorEditorWindowConfigScriptable defaultConfig;
            
            if (!EditorPrefs.HasKey(_DEFAULT_CONFIG_PATH_PREF_KEY))
            {
                defaultConfig = CreateInstance<FrameAnimatorEditorWindowConfigScriptable>();

                string settingDirectoryPath = Path.Combine(Application.dataPath, "Settings");
                if (!Directory.Exists(settingDirectoryPath))
                {
                    Directory.CreateDirectory(settingDirectoryPath);
                }

                string defaultConfigPath = "Assets/Settings/DefaultFrameDevToolConfig.asset";
                AssetDatabase.CreateAsset(defaultConfig, defaultConfigPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorPrefs.SetString(_DEFAULT_CONFIG_PATH_PREF_KEY, defaultConfigPath);
            }
            else
            {
                defaultConfig = AssetDatabase.LoadAssetAtPath<FrameAnimatorEditorWindowConfigScriptable>(EditorPrefs.GetString(_DEFAULT_CONFIG_PATH_PREF_KEY));
            }

            return defaultConfig;
        }

        void SetShortCutManager()
        {
            ShortcutManager.RegisterTag(_FRAME_DEV_TOOL_TAG);

            rootVisualElement.RegisterCallback<MouseMoveEvent>((e) => GetMousePosition(e));
        }

        private Vector2 _currentMousePosition;
        private Vector2 GetMousePosition(MouseMoveEvent evt = null)
        {
            if (evt != null)
            {
                _currentMousePosition = evt.mousePosition;
            }

            return _currentMousePosition;
        }

        private void CreateCollisionBoxPool()
        {
            for (int i = 0; i < _bodyBoxPoolSize; i++)
            {
                RectElement rect = new RectElement();
                rect.style.display = DisplayStyle.None;
                rect.SetDraggableAreaVisualElement(_renderingVisualElement);
                _pivotVisualElement.Add(rect);
                _bodyBoxElementList.Add(rect);
            }

            for (int i = 0; i < _hitBoxPoolSize; i++)
            {
                RectElement rect = new RectElement();
                rect.style.display = DisplayStyle.None;
                rect.SetDraggableAreaVisualElement(_renderingVisualElement);
                rect.SetColor(HitBoxColor, Color.Lerp(HitBoxColor, Color.black, 0.1f), Color.Lerp(HitBoxColor, Color.black, 0.2f));
                _pivotVisualElement.Add(rect);
                _hitBoxElementList.Add(rect);
            }

            for (int i = 0; i < _attackBoxPoolSize; i++)
            {
                RectElement rect = new RectElement();
                rect.style.display = DisplayStyle.None;
                rect.SetDraggableAreaVisualElement(_renderingVisualElement);
                rect.SetColor(AttackBoxColor, Color.Lerp(AttackBoxColor, Color.black, 0.1f), Color.Lerp(AttackBoxColor, Color.black, 0.2f));
                _pivotVisualElement.Add(rect);
                _attackBoxElementList.Add(rect);
            }

            _bodyBoxEditingIndicator = rootVisualElement.Q("BodyBoxSelectionIndicator");
            _hitBoxEditingIndicator = rootVisualElement.Q("HitBoxSelectionIndicator");
            _attackBoxEditingIndicator = rootVisualElement.Q("AttackBoxSelectionIndicator");

            var editingBoxType = EditorPrefs.GetString(_EDIT_BOX_PREF_KEY, default(EditBoxType).ToString());
            
            SetEditingBox(Enum.Parse<EditBoxType>(editingBoxType));
        }
        
        void RegisterCallbacks()
        {
            rootVisualElement.Q<Button>("BodyToggleButton").RegisterCallback<ClickEvent>(OnBodyToggle);
            rootVisualElement.Q<Button>("HitToggleButton").RegisterCallback<ClickEvent>(OnHitToggle);
            rootVisualElement.Q<Button>("AttackToggleButton").RegisterCallback<ClickEvent>(OnAttackToggle);
            rootVisualElement.Q<Button>("ColorSelectorButton").RegisterCallback<ClickEvent>(OnColorSelectorClicked);
            rootVisualElement.Q<Toggle>("AutoSaveToggle").RegisterValueChangedCallback(OnAutoSaveToggle);
            rootVisualElement.Q<Button>("SaveAssetButton").RegisterCallback<ClickEvent>(OnSaveButtonClicked);
            rootVisualElement.Q<ObjectField>("AssetReferenceField").RegisterValueChangedCallback(OnAssetSelected);
            rootVisualElement.Q<ObjectField>("SpriteField").RegisterValueChangedCallback(OnSpriteSelected);
            
            rootVisualElement.Q<Button>("PreviousCopyButton").RegisterCallback<ClickEvent>(OnPreviousCopyButtonClicked);
            rootVisualElement.Q<Button>("PreviousImageButton").RegisterCallback<ClickEvent>(OnPreviousImageButtonClicked);
            rootVisualElement.Q<Button>("PreviousFrameButton").RegisterCallback<ClickEvent>(OnPreviousFrameButtonClicked);
            rootVisualElement.Q<Button>("PlayButton").RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            rootVisualElement.Q<Button>("PauseButton").RegisterCallback<ClickEvent>(OnPauseButtonClicked);
            rootVisualElement.Q<Button>("StopButton").RegisterCallback<ClickEvent>(OnStopButtonClicked);
            rootVisualElement.Q<Button>("RemoveButton").RegisterCallback<ClickEvent>(OnRemoveButtonClicked);
            rootVisualElement.Q<Button>("NextFrameButton").RegisterCallback<ClickEvent>(OnNextFrameButtonClicked);
            rootVisualElement.Q<Button>("NextImageButton").RegisterCallback<ClickEvent>(OnNextImageButtonClicked);
            rootVisualElement.Q<Button>("NextCopyButton").RegisterCallback<ClickEvent>(OnNextCopyButtonClicked);
            
            _renderingVisualElement.RegisterCallback<DragUpdatedEvent>(OnBoxRectDrag);
        }

        private void OnDestroy()
        {
            if (_animationCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_animationCoroutine);
            }
            
            ShortcutManager.UnregisterTag(_FRAME_DEV_TOOL_TAG);
        }
        
        private void Update()
        {
            _currentFrameNumberLabel.text = _currentFrame.ToString();
            _currentSpriteNumberLabel.text = _currentSpriteIndex.ToString();
            spriteImage.SetImageFitableVisualElement();
            if (spriteImage.sprite != null)
            {
                _renderingVEStretchRatio = spriteImage.layout.height / spriteImage.sprite.rect.height;
            }
        }

        #region Callbacks
        private void OnBodyToggle(ClickEvent evt)
        {
            SetBodyBoxOn(!_bodyBoxOn);
        }
        
        private void OnHitToggle(ClickEvent evt)
        {
            SetHitBoxOn(!_hitBoxOn);
        }
        
        private void OnAttackToggle(ClickEvent evt)
        {
            SetAttackBoxOn(!_attackBoxOn);
        }
        
        private void OnColorSelectorClicked(ClickEvent evt)
        {
            _colorSelectorOn = !_colorSelectorOn;
        }
        
        private void OnAutoSaveToggle(ChangeEvent<bool> evt)
        {
            IEnumerator AutoSaveIndicator()
            {
                _autoSaveIndicator.visible = true;
                yield return new EditorWaitForSeconds(2);
                _autoSaveIndicator.visible = false;
            }
            
            IEnumerator AutoSave()
            {
                EditorWaitForSeconds autoSaveInterval = new EditorWaitForSeconds(Setting.AutoSaveInterval);

                while (true)
                {
                    yield return autoSaveInterval;
                    if (_targetAsset != null)
                    {
                        OnSaveButtonClicked(null);
                        EditorCoroutineUtility.StartCoroutineOwnerless(AutoSaveIndicator());
                    }
                }
            }

            if (evt.newValue)
            {
                _autoSaveCoroutine = EditorCoroutineUtility.StartCoroutine(AutoSave(), this);
            }
            else
            {
                if (_autoSaveCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_autoSaveCoroutine);
                }
            }
        }
        
        private void OnSaveButtonClicked(ClickEvent evt)
        {
            if (_targetAsset == null) return;

            FrameAnimationSprites spritesAsset = _targetAsset.Sprites;
            FrameAnimationFrames framesAsset = _targetAsset.Frames;
            RectsScriptable bodyRectsAsset = _targetAsset.BodyBoxRects;
            RectsScriptable hitRectsAsset = _targetAsset.HitBoxRects;
            RectsScriptable attackRectsAsset = _targetAsset.AttackBoxRects;

            string targetAssetDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_targetAsset));
            
            Debug.Assert(targetAssetDirectory != null);
            
            if (spritesAsset == null)
            {
                spritesAsset = CreateInstance<FrameAnimationSprites>();
                AssetDatabase.CreateAsset(spritesAsset, Path.Combine(targetAssetDirectory, $"{_targetAsset.name}_Sprites.asset"));
            }

            if (framesAsset == null)
            {
                framesAsset = CreateInstance<FrameAnimationFrames>();
                AssetDatabase.CreateAsset(framesAsset, Path.Combine(targetAssetDirectory, $"{_targetAsset.name}_Frames.asset"));
            }
            
            if (bodyRectsAsset == null)
            {
                bodyRectsAsset = CreateInstance<RectsScriptable>();
                AssetDatabase.CreateAsset(bodyRectsAsset, Path.Combine(targetAssetDirectory, $"{_targetAsset.name}_BodyBoxes.asset"));
            }

            if (hitRectsAsset == null)
            {
                hitRectsAsset = CreateInstance<RectsScriptable>();
                AssetDatabase.CreateAsset(hitRectsAsset, Path.Combine(targetAssetDirectory, $"{_targetAsset.name}_HitBoxes.asset"));
            }
            
            if (attackRectsAsset == null)
            {
                attackRectsAsset = CreateInstance<RectsScriptable>();
                AssetDatabase.CreateAsset(attackRectsAsset, Path.Combine(targetAssetDirectory, $"{_targetAsset.name}_AttackBoxes.asset"));
            }
            
            Assert.IsNotNull(spritesAsset);
            Assert.IsNotNull(framesAsset);
            Assert.IsNotNull(bodyRectsAsset);
            Assert.IsNotNull(hitRectsAsset);
            Assert.IsNotNull(attackRectsAsset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _targetAsset.Sprites = spritesAsset;
            _targetAsset.Frames = framesAsset;
            _targetAsset.BodyBoxRects = bodyRectsAsset;
            _targetAsset.HitBoxRects = hitRectsAsset;
            _targetAsset.AttackBoxRects = attackRectsAsset;

            bodyRectsAsset.Sequences.Clear();
            hitRectsAsset.Sequences.Clear();
            attackRectsAsset.Sequences.Clear();

            List<Sprite> spriteList = new List<Sprite>();
            List<int> frameList = new List<int>();
            Sprite previousSprite = _frameDatas[0].Sprite;
            int lastSpriteFrame = 0;
            
            List<Rect> bodyRects = new List<Rect>();
            List<Rect> hitRects = new List<Rect>();
            List<Rect> attackRects = new List<Rect>();
            
            for (int i = 1; i <= _maxFrameNumber; i++)
            {
                bodyRects.Clear();
                hitRects.Clear();
                attackRects.Clear();

                for (int j = 0; j < _frameDatas[i].BodyBox.Count; ++j)
                {
                    bodyRects.Add(divideRect(_frameDatas[i].BodyBox[j], _frameDatas[i].Sprite.pixelsPerUnit));
                }
                
                for (int j = 0; j < _frameDatas[i].HitBox.Count; ++j)
                {
                    hitRects.Add(divideRect(_frameDatas[i].HitBox[j], _frameDatas[i].Sprite.pixelsPerUnit));
                }
                
                for (int j = 0; j < _frameDatas[i].AttackBox.Count; ++j)
                {
                    attackRects.Add(divideRect(_frameDatas[i].AttackBox[j], _frameDatas[i].Sprite.pixelsPerUnit));
                }
                
                bodyRectsAsset.Sequences.Add(new List<Rect>(bodyRects));
                hitRectsAsset.Sequences.Add(new List<Rect>(hitRects));
                attackRectsAsset.Sequences.Add(new List<Rect>(attackRects));
                
                if (_frameDatas[i].Sprite == previousSprite)
                {
                    ++lastSpriteFrame;
                }
                else
                {
                    spriteList.Add(previousSprite);
                    frameList.Add(lastSpriteFrame);
                    lastSpriteFrame = 1;
                    previousSprite = _frameDatas[i].Sprite;
                }
            }
            
            spriteList.Add(_frameDatas[_maxFrameNumber].Sprite);
            frameList.Add(lastSpriteFrame);
            
            spritesAsset.Sequences = spriteList.ToArray();
            framesAsset.Sequences = frameList.ToArray();
            
            EditorUtility.SetDirty(spritesAsset);
            EditorUtility.SetDirty(framesAsset);
            EditorUtility.SetDirty(bodyRectsAsset);
            EditorUtility.SetDirty(hitRectsAsset);
            EditorUtility.SetDirty(attackRectsAsset);
            
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spritesAsset));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(framesAsset));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(bodyRectsAsset));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(hitRectsAsset));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(attackRectsAsset));
        }
        
        private void OnAssetSelected(ChangeEvent<Object> evt)
        {
            _targetAsset = evt.newValue as FrameAnimationInfoScriptable;
            _frameDatas.Clear();

            if (_targetAsset == null)
            {
                _frameDatas.Add(new FrameData(_INVALID_INDEX_NUM, NoSpriteIndicator));
                SetFrame(0);
                EditorPrefs.SetString(_TARGET_ASSET_PREF_KEY, "");
                return;
            }
            
            _frameDatas.Add(new FrameData(_targetAsset.GetSpriteIndexOfFrame(0), _targetAsset.GetSpriteOfIndex(0)));
            
            List<Rect> pixelBodyRects = new List<Rect>();
            List<Rect> pixelHitRects = new List<Rect>();
            List<Rect> pixelAttackRects = new List<Rect>();
            
            for (int i = 1; i <= _targetAsset.TotalFrameNum; i++)
            {
                int spriteIndex = _targetAsset.IsValid() ? _targetAsset.GetSpriteIndexOfFrame(i) : _INVALID_INDEX_NUM;
                Sprite sprite = _targetAsset.IsValid() ? _targetAsset.GetSpriteOfFrame(i) : NoSpriteIndicator;
                
                pixelBodyRects.Clear();
                pixelHitRects.Clear();
                pixelAttackRects.Clear();

                var realBodyRects = _targetAsset.GetBodyRectsOfFrame(i);
                var realHitRects = _targetAsset.GetHitRectsOfFrame(i);
                var realAttackRects = _targetAsset.GetAttackRectsOfFrame(i);
                
                for (int j = 0; j < realBodyRects.Length; ++j)
                {
                    pixelBodyRects.Add(multiplyRect(realBodyRects[j], sprite.pixelsPerUnit));
                }
                
                for (int j = 0; j < realHitRects.Length; ++j)
                {
                    pixelHitRects.Add(multiplyRect(realHitRects[j], sprite.pixelsPerUnit));
                }
                
                for (int j = 0; j < realAttackRects.Length; ++j)
                {
                    pixelAttackRects.Add(multiplyRect(realAttackRects[j], sprite.pixelsPerUnit));
                }
                
                _frameDatas.Add(new FrameData(spriteIndex, sprite, pixelBodyRects, pixelHitRects, pixelAttackRects));
            }
            
            if (AssetDatabase.GetAssetPath(_targetAsset) == EditorPrefs.GetString(_TARGET_ASSET_PREF_KEY))
            {
                SetFrame(EditorPrefs.GetInt(_TARGET_FRAME_PREF_KEY, 0));
            }
            else
            {
                SetFrame(0);
            } 
            
            EditorPrefs.SetString(_TARGET_ASSET_PREF_KEY, AssetDatabase.GetAssetPath(_targetAsset));
        }
        
        private void OnSpriteSelected(ChangeEvent<Object> evt)
        {
            Sprite newSprite = evt.newValue as Sprite;

            if (_currentFrame == 0)
            {
                Printer.Print($"FrameAnimation Editor에서 0번째 sprite를 수정하는것은 불가능합니다.", logLevel:LogLevel.Warning);
                _spriteField.SetValueWithoutNotify(evt.previousValue);
                return;
            }
            
            if (_currentFrame <= _maxFrameNumber)
            {
                // 이미 존재하는 FrameData를 수정
                
                _currentFrameData.Sprite = newSprite;

                if (_currentFrame > 0)
                {
                    // 이전 프레임이 있을경우
                    if (_currentFrame == 1)
                    {
                        _frameDatas[0].Sprite = newSprite;
                    }
                    
                    if (_previousFrameData.Sprite == newSprite)
                    {
                        _currentFrameData.SpriteIndex = _previousFrameData.SpriteIndex;
                    }
                    else
                    {
                        _currentFrameData.SpriteIndex = _previousFrameData.SpriteIndex + 1;
                    }
                }
                
                if (_currentFrame < _maxFrameNumber)
                {
                    // 다음 프레임이 있을경우 다음 프레임부터 마지막 프레임까지 SpriteIndex 증가
                    int indexDelta = 0;

                    if (_nextFrameData.Sprite == newSprite)
                    {
                        indexDelta = _currentFrameData.SpriteIndex - _nextFrameData.SpriteIndex;
                    }
                    else
                    {
                        indexDelta = _currentFrameData.SpriteIndex - _nextFrameData.SpriteIndex + 1;
                    }

                    for (int i = _currentFrame+1; i <= _maxFrameNumber; i++)
                    {
                        _frameDatas[i].SpriteIndex += indexDelta;
                    }
                } else if (_currentFrame == _maxFrameNumber)
                {
                    for (int i = _frameDatas.Count-1; i > 0; i--)
                    {
                        if (_frameDatas[i].Sprite == NoSpriteIndicator)
                        {
                            _frameDatas.RemoveAt(i);
                            if (i == 1)
                            {
                                _frameDatas[0].SpriteIndex = _INVALID_INDEX_NUM;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                // 이미 존재하지 않던 FrameData를 추가
                if (FrameDataIsValid)
                {
                    int previousMaxSpriteIndex = _maxSpriteIndex;
                
                    for (int i = _maxFrameNumber+1; i < _currentFrame; i++)
                    {
                        _frameDatas.Add(new FrameData(previousMaxSpriteIndex+1, NoSpriteIndicator));
                    }

                    if (_previousFrameData.Sprite == newSprite)
                    {
                        _frameDatas.Add(new FrameData(_maxSpriteIndex, newSprite));
                    }
                    else
                    {
                        _frameDatas.Add(new FrameData(_maxSpriteIndex+1, newSprite));
                    }
                }
                else
                {
                    _frameDatas[0].SpriteIndex = 0;

                    if (_currentFrame == 1)
                    {
                        _frameDatas[0].Sprite = newSprite;
                        _frameDatas.Add(new FrameData(0, newSprite));
                    }
                    else
                    {
                        for (int i = 1; i < _currentFrame; i++)
                        {
                            _frameDatas.Add(new FrameData(0, NoSpriteIndicator));
                        }
                        
                        _frameDatas.Add(new FrameData(1, newSprite));
                    }
                }
            }
            
            SetFrame(_currentFrame);
        }
        
        private void OnSpriteImageChanged(ChangeEvent<Sprite> evt)
        {
            Sprite newSprite = evt.newValue;

            if (newSprite == NoSpriteIndicator || newSprite == null)
            {
                _pivotVisualElement.SwitchDisplay(false);
                return;
            }

            _pivotVisualElement.SwitchDisplay(true);

            float xPivotPercent = newSprite.pivot.x / newSprite.rect.width * 100;
            float yPivotPercent = newSprite.pivot.y / newSprite.rect.height * 100;
            _pivotVisualElement.SetPositionWithPercent(new Vector2(xPivotPercent, 100 - yPivotPercent));
        }

        private void OnPreviousCopyButtonClicked(ClickEvent evt)
        {
            if (!CurrentFrameIsValid) return;
            
            _frameDatas.Insert(_currentFrame, new FrameData(_currentFrameData));
            SetFrame(_currentFrame);
        }
        
        private void OnPreviousImageButtonClicked(ClickEvent evt)
        {
            SetSpriteIndex(_currentSpriteIndex - 1);
            OnPauseButtonClicked(evt);
        }
        
        private void OnPreviousFrameButtonClicked(ClickEvent evt)
        {
            SetFrame(_currentFrame - 1);
            OnPauseButtonClicked(evt);
        }
        
        private void OnPlayButtonClicked(ClickEvent evt)
        {
            if (_targetAsset == null) return;
            
            _isPlaying = true;

            _playButton.SwitchDisplay(false);
            _pauseButton.SwitchDisplay(true);
            _removeButton.SwitchDisplay(false);
            _stopButton.SwitchDisplay(true);
            
            _animationCoroutine = EditorCoroutineUtility.StartCoroutine(PlayAnimationCoroutine(), this);

            IEnumerator PlayAnimationCoroutine()
            {
                if (_targetAsset == null) yield break;

                var waitFixedTime = new EditorWaitForSeconds(1/(float)AnimationFPS);
            
                while (true)
                {
                    yield return waitFixedTime;
                    if (_currentFrame >= _targetAsset.TotalFrameNum)
                    {
                        SetFrame(0);
                    }
                    else
                    {
                        SetFrame(++_currentFrame);
                    }
                }
            }
        }

        private void OnPauseButtonClicked(ClickEvent evt)
        {
            if (_targetAsset == null) return;

            _isPlaying = false;

            _playButton.SwitchDisplay(true);
            _pauseButton.SwitchDisplay(false);
            _removeButton.SwitchDisplay(true);
            _stopButton.SwitchDisplay(false);
            
            if (_animationCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_animationCoroutine);
            }
        }

        private void OnStopButtonClicked(ClickEvent evt)
        {
            OnPauseButtonClicked(evt);
            SetFrame(0);
        }

        private void OnRemoveButtonClicked(ClickEvent evt)
        {
            if (!CurrentFrameIsValid) return;

            _frameDatas.RemoveAt(_currentFrame);
            SetFrame(_currentFrame);
        }

        private void OnNextFrameButtonClicked(ClickEvent evt)
        {
            SetFrame(_currentFrame + 1);
            OnPauseButtonClicked(evt);
        }

        private void OnNextImageButtonClicked(ClickEvent evt)
        {
            SetSpriteIndex(_currentSpriteIndex + 1);
            OnPauseButtonClicked(evt);
        }

        private void OnNextCopyButtonClicked(ClickEvent evt)
        {
            if (!CurrentFrameIsValid) return;

            _frameDatas.Insert(_currentFrame, new FrameData(_currentFrameData));
            SetFrame(_currentFrame+1);
        }
        
        private void OnBoxRectDrag(DragUpdatedEvent evt)
        {
            RectElement draggingRect = DragAndDrop.GetGenericData(RectElement.DRAGANDDROP_DATA_KEY) as RectElement;
            
            if (draggingRect == null) return;

            if (_bodyBoxElementList.Contains(draggingRect))
            {
                int index = _bodyBoxElementList.IndexOf(draggingRect);

                _currentFrameData.BodyBox[index] = divideRect(_bodyBoxElementList[index].Rect, _renderingVEStretchRatio);
            }

            if (_hitBoxElementList.Contains(draggingRect))
            {
                int index = _hitBoxElementList.IndexOf(draggingRect);

                _currentFrameData.HitBox[index] = divideRect(_hitBoxElementList[index].Rect, _renderingVEStretchRatio);
            }

            if (_attackBoxElementList.Contains(draggingRect))
            {
                int index = _attackBoxElementList.IndexOf(draggingRect);

                _currentFrameData.AttackBox[index] = divideRect(_attackBoxElementList[index].Rect, _renderingVEStretchRatio);
            }
        }
        #endregion

        #region Image Control
        private void SetFrame(int frame)
        {
            if (frame < 0) return;
            
            _currentFrame = frame;
            if (_currentFrame == 0 && !FrameDataIsValid)
            {
                _currentSpriteIndex = 0;
                SetSpriteImageWithNoSpriteIndicator();
            } else if (_currentFrame <= _maxFrameNumber)
            {
                _currentSpriteIndex = _currentFrameData.SpriteIndex;
                SetSpriteImageWithCurrentFrameData();
            }
            else
            {
                _currentSpriteIndex = _maxSpriteIndex + 1;
                SetSpriteImageWithNoSpriteIndicator();
            }
        }

        private void SetSpriteIndex(int index)
        {
            if (_targetAsset == null) return;
            if (index < 0) return;

            if (!FrameDataIsValid)
            {
                _currentSpriteIndex = 0;
                _currentFrame = 1;
                
                SetSpriteImageWithNoSpriteIndicator();
            } else if (index == 0)
            {
                _currentSpriteIndex = 0;
                _currentFrame = 1;

                SetSpriteImageWithCurrentFrameData();
            } else if (index <= _maxSpriteIndex)
            {
                _currentSpriteIndex = index;
                for (int i = 0; i < _frameDatas.Count; i++)
                {
                    if (_frameDatas[i].SpriteIndex == index)
                    {
                        _currentFrame = i;
                        break;
                    }
                }
                
                SetSpriteImageWithCurrentFrameData();
            }
            else
            {
                _currentSpriteIndex = _maxSpriteIndex + 1;
                _currentFrame = _maxFrameNumber + 1;
                
                SetSpriteImageWithNoSpriteIndicator();
            }
        }

        void SetSpriteImageWithCurrentFrameData()
        {
            SetSpriteImage(_currentFrameData.Sprite);
            _noSpriteIndicator.SwitchDisplay(false);
            _spriteField.SetValueWithoutNotify(spriteImage.sprite);
            
            SetBodyBoxOn(_bodyBoxOn);
            SetHitBoxOn(_hitBoxOn);
            SetAttackBoxOn(_attackBoxOn);
        }

        void SetSpriteImageWithNoSpriteIndicator()
        {
            SetSpriteImage(NoSpriteIndicator);
            _noSpriteIndicator.SwitchDisplay(true);
            _spriteField.SetValueWithoutNotify(null);
        }

        void SetSpriteImage(Sprite sprite)
        {
            if (spriteImage.sprite == sprite) return;
            
            Sprite previousSprite = spriteImage.sprite;
            spriteImage.sprite = sprite;
            using (ChangeEvent<Sprite> evt = ChangeEvent<Sprite>.GetPooled(previousSprite, sprite))
            {
                evt.target = (IEventHandler) spriteImage;
                spriteImage.SendEvent((EventBase) evt);
            }
        }

        void SetEditingBox(EditBoxType boxType)
        {
            _currentEditBox = boxType;

            switch (_currentEditBox)
            {
                default:
                case EditBoxType.Body:
                    _bodyBoxEditingIndicator.visible = true;
                    _hitBoxEditingIndicator.visible = false;
                    _attackBoxEditingIndicator.visible = false;
                    break;
                case EditBoxType.Hit:
                    _bodyBoxEditingIndicator.visible = false;
                    _hitBoxEditingIndicator.visible = true;
                    _attackBoxEditingIndicator.visible = false;
                    break;
                case EditBoxType.Attack:
                    _bodyBoxEditingIndicator.visible = false;
                    _hitBoxEditingIndicator.visible = false;
                    _attackBoxEditingIndicator.visible = true;
                    break;
            }
            
            EditorPrefs.SetString(_EDIT_BOX_PREF_KEY, _currentEditBox.ToString());
        }

        void SetBodyBoxOn(bool on)
        {
            _bodyBoxOn = on;

            _bodyBoxElementList.ForEach((re) => re.SwitchDisplay(false));

            if (_bodyBoxOn)
            {
                for (int i = 0; i < _currentFrameData.BodyBox.Count; i++)
                {
                    _bodyBoxElementList[i].SwitchDisplay();
                    _bodyBoxElementList[i].Rect = multiplyRect(_currentFrameData.BodyBox[i], _renderingVEStretchRatio);
                    _bodyBoxElementList[i].SetColor(BodyBoxColor, Color.Lerp(BodyBoxColor, Color.black, 0.25f), Color.Lerp(BodyBoxColor, Color.black, 0.5f));
                }
            }
        }
        
        void SetHitBoxOn(bool on)
        {
            _hitBoxOn = on;
            
            _hitBoxElementList.ForEach((re) => re.SwitchDisplay(false));

            if (_hitBoxOn)
            {
                for (int i = 0; i < _currentFrameData.HitBox.Count; i++)
                {
                    _hitBoxElementList[i].SwitchDisplay();
                    _hitBoxElementList[i].Rect = multiplyRect(_currentFrameData.HitBox[i], _renderingVEStretchRatio);
                    _hitBoxElementList[i].SetColor(HitBoxColor, Color.Lerp(HitBoxColor, Color.black, 0.25f), Color.Lerp(HitBoxColor, Color.black, 0.5f));
                }
            }
        }
        
        void SetAttackBoxOn(bool on)
        {
            _attackBoxOn = on;
            
            _attackBoxElementList.ForEach((re) => re.SwitchDisplay(false));

            if (_attackBoxOn)
            {
                for (int i = 0; i < _currentFrameData.AttackBox.Count; i++)
                {
                    _attackBoxElementList[i].SwitchDisplay();
                    _attackBoxElementList[i].Rect = multiplyRect(_currentFrameData.AttackBox[i], _renderingVEStretchRatio);
                    _attackBoxElementList[i].SetColor(AttackBoxColor, Color.Lerp(AttackBoxColor, Color.black, 0.25f), Color.Lerp(AttackBoxColor, Color.black, 0.5f));
                }
            }
        }
        
        void AddEditingBox()
        {
            (List<Rect>, List<RectElement>, Color) boxData = _currentEditBox switch
            {
                EditBoxType.Body => (_frameDatas[_currentFrame].BodyBox, _bodyBoxElementList, BodyBoxColor),
                EditBoxType.Hit => (_frameDatas[_currentFrame].HitBox, _hitBoxElementList, HitBoxColor),
                EditBoxType.Attack => (_frameDatas[_currentFrame].AttackBox, _attackBoxElementList, AttackBoxColor),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            int currentBoxCount = boxData.Item1.Count;

            if (currentBoxCount >= boxData.Item2.Count)
            {
                Printer.Print("FrameDevTool Config의 pool size를 늘려주세요. 최대 한도에 도달했습니다.");
                return;
            }

            var mousePosition = GetMousePosition();
            var pivotPosition = _pivotVisualElement.worldBound.position;

            Rect newRect = new Rect(mousePosition.x - pivotPosition.x, pivotPosition.y - mousePosition.y, 20, 20);
            
            boxData.Item1.Add(newRect);
            boxData.Item2[currentBoxCount].SwitchDisplay(true);
            boxData.Item2[currentBoxCount].Rect = boxData.Item1[currentBoxCount];
            boxData.Item2[currentBoxCount].SetColor(boxData.Item3, Color.Lerp(boxData.Item3, Color.black, 0.25f), Color.Lerp(boxData.Item3, Color.black, 0.5f));
        }

        void RemoveEditingBox()
        {
            (List<Rect>, List<RectElement>) boxData = _currentEditBox switch
            {
                EditBoxType.Body => (_frameDatas[_currentFrame].BodyBox, _bodyBoxElementList),
                EditBoxType.Hit => (_frameDatas[_currentFrame].HitBox, _hitBoxElementList),
                EditBoxType.Attack => (_frameDatas[_currentFrame].AttackBox, _attackBoxElementList),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            // find mous pointing rect
            RectElement removeRectElement = null;

            foreach (RectElement rectElement in boxData.Item2)
            {
                var mouseDiff = GetMousePosition() - rectElement.worldBound.position;
                if (mouseDiff.x <= rectElement.worldBound.width && mouseDiff.x >= 0
                                                                && mouseDiff.y <= rectElement.worldBound.height && mouseDiff.y >= 0)
                {
                    removeRectElement = rectElement;
                    break;
                }
            }
            
            if (removeRectElement == null)
            {
                return;
            }
            
            removeRectElement.SwitchDisplay(false);
            boxData.Item2.Remove(removeRectElement);
            boxData.Item2.Add(removeRectElement);
            boxData.Item1.Remove(removeRectElement.Rect);
        }
        
        Rect multiplyRect(Rect rect, float multiplyValue)
        {
            return new Rect(rect.x * multiplyValue, rect.y * multiplyValue, rect.width * multiplyValue, rect.height * multiplyValue);
        }
        
        Rect divideRect(Rect rect, float divideValue)
        {
            return new Rect(rect.x / divideValue, rect.y / divideValue, rect.width / divideValue, rect.height / divideValue);
        }
        #endregion

        #region ShortCuts
        [Shortcut("FrameDevTool BodyBoxEdit", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.Q)]
        static void EditBodyBoxShortCut()
        {
            
            GetWindow<FrameAnimatorEditorWindow>().SetEditingBox(EditBoxType.Body);
        }
        
        [Shortcut("FrameDevTool HitBoxEdit", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.W)]
        static void EditHitBoxShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().SetEditingBox(EditBoxType.Hit);
        }
        
        [Shortcut("FrameDevTool AttackBoxEdit", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.E)]
        static void EditAttackBoxShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().SetEditingBox(EditBoxType.Attack);
        }

        [Shortcut("FrameDevTool AddEditingBox", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.Mouse0, ShortcutModifiers.Control)]
        static void AddBoxShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().AddEditingBox();
        }

        [Shortcut("FrameDevTool RemoveEditingBox", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.Mouse0, ShortcutModifiers.Shift)]
        static void RemoveBoxShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().RemoveEditingBox();
        }
        
        [Shortcut("FrameDevTool PreviousImage", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.DownArrow)]
        static void PreviousImageShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().OnPreviousImageButtonClicked(null);
        }
        
        [Shortcut("FrameDevTool PreviousFrame", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.LeftArrow)]
        static void PreviousFrameShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().OnPreviousFrameButtonClicked(null);
        }
        
        [Shortcut("FrameDevTool NextFrame", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.RightArrow)]
        static void NextFrameShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().OnNextFrameButtonClicked(null);
        }
        
        [Shortcut("FrameDevTool NextImage", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.UpArrow)]
        static void NextImageShortCut()
        {
            GetWindow<FrameAnimatorEditorWindow>().OnNextImageButtonClicked(null);
        }
        
        [Shortcut("FrameDevTool PlayAnimation", typeof(FrameAnimatorEditorWindow), _FRAME_DEV_TOOL_TAG, KeyCode.Space)]
        static void PlayAnimationShortCut()
        {
            var window = GetWindow<FrameAnimatorEditorWindow>();
            if (!window._isPlaying)
            {
                window.OnPlayButtonClicked(null);
            }
            else
            {
                window.OnPauseButtonClicked(null);
            }
        }
        #endregion

        [MenuItem("Tools/FrameAnimation/Print Info")]
        static void PrintInfo()
        {
            StringBuilder builder = new StringBuilder();
            
            foreach (FrameData data in GetWindow<FrameAnimatorEditorWindow>()._frameDatas)
            {
                builder.AppendLine(data.ToString());
            }
            
            Printer.Print(builder);
        }
    }
}
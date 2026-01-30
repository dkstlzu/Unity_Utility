using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace dkstlzu.Utility
{
    [Serializable]
    public class FastScroll
    {
        [Serializable]
        public class Item
        {
            public int TypeIndex;
            public int Index;
            public GameObject gameObject;
            public bool IsObjectAllocated => gameObject;

            public float Height;

            public Item(int typeIndex)
            {
                TypeIndex = typeIndex;
            }

            public virtual void OnAllocateObject(GameObject obj)
            {
                gameObject = obj;
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Clone)"))
                {
                    obj.name = obj.name.Replace("(Clone)", $" {Index}");
                }
#endif
            }

            public virtual void OnlyRefreshUI(GameObject obj)
            {
                GameObject restoreObj = gameObject;
                OnAllocateObject(obj);
                OnAllocateObject(restoreObj);
            }

            public void BakeAllocateObject(GameObject obj, bool isVertical)
            {
                OnAllocateObject(obj);
                if (isVertical)
                {
                    Height = obj.GetRectTransform().rect.height;
                }
                else
                {
                    Height = obj.GetRectTransform().rect.width;
                }
            }

            public virtual GameObject DeallocateObject()
            {
                GameObject beforeObj = gameObject;
                gameObject = null;
                return beforeObj;
            }
        }

        [Serializable]
        class BaseInfo
        {
            public GameObject BaseObj;
            public GameObject SampleObj;
            public float Spacing;
            public bool PreBakeHeight;
            public float ForceHeight;
        }

        [SerializeField] [ReadOnly] protected ScrollRect _scrollRect;
        protected RectTransform _scrollRectTrans;
        [SerializeField] [ReadOnly] protected Vector2 _scrollPosition;
        [SerializeField] [ReadOnly] protected RectTransform _content;
        [SerializeField] [ReadOnly] protected RectTransform _contentPool;

        [ReadOnly] public bool IsVertical = true;

        [SerializeField] [ReadOnly] protected float _paddingTop;
        [SerializeField] [ReadOnly] protected float _paddingBottom;

        [SerializeField] [ReadOnly] protected int _nowTopIndex;
        [SerializeField] [ReadOnly] protected int _nowShowCountOnScrollRect;
        [SerializeField] [ReadOnly] protected float _startPosition;
        [SerializeField] [ReadOnly] protected float _endPosition;

        [SerializeField] List<BaseInfo> _baseInfos = new List<BaseInfo>();

        protected List<List<GameObject>> _objPool = new List<List<GameObject>>();

        [SerializeReference] public List<Item> ItemList = new List<Item>();
        [SerializeReference] public List<Item> ShownItemList = new List<Item>();

        protected Action<Vector2> _onScrollValueChanged;

        protected int _reqMoreTotalCount;
        protected Action<Action> _onRequestMoreToServer;
        protected Func<Task> _onRequestMoreToServerAsync;
        protected bool _requestMoreOnTop;
        protected float _requestMorePosition;
        protected bool _reqPending;

        public FastScroll(ScrollRect scrollRect, RectTransform content, RectTransform contentPool, float paddingTop = 0, float paddingBottom = 0)
        {
            _scrollRect = scrollRect;
            if (!scrollRect.vertical && scrollRect.horizontal)
            {
                IsVertical = false;
            }

            _content = content;
            _contentPool = contentPool;
            contentPool.gameObject.SetActive(false);
            _scrollRectTrans = scrollRect.GetComponent<RectTransform>();

            _paddingTop = paddingTop;
            _paddingBottom = paddingBottom;

            if (IsVertical)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, paddingTop + paddingBottom);
            }
            else
            {
                content.sizeDelta = new Vector2(paddingTop + paddingBottom, content.sizeDelta.y);
            }

            scrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        public void AddType(GameObject baseObj, float spacingY, int preInstantiateCount, bool preBakeHeight = true, float forceHeight = 0)
        {
            BaseInfo baseInfo = new BaseInfo()
            {
                BaseObj = baseObj,
                SampleObj = Object.Instantiate(baseObj, baseObj.transform.parent),
                Spacing = spacingY,
                PreBakeHeight = preBakeHeight,
                ForceHeight = forceHeight
            };

            baseInfo.SampleObj.SetActive(false);

            _baseInfos.Add(baseInfo);

            var pool = new List<GameObject>(preInstantiateCount);

            baseObj.SetActive(true);

            for (int i = 0; i < preInstantiateCount; i++)
            {
                GameObject newObj = Object.Instantiate(baseObj, _contentPool);
                pool.Add(newObj);
            }

            baseObj.SetActive(false);

            _objPool.Add(pool);
        }

        public int GetCount()
        {
            return ItemList.Count;
        }

        public void SetOnScrollValueChanged(Action<Vector2> onScrollValueChanged)
        {
            _onScrollValueChanged = onScrollValueChanged;
        }

        public void AddItem(Item addItem)
        {
            BaseInfo baseInfo = _baseInfos[addItem.TypeIndex];

            baseInfo.SampleObj.SetActive(true);
            if (baseInfo.ForceHeight > 0)
            {
                addItem.Height = baseInfo.ForceHeight;
            }
            else
            {
                addItem.BakeAllocateObject(baseInfo.SampleObj, IsVertical);
                addItem.DeallocateObject();
            }

            baseInfo.SampleObj.SetActive(false);
            addItem.Index = ItemList.Count;

            ItemList.Add(addItem);
            if (addItem.Index > 0)
            {
                if (IsVertical)
                {
                    _content.sizeDelta += new Vector2(0, baseInfo.Spacing);
                }
                else
                {
                    _content.sizeDelta += new Vector2(baseInfo.Spacing, 0);
                }
            }

            if (IsVertical)
            {
                _content.sizeDelta += new Vector2(0, addItem.Height);
            }
            else
            {
                _content.sizeDelta += new Vector2(addItem.Height, 0);
            }

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void AddItems(List<Item> addItems)
        {
            Vector2 contentSizeDelta = Vector2.zero;

            foreach (Item addItem in addItems)
            {
                BaseInfo baseInfo = _baseInfos[addItem.TypeIndex];

                baseInfo.SampleObj.SetActive(true);
                if (baseInfo.ForceHeight > 0)
                {
                    addItem.Height = baseInfo.ForceHeight;
                }
                else
                {
                    addItem.BakeAllocateObject(baseInfo.SampleObj, IsVertical);
                    addItem.DeallocateObject();
                }

                baseInfo.SampleObj.SetActive(false);
                addItem.Index = ItemList.Count;

                ItemList.Add(addItem);

                if (addItem.Index > 0)
                {
                    if (IsVertical)
                    {
                        contentSizeDelta += new Vector2(0, baseInfo.Spacing);
                    }
                    else
                    {
                        contentSizeDelta += new Vector2(baseInfo.Spacing, 0);
                    }
                }

                if (IsVertical)
                {
                    contentSizeDelta += new Vector2(0, addItem.Height);
                }
                else
                {
                    contentSizeDelta += new Vector2(addItem.Height, 0);
                }
            }

            _content.sizeDelta += contentSizeDelta;

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void InsertItem(int index, Item addItem)
        {
            (int previousTopIndex, int _) = GetTopIndex();
            Item previousTopItem = ItemList[previousTopIndex];

            BaseInfo baseInfo = _baseInfos[addItem.TypeIndex];

            baseInfo.SampleObj.SetActive(true);
            if (baseInfo.ForceHeight > 0)
            {
                addItem.Height = baseInfo.ForceHeight;
            }
            else
            {
                addItem.BakeAllocateObject(baseInfo.SampleObj, IsVertical);
                addItem.DeallocateObject();
            }

            baseInfo.SampleObj.SetActive(false);
            addItem.Index = index;

            ItemList.Insert(index, addItem);

            // 1보다 많다는건 이전에 이미 있었다는 뜻
            if (ItemList.Count > 1)
            {
                if (IsVertical)
                {
                    _content.sizeDelta += new Vector2(0, baseInfo.Spacing);
                }
                else
                {
                    _content.sizeDelta += new Vector2(baseInfo.Spacing, 0);
                }
            }

            if (IsVertical)
            {
                _content.sizeDelta += new Vector2(0, addItem.Height);
            }
            else
            {
                _content.sizeDelta += new Vector2(addItem.Height, 0);
            }

            for (int i = index + 1; i < ItemList.Count; i++)
            {
                ItemList[i].Index++;
            }

            Reset();
            ScrollTo(previousTopItem.Index);
        }

        public void InsertItems(int index, List<Item> addItems)
        {
            (int previousTopIndex, int _) = GetTopIndex();
            Item previousTopItem = ItemList[previousTopIndex];

            Vector2 contentSizeDelta = Vector2.zero;

            foreach (Item addItem in addItems)
            {
                BaseInfo baseInfo = _baseInfos[addItem.TypeIndex];

                baseInfo.SampleObj.SetActive(true);
                if (baseInfo.ForceHeight > 0)
                {
                    addItem.Height = baseInfo.ForceHeight;
                }
                else
                {
                    addItem.BakeAllocateObject(baseInfo.SampleObj, IsVertical);
                    addItem.DeallocateObject();
                }

                baseInfo.SampleObj.SetActive(false);
                addItem.Index = index;

                ItemList.Insert(index, addItem);
                index++;

                // 1보다 많다는건 이전에 이미 있었다는 뜻
                if (ItemList.Count > 1)
                {
                    if (IsVertical)
                    {
                        contentSizeDelta += new Vector2(0, baseInfo.Spacing);
                    }
                    else
                    {
                        contentSizeDelta += new Vector2(baseInfo.Spacing, 0);
                    }
                }

                if (IsVertical)
                {
                    contentSizeDelta += new Vector2(0, addItem.Height);
                }
                else
                {
                    contentSizeDelta += new Vector2(addItem.Height, 0);
                }
            }

            _content.sizeDelta += contentSizeDelta;
            for (int i = index; i < ItemList.Count; i++)
            {
                ItemList[i].Index += addItems.Count;
            }

            Reset();
            ScrollTo(previousTopItem.Index);
        }

        public void RemoveItem(Item removeItem)
        {
            Vector2 contentSizeDelta = Vector2.zero;

            int index = ItemList.IndexOf(removeItem);
            if (index < 0)
            {
                return;
            }

            ReturnToPool(removeItem.TypeIndex, removeItem.DeallocateObject());
            ItemList.RemoveAt(index);

            for (int i = index; i < ItemList.Count; i++)
            {
                ItemList[i].Index = i;
            }

            BaseInfo baseInfo = _baseInfos[removeItem.TypeIndex];

            if (removeItem.Index > 0)
            {
                if (IsVertical)
                {
                    contentSizeDelta -= new Vector2(0, baseInfo.Spacing);
                }
                else
                {
                    contentSizeDelta -= new Vector2(baseInfo.Spacing, 0);
                }
            }

            if (IsVertical)
            {
                contentSizeDelta -= new Vector2(0, removeItem.Height);
            }
            else
            {
                contentSizeDelta -= new Vector2(removeItem.Height, 0);
            }

            _content.sizeDelta += contentSizeDelta;

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void RemoveItems(List<Item> removeItems)
        {
            Vector2 contentSizeDelta = Vector2.zero;

            foreach (Item removeItem in removeItems.OrderByDescending(i => i.Index))
            {
                int index = ItemList.IndexOf(removeItem);
                if (index < 0)
                {
                    continue;
                }

                ReturnToPool(removeItem.TypeIndex, removeItem.DeallocateObject());
                ItemList.RemoveAt(index);

                for (int i = index; i < ItemList.Count; i++)
                {
                    ItemList[i].Index = i;
                }

                BaseInfo baseInfo = _baseInfos[removeItem.TypeIndex];

                if (removeItem.Index > 0)
                {
                    if (IsVertical)
                    {
                        contentSizeDelta -= new Vector2(0, baseInfo.Spacing);
                    }
                    else
                    {
                        contentSizeDelta -= new Vector2(baseInfo.Spacing, 0);
                    }
                }

                if (IsVertical)
                {
                    contentSizeDelta -= new Vector2(0, removeItem.Height);
                }
                else
                {
                    contentSizeDelta -= new Vector2(removeItem.Height, 0);
                }
            }

            _content.sizeDelta += contentSizeDelta;

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void ShowItems(int topIndex, int showCountOnScrollRect)
        {
            ShownItemList.Clear();
            float showPosition = _paddingTop;

            for (int i = 0; i < topIndex; i++)
            {
                if (i > 0)
                {
                    showPosition += _baseInfos[ItemList[i].TypeIndex].Spacing;
                }

                showPosition += ItemList[i].Height;
            }

            for (int i = topIndex; i < showCountOnScrollRect + topIndex; i++)
            {
                int index = i;
                if (index >= ItemList.Count)
                    continue;

                if (index > 0)
                {
                    showPosition += _baseInfos[ItemList[index].TypeIndex].Spacing;
                }

                if (ItemList[index].IsObjectAllocated == false)
                {
                    var trans = GetFromPool(ItemList[index].TypeIndex);
                    trans.gameObject.SetActive(true);

                    if (index < _nowTopIndex)
                    {
                        trans.SetAsFirstSibling();
                    }
                    else
                    {
                        trans.SetAsLastSibling();
                    }

                    // 높이를 미리 계산하지 않고 매번 그때그때 계산하는 경우
                    if (_baseInfos[ItemList[index].TypeIndex].PreBakeHeight == false)
                    {
                        float previousHeight = ItemList[index].Height;

                        if (_baseInfos[ItemList[index].TypeIndex].ForceHeight > 0)
                        {
                            ItemList[index].Height = _baseInfos[ItemList[index].TypeIndex].ForceHeight;
                        }
                        else
                        {
                            ItemList[index].BakeAllocateObject(trans.gameObject, IsVertical);
                        }

                        if (IsVertical)
                        {
                            _content.sizeDelta += new Vector2(0, ItemList[index].Height - previousHeight);
                        }
                        else
                        {
                            _content.sizeDelta += new Vector2(ItemList[index].Height - previousHeight, 0);
                        }
                    }
                    else
                    {
                        ItemList[index].OnAllocateObject(trans.gameObject);
                    }

                    Vector3 pos = trans.localPosition;
                    if (IsVertical)
                    {
                        pos.y = -showPosition;
                    }
                    else
                    {
                        pos.x = showPosition;
                    }

                    trans.localPosition = pos;
                }

                ShownItemList.Add(ItemList[index]);
                showPosition += ItemList[index].Height;
            }
        }

        (int, int) GetTopIndex()
        {
            Vector2 scrollPos = _scrollRect.normalizedPosition;
            scrollPos.y = 1 - scrollPos.y;

            float scrollPosition;
            if (IsVertical)
            {
                scrollPosition = scrollPos.y * (_content.rect.height - _scrollRectTrans.rect.height);
            }
            else
            {
                scrollPosition = scrollPos.x * (_content.rect.width - _scrollRectTrans.rect.width);
            }

            int findIndex = -1;
            int showCountOnScrollRect = 0;
            _startPosition = 0;
            _endPosition = 0;

            for (int i = 0; i < ItemList.Count; i++)
            {
                if (i == 0)
                {
                    _startPosition = _paddingTop;
                    _endPosition = _startPosition + ItemList[i].Height;
                }
                else
                {
                    _startPosition = _endPosition + _baseInfos[ItemList[i].TypeIndex].Spacing;
                    _endPosition = _startPosition + ItemList[i].Height;
                }

                if (findIndex < 0)
                {
                    if (scrollPosition - ItemList[i].Height <= _endPosition)
                    {
                        showCountOnScrollRect = 1;
                        findIndex = i;
                    }
                }
                else
                {
                    if (IsVertical && scrollPosition + _scrollRectTrans.rect.height <= _startPosition)
                    {
                        break;
                    }
                    else if (!IsVertical && scrollPosition + _scrollRectTrans.rect.width <= _startPosition)
                    {
                        break;
                    }

                    showCountOnScrollRect++;
                }
            }

            return (findIndex, showCountOnScrollRect);
        }

        RectTransform GetFromPool(int objTypeIndex)
        {
            var pool = _objPool[objTypeIndex];

            if (pool.Count == 0)
            {
                // AddType과 동일한 동작을 위해 일부러 contentPool에 갔다오는겁니다. (transform이 변함)
                GameObject newObj = Object.Instantiate(_baseInfos[objTypeIndex].BaseObj, _contentPool);
                newObj.transform.SetParent(_content.transform);
                return (RectTransform)newObj.transform;
            }
            else
            {
                Transform trans = pool[0].transform;
                trans.SetParent(_content.transform);
                pool.RemoveAt(0);
                return (RectTransform)trans;
            }
        }

        void ReturnToPool(int objTypeIndex, GameObject obj)
        {
            _objPool[objTypeIndex].Add(obj);
            obj.transform.SetParent(_contentPool.transform);
        }

        public void Refresh()
        {
            (int topIndex, int showCountOnScrollRect) = GetTopIndex();

            if (_nowTopIndex < topIndex)
            {
                for (int i = _nowTopIndex; i < topIndex; i++)
                {
                    if (i >= ItemList.Count || i < 0)
                        continue;
                    if (ItemList[i].IsObjectAllocated == false)
                        continue;
                    GameObject obj = ItemList[i].DeallocateObject();

                    ReturnToPool(ItemList[i].TypeIndex, obj);
                }
            }

            if (_nowTopIndex + _nowShowCountOnScrollRect > topIndex + showCountOnScrollRect)
            {
                for (int i = topIndex + showCountOnScrollRect; i <= _nowTopIndex + showCountOnScrollRect; i++)
                {
                    if (i >= ItemList.Count || i < 0)
                        continue;
                    if (ItemList[i].IsObjectAllocated == false)
                        continue;
                    GameObject obj = ItemList[i].DeallocateObject();
                    ReturnToPool(ItemList[i].TypeIndex, obj);
                }
            }

            ShowItems(topIndex, showCountOnScrollRect);

            _nowTopIndex = topIndex;
            _nowShowCountOnScrollRect = showCountOnScrollRect;
        }

        void OnValueChanged(Vector2 value)
        {
            if (ItemList == null || ItemList.Count == 0)
                return;

            _scrollPosition = Vector2.up - value;

            Refresh();

            _onScrollValueChanged?.Invoke(value);

            if (_reqMoreTotalCount > ItemList.Count && _reqPending == false)
            {
                bool verticalPositionRefresh = (_requestMoreOnTop && _scrollPosition.y <= _requestMorePosition) || (!_requestMoreOnTop && _scrollPosition.y >= _requestMorePosition);
                bool horizontalPositionRefresh = (_requestMoreOnTop && _scrollPosition.x <= _requestMorePosition) || (!_requestMoreOnTop && _scrollPosition.x >= _requestMorePosition);

                if ((IsVertical && verticalPositionRefresh) || (!IsVertical && horizontalPositionRefresh))
                {
                    RequestMoreToServer();
                }
            }
        }

        private async Task RequestMoreToServer()
        {
            _reqPending = true;
            if (_onRequestMoreToServer != null)
            {
                _onRequestMoreToServer(() => { _reqPending = false; });
            }
            else if (_onRequestMoreToServerAsync != null)
            {
                await _onRequestMoreToServerAsync();

                _reqPending = false;
            }
        }

        public float GetTotalHeight()
        {
            return IsVertical ? _content.rect.height : _content.rect.width;
        }

        public void SetOnRequestMoreToServer(int reqMoreTotalCount, Action<Action> onRequestMoreToServer, bool requestMoreOnTop, float requestMorePosition)
        {
            _reqMoreTotalCount = reqMoreTotalCount;
            _onRequestMoreToServer = onRequestMoreToServer;
            _onRequestMoreToServerAsync = null;
            _requestMoreOnTop = requestMoreOnTop;
            _requestMorePosition = requestMorePosition;
        }

        public void SetOnRequestMoreToServer(int reqMoreTotalCount, Func<Task> onRequestMoreToServerAsync, bool requestMoreOnTop, float requestMorePosition)
        {
            _reqMoreTotalCount = reqMoreTotalCount;
            _onRequestMoreToServer = null;
            _onRequestMoreToServerAsync = onRequestMoreToServerAsync;
            _requestMoreOnTop = requestMoreOnTop;
            _requestMorePosition = requestMorePosition;
        }

        public void ScrollTo(float x, float y)
        {
            float sx = Mathf.Max(0, x / (_content.sizeDelta.x - _scrollRectTrans.rect.width));

            float sy = 0;
            if (_content.sizeDelta.y > 0)
                sy = Mathf.Max(0, (y) / (_content.sizeDelta.y));

            _scrollRect.normalizedPosition = new Vector2(sx, sy);
            _scrollPosition = new Vector2(sx, sy);
            Refresh();
        }

        public void ScrollTo(int index)
        {
            float otherAxis = 0;
            float targetPosition = _paddingTop;
            Vector2 contentSizeDelta = Vector2.zero;

            for (int i = 0; i < index; i++)
            {
                BaseInfo baseInfo = _baseInfos[ItemList[i].TypeIndex];

                if (!baseInfo.PreBakeHeight)
                {
                    float previousHeight = ItemList[index].Height;

                    ItemList[i].BakeAllocateObject(baseInfo.SampleObj, IsVertical);
                    ItemList[i].DeallocateObject();

                    if (IsVertical)
                    {
                        contentSizeDelta += new Vector2(0, baseInfo.Spacing + ItemList[i].Height - previousHeight);
                    }
                    else
                    {
                        contentSizeDelta += new Vector2(baseInfo.Spacing + ItemList[i].Height - previousHeight, 0);
                    }
                }

                targetPosition += ItemList[i].Height;

                if (i > 0)
                {
                    targetPosition += _baseInfos[ItemList[i].TypeIndex].Spacing;
                }
            }

            _content.sizeDelta += contentSizeDelta;

            if (IsVertical)
            {
                float targetScrollPosition = 1 - targetPosition / (_content.rect.height - _scrollRectTrans.rect.height);
                _scrollRect.normalizedPosition = new Vector2(otherAxis, targetScrollPosition);
                _scrollPosition = new Vector2(otherAxis, targetScrollPosition);
            }
            else
            {
                float targetScrollPosition = targetPosition / (_content.rect.width - _scrollRectTrans.rect.width);
                _scrollRect.normalizedPosition = new Vector2(targetScrollPosition, otherAxis);
                _scrollPosition = new Vector2(targetScrollPosition, otherAxis);
            }

            Refresh();
        }

        public Vector2 GetScrollPosition()
        {
            return new Vector2(_scrollPosition.x, 1 - _scrollPosition.y);
        }

        private void Reset()
        {
            foreach (Item item in ItemList)
            {
                if (item.IsObjectAllocated == false)
                {
                    continue;
                }

                GameObject obj = item.DeallocateObject();
                ReturnToPool(item.TypeIndex, obj);
            }
        }

        public void Clear()
        {
            foreach (var item in ItemList)
            {
                if (item.IsObjectAllocated == false)
                    continue;
                GameObject obj = item.DeallocateObject();
                ReturnToPool(item.TypeIndex, obj);
            }

            ItemList.Clear();
            if (IsVertical)
            {
                _content.sizeDelta = new Vector2(0, _paddingTop + _paddingBottom);
            }
            else
            {
                _content.sizeDelta = new Vector2(_paddingTop + _paddingBottom, 0);
            }

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);

            ScrollTo(0, 0);
        }
    }
}
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public abstract class BTGraphNodeBase : Node, IBTSerializableNode
    {
        protected string _guid;
        private List<BTGraphNodeAttacher> _attachers;
        protected List<BTGraphNodeAttacher> Attachers
        {
            get
            {
                if (_attachers == null)
                {
                    _attachers = new List<BTGraphNodeAttacher>();
                }

                return _attachers;
            }
        }

        public string Guid => _guid;
        public abstract string Name { get; }
        public BTGraphOrderLabel OrderLabel { get; set; }
        public int OrderValue
        {
            get => OrderLabel.Value;
            set => OrderLabel.Value = value;
        }

        public int x { get; protected set; }
        public int y { get; protected set; }

        protected Action<string, Vector2, Action<BTGraphInitParamsAttacher>> OpenDecoSearchWnd;
        protected Action<string, Vector2, Action<BTGraphInitParamsAttacher>> OpenServiceSearchWnd;

        public abstract void OnConnect(BTDesignContainer designContainer, string parentGuid);
        public abstract void OnCreate(BTDesignContainer designContainer, Vector2 position);
        public abstract void OnDelete(BTDesignContainer designContainer);
        public abstract void OnMove(BTDesignContainer designContainer, Vector2 moveDelta);

        public int LabelPosX
        {
            get
            {
                const int maxCharForDefaultSize = 8;
                int titleLen = TextContentLength;
                var posX = 108 + (titleLen <= maxCharForDefaultSize ? 0 : (titleLen - maxCharForDefaultSize) * 17) - 14;
                return posX;
            }
        }

        private int TextContentLength
        {
            get
            {
                if (_attachers == null || _attachers.Count == 0)
                {
                    return Name.Length;
                }

                var longestDecorator = _attachers.Aggregate((longest, next) => next.Name.Length > longest.Name.Length ? next : longest);
                return Mathf.Max(Name.Length, longestDecorator.Name.Length);
            }
        }

        public void InitAttachers(List<BTSerializableAttacher> serializedAttachers)
        {
            _attachers = new List<BTGraphNodeAttacher>(serializedAttachers.Count);

            foreach (var serializedAttacher in serializedAttachers)
            {   
                bool isDecorator = typeof(IBTDecorator).IsAssignableFrom(serializedAttacher.task.GetType());
                BTGraphInitParamsAttacher initParams = ToGraphInitParams(serializedAttacher);
                var graphAttacher = isDecorator ? AttachNewDecorator(initParams) : AttachNewService(initParams);
            }
        }

        private BTGraphInitParamsAttacher ToGraphInitParams(BTSerializableAttacher serializedAttacher)
        {
            var icon = BTGlobalSettings.Instance.GetIcon(serializedAttacher.task.GetType());
            var initParams = new BTGraphInitParamsAttacher()
            {
                guid = serializedAttacher.guid,
                nodeID = _guid,
                name = serializedAttacher.name,
                icon = icon,
                task = serializedAttacher.task
            };

            return initParams;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (!AreAttachersAllowed)
            {
                return;
            }

            evt.menu.InsertAction(1, "Add Decorator", action => 
            {
                var rect = GetPosition();
                var mousePos = rect.position + new Vector2(rect.width, 0f);
                OpenDecoSearchWnd(_guid, mousePos, initParams => AttachNewDecorator(initParams));
            });

            evt.menu.InsertAction(2, "Add Service", action => 
            {
                var rect = GetPosition();
                var mousePos = rect.position + new Vector2(rect.width, 0f);
                OpenServiceSearchWnd(_guid, mousePos, initParams => AttachNewService(initParams));
            });

            evt.menu.InsertSeparator("/", 1);
        }

        protected abstract bool AreAttachersAllowed { get; }

        private BTGraphNodeAttacher AttachNewService(BTGraphInitParamsAttacher initParams) 
            => AddNewAttacher(initParams, BTGraphNodeAttacher.CreateService);

        private BTGraphNodeAttacher AttachNewDecorator(BTGraphInitParamsAttacher initParams)
            => AddNewAttacher(initParams, BTGraphNodeAttacher.CreateDecorator);

        private BTGraphNodeAttacher AddNewAttacher(BTGraphInitParamsAttacher initParams, Func<BTGraphInitParamsAttacher, BTGraphNodeAttacher> ctor)
        {
            BTGraphNodeAttacher attacher = ctor(initParams);
            extensionContainer.style.backgroundColor = Utils.ColorExtension.Create(62f);
            extensionContainer.style.paddingTop = 3f;
            extensionContainer.Add(attacher);
            Attachers.Add(attacher);
            RefreshExpandedState();

            if (OrderLabel != null)
            {
                OrderLabel.SetRealPosition(new Vector2(x + LabelPosX, y));
            }

            return attacher;
        }
    }
}
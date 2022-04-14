using System.Collections.Generic;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField]
        private GameObject _actor = null;

        [SerializeField]
        private BTDesignContainer _designContainer = null;

        public BTDesignContainer DesignContainer => _designContainer;

        private BTBaseNode _root;
        private RuntimeBlackboard _runtimeBlackboard;

        private void Start()
        {
            if (_designContainer.NodeDataList.Count == 0 || _designContainer.TaskDataList.Count == 0)
            {
                Debug.LogError("Invalid Behavior Tree");
                gameObject.SetActive(false);
            }

            var (root, nodeDataList) = Extract(_designContainer);
            _root = root;

            if (_root == null)
            {
                Debug.LogError("No Root node was found");
                gameObject.SetActive(false);
            }

            InitTree(nodeDataList);
        }

        private (BTBaseNode root, BTNodeData[] nodeDataList) Extract(BTDesignContainer designContainer)
        {
            BTBaseNode root = null;
            var nodeDict = new Dictionary<string, (BTBaseNode node, int yPos)>(designContainer.NodeDataList.Count);
            var linkDataList = new List<BTLinkData>(designContainer.NodeDataList.Count);
            var linkDict = new Dictionary<BTBaseNode, List<(BTBaseNode node, int yPos)>>(designContainer.NodeDataList.Count);
            
            designContainer.NodeDataList.ForEach(nodeData => 
            {
                var node = BTNodeFactory.Create(nodeData.NodeType, nodeData.Guid);

                if (nodeData.NodeType == BTNodeType.Root)
                {
                    root = node;
                }
                
                nodeDict.Add(nodeData.Guid, (node, Mathf.FloorToInt(nodeData.Position.y)));
                linkDict.Add(node, new List<(BTBaseNode node, int yPos)>());
                
                if (!string.IsNullOrEmpty(nodeData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = nodeData.ParentGuid, endGuid = nodeData.Guid });
                }
            });

            designContainer.TaskDataList.ForEach(taskData => 
            {
                var node = BTNodeFactory.CreateLeaf(taskData.Task, taskData.Guid);

                nodeDict.Add(taskData.Guid, (node, Mathf.FloorToInt(taskData.Position.y)));
                linkDict.Add(node, new List<(BTBaseNode node, int yPos)>());
                
                if (!string.IsNullOrEmpty(taskData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = taskData.ParentGuid, endGuid = taskData.Guid });
                }
            });

            var nodePriorityComparer = new NodePriorityComparer();

            linkDataList.ForEach(linkData =>
            {
                var (parent, child) = (nodeDict[linkData.startGuid], nodeDict[linkData.endGuid]);
                var children = linkDict[parent.node];
                children.Add(child);
                children.Sort(nodePriorityComparer);
            }); // Needs optimization

            return (root, MapToNodeDataList(linkDict));
        }

        private class NodePriorityComparer : IComparer<(BTBaseNode node, int yPos)>
        {
            public int Compare((BTBaseNode node, int yPos) x, (BTBaseNode node, int yPos) y) => x.yPos.CompareTo(y.yPos);
        }

        private BTNodeData[] MapToNodeDataList(Dictionary<BTBaseNode, List<(BTBaseNode node, int yPos)>> linkDict)
        {
            var nodeDataList = new BTNodeData[linkDict.Count];
            int idx = 0;

            System.Func<List<(BTBaseNode node, int yPos)>, BTBaseNode[]> MapToBaseNodeList = childrenData =>
            {
                var childNodes = new BTBaseNode[childrenData.Count];

                for (int i = 0; i < childrenData.Count; i++)
                {
                    childNodes[i] = childrenData[i].node;
                }

                return childNodes;
            };

            foreach (var entry in linkDict)
            {
                nodeDataList[idx++] = new BTNodeData(entry.Key, MapToBaseNodeList(entry.Value));
            }

            return nodeDataList;
        }

        private void InitTree(BTNodeData[] nodeDataList)
        {
            // Debug.Log(nodeDataList.Length);
            foreach (var data in nodeDataList)
            {
                data.Node.Init(data.Children, _actor, null);
            }

            _runtimeBlackboard = _designContainer.Blackboard.RuntimeBlackboard;
        }

        private void Update()
        {
            _root.Update(_actor, _runtimeBlackboard);
        }
    }
}
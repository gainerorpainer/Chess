using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    class TreeNode
    {
        public Board Board;
        public double Evaluation;
        public List<TreeNode> Children;
        public TreeNode Parent;
        public bool IsComplete;
        public Move Move;
    }
}

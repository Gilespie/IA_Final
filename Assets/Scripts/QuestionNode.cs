using System;

public class QuestionNode : ITreeNode
{
    Func<bool> _question;
    ITreeNode _trueNode;
    ITreeNode _falseNode;

    public QuestionNode(Func<bool> question, ITreeNode trueNode, ITreeNode falseNode)
    {
        _question = question;
        _trueNode = trueNode;
        _falseNode = falseNode;
    }

    public void Execute()
    {
        if(_question.Invoke())
        {
            _trueNode.Execute();
        }
        else
        {
            _falseNode.Execute();
        }
    }
}
using System.IO;

namespace Scope.Deserialization
{
  public class CryXmlNode
  {
    public CryXmlNode(BinaryReader r, int id)
    {
      NodeID = id;
      ElementNameOffset = r.ReadInt();
      ContentOffset = r.ReadInt();
      AttributeCount = r.ReadInt16();
      ChildCount = r.ReadInt16();
      ParentNodeIndex = r.ReadInt();
      FirstAttributeIndex = r.ReadInt();
      FirstChildIndex = r.ReadInt();
      Reserved = r.ReadInt();
    }

    public int NodeID { get; }
    public int ElementNameOffset { get; }
    public int ContentOffset { get; }
    public short AttributeCount { get; }
    public short ChildCount { get; }
    public int ParentNodeIndex { get; }
    public int FirstAttributeIndex { get; }
    public int FirstChildIndex { get; }
    public int Reserved { get; }
  }
}


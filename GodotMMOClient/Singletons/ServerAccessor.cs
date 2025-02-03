using Godot;

public partial class ServerAccessor : Node
{
    public static Server GetServerNode()
    {
        return ((SceneTree)Engine.GetMainLoop()).Root.GetNode<Server>("ServerNode");
    }
}
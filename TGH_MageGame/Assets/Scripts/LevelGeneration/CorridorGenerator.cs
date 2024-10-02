using System.Collections.Generic;
using System.Linq;

public class CorridorGenerator {

    //generates corridors
    public List<Node> CreateCorridors(List<RoomNode> allNodesCollection, int corridorWidth) {
        List<Node> corridorList = new List<Node>();
        //queue of nodes starting with node at lowest index
        Queue<RoomNode> structuresToCheck = new Queue<RoomNode>(allNodesCollection.OrderByDescending(node => node.TreeLayerIndex).ToList());
        //loop while there are structures to check
        while (structuresToCheck.Count > 0) {
            //pop a node
            var node = structuresToCheck.Dequeue();
            //test if it doesn't have any children
            if (node.ChildrenNodes.Count == 0) {
                continue;
            }

            CorridorNode corridor = new CorridorNode(node.ChildrenNodes[0], node.ChildrenNodes[1], corridorWidth);
            corridorList.Add(corridor);
        }


        return corridorList;
    }
}
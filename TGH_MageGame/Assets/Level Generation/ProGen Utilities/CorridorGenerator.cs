using System.Collections.Generic;
using System.Linq;

public class CorridorGenerator {

    //generates corridors
    public List<CorridorNode> CreateCorridors(List<RoomNode> allNodesCollection, int corridorWidth) {
        List<CorridorNode> corridorList = new List<CorridorNode>();
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

            RoomNode room1 = (RoomNode) node.ChildrenNodes[0];
            RoomNode room2 = (RoomNode) node.ChildrenNodes[1];

            CorridorNode corridor = new CorridorNode(room1, room2, corridorWidth);

            corridorList.Add(corridor);
        }


        return corridorList;
    }
}

import clusterNode = require("models/database/cluster/clusterNode");

class clusterTopology {

    leader = ko.observable<string>();
    nodeTag = ko.observable<string>();

    nodes = ko.observableArray<clusterNode>([]);

    constructor(dto: clusterTopologyDto) {
        this.leader(dto.Leader);
        this.nodeTag(dto.NodeTag);

        const topologyDto = dto.Topology;

        const members = this.mapNodes("Member", topologyDto.Members);
        const promotables = this.mapNodes("Promotable", topologyDto.Promotables);
        const watchers = this.mapNodes("Watcher", topologyDto.Watchers);

        this.nodes(_.concat<clusterNode>(members, promotables, watchers));
    }

    private mapNodes(type: clusterNodeType, dict: System.Collections.Generic.Dictionary<string, string>): Array<clusterNode> {
        return _.map(dict, (v, k) => clusterNode.for(k, v, type));
    }

    updateWith(incomingChanges: clusterTopologyDto) {
        const newTopology = incomingChanges.Topology;

        const existingNodes = this.nodes();
        const newNodes = _.concat<clusterNode>(
            this.mapNodes("Member", newTopology.Members),
            this.mapNodes("Promotable", newTopology.Promotables),
            this.mapNodes("Watcher", newTopology.Watchers)
        );
        const newServerUrls = new Set(newNodes.map(x => x.serverUrl()));

        const toDelete = existingNodes.filter(x => !newServerUrls.has(x.serverUrl()));
        toDelete.forEach(x => this.nodes.remove(x));

        newNodes.forEach(node => {
            const matchedNode = existingNodes.find(x => x.serverUrl() === node.serverUrl());

            if (matchedNode) {
                matchedNode.updateWith(node);
            } else {
                this.nodes.push(node);
            }
        });

        this.nodeTag(incomingChanges.NodeTag);
        this.leader(incomingChanges.Leader);
    }
}

export = clusterTopology;
import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { Project } from '../shared/types/project'

export function createDefaultProject() {
    const project: Project = {
        SensorSampleInterval: '00:10:00',
        DataLinkProtocolType: DataLinkProtocolType.PureAloha,
        ClusterizationAlgorithm: {
            $type: ClusterizationAlgorithmType.RetardedClusterization,
            NumberOfClusters: 6
        },
        AreaLimits: {
            Min: {
                X: 0,
                Y: 0,
                Z: 0
            },
            Max: {
                X: 10_000,
                Y: 10_000,
                Z: 10_000
            }
        },
        ChannelManager: {
            NumberOfChannels: 1
        },
        Environment: {
            Sensors: []
        },
        Result: undefined
    }

    return project
}

import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { ModemType } from '../shared/types/modem'
import { NetworkProtocolType } from '../shared/types/networkProtocolType'
import { Project } from '../shared/types/project'

export function createDefaultProject() {
    const project: Project = {
        SensorSettings: {
            Modem: {
                $type: ModemType.AquaModem1000
            },

            SampleInterval: '00:30:00',

            InitialSensorBattery: 864_000,
            BatteryDeadCharge: 100,

            DataLinkProtocol: {
                $type: DataLinkProtocolType.PureAloha,
                Timeout: 10,
                TimeoutRelativeDeviation: 0.5,
                AckTimeout: 20,
                AckRetries: 3
            },

            NetworkProtocol: {
                $type: NetworkProtocolType.BasicNetworkProtocol,
                ResendWarningCount: 1
            },

            ClusterizationAlgorithm: {
                $type: ClusterizationAlgorithmType.RetardedClusterization,
                NumberOfClusters: 6
            }
        },

        SimulationSettings: {
            DeadSensorsPercent: 33,

            MaxProcessedEvents: 0,
            MaxCycles: 0,
            PrintEveryNthEvent: 10_000,

            ShouldSkipHello: false,
            ShouldSkipCycles: false,

            CyclesCountBeforeSkip: 50,

            Verbose: false,
            CreateAllDeltas: false,
            SaveOutput: false
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

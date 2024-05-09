import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { ModemType } from '../shared/types/modem'
import { Project } from '../shared/types/project'

export function createDefaultProject() {
    const project: Project = {
        SensorSettings: {
            Modem: {
                $type: ModemType.AquaModem1000
            },

            SensorSampleInterval: '00:10:00',
            StartSamplingTime: '0001-01-02T00:00:00',

            InitialSensorBattery: 864_000,
            BatteryDeadCharge: 100,

            DataLinkProtocol: {
                $type: DataLinkProtocolType.PureAloha,
                Timeout: 10,
                TimeoutRelativeDeviation: 0.5,
                AckTimeout: 20,
                AckRetries: 3
            },

            ClusterizationAlgorithm: {
                $type: ClusterizationAlgorithmType.RetardedClusterization,
                NumberOfClusters: 6
            }
        },

        SimulationSettings: {
            DeadSensorsPercent: 33,

            MaxProcessedEvents: 5_000_000,
            MaxCycles: 1_000,
            PrintEveryNthEvent: 10_000,

            ShouldSkipHello: true
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

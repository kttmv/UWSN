import { NetworkProtocolType } from '../shared/types/networkProtocolType'
import { ProjectData } from '../shared/types/projectData'

export function createDefaultProject() {
    const project: ProjectData = {
        SensorSampleInterval: '00:10:00',
        NetworkProtocolType: NetworkProtocolType.PureAloha,
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
        }
    }

    return project
}

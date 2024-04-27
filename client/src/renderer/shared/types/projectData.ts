import { NetworkProtocolType } from './networkProtocolType'
import { SensorData } from './sensorData'
import { Vector3Data } from './vector3Data'

export type ProjectData = {
    SensorSampleInterval: string
    NetworkProtocolType: NetworkProtocolType
    AreaLimits: {
        Min: Vector3Data
        Max: Vector3Data
    }
    ChannelManager: {
        NumberOfChannels: number
    }
    Environment: {
        Sensors: SensorData[]
    }
}

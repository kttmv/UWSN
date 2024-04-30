import { Frame } from './frame'
import { NetworkProtocolType } from './networkProtocolType'
import { Sensor } from './sensor'
import { Signal } from './signal'
import { SimulationDelta } from './simulationDelta'
import { Vector3 } from './vector3'

export type Project = {
    SensorSampleInterval: string
    NetworkProtocolType: NetworkProtocolType
    AreaLimits: {
        Min: Vector3
        Max: Vector3
    }
    ChannelManager: {
        NumberOfChannels: number
    }
    Environment: {
        Sensors: Sensor[]
    }
    Result:
        | {
              TotalSends: number
              TotalReceives: number
              TotalCollisions: number
              AllFrames: Frame[]
              AllSignals: Signal[]
              AllDeltas: Record<string, SimulationDelta>[]
          }
        | undefined
}

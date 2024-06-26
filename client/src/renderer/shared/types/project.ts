import { Frame } from './frame'
import { SensorData } from './sensorData'
import { SensorSettings } from './sensorSettings'
import { Signal } from './signal'
import { SimulationDelta } from './simulationDelta'
import { SimulationSettings } from './simulationSettings'
import { Vector3 } from './vector3'

export type Project = {
    SensorSettings: SensorSettings

    SimulationSettings: SimulationSettings

    AreaLimits: {
        Min: Vector3
        Max: Vector3
    }

    ChannelManager: {
        NumberOfChannels: number
    }

    Environment: {
        Sensors: SensorData[]
    }

    Result:
        | {
              TotalSends: number
              TotalReceives: number
              TotalCollisions: number
              AllFrames: Frame[]
              AllSignals: Signal[]
              Deltas: SimulationDelta[]
          }
        | undefined
}

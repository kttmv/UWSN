import { SensorState } from './sensorState'
import { Vector3 } from './vector3'

export type SensorSimulationState = {
    Id: number
    State: SensorState
    Position: Vector3
    ClusterId: number | undefined
    IsReference: boolean
    Battery: number
}

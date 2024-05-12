import { SensorState } from './sensorState'

export type SensorDelta = {
    Id: number
    State: SensorState
    ClusterId: number | undefined
    IsReference: boolean | undefined
    Battery: number | undefined
}

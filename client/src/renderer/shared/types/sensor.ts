import { Vector3 } from './vector3'

export type Sensor = {
    Id: number
    Position: Vector3
    ClusterId: number | undefined
    IsReference: boolean
    Battery: number
}

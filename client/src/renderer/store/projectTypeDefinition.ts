export type ProjectData = undefined | {
    AreaLimits: AreaLimitsData
    Sensors: SensorData[]
}

type Vector3Data = {
    X: number
    Y: number
    Z: number
}
type SensorData = {
    Id: number
    Position: Vector3Data
}

type AreaLimitsData = {
    Item1: Vector3Data
    Item2: Vector3Data
}

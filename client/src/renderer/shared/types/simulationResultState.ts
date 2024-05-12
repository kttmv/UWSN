import { Sensor } from './sensor'
import { Signal } from './signal'

export type SimulationState = {
    Time: string
    CycleId: number
    Signals: Signal[]
    Sensors: Sensor[]
}

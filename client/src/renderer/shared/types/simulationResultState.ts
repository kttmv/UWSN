import { Sensor } from './sensor'
import { Signal } from './signal'

export type SimulationState = {
    Time: string
    Signals: Signal[]
    Sensors: Sensor[]
}

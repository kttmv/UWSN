import { SensorSimulationState } from './sensorSimulationState'
import { Signal } from './signal'

export type SimulationState = {
    Time: string
    CycleId: number
    Signals: Signal[]
    Sensors: SensorSimulationState[]
}

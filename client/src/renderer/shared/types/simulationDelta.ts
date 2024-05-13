import { SensorDelta } from './sensorDelta'
import { SignalDelta } from './signalDelta'

export type SimulationDelta = {
    Time: string
    CycleId: number
    SignalDeltas: SignalDelta[] | undefined
    SensorDeltas: SensorDelta[] | undefined
}

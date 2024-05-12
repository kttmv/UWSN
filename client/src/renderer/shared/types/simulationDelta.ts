import { SensorDelta } from './clusterizationDelta'
import { SignalDelta } from './signalDelta'

export type SimulationDelta = {
    Time: string
    SignalDeltas: SignalDelta[] | undefined
    SensorDeltas: SensorDelta[] | undefined
}

import { ClusterizationDelta } from './clusterizationDelta'
import { SignalDelta } from './signalDelta'

export type SimulationDelta = {
    Time: string
    SignalDeltas: SignalDelta[]
    ClusterizationDeltas: ClusterizationDelta[]
}

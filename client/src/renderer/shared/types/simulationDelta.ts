import { SignalDelta } from './signalDelta'

export type SimulationDelta = {
    Time: string
    SignalDeltas: SignalDelta[]
}

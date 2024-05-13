export type SimulationSettings = {
    Verbose: boolean
    CreateAllDeltas: boolean
    SaveOutput: boolean

    MaxProcessedEvents: number
    MaxCycles: number
    PrintEveryNthEvent: number

    DeadSensorsPercent: number

    ShouldSkipHello: boolean

    ShouldSkipCycles: boolean
    CyclesCountBeforeSkip: number
}

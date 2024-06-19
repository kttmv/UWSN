import { create } from 'zustand'
import { readFile, writeFile } from '../shared/helpers/fsHelpers'
import { Project } from '../shared/types/project'
import { SensorSimulationState } from '../shared/types/sensorSimulationState'
import { SimulationState } from '../shared/types/simulationResultState'

type State = {
    projectFilePath: string
    project: Project | undefined

    simulationDeltaIndex: number
    simulationState: SimulationState

    isShellRunning: boolean

    setProjectFilePath: (value: string) => void
    setProject: (value: Project) => void
    updateProject: () => void

    setDeltaIndex: (value: number, toClosestSignalDelta: boolean) => void

    setIsShellRunning: (value: boolean) => void
}

export const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,

    simulationDeltaIndex: -1,
    simulationState: createDefaultState(undefined),

    isShellRunning: false,

    setProjectFilePath: async (path: string) => {
        await parseProjectFile(path, set, get)
        get().setDeltaIndex(-1, false)
        set({ projectFilePath: path })
    },

    updateProject: async () => {
        const path = get().projectFilePath
        get().setDeltaIndex(-1, false)
        await parseProjectFile(path, set, get)
    },

    setProject: async (newProject: Project) => {
        const path = get().projectFilePath

        try {
            writeFile(path, JSON.stringify(newProject, null, 4))

            get().setDeltaIndex(-1, false)

            set(() => ({ project: newProject }))
        } catch (error) {
            console.error(
                `Не удалось записать изменения в файл ${path}.`,
                error
            )
        }
    },

    setDeltaIndex: (index: number, toClosestSignalDelta: boolean) => {
        const project = get().project

        if (!project || !project.Result || index === -1) {
            return
        }

        if (index === get().simulationDeltaIndex) {
            return
        }

        let state = createDefaultState(project)

        const direction = index > get().simulationDeltaIndex ? 1 : -1

        if (toClosestSignalDelta) {
            let i = index
            while (true) {
                const simulationDelta = project.Result.Deltas[i]

                const signalDeltas = simulationDelta.SignalDeltas

                if (
                    signalDeltas ||
                    i === -1 ||
                    i === project.Result.Deltas.length - 1
                ) {
                    state = calculateSimulationState(i, project, get())
                    set({ simulationDeltaIndex: i, simulationState: state })

                    return
                }

                i += direction
            }
        }

        state = calculateSimulationState(index, project, get())
        set({ simulationDeltaIndex: index, simulationState: state })
    },

    setIsShellRunning: (value: boolean) => {
        set({ isShellRunning: value })
    }
}))

export function calculateSimulationState(
    index: number,
    project: Project | undefined,
    currentStoreState: State
): SimulationState {
    let startIndex = 0
    let state: SimulationState | null = null

    if (
        currentStoreState.simulationDeltaIndex <= index &&
        currentStoreState.simulationDeltaIndex > 0
    ) {
        state = currentStoreState.simulationState
        startIndex = currentStoreState.simulationDeltaIndex + 1
    } else {
        state = createDefaultState(project)
    }

    if (!project || !project.Result || index === -1) {
        return state
    }

    if (index >= project.Result.Deltas.length || index < -1) {
        throw new Error('Неправильный индекс дельты симуляции')
    }

    for (let i = startIndex; i <= index; i++) {
        const simulationDelta = project.Result.Deltas[i]

        state.Time = simulationDelta.Time
        state.CycleId = simulationDelta.CycleId

        const signalDeltas = simulationDelta.SignalDeltas
        const sensorDeltas = simulationDelta.SensorDeltas

        if (signalDeltas) {
            for (const delta of signalDeltas) {
                switch (delta.Type) {
                    case 'Add': {
                        const signal = project.Result.AllSignals[delta.SignalId]
                        state.Signals.push(signal)
                        break
                    }
                    case 'Remove': {
                        const signal = project.Result.AllSignals[delta.SignalId]
                        state.Signals = state.Signals.filter(
                            (x) => x !== signal
                        )
                        break
                    }
                    default: {
                        throw new Error('Что-то пошло не так')
                    }
                }
            }
        }

        if (sensorDeltas) {
            for (const delta of sensorDeltas) {
                const sensor = state.Sensors[delta.Id]

                // ниже нужно обязательно писать именно
                // ... !== undefined, так как 0 - это тоже false
                // (я обожаю джаваскрипт. я только что потратил
                // час времени на то, чтобы понять, в чем трабл
                // с нулевым кластером)

                if (delta.ClusterId !== undefined) {
                    sensor.ClusterId = delta.ClusterId
                }

                if (delta.IsReference !== undefined) {
                    sensor.IsReference = delta.IsReference
                }

                if (delta.Battery !== undefined) {
                    sensor.Battery = delta.Battery
                }

                if (delta.State !== undefined) {
                    sensor.State = delta.State
                }
            }
        }
    }

    return state
}

async function parseProjectFile(
    path: string,
    set: (partial: (state: State) => State | Partial<State>) => void,
    get: () => State
) {
    try {
        const content = await readFile(path)
        const project = JSON.parse(content)

        const state = calculateSimulationState(-1, project, get())

        set(() => ({
            simulationDeltaIndex: -1,
            simulationState: state,
            project
        }))
    } catch (error) {
        console.error(`Не удалось прочитать файл ${path}.`, error)
    }
}

function createDefaultState(project: Project | undefined): SimulationState {
    let sensors: SensorSimulationState[] | undefined
    if (project) {
        sensors = project.Environment.Sensors.map((x) => ({
            Id: x.Id,
            State: undefined,
            Position: x.Position,
            ClusterId: undefined,
            IsReference: false,
            Battery: project.SensorSettings.InitialSensorBattery
        }))
    } else {
        sensors = []
    }

    return {
        Time: '0001-01-01T00:00:00.00',
        CycleId: 0,
        Signals: [],
        Sensors: sensors
    }
}

import { create } from 'zustand'
import { readFile, writeFile } from '../shared/helpers/fsHelpers'
import { Project } from '../shared/types/project'
import { Sensor } from '../shared/types/sensor'
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

    setDeltaIndex: (value: number) => void

    setIsShellRunning: (value: boolean) => void
}

export const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,

    simulationDeltaIndex: -1,
    simulationState: createDefaultState(undefined),

    isShellRunning: false,

    setProjectFilePath: async (path: string) => {
        await parseProjectFile(path, set)
        get().setDeltaIndex(-1)
        set({ projectFilePath: path })
    },

    updateProject: async () => {
        const path = get().projectFilePath
        get().setDeltaIndex(-1)
        await parseProjectFile(path, set)
    },

    setProject: async (newProject: Project) => {
        const path = get().projectFilePath

        try {
            writeFile(path, JSON.stringify(newProject, null, 4))

            get().setDeltaIndex(-1)

            set(() => ({ project: newProject }))
        } catch (error) {
            console.error(
                `Не удалось записать изменения в файл ${path}.`,
                error
            )
        }
    },

    setDeltaIndex: (index: number) => {
        const project = get().project

        const state = calculateSimulationState(index, project)

        set({ simulationDeltaIndex: index, simulationState: state })
    },

    setIsShellRunning: (value: boolean) => {
        set({ isShellRunning: value })
    }
}))

export function calculateSimulationState(
    index: number,
    project: Project | undefined
): SimulationState {
    const state = createDefaultState(project)

    if (!project || !project.Result || index === -1) {
        return state
    }

    if (index >= project.Result.Deltas.length || index < -1) {
        throw new Error('Неправильный индекс дельты симуляции')
    }

    for (let i = 0; i <= index; i++) {
        const simulationDelta = project.Result.Deltas[i]

        state.Time = simulationDelta.Time

        const signalDeltas = simulationDelta.SignalDeltas
        const sensorDeltas = simulationDelta.SensorDeltas

        for (const delta of signalDeltas) {
            switch (delta.Type) {
                case 'Add': {
                    const signal = project.Result.AllSignals[delta.SignalId]
                    state.Signals.push(signal)
                    break
                }
                case 'Remove': {
                    const signal = project.Result.AllSignals[delta.SignalId]
                    state.Signals = state.Signals.filter((x) => x !== signal)
                    break
                }
                default: {
                    throw new Error('Что-то пошло не так')
                }
            }
        }

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
                sensor.Battery += delta.Battery
            }
        }
    }

    return state
}

async function parseProjectFile(
    path: string,
    set: (partial: (state: State) => State | Partial<State>) => void
) {
    try {
        const content = await readFile(path)
        const project = JSON.parse(content)

        const state = calculateSimulationState(-1, project)

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
    let sensors: Sensor[] | undefined
    if (project) {
        sensors = project.Environment.Sensors.map((x) => ({
            Id: x.Id,
            Position: x.Position,
            ClusterId: -1,
            IsReference: false,
            Battery: project.SensorSettings.InitialSensorBattery
        }))
    } else {
        sensors = []
    }

    return {
        Time: '0001-01-01T00:00:00.00',
        Signals: [],
        Sensors: sensors
    }
}

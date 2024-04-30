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

    setProjectFilePath: (value: string) => void
    setProject: (value: Project) => void
    updateProject: () => void

    setDeltaIndex: (value: number) => void
}

export const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,

    simulationDeltaIndex: -1,
    simulationState: createDefaultState(undefined),

    setProjectFilePath: async (path: string) => {
        await parseProjectFile(path, set)
    },

    updateProject: async () => {
        const path = get().projectFilePath
        await parseProjectFile(path, set)
    },

    setProject: async (newProject: Project) => {
        const path = get().projectFilePath

        try {
            writeFile(path, JSON.stringify(newProject, null, 4))

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
    }
}))

function calculateSimulationState(
    index: number,
    project: Project | undefined
): SimulationState {
    if (!project || !project.Result || index >= project.Result.Deltas.length) {
        throw new Error('Что-то пошло не так')
    }

    if (index === -1) {
        return createDefaultState(project)
    }

    const state = createDefaultState(project)

    for (let i = 0; i <= index; i++) {
        const simulationDelta = project.Result.Deltas[i]

        const signalDeltas = simulationDelta.SignalDeltas
        const clusterizationDeltas = simulationDelta.ClusterizationDeltas

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

        for (const delta of clusterizationDeltas) {
            // const sensorData = project.Environment.Sensors[delta.SensorId]
            const sensor = state.Sensors[delta.SensorId]

            sensor.ClusterId = delta.ClusterId
            sensor.IsReference = delta.IsReference
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

        set(() => ({ project }))
    } catch (error) {
        console.error(`Не удалось прочитать файл ${path}.`, error)
    }
}

function createDefaultState(project: Project | undefined): SimulationState {
    let sensors: Sensor[] | undefined = undefined
    if (project) {
        sensors = project.Environment.Sensors.map((x) => ({
            Id: x.Id,
            Position: x.Position,
            ClusterId: -1,
            IsReference: false
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

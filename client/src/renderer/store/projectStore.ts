import { create } from 'zustand'
import { readFile, writeFile } from '../shared/helpers/fsHelpers'
import { Project } from '../shared/types/project'
import { SimulationResultState } from '../shared/types/simulationResultState'

type State = {
    projectFilePath: string
    project: Project | undefined

    currentDeltaIndex: number
    currentSimulationResultState: SimulationResultState

    setProjectFilePath: (value: string) => void
    setProject: (value: Project) => void
    updateProject: () => void

    setDeltaIndex: (value: number) => void
}

export const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,

    currentDeltaIndex: -1,
    currentSimulationResultState: {
        Time: '0001-01-01T00:00:00.00',
        Signals: []
    },

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

        const state = calculateCurrentSimulationResultState(index, project)

        set({ currentDeltaIndex: index, currentSimulationResultState: state })
    }
}))

function calculateCurrentSimulationResultState(
    index: number,
    project: Project | undefined
) {
    if (!project || !project.Result || index >= project.Result.Deltas.length) {
        throw new Error('Что-то пошло не так')
    }

    if (index === -1) {
        return {
            Time: '0001-01-01T00:00:00.00',
            Signals: []
        }
    }

    console.log(index)
    console.log(project.Result.Deltas[index])
    console.log(project.Result.Deltas)

    const state: SimulationResultState = {
        Time: project.Result.Deltas[index].Time,
        Signals: []
    }

    for (let i = 0; i <= index; i++) {
        const signalDeltas = project.Result.Deltas[i].SignalDeltas

        for (const signalDelta of signalDeltas) {
            switch (signalDelta.Type) {
                case 'Add': {
                    const signal =
                        project.Result.AllSignals[signalDelta.SignalId]

                    state.Signals.push(signal)

                    break
                }
                case 'Remove': {
                    const signal =
                        project.Result.AllSignals[signalDelta.SignalId]

                    state.Signals = state.Signals.filter((x) => x !== signal)

                    break
                }
                default: {
                    throw new Error('Что-то пошло не так')
                }
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

        set(() => ({ project }))
    } catch (error) {
        console.error(`Не удалось прочитать файл ${path}.`, error)
    }
}

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
}

export const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,

    currentDeltaIndex: -1,
    currentSimulationResultState: {
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
        const state: SimulationResultState = {
            Signals: []
        }

        if (index === -1) {
            set({
                currentDeltaIndex: index,
                currentSimulationResultState: state
            })
        }

        const project = get().project

        calculateCurrentSimulationResultState(index, project, state)

        set({ currentDeltaIndex: index, currentSimulationResultState: state })
    }
}))

function calculateCurrentSimulationResultState(
    index: number,
    project: Project | undefined,
    state: SimulationResultState
) {
    if (!project || !project.Result || index >= project.Result.Deltas.length) {
        throw new Error('Что-то пошло не так')
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

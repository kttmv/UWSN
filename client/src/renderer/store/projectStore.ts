import { create } from 'zustand'
import { readFile, writeFile } from '../shared/helpers/fsHelpers'
import { Project } from '../shared/types/project'

type State = {
    projectFilePath: string
    setProjectFilePath: (value: string) => void
    project: Project | undefined
    setProject: (value: Project) => void
    updateProject: () => void
}

const useProjectStore = create<State>((set, get) => ({
    projectFilePath: '',
    project: undefined,
    setProjectFilePath: async (path: string) => {
        console.log(`setProjectPath: ${path}`)

        try {
            const content = await readFile(path)
            const project = JSON.parse(content)

            set(() => ({ projectFilePath: path, project }))
        } catch (error) {
            // todo: показывать ошибку в модальном окне
            console.error(`Не удалось прочитать файл ${path}.`, error)
        }
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
    updateProject: async () => {
        const path = get().projectFilePath

        try {
            const content = await readFile(path)
            const project = JSON.parse(content)

            set(() => ({ project }))
        } catch (error) {
            console.error(`Не удалось прочитать файл ${path}.`, error)
        }
    }
}))

export default useProjectStore

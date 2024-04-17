import { create } from 'zustand'
import { readFile } from '../shared/helpers/fsHelpers'
import { ProjectData } from './projectTypeDefinition'

type State = {
    projectFilePath: string
    setProjectFilePath: (value: string) => void
    project: ProjectData
}

const useProjectStore = create<State>((set) => ({
    projectFilePath: '',
    project: undefined,
    setProjectFilePath: async (path: string) => {
        try {
            const content = await readFile(path)
            const project = JSON.parse(content)

            set(() => ({ projectFilePath: path, project }))
        } catch (error) {
            // todo: показывать ошибку в модальном окне
            console.error(`Не удалось прочитать файл ${path}.`, error)
        }
    }
}))

export default useProjectStore

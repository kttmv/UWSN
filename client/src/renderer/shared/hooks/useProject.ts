import { useEffect, useState } from 'react'
import useProjectStore from '../../store/projectStore'
import { readFile } from '../helpers/fsHelpers'

export default function useProjectFile() {
    const { projectFilePath } = useProjectStore()

    // todo: заменить any на нормальное определение файла проекта
    const [fileContent, setFileContent] = useState<any>(null)

    useEffect(() => {
        async function fetchFileContent() {
            try {
                const content = await readFile(projectFilePath)
                setFileContent(JSON.parse(content))
            } catch (error) {
                // todo: показывать ошибку в модальном окне
                console.error('Failed to read file:', error)
            }
        }

        fetchFileContent()
    }, [projectFilePath])

    return fileContent
}

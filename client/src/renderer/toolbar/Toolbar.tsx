import { Button, Card, Tooltip } from '@chakra-ui/react'
import { IconFile, IconFolderOpen } from '@tabler/icons-react'
import { openFile, saveFile } from '../shared/helpers/fsHelpers'
import useProjectStore from '../store/projectStore'

export default function Toolbar() {
    const { setProjectFilePath } = useProjectStore()

    const onCreateNewClick = async () => {
        const result = await saveFile({
            title: 'Укажите название файла создаваемой симуляции',
            filters: [{ name: '', extensions: ['json'] }]
        })

        if (!result.canceled && result.filePath) {
            console.log(result.filePath)
            setProjectFilePath(result.filePath)
        }
    }

    const onOpenClick = async () => {
        const result = await openFile({
            title: 'Выберите существующий файл симуляции',
            filters: [{ name: '', extensions: ['json'] }]
        })

        if (!result.canceled && result.filePaths) {
            setProjectFilePath(result.filePaths[0])
        }
    }

    return (
        <Card borderRadius={0} gap={1} p={1} flexDirection='row'>
            <Tooltip label='Создать новую симуляцию'>
                <Button onClick={onCreateNewClick}>
                    <IconFile />
                </Button>
            </Tooltip>
            <Tooltip label='Открыть симуляцию'>
                <Button onClick={onOpenClick}>
                    <IconFolderOpen />
                </Button>
            </Tooltip>
        </Card>
    )
}

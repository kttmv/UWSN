import { Button, Card, Tooltip } from '@chakra-ui/react'
import { IconFile, IconFolderOpen, IconPlayerPlay } from '@tabler/icons-react'
import {
    showOpenFileDialog,
    showSaveFileDialog,
    writeFile
} from '../shared/helpers/fsHelpers'
import { runSimulation } from '../simulator/simulatorHelper'
import useConsoleStore from '../store/consoleStore'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import { createDefaultProject } from './createDefaultProject'

export default function Toolbar() {
    const {
        project,
        updateProject,
        projectFilePath,
        setProjectFilePath,
        isShellRunning,
        setIsShellRunning
    } = useProjectStore()

    const { addLineToConsoleOutput } = useConsoleStore()

    const { setIsOpen: setConsoleIsOpen } = useConsoleStore()

    const { setIsOpen: setViewerIsOpen } = useViewerStore()

    const onCreateNewClick = async () => {
        const result = await showSaveFileDialog({
            title: 'Укажите название файла создаваемой симуляции',
            filters: [{ name: '', extensions: ['json'] }]
        })

        if (!result.canceled && result.filePath) {
            const project = createDefaultProject()

            await writeFile(result.filePath, JSON.stringify(project))

            setProjectFilePath(result.filePath)
        }
    }

    const onOpenClick = async () => {
        const result = await showOpenFileDialog({
            title: 'Выберите существующий файл симуляции',
            filters: [{ name: '', extensions: ['json'] }]
        })

        if (!result.canceled && result.filePaths) {
            setProjectFilePath(result.filePaths[0])
        }
    }

    const onRunSimulationClick = async () => {
        setIsShellRunning(true)
        setConsoleIsOpen(true)
        addLineToConsoleOutput('Запущен процесс симуляции')

        await runSimulation(projectFilePath)

        setIsShellRunning(false)

        updateProject()
        setViewerIsOpen(true)
    }

    const noSensors = project?.Environment.Sensors.length === 0
    const noProject = project === undefined

    const runSimulationLabel = noProject
        ? 'Не выбран проект'
        : noSensors
          ? 'Отсутствуют сенсоры'
          : isShellRunning
            ? 'Консоль уже запущена'
            : 'Запустить симуляцию'

    return (
        <Card borderRadius={0} gap={2} p={1} flexDirection='row'>
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
            <Tooltip label={runSimulationLabel}>
                <Button
                    onClick={onRunSimulationClick}
                    isDisabled={noSensors || noProject || isShellRunning}
                >
                    <IconPlayerPlay />
                </Button>
            </Tooltip>
        </Card>
    )
}

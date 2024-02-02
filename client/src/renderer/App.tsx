import {
    Box,
    Button,
    Flex,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs
} from '@chakra-ui/react'
import { executeCommand as executeShellCommand } from '.'
import './App.css'
import Console from './console/Console'
import { readFile } from './helpers/fsHelpers'
import useAppStore, { SensorNodeData } from './store'
import Viewer3D from './viewer/Viewer3D'

export default function App() {
    const {
        addSensorNode,
        consoleOutput,
        setConsoleOutput,
        clearSensorsNodes
    } = useAppStore()

    const clickedInit = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe init -1 -1 -1 1 1 1 --file D:\\Env.json'
        )
    }

    const clickedPlaceOrth = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe placeSensorsOrth 5 64 -f D:\\Env.json'
        )
    }

    const clickedPoisson = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe placeSensorsPoisson  5 64 -f D:\\Env.json'
        )
    }

    const clickedRandomStepNormal = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe placeSensorsRndStep Normal 5 0 1 64 -f D:\\Env.json'
        )
    }

    const clickedRandomStepUniform = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe placeSensorsRndStep Uniform 5 0 1 64 -f D:\\Env.json'
        )
    }

    const clickedLoad = () => {
        readFile('D:\\Env.json')
            .then((content) => {
                clearSensorsNodes()

                const data = JSON.parse(content)

                data.Sensors.forEach((sensor: SensorNodeData) => {
                    addSensorNode(sensor)
                })
                setConsoleOutput(
                    consoleOutput +
                        '\n' +
                        `Успешно загружены сенсоры (${data.Sensors.length})`
                )
            })
            .catch(() => {
                console.log('Не удалось открыть файл')
            })
    }

    return (
        <Flex direction={{ base: 'column', lg: 'row' }} h='100vh' gap={4} p={4}>
            <Box
                minW={0}
                h={{ base: '33%', lg: '100%' }}
                w={{ base: '100%', lg: '50%' }}
            >
                <Viewer3D />
            </Box>
            <Flex direction='column' height='100%' flexGrow={1}>
                <Tabs flexGrow={1}>
                    <TabList>
                        <Tab>One</Tab>
                        <Tab>Two</Tab>
                        <Tab>Three</Tab>
                    </TabList>

                    <TabPanels>
                        <TabPanel>
                            <p>one!</p>
                            <Flex direction='column' gap={1}>
                                <Button onClick={clickedInit}>
                                    Инициализация
                                </Button>
                                <Button onClick={clickedPlaceOrth}>
                                    Ортогональное распределение
                                </Button>
                                <Button onClick={clickedPoisson}>
                                    Распределение Пуассона
                                </Button>
                                <Button onClick={clickedRandomStepNormal}>
                                    Распределение со случайным шагом
                                    (нормальное)
                                </Button>
                                <Button onClick={clickedRandomStepUniform}>
                                    Распределение со случайным шагом
                                    (непрерывное)
                                </Button>
                                <Button onClick={clickedLoad}>
                                    Загрузить сенсоры
                                </Button>
                            </Flex>
                        </TabPanel>
                        <TabPanel>
                            <p>two!</p>
                        </TabPanel>
                        <TabPanel>
                            <p>three!</p>
                        </TabPanel>
                    </TabPanels>
                </Tabs>
                <Console />
            </Flex>
        </Flex>
    )
}

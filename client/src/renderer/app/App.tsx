import {
    Button,
    Flex,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    useColorModeValue
} from '@chakra-ui/react'
import { executeShellCommand } from '..'
import Console from '../console/Console'
import { readFile } from '../helpers/fsHelpers'
import EnvironmentTab from '../tabs/EnvironmentTab'
import Viewer3D from '../viewer/Viewer3D'
import './App.css'
import useAppStore, { SensorNodeData } from './store'

export default function App() {
    const bgColor = useColorModeValue('gray.50', 'gray.800')

    const {
        addSensorNode,
        consoleOutput,
        addLineToConsoleOutput,
        clearSensorsNodes
    } = useAppStore()

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
                addLineToConsoleOutput(
                    `Успешно загружены сенсоры (${data.Sensors.length})`
                )
            })
            .catch(() => {
                console.log('Не удалось открыть файл')
            })
    }

    return (
        <Flex direction={{ base: 'column', lg: 'row' }} h='100vh' gap={4} p={2}>
            <Viewer3D />
            <Flex
                direction='column'
                flexGrow={1}
                gap={4}
                minW={0}
                w={{ base: '100%', lg: '50%' }}
            >
                <Tabs h={0} overflowY='auto' flexGrow={1}>
                    <TabList position='sticky' top={0} bg={bgColor} zIndex={50}>
                        <Tab>Окружение</Tab>
                    </TabList>

                    <TabPanels>
                        <EnvironmentTab />
                        <TabPanel>
                            <p>one!</p>
                            <Flex direction='column' gap={1}>
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
                    </TabPanels>
                </Tabs>
                <Console />
            </Flex>
        </Flex>
    )
}

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
import { runSimulatorShell } from '..'
import useApplicationStore, { SensorNodeData } from '../app/store'
import { readFile } from '../helpers/fsHelpers'
import EnvironmentTab from './EnvironmentTab'

export default function ApplicationTabs() {
    const { addSensorNode, addLineToConsoleOutput, clearSensorsNodes } =
        useApplicationStore()

    const bgColor = useColorModeValue('gray.50', 'gray.800')

    const clickedPlaceOrth = () => {
        runSimulatorShell('placeSensorsOrth 5 64 -f D:\\Env.json')
    }

    const clickedPoisson = () => {
        runSimulatorShell('placeSensorsPoisson  5 64 -f D:\\Env.json')
    }

    const clickedRandomStepNormal = () => {
        runSimulatorShell('placeSensorsRndStep Normal 5 0 1 64 -f D:\\Env.json')
    }

    const clickedRandomStepUniform = () => {
        runSimulatorShell(
            'placeSensorsRndStep Uniform 5 0 1 64 -f D:\\Env.json'
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
        <Tabs h={0} overflowY='auto' flexGrow={1}>
            <TabList position='sticky' top={0} bg={bgColor} zIndex={50}>
                <Tab>Окружение</Tab>
                <Tab>Распределение</Tab>
            </TabList>

            <TabPanels>
                <EnvironmentTab />
                <TabPanel>
                    <Flex direction='column' gap={1}>
                        <Button onClick={clickedPlaceOrth}>
                            Ортогональное распределение
                        </Button>
                        <Button onClick={clickedPoisson}>
                            Распределение Пуассона
                        </Button>
                        <Button onClick={clickedRandomStepNormal}>
                            Распределение со случайным шагом (нормальное)
                        </Button>
                        <Button onClick={clickedRandomStepUniform}>
                            Распределение со случайным шагом (непрерывное)
                        </Button>
                        <Button onClick={clickedLoad}>Загрузить сенсоры</Button>
                    </Flex>
                </TabPanel>
            </TabPanels>
        </Tabs>
    )
}

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
import { runSimulatorShell } from '../simulator/simulatorHelper'
import useConsoleStore from '../store/consoleStore'
import EnvironmentTab from './EnvironmentTab'

export default function ApplicationTabs() {
    const { addLineToConsoleOutput } = useConsoleStore()

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

    return (
        <Tabs minH={0} overflowY='auto' flexGrow={1}>
            <TabList position='sticky' top={0} bg={bgColor} zIndex={50}>
                <Tab>Акватория</Tab>
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
                    </Flex>
                </TabPanel>
            </TabPanels>
        </Tabs>
    )
}

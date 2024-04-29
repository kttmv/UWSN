import {
    Tab,
    TabList,
    TabPanels,
    Tabs,
    useColorModeValue
} from '@chakra-ui/react'
import EnvironmentTab from './EnvironmentTab'
import SensorPlacementTab from './SensorPlacementTab'

export default function ApplicationTabs() {
    const bgColor = useColorModeValue('gray.50', 'gray.800')

    return (
        <Tabs minH={0} overflowY='auto' flexGrow={1}>
            <TabList position='sticky' top={0} bg={bgColor} zIndex={50}>
                <Tab>Акватория</Tab>
                <Tab>Расстановка</Tab>
            </TabList>

            <TabPanels>
                <EnvironmentTab />
                <SensorPlacementTab />
            </TabPanels>
        </Tabs>
    )
}

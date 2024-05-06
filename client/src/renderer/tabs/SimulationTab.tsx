import { Flex, TabPanel } from '@chakra-ui/react'
import { useProjectStore } from '../store/projectStore'

export default function SimulationTab() {
    const { project, setProject } = useProjectStore()

    return (
        <TabPanel>
            <form>
                <Flex direction='column' gap={4}></Flex>
            </form>
        </TabPanel>
    )
}

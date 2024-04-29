import { Flex, TabPanel } from '@chakra-ui/react'
import EnvironmentBoundaries from './EnvironmentBoundaries'

export default function EnvironmentTab() {
    return (
        <TabPanel>
            <Flex direction='column' gap={4}>
                <EnvironmentBoundaries />
            </Flex>
        </TabPanel>
    )
}

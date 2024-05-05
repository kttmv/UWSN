import { Flex, Heading } from '@chakra-ui/react'
import Console from '../console/Console'
import { useProjectStore } from '../store/projectStore'
import ApplicationTabs from '../tabs/ApplicationTabs'
import Toolbar from '../toolbar/Toolbar'
import Viewer from '../viewer/Viewer'

export default function App() {
    const { project } = useProjectStore()

    return (
        <Flex direction='column' h='100vh' minW={0} minH={0}>
            <Toolbar />
            {project ? (
                <Flex
                    direction={{ base: 'column', lg: 'row' }}
                    flexGrow={1}
                    gap={2}
                    m={2}
                    minW={0}
                    minH={0}
                >
                    <Viewer />
                    <Flex
                        direction='column'
                        flexGrow={1}
                        gap={4}
                        minW={0}
                        minH={0}
                        w={{ base: '100%', lg: '50%' }}
                        h='100%'
                    >
                        <ApplicationTabs />
                        <Console />
                    </Flex>
                </Flex>
            ) : (
                <Flex
                    direction='column'
                    h='100vh'
                    minW={0}
                    minH={0}
                    alignItems='center'
                    justifyContent='center'
                >
                    <Heading maxW='90%' textAlign='center'>
                        Для начала работы создайте новую симуляцию, или выберите
                        существующую
                    </Heading>
                </Flex>
            )}
        </Flex>
    )
}

import { Flex } from '@chakra-ui/react'
import Console from '../console/Console'
import ApplicationTabs from '../tabs/ApplicationTabs'
import Toolbar from '../toolbar/Toolbar'
import Viewer3D from '../viewer/Viewer3D'
import './App.css'

export default function App() {
    return (
        <Flex direction='column' h='100vh' minW={0} minH={0}>
            <Toolbar />
            <Flex
                direction={{ base: 'column', lg: 'row' }}
                flexGrow={1}
                gap={2}
                m={2}
                minW={0}
                minH={0}
            >
                <Viewer3D />
                <Flex
                    direction='column'
                    flexGrow={1}
                    gap={4}
                    minW={0}
                    minH={0}
                    w={{ base: '100%', lg: '50%' }}
                >
                    <ApplicationTabs />
                    <Console />
                </Flex>
            </Flex>
        </Flex>
    )
}

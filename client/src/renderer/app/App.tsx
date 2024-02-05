import { Flex } from '@chakra-ui/react'
import Console from '../console/Console'
import ApplicationTabs from '../tabs/ApplicationTabs'
import Viewer3D from '../viewer/Viewer3D'
import './App.css'

export default function App() {
    return (
        <Flex direction={{ base: 'column', lg: 'row' }} h='100vh' gap={4}>
            <Viewer3D />
            <Flex
                direction='column'
                flexGrow={1}
                gap={4}
                minW={0}
                w={{ base: '100%', lg: '50%' }}
            >
                <ApplicationTabs />
                <Console />
            </Flex>
        </Flex>
    )
}

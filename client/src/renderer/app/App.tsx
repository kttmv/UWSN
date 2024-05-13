import { Flex, Heading } from '@chakra-ui/react'
import Console from '../console/Console'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import ApplicationTabs from '../tabs/ApplicationTabs'
import Toolbar from '../toolbar/Toolbar'
import Viewer from '../viewer/Viewer'

export default function App() {
    const { project } = useProjectStore()
    const { isOpen, isFullscreen } = useViewerStore()

    console.log('test')

    if (!project) {
        return (
            <Flex direction='column' h='100vh' minW={0} minH={0}>
                <Toolbar />
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
            </Flex>
        )
    }

    let viewerHeight = isOpen ? { base: '50%', lg: '100%' } : undefined
    let mainBlockHeight = '100%'

    let mainBlockWidth = { base: '100%', lg: '50%' }

    if (isFullscreen) {
        viewerHeight = { base: '100%', lg: '100%' }
        mainBlockHeight = '0%'
        mainBlockWidth = { base: '0%', lg: '0%' }
    }

    return (
        <Flex direction='column' h='100vh' minW={0} minH={0}>
            <Toolbar />
            <Flex
                direction={{ base: 'column', lg: 'row' }}
                flexGrow={1}
                gap={2}
                margin={2}
                minWidth={0}
                minHeight={0}
            >
                <Flex
                    height={viewerHeight}
                    direction={{ base: 'column', lg: 'row' }}
                    flexGrow={isOpen ? 1 : 0}
                    minWidth={0}
                    minHeight={0}
                    position='relative'
                >
                    <Viewer />
                </Flex>
                {!isFullscreen && (
                    <Flex
                        direction='column'
                        flexGrow={1}
                        gap={4}
                        minWidth={0}
                        minHeight={0}
                        width={mainBlockWidth}
                        height={mainBlockHeight}
                    >
                        <ApplicationTabs />
                        <Console />
                    </Flex>
                )}
            </Flex>
        </Flex>
    )
}

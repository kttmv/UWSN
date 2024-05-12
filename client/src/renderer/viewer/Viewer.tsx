import { Button, Flex } from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import useViewerStore from '../store/viewerStore'
import ViewerCanvas from './ViewerCanvas'
import ViewerDeltaSettings from './ViewerDeltaSettings'
import ViewerSelectedObjectInfo from './ViewerSelectedObjectInfo'
import ViewerSettings from './ViewerSettings'
import ViewerStateInfo from './ViewerStateInfo'

export default function Viewer() {
    const { isOpen, setIsOpen } = useViewerStore()

    return (
        <Flex
            direction={{ base: 'column', lg: 'row' }}
            h={isOpen ? { base: '50%', lg: '100%' } : undefined}
            flexGrow={isOpen ? 1 : 0}
            minW={0}
            minH={0}
            position='relative'
        >
            {isOpen && (
                <>
                    <ViewerCanvas />
                    <ViewerSettings />
                    <ViewerDeltaSettings />
                    <ViewerSelectedObjectInfo />
                    <ViewerStateInfo />
                </>
            )}

            <Button
                borderTopRightRadius={{ base: 0, lg: 'md' }}
                borderTopLeftRadius={{ base: 0, lg: 0 }}
                borderBottomLeftRadius={{ base: 'md', lg: 0 }}
                size='xs'
                h={{ lg: '100%' }}
                w={{ lg: 6 }}
                onClick={() => setIsOpen(!isOpen)}
            >
                <Flex alignItems='center' transform={{ lg: 'rotate(-90deg)' }}>
                    {isOpen ? <IconCaretUp /> : <IconCaretDown />}
                    {isOpen ? 'Скрыть ' : 'Показать '}
                    3D просмотр
                </Flex>
            </Button>
        </Flex>
    )
}

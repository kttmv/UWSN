import { Button, Flex } from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useState } from 'react'
import Viewer3DCanvas from './Viewer3DCanvas'
import Viewer3DDeltaSettings from './Viewer3DDeltaSettings'
import Viewer3DSettings from './Viewer3DSettings'

export default function Viewer3D() {
    const [isOpen, setIsOpen] = useState(true)

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
                    <Viewer3DCanvas />
                    <Viewer3DSettings />
                    <Viewer3DDeltaSettings />
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

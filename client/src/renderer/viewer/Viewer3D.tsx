import { Button, Flex, useBreakpointValue } from '@chakra-ui/react'
import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import {
    IconCaretDown,
    IconCaretLeft,
    IconCaretRight,
    IconCaretUp
} from '@tabler/icons-react'
import { useState } from 'react'
import useAppStore from '../app/store'
import SensorNode from './SensorNode'

export default function Viewer3D() {
    const [isOpen, setIsOpen] = useState(true)

    const toggleButtonHideIcon = useBreakpointValue({
        base: <IconCaretUp />,
        md: <IconCaretLeft />
    })

    const toggleButtonShowIcon = useBreakpointValue({
        base: <IconCaretDown />,
        md: <IconCaretRight />
    })

    const { sensorNodes } = useAppStore()

    return (
        <Flex
            direction={{ base: 'column', lg: 'row' }}
            h={{ base: '33%', lg: '100%' }}
            w={{ base: '100%', lg: '50%' }}
            minW={0}
            gap={1}
        >
            {isOpen && (
                <Canvas className='rounded-xl shadow-md'>
                    <Sky />
                    <ambientLight />
                    <spotLight
                        position={[10, 10, 10]}
                        angle={0.15}
                        penumbra={1}
                        decay={0}
                        intensity={Math.PI}
                    />
                    <pointLight
                        position={[-10, -10, -10]}
                        decay={0}
                        intensity={Math.PI}
                    />
                    <OrbitControls makeDefault />
                    {sensorNodes.map((node) => (
                        <SensorNode
                            key={node.Id}
                            position={[
                                node.Position.X,
                                node.Position.Y,
                                node.Position.Z
                            ]}
                        />
                    ))}
                </Canvas>
            )}
            <Button
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

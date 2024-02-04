import { Button, Flex, useBreakpointValue } from '@chakra-ui/react'
import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useState } from 'react'
import useAppStore from '../app/store'
import SensorNode from './SensorNode'

export default function Viewer3D() {
    const [isOpen, setIsOpen] = useState(true)

    const canvasClassName = useBreakpointValue({
        base: 'rounded-t-md',
        lg: 'rounded-l-md'
    })

    const { sensorNodes } = useAppStore()

    return (
        <Flex
            direction={{ base: 'column', lg: 'row' }}
            h={isOpen ? { base: '33%', lg: '100%' } : undefined}
            w={isOpen ? { base: '100%', lg: '50%' } : undefined}
            minW={0}
        >
            {isOpen && (
                <Canvas className={canvasClassName}>
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

import { Button, Flex, useBreakpointValue } from '@chakra-ui/react'
import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useState } from 'react'
import { useProjectStore } from '../store/projectStore'
import useViewerSettingsStore from '../store/viewerSettingsStore'
import GridRectangle from './GridRectangle'
import SensorNode from './SensorNode'
import Viewer3DSettings from './Viewer3DSettings'

export default function Viewer3D() {
    const [isOpen, setIsOpen] = useState(true)

    const canvasClassName = useBreakpointValue({
        base: 'rounded-none',
        lg: 'rounded-l-md'
    })

    const { project } = useProjectStore()

    const { scale } = useViewerSettingsStore()

    return (
        <Flex
            direction={{ base: 'column', lg: 'row' }}
            h={isOpen ? { base: '33%', lg: '100%' } : undefined}
            flexGrow={1}
            minW={0}
            minH={0}
            position='relative'
        >
            {isOpen && (
                <>
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
                        {project && (
                            <>
                                {project.Environment.Sensors.map((sensor) => (
                                    <SensorNode
                                        key={sensor.Id}
                                        position={[
                                            sensor.Position.X / scale,
                                            sensor.Position.Y / scale,
                                            sensor.Position.Z / scale
                                        ]}
                                    />
                                ))}
                                <GridRectangle
                                    v1={project.AreaLimits.Min}
                                    v2={project.AreaLimits.Max}
                                />
                            </>
                        )}
                    </Canvas>

                    <Viewer3DSettings />
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

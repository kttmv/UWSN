import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import { useProjectStore } from '../store/projectStore'
import useViewerSettingsStore from '../store/viewerSettingsStore'
import GridRectangle from './GridRectangle'
import SensorNode from './SensorNode'

export default function Viewer3DCanvas() {
    const { project } = useProjectStore()

    const { scale } = useViewerSettingsStore()

    return (
        <Canvas>
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
    )
}

import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import GridRectangle from './GridRectangle'
import SensorNode from './SensorNode'
import Signal from './Signal'

export default function Viewer3DCanvas() {
    const { project, simulationState } = useProjectStore()

    const { scale } = useViewerStore()

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
                    {simulationState.Sensors.map((sensor) => (
                        <SensorNode
                            key={sensor.Id}
                            clusterId={sensor.ClusterId}
                            isReference={sensor.IsReference}
                            position={[
                                sensor.Position.X / scale,
                                sensor.Position.Y / scale,
                                sensor.Position.Z / scale
                            ]}
                        />
                    ))}

                    {simulationState.Signals.map((signal, i) => {
                        const sender =
                            project.Environment.Sensors[signal.SenderId]
                        const receiver =
                            project.Environment.Sensors[signal.ReceiverId]

                        const from = {
                            X: sender.Position.X / scale,
                            Y: sender.Position.Y / scale,
                            Z: sender.Position.Z / scale
                        }

                        const to = {
                            X: receiver.Position.X / scale,
                            Y: receiver.Position.Y / scale,
                            Z: receiver.Position.Z / scale
                        }

                        return <Signal key={i} from={from} to={to} />
                    })}

                    <GridRectangle
                        v1={project.AreaLimits.Min}
                        v2={project.AreaLimits.Max}
                    />
                </>
            )}
        </Canvas>
    )
}

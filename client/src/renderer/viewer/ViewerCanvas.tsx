import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import { useEffect } from 'react'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import FramePath from './FramePath'
import GridRectangle from './GridRectangle'
import SensorNode from './SensorNode'
import Signal from './Signal'

export default function ViewerCanvas() {
    const { project, simulationState } = useProjectStore()

    const { scale, selectedSensor, setSelectedSensor } = useViewerStore()

    useEffect(() => {
        if (selectedSensor)
            setSelectedSensor(simulationState.Sensors[selectedSensor.Id])
    }, [simulationState])

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
                        <SensorNode key={sensor.Id} sensor={sensor} />
                    ))}

                    {simulationState.Signals.map((signal, i) => {
                        return <Signal key={i} signal={signal} />
                    })}

                    <FramePath />

                    <GridRectangle
                        v1={project.AreaLimits.Min}
                        v2={project.AreaLimits.Max}
                    />
                </>
            )}
        </Canvas>
    )
}

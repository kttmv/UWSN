import { OrbitControls, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import useAppStore from '../app/store'
import SensorNode from './SensorNode'

export default function Viewer3D() {
    const { sensorNodes } = useAppStore()

    return (
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
    )
}

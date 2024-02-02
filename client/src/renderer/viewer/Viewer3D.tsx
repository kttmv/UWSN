import { OrbitControls, OrthographicCamera, Sky } from '@react-three/drei'
import { Canvas } from '@react-three/fiber'
import useAppStore from '../store'
import SensorNode from './SensorNode'

export default function Viewer3D() {
    const { sensorNodes } = useAppStore()

    // const bounds = useBounds()
    // useEffect(() => {
    //     // Calculate scene bounds
    //     bounds.refresh().clip().fit()

    //     // Or, focus a specific object or box3
    //     // bounds.refresh(ref.current).clip().fit()
    //     // bounds.refresh(new THREE.Box3()).clip().fit()

    //     // Or, move the camera to a specific position, and change its orientation
    //     // bounds.moveTo([0, 10, 10]).lookAt({ target: [5, 5, 0], up: [0, -1, 0] })

    //     // For orthographic cameras, reset has to be used to center the view (fit would only change its zoom to match the bounding box)
    //     // bounds.refresh().reset().clip().fit()
    // }, [JSON.stringify(sensorNodes)])

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
            {/* <OrthographicCamera makeDefault position={[100, 100, 100]} /> */}
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

import { Edges } from '@react-three/drei'
import { useRef, useState } from 'react'
import * as THREE from 'three'
import { Mesh } from 'three'
import { Sensor } from '../shared/types/sensor'
import useViewerStore from '../store/viewerStore'

const clusterColors: THREE.ColorRepresentation[] = [
    'red',
    'green',
    'violet',
    'yellow',
    'khaki',
    'blue',
    'mediumvioletred',
    'tomato',
    'purple',
    'magenta',
    'skyblue',
    'lime',
    'crimson',
    'salmon',
    'goldenrod',
    'limegreen',
    'teal',
    'olive',
    'navy',
    'aqua',
    'brown'
]

interface Props {
    sensor: Sensor
}

export default function SensorNode({ sensor }: Props) {
    const meshRef = useRef<Mesh>(null)

    const [hovered, setHover] = useState(false)

    const { scale, selectedSensor, setSelectedSensor } = useViewerStore()

    const isSelected = selectedSensor && sensor.Id === selectedSensor.Id

    let color: THREE.ColorRepresentation
    if (sensor.ClusterId > -1) {
        color = clusterColors[sensor.ClusterId % clusterColors.length]
    } else {
        color = 'orange'
    }

    const setSelected = () => {
        if (isSelected) {
            setSelectedSensor(undefined)
        } else {
            setSelectedSensor(sensor)
        }
    }

    return (
        <mesh
            position={[
                sensor.Position.X / scale,
                sensor.Position.Y / scale,
                sensor.Position.Z / scale
            ]}
            ref={meshRef}
            scale={isSelected ? 2 : 1}
            onClick={setSelected}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}
        >
            {!sensor.IsReference && <boxGeometry args={[1, 1, 1]} />}
            {sensor.IsReference && <sphereGeometry args={[1]} />}
            <meshStandardMaterial
                color={hovered ? 'pink' : isSelected ? 'white' : color}
            />
            <Edges />
        </mesh>
    )
}

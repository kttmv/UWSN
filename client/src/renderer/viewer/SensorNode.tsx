import { Edges } from '@react-three/drei'
import { useRef, useState } from 'react'
import * as THREE from 'three'
import { Mesh } from 'three'
import { SensorSimulationState } from '../shared/types/sensorSimulationState'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'

const clusterColors: THREE.ColorRepresentation[] = [
    'red',
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
    sensor: SensorSimulationState
}

export default function SensorNode({ sensor }: Props) {
    const meshRef = useRef<Mesh>(null)

    const [hovered, setHover] = useState(false)

    const { scale, selectedSensor, setSelectedSensor } = useViewerStore()

    const { project } = useProjectStore()
    if (!project) {
        throw new Error('Project не определен')
    }

    const isSelected = selectedSensor && sensor.Id === selectedSensor.Id

    let color: THREE.ColorRepresentation
    let isDead = false

    if (sensor.ClusterId === undefined) {
        // кластер не определен
        color = 'orange'
    } else if (
        sensor.Battery <= project.SensorSettings.BatteryDeadCharge ||
        sensor.ClusterId < 0
    ) {
        // сенсор разряжен или относится к специальному кластеру для умерших сенсоров
        isDead = true
        color = 'black'
    } else {
        // у сенсора есть кластер
        color = clusterColors[sensor.ClusterId % clusterColors.length]
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
            rotation={[Math.PI, 0, 0]}
            ref={meshRef}
            scale={isSelected ? 2 : 1}
            onClick={setSelected}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}
        >
            {isDead ? (
                <coneGeometry args={[0.5, 1, 8]} />
            ) : sensor.IsReference ? (
                <sphereGeometry args={[1]} />
            ) : (
                <boxGeometry args={[1, 1, 1]} />
            )}
            <meshStandardMaterial
                color={hovered ? 'pink' : isSelected ? 'white' : color}
            />
            <Edges />
        </mesh>
    )
}

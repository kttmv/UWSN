import { Edges } from '@react-three/drei'
import { Vector3 } from '@react-three/fiber'
import { useRef, useState } from 'react'
import * as THREE from 'three'
import { Mesh } from 'three'

const clusterColors: THREE.ColorRepresentation[] = [
    'red',
    'green',
    'blue',
    'violet',
    'yellow',
    'khaki',
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
    position: Vector3
    clusterId: number
    isReference: boolean
}

export default function SensorNode({
    position,
    clusterId,
    isReference
}: Props) {
    const meshRef = useRef<Mesh>(null)

    const [hovered, setHover] = useState(false)
    const [active, setActive] = useState(false)

    let color: THREE.ColorRepresentation
    if (clusterId > -1) color = clusterColors[clusterId % clusterColors.length]
    else color = 'orange'

    return (
        <mesh
            position={position}
            ref={meshRef}
            scale={active ? 1.25 : 1}
            onClick={() => setActive(!active)}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}
        >
            {!isReference && <boxGeometry args={[1, 1, 1]} />}
            {isReference && <sphereGeometry args={[1.5]} />}
            <meshStandardMaterial
                color={hovered ? 'pink' : active ? 'hotpink' : color}
            />
            <Edges />
        </mesh>
    )
}

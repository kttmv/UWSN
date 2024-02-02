import { Edges } from '@react-three/drei'
import { Vector3 } from '@react-three/fiber'
import { useRef, useState } from 'react'
import { Mesh } from 'three'

interface Props {
    position: Vector3
}

export default function SensorNode({ position }: Props) {
    const meshRef = useRef<Mesh>(null!)

    const [hovered, setHover] = useState(false)
    const [active, setActive] = useState(false)

    return (
        <mesh
            position={position}
            ref={meshRef}
            scale={active ? 1.25 : 1}
            onClick={() => setActive(!active)}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}
        >
            <boxGeometry args={[1, 1, 1]} />
            <meshStandardMaterial
                color={hovered ? 'hotpink' : active ? 'red' : 'orange'}
            />
            <Edges />
        </mesh>
    )
}

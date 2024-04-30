import { Line } from '@react-three/drei'
import { Vector3 } from '../shared/types/vector3'

type Props = {
    from: Vector3
    to: Vector3
}

export default function Signal({ from, to }: Props) {
    return (
        <>
            <Line
                points={[
                    [from.X, from.Y, from.Z],
                    [to.X, to.Y, to.Z]
                ]}
                vertexColors={[
                    [0.05, 0.05, 0.05],
                    [0.2, 0.3, 0.3]
                ]}
                lineWidth={5}
            />
        </>
    )
}

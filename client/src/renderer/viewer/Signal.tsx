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
                color='black'
                lineWidth={5}
            />
        </>
    )
}

import { Vector3 } from '../shared/types/vector3'
import GridRectangle from './GridRectangle'

interface Props {
    v1: Vector3
    v2: Vector3
}

export default function AreaLimits({ v1, v2 }: Props) {
    return (
        <>
            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X,
                    Y: v2.Y,
                    Z: v1.Z
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X,
                    Y: v1.Y,
                    Z: v2.Z
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v1.X,
                    Y: v2.Y,
                    Z: v1.Z
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v2.X,
                    Y: v1.Y,
                    Z: v1.Z
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X,
                    Y: v2.Y,
                    Z: v1.Z
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X,
                    Y: v1.Y,
                    Z: v2.Z
                }}
            />
        </>
    )
}
